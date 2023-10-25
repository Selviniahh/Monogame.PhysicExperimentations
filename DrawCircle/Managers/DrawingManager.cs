using System;
using System.Collections.Generic;
using Fluid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DrawCircle.Managers;


public class DrawingManager
{
    public bool DrawingEnabled;
    public Color DrawColor;
    public Texture2D Pixel;
    private readonly List<Vector2> _currentDrawingPoints = new List<Vector2>();
    private readonly HashSet<Vector2> _permanentDrawings = new HashSet<Vector2>();
    public static HashSet<Vector2> LineSegments { get; private set; } = new();

    public float Scale; 

    public DrawingManager()
    {
        DrawingEnabled = false;
        Pixel = new Texture2D(Globals.GraphicsDevice, 1, 1);
        DrawColor = Color.Red;
        Pixel.SetData(new Color[] {DrawColor});
        Scale = 4;
    }

    public void Update()
    {
        if (DrawingEnabled && InputManager.MouseDragged)
        {
            CaptureCurrentDrawing();
        }
        else
        {
            // If drawing is disabled, clear the list
            _currentDrawingPoints.Clear();
        }
    }

    private void CaptureCurrentDrawing()
    {
        Vector2 newPoint = new Vector2(InputManager.MouseRectangle.X, InputManager.MouseRectangle.Y);

        // Interpolate between the last point and the new point if the list is not empty
        if (_currentDrawingPoints.Count > 0)
        {
            Vector2 lastPoint = _currentDrawingPoints[^1];
            InterpolatePoints(lastPoint, newPoint);
        }

        _currentDrawingPoints.Add(newPoint);
    }

    private void InterpolatePoints(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int numPoints = (int)Math.Ceiling(distance);

        for (int i = 1; i <= numPoints; i++)
        {
            // Calculate the interpolated point
            Vector2 interpolatedPoint = Vector2.Lerp(start, end, i / (float)numPoints);

            _currentDrawingPoints.Add(interpolatedPoint);
        }
    }
    
    public void Draw()
    {
        foreach (var drawing in _currentDrawingPoints)
        {
            if (DrawingEnabled && InputManager.MouseDragged)
            {
                _permanentDrawings.Add(drawing);
            }
            
        }

        foreach (var drawing in _permanentDrawings)
        {
            Globals.SpriteBatch.Draw(Pixel, drawing, null,DrawColor,0,Vector2.Zero,Scale,SpriteEffects.None,1);
        }
        
        for (int i = 0; i < _currentDrawingPoints.Count - 1; i++)
        {
            LineSegments.Add(_currentDrawingPoints[i]);
            LineSegments.Add(_currentDrawingPoints[i+1]);
        }
    }
    
    
}