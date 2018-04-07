using Tasks;
using UnityEngine;

namespace Sample
{
	public struct MoveCubeTask : ITask<CubeData, CubeData>
	{
		private readonly float acceleration;
		private readonly float targetSpeed;
		private readonly float deltaTime;

		public MoveCubeTask(float acceleration, float targetSpeed, float deltaTime)
		{
			this.acceleration = acceleration;
			this.targetSpeed = targetSpeed;
			this.deltaTime = deltaTime;
		}

		public CubeData Execute(CubeData input)
		{
			Vector3 toTarget = input.Target - input.Position;
			Vector3 targetVelocity = Vector3.ClampMagnitude(toTarget, targetSpeed);

			//Update velocity
			input.Velocity = Vector3.MoveTowards(input.Velocity, targetVelocity, acceleration * deltaTime);

			//Update position
			input.Position += input.Velocity * deltaTime;

			//Update rotation
			if(input.Velocity != Vector3.zero)
				input.Rotation = Quaternion.LookRotation(input.Velocity, Vector3.up);

			return input;
		}
	}
}