using GamepadPlugin.InputControllers;
using LumosLIB.Kernel;
using LumosLIB.Tools.I18n;
using LumosProtobuf;
using LumosProtobuf.Input;
using org.dmxc.lumos.Kernel.Input.v2;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace GamepadPlugin.Model
{
    internal class InputAssignmentNodeButton : AbstractInputSource
    {
        private readonly GamepadController controller;
        private readonly int buttonIndex;

        public override EWellKnownInputType AutoGraphIOType => EWellKnownInputType.Bool;

        public override object Min => false;

        public override object Max => true;

        public InputAssignmentNodeButton(GamepadController controller, string buttonName, int buttonIndex) : base($"Source/GP_{controller.Serial}_Button_{buttonIndex}", buttonName, CreateForController(controller), null)
        {
            this.controller = controller;
            this.buttonIndex = buttonIndex;
            this.controller.ButtonEvent += Controller_ButtonEvent;
        }

        private void Controller_ButtonEvent(object sender, ButtonEventArgs e)
        {
            if (e.ButtonId == buttonIndex)
            {
                this.CurrentValue = e.State == 1 ? true : false;
            }
        }

        internal static ParameterCategory CreateForController(GamepadController controller)
        {
            
            var gamepadCategory = ParameterCategoryTools.FromName(controller?.Name ?? "Default");
            var root = new ParameterCategory
            {
                Id = "Gamepads",
                Name = T._("Gamepads"),
                SubCategory = gamepadCategory,
            };
            return root;
        }
    }



}
