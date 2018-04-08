using System.Collections.Generic;
using Tasks;
using Utils;
using UnityEngine;

namespace Sample
{
	public class SampleController : MonoBehaviour
	{
		//---> Config
		[SerializeField] private Mesh mesh;
		[SerializeField] private Material material;
		[SerializeField] private Transform targetTrans;
		[SerializeField] private int executorCount = 7;
		[SerializeField] private int batchSize = 50;
		[SerializeField] private int cubeCount = 35000;
		[SerializeField] private float minInitialSpawnPoint = -100f;
		[SerializeField] private float maxInitialSpawnPoint = 100f;
		[SerializeField] private float minPartitionSize = 3.5f;
		[SerializeField] private float maxPartitionSize = 5f;
		[SerializeField] private float cubeRadius = 1.25f;
		[SerializeField] private float cubeSeperationForce = 2f;
		[SerializeField] private float cubeVeloInheritance = .75f;
		[SerializeField] private float targetRadius = 5f;
		[SerializeField] private float targetSeperationForce = 10f;
		[SerializeField] private float targetVeloInheritance = 1f;
		[SerializeField] private float maxDistanceBeforeRespawn = 200f;

		//---> Buffers
		private Vector2[] spawnPoints;
		private CubeData[] cubeData;
		private Matrix4x4[] cubeMatrices;
		private PartitionSet<CubeData> partitionedCubes;
		private RenderSet renderSet;

		//---> Misc
		private TaskManager taskManager;
		private GridPartitioner gridPartitioner;

		//---> Tasks
		private PartitionCubeTask partitionCubeTask;
		private MoveCubeTask moveCubeTask;
		private CalculateMatrixTask calculateMatrixTask;
		private RespawnCubeTask respawnCubeTask;
		private AddToRenderSetTask addToRenderSetTask;

		//---> Depenencies that 'HAVE' to be completed before we can start a new frame
		private DependencySet finishDepenency;

		//---> Info about target
		private Vector2 targetPosition;
		private Vector2 targetVelocity;

		protected void Start()
		{
			if(mesh == null) { Debug.LogError("[SampleController] No 'mesh' provided"); return; }
			if(material == null) { Debug.LogError("[SampleController] No 'material' provided"); return; }
			if(targetTrans == null) { Debug.LogError("[SampleController] No 'targetTrans' provided"); return; }
			
			//Allocate arrays
			spawnPoints = new Vector2[cubeCount];
			cubeData = new CubeData[cubeCount];
			cubeMatrices = new Matrix4x4[cubeCount];
			partitionedCubes = new PartitionSet<CubeData>();
			renderSet = new RenderSet(mesh, material);

			//Setup initial data
			for (int i = 0; i < cubeCount; i++)
			{
				spawnPoints[i] = new Vector2(	Random.Range(minInitialSpawnPoint, maxInitialSpawnPoint), 
												Random.Range(minInitialSpawnPoint, maxInitialSpawnPoint));
				cubeData[i] = new CubeData { ID = i, Position = spawnPoints[i] };
			}

			//Create misc stuff
			taskManager = new TaskManager(executorCount);
			gridPartitioner = new GridPartitioner();

			//Create tasks
			partitionCubeTask = new PartitionCubeTask(partitionedCubes, gridPartitioner);
			moveCubeTask = new MoveCubeTask(gridPartitioner, partitionedCubes);
			calculateMatrixTask = new CalculateMatrixTask();
			respawnCubeTask = new RespawnCubeTask(spawnPoints);
			addToRenderSetTask = new AddToRenderSetTask(renderSet);
		}

		protected void Update()
		{
			if(taskManager == null)
				return;

			//---> Update target info based on the linked-in transform
			UpdateTargetInfo();

			//---> Render the data from the previous tasks
			if(finishDepenency != null)
			{
				//Wait for (and help out with) completing the tasks from the previous frame
				finishDepenency.Join();

				//Render the results from the previous tasks
				renderSet.Render();
				renderSet.Clear();
			}

			//---> Update the tasks with info from the inspector
			gridPartitioner.PartitionSize = Random.Range(minPartitionSize, maxPartitionSize); //Randomize to make the grid less noticeable
			moveCubeTask.CubeRadius = cubeRadius;
			moveCubeTask.CubeSeperationForce = cubeSeperationForce;
			moveCubeTask.CubeVeloInheritance = cubeVeloInheritance;
			moveCubeTask.TargetRadius = targetRadius;
			moveCubeTask.TargetSeperationForce = targetSeperationForce;
			moveCubeTask.TargetVeloInheritance = targetVeloInheritance;
			moveCubeTask.DeltaTime = Time.deltaTime;
			moveCubeTask.TargetPosition = targetPosition;
			moveCubeTask.TargetVelocity = targetVelocity;
			respawnCubeTask.MaxDistance = maxDistanceBeforeRespawn;

			//---> Clear some data from the previous frame
			partitionedCubes.ClearData();

			//---> Schedule tasks for this frame
			//NOTE: there is no safety yet, so you manually need to check what resources the taska are 
			//using and setup depenendcies between the tasks accordingly
			ITaskDependency partitionCubesDep = taskManager.ScheduleArray(cubeData, partitionCubeTask, batchSize);

			ITaskDependency updateCubeDep = taskManager.ScheduleArray(cubeData, moveCubeTask, batchSize, partitionCubesDep);

			ITaskDependency calculateMatricesDep = taskManager.ScheduleArray(cubeData, cubeMatrices, calculateMatrixTask, batchSize, updateCubeDep);

			//Note: There last two tasks both depend on the previous one so they run in parallel 
			ITaskDependency respawnCubesDep = taskManager.ScheduleArray(cubeData, respawnCubeTask, batchSize, calculateMatricesDep);
			ITaskDependency addToRenderSetDep = taskManager.ScheduleArray(cubeMatrices, addToRenderSetTask, batchSize, calculateMatricesDep);

			//---> Setup the finish dependency
			finishDepenency = new DependencySet(respawnCubesDep, addToRenderSetDep);
		}

		protected void OnDestroy()
		{
			if(taskManager != null)
				taskManager.Dispose();
			if(partitionedCubes != null)
				partitionedCubes.Dispose();
			if(renderSet != null)
				renderSet.Dispose();
		}

		private void UpdateTargetInfo()
		{
			Vector2 newTargetPos = targetTrans == null ? Vector2.zero : new Vector2(targetTrans.position.x, targetTrans.position.z);
			Vector2 targetDiff = newTargetPos - targetPosition;
			targetPosition = newTargetPos;
			targetVelocity = targetDiff == Vector2.zero ? Vector2.zero : targetDiff / Time.deltaTime;
		}
	}
}