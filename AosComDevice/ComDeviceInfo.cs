using System;

namespace AosComDevice
{
    public class ComDeviceInfo : EventArgs, IEquatable<ComDeviceInfo>
    {
        public ComDeviceInfo(string deviceID, string name, string port)
        {
            DeviceID = deviceID;
            Name = name;
            Port = port;
        }
        public string DeviceID { get; private set; }
        public string Name { get; private set; }
        public string Port { get; private set; }

        public bool Equals(ComDeviceInfo other)
        {
            return DeviceID.Equals(other.DeviceID) &&
                Name.Equals(other.Name) &&
                Port.Equals(other.Port);
        }
    }
}
