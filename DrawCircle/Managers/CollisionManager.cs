using System;
using System.Collections.Generic;
using System.Diagnostics;
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

public struct CollisionSettings
{
    public float CollisionMultiplyer = -2;

    public CollisionSettings() { }
}

public class CollisionManager
{
    private readonly List<Circle.Circle> _circles;
    private readonly List<DrawableVector> _drawableVectors = new List<DrawableVector>();
    public static CollisionSettings CollisionSettings = new CollisionSettings();


    public CollisionManager(List<Circle.Circle> circles)
    {
        _circles = circles;
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

    //If circles are overlapping, detect and call resolve
    private void CheckCollisions()
    {
        for (int i = 0; i < _circles.Count - 1; i++)
        {
            for (int j = i + 1; j < _circles.Count; j++)
            {
                if (IsColliding(_circles[i].Position,_circles[i].Origin.X,_circles[j].Position,_circles[j].Origin.X)) //Check if both circles are colliding
                {
                    ResolveCollision(_circles[i], _circles[j]);
                    break;
                }
            }
        }
        
        foreach (var circle in _circles)
        {
            foreach (var segment in DrawingManager.LineSegments)
            {
                if (IsCollidingWithSegment(circle.Position, circle.Origin.X, segment))
                {
                    ResolveSegmentCollision(circle, segment);
                    break;
                }
            }
        }
    }
    
    // New collision detection and resolution methods
    public static bool IsCollidingWithSegment(Vector2 circlePosition, float circleRadius, Vector2 segment)
    {
        return ((circlePosition - segment).Length() < circleRadius + CollisionSettings.CollisionMultiplyer);
    }

    private void ResolveSegmentCollision(Circle.Circle circle, Vector2 startSegment)
    {
        // Calculate the vector between the centers of the circles
        Vector2 delta = circle.Position - startSegment;

        // Normalize the delta vector
        Vector2 direction = Vector2.Normalize(delta);
        
        // Calculate the distance between the circles
        float distance = delta.Length();

        // Calculate the amount of overlap between the circles so we can send them opposite direction just enough to resolve collision
        float overlap = circle.Origin.X - distance;

        if (circle.Gravity.SlideOff || circle.Gravity.RealisticBounce)
        {
            if (circle.IsAllPositionOverlappingWindowBounds(circle.Position + direction * (overlap / 2.0f),circle.Origin))
                return;
            
            circle.Position += direction * (overlap / 2.0f);
        }
        
        // Reflect the direction (this is a simple way to make them "bounce")
        if (circle.Gravity.SimpleBounce)
        {
            circle.Gravity.Direction = -circle.Gravity.Direction;
        }
        else if (circle.Gravity.RealisticBounce)
        {
            Debug.WriteLine(circle.Name);
            circle.Gravity.Direction = Vector2.Reflect(circle.Gravity.Direction, direction);
        }
    }
    
    public static bool IsColliding(Vector2 b1Position, float b1Radius, Vector2 b2Position,float b2Radius)
    {
        return (b1Position - b2Position).Length() < (b1Radius + b2Radius + CollisionSettings.CollisionMultiplyer);
    }
    
    //If you colliding manually, b2 is your selected circle
    private void ResolveCollision(Circle.Circle b1, Circle.Circle b2)
    {
        // Calculate the vector between the centers of the circles
        Vector2 delta = b1.Position - b2.Position;

        // Normalize the delta vector
        Vector2 direction = Vector2.Normalize(delta);
        
        // Calculate the distance between the circles
        float distance = delta.Length();

        // Calculate the amount of overlap between the circles so we can send them opposite direction just enough to resolve collision
        float overlap = b1.Origin.X + b2.Origin.X - distance;

        if (b1.Gravity.SlideOff || b1.Gravity.RealisticBounce || b2.Gravity.SlideOff || b2.Gravity.RealisticBounce)
        {
            if (b1.IsAllPositionOverlappingWindowBounds(b1.Position + direction * (overlap / 2.0f),b1.Origin) || b2.IsAllPositionOverlappingWindowBounds(b2.Position - direction * (overlap / 2.0f),b2.Origin)) 
                return;
            
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
            Debug.WriteLine(b1.Name);
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

        if (b1.IsSelected || b2.IsSelected)
        {
            var selectedCircle = b1.IsSelected ? b1 : b2;
            _drawableVectors.Add(new DrawableVector(selectedCircle.Position, direction, 500f, Color.Green));
            _drawableVectors.Add(new DrawableVector(selectedCircle.Position, selectedCircle.Gravity.Direction, 500f, Color.Blue));
            _drawableVectors.Add(new DrawableVector(selectedCircle.Position, Vector2.Reflect(selectedCircle.Gravity.Direction, selectedCircle.Gravity.Direction), 500f, Color.Red));
        }
    }

    private void DrawVector(Vector2 startPoint, Vector2 direction, float length, Color color)
    {
        // Calculate the rotation angle
        float rotation = (float)Math.Atan2(direction.Y, direction.X);

        // Create a 1x1 white pixel texture if you haven't already
        Texture2D pixel = new Texture2D(Globals.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        pixel.SetData(new[] { Color.White });

        // Draw the vector
        Globals.SpriteBatch.Draw(pixel, startPoint, null, color, rotation, Vector2.Zero, new Vector2(length, 5), SpriteEffects.None, 0);
    }
}