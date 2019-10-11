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

using SharpRTL.Types;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpRTL {
  /// <summary>
  /// librtlsdr Native Methods
  /// </summary>
  public class Native {
    #region Constants
    private const string LibRtlSdr = "librtlsdr";
    #endregion
    #region Callbacks
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RtlSdrReadAsyncCallback(IntPtr buf, uint len, IntPtr ctx);
    #endregion
    #region Standard Calls
    /// <summary>
    /// Gets the number of available RTLSDR Devices in the USB Bus
    /// </summary>
    /// <returns>count of the rtlsdr devices</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint rtlsdr_get_device_count();

    /// <summary>
    /// Get the RTLSDR Device Name
    /// </summary>
    /// <param name="index">index of the device</param>
    /// <returns></returns>
    public static string rtlsdr_get_device_name(uint index) {return Marshal.PtrToStringAnsi(_rtlsdr_get_device_name(index));}

    [DllImport(LibRtlSdr, EntryPoint = "rtlsdr_get_device_name", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr _rtlsdr_get_device_name(uint index);

    /// <summary>
    /// Get USB device strings.
    /// <para>NOTE: The string arguments must provide space for up to 256 bytes.</para>
    /// </summary>
    /// <param name="index">index the device index</param>
    /// <param name="manufact">manufacter manufacturer name, may be NULL</param>
    /// <param name="product">product product name, may be NULL</param>
    /// <param name="serial">serial serial number, may be NULL</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_get_device_usb_strings(uint index, StringBuilder manufacturer, StringBuilder product, StringBuilder serial);

    /// <summary>
    /// Get device index by USB serial string descriptor.
    /// </summary>
    /// <param name="serial">Serial serial string of the device</param>
    /// <returns>Device index of first device where the name matched</returns>
    /// <returns>-1 if name is null</returns>
    /// <returns>-2 if no devices were found at all</returns>
    /// <returns>-3 if devices were found, but none with matching name</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_get_index_by_serial([MarshalAs(UnmanagedType.LPStr)] string serial);

    /// <summary>
    /// Opens the RTLSDR Device
    /// </summary>
    /// <param name="dev">A reference to a pointer</param>
    /// <param name="index">Device Index</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_open(out IntPtr dev, uint index);

    /// <summary>
    /// Closes an RTLSDR Device
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_close(IntPtr dev);
    #endregion
    #region Configuration Functions

    /// <summary>
    /// Set crystal oscillator frequencies used for the RTL2832 and the tuner IC.
    /// <para>
    /// Usually both ICs use the same clock. Changing the clock may make sense if
    /// you are applying an external clock to the tuner or to compensate the
    /// frequency (and samplerate) error caused by the original (cheap) crystal.
    /// </para>
    /// <para>
    /// NOTE: Call this function only if you fully understand the implications.
    /// </para>
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="rtlFreq">Frequency value used to clock the RTL2832 in Hz</param>
    /// <param name="tunerFreq">Frequency value used to clock the tuner IC in Hz</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_xtal_freq(IntPtr dev, uint rtlFreq, uint tunerFreq);

    /// <summary>
    /// Get crystal oscillator frequencies used for the RTL2832 and the tuner IC.
    /// <para>Usually both ICs use the same clock.</para>
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="rtlFreq">Frequency value used to clock the RTL2832 in Hz</param>
    /// <param name="tunerFreq">Frequency value used to clock the tuner IC in Hz</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_get_xtal_freq(IntPtr dev, out uint rtlFreq, out uint tunerFreq);

    /// <summary>
    /// Get USB device strings.
    /// <para>NOTE: The string arguments must provide space for up to 256 bytes.</para>
    /// </summary>
    /// <param name="index">index the device index</param>
    /// <param name="manufact">manufacter manufacturer name, may be NULL</param>
    /// <param name="product">product product name, may be NULL</param>
    /// <param name="serial">serial serial number, may be NULL</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_get_usb_strings(IntPtr dev, StringBuilder manufact, StringBuilder product, StringBuilder serial);

    /// <summary>
    /// Write the device EEPROM
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="data">Buffer of data to be written</param>
    /// <param name="offset">Address where the data should be read from</param>
    /// <param name="len">Length of the data</param>
    /// <returns>0 on success</returns>
    /// <returns>-1 if device handle is invalid</returns>
    /// <returns>-2 if EEPROM size is exceeded</returns>
    /// <returns>-3 if no EEPROM was found</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_write_eeprom(IntPtr dev, ref byte[] data, byte offset, UInt16 len);

    /// <summary>
    /// Read the device EEPROM
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="data">Buffer where the data should be written</param>
    /// <param name="offset">Offset address where the data should be written</param>
    /// <param name="len">Length of the data</param>
    /// <returns>0 on success</returns>
    /// <returns>-1 if device handle is invalid</returns>
    /// <returns>-2 if EEPROM size is exceeded</returns>
    /// <returns>-3 if no EEPROM was found</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_read_eeprom(IntPtr dev, ref byte[] data, byte offset, UInt16 len);

    /// <summary>
    /// Set the frequency the device is tuned to.
    /// </summary>
    /// <param name="dev">Device handle given by rtlsdr_open()</param>
    /// <param name="freq">Frequency in Hz</param>
    /// <returns>0 on error, frequency in Hz otherwise</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint rtlsdr_set_center_freq(IntPtr dev, uint freq);

    /// <summary>
    /// Get actual frequency the device is tuned to.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>0 on error, frequency in Hz otherwise</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint rtlsdr_get_center_freq(IntPtr dev);

    /// <summary>
    /// Set the frequency correction value for the device.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="ppm">Correction value in parts per million (ppm)</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_freq_correction(IntPtr dev, int ppm);

    /// <summary>
    /// Get actual frequency correction value of the device.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>Correction value in parts per million (ppm)</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_get_freq_correction(IntPtr dev);

    /// <summary>
    /// Get the tuner type.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>RTLSDR_TUNER_UNKNOWN on error, tuner type otherwise</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern TunerType rtlsdr_get_tuner_type(IntPtr dev);

    /// <summary>
    /// Get a list of gains supported by the tuner.
    /// <para>
    /// NOTE: The gains argument must be preallocated by the caller. If NULL is
    /// being given instead, the number of available gain values will be returned.
    /// </para>
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="gains">Array of gain values. In tenths of a dB, 115 means 11.5 dB</param>
    /// <returns><= 0 on error, number of available (returned) gain values otherwise</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_get_tuner_gains(IntPtr dev, [In, Out] int[] gains);

    /// <summary>
    /// Set the gain for the device.
    /// <para>Manual gain mode must be enabled for this to work.</para>
    /// <para>Valid gain values may be queried with rtlsdr_get_tuner_gains function.</para>
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="gain">Gain in tenths of a dB, 115 means 11.5 dB</param>
    /// <see cref="rtlsdr_get_tuner_gains(IntPtr, int[])"/>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_tuner_gain(IntPtr dev, int gain);

    /// <summary>
    /// Get actual gain the device is configured to.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>0 on error, gain in tenths of a dB, 115 means 11.5 dB.</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_get_tuner_gain(IntPtr dev);

    /// <summary>
    /// Set the gain mode (automatic/manual) for the device.
    /// <para>Manual gain mode must be enabled for the gain setter function to work.</para>
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="manual">True means manual gain mode shall be enabled</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_tuner_gain_mode(IntPtr dev, bool manual);

    /// <summary>
    /// Set the sample rate for the device, also selects the baseband filters
    /// according to the requested sample rate for tuners where this is possible.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="rate">The sample rate to be set</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_sample_rate(IntPtr dev, uint rate);

    /// <summary>
    /// Get actual sample rate the device is configured to.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>0 on error, sample rate in Hz otherwise</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint rtlsdr_get_sample_rate(IntPtr dev);

    /// <summary>
    /// Enable test mode that returns an 8 bit counter instead of the samples.
    /// <para>The counter is generated inside the RTL2832.</para>
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="enabled">True means enabled, False disabled</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_testmode(IntPtr dev, bool enabled);

    /// <summary>
    /// Enable or disable the internal digital AGC of the RTL2832.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="enabled">True means enabled, 0 disabled</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_agc_mode(IntPtr dev, bool enabled);

    /// <summary>
    /// Enable or disable the direct sampling mode. When enabled, the IF mode
    /// of the RTL2832 is activated, and rtlsdr_set_center_freq() will control
    /// the IF-frequency of the DDC, which can be used to tune from 0 to 28.8 MHz
    /// (xtal frequency of the RTL2832).
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="mode">The direct sampling mode</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_direct_sampling(IntPtr dev, DirectSamplingMode mode);

    /// <summary>
    /// Get state of the direct sampling mode
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>DirectSamplingMode</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern DirectSamplingMode rtlsdr_get_direct_sampling(IntPtr dev);

    /// <summary>
    /// Enable or disable offset tuning for zero-IF tuners, which allows to avoid problems caused by the DC offset of the ADCs and 1/f noise.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="enabled">True for enabled, False for Disabled</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_offset_tuning(IntPtr dev, bool enabled);

    /// <summary>
    /// Get state of the offset tuning mode
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>-1 on error, 0 means disabled, 1 enabled</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_get_offset_tuning(IntPtr dev);

    #endregion
    #region Streaming Functions
    /// <summary>
    /// Resets the RTLSDR Buffer
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_reset_buffer(IntPtr dev);

    /// <summary>
    /// Reads data from rtlsdr synchronously
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="buf">The buffer output</param>
    /// <param name="len">The length to read</param>
    /// <param name="nRead">How many bytes was read (output)</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_read_sync(IntPtr dev, IntPtr buf, int len, out int nRead);

    /// <summary>
    /// Read samples from the device asynchronously. This function will block until it is being canceled using rtlsdr_cancel_async()
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="cb">Callback function to return received samples</param>
    /// <param name="ctx">User specific context to pass via the callback function</param>
    /// <param name="bufNum">Optional buffer count, buf_num * buf_len = overall buffer size. Set to 0 for default buffer count (15)</param>
    /// <param name="bufLen">Optional buffer length, must be multiple of 512. should be a multiple of 16384 (URB size), set to 0 for default buffer length(16 * 32 * 512)</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_read_async(IntPtr dev, RtlSdrReadAsyncCallback cb, IntPtr ctx, uint bufNum, uint bufLen);

    /// <summary>
    /// Cancel all pending asynchronous operations on the device.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_cancel_async(IntPtr dev);

    /// <summary>
    /// Read samples from the device asynchronously. This function will block until it is being canceled using rtlsdr_cancel_async()
    /// <para>NOTE: This function is deprecated and is subject for removal.</para>
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="cb">Callback function to return received samples</param>
    /// <param name="ctx">User specific context to pass via the callback function</param>
    /// <returns>0 on success</returns>
    [Obsolete("rtlsdr_wait_async(IntPtr, RtlSdrReadAsyncCallback, IntPtr) is deprecated, please use rtlsdr_read_async(IntPtr, RtlSdrReadAsyncCallback, IntPtr, uint, uint) instead.")]
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_wait_async(IntPtr dev, RtlSdrReadAsyncCallback cb, IntPtr ctx);
    #endregion
    #region Extended RTLSDR
    /// <summary>
    /// Sets the IF Filter (The bandwidth) of the Tuner for the device.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="bandwidth">Bandwidth in Hz. Zero means automatic BW selection.</param>
    /// <param name="applied_BandWidth">The applied value in Hz or 0 if unknown.</param>
    /// <param name="apply_bw">true to really apply configure the tuner chip; false for just returning applied_bw</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_and_get_tuner_bandwidth(IntPtr dev, uint bandwidth, out uint applied_bandWidth, bool apply_bw);

    /// <summary>
    /// Sets the IF Filter (The bandwidth) of the Tuner for the device.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="bandwidth">Bandwidth in Hz. Zero means automatic BW selection</param>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_tuner_bandwidth(IntPtr dev, uint bandwidth);

    /// <summary>
    /// Set the intermediate frequency gain for the device.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="stage">Intermediate frequency gain stage number (1 to 6 for E4000)</param>
    /// <param name="gain">Gain in tenths of a dB, -30 means -3.0 dB</param>
    /// <see cref="rtlsdr_set_tuner_gain_mode(IntPtr, bool)"/>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_tuner_if_gain(IntPtr dev, int stage, int gain);

    /// <summary>
    /// Set LNA / Mixer / VGA Device Gain for R820T device is configured to.
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="lna_gain">LNA Gain in tenths of a dB, -30 means -3.0 dB</param>
    /// <param name="mixer_gain">Mixer Gain in tenths of a dB, -30 means -3.0 dB</param>
    /// <param name="vga_gain">VGA Gain in tenths of a dB, -30 means -3.0 dB</param>
    /// <see cref="rtlsdr_set_tuner_gain_mode(IntPtr, bool)"/>
    /// <returns>0 on success</returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_set_tuner_gain_ext(IntPtr dev, int lna_gain, int mixer_gain, int vga_gain);

    /// <summary>
    /// Read from the remote control (RC) infrared (IR) sensor
    /// </summary>
    /// <param name="dev">The device handle given by rtlsdr_open()</param>
    /// <param name="buf">Buffer to write IR signal (MSB=pulse/space, 7LSB=duration*20usec), recommended 128-bytes</param>
    /// <param name="buf_len">Size of buf</param>
    /// <returns></returns>
    [DllImport(LibRtlSdr, CallingConvention = CallingConvention.Cdecl)]
    public static extern int rtlsdr_ir_query(IntPtr dev, IntPtr buf, int buf_len);
    #endregion
  }
}
