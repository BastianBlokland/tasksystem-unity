using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public class RenderSet
	{
		//1023, is the max render count for a single call (https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstanced.html)
		private const int BATCH_SIZE = 1023;

		private volatile bool renderLock;
		private readonly Mesh mesh;
		private readonly Material material;
		private readonly SubArray<Matrix4x4>[] buckets;

		public RenderSet(Mesh mesh, Material material, int maxBatches)
		{
			this.mesh = mesh;
			this.material = material;

			buckets = new SubArray<Matrix4x4>[maxBatches];
			for (int i = 0; i < maxBatches; i++)
				buckets[i] = new SubArray<Matrix4x4>(BATCH_SIZE);
		}

		/// <summary>
		/// Important note: Every cell can only contain a maximum of 1023 elements!
		/// </summary>
		public void Add(int hash, Matrix4x4 matrix)
		{
			//Not allowed to add more items when we are rendering
			if(renderLock)
				return;

			int bucketNum = HashUtils.GetBucket(hash, buckets.Length);
			SubArray<Matrix4x4> batch = buckets[bucketNum];
			
			//NOTE: Very important to realize that we DON'T render the object if there is no space in the batch anymore
			batch.Add(matrix);
		}

		public void Clear()
		{
			for (int i = 0; i < buckets.Length; i++)
				buckets[i].Clear();
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
			for (int i = 0; i < buckets.Length; i++)
			{
				if(buckets[i].Count > 0)
				{
					Graphics.DrawMeshInstanced(mesh, 0, material, buckets[i].Data, buckets[i].Count);
				}
			}
			renderLock = false;
		}
	}
}