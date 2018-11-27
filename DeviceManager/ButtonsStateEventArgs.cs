using System;
using System.Collections.Generic;

namespace ComDeviceManager
{
    public class ButtonsStateEventArgs : EventArgs
    {
        public Dictionary<int, byte> Data { get; private set; }
        public ButtonsStateEventArgs(Dictionary<int, byte> data) { Data = data; }
    }
}
