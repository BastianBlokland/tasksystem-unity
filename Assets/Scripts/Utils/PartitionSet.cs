using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public class PartitionSet<T>
		where T : struct
	{
		private class Entry
		{
			public readonly int Partition;
			public readonly List<T> Data = new List<T>();

			public Entry(int partition)
			{
				Partition = partition;
			}
		}

		public int PartitionCount { get { return entries.Count; } }

		private readonly List<Entry> entries = new List<Entry>();

		public void Add(int partition, T data)
		{
			Entry entry = Find(partition);
			if(entry == null)
			{
				entry = new Entry(partition);
				entries.Add(entry);
			}
			entry.Data.Add(data);
		}

		public List<T> Get(int partition)
		{
			Entry entry = Find(partition);
			if(entry != null)
				return entry.Data;
			return null;
		}

		public void Clear()
		{
			entries.Clear();
		}

		public void ClearData()
		{
			for (int i = 0; i < entries.Count; i++)
				entries[i].Data.Clear();
		}

		private Entry Find(int partition)
		{
			for (int i = 0; i < entries.Count; i++)
			{
				if(entries[i].Partition == partition)
					return entries[i];
			}
			return null;
		}
	}
}