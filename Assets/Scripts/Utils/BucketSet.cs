using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public class BucketSet<T>
		where T : struct
	{
		private readonly int bucketCount;
		private readonly int maxBucketSize;
		private readonly T[] data;
		private readonly int[] bucketSizes;
		private readonly object[] writeLocks;

		public BucketSet(int bucketCount = 10000, int maxBucketSize = 10)
		{
			this.bucketCount = bucketCount;
			this.maxBucketSize = maxBucketSize;

			data = new T[bucketCount * maxBucketSize];

			bucketSizes = new int[bucketCount];
			for (int i = 0; i < bucketSizes.Length; i++)
				bucketSizes[i] = 0;

			writeLocks = new object[bucketCount];
			for (int i = 0; i < bucketSizes.Length; i++)
				writeLocks[i] = new object();
		}

		public void Add(int hash, T item)
		{
			int bucketNum = HashUtils.GetBucket(hash, bucketCount);

			if(bucketSizes[bucketNum] < maxBucketSize)
			{
				lock(writeLocks[bucketNum])
				{
					int index = bucketNum * maxBucketSize + bucketSizes[bucketNum];
					data[index] = item;
					bucketSizes[bucketNum]++;
				}
			}
		}

		public int GetBucketSize(int hash)
		{
			int bucketNum = HashUtils.GetBucket(hash, bucketCount);
			return bucketSizes[bucketNum];
		}

		public T GetBucketElement(int hash, int index)
		{
			int bucketNum = HashUtils.GetBucket(hash, bucketCount);
			int startIndex = bucketNum * maxBucketSize;
			return data[startIndex + index];
		}

		public void Clear()
		{
			for (int i = 0; i < bucketSizes.Length; i++)
				bucketSizes[i] = 0;
		}
	}
}