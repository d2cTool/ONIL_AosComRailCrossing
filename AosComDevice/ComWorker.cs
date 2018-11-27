using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AosComDevice
{
    public class ComWorker
    {
        private ComReader comReader;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ComDeviceBaseModel ComDeviceModel;
        private ComDeviceInfo comDeviceInfo;
        public bool IsConnected { get; private set; }
        public int FirmwareVersion => ComDeviceModel.Firmware;
        public string DeviceSerial => ComDeviceModel.Serial;

        public event EventHandler<ConnectDeviceEventArgs> ConnectDevice;
        public event EventHandler DisconnectDevice;
        public event EventHandler<ComMsgEventArgs> GetMessage;
        public event EventHandler<FirmwareVersionEventArgs> ChangeFirmwareVersion;

        public ComWorker(ComReader reader)
        {
            comReader = reader;
        }

        public void Start()
        {
            ComDeviceModel = new ComDeviceBaseModel();
            comReader.GetConnection += ComReader_GetConnection;
            comReader.DataReceived += ComReader_DataReceived;
            comReader.LostConnection += ComReader_LostConnection;
            comReader.Start();
        }

        public void Stop()
        {
            comReader.GetConnection -= ComReader_GetConnection;
            comReader.DataReceived -= ComReader_DataReceived;
            comReader.LostConnection -= ComReader_LostConnection;
            comReader.Stop();
        }

        private void ComReader_GetConnection(object sender, ComDeviceInfo e)
        {
            //logger.Error($"Get new connection: {e.Port}");
            comDeviceInfo = e;

            Send("e");
            Send("n");
        }

        private void ComReader_LostConnection(object sender, EventArgs e)
        {
            IsConnected = false;
            ComDeviceModel = new ComDeviceBaseModel();

            DisconnectDevice?.Invoke(this, EventArgs.Empty);
        }

        private void ComReader_DataReceived(object sender, ComMsgEventArgs e)
        {
            if (IsConnected)
            {
                GetMessage?.Invoke(this, e);
            }

            try
            {
                Parse(e.Data);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Can't parse: {0}", e.Data);
            }
        }

        private void Parse(string msg)
        {
            //n28FFBC6BA1160506
            Regex serials = new Regex(@"^n([0-9A-F]{16})");

            //e180428
            Regex version = new Regex(@"^e(\d+)");

            //PereEzdMaket ver180508. Speed 115200. Input n[umber], e[rsion], l[ow]cb,h[igh]cb. Output b[utton]cb,i[ndicator]cb, (cb = chip+byte) v[cc]=5.00
            Regex welcome = new Regex(@"^PereEzdMaket ver(\d+)");

            Match match = serials.Match(msg);
            if (match.Success)
            {
                string deviceSerial = match.Groups[1].Value;

                if (ComDeviceModel.Serial != deviceSerial)
                {
                    if (!IsConnected)
                    {
                        IsConnected = true;
                        ConnectDevice?.Invoke(this, new ConnectDeviceEventArgs(deviceSerial));
                    }
                }

                ComDeviceModel.Serial = deviceSerial;
            }

            match = version.Match(msg);
            if (match.Success)
            {
                int firmwareVersion = int.Parse(match.Groups[1].Value);

                if (ComDeviceModel.Firmware != firmwareVersion)
                {
                    if (IsConnected)
                    {
                        ChangeFirmwareVersion?.Invoke(this, new FirmwareVersionEventArgs(firmwareVersion));
                    }
                }

                ComDeviceModel.Firmware = firmwareVersion;
            }

            match = welcome.Match(msg);
            if (match.Success)
            {
                Send("e");
                Send("n");

                int firmwareVersion = int.Parse(match.Groups[1].Value);

                if (ComDeviceModel.Firmware != firmwareVersion)
                {
                    if (IsConnected)
                    {
                        ChangeFirmwareVersion?.Invoke(this, new FirmwareVersionEventArgs(firmwareVersion));
                    }
                }

                ComDeviceModel.Firmware = firmwareVersion;
            }
        }

        public bool FirmwareUpdate(string hexFile)
        {
            if (!IsConnected)
            {
                return false;
            }

            try
            {
                string directoryName = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

                comReader.Stop();

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    FileName = @"ThirdParty\avrdude.exe",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardError = true,
                    WorkingDirectory = directoryName,
                    Arguments = string.Format("-C\"ThirdParty\\avrdude.conf\" -q -pm328p -carduino -P{0} -b57600 -Uflash:w:\"{1}\":i", comDeviceInfo.Port, hexFile)
                    //Arguments = string.Format("-C\"ThirdParty\\avrdude.conf\" -v -v -pm328p -carduino -P{0} -b57600 -Uflash:w:\"{1}\":i", ComPort, hexFile)
                };

                using (Process exeProcess = Process.Start(startInfo))
                {
                    string error = exeProcess.StandardError.ReadToEnd();
                    logger.Info("Firmware update: com: {0}, output: {1}", comDeviceInfo.Port, error);
                    exeProcess.WaitForExit();

                    comReader.Start();
                    comReader.Send("e"); // TODO fix
                    return (exeProcess.ExitCode == 0) ? true : false;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }

        public void SetLamp(int index, byte value)
        {
            if (IsConnected)
            {
                if (value == 1)
                {
                    comReader.Send($"h{index}");
                }

                if (value == 0)
                {
                    comReader.Send($"l{index}");
                }
            }
        }

        public void ClearDeviceState()
        {
            if (IsConnected)
            {
                comReader.Send("r");
            }
        }

        public void Send(string msg)
        {
            comReader.Send(msg);
        }
    }
}
