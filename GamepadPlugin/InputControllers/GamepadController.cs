using GamepadPlugin.Extensions;
using GamepadPlugin.Model;
using Lumos.GUI.User;
using LumosLIB.Kernel.Scene.Fanning;
using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace GamepadPlugin.InputControllers
{

    /// <summary>
    /// Implementation that is designed directly for the XBox-Styled controller
    /// </summary>
    public class GamepadController : InputControllerBase
    {
        public const byte A_BUTTON_INDEX = 0;
        public const byte B_BUTTON_INDEX = 1;
        public const byte X_BUTTON_INDEX = 2;
        public const byte Y_BUTTON_INDEX = 3;
        public const byte BACK_BUTTON_INDEX = 4;
        public const byte XBOX_BUTTON_INDEX = 5;
        public const byte START_BUTTON_INDEX = 6;
        public const byte LEFT_THUMB_INDEX = 7;
        public const byte RIGHT_THUMB_INDEX = 8;
        public const byte LB_INDEX = 9;
        public const byte RB_INDEX = 10;
        public const byte UP_INDEX = 11;
        public const byte DOWN_INDEX = 12;
        public const byte LEFT_INDEX = 13;
        public const byte RIGHT_INDEX = 14;

        public const byte LEFT_STICK_X_AXIS = 0;
        public const byte LEFT_STICK_Y_AXIS = 1;
        public const byte RIGHT_STICK_X_AXIS = 2;
        public const byte RIGHT_STICK_Y_AXIS = 3;
        public const byte LEFT_TRIGGER_AXIS = 4;
        public const byte RIGHT_TRIGGER_AXIS = 5;

        public bool IsAPressed => ButtonStates.ContainsKey(A_BUTTON_INDEX) && ButtonStates[A_BUTTON_INDEX] > 0;
        public bool IsBPressed => ButtonStates.ContainsKey(B_BUTTON_INDEX) && ButtonStates[B_BUTTON_INDEX] > 0;
        public bool IsXPressed => ButtonStates.ContainsKey(X_BUTTON_INDEX) && ButtonStates[X_BUTTON_INDEX] > 0;
        public bool IsYPressed => ButtonStates.ContainsKey(Y_BUTTON_INDEX) && ButtonStates[Y_BUTTON_INDEX] > 0;

        public bool IsWindowPressed => ButtonStates.ContainsKey(BACK_BUTTON_INDEX) && ButtonStates[BACK_BUTTON_INDEX] > 0;
        public bool IsXboxPressed => ButtonStates.ContainsKey(XBOX_BUTTON_INDEX) && ButtonStates[XBOX_BUTTON_INDEX] > 0;
        public bool IsMenuPressed => ButtonStates.ContainsKey(START_BUTTON_INDEX) && ButtonStates[START_BUTTON_INDEX] > 0;
        public bool IsLeftJoyconPressed => ButtonStates.ContainsKey(LEFT_THUMB_INDEX) && ButtonStates[LEFT_THUMB_INDEX] > 0;
        public bool IsRightJoyconPressed => ButtonStates.ContainsKey(RIGHT_THUMB_INDEX) && ButtonStates[RIGHT_THUMB_INDEX] > 0;

        public bool IsLbPressed => ButtonStates.ContainsKey(LB_INDEX) && ButtonStates[LB_INDEX] > 0;
        public bool IsRbPressed => ButtonStates.ContainsKey(RB_INDEX) && ButtonStates[RB_INDEX] > 0;

        public bool IsUpPressed => ButtonStates.ContainsKey(UP_INDEX) && ButtonStates[UP_INDEX] > 0;
        public bool IsDownPressed => ButtonStates.ContainsKey(DOWN_INDEX) && ButtonStates[DOWN_INDEX] > 0;
        public bool IsLeftPressed => ButtonStates.ContainsKey(LEFT_INDEX) && ButtonStates[LEFT_INDEX] > 0;
        public bool IsRightPressed => ButtonStates.ContainsKey(RIGHT_INDEX) && ButtonStates[RIGHT_INDEX] > 0;

        public bool IsConnected => SDL.SDL_GameControllerGetAttached(gameControllerPtr) == SDL.SDL_bool.SDL_TRUE;

        public short LeftTriggerValue => AxisValues.ContainsKey(LEFT_TRIGGER_AXIS) ? AxisValues[LEFT_TRIGGER_AXIS] : (short)0;
        public short RightTriggerValue => AxisValues.ContainsKey(RIGHT_TRIGGER_AXIS) ? AxisValues[RIGHT_TRIGGER_AXIS] : (short)0;

        // Thumbstick-Eigenschaften
        public Point LeftThumbStick => new Point(
            NormalizeAxisValue(AxisValues.GetValueOrDefault(LEFT_STICK_X_AXIS)),
            NormalizeAxisValue(AxisValues.GetValueOrDefault(LEFT_STICK_Y_AXIS))
        );

        public Point RightThumbStick => new Point(
            NormalizeAxisValue(AxisValues.GetValueOrDefault(RIGHT_STICK_X_AXIS)),
            NormalizeAxisValue(AxisValues.GetValueOrDefault(RIGHT_STICK_Y_AXIS))
        );

        public double LeftThumbAbsoluteX { get; private set; }

        public double LeftThumbAbsoluteY { get; private set; }

        public double RightThumbAbsoluteX { get; private set; }

        public double RightThumbAbsoluteY { get; private set; }

        public Point LeftThumbAbsolutePoint => new Point(LeftThumbAbsoluteX, LeftThumbAbsoluteY);
        public Point RightThumbAbsolutePoint => new Point(RightThumbAbsoluteX, RightThumbAbsoluteY);

        public double LeftAbsoluteSpeed { get; set; } = 2;
        public double RightAbsoluteSpeed { get; set; } = 2;


        

        public void ResetLeftGnobAbsolute()
        {
            LeftThumbAbsoluteX = 0;
            LeftThumbAbsoluteY = 0;
        }

        public void ResetRightGnobAbsolute()
        {
            RightThumbAbsoluteX = 0;
            RightThumbAbsoluteY = 0;
        }

        private Task controllerThread;

        private GamepadController(int controllerIndex) : base(controllerIndex)
        {
            
        }

        protected override void OnPoll()
        {
            base.OnPoll();
            MoveAbsolutePoints();
        }

        public void Close()
        {
            closed = true;
            SDL.SDL_GameControllerClose(gameControllerPtr);
            SDL.SDL_Quit();
            // Enable events for game controllers
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION, SDL.SDL_DISABLE);
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN, SDL.SDL_DISABLE);
            //SDL.SDL_EventState(SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP, SDL.SDL_DISABLE);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void MoveLeftThumbAbsolutePoint()
        {
            // Exponent für die Beschleunigung (z. B. 2 für quadratisch, 3 für kubisch)
            const double accelerationExponent = 2.0;

            // Normierte Stick-Werte
            double stickX = LeftThumbStick.X;
            double stickY = LeftThumbStick.Y;

            // Exponentielle Beschleunigung anwenden, Vorzeichen erhalten
            double speedX = Math.Sign(stickX) * Math.Pow(Math.Abs(stickX), accelerationExponent);
            double speedY = Math.Sign(stickY) * Math.Pow(Math.Abs(stickY), accelerationExponent);

            // Geschwindigkeitsskalierung
            double deltaX = speedX * LeftAbsoluteSpeed / 50.0; // 50 statt 100 für mehr Reaktionsfreude
            double deltaY = speedY * LeftAbsoluteSpeed / 50.0;

            // Neue Position berechnen und begrenzen
            
            var newPointX = Clamp(LeftThumbAbsoluteX + deltaX, -1.0, 1.0);
            var newPointY = Clamp(LeftThumbAbsoluteY + deltaY, -1.0, 1.0);

            newPointX = Math.Round(newPointX, 3);
            newPointY = Math.Round(newPointY, 3);

            if (newPointX != LeftThumbAbsoluteX || newPointY != LeftThumbAbsoluteY)
            {
                LeftThumbAbsoluteX = newPointX;
                LeftThumbAbsoluteY = newPointY;
                LeftThumbAbsoluteChanged?.Invoke(this, EventArgs.Empty);
                NotifyPropertyChanged(nameof(LeftThumbAbsoluteX));
                NotifyPropertyChanged(nameof(LeftThumbAbsoluteY));
            }
        }

        private void MoveRightThumbAbsolutePoint()
        {
            var newPointX = RightThumbAbsoluteX + RightThumbStick.X * RightAbsoluteSpeed / 100f;
            var newPointY = RightThumbAbsoluteY + RightThumbStick.Y * RightAbsoluteSpeed / 100f;
            if (newPointX > 1)
            {
                newPointX = 1;
            }
            else if (newPointX < -1)
            {
                newPointX = -1;
            }
            if (newPointY > 1)
            {
                newPointY = 1;
            }
            else if (newPointY < -1)
            {
                newPointY = -1;
            }
            newPointX = Math.Round(newPointX, 2);
            newPointY = Math.Round(newPointY, 2);
            if (newPointX != RightThumbAbsoluteX || newPointY != RightThumbAbsoluteY)
            {
                RightThumbAbsoluteX = newPointX;
                RightThumbAbsoluteY = newPointY;
                RightThumbAbsoluteChanged?.Invoke(this, EventArgs.Empty);
                NotifyPropertyChanged(nameof(RightThumbAbsoluteX));
                NotifyPropertyChanged(nameof(RightThumbAbsoluteY));
            }
        }

        private void MoveAbsolutePoints()
        {
            // The Left THumb Stick should have the speed in which the point moves
            // this method is called in an interval and the PointX and PointY are updated
            if (LeftThumbStick.X != 0 || LeftThumbStick.Y != 0)
            {
                MoveLeftThumbAbsolutePoint();
            }

            if (RightThumbStick.X != 0 || RightThumbStick.Y != 0)
            {
                MoveRightThumbAbsolutePoint();
            }



        }

        public event EventHandler LeftThumbAbsoluteChanged;
        public event EventHandler RightThumbAbsoluteChanged;


        protected override void OnButton(ButtonEventArgs args)
        {
            switch (args.ButtonId)
            {
                case A_BUTTON_INDEX:
                    NotifyPropertyChanged(nameof(IsAPressed));
                    break;
                case B_BUTTON_INDEX:
                    NotifyPropertyChanged(nameof(IsBPressed));
                    break;
                case X_BUTTON_INDEX:
                    NotifyPropertyChanged(nameof(IsXPressed));
                    break;
                case Y_BUTTON_INDEX:
                    NotifyPropertyChanged(nameof(IsYPressed));
                    break;
                case BACK_BUTTON_INDEX:
                    NotifyPropertyChanged(nameof(IsWindowPressed));
                    break;
                case XBOX_BUTTON_INDEX:
                    NotifyPropertyChanged(nameof(IsXboxPressed));
                    break;
                case START_BUTTON_INDEX:
                    NotifyPropertyChanged(nameof(IsMenuPressed));
                    break;
                case LEFT_THUMB_INDEX:
                    NotifyPropertyChanged(nameof(IsLeftJoyconPressed));
                    break;
                case RIGHT_THUMB_INDEX:
                    NotifyPropertyChanged(nameof(IsRightJoyconPressed));
                    break;
                case LB_INDEX:
                    NotifyPropertyChanged(nameof(IsLbPressed));
                    break;
                case RB_INDEX:
                    NotifyPropertyChanged(nameof(IsRbPressed));
                    break;
                case UP_INDEX:
                    NotifyPropertyChanged(nameof(IsUpPressed));
                    break;
                case DOWN_INDEX:
                    NotifyPropertyChanged(nameof(IsDownPressed));
                    break;
                case LEFT_INDEX:
                    NotifyPropertyChanged(nameof(IsLeftPressed));
                    break;
                case RIGHT_INDEX:
                    NotifyPropertyChanged(nameof(IsRightPressed));
                    break;
            }
        }

        public override bool IsThumbstick(byte axisIndex)
        {
            return axisIndex == LEFT_STICK_X_AXIS || axisIndex == LEFT_STICK_Y_AXIS ||
                   axisIndex == RIGHT_STICK_X_AXIS || axisIndex == RIGHT_STICK_Y_AXIS;
        }

        public override bool IsTrigger(byte axisIndex)
        {
            return axisIndex == LEFT_TRIGGER_AXIS || axisIndex == RIGHT_TRIGGER_AXIS;
        }

        protected override void OnThumbstick(AxisEventArgs args)
        {
            switch (args.AxisId)
            {
                case LEFT_STICK_X_AXIS:
                case LEFT_STICK_Y_AXIS:
                    NotifyPropertyChanged(nameof(LeftThumbStick));
                    break;
                case RIGHT_STICK_X_AXIS:
                case RIGHT_STICK_Y_AXIS:
                    NotifyPropertyChanged(nameof(RightThumbStick));
                    break;
            }
        }

        protected override void OnTrigger(TriggerEventArgs args)
        {
            switch (args.TriggerId)
            {
                case LEFT_TRIGGER_AXIS:
                    NotifyPropertyChanged(nameof(LeftTriggerValue));
                    break;
                case RIGHT_TRIGGER_AXIS:
                    NotifyPropertyChanged(nameof(RightTriggerValue));
                    break;
            }
        }

        public static GamepadController GetController(int controllerIndex)
        {
            var controller = new GamepadController(controllerIndex);
            return controller;
        }
    }
}
