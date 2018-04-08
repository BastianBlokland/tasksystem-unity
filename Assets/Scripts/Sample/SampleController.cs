using System.Collections.Generic;
using Tasks;
using Utils;
using UnityEngine;

namespace Sample
{
	public class SampleController : MonoBehaviour
	{
		private const int CUBE_COUNT = 20000;

		[SerializeField] private Mesh mesh;
		[SerializeField] private Material material;
		[SerializeField] private Transform targetTrans;
		[SerializeField] private int batchSize = 10;

		private readonly Vector2[] spawnPoints = new Vector2[CUBE_COUNT];
		private readonly CubeData[] cubeData = new CubeData[CUBE_COUNT];
		private readonly Matrix4x4[] cubeMatrices = new Matrix4x4[CUBE_COUNT];
		private readonly PartitionSet<CubeData> partitionedCubes = new PartitionSet<CubeData>();
	
		private ITaskHandle respawnCubesHandle;
		private ITaskHandle fillRenderSetHandle;

		private Vector2 lastTargetPosition;
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
			//Render the data from the last frame
			if(fillRenderSetHandle != null)
			{
				fillRenderSetHandle.Join();

				renderSet.Render();
				renderSet.Clear();
			}
			if(respawnCubesHandle != null)
				respawnCubesHandle.Join();

			//Partition the cubes
			GridPartitioner partitioner = new GridPartitioner(Random.Range(7f, 10f)); //Use a 'random' partition size to make the 'grid' less noticeable
			partitionedCubes.ClearData();
			ITaskHandle partitionCubesHandle = taskManager.ScheduleArray(cubeData, new PartitionCubesTask(partitionedCubes, partitioner), batchSize);

			//After the partitioning move the cubes
			Vector2 targetPosition = new Vector2(targetTrans.position.x, targetTrans.position.z);
			Vector2 targetDiff = targetPosition - lastTargetPosition;
			Vector2 targetVelocity = targetDiff == Vector2.zero ? Vector2.zero : targetDiff / Time.deltaTime;
			lastTargetPosition = targetPosition;

			MoveCubeTask moveTask = new MoveCubeTask
			(
				deltaTime: Time.deltaTime,
				targetPosition: targetPosition,
				targetVelocity: targetVelocity,
				partitioner: partitioner,
				others: partitionedCubes
			);
			ITaskHandle updateCubeHandle = taskManager.ScheduleArray(cubeData, moveTask, batchSize, partitionCubesHandle);

			//After that calculate the new matrices
			ITaskHandle calculateMatricesHandle = taskManager.ScheduleArray(cubeData, cubeMatrices, new CalculateMatricesTask(), batchSize, updateCubeHandle);

			//After that in parallel fill the render-set and respawn cubes that are too far away
			respawnCubesHandle = taskManager.ScheduleArray(cubeData, new RespawnCubesTask(spawnPoints), batchSize, calculateMatricesHandle);
			fillRenderSetHandle = taskManager.ScheduleArray(cubeMatrices, new FillRenderSetTask(renderSet), batchSize, calculateMatricesHandle);
		}

		protected void OnDestroy()
		{
			taskManager.Dispose();
			partitionedCubes.Dispose();
			renderSet.Dispose();
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