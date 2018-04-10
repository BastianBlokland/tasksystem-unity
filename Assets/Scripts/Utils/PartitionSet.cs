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
		private readonly ConcurrentDictionary<int, List<T>> entries = new ConcurrentDictionary<int, List<T>>();

		public void Add(int partition, T data)
		{
			List<T> entryList = entries.GetOrAdd(partition, (key) => new List<T>());
			lock(entryList)
			{
				entryList.Add(data);
			}
		}

		public List<T> Get(int partition)
		{
			List<T> result = null;
			entries.TryGetValue(partition, out result);
			return result;
		}

		public void Clear()
		{
			foreach(KeyValuePair<int, List<T>> entry in entries)
			{
				lock(entry.Value)
					entry.Value.Clear();
			}
		}
	}
}