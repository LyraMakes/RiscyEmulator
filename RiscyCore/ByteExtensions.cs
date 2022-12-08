using System.Text;

namespace RiscyCore;

public static class ByteExtensions
{
    public static char GetAsciiChar(this byte b)
    {
        return Encoding.ASCII.GetChars(new byte[] { b })[0];
    }
}