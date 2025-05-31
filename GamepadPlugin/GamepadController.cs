using GamepadPlugin.Extensions;
using Lumos.GUI.User;
using LumosLIB.Kernel.Scene.Fanning;
using SDL2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamepadPlugin
{
    public class ButtonEvent : EventArgs
    {
        public byte ButtonId { get; }
        public byte State { get; }

        public ButtonEvent(byte buttonId, byte state)
        {
            ButtonId = buttonId;
            State = state;
        }
    }

    public class AxisEvent : EventArgs
    {
        public byte AxisId { get; }
        public short Value { get; }

        public AxisEvent(byte axisId, short value)
        {
            AxisId = axisId;
            Value = value;
        }
    }

    public class GamepadController : INotifyPropertyChanged
    {
        private nint gameControllerPtr;
        private readonly int controllerIndex;

        private const byte A_BUTTON_INDEX = 0;
        private const byte B_BUTTON_INDEX = 1;
        private const byte X_BUTTON_INDEX = 2;
        private const byte Y_BUTTON_INDEX = 3;
        private const byte WINDOW_BUTTON_INDEX = 4;
        private const byte XBOX_BUTTON_INDEX = 5;
        private const byte MENU_BUTTON_INDEX = 6;
        private const byte LEFT_JOYCON_INDEX = 7;
        private const byte RIGHT_JOYCON_INDEX = 8;
        private const byte LB_INDEX = 9;
        private const byte RB_INDEX = 10;
        private const byte UP_INDEX = 11;
        private const byte DOWN_INDEX = 12;
        private const byte LEFT_INDEX = 13;
        private const byte RIGHT_INDEX = 14;

        private const byte LEFT_STICK_X_AXIS = 0;
        private const byte LEFT_STICK_Y_AXIS = 1;
        private const byte RIGHT_STICK_X_AXIS = 2;
        private const byte RIGHT_STICK_Y_AXIS = 3;
        private const byte LEFT_TRIGGER_AXIS = 4;
        private const byte RIGHT_TRIGGER_AXIS = 5;


        public event EventHandler<ButtonEvent> ButtonEvent;
        public event EventHandler<AxisEvent> AxisEvent;
        public event EventHandler PollEvent;

        public SDL.SDL_Event? CurrentEvent { get; private set; }

        public ConcurrentDictionary<byte, short> AxisValues { get; } = new ConcurrentDictionary<byte, short>();
        public ConcurrentDictionary<byte, byte> ButtonStates { get; } = new ConcurrentDictionary<byte, byte>();

        public bool IsAPressed => ButtonStates.ContainsKey(A_BUTTON_INDEX) && ButtonStates[A_BUTTON_INDEX] > 0;
        public bool IsBPressed => ButtonStates.ContainsKey(B_BUTTON_INDEX) && ButtonStates[B_BUTTON_INDEX] > 0;
        public bool IsXPressed => ButtonStates.ContainsKey(X_BUTTON_INDEX) && ButtonStates[X_BUTTON_INDEX] > 0;
        public bool IsYPressed => ButtonStates.ContainsKey(Y_BUTTON_INDEX) && ButtonStates[Y_BUTTON_INDEX] > 0;

        public bool IsWindowPressed => ButtonStates.ContainsKey(WINDOW_BUTTON_INDEX) && ButtonStates[WINDOW_BUTTON_INDEX] > 0;
        public bool IsXboxPressed => ButtonStates.ContainsKey(XBOX_BUTTON_INDEX) && ButtonStates[XBOX_BUTTON_INDEX] > 0;
        public bool IsMenuPressed => ButtonStates.ContainsKey(MENU_BUTTON_INDEX) && ButtonStates[MENU_BUTTON_INDEX] > 0;
        public bool IsLeftJoyconPressed => ButtonStates.ContainsKey(LEFT_JOYCON_INDEX) && ButtonStates[LEFT_JOYCON_INDEX] > 0;
        public bool IsRightJoyconPressed => ButtonStates.ContainsKey(RIGHT_JOYCON_INDEX) && ButtonStates[RIGHT_JOYCON_INDEX] > 0;

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
        public (float X, float Y) LeftThumbStick => (
            NormalizeAxisValue(AxisValues.GetValueOrDefault(LEFT_STICK_X_AXIS)),
            NormalizeAxisValue(AxisValues.GetValueOrDefault(LEFT_STICK_Y_AXIS))
        );

        public (float X, float Y) RightThumbStick => (
            NormalizeAxisValue(AxisValues.GetValueOrDefault(RIGHT_STICK_X_AXIS)),
            NormalizeAxisValue(AxisValues.GetValueOrDefault(RIGHT_STICK_Y_AXIS))
        );

        public float PointX { get; private set; }

        public float PointY { get; private set; }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ResetPoint()
        {
            PointX = 0;
            PointY = 0;
        }

        private float NormalizeAxisValue(short value)
        {
            
            var val = value < 0
                ? value / 32768.0f
                : value / 32767.0f;
            // Calculate in drift zone of the thumb stick
            if (MathF.Abs(val) < 0.1f)
            {
                return 0;
            }

            return MathF.Round(val, 2);
        }

        public string Name => SDL.SDL_GameControllerName(gameControllerPtr);

        private bool closed = false;

        private GamepadController(int controllerIndex)
        {
            this.controllerIndex = controllerIndex;
            PointPositionChanged += GamepadController_PointPositionChanged;
        }

        private void GamepadController_PointPositionChanged(object? sender, EventArgs e)
        {
            var deviceGroup = UserManager.getInstance().SelectedDeviceGroup;
            if (deviceGroup != null)
            {
                var posProp = deviceGroup.GUIProperties
                        .FirstOrDefault(n => n.PropertyType == org.dmxc.lumos.Kernel.DeviceProperties.EPropertyType.Position);
                if (posProp != null)
                {
                    var pos = new org.dmxc.lumos.Kernel.PropertyType.Position(PointX * 180, PointY * 90);
                    PositionFannedValue p = PositionFannedValue.FromOperatorAndValues("", new object[] { pos });
                    posProp.ProgrammerValue = p;
                }

            }
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

        private void MovePointAround()
        {
            // The Left THumb Stick should have the speed in which the point moves
            // this method is called in an interval and the PointX and PointY are updated
            if (LeftThumbStick.X == 0 && LeftThumbStick.Y == 0)
            {
                return;
            }
            var newPointX = PointX + (LeftThumbStick.X * 5 / 100f);
            var newPointY = PointY + (LeftThumbStick.Y * 5 / 100f);
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


            newPointX = MathF.Round(newPointX, 2);
            newPointY = MathF.Round(newPointY, 2);

            if (newPointX != PointX || newPointY != PointY)
            {
                PointX = newPointX;
                PointY = newPointY;
                PointPositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler PointPositionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void InitInTask()
        {
            Task.Run(async () =>
            {
                // Initialize SDL2
                if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_GAMECONTROLLER) < 0)
                {
                    throw new Exception($"SDL could not initialize! SDL_Error: {SDL.SDL_GetError()}");
                }

                int joystickCount = SDL.SDL_NumJoysticks();
                if (joystickCount < controllerIndex)
                {
                    throw new Exception("No joysticks connected!");
                }

                SDL.SDL_JoystickEventState(SDL.SDL_ENABLE);
                IntPtr gameController = SDL.SDL_GameControllerOpen(controllerIndex);
                if (gameController == IntPtr.Zero)
                {
                    throw new Exception($"Could not open gamecontroller! SDL_Error: {SDL.SDL_GetError()}");
                }
                this.gameControllerPtr = gameController;

                while (!closed)
                {
                    SDL.SDL_Event e;
                    int pollIterations = 0;
                    var sw = Stopwatch.StartNew();

                    while (SDL.SDL_PollEvent(out e) != 0)
                    {
                        pollIterations++;
                        if (e.type == SDL.SDL_EventType.SDL_QUIT)
                        {
                            closed = true;
                        }

                        if (e.type == SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION)
                        {
                            AxisValues.AddOrUpdate(e.caxis.axis, e.caxis.axisValue, (key, oldValue) => e.caxis.axisValue);
                            //AxisEvent?.Invoke(this, new AxisEvent(e.caxis.axis, e.caxis.axisValue));

                            // 0 = Left Stick X (min: -32768, max: 32767)
                            // 1 = Left Stick Y

                            // 4 = LT
                            // 5 = RT

                            // logger?.LogInformation($"Axis {e.caxis.axis} moved to {e.caxis.axisValue}");


                        }
                        else if (e.type == SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN || e.type == SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP)
                        {
                            ButtonStates.AddOrUpdate(e.cbutton.button, e.cbutton.state, (key, oldValue) => e.cbutton.state);
                            //ButtonEvent?.Invoke(this, new ButtonEvent(e.cbutton.button, e.cbutton.state));

                            // 0 = A
                            // 1 = B
                            // 2 = X
                            // 3 = Y
                            // 4 = Windows
                            // 5 = XBox Key
                            // 6 = Menu
                            // 7 = Stick top left
                            // 8 = Stick Right
                            // 9 = LB
                            // 10 = RB
                            // 11 = Up
                            // 12 = Down
                            // 13 = Left
                            // 14 = Right

                            if (e.cbutton.button == UP_INDEX)
                            {
                                PointY += 0.1f;
                            }
                            else if (e.cbutton.button == DOWN_INDEX)
                            {
                                PointY -= 0.1f;
                            }
                            else if (e.cbutton.button == LEFT_INDEX)
                            {
                                PointX -= 0.1f;
                            }
                            else if (e.cbutton.button == RIGHT_INDEX)
                            {
                                PointX += 0.1f;
                            }

                            switch (e.cbutton.button)
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
                                case WINDOW_BUTTON_INDEX:
                                    NotifyPropertyChanged(nameof(IsWindowPressed));
                                    break;
                                case XBOX_BUTTON_INDEX:
                                    NotifyPropertyChanged(nameof(IsXboxPressed));
                                    break;
                                case MENU_BUTTON_INDEX:
                                    NotifyPropertyChanged(nameof(IsMenuPressed));
                                    break;
                                case LEFT_JOYCON_INDEX:
                                    NotifyPropertyChanged(nameof(IsLeftJoyconPressed));
                                    break;
                                case RIGHT_JOYCON_INDEX:
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
                            default:
                                    break;
                            }

                            //logger?.LogInformation($"Button {e.cbutton.button} {(e.cbutton.state == 1 ? "pressed" : "released")}");
                        }
                        else
                        {
                            CurrentEvent = e;
                        }
                    }
                    sw.Stop();

                    if (pollIterations > 0)
                    {
                        //logger?.LogInformation($"Polled {pollIterations} events in {sw.ElapsedMilliseconds}ms");
                        PollEvent?.Invoke(this, EventArgs.Empty);
                    }

                    if (sw.ElapsedMilliseconds > 100)
                    {
                        //logger?.LogWarning($"Polling took {sw.ElapsedMilliseconds}ms");
                    }

                    MovePointAround();
                    // Thread Sleep verwenden und nicht Task Delay sonst hängt der ganze Bums...
                    // Bei 10ms crasht das häufige Senden die LumosGUI
                    //System.Threading.Thread.Sleep(50);
                    await Task.Delay(TimeSpan.FromMilliseconds(100));

                }
            });
        }

        public static GamepadController GetController(int controllerIndex)
        {
            var controller = new GamepadController(controllerIndex);
            controller.InitInTask();
            return controller;
        }
    }
}
