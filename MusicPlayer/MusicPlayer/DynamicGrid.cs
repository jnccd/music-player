using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace MusicPlayer
{
    public class DynamicGrid
    {
        public GridPoint[,] Points;
        List<GridSpring> VertSprings = new List<GridSpring>();
        List<GridSpring> HorzSprings = new List<GridSpring>();
        public Rectangle Field;
        public int PointSpacing;
        public float Damping;
        public float Traction;

        public DynamicGrid(Rectangle Field, int PointSpacing, float Damping, float Traction)
        {
            this.PointSpacing = PointSpacing;
            this.Damping = Damping;
            this.Traction = Traction;

            // Create the Points
            Points = new GridPoint[Field.Width / PointSpacing + 2, Field.Height / PointSpacing + 2];
            for (int ix = 0; ix < Points.GetLength(0); ix++)
            {
                for (int iy = 0; iy < Points.GetLength(1); iy++)
                {
                    if (iy == Points.GetLength(1) - 1 || ix == 0 || ix == Points.GetLength(0) - 1)
                    {
                        Points[ix, iy] = new GridPoint(new Vector2(ix * PointSpacing + Field.X, (Points.GetLength(1) - 1) * PointSpacing + Field.Y), false, this);
                    }
                    else
                    {
                        Points[ix, iy] = new GridPoint(new Vector2(ix * PointSpacing + Field.X, (Points.GetLength(1) - 1) * PointSpacing + Field.Y), true, this);
                    }
                }
            }

            // Create the horz Springs
            for (int ix = 0; ix < Points.GetLength(0) - 1; ix++)
            {
                for (int iy = 0; iy < Points.GetLength(1); iy++)
                {
                    HorzSprings.Add(new GridSpring(Points[ix, iy], Points[ix + 1, iy], this));
                }
            }

            // Create the vert Springs
            for (int ix = 0; ix < Points.GetLength(0); ix++)
            {
                for (int iy = 0; iy < Points.GetLength(1) - 1; iy++)
                {
                    VertSprings.Add(new GridSpring(Points[ix, iy], Points[ix, iy + 1], this));
                }
            }

            this.Field = new Rectangle(Field.X, Field.Y, Points.GetLength(0) * PointSpacing, Points.GetLength(1) * PointSpacing);
        }

        public void ApplyForce(Vector2 Pos, float Strength)
        {
            lock (Points)
            {
                for (int ix = 0; ix < Points.GetLength(0); ix++)
                {
                    for (int iy = 0; iy < Points.GetLength(1); iy++)
                    {
                        Points[ix, iy].GetPulledBy(Pos, Strength * 25);
                    }
                }
            }
        }
        public void Twist(Vector2 Pos, float Strength, float DeviationAngle)
        {
            lock (Points)
            {
                for (int ix = 0; ix < Points.GetLength(0); ix++)
                {
                    for (int iy = 0; iy < Points.GetLength(1); iy++)
                    {
                        Points[ix, iy].OrbitAround(Pos, Strength * 25, DeviationAngle);
                    }
                }
            }
        }

        private void UpdatePoints()
        {
            lock (Points)
            {
                for (int ix = 0; ix < Points.GetLength(0); ix++)
                {
                    for (int iy = 0; iy < Points.GetLength(1); iy++)
                    {
                        Points[ix, iy].Update();
                    }
                }
            }
        }
        private void UpdateHorzSprings()
        {
            lock (HorzSprings)
            {
                for (int i = 0; i < HorzSprings.Count; i++)
                {
                    HorzSprings[i].Update();
                }
            }
        }
        private void UpdateVertSprings()
        {
            lock (VertSprings)
            {
                for (int i = 0; i < VertSprings.Count; i++)
                {
                    VertSprings[i].Update();
                }
            }
        }

        public void Clear()
        {
            for (int ix = 0; ix < Points.GetLength(0); ix++)
            {
                for (int iy = 0; iy < Points.GetLength(1); iy++)
                {
                    Points[ix, iy].Pos = new Vector2(ix * PointSpacing + Field.X, (Points.GetLength(1) - 1) * PointSpacing + Field.Y);
                    Points[ix, iy].Vel = Vector2.Zero;
                }
            }
        }

        public void Update()
        {
            //Task.Factory.StartNew(() => UpdateHorzSprings());
            //Task.Factory.StartNew(() => UpdateVertSprings());
            UpdateHorzSprings();
            UpdateVertSprings();
            UpdatePoints();
        }
        public void Draw(SpriteBatch SB)
        {
            lock (HorzSprings)
            {
                for (int i = 0; i < HorzSprings.Count; i++)
                {
                    if (i < HorzSprings.Count)
                    {
                        HorzSprings[i].Draw(SB);
                    }
                }
            }
            lock (VertSprings)
            {
                for (int i = 0; i < VertSprings.Count; i++)
                {
                    if (i < VertSprings.Count)
                    {
                        VertSprings[i].Draw(SB);
                    }
                }
            }
            lock (Points)
            {
                for (int ix = 0; ix < Points.GetLength(0); ix++)
                {
                    for (int iy = 0; iy < Points.GetLength(1); iy++)
                    {
                        Points[ix, iy].Draw(SB);
                    }
                }
            }
        }
        public void DrawShadow(SpriteBatch SB)
        {
            lock (HorzSprings)
            {
                for (int i = 0; i < HorzSprings.Count; i++)
                {
                    if (i < HorzSprings.Count)
                    {
                        HorzSprings[i].DrawAsShadow(SB);
                    }
                }
            }
            lock (VertSprings)
            {
                for (int i = 0; i < VertSprings.Count; i++)
                {
                    if (i < VertSprings.Count)
                    {
                        VertSprings[i].DrawAsShadow(SB);
                    }
                }
            }
        }
    }
}
