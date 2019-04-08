﻿using Microsoft.Xna.Framework;
using CrawlIT.Shared.Entity;

namespace CrawlIT.Shared.Camera
{
    public class Camera
    {
        public Matrix Transform { get; private set; }
        public float Zoom;
        private readonly int _width;
        private readonly int _height;
        private float _posX;
        private float _posY;

        public Camera(int width, int height, float zoom)
        {
            _width = width;
            _height = height;
            Zoom = zoom;
        }

        public void Follow(Player target)
        {
            _posX = target == null ? 0
                                   : MathHelper.Lerp(_posX, target.PosX, 0.08f);
            _posY = target == null ? 0
                                   : MathHelper.Lerp(_posY, target.PosY, 0.08f);

            Transform = 
                Matrix.CreateTranslation(new Vector3(- _posX - target?.FrameWidth * 0.5f ?? 0,
                                                     - _posY - target?.FrameHeight * 0.5f ?? 0,
                                                     0))
                                         * Matrix.CreateRotationZ(0)
                                         * Matrix.CreateScale(new Vector3(Zoom, Zoom, 1))
                                         * Matrix.CreateTranslation(new Vector3(_width * 0.5f, _height * 0.5f, 0));
        }
    }
}
