using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class PartitionCubesTask : ITask<CubeData>
	{
		private readonly PartitionSet<CubeData> partitionSet;
		private readonly GridPartitioner partitioner;

		public PartitionCubesTask(PartitionSet<CubeData> partitionSet, GridPartitioner partitioner)
		{
			this.partitionSet = partitionSet;
			this.partitioner = partitioner;
		}

		public void Execute(ref CubeData cubeData)
		{
			int partition = partitioner.Partition(cubeData.Position);
			partitionSet.Add(partition, cubeData);
		}
	}
}