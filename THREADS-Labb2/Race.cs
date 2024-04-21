using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace THREADS_Labb2
{
    internal class Race
    {
        private static readonly object _lock = new object(); // Använder ett låsobjekt för att synkronisera trådtillgång till delade resurser
        private static Random random = new Random(); // Slumpgenerator för att hantera slumpmässiga händelser
        private static Car car1 = new Car(1, "Sapphire", 33.33m); // Två bilinstanser som tävlar
        private static Car car2 = new Car(2, "Ruby", 33.33m);
        private static volatile bool raceOver = false; // En flagga för att ange om tävlingen är över

        public static void StartRace()
        {
            // Skapar trådar för användarinput och för varje bil som ska tävla
            Thread inputThread = new Thread(HandleInput);
            Thread raceThread1 = new Thread(() => RaceSimulation(car1));
            Thread raceThread2 = new Thread(() => RaceSimulation(car2));
            Console.WriteLine("Race is starting");
            // Nedräkning före start
            for (int i = 3; i >= 1; i--)
            {
                Thread.Sleep(1000);
                Console.WriteLine(i);
            }
            Console.WriteLine("And they are off!");
            Console.WriteLine("Type 'status' to display the current distance of the cars or 'clear' to clear the console window.");

            // Startar alla trådar
            inputThread.Start();
            raceThread1.Start();
            raceThread2.Start();

            // Väntar på att tävlingstrådarna ska avslutas
            raceThread1.Join();
            raceThread2.Join();
            raceOver = true; // Markerar att tävlingen är över
            inputThread.Join(); // Väntar på att inputtråden ska avslutas

            AnnounceWinner();
        }

        // Returnerar ett trådsäkert slumpmässigt tal
        public static int SafeRandomNext(int min, int max)
        {
            lock (_lock)
            {
                return random.Next(min, max);
            }
        }

        // Hanterar användarinmatning under tävlingen
        static void HandleInput()
        {
            while (!raceOver)
            {
                if (Console.KeyAvailable)
                {
                    string input = Console.ReadLine();
                    if (input.Equals("status", StringComparison.OrdinalIgnoreCase))
                    {
                        DisplayStatus();
                    }
                    else if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.Clear();
                        Console.WriteLine("Type 'status' to display the current distance of the cars or 'clear' to clear the console window.");
                    }
                    else
                    {
                        Console.WriteLine("Commands are 'status' and 'clear'");
                    }
                }
                Thread.Sleep(100);
            }
        }

        // Visar aktuell status för bilarnas position och hastighet
        static void DisplayStatus()
        {
            lock (_lock)
            {
                string car1Status = car1.IsStopped ? "0.00 km/h (stopped)" : $"{car1.Speed * 3.6m:F2} km/h";
                string car2Status = car2.IsStopped ? "0.00 km/h (stopped)" : $"{car2.Speed * 3.6m:F2} km/h";
                Console.WriteLine($"Current distance -- {car1.Name} at {car1.Distance:F2} meters, going {car1Status}" +
                    $"\nCurrent distance -- {car2.Name} at {car2.Distance:F2} meters, going {car2Status}");
            }
        }

        // Tillkännager vinnaren av tävlingen
        static void AnnounceWinner()
        {
            TimeSpan car1Time = car1.Timer.Elapsed;
            TimeSpan car2Time = car2.Timer.Elapsed;

            Console.WriteLine($"{car1.Name} finished in {car1Time.TotalSeconds:F2} seconds.");
            Console.WriteLine($"{car2.Name} finished in {car2Time.TotalSeconds:F2} seconds.");

            if (car1Time < car2Time)
            {
                Console.WriteLine($"{car1.Name} has won!");
            }
            else if (car2Time < car1Time)
            {
                Console.WriteLine($"{car2.Name} has won!");
            }
            else
            {
                Console.WriteLine("The race ended in a tie!");
            }
            Console.ReadLine();
        }

        // Simulerar tävlingsprocessen för en bil
        public static void RaceSimulation(Car car)
        {
            car.Timer.Start();
            decimal initialSpeed = 33.33m;
            car.Speed = initialSpeed;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (!raceOver && car.Distance < 10000)
            {
                car.Distance += car.Speed;
                if (car.Distance >= 10000)
                {
                    break;
                }

                if (stopwatch.ElapsedMilliseconds >= 30000)
                {
                    int randomNumber = SafeRandomNext(1, 51);
                    HandleRandomEvent(car, ref initialSpeed, randomNumber);
                    stopwatch.Restart();
                }

                Thread.Sleep(1000);
            }

            car.Timer.Stop();
            lock (_lock)
            {
                Console.WriteLine($"{car.Name} has finished the race!");
            }
        }

        // Hanterar slumpmässiga händelser som påverkar bilens hastighet eller orsakar fördröjningar
        static void HandleRandomEvent(Car car, ref decimal speed, int randomNumber)
        {
            decimal oldSpeed = speed * 3.6m;

            if (randomNumber == 1)
            {
                Console.WriteLine($"{car.Name} has run out of fuel and needs to refuel, stopping for 30 seconds");
                car.IsStopped = true;
                Thread.Sleep(30000);
                car.IsStopped = false;
            }
            else if (randomNumber >= 2 && randomNumber <= 3)
            {
                Console.WriteLine($"{car.Name} has a flat tire and needs to change it, stopping for 20 seconds");
                car.IsStopped = true;
                Thread.Sleep(20000);
                car.IsStopped= false;
            }
            else if (randomNumber >= 4 && randomNumber <= 8)
            {
                Console.WriteLine($"{car.Name} has a bird on the windshield and needs to clean it, stopping for 10 seconds");
                car.IsStopped = true;
                Thread.Sleep(10000);
                car.IsStopped = false;
            }
            else if (randomNumber >= 9 && randomNumber <= 18)
            {
                decimal reduction = 0.278m * (1 + (decimal)random.NextDouble());
                speed -= reduction;
                decimal newSpeed = speed * 3.6m;
                decimal speedReduction = oldSpeed - newSpeed;
                Console.WriteLine($"{car.Name} has an engine issue and the speed is reduced by {speedReduction:F2} km/h to {newSpeed:F2} km/h");
            }
            car.Speed = speed;
        }
    }
}
