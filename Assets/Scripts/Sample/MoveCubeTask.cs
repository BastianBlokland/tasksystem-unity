using Tasks;
using UnityEngine;

namespace Sample
{
	public struct MoveCubeTask : ITask<CubeData, CubeData>
	{
		private readonly float acceleration;
		private readonly float targetSpeed;
		private readonly float deltaTime;
		private readonly CubeData[] others;

		public MoveCubeTask(float acceleration, float targetSpeed, float deltaTime, CubeData[] others)
		{
			this.acceleration = acceleration;
			this.targetSpeed = targetSpeed;
			this.deltaTime = deltaTime;
			this.others = others;
		}

		public CubeData Execute(CubeData input)
		{
			const float RADIUS = 2f;
			const float DOUBLE_RADIUS = RADIUS * 2;
			const float SEPERATION_FORCE = .5f;

			Vector2 toTarget = input.Target - input.Position;
			Vector2 targetVelocity = Vector3.ClampMagnitude(toTarget, targetSpeed);

			//Avoid others
			for (int i = 0; i < others.Length; i++)
			{
				//Skip ourselves
				if(others[i].ID == input.ID)
					continue;

				Vector2 toOther = others[i].Position - input.Position;
				float sqrDist = toOther.sqrMagnitude;
				if(sqrDist < (DOUBLE_RADIUS * DOUBLE_RADIUS))
				{
					float dist = Mathf.Sqrt(sqrDist);
					float overLap = DOUBLE_RADIUS - dist;

					Vector2 seperationDir = dist == 0f ? Vector2.up : toOther / dist;
					input.Velocity -= seperationDir * overLap * SEPERATION_FORCE;
				}
			}

			//Accelerate towards target
			input.Velocity = Vector2.MoveTowards(input.Velocity, targetVelocity, acceleration * deltaTime);

			//Update position
			input.Position += input.Velocity * deltaTime;

			//Update rotation
			if(input.Velocity != Vector2.zero)
				input.Rotation = Vector2.SignedAngle(Vector2.up, input.Velocity);

			return input;
		}
	}
}