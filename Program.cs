using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Gma.System.MouseKeyHook;

namespace XboxRemoteControl
{
    // Used in script mode to define each input command.
    public record ScriptInput(string Command, bool IsKeyDown, int DelayMs);

    public enum ControllerType
    {
        Xbox360,
        DualShock4
    }

    class Program
    {
        private static IDualShock4Controller psController;
        private static IXbox360Controller xBoxController;
        private static ViGEmClient client;
        private static IKeyboardMouseEvents globalHook;
        private static ControllerType selectedController;

        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Xbox/PlayStation Remote Play Keyboard Control");
                    Console.WriteLine("Your mouse may get choppy but you will be able to close the tool.");
                    Console.WriteLine("Type 's' for script mode, type 'i' for input mode.");
                RETRY:
                    var input = Console.ReadLine();
                    if (input.Equals("s", StringComparison.OrdinalIgnoreCase))
                    {
                        args = new string[] { "script" };
                    }
                    else if (input.Equals("i", StringComparison.OrdinalIgnoreCase))
                    {
                        args = Array.Empty<string>();
                    }
                    else
                    {
                        Console.WriteLine("Invalid input.");
                        goto RETRY;
                    }
                }

                // Ask user which controller to use.
                Console.WriteLine("Select controller type: (X) for Xbox 360 or (D) for DualShock 4");
                var controllerInput = Console.ReadLine();
                if (controllerInput.Equals("D", StringComparison.OrdinalIgnoreCase))
                {
                    selectedController = ControllerType.DualShock4;
                }
                else
                {
                    selectedController = ControllerType.Xbox360;
                }

                // Create and connect the virtual controller
                client = new ViGEmClient();
                if (selectedController == ControllerType.Xbox360)
                {
                    // Initialize DualShock4 controller for compatibility if needed.
                    psController = client.CreateDualShock4Controller();
                    xBoxController = client.CreateXbox360Controller();
                    xBoxController.Connect();
                    Console.WriteLine("Virtual Xbox 360 Controller created and connected.");
                }
                else if (selectedController == ControllerType.DualShock4)
                {
                    psController = client.CreateDualShock4Controller();
                    psController.Connect();
                    Console.WriteLine("Virtual DualShock 4 Controller created and connected.");
                }

                bool scriptMode = args.Length > 0 &&
                                  args[0].Equals("script", StringComparison.OrdinalIgnoreCase);

                // Subscribe to global keyboard events
                globalHook = Hook.GlobalEvents();
                if (scriptMode)
                {
                    Console.WriteLine("Running in Script Mode.");
                    Console.WriteLine("Press Q or Escape at any time to exit.");
                    globalHook.KeyDown += GlobalHookKeyDown; // For exit detection in script mode.
                    await RunScript();
                    CleanupAndExit();
                }
                else
                {
                    Console.WriteLine("Xbox Remote Play Keyboard Control (Global Hook)");
                    Console.WriteLine("Mapped keys:");
                    Console.WriteLine("  W - Up");
                    Console.WriteLine("  A - Left");
                    Console.WriteLine("  S - Down");
                    Console.WriteLine("  D - Right");
                    Console.WriteLine("  J - A Button");
                    Console.WriteLine("  K - B Button");
                    Console.WriteLine("  U - X Button");
                    Console.WriteLine("  I - Y Button");
                    Console.WriteLine("  O - Left Shoulder");
                    Console.WriteLine("  P - Right Shoulder");
                    Console.WriteLine("  N - Back");
                    Console.WriteLine("  M - Start");
                    Console.WriteLine("  Z - Left Trigger");
                    Console.WriteLine("  X - Right Trigger");
                    Console.WriteLine("  Shift + Arrow Keys - Right Stick");
                    Console.WriteLine("Press Q or Escape to quit.");

                    globalHook.KeyDown += GlobalHookKeyDown;
                    globalHook.KeyUp += GlobalHookKeyUp;

                    // Start a message loop so the global hook receives events.
                    Application.Run();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }

        private static void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {
            // Pressing Q or Escape exits the application.
            if (e.KeyCode == Keys.Q || e.KeyCode == Keys.Escape)
            {
                CleanupAndExit();
                return;
            }

            string command = MapKey(e.KeyCode, e.Shift);
            if (!string.IsNullOrEmpty(command))
            {
                // On key down, set button state to true.
                Task.Run(() => SendControllerCommand(command, true));
            }
        }

        private static void GlobalHookKeyUp(object sender, KeyEventArgs e)
        {
            string command = MapKey(e.KeyCode, e.Shift);
            if (!string.IsNullOrEmpty(command))
            {
                // On key up, set button state back to false.
                Task.Run(() => SendControllerCommand(command, false));
            }
        }

