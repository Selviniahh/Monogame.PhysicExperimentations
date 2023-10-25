using System;
using System.Collections.Generic;
using System.Diagnostics;
using Fluid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DrawCircle.Circle
{
    public struct Gravity
    {
        public Vector2 Direction;
        public bool SlideOff;
        public bool RealisticBounce;
        public bool SimpleBounce;
        public float Velocity; 
        
        public Gravity(Vector2 direction)
        {
            Direction = direction;
            SlideOff = false;
            RealisticBounce = true;
            SimpleBounce = false;
            Velocity = 250;
        }
    }

    public struct Options
    {
        public bool DrawTrace;
        public bool DrawCenterPoint;
        public bool DrawSinCosValues;
        public bool MakeWaveY;
        public bool MakeWaveX;
        public bool IsCircling;
        public bool IsSimpleHarmonicMotion;
        public bool Gravity;
        public bool MakeCounterClockwise;
        public float VelocityMultiplyer;
    }

    public struct CircularMovement
    {
        public float Radius;
        public float Velocity;
        
        public CircularMovement()
        {
            Radius = 100;
            Velocity = 2;
        }
    }
    
    public struct OscillatingMotion
    {
        public  float WaveAmplitude;
        public  float WaveFrequency;

        public OscillatingMotion()
        {
            WaveAmplitude = 50f;
            WaveFrequency = 1f;
        }
    }

    public struct SimpleHarmonicMotion
    {
        public float ShmAmplitude;
        public float ShmFrequency;
        public Vector2 ShmInitialPosition;
        public float Phase;
        public Vector2 Direction;

        public SimpleHarmonicMotion(Vector2 shmInitialPosition, float phase)
        {
            ShmAmplitude = 200;
            ShmFrequency = 1f;
            ShmInitialPosition = shmInitialPosition;
            Phase = phase;
            Direction = Vector2.Zero;
        }
    }

    public class Circle
    {
        public SimpleHarmonicMotion Shm;
        public Options Options;
        public Gravity Gravity;
        public OscillatingMotion OscillatingMotion;
        public CircularMovement CircularMovement;
        
        public readonly Texture2D Texture;
        public Vector2 Position;
        public readonly string Name;
        public bool IsBeingDragged { get; set; }
        public bool IsSelected { get; set; }
        public Rectangle Bounds => new Rectangle((int)(Position.X - Origin.X), (int)(Position.Y - Origin.Y), Texture.Width, Texture.Height);
        public readonly SpriteFont Font;
        private readonly List<Vector2> _positionHistory = new List<Vector2>();
        public Vector2 Origin { get; set; }
        private float Angle { get; set; } = 0.0f; // keep track of the angle for the circle
        private Color Color { get; set; }

        private readonly Texture2D _pixel;
        private readonly Texture2D _whitePixel;
        private readonly int _maxHistoryPosCount;


        public Circle(Texture2D texture, SpriteFont font, float initialPhase, string name)
        {
            //Circle itself
            Texture = texture;
            Font = font;
            Name = name;
            Origin = new Vector2((float)texture.Width / 2, (float)texture.Height / 2);
            Position = new Vector2(Globals.GameBounds.X / 2.0f, Globals.GameBounds.Y / 2.0f);
            Color = Color.White;
            Angle += Options.VelocityMultiplyer;
            _maxHistoryPosCount = 1000;
            
            _pixel = new Texture2D(Globals.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            _pixel.SetData(new[] { Color.White }); // so that we can draw whatever color we want on top of it
            _whitePixel = new Texture2D(Globals.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new Color[] { Color.White });

            //OscillatingMotion
            OscillatingMotion = new OscillatingMotion();
            
            //CircularMovement
            CircularMovement = new CircularMovement();
            
            //SHM
            Shm = new SimpleHarmonicMotion(Position, initialPhase);
            float randomAngle = (float)(new Random().NextDouble() * 2 * Math.PI); //Take random double from 0-2pi
            Shm.Direction = new Vector2((float)Math.Cos(randomAngle), (float)Math.Sin(randomAngle)); //Take it's cos and sin it will be direction

            //Gravity
            Gravity = new Gravity(Globals.RandomDirection());
            Options.VelocityMultiplyer = 1f;
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            UpdateVelocityMultiplyer();
            
            UpdateMovements(deltaTime);
            
            UpdatePositionHistory();
        }

        public void Draw()
        {
            //Draw Circle
            Color drawColor = IsSelected ? Color.Gray : Color;
            Globals.SpriteBatch.Draw(Texture, Position, null, drawColor, 0, Origin, 1, SpriteEffects.None, 1);

            DrawMovements();
            
            if (Options.DrawSinCosValues)
            {
                DrawSinCos();
            }
        }

        private void DrawMovements()
        {
            if (Options.DrawCenterPoint)
            {
                DrawCenterPointAndThread();
            }
            
            if (Options.DrawTrace)
            {
                DrawTrace();
            }
        }

        private void UpdateMovements(float deltaTime)
        {
            if (Options.IsCircling)
            {
                UpdateCircularMovement(deltaTime);
            }
            
            if (Options.IsSimpleHarmonicMotion)
            {
                UpdateSimpleHarmonicMotion(deltaTime);
            }
            
            if (Options.MakeWaveX)
            {
                UpdateWaveMovementX();
            }
            
            if (Options.MakeWaveY)
            {
                UpdateWaveMovementY();
            }
            
            if (!Options.Gravity)
            {
                UpdateNoGravityMovement();
            }
        }

        private void UpdateCircularMovement(float deltaTime)
        {
            // Calculate the center of the screen
            float centerX = Globals.GameBounds.X / 2.0f;
            float centerY = Globals.GameBounds.Y / 2.0f;
            // Movements

            Angle += Options.VelocityMultiplyer * CircularMovement.Velocity * deltaTime;
            Position.X = centerX + CircularMovement.Radius * (float)Math.Cos(Angle);
            Position.Y = centerY + CircularMovement.Radius * (float)Math.Sin(Angle);
        }

        private void UpdateSimpleHarmonicMotion(float deltaTime)
        {
            Shm.Phase += Options.VelocityMultiplyer * deltaTime;
            Position.X = Shm.ShmInitialPosition.X + Shm.ShmAmplitude * (float)Math.Sin(Shm.ShmFrequency * Shm.Phase) * Shm.Direction.X;
            Position.Y = Shm.ShmInitialPosition.Y + Shm.ShmAmplitude * (float)Math.Sin(Shm.ShmFrequency * Shm.Phase * Shm.Direction.Y);
        }

        private void UpdateWaveMovementX()
        {
            Position.X += OscillatingMotion.WaveAmplitude * (float)Math.Sin(OscillatingMotion.WaveFrequency * Angle);
        }

        private void UpdateWaveMovementY()
        {
            Position.Y += OscillatingMotion.WaveAmplitude * (float)Math.Sin(OscillatingMotion.WaveFrequency * Angle);
        }

        private void UpdateNoGravityMovement()
        {
            Position += Gravity.Direction * Gravity.Velocity * Globals.TotalSeconds * Options.VelocityMultiplyer;

            if (IsPositionOverlappingWindowBounds(Position.X, Origin.X, Globals.GameBounds.X, Globals.UISize.X))
            {
                Gravity.Direction = new Vector2(-Gravity.Direction.X, Gravity.Direction.Y); //position.X < Origin.X || 
            }

            if (IsPositionOverlappingWindowBounds(Position.Y, Origin.Y, Globals.GameBounds.Y, Globals.UISize.Y))
            {
                Gravity.Direction = new Vector2(Gravity.Direction.X, -Gravity.Direction.Y); //position.Y < Origin.Y || 
            }
        }

        
        public bool IsPositionOverlappingWindowBounds(float position, float origin, int bounds, int uiSize)
        {
            return position - origin < 0 || position > bounds - origin - uiSize;
        }
        public bool IsAllPositionOverlappingWindowBounds(Vector2 position, Vector2 origin)
        {
            var bounds = Globals.GameBounds;
            var uiSize = Globals.UISize;
            return (position.X - origin.X < 0 || position.X > bounds.X - origin.X - uiSize.X) ||
                   (position.Y - origin.Y < 0 || position.Y > bounds.Y - origin.Y - uiSize.Y);
        }


        private void UpdatePositionHistory()
        {
            //Add the current position to the history
            _positionHistory.Add(Position);
            // Optionally, remove old positions to keep the list from growing indefinitely.
            if (_positionHistory.Count > _maxHistoryPosCount)
            {
                _positionHistory.RemoveAt(0);
            }
        }

        private void UpdateVelocityMultiplyer()
        {
            if (Options.MakeCounterClockwise)
            {
                Options.VelocityMultiplyer = -Math.Abs(Options.VelocityMultiplyer);
            }
            else
            {
                Options.VelocityMultiplyer = +Math.Abs(Options.VelocityMultiplyer);
            }
        }

        private void DrawSinCos()
        {
            string sinAndCos = "Cos " + ((Position.X - Globals.GameBounds.X / 2.0f) / CircularMovement.Radius).ToString("N2") + " Sin " + ((Position.Y - Globals.GameBounds.Y / 2.0f) / CircularMovement.Radius).ToString("N2");
            Vector2 stringSize = Font.MeasureString(sinAndCos);
            Vector2 stringPosition = new Vector2(Position.X - stringSize.X / 2, Position.Y - Origin.Y - 25 - stringSize.Y / 2);

            Globals.SpriteBatch.DrawString(Font, sinAndCos, stringPosition - new Vector2(10, 0), Color.White);
        }

        private void DrawTrace()
        {
            foreach (var pos in _positionHistory)
            {
                // Draw a small dot at the historical position
                Globals.SpriteBatch.Draw(_whitePixel, pos, null, Color.Red, 0, new Vector2(0.5f, 0.5f), 1f, SpriteEffects.None, 1);
            }
        }

        private void DrawCenterPointAndThread()
        {
            //Draw Center Point
            var centerPosition = new Vector2((float)Globals.GameBounds.X / 2, (float)Globals.GameBounds.Y / 2);
            Globals.SpriteBatch.Draw(Texture, centerPosition, null, Color.Red, 0, Origin, 0.2f, SpriteEffects.None, 1); // Scaled down

            // Draw line from center to circle
            Vector2 delta = Position - centerPosition;
            float rotation = (float)Math.Atan2(delta.Y, delta.X);
            float distance = Vector2.Distance(centerPosition, Position);

            Globals.SpriteBatch.Draw(_pixel, centerPosition, null, Color.Blue, rotation, Vector2.Zero, new Vector2(distance, 1), SpriteEffects.None, 0);
        }

        public void HandleTrace(ref bool clearTrace)
        {
            ClearTrace(ref clearTrace);
            LetClearTrace(ref clearTrace);
        }

        private void LetClearTrace(ref bool clearTrace) => clearTrace = true;

        private void ClearTrace(ref bool clearTrace)
        {
            if (clearTrace)
            {
                _positionHistory.Clear();
                clearTrace = false;
            }
        }

        
    }
}