using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Gma.System.MouseKeyHook;

namespace XboxRemoteControl
{
    // Used in script mode to define each input command.
    public record ScriptInput(string Command, bool IsKeyDown, int DelayMs);

    class Program
    {
        private static IXbox360Controller controller;
        private static ViGEmClient client;
        private static IKeyboardMouseEvents globalHook;

        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Xbox Remote Play Keyboard Control");
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

                // Create and connect the virtual controller
                client = new ViGEmClient();
                controller = client.CreateXbox360Controller();
                controller.Connect();
                Console.WriteLine("Virtual Xbox 360 Controller created and connected.");

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
                Task.Run(() => SendXboxCommand(command, true));
            }
        }

        private static void GlobalHookKeyUp(object sender, KeyEventArgs e)
        {
            string command = MapKey(e.KeyCode, e.Shift);
            if (!string.IsNullOrEmpty(command))
            {
                // On key up, set button state back to false.
                Task.Run(() => SendXboxCommand(command, false));
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

        private static async Task SendXboxCommand(string command, bool isKeyDown)
        {
            if (controller == null)
                return;

            switch (command)
            {
                case "Up":
                    controller.SetButtonState(Xbox360Button.Up, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "Down":
                    controller.SetButtonState(Xbox360Button.Down, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "Left":
                    controller.SetButtonState(Xbox360Button.Left, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "Right":
                    controller.SetButtonState(Xbox360Button.Right, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "A":
                    controller.SetButtonState(Xbox360Button.A, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "B":
                    controller.SetButtonState(Xbox360Button.B, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "X":
                    controller.SetButtonState(Xbox360Button.X, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "Y":
                    controller.SetButtonState(Xbox360Button.Y, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "LeftShoulder":
                    controller.SetButtonState(Xbox360Button.LeftShoulder, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "RightShoulder":
                    controller.SetButtonState(Xbox360Button.RightShoulder, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "Back":
                    controller.SetButtonState(Xbox360Button.Back, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "Start":
                    controller.SetButtonState(Xbox360Button.Start, isKeyDown);
                    controller.SubmitReport();
                    break;
                case "LeftTrigger":
                    controller.SetSliderValue(Xbox360Slider.LeftTrigger, isKeyDown ? (byte)255 : (byte)0);
                    controller.SubmitReport();
                    break;
                case "RightTrigger":
                    controller.SetSliderValue(Xbox360Slider.RightTrigger, isKeyDown ? (byte)255 : (byte)0);
                    controller.SubmitReport();
                    break;
                case "RightStickUp":
                    controller.SetAxisValue(Xbox360Axis.RightThumbY, isKeyDown ? short.MaxValue : (short)0);
                    controller.SubmitReport();
                    break;
                case "RightStickDown":
                    controller.SetAxisValue(Xbox360Axis.RightThumbY, isKeyDown ? short.MinValue : (short)0);
                    controller.SubmitReport();
                    break;
                case "RightStickLeft":
                    controller.SetAxisValue(Xbox360Axis.RightThumbX, isKeyDown ? short.MinValue : (short)0);
                    controller.SubmitReport();
                    break;
                case "RightStickRight":
                    controller.SetAxisValue(Xbox360Axis.RightThumbX, isKeyDown ? short.MaxValue : (short)0);
                    controller.SubmitReport();
                    break;
                default:
                    Console.WriteLine($"No command action defined for {command}");
                    break;
            }
            Console.Write($"\r{command} " + (isKeyDown ? "Pressed" : "Released"));
            await Task.CompletedTask;
        }

        private static async Task RunScript()
        {
            // Prompt the user for the script chain and extra delay value.
            Console.WriteLine("Enter your chain of inputs as a comma separated list.");
            Console.WriteLine("Each input should be in the format: Command;IsKeyDown;DelayMs");
            Console.WriteLine("Example:");
            Console.WriteLine("Right;true;100,Right;false;100,Left;true;100,Left;false;100");
            Console.WriteLine("Or type 'cr' for a predefined script to move the camera right slowly for two seconds, then stop, and repeat.");
            var inputChain = Console.ReadLine();

            ScriptInput[] inputs;
            if (inputChain.Equals("cr", StringComparison.OrdinalIgnoreCase))
            {
                inputs = new ScriptInput[]
                {
                    new ScriptInput("RightStickRight", true, 2000),
                    new ScriptInput("RightStickRight", false, 1000),
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
                await SendXboxCommand(inputs[index].Command, inputs[index].IsKeyDown);
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
            if (controller != null)
            {
                controller.Disconnect();
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


