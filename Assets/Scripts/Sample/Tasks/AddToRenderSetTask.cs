using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class AddToRenderSetTask : ITask<CubeData>
	{
		private readonly RenderSet renderSet;
		private readonly PositionHasher hasher;

		public AddToRenderSetTask(RenderSet renderSet, PositionHasher hasher)
		{
			this.renderSet = renderSet;
			this.hasher = hasher;
		}

		public void Execute(ref CubeData cube)
		{
			int hash = hasher.Hash(cube.Position);
			Matrix4x4 matrix = new Matrix4x4
			(
				column0: new Vector4(1f, 0f, 0f, 0f),
				column1: new Vector4(0f, 1f, 0f, 0f),
				column2: new Vector4(0f, 0f, 1f, 0f),
				column3: new Vector4(cube.Position.x, 0f, cube.Position.y, 1f)
			);
			renderSet.Add(hash, matrix);
		}
	}
}