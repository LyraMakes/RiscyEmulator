namespace RiscyCore;

public class InvalidOpcodeException : Exception
{
    public InvalidOpcodeException(byte inst) : base($"{inst} Opcode is invalid Riscy Assembly") { }
}