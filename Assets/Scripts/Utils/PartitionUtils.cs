using UnityEngine;

namespace Utils
{
	public class GridPartitioner
	{
		private readonly float partitionSize;

		public GridPartitioner(float partitionSize)
		{
			this.partitionSize = partitionSize;
		}

		public int Partition(Vector2 value)
		{
			return Partition(value.x) * Partition(value.y);
		}

		public int Partition(float value)
		{
			return Mathf.RoundToInt(value / partitionSize);
		}
	}
}