        private static string MapKey(Keys key, bool shift)
        {
            if (shift)
            {
                return key switch
                {
                    Keys.Up => "RightStickUp",
                    Keys.Down => "RightStickDown",
                    Keys.Left => "RightStickLeft",
                    Keys.Right => "RightStickRight",
                    _ => string.Empty,
                };
            }
            else
            {
                return key switch
                {
                    Keys.W => "Up",
                    Keys.A => "Left",
                    Keys.S => "Down",
                    Keys.D => "Right",
                    Keys.J => "A",
                    Keys.K => "B",
                    Keys.U => "X",
                    Keys.I => "Y",
                    Keys.O => "LeftShoulder",
                    Keys.P => "RightShoulder",
                    Keys.N => "Back",
                    Keys.M => "Start",
                    Keys.Z => "LeftTrigger",
                    Keys.X => "RightTrigger",
                    _ => string.Empty,
                };
            }
        }

        private static async Task SendControllerCommand(string command, bool isKeyDown)
        {
            if (selectedController == ControllerType.Xbox360)
            {
                if (xBoxController == null)
                    return;

                switch (command)
                {
                    case "Up":
                        xBoxController.SetButtonState(Xbox360Button.Up, isKeyDown);
                        break;
                    case "Down":
                        xBoxController.SetButtonState(Xbox360Button.Down, isKeyDown);
                        break;
                    case "Left":
                        xBoxController.SetButtonState(Xbox360Button.Left, isKeyDown);
                        break;
                    case "Right":
                        xBoxController.SetButtonState(Xbox360Button.Right, isKeyDown);
                        break;
                    case "A":
                        xBoxController.SetButtonState(Xbox360Button.A, isKeyDown);
                        break;
                    case "B":
                        xBoxController.SetButtonState(Xbox360Button.B, isKeyDown);
                        break;
                    case "X":
                        xBoxController.SetButtonState(Xbox360Button.X, isKeyDown);
                        break;
                    case "Y":
                        xBoxController.SetButtonState(Xbox360Button.Y, isKeyDown);
                        break;
                    case "LeftShoulder":
                        xBoxController.SetButtonState(Xbox360Button.LeftShoulder, isKeyDown);
                        break;
                    case "RightShoulder":
                        xBoxController.SetButtonState(Xbox360Button.RightShoulder, isKeyDown);
                        break;
                    case "Back":
                        xBoxController.SetButtonState(Xbox360Button.Back, isKeyDown);
                        break;
                    case "Start":
                        xBoxController.SetButtonState(Xbox360Button.Start, isKeyDown);
                        break;
                    case "LeftTrigger":
                        xBoxController.SetSliderValue(Xbox360Slider.LeftTrigger, isKeyDown ? (byte)255 : (byte)0);
                        break;
                    case "RightTrigger":
                        xBoxController.SetSliderValue(Xbox360Slider.RightTrigger, isKeyDown ? (byte)255 : (byte)0);
                        break;
                    case "RightStickUp":
                        xBoxController.SetAxisValue(Xbox360Axis.RightThumbY, isKeyDown ? short.MaxValue : (short)0);
                        break;
                    case "RightStickDown":
                        xBoxController.SetAxisValue(Xbox360Axis.RightThumbY, isKeyDown ? short.MinValue : (short)0);
                        break;
                    case "RightStickLeft":
                        xBoxController.SetAxisValue(Xbox360Axis.RightThumbX, isKeyDown ? short.MinValue : (short)0);
                        break;
                    case "RightStickRight":
                        xBoxController.SetAxisValue(Xbox360Axis.RightThumbX, isKeyDown ? short.MaxValue : (short)0);
                        break;
                    default:
                        Console.WriteLine($"No command action defined for {command}");
                        break;
                }
                xBoxController.SubmitReport();
            }
            else if (selectedController == ControllerType.DualShock4)
            {
                if (psController == null)
                    return;

                switch (command)
                {
                    case "Up":
                        psController.SetDPadDirection(isKeyDown ? DualShock4DPadDirection.North : DualShock4DPadDirection.None);
                        break;
                    case "Down":
                        psController.SetDPadDirection(isKeyDown ? DualShock4DPadDirection.South : DualShock4DPadDirection.None);
                        break;
                    case "Left":
                        psController.SetDPadDirection(isKeyDown ? DualShock4DPadDirection.West : DualShock4DPadDirection.None);
                        break;
                    case "Right":
                        psController.SetDPadDirection(isKeyDown ? DualShock4DPadDirection.East : DualShock4DPadDirection.None);
                        break;
                    case "A":
                        psController.SetButtonState(DualShock4Button.Cross, isKeyDown);
                        break;
                    case "B":
                        psController.SetButtonState(DualShock4Button.Circle, isKeyDown);
                        break;
                    case "X":
                        psController.SetButtonState(DualShock4Button.Square, isKeyDown);
                        break;
                    case "Y":
                        psController.SetButtonState(DualShock4Button.Triangle, isKeyDown);
                        break;
                    case "LeftShoulder":
                        psController.SetButtonState(DualShock4Button.ShoulderLeft, isKeyDown);
                        break;
                    case "RightShoulder":
                        psController.SetButtonState(DualShock4Button.ShoulderRight, isKeyDown);
                        break;
                    case "Back":
                        psController.SetButtonState(DualShock4Button.Share, isKeyDown);
                        break;
                    case "Start":
                        psController.SetButtonState(DualShock4Button.Options, isKeyDown);
                        break;
                    case "LeftTrigger":
                        psController.SetSliderValue(DualShock4Slider.LeftTrigger, isKeyDown ? (byte)255 : (byte)0);
                        break;
                    case "RightTrigger":
                        psController.SetSliderValue(DualShock4Slider.RightTrigger, isKeyDown ? (byte)255 : (byte)0);
                        break;
                    case "RightStickUp":
                        psController.SetAxisValue(DualShock4Axis.RightThumbY, isKeyDown ? byte.MaxValue : (byte)0);
                        break;
                    case "RightStickDown":
                        psController.SetAxisValue(DualShock4Axis.RightThumbY, isKeyDown ? byte.MaxValue : (byte)0);
                        break;
                    case "RightStickLeft":
                        psController.SetAxisValue(DualShock4Axis.RightThumbX, isKeyDown ? byte.MaxValue : (byte)0);
                        break;
                    case "RightStickRight":
                        psController.SetAxisValue(DualShock4Axis.RightThumbX, isKeyDown ? byte.MaxValue : (byte)0);
                        break;
                    default:
                        Console.WriteLine($"No command action defined for {command}");
                        break;
                }
                psController.SubmitReport();
            }
            Console.Write($"\r{command} " + (isKeyDown ? "Pressed" : "Released"));
            await Task.CompletedTask;
        }

