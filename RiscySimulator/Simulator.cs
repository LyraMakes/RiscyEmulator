using RiscyCore;
using RiscyLogging;

namespace RiscySimulator;

public class Simulator
{
	public Simulator()
	{
		_regs = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		_PC = 0x00;
		_SP = 0x00;

		_Page = 0x00;
		_DataPage = 0x01;
		_StackPage = 0x00;

		_isHalted = false;

		_logger = new NoneLogger();

	}

    private readonly byte[,] _memory = new byte[256, 256];

    private readonly byte[] _regs;
    
    private byte _PC;

    private byte _SP;

    private byte _Page;
    private byte _StackPage;
    private byte _DataPage;
    

    private bool _isHalted;


    private ILogger _logger;
    
    public void ExecuteWriteMacro(byte[] macro)
    {
	    if (macro.Length % 3 != 0)
		    throw new InvalidMacroException(
			    $"Macro array is not a valid length ({macro.Length}).Must be a multiple of 3");

	    if (macro.Length == 0) return;

	    for (int i = 0; i < macro.Length - 2; i += 3)
	    {
		    byte page = macro[i];
		    byte addr = macro[i + 1];
		    byte value = macro[i + 2];
		    
		    _logger.WriteLine($"Setting [{page:x2}, {addr:x2}] to {value:x2}");
		    _memory[page, addr] = value;
	    }
    }
    
    public void ExecuteReadMacro(byte[] macro)
    {
	    if (macro.Length % 2 != 0)
		    throw new InvalidMacroException(
			    $"Macro array is not a valid length ({macro.Length}).Must be a multiple of 2");

	    if (macro.Length == 0) return;

	    for (int i = 0; i < macro.Length - 1; i += 2)
	    {
		    byte page = macro[i];
		    byte addr = macro[i + 1];
		    byte value = _memory[page, addr];

		    string logMessage = $"Reading {value:x2} from [{page:x2}, {addr:x2}]";

		    Console.WriteLine(logMessage);
		    _logger.WriteLine(logMessage);
	    }
    }
    
    public void LoadProgramIntoMemory(IEnumerable<byte> prog)
    {
	    byte initialPage = _Page;
	    
	    int i = 0;
	    foreach (byte x in prog)
	    {
		    //try
		    //{
			    _memory[_Page, i] = x;
		    //}
		    //catch (IndexOutOfRangeException e)
		    //{
			    //Console.Error.WriteLine($"PAGE: {_Page}");
		    //}

		    i = (i + 1) & 0xFF;
		    if (i == 0) _Page++;
	    }

	    _Page = initialPage;
	    
	    DumpMemory();
    }

    public void Run()
    {
	    while (!_isHalted)
	    {
		    ProcessorCycle();
	    }

	    DumpMemory();
    }

    private void DumpMemory()
    {
	    _logger.WriteLine("Dumping mem page 0:");
	    WriteMemory(0);
	    _logger.WriteLine("Dumping mem page 1:");
	    WriteMemory(1);
	    _logger.WriteLine("Dumping mem page 2:");
	    WriteMemory(2);
	    
    }

    private void WriteMemory(int page)
    {
	    const byte cols = 0x10;
	    _logger.Write("XX | ");
	    for (int i = 0; i < cols; i++)
	    {
		    _logger.Write($"{i:x2} ");
	    }
	    
	    _logger.Write($"\n---+-{new string('-', 3 * cols)}");
	    
	    //_logger.Write("XX  0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F");
	    for (int i = 0; i < 256; i++)
	    {
		    if (i % cols == 0) _logger.Write($"\n{i / cols:x2} | ");
		    _logger.Write($"{_memory[page, i]:x2} ");
	    }
	    
	    _logger.WriteLine("\n");
    }

    public void SetLogger(ILogger logger) => _logger = logger;
    
    
    

