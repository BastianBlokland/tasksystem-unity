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

		public float Fuzz
		{
			get { return fuzz; }
			set { fuzz = value; }
		}

		private float partitionSize;
		private float fuzz;

		public GridPartitioner(float partitionSize = 10)
		{
			this.partitionSize = partitionSize;
			this.fuzz = 0;
		}

		public int Partition(Vector2 value)
		{
			return Partition(value.x) * Partition(value.y);
		}

		public int Partition(float value)
		{
			return Mathf.RoundToInt((value + partitionSize * fuzz) / partitionSize);
		}
	}
}