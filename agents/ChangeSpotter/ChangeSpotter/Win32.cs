using System.Drawing;
using System.Runtime.InteropServices;

namespace ChangeSpotter;

sealed class Win32
{
    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

    [DllImport("user32.dll")]
    static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("User32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
    public static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }

    public static Color GetPixelColor(int x, int y)
    {
        IntPtr hdc = GetDC(IntPtr.Zero);
        uint pixel = GetPixel(hdc, x, y);
        ReleaseDC(IntPtr.Zero, hdc);
        Color color = Color.FromArgb((int)(pixel & 0x000000FF),
            (int)(pixel & 0x0000FF00) >> 8,
            (int)(pixel & 0x00FF0000) >> 16);

        return color;
    }

    public static (int x, int y) GetCursorPosition()
    {
        POINT lpPoint;
        GetCursorPos(out lpPoint);
        //ScreenToClient(GetDC(IntPtr.Zero), ref lpPoint);

        return (lpPoint.X, lpPoint.Y);
    }

    public static (int x, int y) GetScreenSize()
    {
        return (GetSystemMetrics(0), GetSystemMetrics(1));
    }
}
