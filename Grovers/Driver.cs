using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System.Collections.Generic;
using System.Linq;

namespace Quantum.Grovers
{
    public class Driver
    {
        public static void Main(string[] args)
        {
            //RunTests();
            //RunGrovers();
            RunThreeQubitSingleIterGrovers();
            //RunThreeQubitMultipleIters();
            //RunWithIncreasingNumberOfQubits();
            //TimeIncreasingNumberOfQubits();

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

        public static void RunThreeQubitSingleIterGrovers()
        {
            var sim = new QuantumSimulator();
            int numSuccesses = 0;
            int numTotal = 1000;
            for (int i = 0; i < numTotal; i++)
            {
                var found = SingleIterGrover.Run(sim, 3).Result;
                if (found.TrueForAll(r => r == Result.Zero))
                {
                    numSuccesses++;
                    //System.Console.WriteLine(found);
                }
            }
            System.Console.WriteLine($"Result of measurements: {numSuccesses} / {numTotal}");
        }

        public static void RunThreeQubitMultipleIters()
        {
            // TODO change so it does what it says it should do
            var sim = new QuantumSimulator();
            int numSuccesses = 0;
            int numTotal = 1000;
            for (int i = 0; i < numTotal; i++)
            {
                var found = SingleIterGrover.Run(sim, 4).Result; // Number of total qubits is 4 because of ancilla qubit
                if (found.TrueForAll(r => r == Result.Zero))
                {
                    numSuccesses++;
                    //System.Console.WriteLine(found);
                }
            }
            System.Console.WriteLine($"Result of measurements: {numSuccesses} / {numTotal}");
        }

        public static void RunWithIncreasingNumberOfQubits()
        {
            System.Console.WriteLine($"Running single iterations of Grover's, with #1 number of qubits used for input (i.e. excluding ancilla); and #2 successful measurements out of 1000...");
            var sim = new QuantumSimulator();
            for (int numTotalQubits = 3; numTotalQubits <= 14; numTotalQubits++)
            {
                int numSuccesses = 0;
                for (int i = 0; i < 1000; i++)
                {
                    var found = SingleIterGrover.Run(sim, numTotalQubits - 1).Result;
                    if (found.TrueForAll(r => r == Result.Zero))
                    {
                        numSuccesses++;
                    }
                }
                // print number of qubits without ancilla, and number of sucessess out of 1000 runs
                System.Console.WriteLine($"{numTotalQubits - 1}; {numSuccesses}");
            }
        }

        public static void TimeIncreasingNumberOfQubits()
        {
            var sim = new QuantumSimulator();
            int numRuns = 100;
            System.Console.WriteLine($"Running single iterations of Grover's, with #1 number of total qubits, and #2 average number of miliseconds in one run over {numRuns}, standard deviation of times...");

            for (int numTotalQubits = 3; numTotalQubits <= 3; numTotalQubits++)
            {
                int numSuccesses = 0;
                var execTimes = new List<double>(numRuns);
                for (int i = 0; i < numRuns; i++)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var found = SingleIterGrover.Run(sim, numTotalQubits - 1).Result;
                    if (found.TrueForAll(r => r == Result.Zero))
                    {
                        numSuccesses++;
                    }
                    watch.Stop();
                    double elapsedMs = watch.ElapsedMilliseconds;
                    // Discard first outlier
                    if (!(elapsedMs > 100 && i == 0))
                    {
                        execTimes.Add(elapsedMs);
                    }
                }
                var avg = execTimes.Average();
                var stdev = System.Math.Sqrt(execTimes.Average(v => System.Math.Pow(v - avg, 2)));
                System.Console.WriteLine($"{numTotalQubits}, {avg}, {stdev}");
            }
        }
    }
}