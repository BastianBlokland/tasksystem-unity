using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Utils;

namespace Profiler
{
	public class Timeline : MonoBehaviour
	{
		private struct TrackEntry
		{
			public readonly string Label;
			public readonly TimelineTrack Track;

			public TrackEntry(string label, TimelineTrack track)
			{
				Label = label;
				Track = track;
			}
		}

		public float CurrentTime { get { return (float)timer.Elapsed.TotalSeconds; } }
		public float LeftTime { get { return RightTime - timeRange; } }
		public float RightTime { get { return viewTime; } }

		[Range(.01f, 1f)]
		[SerializeField] private float timeRange = .5f;
		[SerializeField] private bool paused;

		private readonly List<TrackEntry> tracks = new List<TrackEntry>();
		private readonly Stopwatch timer = new Stopwatch();
		private readonly List<TimelineItem> itemCache = new List<TimelineItem>();
		private readonly Color[] trackColors = new Color[] { Color.blue, Color.green, Color.yellow, Color.cyan, Color.magenta, Color.red };

		private float viewTime;
		private bool started;

		public T CreateTrack<T>(string label) where T : TimelineTrack, new()
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
			const float HEADER_HEIGHT = 20f;
			const float SPACING = 10f;

			if(!started)
			{
				GUI.Label(rect, "Not yet started");
				return;
			}
			if(!paused)
				viewTime = CurrentTime;

			for (int i = 0; i < tracks.Count; i++)
			{
				Rect itemRect = new Rect(rect.x, rect.y + (rect.height / tracks.Count) * i, rect.width, (rect.height / tracks.Count) - SPACING);

				//Draw header
				GUI.color = Color.white;
				GUI.Label(new Rect(itemRect.x, itemRect.y, itemRect.width, HEADER_HEIGHT), tracks[i].Label);
				
				//Draw content
				Rect contentRect = new Rect(itemRect.x, itemRect.y + HEADER_HEIGHT, itemRect.width, Mathf.Max(1f, itemRect.height - HEADER_HEIGHT));

				GUI.color = Color.gray;
				GUI.DrawTexture(contentRect, Texture2D.whiteTexture);

				GUI.color = trackColors[i % trackColors.Length];
				DrawTrack(contentRect, tracks[i], LeftTime, RightTime, CurrentTime);
			}
		}

		private void DrawTrack(Rect rect, TrackEntry trackEntry, float leftTime, float rightTime, float currentTime)
		{
			trackEntry.Track.GetItems(itemCache);
			for (int i = 0; i < itemCache.Count; i++)
			{
				TimelineItem item = itemCache[i];
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