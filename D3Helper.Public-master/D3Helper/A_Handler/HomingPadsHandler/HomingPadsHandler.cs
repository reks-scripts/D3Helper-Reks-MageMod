using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enigma.D3;
using Enigma.D3.UI;
using D3Helper.A_Enums;
using D3Helper.A_Collector;
using D3Helper.A_Tools.FileSystemWatcherMemoryCache;
using SlimDX.DirectInput;

namespace D3Helper.A_Handler.AutoCube
{
    class HomingPadsHandler
    {
        private LogFileHandler _Watcher;


        public bool HomingPadsEquipped
        {
            get
            {
                return Properties.Settings.Default.HomingPadsBool;
            }
        }

        public bool IsInGame
        {
            get
            {
                return A_Collection.Me.HeroStates.isInGame;
            }
        }

        public bool IsInRift
        {
            get
            {
                return A_Tools.T_LevelArea.IsRift();
            }
        }

        public bool IsTeleporting
        {
            get
            {
                return A_Collection.Me.HeroStates.isTeleporting;
            }
        }

        public HomingPadsHandler()
        {
            while (true)
            {
                if (HomingPadsEquipped && IsInGame && IsInRift && !IsTeleporting)
                {
                    checkRosLogFile();
                }
                Thread.Sleep(1000);
            }
        }

        public void checkRosLogFile()
        {
            // need to read in the ROS-BOT log file and search for "INFO - Runstep ended: Wait for items" and the end of the file
            // if we find it, the bot has finished the rift so we will interupt it by pausing and then portal back to town

            // pause ROS-BOT
            WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.F6);
            Thread.Sleep(500);

            // do town portal
            WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.VK_T);
            // check how long town portal animation is and add a little buffer to load home town map
            // maybe wait on home town map loaded event? 
            Thread.Sleep(5000);

            // unpause ROS-BOT
            WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.F6);
        }
    }
}