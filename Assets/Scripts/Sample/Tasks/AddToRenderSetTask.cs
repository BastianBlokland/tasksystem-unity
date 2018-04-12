using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class AddToRenderSetTask : ITask<CubeData>
	{
		public Vector4 HitByTarget1Color;
		public Vector4 HitByTarget2Color;

		private readonly RenderSet renderSet;

		public AddToRenderSetTask(RenderSet renderSet)
		{
			this.renderSet = renderSet;
		}

		public void Execute(ref CubeData cube, int index, int batch)
		{
			Matrix4x4 matrix = new Matrix4x4
			(
				column0: new Vector4(1f, 0f, 0f, 0f),
				column1: new Vector4(0f, 1f, 0f, 0f),
				column2: new Vector4(0f, 0f, 1f, 0f),
				column3: new Vector4(cube.Position.x, 0f, cube.Position.y, 1f)
			);

			Vector4 color = Color.gray;
			if(cube.TimeNotHitTarget1 < 10f)
				color = Vector4.Lerp(HitByTarget1Color, color, cube.TimeNotHitTarget1 * .1f);
			if(cube.TimeNotHitTarget2 < 10f)
				color = Vector4.Lerp(HitByTarget2Color, color, cube.TimeNotHitTarget2 * .1f);

			renderSet.Add(batch, matrix, color);
		}
	}
}