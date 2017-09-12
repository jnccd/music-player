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
    public class GridSpring
    {
        DynamicGrid Parent;
        public GridPoint End1;
        public GridPoint End2;
        private float Length0;

        public GridSpring(GridPoint End1, GridPoint End2, DynamicGrid ParentGrid)
        {
            this.End1 = End1;
            this.End2 = End2;
            Length0 = (End1.Pos - End2.Pos).Length();
            Parent = ParentGrid;
        }

        public void Update()
        {
            lock (End1)
            {
                lock (End2)
                {
                    End1.ApplyForce((End2.Pos - End1.Pos) / Parent.Traction);
                    End2.ApplyForce((End1.Pos - End2.Pos) / Parent.Traction);
                    //End1.Update();
                    //End2.Update();
                }
            }
        }
        public void Draw(SpriteBatch SB)
        {
            lock (End1)
            {
                lock (End2)
                {
                    Assets.DrawLine(End1.Pos, End2.Pos, 1, XNA.primaryColor, SB);
                }
            }
            //SB.DrawString(Assets.Font, ((int)(End1.Pos - End2.Pos).Length()).ToString(), (End1.Pos + End2.Pos) / 2, Color.Blue);
        }
        public void DrawAsShadow(SpriteBatch SB)
        {
            lock (End1)
            {
                lock (End2)
                {
                    Assets.DrawLine(End1.Pos + new Vector2(5), End2.Pos + new Vector2(5), 1, Color.Black * 0.6f, SB);
                }
            }
            //SB.DrawString(Assets.Font, ((int)(End1.Pos - End2.Pos).Length()).ToString(), (End1.Pos + End2.Pos) / 2, Color.Blue);
        }
    }
}
