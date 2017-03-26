using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MusicPlayer
{
    public static class Control
    {
        public static MouseState CurMS;
        public static MouseState LastMS;
        public static KeyboardState CurKS;
        public static KeyboardState LastKS;

        public static void Update()
        {
            LastKS = CurKS;
            LastMS = CurMS;

            CurMS = Mouse.GetState();
            CurKS = Keyboard.GetState();
        }

        public static bool WasKeyJustPressed(Keys K) { return CurKS.IsKeyDown(K) && LastKS.IsKeyUp(K); }
        public static Vector2 GetMouseVector() { return new Vector2(CurMS.X, CurMS.Y); }
        public static Rectangle GetMouseRect() { return new Rectangle(CurMS.X, CurMS.Y, 1, 1); }
        public static bool WasLMBJustPressed() { return CurMS.LeftButton == ButtonState.Pressed && LastMS.LeftButton == ButtonState.Released; }
        public static bool WasRMBJustPressed() { return CurMS.RightButton == ButtonState.Pressed && LastMS.RightButton == ButtonState.Released; }
    }
}
