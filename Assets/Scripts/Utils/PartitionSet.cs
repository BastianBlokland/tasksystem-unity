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
		private readonly ConcurrentDictionary<int, SubArray<T>> entries = new ConcurrentDictionary<int, SubArray<T>>();

		public PartitionSet(int maxPartitionEntryCount = 100)
		{
			this.maxPartitionEntryCount = maxPartitionEntryCount;
		}

		public void Add(int partition, T data)
		{
			SubArray<T> entry = entries.GetOrAdd(partition, (key) => new SubArray<T>(maxPartitionEntryCount));
			lock(entry)
			{
				entry.Add(data);
			}
		}

		public SubArray<T> Get(int partition)
		{
			SubArray<T> result = null;
			entries.TryGetValue(partition, out result);
			return result;
		}

		public void Clear()
		{
			foreach(KeyValuePair<int, SubArray<T>> entry in entries)
			{
				lock(entry.Value)
					entry.Value.Clear();
			}
		}
	}
}