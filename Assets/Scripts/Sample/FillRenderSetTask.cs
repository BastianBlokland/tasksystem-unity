using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class FillRenderSetTask : ITask<Matrix4x4>
	{
		private readonly RenderSet renderSet;

		public FillRenderSetTask(RenderSet renderSet)
		{
			this.renderSet = renderSet;
		}

		public void Execute(ref Matrix4x4 matrix)
		{
			renderSet.Add(matrix);
		}
	}
}