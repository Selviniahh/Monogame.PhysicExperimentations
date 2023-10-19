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
    private Random Random = new Random();

    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly UserInterface _ui;
    private readonly CollisionManager _ColManager;
    
    private const float PhaseIncrement = (float)Math.PI / 8;  // The increment value
    public GameManager(Game game)
    {
        var texture = Globals.Content.Load<Texture2D>("orb-blue");
        var font = Globals.Content.Load<SpriteFont>("font");
        _circles.Add(new Circle(texture, font,0,"Circle" + _circles.Count));
        _selectedCircle = _circles[0];
        _imGuiRenderer = new ImGuiRenderer(game).Initialize().RebuildFontAtlas();
        _ui = new UserInterface(_circles[0]);
        _ui.OnNewCircleNeeded += AddNewCircle;
        _ui.ChangeForAllCirclesRequired += MakeChangeForAllCircles;
        _ColManager = new CollisionManager(_circles);
    }

    private void AddNewCircle()
    {
        //Generate a random initial phase
        float initialPhase = (float)(Random.Shared.NextDouble() * 2 * Math.PI);

        //Create a new circle and add it to the list
        var newCircle = new Circle(_selectedCircle.Texture, _selectedCircle.Font, initialPhase, "sa");
        _circles.Add(newCircle);
    }

    public void MakeChangeForAllCircles(UserInterface.ChangeAction changeAction)
    {
        foreach (var circle in _circles)
        {
            changeAction(circle);
        }
    }

    public void Update(GameTime gameTime)
    {
        Globals.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / Globals.Fps);
        InputManager.Update(gameTime);
        
        List<Circle> circlesToRemove = new List<Circle>();
        foreach (var circle in _circles)
        {
            circle.Update(gameTime);
            if (InputManager.IsShapeClicked(circle.Bounds))
            {
                _selectedCircle = circle;
                foreach (var circle1 in _circles)
                {
                    if (circle1.IsSelected) circle1.IsSelected = false;
                }
                circle.IsSelected = true;
            }

            if (InputManager.IsShapeRightClicked(circle.Bounds))
            {
                circlesToRemove.Add(circle);
            }
        }
        
        _ColManager.Update();

        // If the selected circle is in the removal list, select a new one
        if (circlesToRemove.Contains(_selectedCircle))
        {
            // Remove it first so it won't be re-selected
            _circles.Remove(_selectedCircle);  
            
            if (_circles.Count > 0) _selectedCircle = _circles[Random.Next(0, _circles.Count)]; 
        }

        // Remove the circles
        foreach (var circle in circlesToRemove)
        {
            _circles.Remove(circle);
        }
        _ui.Update(_selectedCircle);
    }

    public void Draw()
    {
        foreach (var circle in _circles)
        {
            circle.Draw();
        }
        _ColManager.Draw();
    }

    public void DrawUI(GameTime gameTime)
    {
        //Window section
        _imGuiRenderer.BeginLayout(gameTime);
        _ui.Render(_selectedCircle);
        _imGuiRenderer.EndLayout();
    }
}