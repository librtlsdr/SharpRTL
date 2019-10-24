///////////////////////////////////////////////////////////////////////////////////
///    C# RTLSDR Bindings                                                       ///
///    Copyright(C) 2016 Lucas Teske                                            ///
///                                                                             ///
///    This program is free software: you can redistribute it and/or modify     ///
///    it under the terms of the GNU General Public License as published by     ///
///    the Free Software Foundation, either version 3 of the License, or        ///
///    any later version.                                                       ///
///                                                                             ///
///    This program is distributed in the hope that it will be useful,          ///
///    but WITHOUT ANY WARRANTY; without even the implied warranty of           ///
///    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the              ///
///    GNU General Public License for more details.                             ///
///                                                                             ///
///    You should have received a copy of the GNU General Public license        ///
///    along with this program.If not, see<http://www.gnu.org/licenses/>.       ///
///////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;
using System.Threading;
using SharpRTL;

namespace SharpRTL.Types {
  public class RtlDevice {
    #region Constants
    private const uint DefaultFrequency = 106300000;
    private const int DefaultSamplerate = 2560000;
    #endregion
    #region Delegates
    public delegate void SamplesAvailableEvent(ref byte[] data, int length);
    #endregion
    #region Static Fields
    private static readonly SharpRTL.Native.RtlSdrReadAsyncCallback _rtlCallback = new SharpRTL.Native.RtlSdrReadAsyncCallback(rtlsdrSamplesAvailableCallback);
    private static readonly uint _readLength = 16384;
    #endregion
    #region Fields
    private uint _centerFrequency = DefaultFrequency;
    private IntPtr _dev;
    private int _frequencyCorrection;
    private GCHandle _gcHandle;
    private uint _sampleRate = DefaultSamplerate;
    private DirectSamplingMode _samplingMode;
    private bool _useOffsetTuning;
    private bool _useRtlAGC;
    private bool _useTunerAGC = true;
    private Thread _worker;
    private int _lnaGain = 0;
    private int _mixerGain = 0;
    private int _vgaGain = 0;
    #endregion
    #region Readonly Fields
    private readonly uint _index;
    private readonly string _name;
    private readonly int[] _supportedGains;
    private readonly bool _supportsOffsetTuning;
    #endregion
    #region Events
    public event SamplesAvailableEvent SamplesAvailable;
    #endregion
    #region Constructors / Destructors
    public RtlDevice(uint index) {
      _index = index;
      if (Native.rtlsdr_open(out _dev, _index) != 0) {
        throw new Exception("Cannot open RTL device. Is the device locked somewhere?");
      }

      int tunerGains = (_dev == IntPtr.Zero) ? 0 : Native.rtlsdr_get_tuner_gains(_dev, null);
      if (tunerGains < 0) {
        tunerGains = 0;
      }
      _useTunerAGC = true;

      _supportsOffsetTuning = Native.rtlsdr_set_offset_tuning(_dev, false) != -2;
      _supportedGains = new int[tunerGains];
      if (tunerGains >= 0) {
        Native.rtlsdr_get_tuner_gains(_dev, _supportedGains);
      }

      _name = Native.rtlsdr_get_device_name(_index);
      _gcHandle = GCHandle.Alloc(this);
    }
    public void Dispose() {
      Stop();
      Native.rtlsdr_close(_dev);
      if (_gcHandle.IsAllocated) {
        _gcHandle.Free();
      }
      _dev = IntPtr.Zero;
      GC.SuppressFinalize(this);
    }
    #endregion
    #region Methods
    /// <summary>
    /// Callback from the librtlsdr when samples are available.
    /// </summary>
    /// <param name="buf">Sample buffer</param>
    /// <param name="len">Length of sample buffer</param>
    /// <param name="ctx">Context</param>
    private static void rtlsdrSamplesAvailableCallback(IntPtr buff, uint len, IntPtr ctx) {
      GCHandle handle = GCHandle.FromIntPtr(ctx);
      if (handle.IsAllocated) {
        RtlDevice target = (RtlDevice)handle.Target;

        byte[] managedArray = new byte[len];
        Marshal.Copy(buff, managedArray,0 , (int)len);
        target.SamplesAvailable(ref managedArray, (int)len);
      }
    }
    /// <summary>
    /// Starts the working thread
    /// </summary>
    public void Start() {
      if (_worker != null) {
        throw new Exception("This device worker is already running!");
      }

      if (Native.rtlsdr_set_sample_rate(_dev, _sampleRate) != 0) {
        throw new Exception("Cannot set device sample rate!");
      }

      if (Native.rtlsdr_set_center_freq(_dev, _centerFrequency) != 0) {
        throw new Exception("Cannot set device center frequency!");
      }

      if (Native.rtlsdr_set_tuner_gain_mode(_dev, _useTunerAGC) != 0) {
        throw new Exception("Cannot set Gain Mode");
      }

      if (Native.rtlsdr_set_tuner_gain_ext(_dev, _lnaGain, _mixerGain, _vgaGain) != 0) {
        throw new Exception("Cannot set gains");
      }

      if (Native.rtlsdr_reset_buffer(_dev) != 0) {
        throw new Exception("Cannot reset rtlsdr buffer");
      }

      _worker = new Thread(new ThreadStart(WorkerMethod));
      _worker.Priority = ThreadPriority.Highest;
      _worker.Start();
    }
    /// <summary>
    /// Stops the Working Thread
    /// </summary>
    public void Stop() {
      if (_worker != null) {
        Native.rtlsdr_cancel_async(_dev);
        if (_worker.ThreadState == ThreadState.Running) {
          _worker.Join();
        }
        _worker = null;
      }
    }
    /// <summary>
    /// Refresh the Gains
    /// </summary>
    private void RefreshGains() {
      if (_dev != IntPtr.Zero) {
        Native.rtlsdr_set_tuner_gain_ext(_dev, _lnaGain, _mixerGain, _vgaGain);
      }
    }
    /// <summary>
    /// Internal Stream Processor: Calls the RTLSDR Library ReadAsync
    /// </summary>
    private void WorkerMethod() {
      Native.rtlsdr_read_async(_dev, _rtlCallback, (IntPtr)_gcHandle, 0, _readLength);
    }
    #endregion
    #region Properties
    /// <summary>
    /// The Device Handle for calling Native calls
    /// </summary>
    public IntPtr DeviceHandle {
      get { return _dev; }
    }
    /// <summary>
    /// Device Tuned Frequency
    /// </summary>
    public uint Frequency {
      get { return _centerFrequency; }
      set {
        _centerFrequency = value;
        if (_dev != IntPtr.Zero) {
          Native.rtlsdr_set_center_freq(_dev, _centerFrequency);
        }
      }
    }
    /// <summary>
    /// Device Frequency Correction (PPM)
    /// </summary>
    public int FrequencyCorrection {
      get { return _frequencyCorrection; }
      set {
        _frequencyCorrection = value;
        if (_dev != IntPtr.Zero) {
          Native.rtlsdr_set_freq_correction(_dev, _frequencyCorrection);
        }
      }
    }
    /// <summary>
    /// R820T LNA Gain
    /// </summary>
    public int LNAGain {
      get { return _lnaGain; }
      set {
        _lnaGain = value;
        RefreshGains();
      }
    }
    /// <summary>
    /// R820T Mixer Gain
    /// </summary>
    public int MixerGain {
      get { return _mixerGain; }
      set {
        _mixerGain = value;
        RefreshGains();
      }
    }
    /// <summary>
    /// R820T VGA Gain
    /// </summary>
    public int VGAGain {
      get { return _vgaGain; }
      set {
        _vgaGain = value;
        RefreshGains();
      }
    }
    /// <summary>
    /// Device Index
    /// </summary>
    public uint Index { get {return _index;} }
    /// <summary>
    /// Running State
    /// </summary>
    public bool IsRunning { get {return _worker != null;}}
    /// <summary>
    /// Device Name
    /// </summary>
    public string Name { get { return _name;} }
    /// <summary>
    /// Device Sample Rate
    /// </summary>
    public uint SampleRate {
      get { return _sampleRate; }
      set {
        _sampleRate = value;
        if (_dev != IntPtr.Zero) {
          Native.rtlsdr_set_sample_rate(_dev, _sampleRate);
        }
      }
    }
    /// <summary>
    /// Device Sampling Mode
    /// </summary>
    public DirectSamplingMode SamplingMode {
      get { return _samplingMode; }
      set {
        _samplingMode = value;
        if (_dev != IntPtr.Zero) {
          Native.rtlsdr_set_direct_sampling(_dev, _samplingMode);
        }
      }
    }
    /// <summary>
    /// Device supports offset tunning
    /// </summary>
    public bool SupportsOffsetTuning { get { return  _supportsOffsetTuning; } }

