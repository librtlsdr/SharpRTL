using SharpRTL;
using SharpRTL.Types;
using System;

namespace SampleApp {
  class Program {
    #region Fields
    private readonly static ConsoleColor defaultFg = Console.ForegroundColor;
    private static int samplesReceived = 0;
    #endregion
    #region Main Code
    static void Main(string[] args) {
      #region Device List Block
      uint deviceCount = Native.rtlsdr_get_device_count();
      if (deviceCount == 0) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("No Available Devices");
        Console.ForegroundColor = defaultFg;
        waitClickAndExit();
      }

      Console.ForegroundColor = ConsoleColor.DarkCyan;
      for (uint i = 0; i < deviceCount; i++) {
        string devName = Native.rtlsdr_get_device_name(i);
        Console.WriteLine("Device({0}): {1}", i, devName);
      }
      Console.ForegroundColor = defaultFg;
      #endregion
      #region Device Select Block
      uint idx = 0;
      if (deviceCount > 1) {
        bool valid = false;
        while (!valid) {
          Console.WriteLine("Please enter the device number: ");
          string devNumStr = Console.ReadLine();
          valid = uint.TryParse(devNumStr, out idx);
        }
      }
      #endregion
      #region Device Open Block
      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("Opening device {0}", idx);
      RtlDevice device = new RtlDevice(idx);
      Console.WriteLine("Tuner Type: {0}", device.TunerType.ToString());
      Console.WriteLine("Tuned to: {0}", device.Frequency);
      Console.WriteLine("Sample Rate: {0}", device.SampleRate);
      device.SamplesAvailable += Device_SamplesAvailable;
      Console.WriteLine("Starting Worker Thread");
      device.Start();
      Console.ForegroundColor = defaultFg;
      #endregion
      #region Exit Block
      Console.WriteLine("Press any key to exit.");
      Console.ReadLine();

      Console.WriteLine("Stopping device");
      device.Stop();

      Environment.Exit(0);
      #endregion
    }
    #endregion
    #region Samples Callback
    private static void Device_SamplesAvailable(ref byte[] data, int length) {
      samplesReceived += length;
      if (samplesReceived >= 1024 * 1024) {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Received {0} samples!", samplesReceived);
        Console.ForegroundColor = defaultFg;
        samplesReceived = 0;
      }
    }
    #endregion
    #region Utils
    public static void waitClickAndExit() {
      Console.WriteLine("Press any key to exit.");
      Console.ReadLine();
      Environment.Exit(0);
    }
    #endregion
  }
}
