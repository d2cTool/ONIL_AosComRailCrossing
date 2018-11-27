using NLog;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;

namespace AosComDevice
{
    public sealed class ComReader : IDisposable
    {
        private static readonly object padlock = new object();
        private static volatile ComReader instance;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SerialPort com;
        private readonly ComDeviceInfoManager comDeviceManager;

        private Thread thrd;
        private bool _shouldStop;

        public event EventHandler<ComDeviceInfo> GetConnection;
        public event EventHandler<ComMsgEventArgs> DataReceived;
        public event EventHandler LostConnection;

        private ComReader()
        {
            _shouldStop = true;
            comDeviceManager = new ComDeviceInfoManager();
        }

        public static ComReader Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new ComReader();
                        }
                    }
                }
                return instance;
            }
        }

        private void Connect()
        {
            com = new SerialPort(comDeviceManager.ComDeviceInfo.Port, 115200, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                WriteBufferSize = 4096,
                ReadBufferSize = 4096,
                RtsEnable = false,
                DtrEnable = false,
                NewLine = "\r\n"
            };

            com.DataReceived += DataReceivedHandler;
            com.Open();
        }

        private void Disconnect()
        {
            if (com == null)
            {
                return;
            }

            com.DataReceived -= DataReceivedHandler;

            if (com.IsOpen)
            {
                com.Close();
            }
            com.Dispose();
            com = null;
        }

        private void Loop()
        {
            while (!_shouldStop)
            {
                try
                {
                    Thread.Sleep(0);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    Disconnected();
                }
            }
        }

        private void ComDeviceManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (comDeviceManager.ComDeviceInfo == null)
                {
                    Disconnected();
                }
                else
                {
                    thrd = new Thread(Loop);
                    thrd.IsBackground = true;
                    thrd.Name = "Com listener";
                    thrd.Start();
                    Connected();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void Start()
        {
            _shouldStop = false;
            comDeviceManager.PropertyChanged += ComDeviceManager_PropertyChanged;
            comDeviceManager.Start();
        }

        public void Stop()
        {
            _shouldStop = true;
            comDeviceManager.PropertyChanged -= ComDeviceManager_PropertyChanged;
            comDeviceManager.Stop();
            Disconnect();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort serial = (SerialPort)sender;
                string indata = serial.ReadLine();
                //logger.Debug(" - read: {0}", indata);
                DataReceived?.Invoke(this, new ComMsgEventArgs(indata));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void Send(string data)
        {
            try
            {
                if (!_shouldStop)
                {
                    if (com.IsOpen)
                    {
                        com.Write(data);
                        //logger.Debug(" - send: {0}", data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Can't send: {0}", data);
                Disconnected();
            }
        }

        private void Connected()
        {
            _shouldStop = false;
            Connect();
            GetConnection?.Invoke(this, comDeviceManager.ComDeviceInfo);
        }

        private void Disconnected()
        {
            _shouldStop = true;
            Disconnect();
            LostConnection?.Invoke(null, EventArgs.Empty);
        }

        public void Dispose()
        {
            Disconnect();
            comDeviceManager.Dispose();
        }
    }
}
