using SocketClient.Core;
using SocketClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SocketClient
{
    internal class MainViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly Core.SocketClient _client;
        private ObservableCollection<Enemy> _enemies;
        private Enemy? _selectEnemy;

        public ObservableCollection<Enemy> Enemies
        {
            get => _enemies;
            set => Set(ref _enemies, value);
        }
        public Enemy? SelectedEnemy
        {
            get => _selectEnemy;
            set => Set(ref _selectEnemy, value);
        }
        public int Score
        {
            get => _enemies.Count;
        }

        public RelayCommand SendCommand { get; init; }

        public MainViewModel()
        {
            _enemies = [];
            using StreamReader reader = new("./config.json");
            string text = reader.ReadToEnd();

            Config config = JsonSerializer.Deserialize<Config>(text) 
                ?? new Config("localhost", 8888);

            _client = new Core.SocketClient(config.Host, config.Port);
            _client.OnResponse += ChangeEnemy;

            if (!_client.IsSocketStillConnected()) MessageBox.Show("Socket не подключен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

            SendCommand = new RelayCommand(SendAction, o => SelectedEnemy != null);
        }


        private void ChangeEnemy(object? sender, List<Enemy> e)
        {
            Enemies = new(e);
            SelectedEnemy = null;
        }
        private void SendAction(object? obj)
        {
            if (SelectedEnemy is null) return;
            _client.Send(SelectedEnemy);
        }
    }
}
