using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public class RenderSet
	{
		//1023, is the max render count for a single call (https://docs.unity3d.com/ScriptReference/Graphics.DrawMeshInstanced.html)
		private const int CHUNK_SIZE = 1023;

		private struct Chunk
		{
			public Matrix4x4[] Data;
			public int Count;
		}

		private readonly Mesh mesh;
		private readonly Material material;

		private List<Chunk> chunks = new List<Chunk>();

		public RenderSet(Mesh mesh, Material material)
		{
			this.mesh = mesh;
			this.material = material;
		}

		public void Add(Matrix4x4 matrix)
		{
			for (int i = 0; i < chunks.Count; i++)
			{
				Chunk chunk = chunks[i];
				if(chunk.Count < CHUNK_SIZE)
				{
					chunk.Data[chunk.Count] = matrix;
					chunk.Count++;
					chunks[i] = chunk;
					return;
				}
			}

			Chunk newChunk = new Chunk
			{
				Data = new Matrix4x4[CHUNK_SIZE],
				Count = 1
			};
			newChunk.Data[0] = matrix;
			chunks.Add(newChunk);
		}

		public void Clear()
		{
			for (int i = 0; i < chunks.Count; i++)
			{
				Chunk chunk = chunks[i];
				chunk.Count = 0;
				chunks[i] = chunk;
			}
		}

		public void Render()
		{
			for (int i = 0; i < chunks.Count; i++)
			{
				Chunk chunk = chunks[i];
				if(chunk.Count > 0)
					Graphics.DrawMeshInstanced(mesh, 0, material, chunk.Data, chunk.Count);
			}
		}
	}
}