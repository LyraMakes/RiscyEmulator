using RiscyCore;
using RiscyLogging;

namespace RiscyEmulator;

#pragma warning disable CA1822
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming
public class Simulator
{
	public Simulator()
	{
		_A = 0x00;
		_B = 0x00;
		_C = 0x00;
		_D = 0x00;

		_PC = 0x00;
		_SP = 0x00;

		_Page = 0x00;
		_DataPage = 0x01;
		_StackPage = 0x00;

		_isHalted = false;

		_logger = new NoneLogger();

	}

    private readonly byte[,] _memory = new byte[256, 256];
    
    private byte _A;
    
    private byte _B;
    private byte _C;
    private byte _D;

    private byte _PC;

    private byte _SP;

    private byte _Page;
    private byte _StackPage;
    private byte _DataPage;
    

    private bool _isHalted;


    private ILogger _logger;
    
    #region Instructions

    private void _JALR(ref byte rd, ref byte rs)
    {
	    rd = (byte)(_PC + 1);
	    _PC = (byte)(rs - 1);
    }

    private void _JMPFAR(ref byte rd, ref byte rs)
    {
	    rs = (byte)(_PC + 1);
	    _Page = _C;
	    _PC = (byte)(rd - 1);
    }

    private void _PAGE(ref byte rd) => _DataPage = rd;
    private void _STACKPAGE(ref byte rd) => _StackPage = rd;
    
    private void _NOP() {}
    private void _HALT() => _isHalted = true;

    private void _OUT(ref byte rd) => Console.Write(rd.GetAsciiChar());
    private void _IN(ref byte rd) => rd = byte.Parse(Console.ReadLine() ?? string.Empty);
    
    
    private void _LOAD(ref byte rd, ref byte rs) => rd = _memory[_DataPage, rs];
    private void _STORE(ref byte rd, ref byte rs) => _memory[_DataPage, rs] = rd;
    private void _PUSH(ref byte rd)
    {
	    _SP--;
	    _memory[_StackPage, _SP] = rd;
    }

    private void _POP(ref byte rd)
    {
	    rd = _memory[_StackPage, _SP];
	    _SP++;
    }

    private void _ADD(ref byte rd, ref byte rs) => rd = (byte)(rd + rs);
    private void _SUB(ref byte rd, ref byte rs) => rd = (byte)(rd - rs);
    private void _INC(ref byte rd) => (rd)++;
    private void _DEC(ref byte rd) => (rd)--;
    private void _NAND(ref byte rd, ref byte rs) => rd = (byte)(rd & rs);
    private void _SLI(ref byte rd, byte imm) => rd = (byte)((rd << 4) | (imm & 0x0F));


    private void _SKIPNZ(ref byte rd) => _PC += (rd != 0x0).ToByte();
    private void _SKIPZ(ref byte rd) => _PC += (rd == 0x0).ToByte();

