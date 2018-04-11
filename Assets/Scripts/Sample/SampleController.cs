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
		[SerializeField] private int maxRenderBatches = 500;
		[SerializeField] private float renderCellSize = 1f;		

		//---> Buffers
		private CubeData[] cubeData;
		private BucketSet<CubeData> bucketedCubes;
		private RenderSet renderSet;

		//---> Misc
		private TaskManager taskManager;
		private PositionHasher avoidanceHasher;
		private PositionHasher renderHasher;
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
		private Vector2 targetPosition;
		private Vector2 targetVelocity;

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
			if(targetTrans == null) { Debug.LogError("[SampleController] No 'targetTrans' provided"); return; }
			
			//Allocate arrays
			cubeData = new CubeData[cubeCount];
			bucketedCubes = new BucketSet<CubeData>(bucketCount: avoidanceBucketCount, maxBucketSize: avoidanceMaxBucketSize);
			renderSet = new RenderSet(mesh, material, maxBatches: maxRenderBatches);

			//Create misc stuff
			int numExecutors = useMultiThreading ? (System.Environment.ProcessorCount - 1) : 0;
			Debug.Log(string.Format("[SampleController] Staring 'TaskManager' with '{0}' executors", numExecutors));
			taskManager = new TaskManager(numExecutors);
			avoidanceHasher = new PositionHasher();
			renderHasher = new PositionHasher();
			random = new ShiftRandomProvider();

			//Create tasks
			startTask = new StartFrameTask();
			bucketCubeTask = new BucketCubeTask(bucketedCubes, avoidanceHasher);
			moveCubeTask = new MoveCubeTask(avoidanceHasher, bucketedCubes);
			respawnCubeTask = new RespawnCubeTask(random);
			addToRenderSetTask = new AddToRenderSetTask(renderSet, renderHasher);

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
				cubeData[i] = new CubeData { ID = i, Position = random.Inside(spawnArea) };
		}

		protected void Update()
		{
			if(taskManager == null)
				return;

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
			moveCubeTask.TargetPosition = targetPosition;
			moveCubeTask.TargetVelocity = targetVelocity;
			respawnCubeTask.MaxDistance = maxDistanceBeforeRespawn;
			respawnCubeTask.RespawnArea = MathUtils.FromCenterAndSize(Vector2.zero, spawnAreaSize);
			renderHasher.CellSize = renderCellSize;

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
				batchSize: batchSize, 
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

		private void UpdateTargetInfo()
		{
			Vector2 newTargetPos = targetTrans == null ? Vector2.zero : new Vector2(targetTrans.position.x, targetTrans.position.z);
			Vector2 targetDiff = newTargetPos - targetPosition;
			targetPosition = newTargetPos;
			targetVelocity = targetDiff == Vector2.zero ? Vector2.zero : targetDiff / Time.deltaTime;
		}
	}
}