using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Microsoft.Extensions.Logging;
using SocketServer.Models;
using System.Text.Json;

namespace SocketServer.Servers;

internal class Server
{
    private const string _categoryName = "Server";
    private const int _port = 8888;
    private const int _backlog = 10_000;

    private readonly ILogger _logger;
    private readonly Socket _socket;
    private readonly GameManager _game;
    private readonly CancellationTokenSource _cts;

    private bool _isStart = false;
    private readonly List<Socket> _clients = new();

    public Server(int port)
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = factory.CreateLogger(_categoryName);

        IPEndPoint ipPoint = new(IPAddress.Any, port);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) 
        {
            SendTimeout = 2 * 1000,
            ReceiveTimeout = 2 * 1000,
        };


        _socket.Bind(ipPoint);

        _game = new GameManager();
        _game.ChangeEnemy += NewEnemyEvent;
        _game.GameOver += LoseEvent;

        this._cts = new CancellationTokenSource();
    }
    public Server() : this(_port) { }

    private void WriteStringData(string stringData)
    {
        var data = Encoding.UTF8.GetBytes(stringData);
        foreach (var client in _clients)
        {
            int byteCount = client.Send(data);
            if (byteCount >= 0)
            {
                _logger.LogInformation("Data has been sent");
            }
        }
    }
    private void NewEnemyEvent(object? sendere, ChangeEnemyArgs e)
    {
        string json = JsonSerializer.Serialize(e.Enemy);
        this.WriteStringData(json);

        switch (e.Status)
        {
            case EnemyStatus.New:
                _logger.LogInformation("New enemy {Description}, {Count}", e.Enemy.Select(en => en.Id), e.Enemy.Count);
                break;
            case EnemyStatus.Delete:
                _logger.LogInformation("Remove enemy {Description}, {Count}", e.Enemy.Select(en => en.Id), e.Enemy.Count);
                break;
        }
    }
    private void LoseEvent(object? sender, EventArgs e)
    {
        this.WriteStringData("You loser");
        _logger.LogInformation($"Lose game");
    }

    private async Task ListenClient(Socket client)
    {
        while(true)
        {
            var responseData = new byte[client.ReceiveBufferSize];
            var bytes = await client.ReceiveAsync(responseData);
            string response = Encoding.UTF8.GetString(responseData, 0, bytes);

            Dto? result = JsonSerializer.Deserialize<Dto>(response);
            if (result is null)
            {
                _logger.LogError("Un current object {Description}", response);
                return;
            }

            _logger.LogInformation("Set trigger {id}", result.Id);
            _game.SetTarget(result.Id);
        }
    }

    public void Start()
    {
        Start(_backlog);
    }
    public void Start(int backlog)
    {
        _isStart = true;
        _socket.Listen(backlog);
        _logger.LogInformation("Start server {Description}", _socket.RemoteEndPoint);

        _game.Play();

        while (_isStart)
        {
            try
            {
                Socket listenerAccept = _socket.Accept();
                if (listenerAccept != null)
                {
                    _clients.Add(listenerAccept);
                    _logger.LogInformation("Added new client");
                    Task.Run(() => ListenClient(listenerAccept), _cts.Token);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical("Start method, {Dis}", e.Message);
            }
        }
    }
    public void Stop()
    {
        _isStart = false;
        _socket.Close();
        _cts.Cancel();
        _logger.LogInformation("Stop server");
    }
}
