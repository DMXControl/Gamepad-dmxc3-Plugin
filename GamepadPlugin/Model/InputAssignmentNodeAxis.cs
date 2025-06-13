using GamepadPlugin.InputControllers;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using org.dmxc.lumos.Kernel.PropertyType;
using System;
using System.Linq;
using System.Windows;

namespace GamepadPlugin.Model
{
    internal class InputAssignmentNodeAxis : AbstractInputSource
    {
        private readonly GamepadController controller;
        private readonly byte[] axisIds;
        private readonly Func<GamepadController, Point> pointSelector;

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Position;

        public override object Min => new Position(-1,-1);

        public override object Max => new Position(1, 1);

        public InputAssignmentNodeAxis(GamepadController controller, string name, byte[] axisIds, Func<GamepadController, Point> pointSelector) : base($"Source/GP_{controller.Serial}_Axis_{name}", name, InputAssignmentNodeButton.CreateForController(controller), null)
        {
            this.controller = controller;
            this.axisIds = axisIds;
            this.pointSelector = pointSelector;
            this.controller.AxisEvent += Controller_AxisEvent;
        }

        private void Controller_AxisEvent(object sender, AxisEventArgs e)
        {
            if (axisIds.Contains(e.AxisId))
            {
                var point = pointSelector(controller);
                this.CurrentValue = new Position(point.X, point.Y);
            }
        }
    }

    internal class InputAssignmentNodeTrigger : AbstractInputSource
    {
        private GamepadController controller;
        private byte triggerId;

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Numeric;

        public override object Min => 0;

        public override object Max => 32_767;

        public InputAssignmentNodeTrigger(GamepadController controller, string name, byte triggerId) : base($"Source/GP_{controller.Serial}_Trigger_{name}", name, InputAssignmentNodeButton.CreateForController(controller), null)
        {
            this.controller = controller;
            this.triggerId = triggerId;
            this.controller.TriggerEvent += Controller_TriggerEvent;
        }

        private void Controller_TriggerEvent(object sender, TriggerEventArgs e)
        {
            if (e.TriggerId == triggerId)
            {
                this.CurrentValue = e.Value; // Assuming e.Value is of type short or can be cast to it
            }
        }
    }



}
