using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using DrawCircle.Managers;
using Fluid;
using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.ImGui;
using Num = System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace DrawCircle;

public class UserInterface
{
    private Circle.Circle Circle { get; set; }
    private DrawingManager Drawing { get; set; }
    private bool _clearTrace = true;
    public event Action OnNewCircleNeeded;
    public delegate void ChangeAction(Circle.Circle circle);
    public event Action<ChangeAction> ChangeForAllCirclesRequired;

    public UserInterface(Circle.Circle circle, DrawingManager drawing)
    {
        Circle = circle;
        Drawing = drawing;
    }
    
    public void Update(Circle.Circle selectedCircle)
    {
        Circle = selectedCircle;
    }

    public void Render(Circle.Circle selectedCircle)
    {
        Circle = selectedCircle;
        Num.Vector2 pos = new Num.Vector2(Globals.GameBounds.X - Globals.UISize.X, 0);
        Num.Vector2 size = new Num.Vector2(Globals.UISize.X, Globals.GameBounds.Y);
        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(size);
        ImGui.Begin("Main Dock space", ImGuiWindowFlags.DockNodeHost | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);
        if (ImGui.BeginTabBar("MainTabBar"))
        {
            if (ImGui.BeginTabItem("Main"))
            {
                ImGui.Text("Circle name: " + Circle.Name);

                CreateCheckBox(ref Circle.Options.DrawTrace, "DrawTrace","Options", "DrawTrace", "Make circular movement around the radius");
                CreateCheckBox(ref Circle.Options.DrawCenterPoint, "DrawCenterPoint", "Options", "DrawCenterPoint", "Draw a dot on the center of the game window");
                CreateCheckBox(ref Circle.Options.DrawSinCosValues, "DrawSinCosValues","Options", "DrawSinAndCosValues", "On top of the circle draw the sin and cos values");

                if (ImGui.Button("Add New Circle"))
                {
                    OnNewCircleNeeded?.Invoke();
                }

                CreateInputFloat(ref Circle.Options.VelocityMultiplyer, "VelocityMultiplyer","Options", "VelocityMultiplyer", "VelocityMultiplyer for SHM");
                CreateInputFloat(ref CollisionManager.CollisionSettings.CollisionMultiplyer, "CollisionMultiplyer","CollisionSettings", "CollisionMultiplyer", "CollisionMultiplyer for SHM");
                CreateCheckBox(ref Circle.Options.MakeCounterClockwise, "MakeCounterClockwise", "Options", "CounterClockWise", "Invert the direction of the circle");
                
                // CreateInputFloat(ref Globals.Fps, "Frame per second", "Frame per second");

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Movement"))
            {
                if (ImGui.BeginTabBar("Circle"))
                {
                    if (ImGui.BeginTabItem("Circle"))
                    {
                        CreateCheckBox(ref Circle.Options.IsCircling, "IsCircling", "Options", "Circle", "Make circular movement around the radius");
                        CreateInputFloat(ref Circle.CircularMovement.Radius, "Radius", "CircularMovement","Radius", "Radius of the circle");
                        
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("SHM"))
                    {
                        CreateCheckBox(ref Circle.Options.IsSimpleHarmonicMotion, "IsSimpleHarmonicMotion", "Options", "Simple Harmonic Motion", "Make horizontal (X) left and right movement");

                        CreateInputFloat(ref Circle.Shm.ShmAmplitude, "ShmAmplitude", "Shm", "SHM Amplitude", "Amplitude for SHM");
                        CreateInputFloat(ref Circle.Shm.ShmFrequency, "SHMFrequency","Shm", "VelocityMultiplyer", "VelocityMultiplyer for SHM");

                        //Input direction
                        System.Numerics.Vector2 tempShmDirection = new System.Numerics.Vector2(Circle.Shm.Direction.X, Circle.Shm.Direction.Y);
                        ChangeShmDirection(tempShmDirection, "ShmDirection", "VelocityMultiplyer for SHM");

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar(); // End of "Movement" Tab
                }

                ImGui.EndTabItem(); // End of "Movement" Tab
            }

            if (ImGui.BeginTabItem("Motion"))
            {
                CreateCheckBox(ref Circle.Options.MakeWaveY, "MakeWaveY", "Options", "Oscillating Motion", "Make vertical (Y) up and down movement");
                CreateCheckBox(ref Circle.Options.MakeWaveX, "MakeWaveX", "Options", "Wave Pattern", "Make horizontal (X) right and left movement");

                CreateInputFloat(ref Circle.OscillatingMotion.WaveAmplitude, "WaveAmplitude","OscillatingMotion","Wave Amplitude", "Range of motion");
                CreateInputFloat(ref Circle.OscillatingMotion.WaveFrequency, "WaveFrequency","OscillatingMotion","WaveFrequency", "Velocity of the wave");

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Gravity"))
            {
                CreateCheckBox(ref Circle.Options.Gravity, "Gravity", "Options", "Gravity", "if disabled, the circle will float in the game window");

                //If SlideOff enabled, disable the RealisticBounce checkbox
                if (Circle.Gravity.SimpleBounce) ImGui.BeginDisabled(true);
                CreateCheckBox(ref Circle.Gravity.RealisticBounce, "RealisticBounce", "Gravity", "Realistic Bounce", "Use reflection vector to bounce off from overlapped circle rather than invert the direction");
                if ( Circle.Gravity.SimpleBounce) ImGui.EndDisabled();

                // If RealisticBounce or SlideOff is enabled, disable the SimpleBounce checkbox
                if (Circle.Gravity.RealisticBounce) ImGui.BeginDisabled(true);
                CreateCheckBox(ref Circle.Gravity.SimpleBounce, "SimpleBounce", "Gravity", "Unrealistic Bounce", "Invert the direction when overlapped");
                if (Circle.Gravity.RealisticBounce) ImGui.EndDisabled();
                
                // If RealisticBounce is enabled, disable the SlideOff checkbox
                CreateCheckBox(ref Circle.Gravity.SlideOff, "SlideOff", "Gravity", "Slide Off and Push", "Slide off from overlapped circle");
                CreateInputFloat(ref Circle.Gravity.Velocity, "Velocity", "Gravity", "Gravity Velocity", "Velocity of the gravity");
                CreateInputVector(ref Circle.Gravity.Direction, "GravityDirection", "GravityDirection", "Direction", "GravityDirection");
                
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Drawing"))
            {
                CreateCheckBox(ref Drawing.DrawingEnabled,"Drawing","Nothing","Drawing", "Enable drawing");
                CreateInputFloat(ref Drawing.Scale, "Scale", "Drawing", "Scale", "Scale");
            }
            ImGui.EndTabBar();
        }
        ImGui.End();
    }

    private void ChangeShmDirection(Num.Vector2 tempShmDirection, string label, string tooltip)
    {
        ImGui.Text(label);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltip);

        if (ImGui.InputFloat2("##" + label, ref tempShmDirection))
        {
            Circle.Shm.Direction = new Vector2(tempShmDirection.X, tempShmDirection.Y);
            Circle.Shm.Direction.Normalize();
            Circle.HandleTrace(ref _clearTrace);
            
            if (InputManager.CtrlHolding)
            {
                
            }
        }
    }
    private void CreateInputVector(ref Vector2 option, string fieldName, string structName, string label, string tooltip)
    {
        var vector = new Num.Vector2(option.X,option.Y);
        
        ImGui.Text(label);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltip); 
        ImGui.SameLine();
        if (ImGui.InputFloat2("##" + label, ref vector))
        {
            if (InputManager.EnterClicked)
            {
                option = new Vector2(vector.X, vector.Y);
                Circle.HandleTrace(ref _clearTrace);
            }
            
            if (InputManager.WasCtrlHolding(3))
            {
                ImGui.SameLine();
                Debug.WriteLine("Apply to all");
                MakeFieldChangesForAllCircles(option, fieldName, structName);
            }
        }
    }

    private void CreateCheckBox(ref bool option, string fieldName, string structName, string label, string tooltip)
    {
        ImGui.Text(label);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltip);
        ImGui.SameLine();
        if (ImGui.Checkbox("##" + label, ref option))
        {
            Circle.HandleTrace(ref _clearTrace);
            
            if (InputManager.CtrlHolding)
            {
                MakeFieldChangesForAllCircles(option, fieldName, structName);
            }
        }
    }

    private void CreateInputFloat(ref float option, string fieldName, string structName, string label, string tooltip)
    {
        float givenFloat = option; 
        ImGui.Text(label);
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(tooltip);
        ImGui.SameLine();
        if (ImGui.InputFloat("##" + label, ref givenFloat))
        {
            if (InputManager.EnterClicked)
            {
                option = givenFloat;
                Circle.HandleTrace(ref _clearTrace);
            }            
            if (InputManager.WasCtrlHolding(3))
            {
                ImGui.SameLine();
                Debug.WriteLine("Apply to all");
                MakeFieldChangesForAllCircles(option, fieldName, structName);
            }
        }
    }

    private void MakeFieldChangesForAllCircles<T>(T option, string fieldName, string structName)
    {
        T valueOption = option;
        ChangeAction action = circle =>
        {
            FieldInfo fieldInfo = circle.GetType().GetField(structName);
            object optionStruct = fieldInfo?.GetValue(circle);

            if (optionStruct != null)
            {
                //Get the field info from object
                var codeField = optionStruct.GetType().GetField(fieldName);

                //Setting the copy of the struct object. 
                if (codeField != null) codeField.SetValue(optionStruct, valueOption);
                else
                {
                    throw new Exception("Field not found" + codeField.Name);
                }

                //Setting the actual struct object. 
                fieldInfo.SetValue(circle, optionStruct);
            }
        };
        ChangeForAllCirclesRequired?.Invoke(action);
    }
}