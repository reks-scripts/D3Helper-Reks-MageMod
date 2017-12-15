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
using D3Helper.A_Tools.FileSystemWatcher;
using SlimDX.DirectInput;

namespace D3Helper.A_Handler.HomingPads
{
    class HomingPadsHandler
    {
        public bool HomingPadsEquipped
        {
            get => Properties.Settings.Default.HomingPadsBool;
        }

        public int HomingPadsDelay
        {
            get => Properties.Settings.Default.HomingPadsDelay;
        }

        public bool IsInGame
        {
            get => A_Collection.Me.HeroStates.isInGame;
        }

        public bool IsInRift
        {
            get => A_Tools.T_LevelArea.IsRift();
        }

        public bool IsTeleporting
        {
            get => A_Collection.Me.HeroStates.isTeleporting;
        }

        public bool IsHandlingHomingPads
        {
            get => isHandlingHomingPads;
        }

        private bool isHandlingHomingPads = false;
        private LogFile logFile;

        public HomingPadsHandler()
        {

            String pathToLogFile = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\RoS-BoT\\Logs\\Logs.txt";
            logFile = new LogFile(pathToLogFile);

            while (HomingPadsEquipped)
            {
                try
                {
                    var newLogLines = logFile.NewLines;

                    if (LogFile.LookForString(newLogLines, "Runstep ended: Wait for items"))
                    {
                        // RG loot dropped... make sure homing pads handler isn't already handling homing pads...
                        if (!isHandlingHomingPads)
                        {
                            // homing pads are equipped, we're in a rift, we're not teleporting, lets get the hell outta dodge
                            isHandlingHomingPads = true;

                            // sleep for bot to get loot
                            Thread.Sleep(HomingPadsDelay);

                            if (IsInGame && IsInRift && !IsTeleporting)
                            {
                                // pause ROS-BOT
                                WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.F6);
                                Thread.Sleep(200);

                                // do town portal
                                WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.VK_T);
                                // check how long town portal animation is and add a little buffer to load home town map
                                Thread.Sleep(5000);

                                // unpause ROS-BOT (should really verify we're back in town first)
                                WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.F6);
                            }

                            isHandlingHomingPads = false;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
        }
    }
}