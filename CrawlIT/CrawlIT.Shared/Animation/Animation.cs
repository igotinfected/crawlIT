﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace CrawlIT.Shared
{
    public class Animation
    {
        private readonly List<AnimationFrame> _animationFrames = new List<AnimationFrame>();
        private TimeSpan _timeIntoAnimation;

        // Define duration of our animation depending on frame durations
        private TimeSpan Duration
        {
            get
            {
                double totalSeconds = 0;
                foreach (var frame in _animationFrames)
                    totalSeconds += frame.Duration.TotalSeconds;

                return TimeSpan.FromSeconds(totalSeconds); 
            }
        }

        // Add animation frames to our animation
        public void AddFrame(Rectangle rectangle, TimeSpan duration)
        {
            var newFrame = new AnimationFrame()
            {
                SourceRectangle = rectangle,
                Duration = duration
            };

            _animationFrames.Add(newFrame);
        }

        // Update time into animation to switch frames
        public void Update(GameTime gameTime)
        {
            var secondsIntoAnimation =
                _timeIntoAnimation.TotalSeconds
                + gameTime.ElapsedGameTime.TotalSeconds;

            var remainder = secondsIntoAnimation % Duration.TotalSeconds;

            _timeIntoAnimation = TimeSpan.FromSeconds(remainder);
        }

        public Rectangle CurrentRectangle
        {
            get
            {
                AnimationFrame currentFrame = null;

                // Find correct frame depending on how long into the animation we are
                TimeSpan elapsedTime = new TimeSpan();
                foreach (var frame in _animationFrames)
                {
                    if (elapsedTime + frame.Duration >= _timeIntoAnimation)
                    {
                        currentFrame = frame;
                        break;
                    }

                    elapsedTime += frame.Duration;
                }

                // In case timeIntoAnimation exceeds Duration we use the last frame
                if (currentFrame == null)
                    currentFrame = _animationFrames.LastOrDefault();

                // Now we return the corresponding frame rectangle
                if (currentFrame != null)
                    return currentFrame.SourceRectangle;

                // If for whatever reason we still don't have a frame, we return an empty rectangle (w: 0, h:0)
                return Rectangle.Empty;
            }
        }
    }
}
