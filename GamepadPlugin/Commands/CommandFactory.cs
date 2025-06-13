using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamepadPlugin.Commands
{
    public abstract class ControllerInputBase
    {
        
    }

    public class Command
    {
        public string Name { get; set; }
        private Action<ControllerInputBase> action;

        public Command(string name, Action<ControllerInputBase> execute)
        {
            Name = name;
            action = execute;
        }

        public void Execute(ControllerInputBase input)
        {
            action?.Invoke(input);
        }
    }

    public class CommandFactory
    {
        private const string COMMAND_SELECT_PREV_DEVICE = "SelectPreviousDevice";
        private const string COMMAND_SELECT_NEXT_DEVICE = "SelectNextDevice";

        private static readonly Dictionary<string, Command> commands = new Dictionary<string, Command>
        {
            { COMMAND_SELECT_PREV_DEVICE, new Command(COMMAND_SELECT_PREV_DEVICE, ExecuteSelectPreviousDevice) },
            { COMMAND_SELECT_NEXT_DEVICE, new Command(COMMAND_SELECT_NEXT_DEVICE, ExecuteSelectNextDevice) }
        };

        private static void ExecuteSelectPreviousDevice(ControllerInputBase input)
        {
            // Logic to select the previous device
            Console.WriteLine("Executing command: Select Previous Device");
            // Add your implementation here
        }

        private static void ExecuteSelectNextDevice(ControllerInputBase input)
        {
            // Logic to select the next device
            Console.WriteLine("Executing command: Select Next Device");
            // Add your implementation here
        }

        private static void SetDimmer(ControllerInputBase input)
        {
            // Logic to set the dimmer
            Console.WriteLine("Executing command: Set Dimmer");
            // Add your implementation here
        }

        private static void IncremmentDimmer(ControllerInputBase input)
        {
            // Logic to increment the dimmer
            Console.WriteLine("Executing command: Increment Dimmer");
            // Add your implementation here
        }

        private static void DecrementDimmer(ControllerInputBase input)
        {
            // Logic to decrement the dimmer
            Console.WriteLine("Executing command: Decrement Dimmer");
            // Add your implementation here
        }

        private static void OpenShutter(ControllerInputBase input)
        {
            // Logic to open the shutter
            Console.WriteLine("Executing command: Open Shutter");
            // Add your implementation here
        }

        private static void CloseShutter(ControllerInputBase input)
        {
            // Logic to close the shutter
            Console.WriteLine("Executing command: Close Shutter");
            // Add your implementation here
        }

        private static void SetPosition(ControllerInputBase input)
        {
            // Logic to set the position
            Console.WriteLine("Executing command: Set Position");
            // Add your implementation here
        }

        public static void ExecuteCommand(string commandName, ControllerInputBase input)
        {
            if (commands.TryGetValue(commandName, out Command command))
            {
                command.Execute(input);
            }
            else
            {
                Console.WriteLine($"Command '{commandName}' not found.");
            }
        }
    }
}
