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
		private readonly SubArray<T>[] buckets = new SubArray<T>[10000];

		public BucketSet(int maxBucketSize = 100)
		{
			for (int i = 0; i < buckets.Length; i++)
				buckets[i] = new SubArray<T>(maxBucketSize);
		}

		public void Add(int hash, T data)
		{
			SubArray<T> bucket = Get(hash);
			if(bucket != null)
				bucket.Add(data);
		}

		public SubArray<T> Get(int hash)
		{
			int bucketNum = HashUtils.GetBucket(hash, buckets.Length);
			return buckets[bucketNum];
		}

		public void Clear()
		{
			for (int i = 0; i < buckets.Length; i++)
				buckets[i].Clear();
		}
	}
}