using UnityEngine;
using UnityEditor;

namespace Profiler
{
	[CustomEditor(typeof(TrackViewer))]
	public class TrackViewerEditor : Editor
	{
		const float TIMELINE_WIDTH = 750f;
		const float TIMELINE_HEIGHT = 250f;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			TrackViewer viewer = target as TrackViewer;
			if(viewer != null)
			{
				Rect rect = GUILayoutUtility.GetRect(TIMELINE_WIDTH, TIMELINE_HEIGHT);
				viewer.Draw(rect);

				//Keep refreshing
				Repaint();
			}
		}
	}
}