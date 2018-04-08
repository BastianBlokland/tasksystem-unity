using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class RespawnCubeTask : ITask<CubeData>
	{
		public float MaxDistance = 200f;

		private readonly Vector2[] spawnPoints;

		public RespawnCubeTask(Vector2[] spawnPoints)
		{
			this.spawnPoints = spawnPoints;
		}

		public void Execute(ref CubeData data)
		{
			if(data.Position.sqrMagnitude > MaxDistance * MaxDistance)
			{
				data = new CubeData
				{
					ID = data.ID,
					Position = spawnPoints[data.ID],
					Velocity = Vector2.zero
				};
			}	
		}
	}
}