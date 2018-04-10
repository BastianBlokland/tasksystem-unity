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
		private const int CHUNK_SIZE = 1023;

		private class Chunk
		{
			public Matrix4x4[] Data;
			public int Count;
		}

		private readonly Mesh mesh;
		private readonly Material material;

		private readonly ConcurrentDictionary<int, Chunk> chunks = new ConcurrentDictionary<int, Chunk>();

		public RenderSet(Mesh mesh, Material material)
		{
			this.mesh = mesh;
			this.material = material;
		}

		/// <summary>
		/// Important note: Every partition can only contain a maximum of 1023 elements!
		/// </summary>
		public void Add(int partition, Matrix4x4 matrix)
		{
			Chunk chunk = chunks.GetOrAdd(partition, (key) => new Chunk { Data = new Matrix4x4[CHUNK_SIZE], Count = 0 });
			lock(chunk)
			{
				//NOTE: Very important to realize that we DON'T render the object if there is no space in the chunk anymore
				if(chunk.Count < CHUNK_SIZE)
				{
					chunk.Data[chunk.Count] = matrix;
					chunk.Count++;
				}
			}
		}

		public void Clear()
		{
			foreach(KeyValuePair<int, Chunk> kvp in chunks)
			{
				lock(kvp.Value)
					kvp.Value.Count = 0;
			}
		}

		public void Render()
		{
			if(mesh == null)
				throw new Exception("[RenderSet] Unable to render: Mesh is null!");
			if(material == null)
				throw new Exception("[RenderSet] Unable to render: Material is null!");
				
			foreach(KeyValuePair<int, Chunk> kvp in chunks)
			{
				lock(kvp.Value)
				{
					if(kvp.Value.Count > 0)
						Graphics.DrawMeshInstanced(mesh, 0, material, kvp.Value.Data, kvp.Value.Count);
				}
			}
		}
	}
}