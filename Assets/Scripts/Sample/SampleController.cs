using System.Collections.Generic;
using Tasks;
using UnityEngine;

namespace Sample
{
	public class SampleController : MonoBehaviour
	{
		private const int CUBE_COUNT = 10000;

		[SerializeField] private Mesh mesh;
		[SerializeField] private Material material;
		[SerializeField] private Transform targetTrans;

		private readonly CubeData[] cubeData = new CubeData[CUBE_COUNT];
		private readonly ITaskHandle<CubeData>[] moveTasks = new ITaskHandle<CubeData>[CUBE_COUNT];
		private readonly Matrix4x4[] renderMatrices = new Matrix4x4[1023]; //1023, is the max render count for a single call (https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstanced.html)

		private TaskManager taskManager;

		protected void Start()
		{
			taskManager = new TaskManager();

			for (int i = 0; i < CUBE_COUNT; i++)
				SpawnCube(i);
		}

		protected void Update()
		{
			MoveCubeTask moveTask = new MoveCubeTask(acceleration: 5f, targetSpeed: 15f, deltaTime: Time.deltaTime);

			Vector3 targetPosition = targetTrans.position;
			for (int i = 0; i < CUBE_COUNT; i++)
			{
				//Get the new data from the previous task
				if(moveTasks[i] != null)
					cubeData[i] = moveTasks[i].Join();

				//Update target
				cubeData[i].Target = targetPosition;

				//Schedule a new move
				moveTasks[i] = taskManager.ScheduleTask(moveTask, cubeData[i]);
			}

			//Render the cubes
			for (int ci = 0; ci < cubeData.Length; ci += renderMatrices.Length) //Render in chunks of '1023'
			{
				int chunkSize = (ci + renderMatrices.Length) >= cubeData.Length ? (cubeData.Length - ci) : renderMatrices.Length;
				//Prepare the 'chunk' for rendering
				for (int mi = 0; mi < chunkSize; mi++)
					renderMatrices[mi] =	Matrix4x4.Translate(cubeData[ci + mi].Position) * 
											Matrix4x4.Rotate(cubeData[ci + mi].Rotation);
				//Render chunk
				Graphics.DrawMeshInstanced(mesh, 0, material, renderMatrices, chunkSize);
			}
		}

		protected void OnDestroy()
		{
			taskManager.Dispose();
		}

		private void SpawnCube(int id)
		{
			cubeData[id] = new CubeData
			{
				Position = new Vector3(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f)),
				Velocity = Vector3.zero,
				Rotation = Quaternion.identity,
				Target = Vector3.zero
			};
		}
	}
}