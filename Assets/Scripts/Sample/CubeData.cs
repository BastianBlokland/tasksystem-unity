using Tasks;
using UnityEngine;

namespace Sample
{
	public struct CubeData : ITaskData
	{
		public int ID;
		public Vector2 Position;
		public Vector2 Velocity;
		public float Rotation;
	}
}