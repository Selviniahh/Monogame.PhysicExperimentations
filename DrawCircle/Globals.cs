using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Fluid;

public static class Globals
{
    public static float TotalSeconds { get; set; }
    public static ContentManager Content { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static Point GameBounds { get; set; }
    
    public static TimeSpan TargetElapsedTime { get; set; }

    public static GraphicsDevice GraphicsDevice;

    // ReSharper disable once InconsistentNaming
    public static Point UISize { get; set; }

    public static float Fps;

    public static void Update(GameTime gameTime)
    {
        TotalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / Fps);

    }
    
    public static Vector2 RandomDirection()
    {
        var angle = Random.Shared.NextDouble() * 2 * Math.PI;
        return new((float)Math.Sin(angle), (float)-Math.Cos(angle));
    }
}