namespace Quantum.Grovers
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;

	operation GroverSearch () : (Int)
	{
		body
		{
			let NUM_INPUT_QUBITS = 4;
			let NUM_ITERATIONS = 2;
			mutable res = [Zero];

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
					Message($"{i}");
				}

				set res = MultiM(inputQubits);

				ResetAll(register);
			}

			return ResultAsInt(res);
		}
	}

    operation Oracle (inputQubits : Qubit[]) : ()
    {
        body
        {
			// TODO implement
			let nq = Length(inputQubits);
			Message($"Num input qubits: {nq}");
        }
		adjoint auto	
		controlled auto
		controlled adjoint auto
    }

	operation InversionAboutMean (qs : Qubit[]) : ()
	{
		body
		{
			// TODO implement
		}
		adjoint auto
		controlled auto
		controlled adjoint auto
	}
}
