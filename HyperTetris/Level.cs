using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace HyperTetris
{
    public class Level
    {
        public const int SizeX = 11;
        public const int SizeY = 17;

        private const int OffsetX = 27;
        private const int OffsetY = 27;

        public const float TickDecreaseValue = 0.05f;
        public const float NextLevelInterval = 20;
        private Color blankColor = Color.Transparent;

        private Vector2 uiPosition;

        public int CurrentScore { get; set; }
        public int CurrentLevel { get; set; }
        private float nextLevelTimer;

        private Color[,] tiles;

        private bool[] completedRows;

        private Texture2D brickTexture;
        private Shape currentShape;
        private Shape nextShape;

        private float tickInterval = 0.5f;
        private float tickTimer;

        private Texture2D uiBackgroundTexture;

        private SoundEffect rowFinishedSound, gameOverSound;

        private KeyboardState previousKbState;

        public Random Random { get; set; }

        private TetrisGame game;

        public Level(TetrisGame game)
        {
            this.game = game;

            brickTexture = game.Content.Load<Texture2D>("Textures/brick");
            uiBackgroundTexture = game.Content.Load<Texture2D>("Textures/ui_background");

            rowFinishedSound = game.Content.Load<SoundEffect>("Audio/finish_row");
            gameOverSound = game.Content.Load<SoundEffect>("Audio/game_over");

            tiles = new Color[SizeX, SizeY];
            Random = new Random();

            uiPosition = new Vector2(SizeX * TetrisGame.Tilesize + 150, 0);

            completedRows = new bool[SizeY];

            previousKbState = Keyboard.GetState();
        }

        public void Init()
        {
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    tiles[i, j] = blankColor;
                }
            }

            for (int i = 0; i < SizeY; i++)
            {
                completedRows[i] = false;
            }

            nextShape = new Shape(this);
            currentShape = new Shape(this);

            MediaPlayer.Play(game.mainSong);
        }

        private bool TryMoveShape(int xdir, int ydir)
        {
            if (currentShape == null)
                return false;

            currentShape.X += xdir;
            currentShape.Y += ydir;

            if (IsValidPosition(currentShape))
            {
                // Valid movement
                return true;
            }
            else
            {
                // Revert movement
                currentShape.X -= xdir;
                currentShape.Y -= ydir;
                return false;
            }
        }

        private bool TryRotateShape()
        {
            if (currentShape == null)
                return false;

            int prevRotation = currentShape.Rotation;
            currentShape.Rotation++;
            currentShape.Rotation %= 4;


            if (IsValidPosition(currentShape))
            {
                // Valid rotation
                return true;
            }
            else
            {
                currentShape.Rotation = prevRotation;
                return false;
            }
        }

        private bool IsValidPosition(Shape shape)
        {
            for (int i = 0; i < Shape.ShapeSizeX; i++)
            {
                for (int j = 0; j < Shape.ShapeSizeY; j++)
                {
                    if (shape.Tiles[i, j])
                    {
                        Point pos = GetShapeTilePosition(shape, i, j);

                        // Out of Bounds
                        if (pos.X < 0 || pos.X >= SizeX || pos.Y >= SizeY)
                            return false;

                        // Already a tile on this position
                        if (tiles[pos.X, pos.Y] != blankColor)
                            return false;
                    }
                }
            }
            return true;
        }

        private bool IsInEndPosition(Shape shape)
        {
            shape.Y++;
            bool isEndPos = !IsValidPosition(shape);
            shape.Y--;
            return isEndPos;
        }

        private void GetInput()
        {
            KeyboardState kbState = Keyboard.GetState();

            // Move left or right
            if (kbState.IsKeyDown(Keys.Left) && !previousKbState.IsKeyDown(Keys.Left))
                TryMoveShape(-1, 0);
            else if (kbState.IsKeyDown(Keys.Right) && !previousKbState.IsKeyDown(Keys.Right))
                TryMoveShape(1, 0);

            // Move down faster
            if (kbState.IsKeyDown(Keys.Down) && !previousKbState.IsKeyDown(Keys.Down))
                TryMoveShape(0, 1);

            // Rotate
            if (kbState.IsKeyDown(Keys.Up) && !previousKbState.IsKeyDown(Keys.Up))
                TryRotateShape();

            previousKbState = kbState;
        }

        private Point GetShapeTilePosition(Shape shape, int i, int j)
        {
            // Consider Rotation
            int offsetX = i;
            int offsetY = j;

            if (shape.Rotation == 1)
            {
                offsetX = 3 - j - 1;
                offsetY = i;
            }
            else if (shape.Rotation == 2)
            {
                offsetX = 1 - i;
                offsetY = 3 - j;
            }
            else if (shape.Rotation == 3)
            {
                offsetX = j;
                offsetY = 1 - i;
            }

            return new Point(shape.X + offsetX, shape.Y + offsetY);
        }

        public void Update(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            GetInput();

            if (tickTimer > tickInterval)
            {
                tickTimer = 0;

                if (currentShape == null)
                {
                    // Create new shape
                    currentShape = new Shape(nextShape);
                    nextShape = new Shape(this);

                    // Check for game over
                    if (!IsValidPosition(currentShape))
                    {
                        game.gameState = TetrisGame.GameState.GameOver;
                        gameOverSound.Play();
                        MediaPlayer.Pause();
                    }
                }
                else if (TryMoveShape(0, 1))
                {

                }
                else if (IsInEndPosition(currentShape))
                {
                    // Add current shape to tiles
                    for (int i = 0; i < Shape.ShapeSizeX; i++)
                    {
                        for (int j = 0; j < Shape.ShapeSizeY; j++)
                        {
                            if (currentShape.Tiles[i, j])
                            {
                                Point pos = GetShapeTilePosition(currentShape, i, j);

                                tiles[pos.X, pos.Y] = currentShape.Color;
                            }
                        }
                    }
                    currentShape = null;

                    // Check for completed rows
                    for (int i = 0; i < SizeY; i++)
                    {
                        bool completed = true;
                        for (int j = 0; j < SizeX; j++)
                        {
                            if (tiles[j, i] == blankColor)
                                completed = false;
                        }

                        if (completed)
                            completedRows[i] = true;
                    }
                }

                // Remove completed rows
                int firstCompletedRow = 0;
                int completedRowCount = 0;
                for (int i = 0; i < SizeY; i++)
                {
                    if (completedRows[i])
                    {
                        if (firstCompletedRow == 0)
                            firstCompletedRow = i;

                        completedRowCount++;

                        // Remove tiles
                        for (int j = 0; j < SizeX; j++)
                        {
                            tiles[j, i] = blankColor;
                        }

                        rowFinishedSound.Play();

                        completedRows[i] = false;
                    }
                }
                if (completedRowCount > 0)
                {
                    CurrentScore += completedRowCount;
                    
                    // Move up rows
                    for (int i = firstCompletedRow - 1; i>= 0; i--)
                    {
                        for (int j = 0; j < SizeX; j++)
                        {
                            tiles[j, i + completedRowCount] = tiles[j, i];
                            tiles[j, i] = blankColor;
                        }
                    }
                }

            }
            else
                tickTimer += elapsedSeconds;

            if (nextLevelTimer > NextLevelInterval)
            {
                nextLevelTimer = 0;

                // Next Level
                CurrentLevel++;
                if (tickInterval > 0.1f)
                {
                    tickInterval -= TickDecreaseValue;
                }
            }
            else
                nextLevelTimer += elapsedSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
            // Draw Tiles
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    // Grid
                    Color gridColor = (i + j) % 2 == 0 ? Color.White * 0.1f : Color.White * 0.2f;
                    spriteBatch.Draw(game.blankTexture, new Rectangle(OffsetX + i * TetrisGame.Tilesize, OffsetY + j * TetrisGame.Tilesize, TetrisGame.Tilesize, TetrisGame.Tilesize), gridColor);

                    if (tiles[i, j] != null)
                    {
                        spriteBatch.Draw(brickTexture, new Rectangle(OffsetX + i * TetrisGame.Tilesize, OffsetY + j * TetrisGame.Tilesize, TetrisGame.Tilesize, TetrisGame.Tilesize), tiles[i, j]);
                    }
                }
            }

            // Draw Current Shape
            if (currentShape != null)
            {
                for (int i = 0; i < Shape.ShapeSizeX; i++)
                {
                    for (int j = 0; j < Shape.ShapeSizeY; j++)
                    {
                        if (currentShape.Tiles[i, j])
                        {
                            Point pos = GetShapeTilePosition(currentShape, i, j);
                            spriteBatch.Draw(brickTexture, new Rectangle(OffsetX + pos.X * TetrisGame.Tilesize, OffsetY + pos.Y * TetrisGame.Tilesize, TetrisGame.Tilesize, TetrisGame.Tilesize), currentShape.Color);
                        }
                    }
                }
            }
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(uiBackgroundTexture, new Rectangle(0, 0, TetrisGame.Width, TetrisGame.Height), Color.White);

            // Shape preview
            spriteBatch.DrawString(game.fontMain, "Next Shape ", uiPosition + new Vector2(20, 30), Color.Black, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
            for (int i = 0; i < Shape.ShapeSizeX; i++)
            {
                for (int j = 0; j < Shape.ShapeSizeY; j++)
                {
                    if (nextShape.Tiles[i, j])
                    {
                        spriteBatch.Draw(brickTexture, new Rectangle((int)uiPosition.X + 40 + i * TetrisGame.Tilesize, (int)uiPosition.Y + 60 + j * TetrisGame.Tilesize, TetrisGame.Tilesize, TetrisGame.Tilesize), nextShape.Color);
                    }
                }
            }

            // Score and Level
            spriteBatch.DrawString(game.fontMain, "Level " + CurrentLevel, uiPosition + new Vector2(10, 300), Color.Black);
            spriteBatch.DrawString(game.fontMain, "Score " + CurrentScore, uiPosition + new Vector2(10, 340), Color.Black, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);


        }
    }
}
