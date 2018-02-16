namespace Quantum.Grovers
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;

	operation GroverSearch () : (Int)
	{
		body
		{
			let NUM_QUBITS = 4;
			mutable res = Zero;
			using (funcInputQubits = Qubit[NUM_QUBITS])
			{
				ResetAll(funcInputQubits); // Initialize all qubits to |0> (TODO not sure if needed)
				ApplyToEach(H, funcInputQubits);

				using (ancilla = Qubit[1])
				{
					X(ancilla[0]); // Set ancilla to |1>
					H(ancilla[0]); // Apply Hadamard to ancilla

					set res = M(ancilla[0]);
					ResetAll(ancilla);
				}
				ResetAll(funcInputQubits);
			}

			return ResultAsInt([res]);
		}
	}

    operation GroverDiffusionOperator () : ()
    {
        body
        {
            
        }
    }
}
