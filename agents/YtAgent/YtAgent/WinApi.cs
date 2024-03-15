﻿using System.Runtime.InteropServices;

namespace YtAgent;

/// <summary>
/// https://stackoverflow.com/questions/35138778/sending-keys-to-a-directx-game
/// http://www.gamespp.com/directx/directInputKeyboardScanCodes.html
/// </summary>
public class WinApi
{
    public static async Task KeyboardPress(ScanCodes scanCode, int length = 50, int lag = 50, bool shift = false)
    {
        if (shift)
        {
            KeyboardSendInput(ScanCodes.LSHIFT, KeyEventF.None);
        }

        KeyboardSendInput(scanCode, KeyEventF.None);
        await Task.Delay(length);
        KeyboardSendInput(scanCode, KeyEventF.KeyUp);

        if (shift)
        {
            KeyboardSendInput(ScanCodes.LSHIFT, KeyEventF.KeyUp);
        }

        await Task.Delay(lag);
    }

    public static async Task KeyboardType(string text, int length = 50, int lag = 50)
    {
        foreach (var character in text.ToCharArray())
        {
            await KeyboardPress(CharToScanCode(character), length, lag, CharToShiftState(character));
        }
    }

    public static async Task MouseMove(double normalizedX, double normalizedY, int lag = 50)
    {
        MouseSendInput((int)(normalizedX * 65536), (int)(normalizedY * 65536), (uint)(MouseEventFlags.MOVE | MouseEventFlags.MOUSEEVENTF_VIRTUALDESK | MouseEventFlags.ABSOLUTE));
        await Task.Delay(lag);
    }

    public static (double x, double y) MouseGetNormalizedPosition()
    {
        var point = new POINT();
        GetCursorPos(ref point);

        var screenWidth = GetSystemMetrics(0); // SM_CXSCREEN
        var screenHeight = GetSystemMetrics(1); // SM_CXYCREEN

        return ((double)point.x / screenWidth, (double)point.y / screenHeight);
    }

    public static async Task MouseLeftClick(int lag = 50)
    {
        MouseSendInput(0, 0, (uint)(MouseEventFlags.LEFTDOWN | MouseEventFlags.LEFTUP));
        await Task.Delay(lag);
    }

    public static async Task MouseRightClick(int lag = 50)
    {
        MouseSendInput(0, 0, (uint)(MouseEventFlags.RIGHTDOWN | MouseEventFlags.RIGHTUP));
        await Task.Delay(lag);
    }

