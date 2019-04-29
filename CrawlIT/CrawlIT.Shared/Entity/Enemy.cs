﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace CrawlIT.Shared.Entity
{

    public class Enemy : Character
    {
        public Animation.Animation CurrentAnimation;

        private readonly Matrix _scale;

        private readonly Vector2 _position;

        public int Rounds;

        public Rectangle CollisionRectangle;

        public Rectangle CombatRectangle; 

        public Enemy(Texture2D texture, Matrix scale, float posx, float posy, int rounds)
        {
            TextureSheet = texture;
            PosX = posx;
            PosY = posy;
            _position = new Vector2(PosX, PosY);
            FrameWidth = 23;
            FrameHeight = 45;
            _scale = scale;
            Rounds = rounds;

            CollisionRectangle = new Rectangle((int)PosX, (int)PosY, FrameWidth, (int)(FrameHeight / 1.5));

            CombatRectangle = new Rectangle((int)PosX, (int)PosY, FrameWidth + 1, FrameHeight + 1);

            StandUp = new Animation.Animation();
            StandUp.AddFrame(new Rectangle(FrameWidth * 3, 0, FrameWidth, FrameHeight), TimeSpan.FromSeconds(1));

            StandDown = new Animation.Animation();
            StandDown.AddFrame(new Rectangle(0, 0, FrameWidth, FrameHeight), TimeSpan.FromSeconds(1));

            StandLeft = new Animation.Animation();
            StandLeft.AddFrame(new Rectangle(FrameWidth, 0, FrameWidth, FrameHeight), TimeSpan.FromSeconds(1));

            StandRight = new Animation.Animation();
            StandRight.AddFrame(new Rectangle(FrameWidth * 2, 0, FrameWidth, FrameHeight), TimeSpan.FromSeconds(1));

            CurrentAnimation = StandDown;
        }

        public override void Update(GameTime gameTime)
        {
            CurrentAnimation.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //var position = new Vector2(PosX, PosY);
            var sourceRectangle = CurrentAnimation.CurrentRectangle;
            spriteBatch.Draw(TextureSheet, _position, sourceRectangle, Color.White);
        }
    }
}