    /// <summary>
    /// Device Tuner Type
    /// </summary>
    public TunerType TunerType {
      get {
        if (!(_dev == IntPtr.Zero)) {
          return Native.rtlsdr_get_tuner_type(_dev);
        }
        return TunerType.Unknown;
      }
    }
    /// <summary>
    /// Device use offset tunning
    /// </summary>
    public bool UseOffsetTuning {
      get { return _useOffsetTuning; }
      set {
        _useOffsetTuning = value;
        if (_dev != IntPtr.Zero) {
          Native.rtlsdr_set_offset_tuning(_dev, _useOffsetTuning);
        }
      }
    }

    /// <summary>
    /// Use Device RTL Automatic Gain Control
    /// </summary>
    public bool UseRtlAGC {
      get { return _useRtlAGC; }
      set {
        _useRtlAGC = value;
        if (_dev != IntPtr.Zero) {
          Native.rtlsdr_set_agc_mode(_dev, _useRtlAGC);
        }
      }
    }
    /// <summary>
    /// Use Device Tuner Automatic Gain Control
    /// </summary>
    public bool UseTunerAGC {
      get { return _useTunerAGC; }
      set {
        _useTunerAGC = value;
        if (_dev != IntPtr.Zero) {
          Native.rtlsdr_set_tuner_gain_mode(_dev, _useTunerAGC);
        }
      }
    }
    #endregion
  }
}
