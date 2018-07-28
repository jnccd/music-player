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

    public class GaussianDiagram : IDisposable
    {
        public Point P;
        float[] values;
        float[] Diagram;
        float theta;
        bool WithShadow;
        int Height;
        int TargetHeight;
        float NullGaussian;
        RenderTarget2D ShadowTarget;
        Rectangle DrawRect;
        SpriteBatch SB;
        GraphicsDevice GD;
        public float DiagramSize;

        //List<long> DebugTimes = new List<long>();
        //long DebugTime;

        // 3D Acceleration
        VertexPositionColor[] pointList;
        short[] Indices;
        VertexPositionColor[] AA_VPC;
        short[] AA_indices;

        public GaussianDiagram(float[] values, Point P, int Height, bool WithShadow, float theta, GraphicsDevice GD)
        {
            this.values = values;
            Diagram = new float[values.Length];
            this.P = P;
            this.theta = theta;
            this.WithShadow = WithShadow;
            this.Height = Height;
            TargetHeight = Height;

            DiagramSize = 1;
            DrawRect = new Rectangle(0, 0, 1, 0);
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

            ShadowTarget = new RenderTarget2D(GD, Values.WindowSize.X, Values.WindowSize.Y);

            

            pointList = new VertexPositionColor[Diagram.Length * 2];
            for (int i = 0; i < Diagram.Length; i++)
            {
                pointList[i*2] = new VertexPositionColor(new Vector3(i + P.X, P.Y + 10, 0), Color.White);
                pointList[i*2 + 1] = new VertexPositionColor(new Vector3(i + P.X, P.Y - Diagram[i], 0), Color.White);
            }

            Indices = new short[pointList.Length];
            for (int i = 0; i < pointList.Length; i++)
            {
                Indices[i] = (short)(i);
            }

            AA_VPC = new VertexPositionColor[4];
            AA_VPC[0] = new VertexPositionColor(new Vector3(100, 25, 0), Color.White);
            AA_VPC[1] = new VertexPositionColor(new Vector3(100, 100, 0), Color.Transparent);
            AA_VPC[2] = new VertexPositionColor(new Vector3(100, 26, 0), Color.White);
            AA_VPC[3] = new VertexPositionColor(new Vector3(100, 26, 0), Color.Transparent);

            AA_indices = new short[4];
            AA_indices[0] = 0;
            AA_indices[1] = 1;
            AA_indices[2] = 2;
            AA_indices[3] = 3;
        }

        public int Length { get { return Diagram.Length; } }
        public int BaseHeight { get { return Height / 25; } }
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
        public float GetMaximum()
        {
            if (Diagram != null)
            {
                float Max = 0;
                for (int i = 0; i < Diagram.Length - 1; i++)
                    if (Diagram[i] > Max)
                        Max = Diagram[i];

                return Max;
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
        public void NewSmoothen(float Smoothness)
        {
            if (Diagram != null)
            {
                int width = (int)(6 * Smoothness);
                if (width > 12)
                    width = 12;

                for (int d = 1; d <= 6; d++)
                {
                    for (int i = 0; i < Diagram.Length - d; i++)
                    {
                        float Mult = Smoothness / d;

                        if (Mult > 1)
                            Mult = 1;

                        Diagram[i] += (Diagram[i + d] - Diagram[i]) * Mult;
                    }
                    
                    for (int i = Diagram.Length - 1; i >= d; i--)
                    {
                        float Mult = Smoothness / d;

                        if (Mult > 1)
                            Mult = 1;

                        Diagram[i] += (Diagram[i - d] - Diagram[i]) * Mult;
                    }
                }
            }
        }
        public void MultiplyWith(float value)
        {
            if (Diagram != null)
            {
                for (int i = 0; i < Diagram.Length; i++)
                    Diagram[i] *= value;

                for (int i = 0; i < Diagram.Length; i++)
                    if (Diagram[i] > Height)
                        Diagram[i] = Height;
            }
        }
        public bool WasRenderTargetContentLost()
        {
            return ShadowTarget == null || ShadowTarget.IsContentLost;
        }

        public void DrawToRenderTarget(SpriteBatch spriteBatch, GraphicsDevice GD)
        {
            this.SB = spriteBatch;
            this.GD = GD;

            if (Diagram != null)
            {
                GD.SetRenderTarget(ShadowTarget);
                GD.Clear(Color.Transparent);

                int BaseHeight = Height / 25;
                DrawRect.Width = 1;

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                #region DrawLeStuff
                for (int i = 0; i < Diagram.Length; i++)
                {
                    DrawRect.X = i + P.X;
                    DrawRect.Y = P.Y - (int)Diagram[i];
                    DrawRect.Height = (int)Diagram[i] + BaseHeight;
                    spriteBatch.Draw(Assets.White, DrawRect, Color.White);

                    if (config.Default.AntiAliasing)
                    {
                        float NeighbourA;
                        float NeighbourB;

                        if (i > 0) NeighbourA = Diagram[i - 1];
                        else NeighbourA = 0;
                        if (i < Diagram.Length - 1) NeighbourB = Diagram[i + 1];
                        else NeighbourB = 0;

                        if (NeighbourA > Diagram[i])
                        {
                            DrawRect.X = i + P.X;
                            DrawRect.Y = P.Y - (int)Diagram[i] - (int)(NeighbourA - Diagram[i]);
                            DrawRect.Height = (int)(NeighbourA - Diagram[i]);
                            spriteBatch.Draw(Assets.ColorFade, DrawRect, Color.White);
                        }
                        else if (NeighbourB > Diagram[i])
                        {
                            DrawRect.X = i + P.X;
                            DrawRect.Y = P.Y - (int)Diagram[i] - (int)(NeighbourB - Diagram[i]);
                            DrawRect.Height = (int)(NeighbourB - Diagram[i]);
                            spriteBatch.Draw(Assets.ColorFade, DrawRect, Color.White);
                        }
                    }
                }
                #endregion
                spriteBatch.End();
                GD.SetRenderTarget(null);
            }
        }
        public void DrawToRenderTarget3DAcc(SpriteBatch spriteBatch, GraphicsDevice GD)
        {
            this.SB = spriteBatch;
            this.GD = GD;

            if (Diagram != null)
            {
                GD.SetRenderTarget(ShadowTarget);
                GD.Clear(Color.Transparent);

                int BaseHeight = Height / 25;
                
                for (int i = 0; i < Diagram.Length; i++)
                {
                    pointList[i*2].Position.Y = P.Y + 10;
                    pointList[i*2 + 1].Position.Y = P.Y - Diagram[i];
                }

                Assets.basicEffect.CurrentTechnique.Passes[0].Apply();
                GD.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleStrip,
                    pointList,
                    0,
                    pointList.Length,
                    Indices,
                    0,
                    pointList.Length - 2
                );
                

                if (config.Default.AntiAliasing)
                {
                    for (int i = 0; i < Diagram.Length; i++)
                    {
                        float NeighbourA;
                        float NeighbourB;

                        if (i > 0) NeighbourA = Diagram[i - 1];
                        else NeighbourA = 0;
                        if (i < Diagram.Length - 1) NeighbourB = Diagram[i + 1];
                        else NeighbourB = 0;

                        if (NeighbourA > Diagram[i])
                        {
                            AA_VPC[0].Position.X = i + P.X;
                            AA_VPC[1].Position.X = i + P.X;
                            AA_VPC[2].Position.X = i + P.X + 1;

                            AA_VPC[0].Position.Y = P.Y - Diagram[i];
                            AA_VPC[1].Position.Y = P.Y - NeighbourA;
                            AA_VPC[2].Position.Y = P.Y - Diagram[i];

                            Assets.basicEffect.CurrentTechnique.Passes[0].Apply();
                            GD.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, AA_VPC, 0, 3, AA_indices, 0, 1);
                        }
                        else if (NeighbourB > Diagram[i])
                        {
                            AA_VPC[0].Position.X = i + P.X;
                            AA_VPC[1].Position.X = i + P.X;
                            AA_VPC[2].Position.X = i + P.X + 1;

                            AA_VPC[0].Position.Y = P.Y - Diagram[i];
                            AA_VPC[1].Position.Y = P.Y - NeighbourB;
                            AA_VPC[2].Position.Y = P.Y - Diagram[i];

                            Assets.basicEffect.CurrentTechnique.Passes[0].Apply();
                            GD.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, AA_VPC, 0, 3, AA_indices, 0, 1);
                        }
                    }
                }

                GD.SetRenderTarget(null);
            }
        }
        public void DrawRenderTarget(SpriteBatch spriteBatch)
        {
            // Startup Animation
            if (Values.Timer < 40)
            {
                Height = 0;
                DiagramSize = 0;

                // Shadow
                DrawRect.X = P.X + 5;
                DrawRect.Y = P.Y + 5;
                DrawRect.Width = (int)((Length / 2) * (Values.Timer / 40f));
                DrawRect.Height = 8;
                spriteBatch.Draw(Assets.White, DrawRect, Color.Black * 0.6f);

                DrawRect.Width = (int)((Length / 2) * (Values.Timer / 40f));
                DrawRect.Height = 8;
                DrawRect.X = P.X + Length - DrawRect.Width + 5;
                DrawRect.Y = P.Y + 5;
                spriteBatch.Draw(Assets.White, DrawRect, Color.Black * 0.6f);


                // Normal
                DrawRect.X = P.X;
                DrawRect.Y = P.Y;
                DrawRect.Width = (int)((Length / 2) * (Values.Timer / 40f));
                DrawRect.Height = 8;
                spriteBatch.Draw(Assets.White, DrawRect, Program.game.primaryColor);

                DrawRect.Width = (int)((Length / 2) * (Values.Timer / 40f));
                DrawRect.Height = 8;
                DrawRect.X = P.X + Length - DrawRect.Width;
                DrawRect.Y = P.Y;
                spriteBatch.Draw(Assets.White, DrawRect, Program.game.primaryColor);
            }
            else if (Values.Timer < 50)
            { }
            else if (Values.Timer < 300)
            {
                DiagramSize = 1;
                Height = (int)(TargetHeight * Values.AnimationFunction(((Values.Timer - 50) / 50f)));

                // Shadow
                DrawRect.X = P.X + 5;
                DrawRect.Y = P.Y + 5;
                DrawRect.Width = Length - 1;
                DrawRect.Height = 8;
                spriteBatch.Draw(Assets.White, DrawRect, Color.Black * 0.6f);

                // Normal
                DrawRect.X = P.X;
                DrawRect.Y = P.Y;
                DrawRect.Width = Length - 1;
                DrawRect.Height = 8;
                spriteBatch.Draw(Assets.White, DrawRect, Program.game.primaryColor);
            }

            // Drawing the Target
            if (WithShadow)
            {
                DrawRect.X = (int)(Values.WindowSize.X / 2 + (5 - Values.WindowSize.X / 2) * DiagramSize);
                DrawRect.Y = (int)(Values.WindowSize.Y / 2 + (5 - Values.WindowSize.Y / 2) * DiagramSize);
                DrawRect.Width = (int)(Values.WindowSize.X * DiagramSize);
                DrawRect.Height = (int)(Values.WindowSize.Y * DiagramSize);
                spriteBatch.Draw(ShadowTarget, DrawRect, Color.Black * 0.6f);
            }
            DrawRect.X = (int)(Values.WindowSize.X / 2 + (0 - Values.WindowSize.X / 2) * DiagramSize);
            DrawRect.Y = (int)(Values.WindowSize.Y / 2 + (0 - Values.WindowSize.Y / 2) * DiagramSize);
            DrawRect.Width = (int)(Values.WindowSize.X * DiagramSize);
            DrawRect.Height = (int)(Values.WindowSize.Y * DiagramSize);
            spriteBatch.Draw(ShadowTarget, DrawRect, Program.game.primaryColor);
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
                                        1, Color.Lerp(Program.game.primaryColor, Program.game.secondaryColor, i / values.Length), spriteBatch);
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
                                    1, Color.Lerp(Program.game.primaryColor, Program.game.secondaryColor, i / values.Length), spriteBatch);
                }
            }
        }

        public void Dispose()
        {
            ShadowTarget.Dispose();
        }
    }
}
