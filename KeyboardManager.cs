using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MultiDesktop
{
    public class KeyboardManager : Imports
    {

        public struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }

        public LowLevelProc _proc;
        public IntPtr hookId;
        public bool bShareKeyboard = false;
        public bool bCapsOn = false;
        private Networker Networker;
        private int tick = 0;

        public void Init(Networker networker)
        {
            this.Networker = networker;
            _proc = KeyboardHookCallback;
            hookId = HookKeyboard(_proc);
        }

        private IntPtr HookKeyboard(LowLevelProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(13, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public bool UnhookKeyboard()
        {
            return UnhookWindowsHookEx(hookId);
        }

        private string ActionFilter(string action)
        {
            //File.AppendAllText("actions.txt", action + "\n");

            switch (action)
            {
                case "Space": return " ";
                case "Return": return "\n";
                case "Back": return "{BACKSPACE}";
                case "Oemcomma": return ",";
                case "Insert": return "";
                case "Capital": this.bCapsOn = !this.bCapsOn; return "";
                
                default:
                    {
                        if (this.bCapsOn)
                            return action.ToUpper();
                        else
                            return action.ToLower();
                    }
            }

        }

        private IntPtr KeyboardHookCallback(int code, int wParam, IntPtr lParam)
        {
            
            if (code >= 0)
            {
                KBDLLHOOKSTRUCT KeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                if (wParam == 0x100 || wParam == 0x104) //keydown
                {
                    if (bShareKeyboard && KeyInfo.key.Equals(Keys.Back))
                    {
                        Networker.msgsToServer.Add(ActionFilter(KeyInfo.key.ToString()));
                    }
                }

                if (wParam == 0x101 || wParam == 0x105) //keyup
                {
                    if (bShareKeyboard && !KeyInfo.key.Equals(Keys.Back))
                    {
                        Networker.msgsToServer.Add(ActionFilter(KeyInfo.key.ToString()));
                    }
                }


                switch (KeyInfo.key)
                {     
                    case Keys.Insert:
                    case Keys.Alt:
                    case Keys.LShiftKey:
                    case Keys.Tab:
                    case Keys.LMenu: return CallNextHookEx(hookId, code, wParam, lParam);
                    case Keys.Capital:
                        {
                            this.bCapsOn = !this.bCapsOn; 
                            return CallNextHookEx(hookId, code, wParam, lParam);
                        }
                }

                if (bShareKeyboard)
                    return (IntPtr)1;
            }
            
            return CallNextHookEx(hookId, code, wParam, lParam);
        }

    }
}
