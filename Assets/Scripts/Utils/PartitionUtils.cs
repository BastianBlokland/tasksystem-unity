using UnityEngine;

namespace Utils
{
	public class GridPartitioner
	{
		public float PartitionSize
		{
			get { return partitionSize; }
			set { partitionSize = value; }
		}

		private float partitionSize;

		public GridPartitioner(float partitionSize = 10)
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