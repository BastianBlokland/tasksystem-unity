using Tasks;
using UnityEngine;

namespace Sample
{
	public struct CubeData : ITaskData
	{
		public Vector3 Position;
		public Vector3 Velocity;
		public Quaternion Rotation;
		public Vector3 Target;
	}
}