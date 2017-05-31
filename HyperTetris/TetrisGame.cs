using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace HyperTetris
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class TetrisGame : Game
    {
        public static int Width = 800;
        public static int Height = 600;

        public static int Tilesize = 32;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Texture2D blankTexture;
        public SpriteFont fontMain;

        private Level level;

        private Texture2D startScreenTexture;

        // Sound
        public Song introSong;
        public Song mainSong;

        public enum GameState { Start, Level, GameOver }
        public GameState gameState;

        public TetrisGame()
        {
            graphics = new GraphicsDeviceManager(this);
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
            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            graphics.ApplyChanges();

            gameState = GameState.Start;
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            blankTexture = Content.Load<Texture2D>("Textures/blank");
            startScreenTexture = Content.Load<Texture2D>("Textures/start_screen");
            fontMain = Content.Load<SpriteFont>("main_font");

            introSong = Content.Load<Song>("Audio/Tetris_Simple");
            mainSong = Content.Load<Song>("Audio/Tetris_Hyper_Mix");

            MediaPlayer.Volume = 0.7f;
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(introSong);

            level = new Level(this);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (gameState == GameState.Level)
                level.Update(gameTime);
            else
            {
                KeyboardState kbState = Keyboard.GetState();
                if (kbState.IsKeyDown(Keys.Space))
                {
                    gameState = GameState.Level;
                    level.Init();
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            if (gameState == GameState.Start)
            {
                spriteBatch.Draw(startScreenTexture, new Rectangle(0, 0, Width, Height), Color.White);
                spriteBatch.DrawString(fontMain, "Press <Space> to start", new Vector2(300, 500), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            }
            else if (gameState == GameState.Level)
            {
                level.DrawUI(spriteBatch);
                level.Draw(spriteBatch);
            }
            else if (gameState == GameState.GameOver)
            {
                level.DrawUI(spriteBatch);
                level.Draw(spriteBatch);

                spriteBatch.Draw(blankTexture, new Rectangle(0, 0, Width, Height), Color.Black * 0.8f);
                spriteBatch.DrawString(fontMain, "Game Over", new Vector2(300, 150), Color.White);
                spriteBatch.DrawString(fontMain, "Level: " + level.CurrentLevel, new Vector2(350, 250), Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
                spriteBatch.DrawString(fontMain, "Score: " + level.CurrentScore, new Vector2(350, 300), Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);

                spriteBatch.DrawString(fontMain, "Press <Space> to start", new Vector2(300, 500), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

