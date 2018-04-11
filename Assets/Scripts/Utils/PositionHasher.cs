using UnityEngine;

namespace Utils
{
	public class PositionHasher
	{
		private const float MIN_CELL_SIZE = .001f;

		public float CellSize
		{
			get { return cellSize; }
			set { cellSize = Mathf.Max(MIN_CELL_SIZE, value); }
		}

		public float Fuzz
		{
			get { return fuzz; }
			set { fuzz = value; }
		}

		private float cellSize;
		private float fuzz;

		public PositionHasher(float cellSize = 10)
		{
			this.cellSize = Mathf.Max(MIN_CELL_SIZE, cellSize);
			this.fuzz = 0;
		}

		public int Hash(Vector2 value)
		{
			const int MAX_COLUMN_COUNT = 45000; //Grid of this by this will still fit in a int32
			return Hash(value.x) * MAX_COLUMN_COUNT + Hash(value.y);
		}

		public int Hash(float value)
		{
			float resultFloat = (value + cellSize * fuzz) / cellSize;

			//Note: Push away from 0 as we don't want hash 0 (it will make the above logic for Vector2's fail)
			if(resultFloat > 0)
				resultFloat += 1f;
			else
				resultFloat -= 1f;
			return (int)resultFloat;
		}
	}
}