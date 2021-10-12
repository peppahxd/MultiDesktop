using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MultiDesktop
{
    public class MouseManager : Imports
    {
        public LowLevelProc _proc;
        public IntPtr hookId;
        public bool bShareMouse = false;
        private Networker Networker;
        private POINT mousePoint;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData, flags, time;
            public IntPtr dwExtraInfo;
        }

        public void Init(Networker networker)
        {
            this.Networker = networker;
            _proc = MouseHookCallback;
            hookId = HookMouse(_proc);

            mousePoint = new POINT();
            mousePoint.x = 0;
            mousePoint.y = 0;
        }


        private IntPtr HookMouse(LowLevelProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(14, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public bool UnhookMouse()
        {
            return UnhookWindowsHookEx(hookId);
        }


        private int iTick = 0;
        private IntPtr MouseHookCallback(int code, int wParam, IntPtr lParam)
        {
            if (code >= 0)
            {
                if (bShareMouse)
                {
                    switch (wParam)
                    {
                        case (int)MouseMessages.WM_LBUTTONUP: Networker.msgsToServer.Add("mouse:lmb"); return (IntPtr)1;
                        case (int)MouseMessages.WM_RBUTTONUP: Networker.msgsToServer.Add("mouse:rmb"); return (IntPtr)1;
                        case (int)MouseMessages.WM_MOUSEMOVE:
                            {
                                iTick++;
                                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                                if (iTick > 15)
                                {
                                    Networker.msgsToServer.Add("mouse-move:" + hookStruct.pt.x + ":" + hookStruct.pt.y + "@");
                                    iTick = 0;
                                }
                                
                                return CallNextHookEx(hookId, code, wParam, lParam);
                            }
                        case (int)MouseMessages.WM_MOUSEWHEEL:
                            {
                                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                                bool up = true ? hookStruct.mouseData == 7864320 : hookStruct.mouseData == 4287102976;

                                Networker.msgsToServer.Add("mouse-scroll:" + up.ToString() + "@");
                                return CallNextHookEx(hookId, code, wParam, lParam);
                            };
                        default: return (IntPtr)1;
                    }
                }
                
            }
            

            return CallNextHookEx(hookId, code, wParam, lParam);
        }
    }
}
