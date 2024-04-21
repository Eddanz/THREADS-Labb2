using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THREADS_Labb2
{
    internal class Car
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Distance { get; set; }
        public decimal Speed { get; set; }
        public Stopwatch Timer { get; set; }
        public bool IsStopped { get; set; } = false;


        public Car(int id, string name, decimal speed)
        {
            Id = id;
            Name = name;
            Distance = 0;
            Timer = new Stopwatch();
            Speed = speed;
        }
    }
}
