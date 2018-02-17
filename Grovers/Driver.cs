using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.Grovers
{
    public class Driver
    {
        public static void Main(string[] args)
        {
            using (var sim = new QuantumSimulator())
            {
                var res = GroverSearch.Run(sim).Result;
                System.Console.WriteLine($"Result of measurement: {res}");
            }
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}