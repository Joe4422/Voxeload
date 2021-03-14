﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voxeload.World;

namespace Voxeload.Render
{
    public class ChunkMogffdeller
    {
        protected Queue<Chunk> chunksToModel = new();
        protected Queue<ChunkModel> completedModels = new();
        protected bool stopping = false;
        protected Level level;
        protected object lockObject = new();

        public ChunkModeller(Level level)
        {
            this.level = level;
            Thread t = new(() => Run());
            t.IsBackground = true;
            t.Priority = ThreadPriority.AboveNormal;
            t.Start();
        }

        protected void Run()
        {
            while (!stopping)
            {
                if (chunksToModel.Count > 0)
                {
                    Chunk chunk;
                    lock (lockObject)
                    {
                        chunk = chunksToModel.Dequeue();
                    }
                    LoadChunk(chunk);
                }
            }
        }

        public bool Request(Chunk chunk)
        {
            if (!chunksToModel.Contains(chunk))
            {
                lock (lockObject)
                {
                    chunksToModel.Enqueue(chunk);
                }
                return true;
            }
            else return false;
        }

        public ChunkModel Receive()
        {
            if (completedModels.Count > 0)
            {
                return completedModels.Dequeue();
            }

            return null;
        }

        protected void LoadChunk(Chunk chunk)
        {
            if (chunk == null) return;

            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<byte> faces = new();

            lock (chunk.chunkDataLock)
            {
                for (int z = 0; z < Chunk.Z_LENGTH; z++)
                {
                    for (int y = 0; y < Chunk.Y_LENGTH; y++)
                    {
                        for (int x = 0; x < Chunk.X_LENGTH; x++)
                        {
                            byte sides = level.GetVisibleSides((chunk.X * Chunk.X_LENGTH) + x, (chunk.Y * Chunk.Y_LENGTH) + y, (chunk.Z * Chunk.Z_LENGTH) + z);
                            byte id = chunk.GetTileID(x, y, z);

                            if (sides == 0 || id == 0) continue;

                            Tile tile = Tile.tiles[id];

                            if (tile == null) continue;

                            Vector3 offset = new(x, y, z);

                            TileModel model = tile.TileModel.GetModel(sides);

                            foreach (Vector3 vert in model.Vertices)
                            {
                                vertices.Add(vert + offset);
                            }

                            for (int i = 0; i < model.UVs.Length; i++)
                            {
                                int index = (int)tile.TileAppearance[(Tile.Face)model.UVFaces[i]];
                                int texX = index / 16 + index % 16;
                                int texY = 15 - (index / 16);

                                uvs.Add(new((model.UVs[i].X + texX) / 16, (model.UVs[i].Y + texY) / 16));
                            }

                            faces.AddRange(model.UVFaces);
                        }
                    }
                }
            }

            completedModels.Enqueue(new(chunk, vertices.ToArray(), uvs.ToArray(), faces.ToArray()));
        }
    }
}
