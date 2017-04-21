using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        float NullGaussian;

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

                NullGaussian = ComputeGaussian(0);
                float input = values[x];

                //if (input > 1)
                //    input = 1;

                for (int y = Min; y < Max; y++)
                {
                    float value = ComputeGaussian(Math.Abs(x - y)) * input * Height / NullGaussian;
                    //Diagram[y] += value;
                    if (value > Diagram[y])
                        Diagram[y] = value;
                }

                //Diagram[x] = input * Height;
            }

            for (int i = 0; i < Diagram.Length; i++)
                if (Diagram[i] > Height)
                    Diagram[i] = Height;
        }

        public float GetAverage()
        {
            if (Diagram != null)
            {
                float Avg = 0;
                for (int i = 0; i < Diagram.Length; i++)
                    Avg += Diagram[i];
                Avg /= Diagram.Length * Height;
                return Avg;
            }
            else
                return 0;
        }
        public void Smoothen()
        {
            if (Diagram != null)
            {
                for (int i = 0; i < Diagram.Length; i++)
                {
                    if (i > 0)
                        Diagram[i] += (Diagram[i - 1] - Diagram[i]) * 1;
                    if (i < Diagram.Length - 1)
                        Diagram[i] += (Diagram[i + 1] - Diagram[i]) * 1 / (1 + 1);

                    if (i > 1)
                        Diagram[i] += (Diagram[i - 2] - Diagram[i]) * 0.5f;
                    if (i < Diagram.Length - 2)
                        Diagram[i] += (Diagram[i + 2] - Diagram[i]) * 0.5f / (0.5f + 1);

                    if (i > 2)
                        Diagram[i] += (Diagram[i - 3] - Diagram[i]) * 0.25f;
                    if (i < Diagram.Length - 3)
                        Diagram[i] += (Diagram[i + 3] - Diagram[i]) * 0.25f / (0.25f + 1);

                    if (i > 3)
                        Diagram[i] += (Diagram[i - 4] - Diagram[i]) * 0.125f;
                    if (i < Diagram.Length - 4)
                        Diagram[i] += (Diagram[i + 4] - Diagram[i]) * 0.125f / (0.125f + 1);

                    if (i > 4)
                        Diagram[i] += (Diagram[i - 5] - Diagram[i]) * 0.0625f;
                    if (i < Diagram.Length - 5)
                        Diagram[i] += (Diagram[i + 5] - Diagram[i]) * 0.0625f / (0.0625f + 1);

                    if (i > 5)
                        Diagram[i] += (Diagram[i - 6] - Diagram[i]) * 0.03125f;
                    if (i < Diagram.Length - 6)
                        Diagram[i] += (Diagram[i + 6] - Diagram[i]) * 0.03125f / (0.03125f + 1);
                }
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
        public void DrawInputData(SpriteBatch spriteBatch)
        {
            if (Diagram != null)
            {
                if (WithShadow)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        float value = values[i];

                        Assets.DrawLine(new Vector2(i + P.X + 5, P.Y - (int)value * 100 + 5),
                                        new Vector2(i + P.X + 5, P.Y + 15),
                                        1, Color.Black * 0.6f, spriteBatch);
                    }
                }

                for (int i = 0; i < values.Length; i++)
                {
                    float value = values[i];

                    Assets.DrawLine(new Vector2(i + P.X, P.Y - (int)value * 100),
                                    new Vector2(i + P.X, P.Y + 10),
                                    1, Color.Lerp(XNA.primaryColor, XNA.secondaryColor, i / values.Length), spriteBatch);
                }
            }
        }
    }
}
