using System.Collections.Generic;
using Tasks;
using Utils;
using UnityEngine;
using Profiler;

namespace Sample
{
	public class SampleController : MonoBehaviour
	{
		//---> Config
		[SerializeField] private Timeline profiler;
		[SerializeField] private Mesh mesh;
		[SerializeField] private Material material;
		[SerializeField] private Transform targetTrans;
		[SerializeField] private int executorCount = 7;
		[SerializeField] private int batchSize = 50;
		[SerializeField] private int cubeCount = 35000;
		[SerializeField] private Vector2 spawnAreaSize = new Vector2(200f, 200f);
		[SerializeField] private float minPartitionSize = 3f;
		[SerializeField] private float maxPartitionSize = 4f;
		[SerializeField] private float cubeRadius = 1.25f;
		[SerializeField] private float cubeSeperationForce = 2f;
		[SerializeField] private float cubeVeloInheritance = .75f;
		[SerializeField] private float targetRadius = 5f;
		[SerializeField] private float targetSeperationForce = 15f;
		[SerializeField] private float targetVeloInheritance = 1f;
		[SerializeField] private float maxDistanceBeforeRespawn = 200f;

		//---> Buffers
		private CubeData[] cubeData;
		private Matrix4x4[] cubeMatrices;
		private PartitionSet<CubeData> partitionedCubes;
		private RenderSet renderSet;

		//---> Misc
		private TaskManager taskManager;
		private GridPartitioner gridPartitioner;
		private IRandomProvider random;

		//---> Tasks
		private PartitionCubeTask partitionCubeTask;
		private MoveCubeTask moveCubeTask;
		private CalculateMatrixTask calculateMatrixTask;
		private RespawnCubeTask respawnCubeTask;
		private AddToRenderSetTask addToRenderSetTask;

		//---> Depenencies that 'HAVE' to be completed before we can start a new frame
		private DependencySet completeDependency;

		//---> Info about target
		private Vector2 targetPosition;
		private Vector2 targetVelocity;

		//---> Profiling tracks
		private TimelineTrack mainProfilerTrack;
		private TimelineTrack completeProfilerTrack;
		private TaskTimelineTrack partitionCubesProfilerTrack;
		private TaskTimelineTrack moveCubesProfilerTrack;
		private TaskTimelineTrack calculateMatricesProfilerTrack;
		private TaskTimelineTrack respawnCubesProfilerTrack;
		private TaskTimelineTrack addToRenderSetProfilerTrack;

		protected void Start()
		{
			if(mesh == null) { Debug.LogError("[SampleController] No 'mesh' provided"); return; }
			if(material == null) { Debug.LogError("[SampleController] No 'material' provided"); return; }
			if(targetTrans == null) { Debug.LogError("[SampleController] No 'targetTrans' provided"); return; }
			
			//Allocate arrays
			cubeData = new CubeData[cubeCount];
			cubeMatrices = new Matrix4x4[cubeCount];
			partitionedCubes = new PartitionSet<CubeData>();
			renderSet = new RenderSet(mesh, material);

			//Create misc stuff
			taskManager = new TaskManager(executorCount);
			gridPartitioner = new GridPartitioner();
			random = new ShiftRandomProvider();

			//Create tasks
			partitionCubeTask = new PartitionCubeTask(partitionedCubes, gridPartitioner);
			moveCubeTask = new MoveCubeTask(gridPartitioner, partitionedCubes);
			calculateMatrixTask = new CalculateMatrixTask();
			respawnCubeTask = new RespawnCubeTask(random);
			addToRenderSetTask = new AddToRenderSetTask(renderSet);

			//Setup profiler timeline
			if(profiler != null)
			{
				mainProfilerTrack = profiler.CreateTrack<TimelineTrack>("SampleController Update-method");
				completeProfilerTrack = profiler.CreateTrack<TimelineTrack>("Completing on main-thread");
				partitionCubesProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Partition cubes");
				moveCubesProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Move cubes");
				calculateMatricesProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Calculate matrices");
				respawnCubesProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Respawn cubes");
				addToRenderSetProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Creating render-set");
				profiler.Start();
			}

			//Setup initial data
			Rect spawnArea = MathUtils.FromCenterAndSize(Vector2.zero, spawnAreaSize);
			for (int i = 0; i < cubeCount; i++)
				cubeData[i] = new CubeData { ID = i, Position = random.Inside(spawnArea) };
		}

		protected void Update()
		{
			if(taskManager == null)
				return;

			mainProfilerTrack.LogStartWork();

			//---> Update target info based on the linked-in transform
			UpdateTargetInfo();

			//---> Render the data from the previous tasks
			if(completeDependency != null)
			{
				completeProfilerTrack.LogStartWork();
				{
					//Wait for (and help out with) completing the tasks from the previous frame
					completeDependency.Complete();
				}
				completeProfilerTrack.LogEndWork();
				
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
			respawnCubeTask.RespawnArea = MathUtils.FromCenterAndSize(Vector2.zero, spawnAreaSize);

			//---> Clear some data from the previous frame
			partitionedCubes.ClearData();

			//---> Schedule tasks for this frame
			//NOTE: there is no safety yet, so you manually need to check what resources the taska are 
			//using and setup dependencies between the tasks accordingly
			IDependency partitionCubesDep = taskManager.ScheduleArray
			(
				data: cubeData, 
				task: partitionCubeTask, 
				batchSize: batchSize,
				dependency: null,
				tracker: partitionCubesProfilerTrack
			);
			IDependency updateCubeDep = taskManager.ScheduleArray
			(
				data: cubeData, 
				task: moveCubeTask, 
				batchSize: batchSize, 
				dependency: partitionCubesDep, 
				tracker: moveCubesProfilerTrack
			);
			IDependency calculateMatricesDep = taskManager.ScheduleArray
			(
				data1: cubeData, 
				data2: cubeMatrices, 
				task: calculateMatrixTask, 
				batchSize: batchSize, 
				dependency: updateCubeDep,
				tracker: calculateMatricesProfilerTrack
			);

			//Note: There last two tasks both depend on the previous one so they run in parallel 
			IDependency respawnCubesDep = taskManager.ScheduleArray
			(
				data: cubeData, 
				task: respawnCubeTask, 
				batchSize: batchSize, 
				dependency: calculateMatricesDep,
				tracker: respawnCubesProfilerTrack
			);
			IDependency addToRenderSetDep = taskManager.ScheduleArray
			(
				data: cubeMatrices, 
				task: addToRenderSetTask, 
				batchSize: batchSize, 
				dependency: calculateMatricesDep,
				tracker: addToRenderSetProfilerTrack
			);

			//---> Setup the finish dependency
			completeDependency = new DependencySet(respawnCubesDep, addToRenderSetDep);

			mainProfilerTrack.LogEndWork();
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