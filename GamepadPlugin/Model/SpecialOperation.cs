using Lumos.GUI.User;
using LumosLIB.Kernel.Scene.Fanning;
using org.dmxc.lumos.Kernel.PropertyType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GamepadPlugin.Model
{
    public abstract class SpecialOperationBuilder
    {
        public static List<SpecialOperationBuilder> ButtonOperationBilders { get; } = new List<SpecialOperationBuilder>
        {
            EmptyOperationBuilder.Instance,
            new SimpleOperationBuilder("Show Message Box", () => new MessageBoxOperation()),
            // Add other builders here
        };

        public static List<SpecialOperationBuilder> ThumbstickOperationBuilders { get; } = new List<SpecialOperationBuilder>
        {
            EmptyOperationBuilder.Instance,
            new SimpleOperationBuilder("Set Position by Absolute", () => MovePositionAbsoluteOperation.Instance),
            // Add other builders here
        };

        public static List<SpecialOperationBuilder> TriggerOperationBilders { get; } = new List<SpecialOperationBuilder>
        {
            EmptyOperationBuilder.Instance,
            // Add other builders here
        };

        public abstract string Name { get; }

        public abstract SpecialOperation Create();

        public override string ToString() => Name;
    }

    public class EmptyOperationBuilder : SpecialOperationBuilder
    {
        public static readonly EmptyOperationBuilder Instance = new EmptyOperationBuilder();

        public override string Name => "-";
        public override SpecialOperation Create() => EmptyOperation.Instance;
    }

    public class SimpleOperationBuilder : SpecialOperationBuilder
    {
        private readonly Func<SpecialOperation> builder;

        public override string Name { get; }
        public override SpecialOperation Create() => builder();

        public SimpleOperationBuilder(string name, Func<SpecialOperation> builder)
        {
            this.Name = name;
            this.builder = builder;
        }
    }

    public abstract class SpecialOperation
    {

        public virtual void ExecuteDown()
        {
            // Default implementation does nothing
        }

        public virtual void ExecuteUp()
        {
            // Default implementation does nothing
        }

        public virtual void ExecutePosition(Point value)
        {
            // Default implementation does nothing
        }

        public virtual void ExecuteTrigger(double value)
        {
            // Default implementation does nothing
        }
    }

    public class EmptyOperation : SpecialOperation
    {
        public static readonly EmptyOperation Instance = new EmptyOperation();

    }

    public class MessageBoxOperation : SpecialOperation
    {
        public override void ExecuteDown()
        {
            MessageBox.Show("This is a message box operation.", "Message Box", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class MovePositionAbsoluteOperation : SpecialOperation
    {
        public static readonly MovePositionAbsoluteOperation Instance = new MovePositionAbsoluteOperation();
        public override void ExecutePosition(Point value)
        {
            var deviceGroup = UserManager.getInstance().SelectedDeviceGroup;
            if (deviceGroup == null)
            {
                return;
            }


            var posProp = deviceGroup.GUIProperties
                        .FirstOrDefault(n => n.PropertyType == org.dmxc.lumos.Kernel.DeviceProperties.EPropertyType.Position);

            if (posProp == null)
                return;

            var max = (Position)posProp.UpperBound;
            var min = (Position)posProp.LowerBound;

            var pos = new org.dmxc.lumos.Kernel.PropertyType.Position(value.X * max.Pan, value.Y * max.Tilt);
            PositionFannedValue p = PositionFannedValue.FromOperatorAndValues("", new object[] { pos });
            posProp.ProgrammerValue = p;
        }
    }
}
