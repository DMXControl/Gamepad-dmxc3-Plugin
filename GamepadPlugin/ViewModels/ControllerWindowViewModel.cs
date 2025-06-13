using GamepadPlugin.InputControllers;
using GamepadPlugin.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GamepadPlugin.ViewModels
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> execute;

        public RelayCommand(Action<object> execute)
        {
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.execute?.Invoke(parameter);
        }
    }

    public class ControllerWindowViewModel : INotifyPropertyChanged
    {
        public GamepadController Controller { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private SpecialOperation op_leftTrigger = EmptyOperation.Instance;
        private SpecialOperation op_leftBumper = EmptyOperation.Instance;
        private SpecialOperation op_rightBumper = EmptyOperation.Instance;
        private SpecialOperation op_rightTrigger = EmptyOperation.Instance;

        private SpecialOperation op_leftThumbMove = EmptyOperation.Instance;
        private SpecialOperation op_leftThumbClick = EmptyOperation.Instance;
        private SpecialOperation op_padLeft = EmptyOperation.Instance;
        private SpecialOperation op_padUp = EmptyOperation.Instance;
        private SpecialOperation op_padRight = EmptyOperation.Instance;
        private SpecialOperation op_padDown = EmptyOperation.Instance;
        private SpecialOperation op_Back = EmptyOperation.Instance;
        private SpecialOperation op_Start = EmptyOperation.Instance;
        private SpecialOperation op_buttonX = EmptyOperation.Instance;
        private SpecialOperation op_buttonY = EmptyOperation.Instance;
        private SpecialOperation op_buttonB = EmptyOperation.Instance;
        private SpecialOperation op_buttonA = EmptyOperation.Instance;
        private SpecialOperation op_rightThumbMove = EmptyOperation.Instance;
        private SpecialOperation op_rightThumbClick = EmptyOperation.Instance;

        private Dictionary<int, SpecialOperation> buttonOperationSelector = new Dictionary<int, SpecialOperation>();

        private string selectedKeyName;
        public string SelectedKeyName
        {
            get => selectedKeyName;
            set
            {
                if (selectedKeyName != value)
                {
                    selectedKeyName = value;
                    OnPropertyChanged();
                }
            }
            
        }

        public IEnumerable<SpecialOperationBuilder> ButtonOperations => SpecialOperationBuilder.ButtonOperationBilders;
        public IEnumerable<SpecialOperationBuilder> ThumbstickOperations => SpecialOperationBuilder.ThumbstickOperationBuilders;

        public IEnumerable<SpecialOperationBuilder> TriggerOperations => SpecialOperationBuilder.TriggerOperationBilders;

        private SpecialOperationBuilder buttonAOperation = EmptyOperationBuilder.Instance;
        public SpecialOperationBuilder ButtonAOperation
        {
            get => buttonAOperation;
            set
            {
                if (buttonAOperation != value)
                {
                    buttonAOperation = value;
                    op_buttonA = value.Create();
                    OnPropertyChanged();
                }
            }
        }

        private SpecialOperationBuilder leftThumbOperation = EmptyOperationBuilder.Instance;
        public SpecialOperationBuilder LeftThumbOperation
        {
            get => leftThumbOperation;
            set
            {
                if (leftThumbOperation != value)
                {
                    leftThumbOperation = value;
                    op_leftThumbMove = value.Create();
                    OnPropertyChanged();
                }
            }
        }

        public ControllerWindowViewModel(GamepadController controller)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            Controller.ButtonEvent += Controller_ButtonEvent;
            Controller.LeftThumbAbsoluteChanged += Controller_LeftThumbAbsoluteChanged;
        }

        private void Controller_LeftThumbAbsoluteChanged(object sender, EventArgs e)
        {
            op_leftThumbMove.ExecutePosition(Controller.LeftThumbAbsolutePoint);
        }



        private void Controller_ButtonEvent(object sender, ButtonEventArgs e)
        {
            if (buttonOperationSelector.TryGetValue(e.ButtonId, out SpecialOperation operation))
            {
                if (e.State == 1)
                {
                    operation.ExecuteDown();
                }
                else
                {
                    operation.ExecuteUp();
                }
            }
        }

        // his parameterless constructor is for design-time data or testing purposes
        public ControllerWindowViewModel()
        {
            buttonOperationSelector.Add(GamepadController.A_BUTTON_INDEX, op_buttonA);
            buttonOperationSelector.Add(GamepadController.B_BUTTON_INDEX, op_buttonB);
            buttonOperationSelector.Add(GamepadController.X_BUTTON_INDEX, op_buttonX);
            buttonOperationSelector.Add(GamepadController.Y_BUTTON_INDEX, op_buttonY);
            buttonOperationSelector.Add(GamepadController.BACK_BUTTON_INDEX, op_Back);
            buttonOperationSelector.Add(GamepadController.START_BUTTON_INDEX, op_Start);
            buttonOperationSelector.Add(GamepadController.LEFT_THUMB_INDEX, op_leftThumbClick);
            buttonOperationSelector.Add(GamepadController.RIGHT_THUMB_INDEX, op_rightThumbClick);
            buttonOperationSelector.Add(GamepadController.LB_INDEX, op_leftBumper);
            buttonOperationSelector.Add(GamepadController.RB_INDEX, op_rightBumper);
            buttonOperationSelector.Add(GamepadController.UP_INDEX, op_padUp);
            buttonOperationSelector.Add(GamepadController.DOWN_INDEX, op_padDown);
            buttonOperationSelector.Add(GamepadController.LEFT_INDEX, op_padLeft);
            buttonOperationSelector.Add(GamepadController.RIGHT_INDEX, op_padRight);

        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
