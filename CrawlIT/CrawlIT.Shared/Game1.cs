﻿#region Using Statements

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;

using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using TiledSharp;

using ResolutionBuddy;

using CrawlIT.Shared.Entity;
using CrawlIT.Shared.Camera;
using Camera = CrawlIT.Shared.Camera.Camera;
using CrawlIT.Shared.GameStates;
using CrawlIT.Shared.Map;

#endregion

namespace CrawlIT
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private TouchCollection _touchCollection;

        private readonly IResolution _resolution;

        private Camera _staticCamera;
        private Song _backgroundSong;

        private Player _player;
        private Camera _playerCamera;
        private Texture2D _playerTexture;
        private Collision _collision;

        private GameState _menu;
        private GameState _level1;
        private GameState _level2;

        enum _gameStates
        {
            Playing,
            Fighting,
            Menu
        }

        private SpriteFont _font;

        private TiledMapRenderer _mapRenderer;
        private RenderTarget2D _mapRenderTarget;
        private TiledMap _map;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {   // Force orientation to be fullscreen portrait
                SupportedOrientations = DisplayOrientation.Portrait,
                IsFullScreen = true
            };

            _resolution =
                new ResolutionComponent(this, _graphics, new Point(720, 1280),
                                        new Point(720, 1280), true, false);

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _menu = new Menu(GraphicsDevice);
            _menu.SetState(_gameStates.Menu);

            _level1 = new Level1(GraphicsDevice);
            _level1.SetState(_gameStates.Playing);

            _level2 = new Level2(GraphicsDevice);
            _level2.SetState(_gameStates.Playing);

            // TODO: make this work
            // _gameStateManager.GameState = GameState.StartMenu;

            _mapRenderer = new TiledMapRenderer(GraphicsDevice);

            // Repeat _backgroundSong on end
            XnaMediaPlayer.IsRepeating = true;

            base.Initialize();

            PresentationParameters pp = _graphics.GraphicsDevice.PresentationParameters;
            _mapRenderTarget = new RenderTarget2D(GraphicsDevice,
                                                  _graphics.PreferredBackBufferWidth,
                                                  _graphics.PreferredBackBufferHeight,
                                                  false, SurfaceFormat.Color,
                                                  DepthFormat.None,
                                                  pp.MultiSampleCount,
                                                  RenderTargetUsage.DiscardContents);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _backgroundSong = Content.Load<Song>("Audio/Investigations");
            XnaMediaPlayer.Play(_backgroundSong);

            _map = Content.Load<TiledMap>("Maps/test");

            _playerTexture = Content.Load<Texture2D>("Sprites/charactersheet");
            _player = new Player(_playerTexture, _resolution.TransformationMatrix());
            _playerCamera = new Camera(_graphics.PreferredBackBufferWidth,
                                       _graphics.PreferredBackBufferHeight,
                                       8.0f);

            _font = Content.Load<SpriteFont>("Fonts/File");

            _staticCamera = new Camera(0, 0, 1.0f);

            _collision = new Collision(_map);

            //Set the content to the GameStateManager to be able to use it
            GameStateManager.Instance.SetContent(Content);

            //Initialize by adding the Menu screen into the game
            GameStateManager.Instance.AddScreen(_menu);

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
#if !__IOS__
                Game.Activity.MoveTaskToBack(true);
#endif
            }

            GameStateManager.Instance.Update(gameTime);

            _touchCollection = TouchPanel.GetState();
            if (GameStateManager.Instance.GetCurrentState().Equals(_gameStates.Menu) && _touchCollection.Count > 0)
            {
                // TODO: re-implement Start/Exit button touch...
                GameStateManager.Instance.ChangeScreen(_level1);
            }

            if (GameStateManager.Instance.GetCurrentState().Equals(_gameStates.Playing))
            {
                _mapRenderer.Update(_map, gameTime);

                //TODO: Select currentCollision 
                if ((_player.CurrentVelocity.Y > 0 && _collision.HitsFromTheTop(_player)) ||
                    (_player.CurrentVelocity.Y < 0 && _collision.HitsFromTheBottom(_player)))
                {
                    _player.CurrentVelocity = new Vector2(_player.CurrentVelocity.X, 0);
                }

                if ((_player.CurrentVelocity.X < 0 && _collision.HitsFromTheRight(_player) ||
                    _player.CurrentVelocity.X > 0 && _collision.HitsFromTheLeft(_player)))
                {
                    _player.CurrentVelocity = new Vector2(0, _player.CurrentVelocity.Y);
                }

                _player.Update(gameTime);

                _playerCamera.Follow(_player);
                _staticCamera.Follow(null);

                base.Update(gameTime);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>j
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            GameStateManager.Instance.Draw(_spriteBatch);

            if (GameStateManager.Instance.GetCurrentState().Equals(_gameStates.Playing))
            {
                //Little trick to show the tiled map as PointClamp even though we don't use spritebatch to draw it
                GraphicsDevice.SetRenderTarget(_mapRenderTarget);
                GraphicsDevice.BlendState = BlendState.AlphaBlend;
                GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                GraphicsDevice.RasterizerState = RasterizerState.CullNone;

                _mapRenderer.Draw(_map, viewMatrix: _playerCamera.Transform);

                GraphicsDevice.SetRenderTarget(null);
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                _spriteBatch.Draw(_mapRenderTarget,
                                  destinationRectangle: new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight),
                                  color: Color.White);
                _spriteBatch.End();
                //End of little trick

                _spriteBatch.Begin(SpriteSortMode.BackToFront,
                                   BlendState.AlphaBlend,
                                   SamplerState.PointClamp,
                                   null, null, null,
                                   _playerCamera.Transform);
                _player.Draw(_spriteBatch);
                _spriteBatch.End();

                // Saving this here for future reference...
                /*_spriteBatch.Begin(SpriteSortMode.BackToFront,
                                  BlendState.AlphaBlend,
                                  null, null, null, null,
                                  _staticCamera.Transform);*/
            }

            // FPS counter
            var fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            var fpsString = "FPS: " + Math.Ceiling(fps);
            var stringDimensions = _font.MeasureString(fpsString);

            var stringPosX = _resolution.ScreenArea.Width / 2 - stringDimensions.X / 2;
            var stringPosY = _resolution.ScreenArea.Height - stringDimensions.Y / 2;

            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, fpsString, new Vector2(stringPosX, stringPosY), Color.Black);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
