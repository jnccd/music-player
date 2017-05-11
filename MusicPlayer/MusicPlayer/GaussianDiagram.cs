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
                int Min = x - (int)(theta * 2.5f);   if (Min < 0) Min = 0;
                int Max = x + (int)(theta * 2.5f);   if (Max > values.Length) Max = values.Length;

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

        public void ApplyAllOutputData(Action<float> A)
        {
            for (int i = 0; i < Diagram.Length; i++)
                A(Diagram[i]);
        }
        public void ApplyAllInputData(Action<float> A)
        {
            for (int i = 0; i < values.Length; i++)
                A(values[i]);
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
        public float GetAverage(int From, int To)
        {
            if (From < 0) From = 0;
            if (To > Diagram.Length - 1) To = Diagram.Length - 1;

            if (Diagram != null)
            {
                float Avg = 0;
                for (int i = From; i < To; i++)
                    Avg += Diagram[i];
                Avg /= Diagram.Length * Height;
                return Avg;
            }
            else
                return 0;
        }
        public float GetMaximum(int From, int To)
        {
            if (From < 0) From = 0;
            if (To > Diagram.Length - 1) To = Diagram.Length - 1;

            if (Diagram != null)
            {
                float Max = 0;
                for (int i = From; i < To; i++)
                    if (Diagram[i] > Max)
                        Max = Diagram[i];

                return Max;
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
        public void DrawAsBars(SpriteBatch spriteBatch)
        {
            if (Diagram != null)
            {
                float Width = Diagram.Length / 45;


                for (int i = 0; i < 49; i++)
                {
                    float Heigth = GetMaximum((Diagram.Length / 46) * i, (Diagram.Length / 46) * (i + 1));

                    if (WithShadow)
                    {
                        spriteBatch.Draw(Assets.White, new Rectangle((int)(i * Width + P.X) + 5, P.Y + 5,
                                                                    -(int)(Width * 0.7f),
                                                                    -(int)(Heigth + 10)),
                                     Color.Black * 0.6f);
                    }

                    spriteBatch.Draw(Assets.White, new Rectangle((int)(i * Width + P.X), P.Y,
                                                                 -(int)(Width * 0.7f),
                                                                 -(int)(Heigth + 10)),
                                     Color.Lerp(XNA.primaryColor, XNA.secondaryColor, i / 45f));
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

                        if (value > 1)
                            value = 1;

                        Assets.DrawLine(new Vector2(i + P.X + 5, P.Y - (int)(value * Height) + 5),
                                        new Vector2(i + P.X + 5, P.Y + 15),
                                        1, Color.Black * 0.6f, spriteBatch);
                    }
                }

                for (int i = 0; i < values.Length; i++)
                {
                    float value = values[i];

                    if (value > 1)
                        value = 1;

                    Assets.DrawLine(new Vector2(i + P.X, P.Y - (int)(value * Height)),
                                    new Vector2(i + P.X, P.Y + 10),
                                    1, Color.Lerp(XNA.primaryColor, XNA.secondaryColor, i / values.Length), spriteBatch);
                }
            }
        }
    }
}
