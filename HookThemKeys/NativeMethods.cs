using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HookThemKeys
{
    public static class NativeMethods
    {
        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        [Flags]
        public enum WMessages
        {
            Keyfirst = 0x100,
            Keydown = 0x100,
            Keyup = 0x101,
            Char = 0x102,
            Deadchar = 0x103,
            Syskeydown = 0x104,
            Syskeyup = 0x105,
            Syschar = 0x106,
            Sysdeadchar = 0x107,
            Keylast = 0x108,
            Mousefirst = 0x200,
            Mousemove = 0x200,
            Lbuttondown = 0x201,
            Lbuttonup = 0x202,
            Lbuttondblclk = 0x203,
            Rbuttondown = 0x204,
            Rbuttonup = 0x205,
            Rbuttondblclk = 0x206,
            Mbuttondown = 0x207,
            Mbuttonup = 0x208,
            Mbuttondblclk = 0x209,
            Mousewheel = 0x20A,
            XButtonDown = 0x20B,
            XButtonUp = 0x20C,
            Mousehwheel = 0x20E,
        }

        #region user32

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(
            IntPtr hhk
            , int nCode
            , IntPtr wParam
            , IntPtr lParam
        );

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(
            IntPtr hhk
        );

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(
            HookType hookType
            , HookProc lpfn
            , IntPtr hMod
            , uint dwThreadId
        );

        #endregion
    }
}
