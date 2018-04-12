using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class RespawnCubeTask : ITask<CubeData>
	{
		public float MaxDistance = 200f;
		public Rect RespawnArea = new Rect(Vector2.zero, new Vector2(200f, 200f));
		public float RespawnForce = 1f;

		private readonly IRandomProvider random;

		public RespawnCubeTask(IRandomProvider random)
		{
			this.random = random;
		}

		public void Execute(ref CubeData data, int index, int batch)
		{
			if(data.Position.sqrMagnitude > MaxDistance * MaxDistance)
			{
				data = new CubeData
				{
					ID = data.ID,
					Position = random.Inside(RespawnArea),
					Velocity = random.Direction() * RespawnForce
				};
			}	
		}
	}
}