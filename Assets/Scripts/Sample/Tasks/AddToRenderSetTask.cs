using System.Collections.Generic;
using Tasks;
using UnityEngine;
using Utils;

namespace Sample
{
	public class AddToRenderSetTask : ITask<Matrix4x4>
	{
		private readonly RenderSet renderSet;

		public AddToRenderSetTask(RenderSet renderSet)
		{
			this.renderSet = renderSet;
		}

		public void Execute(ref Matrix4x4 matrix)
		{
			renderSet.Add(matrix);
		}
	}
}