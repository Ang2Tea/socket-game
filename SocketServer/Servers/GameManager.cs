using SocketServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
internal enum EnemyStatus
{
    New,
    Delete
}
internal class ChangeEnemyArgs(EnemyStatus status, List<AirObject> enemy)
{
    public EnemyStatus Status { get; set; } = status;
    public List<AirObject> Enemy { get; set; } = enemy;
}

namespace SocketServer.Servers
{
    internal class GameManager
    {
        private const int _airFleetCount = 10;
        private const int _maxGenerationTime = 10;
        private const byte _maxHearts = 30;
        private const byte _maxEnemiesCount = 3;

        private readonly Point _base = new();

        public List<AirObject> AirFleet { get; private set; }
        public List<AirObject> Enemies { get; private set; } = new();
        public byte Hearts { get; private set; } = _maxHearts;

        public event EventHandler<ChangeEnemyArgs>? ChangeEnemy;
        public event EventHandler? GameOver;

        public GameManager() : this(_airFleetCount) { }
        public GameManager(int airFleetCount)
        {
            AirFleet = new List<AirObject>(airFleetCount);
            for (int i = 0; i < airFleetCount; i++)
            {
                AirObject fighter = AirObject.CreateFighter(_base);
                fighter.CompleteMove += FighterCompleteMove;

                AirFleet.Add(fighter);
            }
        }

        private void EnemyCompleteMove(object? sender, EventArgs e)
        {
            if (sender is not AirObject) return;

           // Hearts -= 1;
            Enemies.Remove((AirObject)sender);
            ChangeEnemy?.Invoke(this, new ChangeEnemyArgs(EnemyStatus.Delete, Enemies));
        }
        private void FighterCompleteMove(object? sender, EventArgs e)
        {
            if (sender is not AirObject obj || obj.Target is null) return;

            Enemies.Remove(obj.Target);
            ChangeEnemy?.Invoke(this, new ChangeEnemyArgs(EnemyStatus.Delete, Enemies));

            obj.Target = null;
        }

        private async Task GenEnemies()
        {
            Random rnd = new();
            while(true)
            {                
                int second = rnd.Next(5, _maxGenerationTime);
                await Task.Delay(second * 1000);

                if (Enemies.Count >= _maxEnemiesCount) continue;

                AirObject newEnemy = AirObject.CreateEnemy(_base);
                newEnemy.CompleteMove += EnemyCompleteMove;

                Enemies.Add(newEnemy);
                ChangeEnemy?.Invoke(this, new ChangeEnemyArgs(EnemyStatus.New, Enemies));
            }
        }
        private async Task MoveObject()
        {
            while(true)
            {
                await Task.Delay(1000);

                Enemies.ForEach(e => e.Move());
                AirFleet.ForEach(e => e.Move());
            }
        }
        private async Task ChechHearts()
        {
            while(true)
            {
                if (Hearts > 0) continue;

                GameOver?.Invoke(this, new EventArgs());
                await Task.Delay(30 * 1000);
                Hearts = _maxHearts;
            }
        }

        public void SetTarget(Guid target)
        {
            AirObject enemy = Enemies.First(e => e.Id.Equals(target));
            AirObject? airFleet = AirFleet.FirstOrDefault(e => e.Target is null);
            if (airFleet is null) return;

            airFleet.Target = enemy;
        }

        public void Play()
        {
            Task.Run(this.GenEnemies);
            Task.Run(this.MoveObject);
            Task.Run(this.ChechHearts);
        }
    }
}
