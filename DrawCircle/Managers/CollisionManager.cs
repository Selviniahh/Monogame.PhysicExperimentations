using System;
using System.Collections.Generic;
using Fluid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DrawCircle.Managers;

public struct DrawableVector
{
    public Vector2 StartPoint;
    public Vector2 Direction;
    public float Length;
    public Color Color;

    public DrawableVector(Vector2 startPoint, Vector2 direction, float length, Color color)
    {
        StartPoint = startPoint;
        Direction = direction;
        Length = length;
        Color = color;
    }
}

public class CollisionManager
{
    private readonly List<Circle.Circle> _circles;
    private readonly List<DrawableVector> _drawableVectors = new List<DrawableVector>();


    public CollisionManager(List<Circle.Circle> circles)
    {
        _circles = circles;
    }

    private void CheckCollisions()
    {
        for (int i = 0; i < _circles.Count - 1; i++)
        {
            for (int j = i + 1; j < _circles.Count; j++)
            {
                if ((_circles[i].Position - _circles[j].Position).Length() < (_circles[i].Origin.X + _circles[j].Origin.X)) //Check if both circles are colliding
                {
                    ResolveCollision(_circles[i], _circles[j]);
                    break;
                }
            }
        }
    }

    private void ResolveCollision(Circle.Circle b1, Circle.Circle b2)
    {
        // Calculate the vector between the centers of the circles
        Vector2 delta = b1.Position - b2.Position;

        // Calculate the distance between the circles
        float distance = delta.Length();

        // Calculate the amount of overlap between the circles so we can send them opposite direction just enough to resolve collision
        float overlap = b1.Origin.X + b2.Origin.X - distance;

        // Normalize the delta vector
        Vector2 direction = Vector2.Normalize(delta);

        if (b1.Gravity.SlideOff || b1.Gravity.RealisticBounce || b2.Gravity.SlideOff || b2.Gravity.RealisticBounce)
        {
            b1.Position += direction * (overlap / 2.0f);
            b2.Position -= direction * (overlap / 2.0f);
        }

        // Reflect the direction (this is a simple way to make them "bounce")
        if (b1.Gravity.SimpleBounce)
        {
            b1.Gravity.Direction = -b1.Gravity.Direction;
        }
        else if (b1.Gravity.RealisticBounce)
        {
            b1.Gravity.Direction = Vector2.Reflect(b1.Gravity.Direction, direction);
        }


        if (b2.Gravity.SimpleBounce)
        {
            b2.Gravity.Direction = -b2.Gravity.Direction;
        }
        else if (b2.Gravity.RealisticBounce)
        {
            b2.Gravity.Direction = Vector2.Reflect(b2.Gravity.Direction, -direction);
        }


        _drawableVectors.Add(new DrawableVector(b1.Position, direction, 500f, Color.Green));
        _drawableVectors.Add(new DrawableVector(b1.Position, b1.Gravity.Direction, 500f, Color.Blue));
        _drawableVectors.Add(new DrawableVector(b1.Position, Vector2.Reflect(b1.Gravity.Direction, direction), 500f, Color.Red));
    }

    private void DrawVector(Vector2 startPoint, Vector2 direction, float length, Color color)
    {
        // Calculate the rotation angle
        float rotation = (float)Math.Atan2(direction.Y, direction.X);

        // Create a 1x1 white pixel texture if you haven't already
        Texture2D pixel = new Texture2D(Globals.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        pixel.SetData(new[] { Color.White });

        // Draw the vector
        Globals.SpriteBatch.Draw(pixel, startPoint, null, color, rotation, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
    }

    public void Update()
    {
        CheckCollisions();
    }

    public void Draw()
    {
        foreach (var drawableVector in _drawableVectors)
        {
            DrawVector(drawableVector.StartPoint, drawableVector.Direction, drawableVector.Length, drawableVector.Color);
        }

        _drawableVectors.Clear(); // Clear the list so it doesn't accumulate
    }
}