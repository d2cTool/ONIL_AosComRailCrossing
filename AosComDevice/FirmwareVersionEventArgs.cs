using System;

namespace AosComDevice
{
    public class FirmwareVersionEventArgs : EventArgs
    {
        public int Version { get; private set; }
        public FirmwareVersionEventArgs(int version) { Version = version; }
    }
}