        private static async Task RunScript()
        {
            // Prompt the user for the script chain and extra delay value.
            Console.WriteLine("Enter your chain of inputs as a comma separated list.");
            Console.WriteLine("Each input should be in the format: Command;IsKeyDown;DelayMs");
            Console.WriteLine("Commands are case sensitive.");
            Console.WriteLine("Type 'cr' for a predefined script to move the camera right slowly for two seconds, then stop.");
            var inputChain = Console.ReadLine();

            ScriptInput[] inputs;
            if (inputChain.Equals("cr", StringComparison.OrdinalIgnoreCase))
            {
                inputs = new ScriptInput[]
                {
                    new ScriptInput("RightStickRight", true, 2000),
                    new ScriptInput("RightStickRight", false, 1000)
                };
            }
            else
            {
                try
                {
                    inputs = inputChain.Split(',')
                                  .Select(x =>
                                  {
                                      var parts = x.Split(';');
                                      return new ScriptInput(parts[0].Trim(), bool.Parse(parts[1].Trim()), int.Parse(parts[2].Trim()));
                                  })
                                  .ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error parsing input: " + ex.Message);
                    return;
                }
            }

            Console.WriteLine("Enter extra delay between input pairs in milliseconds (e.g., 500):");
            var extraDelayStr = Console.ReadLine();
            int extraDelayBetweenPairsMs = int.TryParse(extraDelayStr, out int delay) ? delay : 500;

            Console.WriteLine("Script mode active. Press Q or Escape to exit at any time.");
            int index = 0;
            while (true)
            {
                await SendControllerCommand(inputs[index].Command, inputs[index].IsKeyDown);
                await Task.Delay(inputs[index].DelayMs);

                int nextIndex = (index + 1) % inputs.Length;
                if (!inputs[index].IsKeyDown && inputs[nextIndex].IsKeyDown)
                {
                    await Task.Delay(extraDelayBetweenPairsMs);
                }

                index = nextIndex;
            }
        }

        private static void CleanupAndExit()
        {
            if (globalHook != null)
            {
                globalHook.KeyDown -= GlobalHookKeyDown;
                globalHook.KeyUp -= GlobalHookKeyUp;
                globalHook.Dispose();
            }
            if (selectedController == ControllerType.Xbox360 && xBoxController != null)
            {
                xBoxController.Disconnect();
            }
            else if (selectedController == ControllerType.DualShock4 && psController != null)
            {
                psController.Disconnect();
            }
            if (client != null)
            {
                client.Dispose();
            }
            Console.WriteLine("Exiting...");
            Application.Exit();
        }
    }
}
