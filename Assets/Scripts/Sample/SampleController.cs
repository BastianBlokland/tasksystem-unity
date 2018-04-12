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
		[SerializeField] private Transform target1Trans;
		[SerializeField] private Transform target2Trans;
		[SerializeField] private bool useMultiThreading = true;
		[SerializeField] private int batchSize = 100;
		[SerializeField] private int cubeCount = 35000;
		[SerializeField] private Vector2 spawnAreaSize = new Vector2(200f, 200f);
		[SerializeField] private float avoidanceCellSize = 2f;
		[SerializeField] private int avoidanceBucketCount = 10000;
		[SerializeField] private int avoidanceMaxBucketSize = 10;
		[SerializeField] private float cubeRadius = 1.25f;
		[SerializeField] private float cubeSeperationForce = 2f;
		[SerializeField] private float cubeVeloInheritance = .75f;
		[SerializeField] private float targetRadius = 5f;
		[SerializeField] private float targetSeperationForce = 15f;
		[SerializeField] private float targetVeloInheritance = 1f;
		[SerializeField] private float maxDistanceBeforeRespawn = 200f;
		[SerializeField] private Vector2 respawnAreaSize = new Vector2(200f, 200f);
		[SerializeField] private float respawnForce = 1f;
		[SerializeField] private Color hitByTarget1Color = Color.red;
		[SerializeField] private Color hitByTarget2Color = Color.blue;

		//---> Buffers
		private CubeData[] cubeData;
		private BucketSet<CubeData> bucketedCubes;
		private RenderSet renderSet;

		//---> Misc
		private TaskManager taskManager;
		private PositionHasher avoidanceHasher;
		private IRandomProvider random;

		//---> Tasks
		private StartFrameTask startTask;
		private BucketCubeTask bucketCubeTask;
		private MoveCubeTask moveCubeTask;
		private RespawnCubeTask respawnCubeTask;
		private AddToRenderSetTask addToRenderSetTask;

		//---> Depenencies that 'HAVE' to be completed before we can start a new frame
		private IDependency completeDependency;

		//---> Info about target
		private Vector2 target1Position;
		private Vector2 target1Velocity;
		private Vector2 target2Position;
		private Vector2 target2Velocity;

		//---> Profiling tracks
		private TimelineTrack completeProfilerTrack;
		private TimelineTrack renderProfilerTrack;
		private TaskTimelineTrack bucketCubesProfilerTrack;
		private TaskTimelineTrack moveCubesProfilerTrack;
		private TaskTimelineTrack respawnCubesProfilerTrack;
		private TaskTimelineTrack addToRenderSetProfilerTrack;

		protected void Start()
		{
			if(mesh == null) { Debug.LogError("[SampleController] No 'mesh' provided"); return; }
			if(material == null) { Debug.LogError("[SampleController] No 'material' provided"); return; }
			
			//Allocate arrays
			cubeData = new CubeData[cubeCount];
			bucketedCubes = new BucketSet<CubeData>(bucketCount: avoidanceBucketCount, maxBucketSize: avoidanceMaxBucketSize);
			renderSet = new RenderSet(mesh, material, maxBatches: Mathf.CeilToInt(cubeCount / 1023f));

			//Create misc stuff
			int numExecutors = useMultiThreading ? (System.Environment.ProcessorCount - 1) : 0;
			Debug.Log(string.Format("[SampleController] Staring 'TaskManager' with '{0}' executors", numExecutors));
			taskManager = new TaskManager(numExecutors);
			avoidanceHasher = new PositionHasher();
			random = new ShiftRandomProvider();

			//Create tasks
			startTask = new StartFrameTask();
			bucketCubeTask = new BucketCubeTask(bucketedCubes, avoidanceHasher);
			moveCubeTask = new MoveCubeTask(avoidanceHasher, bucketedCubes);
			respawnCubeTask = new RespawnCubeTask(random);
			addToRenderSetTask = new AddToRenderSetTask(renderSet);

			//Setup profiler timeline
			if(profiler != null)
			{
				completeProfilerTrack = profiler.CreateTrack<TimelineTrack>("Blocking main-thread to complete tasks");
				renderProfilerTrack = profiler.CreateTrack<TimelineTrack>("Render instanced");
				bucketCubesProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Bucket cubes");
				moveCubesProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Move cubes");
				respawnCubesProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Respawn cubes");
				addToRenderSetProfilerTrack = profiler.CreateTrack<TaskTimelineTrack>("Creating render batches");
				profiler.StartTimers();
			}

			//Setup initial data
			Rect spawnArea = MathUtils.FromCenterAndSize(Vector2.zero, spawnAreaSize);
			for (int i = 0; i < cubeCount; i++)
				cubeData[i] = new CubeData 
				{ 
					ID = i, 
					Position = random.Inside(spawnArea), 
					TimeNotHitTarget1 = 999f,
					TimeNotHitTarget2 = 999f
				};
		}

		protected void Update()
		{
			if(taskManager == null)
				return;

			//---> Update target info based on the linked-in transform
			UpdateTargetInfo(target1Trans, ref target1Position, ref target1Velocity);
			UpdateTargetInfo(target2Trans, ref target2Position, ref target2Velocity);

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
				renderProfilerTrack.LogStartWork();
				{
					renderSet.Render();
					renderSet.Clear();
				}
				renderProfilerTrack.LogEndWork();
			}

			//---> Update the tasks with info from the inspector
			avoidanceHasher.CellSize = avoidanceCellSize;
			avoidanceHasher.Fuzz = random.GetNext(); //Apply random fuzz to make the grid less noticable
			moveCubeTask.CubeRadius = cubeRadius;
			moveCubeTask.CubeSeperationForce = cubeSeperationForce;
			moveCubeTask.CubeVeloInheritance = cubeVeloInheritance;
			moveCubeTask.TargetRadius = targetRadius;
			moveCubeTask.TargetSeperationForce = targetSeperationForce;
			moveCubeTask.TargetVeloInheritance = targetVeloInheritance;
			moveCubeTask.DeltaTime = Time.deltaTime;
			moveCubeTask.Target1Position = target1Position;
			moveCubeTask.Target1Velocity = target1Velocity;
			moveCubeTask.Target2Position = target2Position;
			moveCubeTask.Target2Velocity = target2Velocity;
			respawnCubeTask.MaxDistance = maxDistanceBeforeRespawn;
			respawnCubeTask.RespawnArea = MathUtils.FromCenterAndSize(Vector2.zero, respawnAreaSize);
			respawnCubeTask.RespawnForce = respawnForce;
			addToRenderSetTask.HitByTarget1Color = hitByTarget1Color;
			addToRenderSetTask.HitByTarget2Color = hitByTarget2Color;

			//---> Clear some data from the previous frame
			bucketedCubes.Clear();

			//---> Schedule tasks for this frame
			//NOTE: there is no safety yet, so you manually need to check what resources the taska are 
			//using and setup dependencies between the tasks accordingly
			IDependency startDep = taskManager.ScheduleSingle
			(
				task: startTask
			);
			IDependency bucketCubesDep = taskManager.ScheduleArray
			(
				data: cubeData, 
				task: bucketCubeTask, 
				batchSize: batchSize,
				dependency: startDep,
				tracker: bucketCubesProfilerTrack
			);
			IDependency updateCubeDep = taskManager.ScheduleArray
			(
				data: cubeData, 
				task: moveCubeTask, 
				batchSize: batchSize, 
				dependency: bucketCubesDep, 
				tracker: moveCubesProfilerTrack
			);
			IDependency respawnCubesDep = taskManager.ScheduleArray
			(
				data: cubeData, 
				task: respawnCubeTask, 
				batchSize: batchSize, 
				dependency: updateCubeDep,
				tracker: respawnCubesProfilerTrack
			);
			IDependency addToRenderSetDep = taskManager.ScheduleArray
			(
				data: cubeData, 
				task: addToRenderSetTask, 
				batchSize: RenderSet.BATCH_SIZE, 
				dependency: respawnCubesDep,
				tracker: addToRenderSetProfilerTrack
			);

			//---> Setup the finish dependency
			completeDependency = addToRenderSetDep;
		}

		protected void OnDestroy()
		{
			if(taskManager != null)
				taskManager.Dispose();
		}

		private void UpdateTargetInfo(Transform trans, ref Vector2 targetPosition, ref Vector2 targetVelocity)
		{
			Vector2 newTargetPos = trans == null ? Vector2.zero : new Vector2(trans.position.x, trans.position.z);
			Vector2 targetDiff = newTargetPos - targetPosition;
			targetPosition = newTargetPos;
			targetVelocity = targetDiff == Vector2.zero ? Vector2.zero : targetDiff / Time.deltaTime;
		}
	}
}