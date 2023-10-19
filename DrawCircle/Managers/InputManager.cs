using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Fluid;

public class InputManager
{
    private static float _time = 0;

    private static MouseState _lastMouseState;
    public static bool MouseClicked { get; private set; }
    public static bool MouseRightClicked { get; private set; }
    public static bool CtrlHolding => Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl);
    public static Rectangle MouseRectangle { get; private set; }

    public static void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        
        MouseClicked = mouseState.LeftButton == ButtonState.Pressed && _lastMouseState.LeftButton == ButtonState.Released;
        MouseRightClicked = mouseState.RightButton == ButtonState.Pressed && _lastMouseState.RightButton == ButtonState.Released;
        MouseRectangle = new(mouseState.Position.X, mouseState.Position.Y, 1, 1);
        
        _lastMouseState = mouseState;
        
        _time += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (CtrlHolding) _time = 0;
    }

    public static bool IsShapeClicked(Rectangle shapeBounds)
    {
       return MouseClicked && MouseRectangle.Intersects(shapeBounds);
    }
    
    public static bool IsShapeRightClicked(Rectangle shapeBounds)
    {
        return MouseRightClicked && MouseRectangle.Intersects(shapeBounds);
    }

    public static bool WasCtrlHolding(float time)
    {
        return time > _time;
    }
}