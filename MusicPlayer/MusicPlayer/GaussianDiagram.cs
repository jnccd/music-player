using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicPlayer
{
    class GaussianDiagram
    {
        Point P;
        float[] values;
        float[] Diagram;
        float theta;
        bool WithShadow;
        int Height;

        public GaussianDiagram()
        {
            this.values = null;
            Diagram = null;
            this.P = Point.Zero;
            this.theta = 0;
            this.WithShadow = false;
            this.Height = 0;
        }

        public GaussianDiagram(float[] values, Point P, int Height, bool WithShadow, float theta)
        {
            this.values = values;
            Diagram = new float[values.Length];
            this.P = P;
            this.theta = theta;
            this.WithShadow = WithShadow;
            this.Height = Height;

            for (int x = 0; x < values.Length; x++)
            {
                int Min = x - 15;   if (Min < 0) Min = 0;
                int Max = x + 15;   if (Max > values.Length) Max = values.Length;

                for (int y = Min; y < Max; y++)
                {
                    float value = ComputeGaussian(Math.Abs(x - y)) * values[x] * Height;
                    //Diagram[y] += value;
                    if (value > Diagram[y])
                        Diagram[y] = value;
                }

                if (values[x] > 1)
                    values[x] = 1;
            }
        }

        public float GetAverage()
        {
            if (Diagram != null)
            {
                float Avg = 0;
                for (int i = 0; i < values.Length; i++)
                    Avg += values[i];
                Avg /= values.Length;
                return Avg;
            }
            else
                return 0;
        }
        public void Smoothen()
        {
            for (int i = 1; i < Diagram.Length; i++)
            {
                
            }
        }
        private float ComputeGaussian(float n)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Diagram != null)
            {
                if (WithShadow)
                {
                    for (int i = 0; i < Diagram.Length; i++)
                    {
                        if (Diagram[i] > Height)
                            Diagram[i] = Height;

                        Assets.DrawLine(new Vector2(i + P.X + 5, P.Y - (int)Diagram[i] + 5),
                                        new Vector2(i + P.X + 5, P.Y + 15),
                                        1, Color.Black * 0.6f, spriteBatch);
                    }
                }

                for (int i = 0; i < Diagram.Length; i++)
                {
                    if (Diagram[i] > Height)
                        Diagram[i] = Height;

                    Assets.DrawLine(new Vector2(i + P.X, P.Y - (int)Diagram[i]),
                                    new Vector2(i + P.X, P.Y + 10),
                                    1, Color.Lerp(XNA.primaryColor, XNA.secondaryColor, i / values.Length), spriteBatch);
                }
            }
        }
    }
}
