namespace Quantum.Grovers
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;
	open Microsoft.Quantum.Extensions.Math;

	// A single iteration of a numInputQubits qubit Grover search looking for the |00..0> state
	operation SingleIterGrover (numInputQubits : Int) : (Result[])
	{
		body
		{		
			return Grover(numInputQubits, 1);
		}
	}

	operation Grover (numInputQubits : Int, numIters : Int) : (Result[])
	{
		body
		{
			let numTotalQubits = numInputQubits + 1;
			mutable found = new Result[numInputQubits];

			using (qs = Qubit[numTotalQubits])
			{
				let inp = qs[0..numInputQubits - 1];
				let anc = qs[numTotalQubits - 1];

				ApplyToEach(H, inp);
				X(anc);
				H(anc);
				Oracle(qs);
				InversionAboutMean(inp);

				// What is the Prob in AssertProb ???
				set found = MultiM(inp);

				ResetAll(qs);
			}

			return (found);
		}
	}

	// Oracle to carry out phase inversion on qubits based on some indicator function
	// In this example, the only marked state will be the |00..0> state
    operation Oracle (qs : Qubit[]) : ()
    {
        body
        {
			ApplyToEach(X, qs);
			(Controlled X)(qs[0..Length(qs)-2], qs[Length(qs)-1]);
			ApplyToEach(X, qs);
        }
    }

	operation TestOracle () : (Bool)
	{
		body
		{
			using (qs = Qubit[4])
			{
				Message("Test started");
				// |0000> --> |0001>
				AssertQubit(Zero, qs[3]);
				Oracle (qs);
				AssertQubit(One, qs[3]);
				ResetAll(qs);

				// |0001> --> |0000>
				X(qs[3]);
				AssertQubit(One, qs[3]);
				Oracle(qs);
				AssertQubit(Zero, qs[3]);
				ResetAll(qs);

				// |1110> --> |1110>
				X(qs[0]);
				X(qs[1]);
				X(qs[2]);
				AssertQubit(Zero, qs[3]);
				Oracle (qs);
				AssertQubit(Zero, qs[3]);
				ResetAll(qs);
			}
			
			return true;
		}
	}

	operation InversionAboutMean (qs : Qubit[]) : ()
	{
		body
		{
			ApplyToEach(H, qs);
			RAll0(PI(), qs);		 // Negate the |00..0> state
			ApplyToEach(Negate, qs); // Negate all states, so now all states but the |00..0> state are negated
			ApplyToEach(H, qs);
		}
	}

	// Transforms any state to its negation, i.e. |x> ---> -|x>
	operation Negate (q : Qubit) : ()
	{
		body
		{
			Z(q);
			X(q);
			Z(q);
			X(q);
		}
		adjoint auto
		controlled auto
		adjoint controlled auto
	}

	operation TestInversionAboutMean () : (Bool)
	{
		body
		{
			let TOLERANCE = 1e-5;

			using (qs = Qubit[1])
			{
				// |0> ---> |1>
				AssertQubit(Zero, qs[0]);
				InversionAboutMean(qs);
				AssertQubit(One, qs[0]);
				ResetAll(qs);

				// 1/sqrt(2)*(|0> + |1>) ---> 1/sqrt(2)*(|0> + |1>)
				H(qs[0]);
				let prob = Complex(1.0/Sqrt(2.0), 0.0);
				AssertQubitState((prob, prob), qs[0], TOLERANCE);
				InversionAboutMean(qs);
				AssertQubitState((prob, prob), qs[0], TOLERANCE);
				ResetAll(qs);
			}

			// TODO figure out why compilation fails, if this is called qs
			// This should not be the same scope as the using block above. Maybe it's fixed in the February update.
			using (qs2 = Qubit[3])
			{
				// |000> ---> (-0.75 * |000>) + sum_{x=|001>}^{|111>} (0.25 * |x>)

				// TODO: change this to use AssertAllZero(qs2).
				// For some reason according to the docs, the type of this function is (Qubit[]) : ()
				// But the compiler fails with an error, claiming it's actually (String, Qubit[], Double) : ()
				AssertQubit(Zero, qs2[0]);
				AssertQubit(Zero, qs2[1]);
				AssertQubit(Zero, qs2[2]);

				// This does the same as above, it's here only to test if AsserProb works as I expect it to
				// It can be removed, if AssertAllZero is working above.
				AssertProb([PauliZ; PauliZ; PauliZ], qs2, Zero, 1.0, "Prob of |000> is not as expected", TOLERANCE);
				AssertProb([PauliZ; PauliZ; PauliZ], qs2, One, 0.0, "Prob of |001> is not as expected", TOLERANCE);

				InversionAboutMean(qs2);

				let probAllZero = Complex(-0.75, 0.0);
				let probElse    = Complex(0.25,  0.0);

				AssertProb([PauliZ; PauliZ; PauliZ], qs2, Zero, 0.75, "Prob of |000> is not as expected", TOLERANCE);
				AssertProb([PauliZ; PauliZ; PauliZ], qs2, One,  0.25, "Prob of |001> is not as expected", TOLERANCE);

				ResetAll(qs2);

				// |111>
				ApplyToEach(X, qs2);

				AssertQubit(One, qs2[0]);
				AssertQubit(One, qs2[1]);
				AssertQubit(One, qs2[2]);
				
				InversionAboutMean(qs2);

				AssertProb([PauliZ; PauliZ; PauliZ], qs2, Zero, 0.25, "Prob of |000> is not as expected", TOLERANCE);
				AssertProb([PauliZ; PauliZ; PauliZ], qs2, One,  0.75, "Prob of |001> is not as expected", TOLERANCE); // ???

				ResetAll(qs2);

				// |+++>
				ApplyToEach(H, qs2);

				InversionAboutMean(qs2);

				// TODO what does AssertProb actually do with multiple qubits?
				// Is this asserting the probability of getting a result when measuring only one qubit??
				AssertProb([PauliZ; PauliZ; PauliZ], qs2, Zero, 0.5, "Prob of |000> is not as expected", TOLERANCE);

				ResetAll(qs2);
			}

			return true;
		}

	}
	
	function AbsoluteSquare (c : Complex) : (Double)
	{
		let (real, imag) = c;
		return (real*real + imag*imag);
	}
}
