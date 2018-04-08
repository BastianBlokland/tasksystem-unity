using System.Collections.Generic;
using Tasks;
using Utils;
using UnityEngine;

namespace Sample
{
	public class SampleController : MonoBehaviour
	{
		private const int CUBE_COUNT = 12500;

		[SerializeField] private Mesh mesh;
		[SerializeField] private Material material;
		[SerializeField] private Transform targetTrans;

		private readonly CubeData[] cubeData = new CubeData[CUBE_COUNT];
		private readonly PartitionSet<CubeData> partitionedData = new PartitionSet<CubeData>();
		private readonly Matrix4x4[] renderMatrices = new Matrix4x4[1023]; //1023, is the max render count for a single call (https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstanced.html)
		
		private ITaskHandle<CubeData[]> updateCubeTask;

		private Vector2 lastTargetPosition;
		private GridPartitioner partitioner;
		private TaskManager taskManager;

		protected void Start()
		{
			taskManager = new TaskManager();

			for (int i = 0; i < CUBE_COUNT; i++)
				SpawnCube(i);
		}

		protected void Update()
		{
			Vector2 targetPosition = new Vector2(targetTrans.position.x, targetTrans.position.z);
			Vector2 targetDiff = targetPosition - lastTargetPosition;
			Vector2 targetVelocity = targetDiff == Vector2.zero ? Vector2.zero : targetDiff / Time.deltaTime;
			lastTargetPosition = targetPosition;

			//Observer the results
			if(updateCubeTask != null)
				updateCubeTask.Join();

			//Render the cubes
			for (int ci = 0; ci < cubeData.Length; ci += renderMatrices.Length) //Render in chunks of '1023'
			{
				int chunkSize = (ci + renderMatrices.Length) >= cubeData.Length ? (cubeData.Length - ci) : renderMatrices.Length;
				//Prepare the 'chunk' for rendering
				for (int mi = 0; mi < chunkSize; mi++)
					renderMatrices[mi] = CreateMatrix(cubeData[ci + mi]);

				//Render chunk
				Graphics.DrawMeshInstanced(mesh, 0, material, renderMatrices, chunkSize);
			}

			//Respawn cubes if necessary
			for (int i = 0; i < CUBE_COUNT; i++)
			{
				if(cubeData[i].Position.sqrMagnitude > (200f * 200f))
					SpawnCube(i);
			}

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
			updateCubeTask = taskManager.ScheduleBatch(cubeData, moveTask);
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
				Position = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f)),
				Velocity = Vector2.zero,
				Rotation = 0f
			};
		}

		private Matrix4x4 CreateMatrix(CubeData data)
		{
			float rotInRad = data.Rotation * Mathf.Deg2Rad;
			float cos = Mathf.Cos(rotInRad);
			float sin = Mathf.Sin(rotInRad);
			return new Matrix4x4
			(
				column0: new Vector4(cos, 0f, -sin, 0f),
				column1: new Vector4(0f, 1f, 0f, 0f),
				column2: new Vector4(sin, 0f, cos, 0f),
				column3: new Vector4(data.Position.x, 0f, data.Position.y, 1f)
			);
		}
	}
}