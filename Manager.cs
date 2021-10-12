using System;
using System.Collections.Generic;
using System.Text;

namespace MultiDesktop
{
    public class Manager
    {
        public Networker Networker { get; set; }

        public KeyboardManager KeyboardManager { get; set; }

        public MouseManager MouseManager { get; set; }

        public void Init()
        {
            Networker = new Networker();
            Networker.Init();

            KeyboardManager = new KeyboardManager();
            KeyboardManager.Init(Networker);

            MouseManager = new MouseManager();
            MouseManager.Init(Networker);

        }
    }
}
