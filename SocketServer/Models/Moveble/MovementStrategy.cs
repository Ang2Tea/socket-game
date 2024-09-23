using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Models.Moveble
{
    internal class MovementStrategy(Point startPoint, byte speed)
    {
        // Скорость в условных еденицах
        private readonly byte _speed = speed;
        private Point _point = startPoint;

        public byte Speed => _speed;
        public Point Point => _point;

        public MovementStrategy(Point startPoint) : this(startPoint, 1) { }

        public bool Move(Point target)
        {
            double distance = _point.GetDistance(target);
            double uX = (target.X - _point.X) / distance;
            double uY = (target.Y - _point.Y) / distance;
            Point newPoint = new(_point.X + uX * _speed, _point.Y + uY * _speed);
            double distanceForNewPoint = _point.GetDistance(newPoint);

            bool flightCheck = distance - distanceForNewPoint <= 0;

            _point = flightCheck ? target : newPoint;
            return flightCheck;
        }
    }
}
