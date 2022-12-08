using RiscyCore;

namespace RiscyLogging;

public class LogHelper
{
    public static string InstructionToString(Instructions inst, byte rrd, byte rrs, byte imm)
    {
        string rd = rrd switch
        {
            0 => "A",
            1 => "B",
            2 => "C",
            3 => "D",
            _ => "X"
        };
        string rs = rrs switch
        {
            0 => "A",
            1 => "B",
            2 => "C",
            3 => "D",
            _ => "X"
        };

        string instruction = inst switch
        {
            Instructions.Jalr => $"JALR {rd}, [{rs}]",
            Instructions.Nop => "NOP",
            Instructions.Halt => "HALT",
            Instructions.Out => $"OUT {rd}",
            Instructions.In => $"IN {rd}",
            Instructions.Load => $"LOAD {rd} [{rs}]",
            Instructions.Store => $"STORE {rd} [{rs}]",
            Instructions.Push => $"PUSH {rd}",
            Instructions.Pop => $"POP {rd}",
            Instructions.Add => $"ADD {rd},{rs}",
            Instructions.Sub => $"SUB {rd},{rs}",
            Instructions.Inc => $"INC {rd}",
            Instructions.Dec => $"DEC {rd}",
            Instructions.Nand => $"NAND {rd} {rs}",
            Instructions.Sli => $"SLI {rd}, {imm}",
            Instructions.Skipnz => $"SKIPNZ {rd}",
            Instructions.Skipz => $"SKIPZ {rd}",
            Instructions.Skipl => $"SKIPL {rd}",
            Instructions.Skipge => $"SKIPGE {rd}",
            Instructions.Page => $"PAGE {rd}",
            Instructions.Jmpfar => $"JMPFAR {rd}, [{rs}]",
            Instructions.Stackpage => $"STACKPAGE {rd}",
            _ => $""
        };

        return instruction;
    }
}