    private void _SKIPL(ref byte rd) => _PC += (((rd >> 7) & 0x1) == 0x1).ToByte();
    private void _SKIPGE(ref byte rd) => _PC += (((rd >> 7) & 0x1) == 0x0).ToByte();
    
    
    #endregion

    
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
		    _memory[_Page, i] = x;
		    i = (i + 1) & 0xFF;
	    }

	    _Page = initialPage;
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
	    Instructions inst = DecodeInstruction(instruction);

	    byte rrd = (byte)((instruction >> 2) & 0x03);
	    byte rrs = (byte)(instruction & 0x03);
	    byte imm = (byte)(((instruction & 0x30) >> 2) | (instruction & 0x03));

	    
	    // Execute
	    // Execute Instruction based on rrd and rrs to get the refs for _A, _B, _C, and _D
	    switch (rrd)
	    {
		    case 0:
		    {
			    switch (rrs)
			    {
				    case 0: ExecuteInstruction(inst, ref _A, ref _A, imm); break;
				    case 1: ExecuteInstruction(inst, ref _A, ref _B, imm); break;
				    case 2: ExecuteInstruction(inst, ref _A, ref _C, imm); break;
				    case 3: ExecuteInstruction(inst, ref _A, ref _D, imm); break;
			    }
			    break;
		    }
		    case 1:
		    {
			    switch (rrs)
			    {
				    case 0: ExecuteInstruction(inst, ref _B, ref _A, imm); break;
				    case 1: ExecuteInstruction(inst, ref _B, ref _B, imm); break;
				    case 2: ExecuteInstruction(inst, ref _B, ref _C, imm); break;
				    case 3: ExecuteInstruction(inst, ref _B, ref _D, imm); break;
			    }
			    break;
		    }
		    case 2:
		    {
			    switch (rrs)
			    {
				    case 0: ExecuteInstruction(inst, ref _C, ref _A, imm); break;
				    case 1: ExecuteInstruction(inst, ref _C, ref _B, imm); break;
				    case 2: ExecuteInstruction(inst, ref _C, ref _C, imm); break;
				    case 3: ExecuteInstruction(inst, ref _C, ref _D, imm); break;
			    }
			    break;
		    }
		    case 3:
		    {
			    switch (rrs)
			    {
				    case 0: ExecuteInstruction(inst, ref _D, ref _A, imm); break;
				    case 1: ExecuteInstruction(inst, ref _D, ref _B, imm); break;
				    case 2: ExecuteInstruction(inst, ref _D, ref _C, imm); break;
				    case 3: ExecuteInstruction(inst, ref _D, ref _D, imm); break;
			    }
			    break;
		    }
	    }
	    
	    // WriteBack
	    _PC++;

	    _logger.LogInstruction(inst, rrd, rrs, imm, instruction);
	    
	    _logger.WriteLine($"Executed instruction: {inst.ToString().ToUpper()} with RD: {rrd:x2} RS: {rrs:x2} imm: {imm:x2}");
	    _logger.WriteLine($"Registers: A: {_A:x2} B: {_B:x2} C: {_C:x2} D: {_D:x2} PC: {_PC:x2} SP: {_SP:x2}");
	    _logger.WriteLine();
    }
    
    private Instructions DecodeInstruction(byte instruction)
    {
	    if (((instruction >> 6) & 0x03) == 0x03) return Instructions.Sli;
	    if (instruction == 0) return Instructions.Nop;
	    
	    byte highNybble = (byte)(instruction >> 4);
	    byte rsVal = (byte)(instruction & 0x03);
	    
	    switch (highNybble)
	    {
		    case 0: return instruction == 1 ? Instructions.Halt : Instructions.Page;
		    case 1: return Instructions.Sub;
		    case 2: return Instructions.Load;
		    case 3: return Instructions.Store;
		    case 4:
			    switch (rsVal)
			    {
				    case 0: return Instructions.Skipz;
				    case 1: return Instructions.Skipnz;
				    case 2: return Instructions.Skipl;
				    case 3: return Instructions.Skipge;
			    }
			    break;
		    case 5: return Instructions.Jalr;
		    case 6: return Instructions.Nand;
		    case 7: return Instructions.Add;
		    case 8:
			    switch (rsVal)
			    {
				    case 0: return Instructions.Inc;
				    case 1: return Instructions.Dec;
				    case 2: return Instructions.Out;
				    case 3: return Instructions.In;
			    }
			    break;
		    case 9: return Instructions.Jmpfar;
		    case 10:
			    switch (rsVal)
			    {
				    case 0: return Instructions.Push;
				    case 1: return Instructions.Pop;
				    case 2: return Instructions.Stackpage;
			    }
			    break;
	    }
	    
	    throw new InvalidOpcodeException(instruction);
    }

    private void ExecuteInstruction(Instructions instruction, ref byte rd, ref byte rs, byte imm)
    {
	    switch (instruction)
	    {
		    case Instructions.Jalr:
			    _JALR(ref rd, ref rs);
			    break;
		    case Instructions.Nop:
			    _NOP();
			    break;
		    case Instructions.Halt:
			    _HALT();
			    break;
		    case Instructions.Out:
			    _OUT(ref rd);
			    break;
		    case Instructions.In:
			    _IN(ref rd);
			    break;
		    case Instructions.Load:
			    _LOAD(ref rd, ref rs);
			    break;
		    case Instructions.Store:
			    _STORE(ref rs, ref rs);
			    break;
		    case Instructions.Push:
			    _PUSH(ref rd);
			    break;
		    case Instructions.Pop:
			    _POP(ref rd);
			    break;
		    case Instructions.Add:
			    _ADD(ref rd, ref rs);
			    break;
		    case Instructions.Sub:
			    _SUB(ref rd, ref rs);
			    break;
		    case Instructions.Inc:
			    _INC(ref rd);
			    break;
		    case Instructions.Dec:
			    _DEC(ref rd);
			    break;
		    case Instructions.Nand:
			    _NAND(ref rd, ref rs);
			    break;
		    case Instructions.Sli:
			    _SLI(ref rd, imm);
			    break;
		    case Instructions.Skipnz:
			    _SKIPNZ(ref rd);
			    break;
		    case Instructions.Skipz:
			    _SKIPZ(ref rd);
			    break;
		    case Instructions.Skipl:
			    _SKIPL(ref rd);
			    break;
		    case Instructions.Skipge:
			    _SKIPGE(ref rd);
			    break;
		    case Instructions.Page:
			    _PAGE(ref rd);
			    break;
		    case Instructions.Jmpfar:
			    _JMPFAR(ref rd, ref rs);
			    break;
		    case Instructions.Stackpage:
			    _STACKPAGE(ref rd);
			    break;
		    default:
			    throw new ArgumentOutOfRangeException(nameof(instruction), instruction, null);
	    }
    }
}

// ReSharper restore InconsistentNaming
// ReSharper restore RedundantAssignment
// ReSharper restore MemberCanBeMadeStatic.Local
#pragma warning restore CA1822