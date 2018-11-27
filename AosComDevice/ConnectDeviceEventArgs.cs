using System;

namespace AosComDevice
{
    public class ConnectDeviceEventArgs : EventArgs
    {
        public string DeviceSerial { get; private set; }
        public string SubDeviceSerial { get; private set; }
        public ConnectDeviceEventArgs(string deviceSerial) { DeviceSerial = deviceSerial; }
    }
}
