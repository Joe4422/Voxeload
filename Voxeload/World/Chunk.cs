﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Voxeload.World
{
    public class Chunk : ITileAccess
    {
        public const int X_LENGTH = 16;
        public const int Y_LENGTH = 128;
        public const int Z_LENGTH = 16;

        private readonly byte[,,] tiles;

        private readonly Level level;

        public bool IsDirty { get; set; }

        public Chunk(Level level, IChunkGenerator generator, int x, int z)
        {
            tiles = generator.GenerateChunk(x, z);
            this.level = level;
        }

        public byte GetTileID(int x, int y, int z)
        {
            if (x < 0 || x >= X_LENGTH) return 0;
            if (y < 0 || y >= Y_LENGTH) return 0;
            if (z < 0 || z >= Z_LENGTH) return 0;

            return tiles[z, y, x];
        }

        public void SetTileID(int x, int y, int z, byte id)
        {
            if (x < 0 || x >= X_LENGTH) return;
            if (y < 0 || y >= Y_LENGTH) return;
            if (z < 0 || z >= Z_LENGTH) return;

            tiles[z, y, x] = id;

            IsDirty = true;
        }

        public byte GetVisibleSides(int x, int y, int z)
        {
            byte minusZ = GetTileID(x, y, z - 1);
            byte plusZ = GetTileID(x, y, z + 1);
            byte minusY = GetTileID(x, y - 1, z);
            byte plusY = GetTileID(x, y + 1, z);
            byte minusX = GetTileID(x - 1, y, z);
            byte plusX = GetTileID(x + 1, y, z);

            byte sides = 0;

            if (minusZ == 0) sides |= 1 << 0;
            if (plusZ == 0) sides |= 1 << 1;
            if (minusY == 0) sides |= 1 << 2;
            if (plusY == 0) sides |= 1 << 3;
            if (minusX == 0) sides |= 1 << 4;
            if (plusX == 0) sides |= 1 << 5;

            return sides;
        }
    }
}