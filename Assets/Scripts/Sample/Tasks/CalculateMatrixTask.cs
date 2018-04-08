using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class CalculateMatrixTask : ITask<CubeData, Matrix4x4>
	{
		public void Execute(ref CubeData data, ref Matrix4x4 matrix)
		{
			matrix = new Matrix4x4
			(
				column0: new Vector4(1f, 0f, 0f, 0f),
				column1: new Vector4(0f, 1f, 0f, 0f),
				column2: new Vector4(0f, 0f, 1f, 0f),
				column3: new Vector4(data.Position.x, 0f, data.Position.y, 1f)
			);
		}
	}
}