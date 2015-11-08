using SIClient.Net;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SIClient.Managers
{
    internal class NetClientSelector
    {
        public class ClientChangedEventArgs : EventArgs
        {
            public INetClient Client { get; set; }
        }

        private bool usingTcp = false;
        private HttpNetClient httpClient = new HttpNetClient();
        private TcpNetClient tcpClient = new TcpNetClient();
        private Uri tcpAddress;
        private object locker = new object();

        public INetClient Client { get; private set; }

        public HttpNetClient HttpClient => this.httpClient;
        public TcpNetClient TcpClient => this.tcpClient;

        public event Action<ClientChangedEventArgs, object> OnClientChanged;

        public String HttpAddress
        {
            get
            {
                return this.httpClient.ServerAddress;
            }

            set
            {
                this.httpClient.ServerAddress = value;
                if (!this.usingTcp)
                {
                    this.ForceHttp();
                }
            }
        }

        public Uri TcpAddress
        {
            get
            {
                return this.tcpAddress;
            }

            set
            {
                this.tcpAddress = value;
                this.CreateTcp(value.Host, value.Port);
            }
        }

        public void ForceHttp()
        {
            this.Client = this.httpClient;
            this.usingTcp = false;
            this.OnClientChanged?.Invoke(new ClientChangedEventArgs() { Client = this.Client }, this);
        }

        private void CreateTcp(string address, int port)
        {
            Task.Run(async () =>
            {
                try
                {
                    var addr = (await Dns.GetHostAddressesAsync(address))[0];
                    lock (this.locker)
                    {
                        this.tcpClient.ServerAddress = addr;
                        this.tcpClient.ServerPort = port;
                        this.Client = this.tcpClient;

                        this.usingTcp = true;
                    }
                }
                catch
                {
                    lock (this.locker)
                    {
                        this.Client = this.httpClient;

                        System.Diagnostics.Debug.WriteLine("Error parsing");
                        this.usingTcp = false;
                    }
                }

                this.OnClientChanged?.Invoke(new ClientChangedEventArgs() { Client = this.Client }, this);
            });
        }
    }
}