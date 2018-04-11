using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public class PartitionSet<T>
		where T : struct
	{
		private readonly int maxPartitionEntryCount;
		private readonly SubArray<T>[] buckets = new SubArray<T>[10000];

		public PartitionSet(int maxPartitionEntryCount = 100)
		{
			this.maxPartitionEntryCount = maxPartitionEntryCount;

			for (int i = 0; i < buckets.Length; i++)
				buckets[i] = new SubArray<T>(maxPartitionEntryCount);
		}

		public void Add(int partition, T data)
		{
			SubArray<T> bucket = Get(partition);
			if(bucket != null)
				bucket.Add(data);
		}

		public SubArray<T> Get(int partition)
		{
			int bucketNum = GetBucket(partition, buckets.Length);
			return buckets[bucketNum];
		}

		public void Clear()
		{
			for (int i = 0; i < buckets.Length; i++)
				buckets[i].Clear();
		}

		private static int GetBucket(int hashcode, int bucketCount)
        {
            int bucketNo = (hashcode & 0x7fffffff) % bucketCount;
            return bucketNo;
        }
	}
}