namespace Quantum.Grovers
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;
	open Microsoft.Quantum.Extensions.Math;

	operation GroverSearch () : (Int)
	{
		body
		{
			let NUM_INPUT_QUBITS = 3;
			let NUM_ITERATIONS = 1;
			mutable res = new Result[NUM_INPUT_QUBITS];

			using (register = Qubit[NUM_INPUT_QUBITS + NUM_ITERATIONS])
			{
				let inputQubits = register[0..NUM_INPUT_QUBITS - 1];
				let ancillas    = register[NUM_INPUT_QUBITS..Length(register) - 1];

				ApplyToEach(H, inputQubits);

				for (i in 0 .. NUM_ITERATIONS - 1) {
					let ancilla = ancillas[i];
					X(ancilla); // Set ancilla to |1>
					H(ancilla); // Apply Hadamard to ancilla
					// Run oracle gate with inputQubits and ancilla
					Oracle(inputQubits + [ancilla]);
					InversionAboutMean(inputQubits);
				}

				set res = MultiM(inputQubits);

				ResetAll(register);
			}

			return ResultAsInt(res);
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
			//ApplyToEach(X, qs);
			RAll1(PI(), qs);		// Phase shift on the |11..1> state
			//ApplyToEach(X, qs);
			ApplyToEach(H, qs);
		}
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

			return true;
		}
	}
}
