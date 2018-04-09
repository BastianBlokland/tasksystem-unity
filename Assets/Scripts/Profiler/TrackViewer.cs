using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Utils;

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

		public float CurrentTime { get { return (float)timer.Elapsed.TotalSeconds; } }
		public float LeftTime { get { return CurrentTime - timeRange; } }
		public float RightTime { get { return CurrentTime; } }

		[SerializeField] private float timeRange = 1f;

		private readonly List<TrackEntry> tracks = new List<TrackEntry>();
		private readonly Stopwatch timer = new Stopwatch();
		private readonly List<TrackItem> itemCache = new List<TrackItem>();

		private bool started;

		public T CreateTrack<T>(string label) where T : ProfileTrack, new()
		{
			T newTrack = new T();
			tracks.Add(new TrackEntry(label, newTrack));
			return newTrack;
		}

		public void Start()
		{
			for (int i = 0; i < tracks.Count; i++)
				tracks[i].Track.StartTimer();
			timer.Start();
			started = true;
		}

		public void Draw(Rect rect)
		{
			if(!started)
			{
				GUI.Label(rect, "Not yet started");
				return;
			}

			float trackHeight = (rect.height / tracks.Count) - 10;
			for (int i = 0; i < tracks.Count; i++)
			{
				Rect itemRect = new Rect(rect.x, rect.y + (rect.height / tracks.Count) * i, rect.width, trackHeight);
				DrawTrack(itemRect, tracks[i], LeftTime, RightTime, CurrentTime);
			}
		}

		private void DrawTrack(Rect rect, TrackEntry trackEntry, float leftTime, float rightTime, float currentTime)
		{
			trackEntry.Track.GetItems(itemCache);
			for (int i = 0; i < itemCache.Count; i++)
			{
				TrackItem item = itemCache[i];
				bool inView = MathUtils.DoesRangeOverlap(item.StartTime, item.StopTime, leftTime, rightTime);
				if(inView)
				{
					//Convert to progress in the view
					float p1 = Mathf.InverseLerp(leftTime, rightTime, item.StartTime);
					float p2 = Mathf.InverseLerp(leftTime, rightTime, item.Running ? currentTime : item.StopTime); 

					Rect itemRect = new Rect(rect.x + p1 * rect.width, rect.y, (p2 - p1) * rect.width, rect.height);
					GUI.DrawTexture(itemRect, Texture2D.whiteTexture);
				}
			}
		}
	}
}