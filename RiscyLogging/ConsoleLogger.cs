using RiscyCore;

namespace RiscyLogging;

public class ConsoleLogger : ILogger
{
    public void Write(string content) => Console.Write(content);

    public void WriteLine(string content) => Console.WriteLine(content);
    public void WriteLine() => Console.WriteLine();

    public void LogInstruction(Instructions inst, byte rrd, byte rrs, byte imm, byte instruction)
    {
        Console.WriteLine($"Executed: \"{LogHelper.InstructionToString(inst, rrd, rrs, imm)}\", Literal: {Convert.ToString(instruction, 2)}");
    }
}