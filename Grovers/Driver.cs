using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantum.Grovers
{
    public class Driver
    {
        public delegate void CmdOption();

        private static readonly Dictionary<string, CmdOption> optionsMap = new Dictionary<string, CmdOption>
            {
                {"--help", PrintHelp},
                {"--tests", RunTests},
                {"--time-single-iter", TimeSingleIterWithIncreasingNumberOfQubits},
                {"--results-single-iter", RunSingleIterWithIncreasingNumberOfQubits},
                {"--results-5-qubits-many-iters", RunIncreasingIters5Qubits },
                {"--results-many-qubits-optimal-iters", RunIncreasingQubitsOptimalIters },
                {"--time-many-qubits-optimal-iters", TimeIncreasingQubitsOptimalIters }
            };

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintHelp();
                WaitForKey();
                return;
            }

            if (args.Length == 1)
            {
                if (optionsMap.ContainsKey(args[0]))
                {
                    optionsMap[args[0]]();
                }
                else
                {
                    System.Console.WriteLine($"Invalid argument: {args[0]}");
                    PrintHelp();
                }
                WaitForKey();
                return;
            }

            if (args.Length == 3)
            {
                if (args[0] == "--run")
                {
                    int numInputQubits;
                    int numIterations;

                    try
                    {
                        numInputQubits = System.Int32.Parse(args[1]);
                        numIterations = System.Int32.Parse(args[2]);
                    }
                    catch (System.FormatException e)
                    {
                        System.Console.WriteLine("Arguments in unrecognized format. Integers expected.");
                        System.Console.WriteLine(e.Message);
                        PrintHelp();
                        WaitForKey();
                        return;
                    }
                    ExecuteGrovers(numInputQubits, numIterations);
                }
                else
                {
                    System.Console.WriteLine($"Invalid argument: {args[0]}");
                    PrintHelp();
                }
                WaitForKey();
                return;
            }

            System.Console.WriteLine("Unknown number arguments. Exiting.");
            PrintHelp();
            WaitForKey();
            return;

        }

        private static void WaitForKey()
        {
            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey();
        }

        private static void PrintHelp()
        {
            System.Console.WriteLine(
                "grovers. A Q# implementation of Grover's search algorithm.\n" +
                "Usage: grovers [OPTION]\n\n" +
                "--help                display this help\n" +
                "--tests               run oracle and inversion about mean tests\n\n" +
                "--run [numInputQubits] [numIters]\n" +
                "                      run Grover's with the specified number of input qubits for a the specified number of iterations 1000 times.\n" +
                "                      Note: the total number of qubits required to do this will be [numInputQubits]+[numIters]\n\n" +
                "--time-single-iter    Print timing stats in the process for an increasing number of qubits (1 iter)\n" +
                "--results-single-iter Print ratio of measurement successes for an increasing number of qubits (1 iter)\n" +
                "--results-5-qubits-many-iters" +
                "                      Run an increasing number of iterations with 5 input qubits. Print successes in the process." +
                "--results-many-qubits-optimal-iters" +
                "                      Run an optimal number of iterations for an increasing number of qubits. Print successes in the process." +
                "--time-many-qubits-optimal-iters" +
                "                      Print timing stats for an increasing number of qubits, with O(sqrt(N)) iterations."

                );
        }

        private static void RunIncreasingQubitsOptimalIters()
        {
            for (int i = 2; i < 12; i++)
            {
                int numIters = (int) Math.Pow(2, (i-1)/2.0);
                System.Console.Write($"{i}, {numIters}, ");
                ExecuteGrovers(i, numIters);
            }
        }

        private static void TimeIncreasingQubitsOptimalIters()
        {
            for (int i = 2; i < 12; i++)
            {
                int numIters = (int)Math.Pow(2, (i - 1) / 2.0);
                TimeGrovers(i, numIters);
            }
        }

        private static void RunTests()
        {
            var sim = new QuantumSimulator();
            var res1 = TestOracle.Run(sim).Result;
            var res2 = TestInversionAboutMean.Run(sim).Result;
            System.Console.WriteLine("Running tests finished successfully!");
            return;
        }

        private static void RunThreeQubitSingleIterGrovers()
        {
            var sim = new QuantumSimulator();
            int numSuccesses = 0;
            int numTotal = 1000;
            for (int i = 0; i < numTotal; i++)
            {
                var found = RunSingleIterGrovers.Run(sim, 3).Result;
                if (found.TrueForAll(r => r == Result.Zero))
                {
                    numSuccesses++;
                    //System.Console.WriteLine(found);
                }
            }
            System.Console.WriteLine($"Result of measurements: {numSuccesses} / {numTotal}");
        }


        private static void RunIncreasingIters5Qubits()
        {
            int maxIters = 10;
            for (int numIters = 0; numIters <= maxIters; numIters++)
            {
                System.Console.Write(numIters + ", ");
                ExecuteGrovers(5, numIters);
            }
        }

        /* Runs Grover's with the specified number of input qubits and number of iterations, 1000 times each
         * Prints the numer of successes.
         */
        private static void ExecuteGrovers(int numInputQubits, int numIters)
        {
            var sim = new QuantumSimulator();
            int numSuccesses = 0;
            int numTotal = 1000;
            for (int i = 0; i < numTotal; i++)
            {
                var found = RunGrovers.Run(sim, numInputQubits, numIters).Result; // Number of total qubits is 4 because of ancilla qubit
                if (found.TrueForAll(r => r == Result.Zero))
                {
                    numSuccesses++;
                }
            }
            System.Console.WriteLine($"{numSuccesses}");
        }

        /* Measures the time taken to execute Grover's with the specified number of input qubits and number of iterations, 100 times each
         * Prints the numer of qubits, average time taken for an exeuction, and the standard deviation of execution times.
         */
        private static void TimeGrovers(int numInputQubits, int numIters)
        {
            var sim = new QuantumSimulator();
            int numSuccesses = 0;
            int numRuns = 100;
            var execTimes = new List<double>(numRuns);

            for (int i = 0; i < numRuns; i++)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var found = RunGrovers.Run(sim, numInputQubits, numIters).Result; // Number of total qubits is 4 because of ancilla qubit
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
            System.Console.WriteLine($"{numInputQubits}, {avg}, {stdev}");
        }

        private static void RunSingleIterWithIncreasingNumberOfQubits()
        {
            int numRuns = 1000;
            System.Console.WriteLine($"# number of input qubits, successes out of {numRuns} runs");
            var sim = new QuantumSimulator();
            for (int numInpQubits = 2; numInpQubits <= 13; numInpQubits++)
            {
                int numSuccesses = 0;
                for (int i = 0; i < numRuns; i++)
                {
                    var found = RunSingleIterGrovers.Run(sim, numInpQubits).Result;
                    if (found.TrueForAll(r => r == Result.Zero))
                    {
                        numSuccesses++;
                    }
                }
                // print number of qubits without ancilla, and number of sucessess out of {numRuns} runs
                System.Console.WriteLine($"{numInpQubits}, {numSuccesses}");
            }
        }

        private static void TimeSingleIterWithIncreasingNumberOfQubits()
        {
            var sim = new QuantumSimulator();
            int numRuns = 100;
            System.Console.WriteLine($"# number of input qubits, average execution time in ms over {numRuns} runs, standard deviation over {numRuns} runs");

            for (int numInpQubits = 2; numInpQubits <= 13; numInpQubits++)
            {
                int numSuccesses = 0;
                var execTimes = new List<double>(numRuns);
                for (int i = 0; i < numRuns; i++)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    var found = RunSingleIterGrovers.Run(sim, numInpQubits).Result;
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
                System.Console.WriteLine($"{numInpQubits}, {avg}, {stdev}");
            }
        }
    }
}