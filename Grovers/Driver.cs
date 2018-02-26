using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.Grovers
{
    public class Driver
    {
        public static void Main(string[] args)
        {
            RunTests();
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
            
        }

        public static void RunTests()
        {
            // TODO move these to GroversTests
            var sim = new QuantumSimulator();
            var res1 = TestOracle.Run(sim).Result;
            var res2 = TestInversionAboutMean.Run(sim).Result;
            System.Console.WriteLine("Running tests finished!");
            return;
        }

        public static void RunGrovers()
        {
            using (var sim = new QuantumSimulator())
            {
                int numZeros = 0;
                int numTotal = 100;
                for (int i = 0; i < numTotal; i++)
                {
                    var (res, satinput) = GroverSearch.Run(sim).Result;
                    if (res == 0)
                    {
                        numZeros++;
                    }
                    System.Console.WriteLine($"{res} ");
                }
                System.Console.WriteLine();
                System.Console.WriteLine($"Result of measurements: {numZeros} / {numTotal}");
            }
        }
    }
}