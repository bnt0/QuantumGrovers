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
                int numZeros = 0;
                int numTotal = 512;
                for (int i = 0; i < numTotal; i++)
                {
                    var res = GroverSearch.Run(sim).Result;
                    if (res == 0)
                    {
                        numZeros++;
                    }
                    //System.Console.WriteLine($"{res} ");
                }
                System.Console.WriteLine();
                System.Console.WriteLine($"Result of measurements: {numZeros} / {numTotal}");
            }
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }
    }
}