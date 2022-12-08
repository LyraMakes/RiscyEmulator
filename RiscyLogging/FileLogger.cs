using RiscyCore;

namespace RiscyLogging;

public class FileLogger : ILogger
{
    private string _logfileName;

    //private FileStream _logFile;

    public FileLogger(string fileName)
    {
        _logfileName = fileName;

        if (!File.Exists(fileName)) File.Create(fileName);
    }
    
    public void Write(string content)
    {
        throw new NotImplementedException();
    }

    public void WriteLine(string content)
    {
        throw new NotImplementedException();
    }

    public void WriteLine()
    {
        throw new NotImplementedException();
    }

    public void LogInstruction(Instructions inst, byte rrd, byte rrs, byte imm, byte instruction)
    {
        string instruc = LogHelper.InstructionToString(inst, rrd, rrs, imm);
        throw new NotImplementedException();
    }
}