

using RiscyCore;
using RiscyLogging;

namespace RiscyEmulator
{
    public class Program
    {
        // public static void Main(string[] args)
        // {
        //     byte value = ByteTools.Concatenate(0b0010, 0b11, 0b00);
        //     Console.WriteLine($"0x{Convert.ToString(value, 2).PadLeft(8, '0')}");
        // }
        
        public static void Main(string[] args)
        {
            ILogger logger = args.Contains("--debug") || args.Contains("-d") ? new ConsoleLogger() : new NoneLogger();
            
            Simulator simulator = new Simulator();
            simulator.ExecuteWriteMacro(new byte[] {0x01, 0xFE, 0x02});
            
            simulator.SetLogger(logger);
            // byte[] program = {0xff,0xfe,0x23,0x15,0xc8,0xcb,0xcd,0xcd,0x49,0x5f,0x74,0xcc,
            //     0xcd,0x1b,0xcc,0xde,0x5f,0xff,0xff,0x37,0x01};
            byte[] program = loadFile("mytinybasic.riscy.bin");
            
            simulator.LoadProgramIntoMemory(program);
            
            
            
            
            
            simulator.Run();
            
            simulator.ExecuteReadMacro(new byte[] {0x01, 0xFF});
            
        }

        private ILogger GetLogger(string[] args)
        {
            ILogger logger;
            
            
            if (HasLogfile(args))
            {
                int indxL = Array.IndexOf(args, "-l");
                int indxLf = Array.IndexOf(args, "--logfile");

                string logFileName = indxL != -1 ? args[indxL + 1] : args[indxLf + 1];
                logger = HasDebug(args) ? new ConsoleAndFileLogger(logFileName) : new FileLogger(logFileName);
            } else logger = HasDebug(args) ? new ConsoleLogger() : new NoneLogger();

            return logger;
        }

        private static byte[] loadFile(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        private bool HasDebug(string[] args) => args.Contains("--debug") || args.Contains("-d");
        private bool HasLogfile(string[] args) => args.Contains("--logfile") || args.Contains("-l");
    }
}