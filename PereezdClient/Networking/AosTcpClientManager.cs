using PereezdClient.Networking.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace PereezdClient.Networking
{
    public class AosTcpClientManager
    {
        private readonly Timer timer;

        private Queue<AosCommand> commandQueue;
        private AosTcpClient AosTcpClient;

        private bool IsConnected;
        private bool NeedConnection;

        public event EventHandler<StatusChangedEventArgs> StatusChangedEvent;
        public event EventHandler<AosRequestEventArgs> AosRequestReceivedEvent;

        public AosTcpClientManager(string ip, string port)
        {
            commandQueue = new Queue<AosCommand>();

            IsConnected = false;
            NeedConnection = false;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
            AosTcpClient = new AosTcpClient(endPoint);

            AosTcpClient.AosRequestReceivedEvent += AosTcpClient_AosRequestReceivedEvent;
            AosTcpClient.StatusChangedEvent += AosTcpClient_StatusChangedEvent;

            timer = new Timer(OnTimerElapsed, null, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(5000));
        }

        public void Connect()
        {
            NeedConnection = true;
            AosTcpClient.Connect();
        }

        public void Disconnect()
        {
            NeedConnection = false;
            AosTcpClient.Disconnect();
        }

        public void AddCommand(AosCommand aosCommand)
        {
            commandQueue.Enqueue(aosCommand);
            AosTcpClient.SendRequest(commandQueue.Dequeue());
        }

        private void OnTimerElapsed(object state)
        {
            if (NeedConnection && !IsConnected)
            {
                //AosTcpClient.Connect();
            }

            if (IsConnected && commandQueue.Count > 0)
            {
                AosTcpClient.SendRequest(commandQueue.Dequeue());
            }
        }

        private void AosTcpClient_StatusChangedEvent(object sender, StatusChangedEventArgs e)
        {
            StatusChangedEvent?.Invoke(this, e);

            switch (e.TcpListenerStatus)
            {
                case ClientStatus.Connected:
                    IsConnected = true;
                    break;
                case ClientStatus.Disconnected:
                    IsConnected = false;
                    break;
                case ClientStatus.PortNotFree:
                    IsConnected = false;
                    break;
                default:
                    break;
            }
        }

        private void AosTcpClient_AosRequestReceivedEvent(object sender, AosRequestEventArgs e)
        {
            AosRequestReceivedEvent?.Invoke(this, e);
        }
    }
}
