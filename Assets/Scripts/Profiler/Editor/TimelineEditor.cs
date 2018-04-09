using UnityEngine;
using UnityEditor;

namespace Profiler
{
	[CustomEditor(typeof(Timeline))]
	public class TimelineEditor : Editor
	{
		const float TIMELINE_WIDTH = 750f;
		const float TIMELINE_HEIGHT = 250f;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			Timeline timeline = target as Timeline;
			if(timeline != null)
			{
				Rect rect = GUILayoutUtility.GetRect(TIMELINE_WIDTH, TIMELINE_HEIGHT);

				//Draw background
				GUI.Box(rect, GUIContent.none);

				//Draw content
				GUI.color = Color.white;
				timeline.Draw(rect);

				//Keep refreshing
				Repaint();
			}
		}
	}
}