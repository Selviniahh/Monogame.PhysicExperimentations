using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Fluid.Circle
{
    public class Circle
    {
        private static readonly Random Random = new Random();
        private readonly Texture2D _texture;
        public Vector2 Origin { get; set; }
        public Vector2 Position;
        public float Angle { get; set; } = 0.0f;  // New variable to keep track of the angle
        public Color Color { get; set; }

        public Circle(Texture2D texture)
        {
            _texture = texture;
            Origin = new Vector2((float)texture.Width / 2, (float)texture.Height / 2);
            Position = Vector2.Zero;
            Color = Color.White;
            // Angle = (float)(Random.NextDouble() * 2 * Math.PI);  // Initialize angle randomly
            Angle = 0.0f;
        }

        public void Update()
        {
            // Increment the angle
            Angle += 0.05f;
            // Calculate the radius
            float radius = 100.0f;

            // Calculate the center of the screen
            float centerX = Globals.Bounds.X / 2.0f;
            float centerY = Globals.Bounds.Y / 2.0f;

            // Calculate the new position using sin and cos
            Position.X = centerX + radius * (float)Math.Cos(Angle);
            Position.Y = centerY + radius * (float)Math.Sin(Angle);
        }

        public void Draw()
        {
            //Draw Circle
            Globals.SpriteBatch.Draw(_texture, Position, null, Color, 0, Origin, 1, SpriteEffects.None, 1);
            
            //Draw Center Point
            var centerPosition = new Vector2((float)Globals.Bounds.X / 2, (float)Globals.Bounds.Y / 2);
            Globals.SpriteBatch.Draw(_texture, centerPosition, null, Color.Red, 0, Origin, 0.2f, SpriteEffects.None, 1);  // Scaled down

        }
    }
}