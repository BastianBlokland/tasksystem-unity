using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class RespawnCubesTask : ITask<CubeData>
	{
		private const float MAX_DISTANCE = 200f;
		private const float MAX_DISTANCE_SQR = MAX_DISTANCE * MAX_DISTANCE;

		private readonly Vector2[] spawnPoints;

		public RespawnCubesTask(Vector2[] spawnPoints)
		{
			this.spawnPoints = spawnPoints;
		}

		public void Execute(ref CubeData data)
		{
			if(data.Position.sqrMagnitude > MAX_DISTANCE_SQR)
			{
				data = new CubeData
				{
					ID = data.ID,
					Position = spawnPoints[data.ID],
					Velocity = Vector2.zero,
					Rotation = 0f
				};
			}	
		}
	}
}