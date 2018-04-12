using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public class RenderSet
	{
		private struct BatchData
		{
			public readonly Matrix4x4[] Matrices;
			public readonly Vector4[] Colors;

			public BatchData(Matrix4x4[] matrices, Vector4[] colors)
			{
				Matrices = matrices;
				Colors = colors;
			}
		}

		//1023, is the max render count for a single call (https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstanced.html)
		public const int BATCH_SIZE = 1023;

		private volatile bool renderLock;
		private readonly Mesh mesh;
		private readonly Material material;
		private readonly MaterialPropertyBlock propertyBlock;
		private readonly BatchData[] batches;
		private readonly int[] batchSizes;

		public RenderSet(Mesh mesh, Material material, int maxBatches)
		{
			this.mesh = mesh;
			this.material = material;
			this.propertyBlock = new MaterialPropertyBlock();

			batches = new BatchData[maxBatches];
			batchSizes = new int[maxBatches];
			for (int i = 0; i < maxBatches; i++)
			{
				batches[i] = new BatchData
				(
					matrices: new Matrix4x4[BATCH_SIZE],
					colors: new Vector4[BATCH_SIZE]
				);
				batchSizes[i] = 0;
			}
		}

		/// <summary>
		/// NOTE: ONLY writing to the DIFFERENT batchNum's is threadsafe, so write to 1 batchNum per thread
		/// NOTE2: Every batch can only contain a maximum of 1023 elements!
		/// </summary>
		public void Add(int batchNum, Matrix4x4 matrix, Color color)
		{
			//Not allowed to add more items when we are rendering
			if(renderLock)
				return;

			if(batchSizes[batchNum] < BATCH_SIZE)
			{
				int index = batchSizes[batchNum];
				batches[batchNum].Matrices[index] = matrix;
				batches[batchNum].Colors[index] = color;
				batchSizes[batchNum]++;
			}
		}

		public void Clear()
		{
			for (int i = 0; i < batchSizes.Length; i++)
				batchSizes[i] = 0;
		}

		public void Render()
		{
			if(mesh == null)
				throw new Exception("[RenderSet] Unable to render: Mesh is null!");
			if(material == null)
				throw new Exception("[RenderSet] Unable to render: Material is null!");
			if(renderLock)
				throw new Exception("[RenderSet] There allready is a 'renderLock'");

			renderLock = true;
			for (int i = 0; i < batches.Length; i++)
			{
				if(batchSizes[i] > 0)
				{
					propertyBlock.SetVectorArray("_Color", batches[i].Colors);
					Graphics.DrawMeshInstanced(mesh, 0, material, batches[i].Matrices, batchSizes[i], propertyBlock);
				}
			}
			renderLock = false;
		}
	}
}