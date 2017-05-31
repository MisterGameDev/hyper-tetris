using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HyperTetris
{
    public class Shape
    {
        public const int ShapeSizeX = 2;
        public const int ShapeSizeY = 4;

        public int X { get; set; }
        public int Y { get; set; }

        public bool[,] Tiles { get; set; }
        public int Rotation { get; set; }

        public Color Color { get; set; }
        private Color[] fillColors = { Color.Orange, Color.Red, Color.Green, Color.Blue, Color.Violet };

        public Shape(Level level)
        {
            int shapeWidth = (Rotation == 0 || Rotation == 2) ? 2 : 4;

            X = level.Random.Next(Level.SizeX - shapeWidth);
            Y = 0;
            Rotation = level.Random.Next(4);
            Color = fillColors[level.Random.Next(fillColors.Length)];

            Tiles = new bool[ShapeSizeX, ShapeSizeY];
            int type = level.Random.Next(7);
            if (type == 0) // I
            {
                Tiles[0, 0] = true;
                Tiles[0, 1] = true;
                Tiles[0, 2] = true;
                Tiles[0, 3] = true;
            }
            else if (type == 1) // J
            {
                Tiles[1, 0] = true;
                Tiles[1, 1] = true;
                Tiles[1, 2] = true;
                Tiles[0, 2] = true;
            }
            else if (type == 2) // L
            {
                Tiles[0, 0] = true;
                Tiles[0, 1] = true;
                Tiles[0, 2] = true;
                Tiles[1, 2] = true;
            }
            else if (type == 3) // S
            {
                Tiles[0, 0] = true;
                Tiles[0, 1] = true;
                Tiles[1, 1] = true;
                Tiles[1, 2] = true;
            }
            else if (type == 4) // Z
            {
                Tiles[1, 0] = true;
                Tiles[1, 1] = true;
                Tiles[0, 1] = true;
                Tiles[0, 2] = true;
            }
            else if (type == 5) // T
            {
                Tiles[0, 1] = true;
                Tiles[1, 0] = true;
                Tiles[1, 1] = true;
                Tiles[1, 2] = true;
            }
            else if (type == 6) // O
            {
                Tiles[0, 0] = true;
                Tiles[0, 1] = true;
                Tiles[1, 0] = true;
                Tiles[1, 1] = true;
            }
        }

        public Shape(Shape src)
        {
            X = src.X;
            Y = src.Y;
            Tiles = src.Tiles;
            Rotation = src.Rotation;
            Color = src.Color;
        }
    }
}
