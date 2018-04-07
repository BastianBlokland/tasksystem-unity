using System.Collections.Generic;
using Tasks;
using UnityEngine;

namespace Sample
{
	public class SampleController : MonoBehaviour
	{
		private const int CUBE_COUNT = 5000;

		[SerializeField] private GameObject prefab;
		[SerializeField] private Transform targetTrans;

		private readonly Transform[] graphics = new Transform[CUBE_COUNT];
		private readonly CubeData[] cubeData = new CubeData[CUBE_COUNT];
		private readonly ITaskHandle<CubeData>[] moveTasks = new ITaskHandle<CubeData>[CUBE_COUNT];

		private TaskManager taskManager;
		private int cubeIDCounter;

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

				//Apply the data
				ApplyData(i);

				//Update target
				cubeData[i].Target = targetPosition;

				//Schedule a new move
				moveTasks[i] = taskManager.ScheduleTask(moveTask, cubeData[i]);
			}
		}

		protected void OnDestroy()
		{
			taskManager.Dispose();
		}

		private void SpawnCube(int id)
		{
			graphics[id] = GameObject.Instantiate(prefab).transform;
			cubeData[id] = new CubeData
			{
				Position = new Vector3(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f)),
				Velocity = Vector3.zero,
				Rotation = Quaternion.identity,
				Target = Vector3.zero
			};
			ApplyData(id);
		}

		private void ApplyData(int id)
		{
			graphics[id].position = cubeData[id].Position;
			graphics[id].rotation = cubeData[id].Rotation;
		}
	}
}