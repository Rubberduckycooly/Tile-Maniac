﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Drawing;

namespace RSDKv5
{
    public class TilesConfig
    {
        public static readonly byte[] MAGIC = new byte[] { (byte)'T', (byte)'I', (byte)'L', (byte)'\0' };

        const int TILES_COUNT = 0x400;

        public ColllisionMask[] CollisionPath1 = new ColllisionMask[TILES_COUNT];
        public ColllisionMask[] CollisionPath2 = new ColllisionMask[TILES_COUNT];

        public class ColllisionMask : ICloneable
        {
            public object Clone()
            {
                return this.MemberwiseClone();
            }

            // Collision position for each pixel
            public byte[] Collision;

            // If has collision
            public bool[] HasCollision;

            // Collision Mask config

            //The Slope's angle
            public byte slopeAngle;
            //How the player's physics react to the slope
            public byte physics;
            //How much momentum is gained from the slope
            public byte momentum;
            //Unknown
            public byte unknown;
            //for special occasions, like conveyor belts
            public byte special;
            // If is ceiling, the collision is from below
            public bool IsCeiling;

            public ColllisionMask()
            {
                Collision = new byte[16];
                HasCollision = new bool[16];
                IsCeiling = false;
                slopeAngle = 0;
                physics = 0;
                momentum = 0;
                unknown = 0;
                special = 0;
            }

            public ColllisionMask(Stream stream) : this(new Reader(stream)) { }

            public ColllisionMask(Reader reader)
            {
                Collision = reader.ReadBytes(16);
                HasCollision = reader.ReadBytes(16).Select(x => x != 0).ToArray();
                IsCeiling = reader.ReadBoolean();
                slopeAngle = reader.ReadByte();
                physics = reader.ReadByte();
                momentum = reader.ReadByte();
                unknown = reader.ReadByte();
                special = reader.ReadByte();
            }

            public void Write(System.IO.BinaryWriter writer)
            {

            }

            public void WriteUnc(System.IO.BinaryWriter writer)
            {
                writer.Write(Collision);
                for (int i = 0; i < HasCollision.Length; i++)
                { writer.Write(HasCollision[i]); }
                writer.Write(IsCeiling);
                writer.Write(slopeAngle);
                writer.Write(physics);
                writer.Write(momentum);
                writer.Write(unknown);
                writer.Write(special);
            }

