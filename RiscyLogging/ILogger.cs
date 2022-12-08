using RiscyCore;

namespace RiscyLogging;

public interface ILogger
{
    void Write(string content);
    void WriteLine(string content);
    void WriteLine();
    void LogInstruction(Instructions inst, byte rrd, byte rrs, byte imm, byte instruction);
}