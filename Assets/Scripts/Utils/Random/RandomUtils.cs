using UnityEngine;

namespace Utils
{
	public static class RandomUtils
	{
		public static Vector2 Inside(this IRandomProvider random, Rect rect)
		{
			return rect.position + new Vector2
			(
				x: random.Between(0f, rect.width),
				y: random.Between(0f, rect.height)
			);
		}

		public static int Between(this IRandomProvider random, int minValue, int maxValue)
		{
			return Mathf.FloorToInt(random.Between((float)minValue, (float)maxValue));
		}

		public static float Between(this IRandomProvider random, float minValue, float maxValue)
		{
			return minValue + (maxValue - minValue) * random.GetNext();
		}
	}
}