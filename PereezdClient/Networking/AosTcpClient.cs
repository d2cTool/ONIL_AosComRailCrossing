using NLog;
using PereezdClient.Networking.Protocol;
using System;
using System.Net;
using System.Net.Sockets;

namespace PereezdClient.Networking
{
    public class AosTcpClient : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly byte[] magicHeader = { 4, 3, 1, 0, 0 };

        private readonly IPEndPoint endPoint;
        private TcpClient tcpClient;

        public ClientStatus Status { get; private set; }
        public event EventHandler<StatusChangedEventArgs> StatusChangedEvent;
        public event EventHandler<AosRequestEventArgs> AosRequestReceivedEvent;

        public AosTcpClient(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
            //tcpClient = new TcpClient();
        }

        public void Connect()
        {
            try
            {
                if (tcpClient != null)
                    tcpClient.Close();

                tcpClient = new TcpClient();
                tcpClient.Connect(endPoint);

                RaiseStatusChanged(ClientStatus.Connected);
                BeginReceiveData(tcpClient.Client);
            }
            catch (SocketException ex)
            {
                RaiseStatusChanged(ClientStatus.PortNotFree);
                logger.Error(ex, $"Can't start Client");
            }
        }

        public void Disconnect()
        {
            tcpClient?.Close();
            RaiseStatusChanged(ClientStatus.Disconnected);
        }

        public void SendRequest(AosCommand aosCommand)
        {
            try
            {
                byte[] commandAsByteArray = aosCommand.ToByteArray();
                byte[] length = BitConverter.GetBytes(commandAsByteArray.Length);

                byte[] final = new byte[magicHeader.Length + length.Length + commandAsByteArray.Length];
                magicHeader.CopyTo(final, 0);
                length.CopyTo(final, magicHeader.Length);
                commandAsByteArray.CopyTo(final, magicHeader.Length + length.Length);

                //tcpClient.Client.Send(magicHeader);
                //tcpClient.Client.Send(length);
                //tcpClient.Client.Send(commandAsByteArray);
                tcpClient.Client.Send(final);
            }
            catch (SocketException ex)
            {
                RaiseStatusChanged(ClientStatus.Disconnected);
                logger.Error(ex, $"Can't send: {aosCommand.ToUnicodeString()}");
                Disconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Can't send: {aosCommand.ToUnicodeString()}");
            }
        }

        private void RaiseStatusChanged(ClientStatus status)
        {
            if (Status != status)
            {
                Status = status;
                StatusChangedEvent?.Invoke(this, new StatusChangedEventArgs(Status));
            }
        }

        private void BeginReceiveData(Socket tcpClient)
        {
            try
            {
                SocketStateObject headerState = new SocketStateObject(sizeof(int));
                headerState.socket = tcpClient;

                var sync = tcpClient.BeginReceive(headerState.buffer, 0, headerState.bufferSize, 0, new AsyncCallback(ReceiveHeaderCallback), headerState);
                sync.AsyncWaitHandle.WaitOne();
            }
            catch (SocketException ex)
            {
                logger.Error(ex, $"Can't recieve data");
                RaiseStatusChanged(ClientStatus.Disconnected);
                Disconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Can't recieve data");
            }
        }

        private void ReceiveHeaderCallback(IAsyncResult ar)
        {
            try
            {
                SocketStateObject headerState = (SocketStateObject)ar.AsyncState;
                Socket client = headerState.socket;

                int bytesRead = client.EndReceive(ar);
                if (bytesRead == 0)
                {
                    logger.Error($"Read 0 bytes");
                    return;
                }

                int bodySize = BitConverter.ToInt32(headerState.buffer, 0);

                SocketStateObject bodyState = new SocketStateObject(bodySize);
                bodyState.socket = client;

                var sync = client.BeginReceive(bodyState.buffer, 0, bodyState.bufferSize, 0, new AsyncCallback(ReceiveBodyCallback), bodyState);
                sync.AsyncWaitHandle.WaitOne();
            }
            catch (SocketException ex)
            {
                logger.Error(ex, $"Can't recieve data");
                RaiseStatusChanged(ClientStatus.Disconnected);
                Disconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Can't recieve data");
            }
        }

        private void ReceiveBodyCallback(IAsyncResult ar)
        {
            try
            {
                SocketStateObject bodyState = (SocketStateObject)ar.AsyncState;
                Socket client = bodyState.socket;

                int bytesRead = client.EndReceive(ar);
                if (bytesRead == 0)
                {
                    logger.Error($"Read 0 bytes");
                    return;
                }

                AosCommand tcpRequest = AosCommand.ToObj(bodyState.buffer);

                AosRequestReceivedEvent?.Invoke(this, new AosRequestEventArgs(tcpRequest));

                SocketStateObject headerState = new SocketStateObject(sizeof(int));
                headerState.socket = client;

                var sync = client.BeginReceive(headerState.buffer, 0, headerState.bufferSize, 0, new AsyncCallback(ReceiveHeaderCallback), headerState);
                sync.AsyncWaitHandle.WaitOne();
            }
            catch (SocketException ex)
            {
                logger.Error(ex, $"Can't recieve data");
                RaiseStatusChanged(ClientStatus.Disconnected);
                Disconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Can't recieve data");
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
