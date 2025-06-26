using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace jeux
{
    public class Game1 : Game
    {
        private enum Direction { Up, Down, Left, Right }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private bool _isStartScreen = true;
        private SpriteFont _font;
        private Texture2D _playerSprite;
        private Texture2D _spriteTree;
        private Texture2D _spriteGrass;
        private Texture2D _spriteTerre;
        private Texture2D _spriteTerreLabouree;
        private Texture2D _spriteCoffre;
        private Texture2D _spriteEau;
        private Texture2D _spritePont;

        private Vector2 _playerPosition;
        private Vector2 _treePosition;

        private Direction _facingDirection = Direction.Down;
        private Color _backgroundColor = Color.CornflowerBlue;

        private uint[,] _tiles;
        private int _compteurAction = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _playerPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            _treePosition = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2 + 50);

            try
            {
                string[] lines = File.ReadAllLines("map.txt");
                int rows = lines.Length;
                int cols = lines[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                _tiles = new uint[cols, rows];

                for (int y = 0; y < rows; y++)
                {
                    string[] columns = lines[y].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    for (int x = 0; x < cols; x++)
                    {
                        _tiles[x, y] = uint.TryParse(columns[x], out uint val) ? val : 0;
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Erreur lecture map.txt : " + e.Message);
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<SpriteFont>("Font");
            _playerSprite = Content.Load<Texture2D>("Sprite-Face");
            _spriteTree = Content.Load<Texture2D>("Sprite-Tree");
            _spriteGrass = Content.Load<Texture2D>("Sprite-Grass");
            _spriteTerre = Content.Load<Texture2D>("Sprite-Terre");
            _spriteTerreLabouree = Content.Load<Texture2D>("Sprite-Terre-Labouré");
            _spriteCoffre = Content.Load<Texture2D>("Sprite-Coffre");
            _spriteEau = Content.Load<Texture2D>("Sprite-Ocean");
            _spritePont = Content.Load<Texture2D>("Sprite-Pont");
        }

        protected override void Update(GameTime gameTime)
        {
            var k = Keyboard.GetState();
            var mouse = Mouse.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || k.IsKeyDown(Keys.Escape))
                Exit();

            if (_isStartScreen)
            {
                if (k.IsKeyDown(Keys.Space))
                    _isStartScreen = false;

                base.Update(gameTime);
                return;
            }

            float speedPerSecond = 400f;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 newPosition = _playerPosition;

            if (k.IsKeyDown(Keys.Z) || k.IsKeyDown(Keys.Up))
            {
                _facingDirection = Direction.Up;
                _playerSprite = Content.Load<Texture2D>("Sprite-Back");
                newPosition.Y -= speedPerSecond * deltaTime;
            }
            else if (k.IsKeyDown(Keys.S) || k.IsKeyDown(Keys.Down))
            {
                _facingDirection = Direction.Down;
                _playerSprite = Content.Load<Texture2D>("Sprite-Face");
                newPosition.Y += speedPerSecond * deltaTime;
            }

            if (k.IsKeyDown(Keys.D) || k.IsKeyDown(Keys.Right))
            {
                _facingDirection = Direction.Right;
                _playerSprite = Content.Load<Texture2D>("Sprite-Profile-Droite");
                newPosition.X += speedPerSecond * deltaTime;
            }
            else if (k.IsKeyDown(Keys.Q) || k.IsKeyDown(Keys.Left))
            {
                _facingDirection = Direction.Left;
                _playerSprite = Content.Load<Texture2D>("Sprite-Profile-Gauche");
                newPosition.X -= speedPerSecond * deltaTime;
            }

            int tileSize = 32;
            int futureTileX = (int)newPosition.X / tileSize;
            int futureTileY = (int)newPosition.Y / tileSize;

            if (_tiles != null &&
                futureTileX >= 0 && futureTileX < _tiles.GetLength(0) &&
                futureTileY >= 0 && futureTileY < _tiles.GetLength(1))
            {
                uint futureTile = _tiles[futureTileX, futureTileY];
                if (futureTile != 0 && futureTile != 5)
                {
                    _playerPosition = newPosition;
                }
            }

            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, _playerSprite.Width / 2, _graphics.PreferredBackBufferWidth - _playerSprite.Width / 2);
            _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, _playerSprite.Height / 2, _graphics.PreferredBackBufferHeight - _playerSprite.Height / 2);

            int playerTileX = (int)(_playerPosition.X) / tileSize;
            int playerTileY = (int)(_playerPosition.Y) / tileSize;
            int targetX = playerTileX;
            int targetY = playerTileY;

            switch (_facingDirection)
            {
                case Direction.Up: targetY -= 1; break;
                case Direction.Down: targetY += 1; break;
                case Direction.Left: targetX -= 1; break;
                case Direction.Right: targetX += 1; break;
            }

            if (_tiles != null &&
                targetX >= 0 && targetX < _tiles.GetLength(0) &&
                targetY >= 0 && targetY < _tiles.GetLength(1))
            {
                uint targetTile = _tiles[targetX, targetY];

                if (targetTile != 0 && targetTile != 5 && targetTile != 4)
                {
                    if (mouse.LeftButton == ButtonState.Pressed)
                    {
                        if (_tiles[targetX, targetY] != 2)
                        {
                            _tiles[targetX, targetY] = 2;
                            _compteurAction++;
                        }
                    }
                    else if (mouse.RightButton == ButtonState.Pressed && targetTile == 2)
                    {
                        _tiles[targetX, targetY] = 3;
                    }
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_isStartScreen ? Color.DarkBlue : _backgroundColor);
            _spriteBatch.Begin();

            if (_isStartScreen)
            {
                string title1 = "MON JEU";
                string startText = "Appuie sur ESPACE pour commencer";

                Vector2 titleSize1 = _font.MeasureString(title1);
                Vector2 startTextSize = _font.MeasureString(startText);

                Vector2 titlePos1 = new Vector2(
                    (_graphics.PreferredBackBufferWidth - titleSize1.X) / 2,
                    60
                );

                Vector2 startTextPos = new Vector2(
                    (_graphics.PreferredBackBufferWidth - startTextSize.X) / 2,
                    _graphics.PreferredBackBufferHeight - startTextSize.Y - 30
                );

                _spriteBatch.DrawString(_font, title1, titlePos1, Color.White);
                _spriteBatch.DrawString(_font, startText, startTextPos, Color.LightGray);

                _spriteBatch.End();
                return;
            }

            int tileSize = 32;

            if (_tiles != null)
            {
                for (int y = 0; y < _tiles.GetLength(1); y++)
                {
                    for (int x = 0; x < _tiles.GetLength(0); x++)
                    {
                        Vector2 tilePosition = new Vector2(x * tileSize, y * tileSize);

                        switch (_tiles[x, y])
                        {
                            case 1: _spriteBatch.Draw(_spriteGrass, tilePosition, Color.White); break;
                            case 0: _spriteBatch.Draw(_spriteCoffre, tilePosition, Color.White); break;
                            case 2: _spriteBatch.Draw(_spriteTerre, tilePosition, Color.White); break;
                            case 3: _spriteBatch.Draw(_spriteTerreLabouree, tilePosition, Color.White); break;
                            case 4: _spriteBatch.Draw(_spritePont, tilePosition, Color.White); break;
                            case 5: _spriteBatch.Draw(_spriteEau, tilePosition, Color.White); break;
                        }
                    }
                }
            }

            bool playerIsInFront = _playerPosition.Y >= _treePosition.Y;

            if (!playerIsInFront)
            {
                DrawPlayer();
                DrawTree();
            }
            else
            {
                DrawTree();
                DrawPlayer();
            }

            string compteurText = $"Actions : {_compteurAction}";
            Vector2 compteurSize = _font.MeasureString(compteurText);
            Vector2 compteurPos = new Vector2(_graphics.PreferredBackBufferWidth - compteurSize.X - 10, 10);
            _spriteBatch.DrawString(_font, compteurText, compteurPos, Color.White);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawPlayer()
        {
            _spriteBatch.Draw(_playerSprite, _playerPosition, null, Color.White, 0f,
                new Vector2(_playerSprite.Width / 2f, _playerSprite.Height), 1f, SpriteEffects.None, 0f);
        }

        private void DrawTree()
        {
            _spriteBatch.Draw(_spriteTree, _treePosition, null, Color.White, 0f,
                new Vector2(_spriteTree.Width / 2f, _spriteTree.Height), 1f, SpriteEffects.None, 0f);
        }
    }
}
