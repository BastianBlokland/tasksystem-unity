using UnityEngine;

namespace Utils
{
	public class GridPartitioner
	{
		private const float MIN_PARTITION_SIZE = .001f;

		public float PartitionSize
		{
			get { return partitionSize; }
			set { partitionSize = Mathf.Max(MIN_PARTITION_SIZE, value); }
		}

		public float Fuzz
		{
			get { return fuzz; }
			set { fuzz = value; }
		}

		private float partitionSize;
		private float fuzz;

		public GridPartitioner(float partitionSize = 10)
		{
			this.partitionSize = Mathf.Max(MIN_PARTITION_SIZE, partitionSize);
			this.fuzz = 0;
		}

		public int Partition(Vector2 value)
		{
			return Partition(value.x) * Partition(value.y);
		}

		public int Partition(float value)
		{
			return (int)((value + partitionSize * fuzz) / partitionSize);
		}
	}
}