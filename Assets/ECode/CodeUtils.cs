using System.IO;
using System.Collections.Generic;
using System.Text;

public static class CodeUtils
{
    public static StringBuilder NewLine(StringBuilder builder, int indent)
    {
        return ApendLine(builder).Append(' ', indent * 4);
    }

    public static StringBuilder ApendLine(StringBuilder builder)
    {
        return builder.Append("\r\n");
    }
}
