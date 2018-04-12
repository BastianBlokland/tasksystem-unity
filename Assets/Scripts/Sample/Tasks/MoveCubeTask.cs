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
		public Vector2 Target1Position;
		public Vector2 Target1Velocity;
		public Vector2 Target2Position;
		public Vector2 Target2Velocity;
		
		private readonly PositionHasher hasher;
		private readonly BucketSet<CubeData> cubeLookup;

		public MoveCubeTask(PositionHasher hasher, BucketSet<CubeData> cubeLookup)
		{
			this.hasher = hasher;
			this.cubeLookup = cubeLookup;
		}

		public void Execute(ref CubeData data, int index, int batch)
		{
			//Avoid others in our cell
			int hash = hasher.Hash(data.Position);
			int neighbourAmount = cubeLookup.GetBucketSize(hash);
			for (int i = 0; i < neighbourAmount; i++)
			{
				CubeData neighbour = cubeLookup.GetBucketElement(hash, i);
				
				//Skip ourselves
				if(neighbour.ID == data.ID)
					continue;

				Avoid(ref data, neighbour.Position, CubeRadius, neighbour.Velocity, CubeSeperationForce, CubeVeloInheritance);
			}

			//Avoid target 1
			if(Avoid(ref data, Target1Position, TargetRadius, Target1Velocity, TargetSeperationForce, TargetVeloInheritance))
				data.TimeNotHitTarget1 = 0f;
			else
				data.TimeNotHitTarget1 += DeltaTime;

			//Avoid target 2
			if(Avoid(ref data, Target2Position, TargetRadius, Target2Velocity, TargetSeperationForce, TargetVeloInheritance))
				data.TimeNotHitTarget2 = 0f;
			else
				data.TimeNotHitTarget2 += DeltaTime;

			//Update position
			data.Position += data.Velocity * DeltaTime;
		}

		private bool Avoid(ref CubeData data, Vector2 point, float pointRadius, Vector2 pointVelocity, float seperationForce, float veloInheritance)
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

				return true;	
			}
			return false;
		}
	}
}