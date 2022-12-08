using RiscyCore;

namespace RiscyLogging;

public class NoneLogger : ILogger
{
    public void Write(string content) { }
    public void WriteLine(string content) { }
    public void WriteLine() { }
    public void LogInstruction(Instructions inst, byte rrd, byte rrs, byte imm, byte instruction) { }
}