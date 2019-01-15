using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal static class NativeMethods
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern UIntPtr StrFormatByteSize(
        [MarshalAs(UnmanagedType.I8)] long qdw,
        [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszBuf,
        [MarshalAs(UnmanagedType.U4)] uint cchBuf);

    [DllImport("uxtheme.dll")]
    private static extern int SetWindowTheme(
        IntPtr hwnd,
        [MarshalAs(UnmanagedType.LPWStr)] string pszSubAppName,
        [MarshalAs(UnmanagedType.LPWStr)] string pszSubIdList);

    public static string StrFormatByteSize(long qdw)
    {
        var sb = new StringBuilder();
        StrFormatByteSize(qdw, sb, (uint)sb.MaxCapacity);
        return sb.ToString();
    }

    public static int SetWindowTheme(Control control, string pszSubAppName, string pszSubIdList)
    {
        return SetWindowTheme(control.Handle, pszSubAppName, pszSubIdList);
    }
}
