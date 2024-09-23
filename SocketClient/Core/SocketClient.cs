using SocketClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SocketClient.Core
{
    internal class SocketClient
    {
        private readonly Socket _socket;

        public EventHandler<List<Enemy>>? OnResponse;
        public SocketClient(string host, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(host, port);

            Task.Run(ListenServer);
        }

        private async Task ListenServer()
        {
            while (true)
            {
                var responseData = new byte[1024];
                var byteCount = await _socket.ReceiveAsync(responseData);
                string response = Encoding.UTF8.GetString(responseData, 0, byteCount);

                List<Enemy>? result = JsonSerializer.Deserialize<List<Enemy>>(response);
                if (result is null) continue;

                OnResponse?.Invoke(this, result);
            }
        }
        public void Send(Enemy enemy)
        {
            string json = JsonSerializer.Serialize(enemy);

            var data = Encoding.UTF8.GetBytes(json);
            _socket.Send(data);
        }
        public bool IsSocketStillConnected()
        {
            bool connected = true;
            bool blockingState = _socket.Blocking;
            try
            {
                byte[] tmp = new byte[1];
                _socket.Blocking = false;
                _socket.Send(tmp, 0, 0);
            }
            catch (SocketException)
            {
                connected = false;
            }
            finally
            {
                _socket.Blocking = blockingState;
            }
            
            return connected;
        }
    }
}
