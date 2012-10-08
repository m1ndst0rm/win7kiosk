using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace Kiosk
{
    public class globalKeyboardHook
    {
        public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);

        public struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;

        public List<Keys> HookedKeys = new List<Keys>();

        public IntPtr hhook = IntPtr.Zero;

        public keyboardHookProc SAFE_delegate_callback;

        private KioskForm Sender;

        public globalKeyboardHook(Process p, KioskForm sender)
        {
            this.Sender = sender;
            hook(p);
        }

        ~globalKeyboardHook()
        {
            unhook();
        }

        public void hook(Process p)
        {
            SAFE_delegate_callback = new keyboardHookProc(hookProc);

            IntPtr hInstance = LoadLibrary("User32"); //Global hook
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, SAFE_delegate_callback, hInstance, 0);
            return;
        }

        public void unhook()
        {
            UnhookWindowsHookEx(hhook);
            hhook = IntPtr.Zero;
        }

        public int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
        {
           
            bool blnEat = false;

            switch (wParam)
            {
                case 256:
                case 257:
                case 260:
                case 261:
                    //Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key
                    blnEat = ((lParam.vkCode == 9) && (lParam.flags == 32)) | ((lParam.vkCode == 27) && (lParam.flags == 32)) | ((lParam.vkCode == 27) && (lParam.flags == 0)) | ((lParam.vkCode == 91) && (lParam.flags == 1)) | ((lParam.vkCode == 92) && (lParam.flags == 1)) ;

                    if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                    {
                        if (lParam.vkCode == (int)Keys.Escape)
                        {
                            this.Sender.ShowAdmin();
                        }
                    }
                    break;
            }

            if (blnEat == true)
            {
                return 1;
            }
            else
            {
                return CallNextHookEx(hhook, code, wParam, ref lParam);
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
