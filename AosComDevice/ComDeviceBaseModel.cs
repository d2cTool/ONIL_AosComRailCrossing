namespace AosComDevice
{
    public class ComDeviceBaseModel
    {
        public string Serial;
        public int Firmware;

        public ComDeviceBaseModel()
        {
            Serial = string.Empty;
            Firmware = 0;
        }

        public bool HasInfo()
        {
            if (Serial != string.Empty && Firmware != 0)
            {
                return true;
            }
            return false;
        }
    }
}
