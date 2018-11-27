using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Text.RegularExpressions;

namespace AosComDevice
{
    public sealed class ComDeviceInfoManager : IDisposable, INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ManagementEventWatcher _deviceWatcher;
        private ComDeviceInfo comDeviceInfo;

        public ComDeviceInfo ComDeviceInfo
        {
            get => comDeviceInfo;
            private set
            {
                comDeviceInfo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ComDeviceInfo"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ComDeviceInfoManager()
        {
            _deviceWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3"));
        }

        public void Start()
        {
            GetComDevices();
            _deviceWatcher.EventArrived += _deviceWatcher_EventArrived;
            _deviceWatcher.Start();
        }

        public void Stop()
        {
            comDeviceInfo = null;
            _deviceWatcher.EventArrived -= _deviceWatcher_EventArrived;
            _deviceWatcher.Stop();
        }

        private void _deviceWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            GetComDevices();
        }

        private void GetComDevices()
        {
            var list = new List<ComDeviceInfo>();
            ManagementObjectCollection collection;

            try
            {
                string wmiQuery;

                if (IsUseVM())
                    wmiQuery = "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}' and Status='OK'";
                else
                    wmiQuery = "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}' and Status='OK' and Name like '%USB-SERIAL CH340%'";

                using (var searcher = new ManagementObjectSearcher(@"root\CIMV2", wmiQuery))
                {
                    collection = searcher.Get();
                }

                Regex comRegexp = new Regex(@"COM\d+");

                foreach (var device in collection)
                {
                    string name = (string)device.GetPropertyValue("Name");

                    Match match = comRegexp.Match(name);
                    if (match.Success)
                    {
                        string port = match.Value;
                        list.Add(new ComDeviceInfo(
                            device["DeviceID"].ToString(),
                            name,
                            port
                            ));
                    }
                }
                collection.Dispose();

                if (list.Count > 0)
                {
                    if (ComDeviceInfo != null)
                    {
                        foreach (var item in list)
                        {
                            if (ComDeviceInfo.Equals(item))
                            {
                                return;
                            }
                        }
                    }
                    ComDeviceInfo = list[0];
                }
                else
                {
                    if (ComDeviceInfo == null)
                    {
                        return;
                    }
                    ComDeviceInfo = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private bool IsUseVM()
        {
            ManagementObjectCollection collection;

            using (var searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_BaseBoard WHERE Tag = 'Base Board'"))
            {
                collection = searcher.Get();
            }

            Regex comRegexp = new Regex(@" Corporation");

            foreach (var device in collection)
            {
                string m = (string)device.GetPropertyValue("Manufacturer");
                Match match = comRegexp.Match(m);
                if (match.Success)
                {
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            _deviceWatcher.Dispose();
        }
    }
}
