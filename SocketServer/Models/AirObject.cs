using SocketServer.Models.Moveble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Models
{
    internal class AirObject(Point startPoint, byte speed)
    {
        private const byte _fighterSpeed = 10;
        private const byte _enemySpeed = 1;

        private readonly MovementStrategy _movement = new(startPoint, speed);
        public Guid Id { get; set; } = Guid.NewGuid();
        public Point Point => _movement.Point;
        public AirObject? Target { get; set; }

        public event EventHandler? CompleteMove;

        public static AirObject CreateFighter(Point startPoint)
        {
            return new AirObject(startPoint, _fighterSpeed);
        }

        public static AirObject CreateEnemy(Point target)
        {
            Random genPoint = new();
            double x = genPoint.Next(-5, 5);
            Point startPoint = new(x, 15);
            return new AirObject(startPoint, _enemySpeed)
            {
                Target = new AirObject(target, 0)
            };
        }

        public void Move()
        {
            if (Target is null) return;
            if (_movement.Move(Target.Point)) CompleteMove?.Invoke(this, new());
        }
    }
}
