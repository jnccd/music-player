using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MusicPlayer
{
    public static class GaussianValues
    {
        public static float GetGaussian(int n)
        {
            if (n > -theta && n < theta)
                return Gaussian[n + (int)theta];
            else
                return 0;
        }
        public static void CreateIfNotFilled(float theta)
        {
            if (Gaussian == null || theta != GaussianValues.theta)
            {
                Gaussian = new float[(int)theta * 2];
                GaussianValues.theta = theta;

                for (int i = 0; i < Gaussian.Length; i++)
                    Gaussian[i] = ComputeGaussian(i - (int)theta, theta);
            }
        }
        private static float ComputeGaussian(float n, float theta)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        private static float[] Gaussian;
        private static float theta;
    }

    class GaussianDiagram
    {
        Point P;
        float[] values;
        float[] Diagram;
        float theta;
        bool WithShadow;
        public bool AntiAlising;
        int Height;
        float NullGaussian;
        RenderTarget2D ShadowTarget;

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

            GaussianValues.CreateIfNotFilled(theta);

            for (int x = 0; x < values.Length; x++)
            {
                int Min = x - (int)(theta * 2.5f);   if (Min < 0) Min = 0;
                int Max = x + (int)(theta * 2.5f);   if (Max > values.Length) Max = values.Length;

                NullGaussian = GaussianValues.GetGaussian(0);
                float input = values[x];

                for (int y = Min; y < Max; y++)
                {
                    float value = GaussianValues.GetGaussian(Math.Abs(x - y)) * input * Height / NullGaussian;
                    if (value > Diagram[y])
                        Diagram[y] = value;
                }
            }

            for (int i = 0; i < Diagram.Length; i++)
                if (Diagram[i] > Height)
                    Diagram[i] = Height;
        }

        public void Update(float[] values)
        {
            for (int i = 0; i < Diagram.Length; i++)
                Diagram[i] = 0;

            this.values = values;
            GaussianValues.CreateIfNotFilled(theta);

            for (int x = 0; x < values.Length; x++)
            {
                int Min = x - (int)(theta * 2.5f); if (Min < 0) Min = 0;
                int Max = x + (int)(theta * 2.5f); if (Max > values.Length) Max = values.Length;

                NullGaussian = GaussianValues.GetGaussian(0);
                float input = values[x];

                for (int y = Min; y < Max; y++)
                {
                    float value = GaussianValues.GetGaussian(Math.Abs(x - y)) * input * Height / NullGaussian;
                    if (value > Diagram[y])
                        Diagram[y] = value;
                }
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

        public void DrawToRenderTarget(SpriteBatch spriteBatch, GraphicsDevice GD)
        {
            if (ShadowTarget == null)
                ShadowTarget = new RenderTarget2D(GD, Values.WindowSize.X, Values.WindowSize.Y);

            if (Diagram != null)
            {
                GD.SetRenderTarget(ShadowTarget);
                GD.Clear(Color.Transparent);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                #region DrawLeStuff
                for (int i = 0; i < Diagram.Length; i++)
                {
                    if (Diagram[i] > Height)
                        Diagram[i] = Height;

                    spriteBatch.Draw(Assets.White, new Rectangle(i + P.X, P.Y - (int)Diagram[i], 1, (int)Diagram[i] + 10), Color.White);

                    if (AntiAlising)
                    {
                        float NeighbourA;
                        float NeighbourB;

                        if (i > 0) NeighbourA = Diagram[i - 1];
                        else NeighbourA = 0;
                        if (i < Diagram.Length - 1) NeighbourB = Diagram[i + 1];
                        else NeighbourB = 0;

                        if (NeighbourA > Diagram[i])
                        {
                            spriteBatch.Draw(Assets.ColorFade, new Rectangle(i + P.X, P.Y - (int)Diagram[i] - (int)(NeighbourA - Diagram[i]),
                                1, (int)(NeighbourA - Diagram[i])),
                                Color.White);
                        }
                        else
                        {
                            if (NeighbourB > Diagram[i])
                            {
                                spriteBatch.Draw(Assets.ColorFade, new Rectangle(i + P.X, P.Y - (int)Diagram[i] - (int)(NeighbourB - Diagram[i]),
                                    1, (int)(NeighbourB - Diagram[i])),
                                    Color.White);
                            }
                        }
                    }
                }
                #endregion
                spriteBatch.End();
                GD.SetRenderTarget(null);
            }
        }
        public void DrawRenderTarget(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
            if (WithShadow)
                spriteBatch.Draw(ShadowTarget, new Rectangle(5, 5, Values.WindowSize.X, Values.WindowSize.Y), Color.Black * 0.6f);
            spriteBatch.Draw(ShadowTarget, new Rectangle(0, 0, Values.WindowSize.X, Values.WindowSize.Y), XNA.primaryColor);
            spriteBatch.End();
        }

        public void DrawAsBars(SpriteBatch spriteBatch)
        {
            if (Diagram != null)
            {
                float Width = Diagram.Length / 45;
                
                if (WithShadow)
                {
                    for (int i = 0; i < 48; i++)
                    {
                        int H = (int)GetMaximum((int)(i * Width), (int)((i + 1) * Width));

                        for (int j = 0; j < (int)(Width / 1.2f); j++)
                            Assets.DrawLine(new Vector2(i * Width + j + P.X + 5, P.Y - H + 5),
                                            new Vector2(i * Width + j + P.X + 5, P.Y + (int)(Width / 1.2f) + 5),
                                            1, Color.Black * 0.6f, spriteBatch);
                    }
                }

                for (int i = 0; i < 48; i++)
                {
                    int H = (int)GetMaximum((int)(i * Width), (int)((i + 1) * Width));

                    for (int j = 0; j < (int)(Width / 1.2f); j++)
                        Assets.DrawLine(new Vector2(i * Width + j + P.X, P.Y - H),
                                        new Vector2(i * Width + j + P.X, P.Y + (int)(Width / 1.2f)),
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
