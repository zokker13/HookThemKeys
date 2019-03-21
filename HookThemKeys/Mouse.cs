using System;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HookThemKeys
{

    public enum XMouseButtons
    {
        XButton1 = 0x1,
        XButton2 = 0x2,
    }

    public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);
    public delegate void MouseInputChangeHandler(object sender, MouseInputArgs e);
    public class MouseInputArgs : EventArgs
    {
        public IntPtr LParam { get; }
        public NativeMethods.WMessages WParam { get; }
        public Keys Key { get; }
        public bool Suppress { get; set; } = false;
        public MouseInputArgs(NativeMethods.WMessages wparam, IntPtr lparam, Keys key)
        {
            LParam = lparam;
            WParam = wparam;
            Key = key;
        }
    }

    public static class Mouse
    {
        private static HookProc _mouse;
        private static IntPtr MouseHook { get; set; }

        private static void OnMouseButtonInputChanged(object o, MouseInputArgs e)
        {
            MouseButtonInputChanged?.Invoke(o, e);
        }

        public static event MouseInputChangeHandler MouseButtonInputChanged;
        
        public static bool MouseHookRegistered { get; private set; }

        public static bool HookMouse()
        {
            if (MouseHookRegistered)
            {
                return true;
            }

            bool result = false;

            _mouse = (int code, IntPtr wParam, IntPtr lParam) =>
            {
                NativeMethods.WMessages msg = (NativeMethods.WMessages)wParam;

                // If the code is less than 0, Windows want it to be processed without touching..
                // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644985(v=vs.85).aspx
                if (code < 0 || msg != NativeMethods.WMessages.XButtonDown)
                    return NativeMethods.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                
                MSLLHOOKSTRUCT data = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

                int xButton = data.mouseData >> 16;
                var button = (XMouseButtons) xButton;
                var asKeys = button == XMouseButtons.XButton1 ? Keys.XButton1 : Keys.XButton2;

                MouseInputArgs args = new MouseInputArgs(msg, lParam, asKeys);
                
                OnMouseButtonInputChanged(null, args);

                if (args.Suppress)
                {
                    return (IntPtr) 1;
                }

                // Console.WriteLine($"{data.mouseData.ToString("X4")}");

                return NativeMethods.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            };

            IntPtr hook = NativeMethods.SetWindowsHookEx(NativeMethods.HookType.WH_MOUSE_LL, _mouse, IntPtr.Zero, 0);
            var myerror = Marshal.GetLastWin32Error();
            
            if (hook == IntPtr.Zero)
            {
                // Write to file?
                // myerror
            }
            else
            {
                MouseHook = hook;
                result = true;
                MouseHookRegistered = true;
            }

            return result;
        }

        public static bool UnhookMouse()
        {
            return NativeMethods.UnhookWindowsHookEx(MouseHook);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINTFX pt;
        public int mouseData; // be careful, this must be ints, not uints (was wrong before I changed it...). regards, cmew.
        public int flags;
        public int time;
        private UIntPtr dwExtraInfo;
    }

    [Flags]
    public enum KBDLLHOOKSTRUCTFlags : uint
    {
        LLKHF_EXTENDED = 0x01,
        LLKHF_INJECTED = 0x10,
        LLKHF_ALTDOWN = 0x20,
        LLKHF_UP = 0x80,
    }

    public struct POINTFX
    {
        public FIXED x;
        public FIXED y;
    }

    public struct FIXED
    {
        public short fract;
        public short value;
    }
}
