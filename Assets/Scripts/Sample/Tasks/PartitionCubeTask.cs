using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class BucketCubeTask : ITask<CubeData>
	{
		private readonly BucketSet<CubeData> bucketSet;
		private readonly PositionHasher hasher;

		public BucketCubeTask(BucketSet<CubeData> bucketSet, PositionHasher hasher)
		{
			this.bucketSet = bucketSet;
			this.hasher = hasher;
		}

		public void Execute(ref CubeData cubeData)
		{
			int hash = hasher.Hash(cubeData.Position);
			bucketSet.Add(hash, cubeData);
		}
	}
}