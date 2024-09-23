using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer.Models
{
    internal struct Point(double x, double y)
    {
        public double X { get; set; } = x;
        public double Y { get; set; } = y;

        public Point() : this(0, 0) { }

        public readonly double GetDistance(Point point)
        {
            const int power = 2;
            double dX = point.X - this.X;
            double dY = point.Y - this.Y;

            return Math.Sqrt(Math.Pow(dX, power) + Math.Pow(dY, power));
        }
    }
}
