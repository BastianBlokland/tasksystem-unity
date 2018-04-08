using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class MoveCubeTask : ITask<CubeData>
	{
		public float CubeRadius = 1.25f;
		public float CubeSeperationForce = 2f;
		public float CubeVeloInheritance = .75f;
		public float TargetRadius = 5f;
		public float TargetSeperationForce = 10f;
		public float TargetVeloInheritance = 1f;
		public float DeltaTime = .1f;
		public Vector2 TargetPosition;
		public Vector2 TargetVelocity;
		
		private readonly GridPartitioner partitioner;
		private readonly PartitionSet<CubeData> others;

		public MoveCubeTask(GridPartitioner partitioner, PartitionSet<CubeData> others)
		{
			this.partitioner = partitioner;
			this.others = others;
		}

		public void Execute(ref CubeData data)
		{
			//Avoid others in our partition
			int partition = partitioner.Partition(data.Position);
			List<CubeData> neighbours = others.Get(partition);
			if(neighbours != null)
			{
				for (int i = 0; i < neighbours.Count; i++)
				{
					//Skip ourselves
					if(neighbours[i].ID == data.ID)
						continue;

					Avoid(ref data, neighbours[i].Position, CubeRadius, neighbours[i].Velocity, CubeSeperationForce, CubeVeloInheritance);
				}
			}

			//Avoid the target
			Avoid(ref data, TargetPosition, TargetRadius, TargetVelocity, TargetSeperationForce, TargetVeloInheritance);

			//Update position
			data.Position += data.Velocity * DeltaTime;
		}

		private void Avoid(ref CubeData data, Vector2 point, float pointRadius, Vector2 pointVelocity, float seperationForce, float veloInheritance)
		{
			float sharedRadius = CubeRadius + pointRadius;
			float sharedRadiusSqr = sharedRadius * sharedRadius;

			Vector2 toPoint = point - data.Position;
			float sqrDist = toPoint.sqrMagnitude;

			if(sqrDist < sharedRadiusSqr)
			{
				Vector2 seperationDir = sqrDist == 0f ? Vector2.up : -MathUtils.FastNormalize(toPoint);
				//Note: overlap is NOT linear because we're using the square distance
				float overLap = (sharedRadiusSqr - sqrDist) / sharedRadiusSqr;
				
				//Seperate
				data.Velocity += seperationDir * overLap * seperationForce;

				//Inherit velocity from target
				data.Velocity = Vector2.Lerp(data.Velocity, pointVelocity, overLap * veloInheritance);
			}
		}
	}
}