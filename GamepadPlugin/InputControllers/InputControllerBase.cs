using SDL2;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows.Threading;

namespace GamepadPlugin.InputControllers
{
    /// <summary>
    /// Basic game controller. Can be any type like the Gamepad but might also be an Sony Playstation controller
    /// or something like the controllers of an VR headset.
    /// </summary>
    public abstract class InputControllerBase : INotifyPropertyChanged
    {
        protected nint gameControllerPtr;
        private readonly int controllerIndex;
        private int pollSpeed = 50;
        private DispatcherTimer timer;
        protected bool closed = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ButtonEventArgs> ButtonEvent;
        public event EventHandler<AxisEventArgs> AxisEvent;
        public event EventHandler<TriggerEventArgs> TriggerEvent;

        /// <summary>
        /// A dictionary that holds the last value input for any axis
        /// </summary>
        public ConcurrentDictionary<byte, short> AxisValues { get; } = new ConcurrentDictionary<byte, short>();

        /// <summary>
        /// A dictionary that holds the last input for any button.
        /// </summary>
        public ConcurrentDictionary<byte, byte> ButtonStates { get; } = new ConcurrentDictionary<byte, byte>();

        public SDL.SDL_Event? CurrentEvent { get; private set; }
        public int ControllerIndex => controllerIndex;

        public string Name => SDL.SDL_GameControllerName(gameControllerPtr);

        public string Serial => SDL.SDL_GameControllerGetSerial(gameControllerPtr);

        public double StickDriftCorrectionThreshold { get; set; } = 0.08;

        public int PollSpeed
        {
            get => pollSpeed;
            set
            {
                if (value != pollSpeed)
                {
                    pollSpeed = value;
                    if (timer != null)
                    {
                        timer.Interval = TimeSpan.FromMilliseconds(pollSpeed);
                    }
                }
            }
        }

        protected InputControllerBase(int controllerIndex)
        {
            this.controllerIndex = controllerIndex;
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
            gameControllerPtr = gameController;
            InitInDispatcherTimer();
        }

        protected double NormalizeAxisValue(short value)
        {

            var val = value < 0
                ? value / 32768.0d
                : value / 32767.0d;
            // Calculate in drift zone of the thumb stick
            if (Math.Abs(val) < StickDriftCorrectionThreshold)
            {
                return 0;
            }

            return Math.Round(val, 3);
        }

        public virtual bool IsThumbstick(byte axis) => false;
        public virtual bool IsTrigger(byte axis) => false;

        /// <summary>
        /// Will be called when a Trigger is pressed. For this the <see cref="IsTrigger(byte)"/> needs to be true
        /// on an axis event.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnTrigger(TriggerEventArgs args) { }

        /// <summary>
        /// Will be called when the axis value of a thumbstick changes. Note that there is a correction
        /// for a controller drift set with the <see cref="StickDriftCorrectionThreshold"/> also the
        /// <see cref="IsThumbstick(byte)"/> needs to return true for the axis id of the input.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnThumbstick(AxisEventArgs args) { }

        /// <summary>
        /// Will be called when the input of a button changes
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnButton(ButtonEventArgs args) { }

        protected virtual void OnPoll() { }

        private void HandleAxisEvent(SDL.SDL_Event ev)
        {
            if (!AxisValues.ContainsKey(ev.caxis.axis))
            {
                AxisValues.TryAdd(ev.caxis.axis, ev.caxis.axisValue);
                if (IsThumbstick(ev.caxis.axis))
                {
                    var args = new AxisEventArgs(ev.caxis.axis, ev.caxis.axisValue);
                    OnThumbstick(args);
                    AxisEvent?.Invoke(this, args);
                }
                else if (IsTrigger(ev.caxis.axis))
                {
                    var args = new TriggerEventArgs(ev.caxis.axis, ev.caxis.axisValue);
                    OnTrigger(args);
                    TriggerEvent?.Invoke(this, args);
                }
            }
            else
            {
                if (IsThumbstick(ev.caxis.axis))
                {
                    var newValue = NormalizeAxisValue(ev.caxis.axisValue);
                    if (AxisValues[ev.caxis.axis] != newValue)
                    {
                        AxisValues[ev.caxis.axis] = ev.caxis.axisValue;
                        AxisEvent?.Invoke(this, new AxisEventArgs(ev.caxis.axis, ev.caxis.axisValue));
                    }
                }
                else if (IsTrigger(ev.caxis.axis))
                {
                    var newValue = ev.caxis.axisValue;

                    // Check if the change is at least a step of 5
                    if (Math.Abs(AxisValues[ev.caxis.axis] - newValue) >= 5)
                    {
                        AxisValues[ev.caxis.axis] = ev.caxis.axisValue;
                        TriggerEvent?.Invoke(this, new TriggerEventArgs(ev.caxis.axis, ev.caxis.axisValue));
                    }
                }
            }
        }

        private void InitInDispatcherTimer()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(PollSpeed)
            };

            timer.Tick += (sender, e) =>
            {
                if (closed)
                {
                    timer.Stop();
                    return;
                }
                SDL.SDL_Event ev;
                while (SDL.SDL_PollEvent(out ev) != 0)
                {
                    switch(ev.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            closed = true;
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
                            HandleAxisEvent(ev);
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                        case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
                            ButtonStates.AddOrUpdate(ev.cbutton.button, ev.cbutton.state, (key, oldValue) => ev.cbutton.state);
                            var args = new ButtonEventArgs(ev.cbutton.button, ev.cbutton.state);
                            OnButton(args);
                            ButtonEvent?.Invoke(this, args);
                            break;

                    }
                }
                OnPoll();
            };
            timer.Start();
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
