using System.Collections.Generic;
using UnityEngine;

namespace Profiler
{
	public class TrackViewer : MonoBehaviour
	{
		private struct TrackEntry
		{
			public readonly string Label;
			public readonly ProfileTrack Track;

			public TrackEntry(string label, ProfileTrack track)
			{
				Label = label;
				Track = track;
			}
		}

		private readonly List<TrackEntry> tracks = new List<TrackEntry>();

		public T CreateTrack<T>(string label) where T : ProfileTrack, new()
		{
			T newTrack = new T();
			tracks.Add(new TrackEntry(label, newTrack));
			return newTrack;
		}
	}
}