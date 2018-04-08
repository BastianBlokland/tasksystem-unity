using System.Collections.Generic;
using Tasks;
using Utils;
using UnityEngine;

namespace Sample
{
	public class SampleController : MonoBehaviour
	{
		private const int CUBE_COUNT = 15000;

		[SerializeField] private Mesh mesh;
		[SerializeField] private Material material;
		[SerializeField] private Transform targetTrans;
		[SerializeField] private int batchSize = 10;

		private readonly Vector2[] spawnPoints = new Vector2[CUBE_COUNT];
		private readonly CubeData[] cubeData = new CubeData[CUBE_COUNT];
		private readonly Matrix4x4[] cubeMatrices = new Matrix4x4[CUBE_COUNT];
		private readonly PartitionSet<CubeData> partitionedData = new PartitionSet<CubeData>();
	
		private ITaskHandle updateCubeHandle;
		private ITaskHandle calculateMatricesHandle;
		private ITaskHandle respawnCubesHandle;

		private Vector2 lastTargetPosition;
		private GridPartitioner partitioner;
		private TaskManager taskManager;
		private RenderSet renderSet;

		protected void Start()
		{
			taskManager = new TaskManager();
			renderSet = new RenderSet(mesh, material);

			for (int i = 0; i < CUBE_COUNT; i++)
			{
				spawnPoints[i] = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
				SpawnCube(i);
			}
		}

		protected void Update()
		{
			Vector2 targetPosition = new Vector2(targetTrans.position.x, targetTrans.position.z);
			Vector2 targetDiff = targetPosition - lastTargetPosition;
			Vector2 targetVelocity = targetDiff == Vector2.zero ? Vector2.zero : targetDiff / Time.deltaTime;
			lastTargetPosition = targetPosition;

			//Make sure all handles have been completed
			if(updateCubeHandle != null)
				updateCubeHandle.Join();
			if(calculateMatricesHandle != null)
				calculateMatricesHandle.Join();
			if(respawnCubesHandle != null)
				respawnCubesHandle.Join();

			//Prepare the data for rendering
			renderSet.Clear();
			for (int i = 0; i < CUBE_COUNT; i++)
				renderSet.Add(cubeMatrices[i]);

			//Update partitioned data
			partitioner = new GridPartitioner(Random.Range(7f, 10f)); //Use a 'random' partition size to make the 'grid' less noticeable
			partitionedData.ClearData();
			for (int i = 0; i < CUBE_COUNT; i++)
			{
				int partition = partitioner.Partition(cubeData[i].Position);
				partitionedData.Add(partition, cubeData[i]);
			}

			//Schedule the tasks
			MoveCubeTask moveTask = new MoveCubeTask
			(
				deltaTime: Time.deltaTime,
				targetPosition: targetPosition,
				targetVelocity: targetVelocity,
				partitioner: partitioner,
				others: partitionedData
			);
			updateCubeHandle = taskManager.ScheduleArray(cubeData, moveTask, batchSize);
			calculateMatricesHandle = taskManager.ScheduleArray(cubeData, cubeMatrices, new CalculateMatricesTask(), batchSize, updateCubeHandle);
			respawnCubesHandle = taskManager.ScheduleArray(cubeData, new RespawnCubesTask(spawnPoints), batchSize, updateCubeHandle);

			renderSet.Render();
		}

		protected void OnDestroy()
		{
			taskManager.Dispose();
		}

		private void SpawnCube(int id)
		{
			cubeData[id] = new CubeData
			{
				ID = id,
				Position = spawnPoints[id],
				Velocity = Vector2.zero,
				Rotation = 0f
			};
		}
	}
}