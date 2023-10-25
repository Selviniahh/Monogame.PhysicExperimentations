using System;
using System.Collections.Generic;
using System.Diagnostics;
using DrawCircle;
using DrawCircle.Circle;
using DrawCircle.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.ImGui;

namespace Fluid;

public class GameManager
{
    private List<Circle> _circles = new List<Circle>();
    private Circle _selectedCircle;
    private readonly Random _random = new Random();

    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly UserInterface _ui;
    private readonly CollisionManager _colManager;
    private readonly DrawingManager _drawingManager;
    

    public GameManager(Game game)
    {
        _imGuiRenderer = new ImGuiRenderer(game).Initialize().RebuildFontAtlas();
        
        var texture = Globals.Content.Load<Texture2D>("orb-blue");
        var font = Globals.Content.Load<SpriteFont>("font");
        _circles.Add(new Circle(texture, font, 0, "Circle" + _circles.Count));
        _selectedCircle = _circles[0];

        _drawingManager = new DrawingManager();

        _ui = new UserInterface(_circles[0], _drawingManager);
        _ui.OnNewCircleNeeded += AddNewCircle;
        _ui.ChangeForAllCirclesRequired += MakeChangeForAllCircles;

        _colManager = new CollisionManager(_circles);
    }

    public void Update(GameTime gameTime)
    {
        InputManager.Update(gameTime);
        _colManager.Update();

        HandleInput();
        UpdateCircles(gameTime);
        _drawingManager.Update();
        _ui.Update(_selectedCircle);
    }

    public void Draw()
    {
        foreach (var circle in _circles)
            circle.Draw();
        
        _drawingManager.Draw();
        _colManager.Draw();
    }

    public void MakeChangeForAllCircles(UserInterface.ChangeAction changeAction)
    {
        foreach (var circle in _circles)
        {
            changeAction(circle);
        }
    }

    private void HandleInput()
    {
        foreach (var circle in _circles)
        {
            IsSelected(circle);
            MarkCircleDraggedIfSelected(circle);
        }
        
        void IsSelected(Circle circle)
        {
            if (InputManager.IsShapeClicked(circle.Bounds))
            {
                _selectedCircle = circle;
                foreach (var circle1 in _circles)
                {
                    if (circle1.IsSelected) circle1.IsSelected = false;
                }

                circle.IsSelected = true;
            }
        }

        void MarkCircleDraggedIfSelected(Circle circle)
        {
            if (_selectedCircle == circle && InputManager.MouseDragged)
            {
                circle.IsBeingDragged = true;
            }
            else
            {
                circle.IsBeingDragged = false;
            }
        }
    }

    private void AddNewCircle()
    {
        //Generate a random initial phase
        float initialPhase = (float)(Random.Shared.NextDouble() * 2 * Math.PI);

        //Create a new circle and add it to the list
        var newCircle = new Circle(_selectedCircle.Texture, _selectedCircle.Font, initialPhase, "Circle: " + _circles.Count);
        _circles.Add(newCircle);
    }

    public void DrawUi(GameTime gameTime)
    {
        //Window section
        _imGuiRenderer.BeginLayout(gameTime);
        _ui.Render(_selectedCircle);
        _imGuiRenderer.EndLayout();
    }

    private void UpdateCircles(GameTime gameTime)
    {
        List<Circle> circlesToRemove = new();
        foreach (var circle in _circles)
        {
            if (InputManager.IsShapeRightClicked(circle.Bounds))
            {
                circlesToRemove.Add(circle);
            }

            Vector2 mousePos = new Vector2(InputManager.MouseRectangle.X, InputManager.MouseRectangle.Y);
            if (circle.IsBeingDragged && !circle.IsAllPositionOverlappingWindowBounds(mousePos, circle.Origin) 
                                      && CollisionManager.IsColliding(circle.Position, circle.Origin.X, mousePos, circle.Origin.X))
            {
                circle.Position = mousePos;
            }

            circle.Update(gameTime);
        }

        RemoveCircles(circlesToRemove);
    }

    private void RemoveCircles(List<Circle> circlesToRemove)
    {
        // If the selected circle is in the removal list, select a new one
        if (circlesToRemove.Contains(_selectedCircle))
        {
            // Remove it first so it won't be re-selected
            _circles.Remove(_selectedCircle);

            if (_circles.Count > 0) _selectedCircle = _circles[_random.Next(0, _circles.Count)];
        }

        // Remove the circles
        foreach (var circle in circlesToRemove)
        {
            _circles.Remove(circle);
        }
    }
}