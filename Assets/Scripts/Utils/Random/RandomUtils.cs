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

		public static Vector2 Direction(this IRandomProvider random)
		{
			Vector2 dir = new Vector2(random.GetNext() - .5f, random.GetNext() - .5f);
			if(dir == Vector2.zero) //Should be very rare
				return Vector2.up;
			return MathUtils.FastNormalize(dir);
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