    private void ProcessorCycle()
    {
	    // Fetch
	    byte instruction = _memory[_Page, _PC];

	    // Decode
	    byte rrd = (byte)((instruction >> 2) & 0x03);
	    byte rrs = (byte)(instruction & 0x03);
	    byte imm = (byte)(((instruction & 0x30) >> 2) | (instruction & 0x03));

	    // Execute
	    
	    
	    if (((instruction >> 6) & 0x03) == 0x03)
	    {
		    _regs[rrd] = (byte)((_regs[rrd] << 4) | imm);
		    _logger.LogInstruction(Instructions.Sli, rrd, rrs, imm, instruction);
	    }
	    
	    if (instruction == 0)
	    {
		    _logger.LogInstruction(Instructions.Nop, rrd, rrs, imm, instruction);
	    }
	    
	    byte highNybble = (byte)(instruction >> 4);
	    byte rsVal = (byte)(instruction & 0x03);
	    
	    switch (highNybble)
	    {
		    case 0:
		    {
			    switch (rsVal)
			    {
				    case 0: break;
				    case 1:
				    {
					    _isHalted = true;
					    _logger.LogInstruction(Instructions.Halt, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 2:
				    {
					    _DataPage = _regs[rrd];
					    _logger.LogInstruction(Instructions.Page, rrd, rrs, imm, instruction);
					    break;
				    }
			    }
			    break;
		    }
		    case 1:
		    {
			    _regs[rrd] = (byte)(_regs[rrd] - _regs[rrs]);
			    _logger.LogInstruction(Instructions.Sub, rrd, rrs, imm, instruction);
			    break;
		    }
		    case 2:
		    {
			    _regs[rrd] = _memory[_DataPage, _regs[rrs]];
			    _logger.LogInstruction(Instructions.Load, rrd, rrs, imm, instruction);
			    break;
		    }
		    case 3:
		    {
			    _regs[rrd] = _memory[_DataPage, _regs[rrs]];
			    _logger.LogInstruction(Instructions.Load, rrd, rrs, imm, instruction);
			    break;
		    }
		    case 4:
			    switch (rsVal)
			    {
				    case 0:
				    {
					    _PC += (_regs[rrd] == 0x0).ToByte();
					    _logger.LogInstruction(Instructions.Skipz, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 1:
				    {
					    _PC += (_regs[rrd] != 0x0).ToByte();
					    _logger.LogInstruction(Instructions.Skipnz, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 2:
				    {
					    _PC += (((_regs[rrd] >> 7) & 0x1) == 0x1).ToByte();
					    _logger.LogInstruction(Instructions.Skipl, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 3:
				    {
					    _PC += (((_regs[rrd] >> 7) & 0x1) == 0x0).ToByte();
					    _logger.LogInstruction(Instructions.Skipge, rrd, rrs, imm, instruction);
					    break;
				    }
			    }
			    break;
		    case 5:
		    {
			    byte temp = (byte)(_regs[rrs] - 1);
			    _regs[rrd] = (byte)(_PC + 0x1);
			    _PC = temp;
			    _logger.LogInstruction(Instructions.Jalr, rrd, rrs, imm, instruction);
			    break;
		    }
		    case 6:
		    {
			    _regs[rrd] = (byte)~(_regs[rrd] & _regs[rrs]);
			    _logger.LogInstruction(Instructions.Nand, rrd, rrs, imm, instruction);
			    break;
		    }
		    case 7:
		    {
			    _regs[rrd] = (byte)(_regs[rrd] + _regs[rrs]);
			    _logger.LogInstruction(Instructions.Add, rrd, rrs, imm, instruction);
			    break;
		    }
		    case 8:
			    switch (rsVal)
			    {
				    case 0:
				    {
					    _regs[rrd]++;
					    _logger.LogInstruction(Instructions.Inc, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 1:
				    {
					    _regs[rrd]--;
					    _logger.LogInstruction(Instructions.Dec, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 2:
				    {
					    Console.WriteLine(_regs[rrd].GetAsciiChar());
					    _logger.LogInstruction(Instructions.Out, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 3:
				    {
					    _regs[rrd] = byte.Parse(Console.ReadLine() ?? string.Empty);
					    _logger.LogInstruction(Instructions.In, rrd, rrs, imm, instruction);
					    break;
				    }
			    }
			    break;
		    case 9:
		    {
			    byte temp = (byte)(_regs[rrd] - 1);
			    _regs[rrs] = (byte)(_PC + 1);
			    _Page = _regs[(int)REGISTER.C];
			    _PC = temp;
			    _logger.LogInstruction(Instructions.Jmpfar, rrd, rrs, imm, instruction);
			    break;
		    }
		    case 10:
			    switch (rsVal)
			    {
				    case 0:
				    {
					    _SP--;
					    _memory[_StackPage, _SP] = _regs[rrd];
					    _logger.LogInstruction(Instructions.Push, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 1:
				    {
					    _regs[rrd] = _memory[_StackPage, _SP];
					    _SP++;
					    _logger.LogInstruction(Instructions.Pop, rrd, rrs, imm, instruction);
					    break;
				    }
				    case 2:
				    {
					    _StackPage = _regs[rrd];
					    _logger.LogInstruction(Instructions.Stackpage, rrd, rrs, imm, instruction);
					    break;
				    }
			    }
			    break;
	    }
	    
	    
	    
	    
	    // WriteBack
	    _PC++;
	    if (_PC == 0) _Page++;
	    //if (_logger.GetType() != typeof(NoneLogger)) _logger.WriteLine($"Registers: A: {_regs[0]:x2} B: {_regs[1]:x2} C: {_regs[2]:x2} D: {_regs[3]:x2} PC: {_PC:x2} SP: {_SP:x2}");
	    LogValues();
	    
	    _logger.WriteLine();
    }

    private void LogValues()
    {
	    _logger.WriteLine($"Regs: A: {_regs[0]:x2} B: {_regs[1]:x2} C: {_regs[2]:x2} D: {_regs[3]:x2}");
	    _logger.WriteLine($"     PC: {_PC:x2} SP: {_SP:x2} Page: {_Page:x2}" );
    }
}