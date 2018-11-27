using NLog;
using System;
using System.Runtime.InteropServices;
using AosComDevice;
using System.Xml.Linq;
using ComDeviceManager;

namespace AosComWrapper
{
    public class Wrapper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static volatile Wrapper instance;

        public delegate void OnConnectDelegate(string deviceSerial);
        public delegate void OnDisconnectDelegate();
        public delegate void OnStateChangedDelegate(int btn, byte state);
        public delegate void OnChangedFirmwareDelegate(int version);

        public static OnConnectDelegate onConnectDelegate;
        public static OnDisconnectDelegate onDisconnectDelegate;
        public static OnStateChangedDelegate onStateChangedDelegate;
        public static OnChangedFirmwareDelegate onChangedFirmwareDelegate;

        public static ComReader comReader;
        public static ComWorker comWorker;
        public static DeviceManager DeviceManager;

        private Wrapper() { }

        public static Wrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Wrapper();
                }
                return instance;
            }
        }

        #region events
        private static void Worker_DisconnectDevice(object sender, EventArgs e)
        {
            onDisconnectDelegate?.Invoke();
        }

        private static void Worker_ConnectDevice(object sender, ConnectDeviceEventArgs e)
        {
            onConnectDelegate?.Invoke(e.DeviceSerial);
            //comWorker.GetMessage += ComWorker_GetMessage;
        }

        private static void Worker_ChangeFirmwareVersion(object sender, FirmwareVersionEventArgs e)
        {
            onChangedFirmwareDelegate?.Invoke(e.Version);
        }

        private static void DeviceManager_ChangedState(object sender, ButtonsStateEventArgs e)
        {
            //logger.Debug($"Wrapper DeviceManager_ChangedState");
            foreach (var item in e.Data)
            {
                //logger.Debug($"Btn: {item.Key}: {item.Value}");
                onStateChangedDelegate?.Invoke(item.Key, item.Value);
            }
        }
        #endregion events

        [DllExport(CallingConvention.Cdecl)]
        public static void Init()
        {
            comReader = ComReader.Instance;
            comWorker = new ComWorker(comReader);

            comWorker.ChangeFirmwareVersion += Worker_ChangeFirmwareVersion;
            comWorker.ConnectDevice += Worker_ConnectDevice;
            comWorker.DisconnectDevice += Worker_DisconnectDevice;
            comWorker.Start();
        }

        [DllExport(CallingConvention.Cdecl)]
        public static void Stop()
        {
            comWorker.ChangeFirmwareVersion -= Worker_ChangeFirmwareVersion;
            comWorker.ConnectDevice -= Worker_ConnectDevice;
            comWorker.DisconnectDevice -= Worker_DisconnectDevice;
            comWorker.Stop();

            DeviceManager?.Dispose();
        }

        [DllExport(CallingConvention.Cdecl)]
        public static void InitDevice()
        {
            comWorker.ClearDeviceState();

            //logger.Debug($"Wrapper InitDevice");
            DeviceManager?.Dispose();
            DeviceManager = new DeviceManager(comWorker);
            DeviceManager.ChangeState += DeviceManager_ChangedState;
        }

        [DllExport(CallingConvention.Cdecl)]
        public static void Send(string str)
        {
            comReader?.Send(str);
        }

        [DllExport(CallingConvention.Cdecl)]
        public static int GetFirmware()
        {
            return comWorker?.FirmwareVersion ?? 0;
        }

        [DllExport(CallingConvention.Cdecl)]
        public static int UpdateFirmware(string hexFullName)
        {
            comWorker?.FirmwareUpdate(hexFullName);
            return comWorker?.FirmwareVersion ?? 0;
        }

        [DllExport(CallingConvention.Cdecl)]
        public static int SetLamp(int index, byte value)
        {
            comWorker?.SetLamp(index, value);
            return value;
        }

        [DllExport("OnConnectCallbackFunction", CallingConvention.Cdecl)]
        public static bool OnConnectCallbackFunction(IntPtr callback)
        {
            //logger.Debug($"OnConnectCallbackFunction");
            onConnectDelegate = (OnConnectDelegate)Marshal.GetDelegateForFunctionPointer(callback, typeof(OnConnectDelegate));
            return true;
        }

        [DllExport("OnDisconnectCallbackFunction", CallingConvention.Cdecl)]
        public static bool OnDisconnectCallbackFunction(IntPtr callback)
        {
            //logger.Debug($"OnDisconnectCallbackFunction");
            onDisconnectDelegate = (OnDisconnectDelegate)Marshal.GetDelegateForFunctionPointer(callback, typeof(OnDisconnectDelegate));
            return true;
        }

        [DllExport("OnStateChangedCallbackFunction", CallingConvention.Cdecl)]
        public static bool OnStateChangedCallbackFunction(IntPtr callback)
        {
            //logger.Debug($"OnStateChangedCallbackFunction");
            onStateChangedDelegate = (OnStateChangedDelegate)Marshal.GetDelegateForFunctionPointer(callback, typeof(OnStateChangedDelegate));
            return true;
        }

        [DllExport("OnChangedFirmwareCallbackFunction", CallingConvention.Cdecl)]
        public static bool OnChangedFirmwareCallbackFunction(IntPtr callback)
        {
            //logger.Debug($"OnChangedFirmwareCallbackFunction");
            onChangedFirmwareDelegate = (OnChangedFirmwareDelegate)Marshal.GetDelegateForFunctionPointer(callback, typeof(OnChangedFirmwareDelegate));
            return true;
        }
    }
}
