using RiscyCore;

namespace RiscyLogging;

public class ConsoleAndFileLogger : ILogger
{
    private FileLogger _fileLogger;
    private ConsoleLogger _consoleLogger;

    public ConsoleAndFileLogger(string filename)
    {
        _consoleLogger = new ConsoleLogger();
        _fileLogger = new FileLogger(filename);
    }

    public void Write(string content)
    {
        _consoleLogger.Write(content);
        _fileLogger.Write(content);
    }

    public void WriteLine(string content)
    {
        _consoleLogger.WriteLine(content);
        _fileLogger.WriteLine(content);
    }

    public void WriteLine()
    {
        _consoleLogger.WriteLine();
        _fileLogger.WriteLine();
    }

    public void LogInstruction(Instructions inst, byte rrd, byte rrs, byte imm, byte instruction)
    {
        _consoleLogger.LogInstruction(inst, rrd, rrs, imm, instruction);
        _fileLogger.LogInstruction(inst, rrd, rrs, imm, instruction);
    }
}