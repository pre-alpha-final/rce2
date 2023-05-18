using System.Runtime.InteropServices;

namespace YtAgent;

/// <summary>
/// https://stackoverflow.com/questions/35138778/sending-keys-to-a-directx-game
/// http://www.gamespp.com/directx/directInputKeyboardScanCodes.html
/// </summary>
public class Keyboard
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern IntPtr GetMessageExtraInfo();

    public static void PressKey(ScanCodes scanCode)
    {
        Input[] inputs =
        {
        new Input
        {
            type = (int) InputType.Keyboard,
            inputUnion = new InputUnion
            {
                ki = new KeyboardInput
                {
                    wVk = 0,
                    wScan = (ushort) scanCode,
                    dwFlags = (uint) (KeyEventF.Scancode | KeyEventF.KeyDown),
                    dwExtraInfo = GetMessageExtraInfo()
                }
            }
        },
        new Input
        {
            type = (int) InputType.Keyboard,
            inputUnion = new InputUnion
            {
                ki = new KeyboardInput
                {
                    wVk = 0,
                    wScan = (ushort) scanCode,
                    dwFlags = (uint) (KeyEventF.Scancode | KeyEventF.KeyUp),
                    dwExtraInfo = GetMessageExtraInfo()
                }
            }
        }
    };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
    }

    public static async Task CustomPressKey(ScanCodes scanCode, int length = 100, int lag = 100, bool capital = false)
    {
        if (capital)
        {
            SendInput(ScanCodes.LSHIFT, KeyEventF.None);
        }
        SendInput(scanCode, KeyEventF.None);
        await Task.Delay(length);

        SendInput(scanCode, KeyEventF.KeyUp);
        if (capital)
        {
            SendInput(ScanCodes.LSHIFT, KeyEventF.KeyUp);
        }
        await Task.Delay(lag);
    }

    public static void SendInput(ScanCodes scanCode, KeyEventF additionalFlags)
    {
        uint flagtosend = (uint)(KeyEventF.Scancode | additionalFlags);

        Input[] inputs =
        {
        new Input
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

    public static async Task TextToKeys(string text, int length = 25, int lag = 25)
    {
        foreach (var character in text.ToCharArray())
        {
            switch (character)
            {
                case ' ':
                    await CustomPressKey(ScanCodes.SPACE, length, lag);
                    break;
                case '\n':
                    await CustomPressKey(ScanCodes.RETURN, length, lag);
                    break;
                case '.':
                    await CustomPressKey(ScanCodes.PERIOD, length, lag);
                    break;
                case ',':
                    await CustomPressKey(ScanCodes.COMMA, length, lag);
                    break;
                case '?':
                    await CustomPressKey(ScanCodes.SLASH, length, lag, true);
                    break;
                case '!':
                    await CustomPressKey(ScanCodes.KEY_1, length, lag, true);
                    break;

                case '0':
                    await CustomPressKey(ScanCodes.KEY_0, length, lag);
                    break;
                case '1':
                    await CustomPressKey(ScanCodes.KEY_1, length, lag);
                    break;
                case '2':
                    await CustomPressKey(ScanCodes.KEY_2, length, lag);
                    break;
                case '3':
                    await CustomPressKey(ScanCodes.KEY_3, length, lag);
                    break;
                case '4':
                    await CustomPressKey(ScanCodes.KEY_4, length, lag);
                    break;
                case '5':
                    await CustomPressKey(ScanCodes.KEY_5, length, lag);
                    break;
                case '6':
                    await CustomPressKey(ScanCodes.KEY_6, length, lag);
                    break;
                case '7':
                    await CustomPressKey(ScanCodes.KEY_7, length, lag);
                    break;
                case '8':
                    await CustomPressKey(ScanCodes.KEY_8, length, lag);
                    break;
                case '9':
                    await CustomPressKey(ScanCodes.KEY_9, length, lag);
                    break;

                case 'a':
                case 'A':
                    await CustomPressKey(ScanCodes.A, length, lag, character == 'A');
                    break;
                case 'b':
                case 'B':
                    await CustomPressKey(ScanCodes.B, length, lag, character == 'B');
                    break;
                case 'c':
                case 'C':
                    await CustomPressKey(ScanCodes.C, length, lag, character == 'C');
                    break;
                case 'd':
                case 'D':
                    await CustomPressKey(ScanCodes.D, length, lag, character == 'D');
                    break;
                case 'e':
                case 'E':
                    await CustomPressKey(ScanCodes.E, length, lag, character == 'E');
                    break;
                case 'f':
                case 'F':
                    await CustomPressKey(ScanCodes.F, length, lag, character == 'F');
                    break;
                case 'g':
                case 'G':
                    await CustomPressKey(ScanCodes.G, length, lag, character == 'G');
                    break;
                case 'h':
                case 'H':
                    await CustomPressKey(ScanCodes.H, length, lag, character == 'H');
                    break;
                case 'i':
                case 'I':
                    await CustomPressKey(ScanCodes.I, length, lag, character == 'I');
                    break;
                case 'j':
                case 'J':
                    await CustomPressKey(ScanCodes.J, length, lag, character == 'J');
                    break;
                case 'k':
                case 'K':
                    await CustomPressKey(ScanCodes.K, length, lag, character == 'K');
                    break;
                case 'l':
                case 'L':
                    await CustomPressKey(ScanCodes.L, length, lag, character == 'L');
                    break;
                case 'm':
                case 'M':
                    await CustomPressKey(ScanCodes.M, length, lag, character == 'M');
                    break;
                case 'n':
                case 'N':
                    await CustomPressKey(ScanCodes.N, length, lag, character == 'N');
                    break;
                case 'o':
                case 'O':
                    await CustomPressKey(ScanCodes.O, length, lag, character == 'O');
                    break;
                case 'p':
                case 'P':
                    await CustomPressKey(ScanCodes.P, length, lag, character == 'P');
                    break;
                case 'q':
                case 'Q':
                    await CustomPressKey(ScanCodes.Q, length, lag, character == 'Q');
                    break;
                case 'r':
                case 'R':
                    await CustomPressKey(ScanCodes.R, length, lag, character == 'R');
                    break;
                case 's':
                case 'S':
                    await CustomPressKey(ScanCodes.S, length, lag, character == 'S');
                    break;
                case 't':
                case 'T':
                    await CustomPressKey(ScanCodes.T, length, lag, character == 'T');
                    break;
                case 'u':
                case 'U':
                    await CustomPressKey(ScanCodes.U, length, lag, character == 'U');
                    break;
                case 'v':
                case 'V':
                    await CustomPressKey(ScanCodes.V, length, lag, character == 'V');
                    break;
                case 'w':
                case 'W':
                    await CustomPressKey(ScanCodes.W, length, lag, character == 'W');
                    break;
                case 'x':
                case 'X':
                    await CustomPressKey(ScanCodes.X, length, lag, character == 'X');
                    break;
                case 'y':
                case 'Y':
                    await CustomPressKey(ScanCodes.Y, length, lag, character == 'Y');
                    break;
                case 'z':
                case 'Z':
                    await CustomPressKey(ScanCodes.Z, length, lag, character == 'Z');
                    break;
            }
        }
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
        [FieldOffset(0)] public readonly MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public readonly HardwareInput hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public readonly int dx;
        public readonly int dy;
        public readonly uint mouseData;
        public readonly uint dwFlags;
        public readonly uint time;
        public readonly IntPtr dwExtraInfo;
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
}