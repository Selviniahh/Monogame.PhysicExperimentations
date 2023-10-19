using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Fluid;

public class GameManager
{
    private readonly List<Circle.Circle> _circles = new List<Circle.Circle>();
    public GameManager()
    {
        var texture = Globals.Content.Load<Texture2D>("orb-blue");
        // for (int i = 0; i < 10; i++)
        // {
        //     _circles.Add(new Circle.Circle(texture));
        // }
        _circles.Add(new Circle.Circle(texture));

    }

    public void Update()
    {
        foreach (var circle in _circles)
        {
            circle.Update();
        }
    }

    public void Draw()
    {
        foreach (var circle in _circles)
        {
            circle.Draw();
        }
    }
}