using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamepadPlugin.InputControllers
{
    public class ButtonEventArgs : EventArgs
    {
        public byte ButtonId { get; }
        public byte State { get; }

        public ButtonEventArgs(byte buttonId, byte state)
        {
            ButtonId = buttonId;
            State = state;
        }
    }

    public class AxisEventArgs : EventArgs
    {
        public byte AxisId { get; }
        public short Value { get; }

        public AxisEventArgs(byte axisId, short value)
        {
            AxisId = axisId;
            Value = value;
        }
    }

    public class TriggerEventArgs : EventArgs
    {
        public byte TriggerId { get; }
        public short Value { get; }
        public TriggerEventArgs(byte triggerId, short value)
        {
            TriggerId = triggerId;
            Value = value;
        }
    }
}
