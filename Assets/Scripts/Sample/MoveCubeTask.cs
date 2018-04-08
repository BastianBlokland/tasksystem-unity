using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class MoveCubeTask : ITask<CubeData>
	{
		const float CUBE_RADIUS = 1.25f;
		const float CUBE_SEPERATION_FORCE = 2f;
		const float CUBE_VELO_INHERITANCE = .75f;
		const float TARGET_RADIUS = 5f;
		const float TARGET_SEPERATION_FORCE = 10f;
		const float TARGET_VELO_INHERITANCE = 1f;

		private readonly float deltaTime;
		private readonly Vector2 targetPosition;
		private readonly Vector2 targetVelocity;
		private readonly GridPartitioner partitioner;
		private readonly PartitionSet<CubeData> others;

		public MoveCubeTask(float deltaTime, Vector2 targetPosition, Vector2 targetVelocity, GridPartitioner partitioner, PartitionSet<CubeData> others)
		{
			this.deltaTime = deltaTime;
			this.targetPosition = targetPosition;
			this.targetVelocity = targetVelocity;
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

					Avoid(ref data, neighbours[i].Position, CUBE_RADIUS, neighbours[i].Velocity, CUBE_SEPERATION_FORCE, CUBE_VELO_INHERITANCE);
				}
			}

			//Avoid the target
			Avoid(ref data, targetPosition, TARGET_RADIUS, targetVelocity, TARGET_SEPERATION_FORCE, TARGET_VELO_INHERITANCE);

			//Update position
			data.Position += data.Velocity * deltaTime;

			//Update rotation
			if(data.Velocity != Vector2.zero)
				data.Rotation = Vector2.SignedAngle(Vector2.up, data.Velocity);
		}

		private void Avoid(ref CubeData data, Vector2 point, float pointRadius, Vector2 pointVelocity, float seperationForce, float veloInheritance)
		{
			float sharedRadius = CUBE_RADIUS + pointRadius;
			Vector2 toPoint = point - data.Position;
			float sqrDist = toPoint.sqrMagnitude;
			if(sqrDist < (sharedRadius * sharedRadius))
			{
				float dist = Mathf.Sqrt(sqrDist);
				float overLap = (sharedRadius - dist) / sharedRadius;

				Vector2 seperationDir = dist == 0f ? Vector2.up : toPoint / dist;
				//Seperate
				data.Velocity -= seperationDir * overLap * seperationForce;
				//Inherit velocity from target
				data.Velocity = Vector2.MoveTowards(data.Velocity, pointVelocity, overLap * veloInheritance);
			}
		}
	}
}