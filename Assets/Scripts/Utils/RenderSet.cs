using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utils
{
	public class RenderSet : IDisposable
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

		private readonly ReaderWriterLockSlim threadLock = new ReaderWriterLockSlim();
		private List<Chunk> chunks = new List<Chunk>();

		public RenderSet(Mesh mesh, Material material)
		{
			this.mesh = mesh;
			this.material = material;
		}

		public void Add(Matrix4x4 matrix)
		{
			threadLock.EnterWriteLock();
			{
				bool added = false;
				for (int i = 0; i < chunks.Count; i++)
				{
					Chunk chunk = chunks[i];
					if(chunk.Count < CHUNK_SIZE)
					{
						chunk.Data[chunk.Count] = matrix;
						chunk.Count++;
						chunks[i] = chunk;
						added = true;
						break;
					}
				}

				if(!added)
				{
					Chunk newChunk = new Chunk
					{
						Data = new Matrix4x4[CHUNK_SIZE],
						Count = 1
					};
					newChunk.Data[0] = matrix;
					chunks.Add(newChunk);
				}
			}
			threadLock.ExitWriteLock();
		}

		public void Clear()
		{
			threadLock.EnterWriteLock();
			{
				for (int i = 0; i < chunks.Count; i++)
				{
					Chunk chunk = chunks[i];
					chunk.Count = 0;
					chunks[i] = chunk;
				}
			}
			threadLock.ExitWriteLock();
		}

		public void Render()
		{
			threadLock.EnterReadLock();
			{
				for (int i = 0; i < chunks.Count; i++)
				{
					Chunk chunk = chunks[i];
					if(chunk.Count > 0)
						Graphics.DrawMeshInstanced(mesh, 0, material, chunk.Data, chunk.Count);
				}
			}
			threadLock.ExitReadLock();
		}

		public void Dispose()
		{
			threadLock.Dispose();
		}
	}
}