    private static void KeyboardSendInput(ScanCodes scanCode, KeyEventF additionalFlags)
    {
        uint flagtosend = (uint)(KeyEventF.Scancode | additionalFlags);

        Input[] inputs =
        {
            new()
            {
                type = (int) InputType.Keyboard,
                inputUnion = new InputUnion
                {
                    ki = new KeyboardInput
                    {
                        wVk = 0,
                        wScan = (ushort) scanCode,
                        dwFlags = flagtosend,
                        dwExtraInfo = GetMessageExtraInfo()
                    }
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
    }

    private static void MouseSendInput(int dx, int dy, uint dwFlags)
    {
        Input[] inputs =
        {
            new()
            {
                type = (int) InputType.Mouse,
                inputUnion = new InputUnion
                {
                    mi = new MouseInput
                    {
                        mouseData = 0,
                        time = 0,
                        dx = dx,
                        dy = dy,
                        dwFlags = dwFlags,
                    }
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
    }

    private static ScanCodes CharToScanCode(char character)
    {
        return character switch
        {
            '1' => ScanCodes.KEY_1,
            '2' => ScanCodes.KEY_2,
            '3' => ScanCodes.KEY_3,
            '4' => ScanCodes.KEY_4,
            '5' => ScanCodes.KEY_5,
            '6' => ScanCodes.KEY_6,
            '7' => ScanCodes.KEY_7,
            '8' => ScanCodes.KEY_8,
            '9' => ScanCodes.KEY_9,
            '0' => ScanCodes.KEY_0,
            '-' => ScanCodes.MINUS,
            '=' => ScanCodes.EQUALS,

            '!' => ScanCodes.KEY_1,
            '@' => ScanCodes.KEY_2,
            '#' => ScanCodes.KEY_3,
            '$' => ScanCodes.KEY_4,
            '%' => ScanCodes.KEY_5,
            '^' => ScanCodes.KEY_6,
            '&' => ScanCodes.KEY_7,
            '*' => ScanCodes.KEY_8,
            '(' => ScanCodes.KEY_9,
            ')' => ScanCodes.KEY_0,
            '_' => ScanCodes.MINUS,
            '+' => ScanCodes.EQUALS,

            'a' => ScanCodes.A,
            'b' => ScanCodes.B,
            'c' => ScanCodes.C,
            'd' => ScanCodes.D,
            'e' => ScanCodes.E,
            'f' => ScanCodes.F,
            'g' => ScanCodes.G,
            'h' => ScanCodes.H,
            'i' => ScanCodes.I,
            'j' => ScanCodes.J,
            'k' => ScanCodes.K,
            'l' => ScanCodes.L,
            'm' => ScanCodes.M,
            'n' => ScanCodes.N,
            'o' => ScanCodes.O,
            'p' => ScanCodes.P,
            'q' => ScanCodes.Q,
            'r' => ScanCodes.R,
            's' => ScanCodes.S,
            't' => ScanCodes.T,
            'u' => ScanCodes.U,
            'v' => ScanCodes.V,
            'w' => ScanCodes.W,
            'x' => ScanCodes.X,
            'y' => ScanCodes.Y,
            'z' => ScanCodes.Z,

            'A' => ScanCodes.A,
            'B' => ScanCodes.B,
            'C' => ScanCodes.C,
            'D' => ScanCodes.D,
            'E' => ScanCodes.E,
            'F' => ScanCodes.F,
            'G' => ScanCodes.G,
            'H' => ScanCodes.H,
            'I' => ScanCodes.I,
            'J' => ScanCodes.J,
            'K' => ScanCodes.K,
            'L' => ScanCodes.L,
            'M' => ScanCodes.M,
            'N' => ScanCodes.N,
            'O' => ScanCodes.O,
            'P' => ScanCodes.P,
            'Q' => ScanCodes.Q,
            'R' => ScanCodes.R,
            'S' => ScanCodes.S,
            'T' => ScanCodes.T,
            'U' => ScanCodes.U,
            'V' => ScanCodes.V,
            'W' => ScanCodes.W,
            'X' => ScanCodes.X,
            'Y' => ScanCodes.Y,
            'Z' => ScanCodes.Z,

            '[' => ScanCodes.LBRACKET,
            ']' => ScanCodes.RBRACKET,
            ';' => ScanCodes.SEMICOLON,
            '\'' => ScanCodes.APOSTROPHE,
            '\\' => ScanCodes.BACKSLASH,
            ',' => ScanCodes.COMMA,
            '.' => ScanCodes.PERIOD,
            '/' => ScanCodes.SLASH,

            '{' => ScanCodes.LBRACKET,
            '}' => ScanCodes.RBRACKET,
            ':' => ScanCodes.SEMICOLON,
            '"' => ScanCodes.APOSTROPHE,
            '|' => ScanCodes.BACKSLASH,
            '<' => ScanCodes.COMMA,
            '>' => ScanCodes.PERIOD,
            '?' => ScanCodes.SLASH,

            ' ' => ScanCodes.SPACE,
            '\n' => ScanCodes.RETURN,
            '\t' => ScanCodes.TAB,
            '`' => ScanCodes.GRAVE,
            '~' => ScanCodes.GRAVE,

            _ => throw new NotImplementedException(),
        };
    }

    private static bool CharToShiftState(char character)
    {
        return character switch
        {
            '!' => true,
            '@' => true,
            '#' => true,
            '$' => true,
            '%' => true,
            '^' => true,
            '&' => true,
            '*' => true,
            '(' => true,
            ')' => true,
            '_' => true,
            '+' => true,

            'A' => true,
            'B' => true,
            'C' => true,
            'D' => true,
            'E' => true,
            'F' => true,
            'G' => true,
            'H' => true,
            'I' => true,
            'J' => true,
            'K' => true,
            'L' => true,
            'M' => true,
            'N' => true,
            'O' => true,
            'P' => true,
            'Q' => true,
            'R' => true,
            'S' => true,
            'T' => true,
            'U' => true,
            'V' => true,
            'W' => true,
            'X' => true,
            'Y' => true,
            'Z' => true,

            '{' => true,
            '}' => true,
            ':' => true,
            '"' => true,
            '|' => true,
            '<' => true,
            '>' => true,
            '?' => true,

            '~' => true,

            _ => false,
        };
    }

    #region WinApiImports

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetMessageExtraInfo();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetCursorPos(ref POINT lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetSystemMetrics(int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Input
    {
        public int type;
        public InputUnion inputUnion;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public readonly uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public readonly uint uMsg;
        public readonly ushort wParamL;
        public readonly ushort wParamH;
    }

    [Flags]
    public enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [Flags]
    public enum KeyEventF
    {
        None = 0x0000,
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        Scancode = 0x0008,
    }

    [Flags]
    public enum MouseEventFlags : uint
    {
        LEFTDOWN = 0x00000002,
        LEFTUP = 0x00000004,
        MIDDLEDOWN = 0x00000020,
        MIDDLEUP = 0x00000040,
        MOVE = 0x00000001,
        ABSOLUTE = 0x00008000,
        RIGHTDOWN = 0x00000008,
        RIGHTUP = 0x00000010,
        WHEEL = 0x00000800,
        XDOWN = 0x00000080,
        XUP = 0x00000100,

        MOUSEEVENTF_VIRTUALDESK = 0x4000,
    }

    public enum MouseEventDataXButtons : uint
    {
        XBUTTON1 = 0x00000001,
        XBUTTON2 = 0x00000002
    }

    public enum ScanCodes
    {
        ESCAPE = 0x01,
        KEY_1 = 0x02,
        KEY_2 = 0x03,
        KEY_3 = 0x04,
        KEY_4 = 0x05,
        KEY_5 = 0x06,
        KEY_6 = 0x07,
        KEY_7 = 0x08,
        KEY_8 = 0x09,
        KEY_9 = 0x0A,
        KEY_0 = 0x0B,
        MINUS = 0x0C,
        EQUALS = 0x0D,
        BACK = 0x0E,
        TAB = 0x0F,
        Q = 0x10,
        W = 0x11,
        E = 0x12,
        R = 0x13,
        T = 0x14,
        Y = 0x15,
        U = 0x16,
        I = 0x17,
        O = 0x18,
        P = 0x19,
        LBRACKET = 0x1A,
        RBRACKET = 0x1B,
        RETURN = 0x1C,
        LCONTROL = 0x1D,
        A = 0x1E,
        S = 0x1F,
        D = 0x20,
        F = 0x21,
        G = 0x22,
        H = 0x23,
        J = 0x24,
        K = 0x25,
        L = 0x26,
        SEMICOLON = 0x27,
        APOSTROPHE = 0x28,
        GRAVE = 0x29,
        LSHIFT = 0x2A,
        BACKSLASH = 0x2B,
        Z = 0x2C,
        X = 0x2D,
        C = 0x2E,
        V = 0x2F,
        B = 0x30,
        N = 0x31,
        M = 0x32,
        COMMA = 0x33,
        PERIOD = 0x34,
        SLASH = 0x35,
        RSHIFT = 0x36,
        MULTIPLY = 0x37,
        LMENU = 0x38,
        SPACE = 0x39,
        CAPITAL = 0x3A,
        F1 = 0x3B,
        F2 = 0x3C,
        F3 = 0x3D,
        F4 = 0x3E,
        F5 = 0x3F,
        F6 = 0x40,
        F7 = 0x41,
        F8 = 0x42,
        F9 = 0x43,
        F10 = 0x44,
        NUMLOCK = 0x45,
        SCROLL = 0x46,
        NUMPAD7 = 0x47,
        NUMPAD8 = 0x48,
        NUMPAD9 = 0x49,
        SUBTRACT = 0x4A,
        NUMPAD4 = 0x4B,
        NUMPAD5 = 0x4C,
        NUMPAD6 = 0x4D,
        ADD = 0x4E,
        NUMPAD1 = 0x4F,
        NUMPAD2 = 0x50,
        NUMPAD3 = 0x51,
        NUMPAD0 = 0x52,
        DECIMAL = 0x53,
        F11 = 0x57,
        F12 = 0x58,
        F13 = 0x64,
        F14 = 0x65,
        F15 = 0x66,
        KANA = 0x70,
        CONVERT = 0x79,
        NOCONVERT = 0x7B,
        YEN = 0x7D,
        NUMPADEQUALS = 0x8D,
        CIRCUMFLEX = 0x90,
        AT = 0x91,
        COLON = 0x92,
        UNDERLINE = 0x93,
        KANJI = 0x94,
        STOP = 0x95,
        AX = 0x96,
        UNLABELED = 0x97,
        NUMPADENTER = 0x9C,
        RCONTROL = 0x9D,
        NUMPADCOMMA = 0xB3,
        DIVIDE = 0xB5,
        SYSRQ = 0xB7,
        RMENU = 0xB8,
        HOME = 0xC7,
        UP = 0xC8,
        PRIOR = 0xC9,
        LEFT = 0xCB,
        RIGHT = 0xCD,
        END = 0xCF,
        DOWN = 0xD0,
        NEXT = 0xD1,
        INSERT = 0xD2,
        DELETE = 0xD3,
        LWIN = 0xDB,
        RWIN = 0xDC,
        APPS = 0xDD,
        BACKSPACE = BACK,
        NUMPADSTAR = MULTIPLY,
        LALT = LMENU,
        CAPSLOCK = CAPITAL,
        NUMPADMINUS = SUBTRACT,
        NUMPADPLUS = ADD,
        NUMPADPERIOD = DECIMAL,
        NUMPADSLASH = DIVIDE,
        RALT = RMENU,
        UPARROW = UP,
        PGUP = PRIOR,
        LEFTARROW = LEFT,
        RIGHTARROW = RIGHT,
        DOWNARROW = DOWN,
        PGDN = NEXT,
        LEFTMOUSEBUTTON = 0x100,
        RIGHTMOUSEBUTTON = 0x101,
        MIDDLEWHEELBUTTON = 0x102,
        MOUSEBUTTON3 = 0x103,
        MOUSEBUTTON4 = 0x104,
        MOUSEBUTTON5 = 0x105,
        MOUSEBUTTON6 = 0x106,
        MOUSEBUTTON7 = 0x107,
        MOUSEWHEELUP = 0x108,
        MOUSEWHEELDOWN = 0x109,
    }

    #endregion
}