            public Bitmap DrawCMask(System.Drawing.Color bg, System.Drawing.Color fg, Bitmap tile = null)
            {
                Bitmap b;
                bool HasTile = false;
                if (tile == null)
                { b = new Bitmap(16, 16); }
                else
                {
                 b = tile.Clone(new Rectangle(0, 0, tile.Width, tile.Height), System.Drawing.Imaging.PixelFormat.DontCare);
                    HasTile = true;

                }

                if (!HasTile)
                {
                    for (int h = 0; h < 16; h++) //Set the BG colour
                    {
                        for (int w = 0; w < 16; w++)
                        {
                            b.SetPixel(w, h, bg);
                        }
                    }
                }

                if (!IsCeiling)
                {
                    for (int w = 0; w < 16; w++) //Set the Active/Main (FG) colour
                    {
                        if (Collision[w] <= 15 && HasCollision[w])
                        {
                            b.SetPixel(w, 15, fg);
                        }
                        if (Collision[w] <= 14 && HasCollision[w])
                        {
                            b.SetPixel(w, 14, fg);
                        }
                        if (Collision[w] <= 13 && HasCollision[w])
                        {
                            b.SetPixel(w, 13, fg);
                        }
                        if (Collision[w] <= 12 && HasCollision[w])
                        {
                            b.SetPixel(w, 12, fg);
                        }
                        if (Collision[w] <= 11 && HasCollision[w])
                        {
                            b.SetPixel(w, 11, fg);
                        }
                        if (Collision[w] <= 10 && HasCollision[w])
                        {
                            b.SetPixel(w, 10, fg);
                        }
                        if (Collision[w] <= 9 && HasCollision[w])
                        {
                            b.SetPixel(w, 9, fg);
                        }
                        if (Collision[w] <= 8 && HasCollision[w])
                        {
                            b.SetPixel(w, 8, fg);
                        }
                        if (Collision[w] <= 7 && HasCollision[w])
                        {
                            b.SetPixel(w, 7, fg);
                        }
                        if (Collision[w] <= 6 && HasCollision[w])
                        {
                            b.SetPixel(w, 6, fg);
                        }
                        if (Collision[w] <= 5 && HasCollision[w])
                        {
                            b.SetPixel(w, 5, fg);
                        }
                        if (Collision[w] <= 4 && HasCollision[w])
                        {
                            b.SetPixel(w, 4, fg);
                        }
                        if (Collision[w] <= 3 && HasCollision[w])
                        {
                            b.SetPixel(w, 3, fg);
                        }
                        if (Collision[w] <= 2 && HasCollision[w])
                        {
                            b.SetPixel(w, 2, fg);
                        }
                        if (Collision[w] <= 1 && HasCollision[w])
                        {
                            b.SetPixel(w, 1, fg);
                        }
                        if (Collision[w] <= 0 && HasCollision[w])
                        {
                            b.SetPixel(w, 0, fg);
                        }
                    }
                }

                if (IsCeiling)
                {
                    for (int y = 0; y < 16; y++) //Set the Active/Main (FG) colour
                    {
                        for (int x = 0; x < 16; x++) //Set the Active/Main (FG) colour
                        {
                            b.SetPixel(x, y, fg);
                        }
                    }

                    for (int w = 0; w < 16; w++) //Set the Active/Main (FG) colour
                    {
                        if (Collision[w] <= 15)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 15, bg);
                        }
                        if (Collision[w] <= 14)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 14, bg);
                        }
                        if (Collision[w] <= 13)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 13, bg);
                        }
                        if (Collision[w] <= 12)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 12, bg);
                        }
                        if (Collision[w] <= 11)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 11, bg);
                        }
                        if (Collision[w] <= 10)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 10, bg);
                        }
                        if (Collision[w] <= 9)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 9, bg);
                        }
                        if (Collision[w] <= 8)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 8, bg);
                        }
                        if (Collision[w] <= 7)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 7, bg);
                        }
                        if (Collision[w] <= 6)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 6, bg);
                        }
                        if (Collision[w] <= 5)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 5, bg);
                        }
                        if (Collision[w] <= 4)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 4, bg);
                        }
                        if (Collision[w] <= 3)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 3, bg);
                        }
                        if (Collision[w] <= 2)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 2, bg);
                        }
                        if (Collision[w] <= 1)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 1, bg);
                        }
                        if (Collision[w] <= 0)//&& HasCollision[w])
                        {
                            b.SetPixel(w, 0, fg);
                        }
                    }
                }
                return b;
            }
        }

        public TilesConfig()
        {
            for (int i = 0; i < TILES_COUNT; ++i)
                CollisionPath1[i] = new ColllisionMask();
            for (int i = 0; i < TILES_COUNT; ++i)
                CollisionPath2[i] = new ColllisionMask();
        }

        public TilesConfig(string filename) : this(new Reader(filename))
        {

        }

        public TilesConfig(Stream stream) : this(new Reader(stream))
        {

        }

        private TilesConfig(Reader reader)
        {
            if (!reader.ReadBytes(4).SequenceEqual(MAGIC))
                throw new Exception("Invalid tiles config file header magic");

            using (Reader creader = reader.GetCompressedStream())
            {
                for (int i = 0; i < TILES_COUNT; ++i)
                    CollisionPath1[i] = new ColllisionMask(creader);
                for (int i = 0; i < TILES_COUNT; ++i)
                    CollisionPath2[i] = new ColllisionMask(creader);
            }
            reader.Close();
        }

        public TilesConfig(string filename, bool unc) : this(new Reader(filename), unc)
        {

        }

        public TilesConfig(Stream stream, bool unc) : this(new Reader(stream), unc)
        {

        }

        private TilesConfig(Reader reader, bool unc)
        {
            if (!reader.ReadBytes(4).SequenceEqual(MAGIC))
                throw new Exception("Invalid tiles config file header magic");
            if (!unc)
            {
                using (Reader creader = reader.GetCompressedStream())
                {
                    for (int i = 0; i < TILES_COUNT; ++i)
                        CollisionPath1[i] = new ColllisionMask(creader);
                    for (int i = 0; i < TILES_COUNT; ++i)
                        CollisionPath2[i] = new ColllisionMask(creader);
                }
            }
            else
            {
                for (int i = 0; i < TILES_COUNT; ++i)
                    CollisionPath1[i] = new ColllisionMask(reader);
                for (int i = 0; i < TILES_COUNT; ++i)
                    CollisionPath2[i] = new ColllisionMask(reader);
            }
            reader.Close();
        }

        public void Write(string filename)
        {
            Write(new Writer(filename));
        }

        public void Write(Stream s)
        {
            Write(new Writer(s));
        }

        // Write Compressed
        public void Write(Writer writer)
        {
            writer.Write(MAGIC);
            using (var stream = new MemoryStream())
            {
                using (var writerUncompressed = new BinaryWriter(stream))
                {
                    for (int i = 0; i < TILES_COUNT; ++i)
                        CollisionPath1[i].WriteUnc(writerUncompressed);
                    for (int i = 0; i < TILES_COUNT; ++i)
                        CollisionPath2[i].WriteUnc(writerUncompressed);
                }
                writer.WriteCompressed(stream.ToArray());
            }
            writer.Close();
        }

        public void WriteUnc(string filename)
        {
            WriteUnc(new Writer(filename));
        }

        public void WriteUnc(Stream s)
        {
            WriteUnc(new Writer(s));
        }

        // Write Uncompressed
        public void WriteUnc(Writer writer)
        {
            for (int i = 0; i < TILES_COUNT; ++i)
                CollisionPath1[i].WriteUnc(writer);
            for (int i = 0; i < TILES_COUNT; ++i)
                CollisionPath2[i].WriteUnc(writer);
            writer.Close();
        }
    }
}
