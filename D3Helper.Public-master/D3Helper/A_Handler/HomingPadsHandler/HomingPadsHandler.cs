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
using System.Runtime.InteropServices;

namespace D3Helper.A_Handler.HomingPads
{
    public class HomingPadsHandler
    {
        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);

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

            Process RosBotProcess = Win32Processes.GetProcessesLockingFile(pathToLogFile).FirstOrDefault();
            IntPtr hWndD3 = D3Helper.A_Tools.T_D3Client.GetDiabloWindowHandle();

            while (HomingPadsEquipped)
            {
                Thread.Sleep(50); //reduce cpu usage

                try
                {
                    var newLogLines = logFile.NewLines;

                    if (IsInGame && IsInRift && LogFile.LookForString(newLogLines, "Runstep ended: Wait for items"))
                    {
                        // RG loot dropped... make sure homing pads handler isn't already handling homing pads...
                        if (!isHandlingHomingPads)
                        {
                            // homing pads are equipped, we're in a rift, lets get the hell outta dodge
                            isHandlingHomingPads = true;

                            // sleep for bot to get loot
                            Thread.Sleep(HomingPadsDelay);

                            if (IsInGame && IsInRift)
                            {
                                Log.LogEntry.addLogEntry("Slept for " + HomingPadsDelay.ToString() + "ms after RG but still in rift.  Pausing ROS-BOT and leaving rift.");

                                // pause ROS-BOT
                                SetForegroundWindow(RosBotProcess.MainWindowHandle);
                                WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.F6);
                                Thread.Sleep(500);

                                // do town portal
                                SetForegroundWindow(hWndD3);
                                WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.VK_T);
                                // sleep for town portal animation
                                Thread.Sleep(5500);

                                // unpause ROS-BOT (should really verify we're back in town first)
                                SetForegroundWindow(RosBotProcess.MainWindowHandle);
                                WindowsInput.InputSimulator.SimulateKeyPress(WindowsInput.VirtualKeyCode.F6);
                            } else
                            {
                                Log.LogEntry.addLogEntry("Slept for " + HomingPadsDelay.ToString() + "ms after RG but it looks like ROS-BOT was able to get out in time.");
                            }

                            isHandlingHomingPads = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.ExceptionLogEntry.addExceptionLogEntry(e, ExceptionThread.Handler);
                }
            }
        }
    }
}