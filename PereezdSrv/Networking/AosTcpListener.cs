using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PereezdSrv.Networking
{
    public class AosTcpListener
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private TcpListener tcpListener;
        private readonly IPEndPoint ipEndPoint;

        private TcpClient tcpClient = null;

        public ListenerStatus Status { get; private set; }
        public event EventHandler<StatusChangedEventArgs> StatusChangedEvent;
        public event EventHandler<AosRequestEventArgs> GetRequestReceivedEvent;
        public event EventHandler<AosConnectionArgs> GetConnectionEvent;
        public event EventHandler LostConnectionEvent;

        public AosTcpListener()
        {
            try
            {
                ipEndPoint = new IPEndPoint(IPAddress.Any, 7777);
                tcpListener = new TcpListener(ipEndPoint);
                Status = ListenerStatus.NotListening;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Can't create AosTcpListener");
            }
        }

        public void Start()
        {
            if (Status == ListenerStatus.Listening)
            {
                return;
            }

            try
            {
                tcpListener.Start();
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
                RaiseStatusChanged(ListenerStatus.Listening);
            }
            catch (SocketException ex)
            {
                RaiseStatusChanged(ListenerStatus.PortNotFree);
                logger.Error(ex, $"Can't start AosTcpListener");
            }
        }

        private void OnClientConnected(IAsyncResult ar)
        {
            IPEndPoint endPoint = null;

            try
            {
                tcpClient = tcpListener.EndAcceptTcpClient(ar);
                endPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                try
                {
                    ConnectionReceived(tcpClient);
                }
                catch (Exception ex)
                {
                    tcpClient?.Close();
                    logger.Error(ex, $"Can't get data from {endPoint?.Address}:{endPoint?.Port}");
                    LostConnectionEvent?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (SocketException ex)
            {
                tcpClient?.Close();
                logger.Error(ex, $"Can't recieve connection from {endPoint?.Address}:{endPoint?.Port}");
                LostConnectionEvent?.Invoke(this, EventArgs.Empty);
            }
            catch(Exception ex)
            {
                logger.Error(ex, $"Can't recieve connection from {endPoint?.Address}:{endPoint?.Port}");
                LostConnectionEvent?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                //LostConnectionEvent?.Invoke(this, EventArgs.Empty);

                try
                {
                    if (Status == ListenerStatus.Listening)
                    {
                        tcpListener.BeginAcceptTcpClient(new AsyncCallback(OnClientConnected), null);
                    }
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        private void ConnectionReceived(TcpClient tcpClient)
        {
            try
            {
                SocketStateObject headerState = new SocketStateObject(9); // sizeof(int)
                Socket client = tcpClient.Client;
                headerState.socket = client;

                GetConnectionEvent?.Invoke(this, new AosConnectionArgs(tcpClient.Client.RemoteEndPoint as IPEndPoint));

                var sync = client.BeginReceive(headerState.buffer, 0, headerState.bufferSize, 0, new AsyncCallback(ReceiveHeaderCallback), headerState);
                sync.AsyncWaitHandle.WaitOne();
            }
            catch(Exception ex)
            {
                tcpClient?.Close();
                logger.Error(ex, $"Can't recieve connection");
                LostConnectionEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ReceiveHeaderCallback(IAsyncResult ar)
        {
            try
            {
                SocketStateObject headerState = (SocketStateObject)ar.AsyncState;
                Socket client = headerState.socket;

                int bytesRead = client.EndReceive(ar);
                //int bodySize = BitConverter.ToInt32(headerState.buffer, sizeof(int));
                int bodySize = BitConverter.ToInt32(headerState.buffer, 5);

                SocketStateObject bodyState = new SocketStateObject(bodySize);
                bodyState.socket = client;

                var sync = client.BeginReceive(bodyState.buffer, 0, bodyState.bufferSize, 0, new AsyncCallback(ReceiveBodyCallback), bodyState);
                sync.AsyncWaitHandle.WaitOne();
            }
            catch(Exception ex)
            {
                tcpClient?.Close();
                logger.Error(ex, $"Can't recieve header");
                LostConnectionEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ReceiveBodyCallback(IAsyncResult ar)
        {
            try
            {
                SocketStateObject bodyState = (SocketStateObject)ar.AsyncState;
                Socket client = bodyState.socket;

                int bytesRead = client.EndReceive(ar);

                if(bytesRead == 0)
                {
                    tcpClient?.Close();
                    logger.Error($"Read 0 bytes");
                    LostConnectionEvent?.Invoke(this, EventArgs.Empty);
                    return;
                }

                string tcpRequest = Encoding.Unicode.GetString(bodyState.buffer);

                GetRequestReceivedEvent?.Invoke(this, new AosRequestEventArgs(tcpRequest));

                SocketStateObject headerState = new SocketStateObject(9);
                headerState.socket = client;

                var sync = client.BeginReceive(headerState.buffer, 0, headerState.bufferSize, 0, new AsyncCallback(ReceiveHeaderCallback), headerState);
                sync.AsyncWaitHandle.WaitOne();
            }
            catch(Exception ex)
            {
                tcpClient?.Close();
                logger.Error(ex, $"Can't recieve body");
                LostConnectionEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Send(byte[] response)
        {
            IPEndPoint endPoint = null;
            try
            {
                endPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;

                byte[] header = BitConverter.GetBytes(response.Length);

                //byte[] magicArray = new byte[] { 4, 3, 1, 0, 0 }; // для совместимости
                //tcpClient.Client.Send(magicArray); // для совместимости

                tcpClient.Client.Send(header);
                tcpClient.Client.Send(response);

                return true;
            }
            catch (Exception ex)
            {
                tcpClient?.Close();
                logger.Error(ex, $"Can't send AosResponse: {response.Length} to {endPoint?.Address}:{endPoint?.Port}");
                return false;
            }
        }

        private void RaiseStatusChanged(ListenerStatus status)
        {
            if (Status != status)
            {
                Status = status;
                StatusChangedEvent?.Invoke(this, new StatusChangedEventArgs(Status));
            }
        }

        public void Stop()
        {
            tcpListener?.Stop();
            Status = ListenerStatus.NotListening;
        }
    }
}
