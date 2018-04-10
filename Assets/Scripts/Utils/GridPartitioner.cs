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
			float resultFloat = (value + partitionSize * fuzz) / partitionSize;

			//Note: Push away from 0 as we don't want partition 0 (it will make the above logic for Vector2's fail)
			if(resultFloat > 0)
				resultFloat += 1f;
			else
				resultFloat -= 1f;
			return (int)resultFloat;
		}
	}
}