// <copyright file="HidApi.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    ///   Wrapper class for hidapi DLL.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     split into partial class to group functionality while shutting up
    ///     warnings about order of members.
    ///   </para>
    /// </remarks>
    [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1300:ElementMustBeginWithUpperCaseLetter",
            Justification = "Definition matches hidapi source as closely as possible.")]
    [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
            Justification = "Definition matches hidapi source as closely as possible.")]
    [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1310:FieldNamesMustNotContainUnderscore",
            Justification = "Definition matches hidapi source as closely as possible.")]
    [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "CA1707:ParameterNamesMustNotContainUnderscore",
            Justification = "Definition matches hidapi source as closely as possible.")]
    public sealed partial class HidApi
    {
        /// <summary>
        ///   File name of hidapi DLL.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     On windows for system installed: hidapi.dll
        ///     On linux for system installed: "libhidapi-hidraw" or "libhidapi-libusb"
        ///     unfortunately there is no way simple to automatically
        ///     find the library on all platforms because of different
        ///     naming conventions.
        ///   </para>
        ///   <para>
        ///     Just use hidapi and expect users to supply it in same folder as .exe
        ///     For development purposes, I downloaded the release from
        ///     https://github.com/libusb/hidapi/releases/download/hidapi-0.9.0/hidapi-win.zip
        ///     and unzip -j ~/Downloads/hidapi-win.zip hidapi-win/x64/hidapi.dll
        ///     next to this source file.
        ///   </para>
        /// </remarks>
        public const string DllFileName = "hidapi";
    }

    /// <summary>
    ///   Split into partial classes to group all Native Methods together
    ///   while shutting up warnings about order of elements.
    /// </summary>
    public partial class HidApi
    {
        private partial class NativeMethods
        {
            /// <returns>Status code <c>int</c>.</returns>
            [DllImport(HidApi.DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int hid_init();

            /// <returns>Status code <c>int</c>.</returns>
            [DllImport(HidApi.DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int hid_exit();
        }
    }

    /// <summary>
    ///   Split into partial class to group resource management functionality
    ///   while shutting up warnings about order of members.
    /// </summary>
    public partial class HidApi
    {
        private static HidApi singleton = null;

        private HidApi()
        {
            var exit_code = NativeMethods.hid_init();
            if (exit_code != 0)
            {
                throw new InvalidOperationException($"hid_init returned {exit_code}");
            }
        }

        /// <summary>
        ///   Finalizes an instance of the <see cref="HidApi"/> class.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Override finalizer because the constructor allocates unmanaged resources.
        ///   </para>
        /// </remarks>
        ~HidApi()
        {
            var exit_code = NativeMethods.hid_exit();
            if (exit_code != 0)
            {
                // throwing in finalizers is forbidden, and I do not want to
                // figure out logging right now.
                Console.WriteLine($"hid_exit returned {exit_code}");
            }
            else
            {
                HidApi.singleton = null;
            }
        }

        /// <summary>
        ///   Gets singleton object wrapping the initialized hidapi library.
        ///   The library will call hid_exit automatically when it is garbage
        ///   collected.
        /// </summary>
        public static HidApi Library
        {
            get
            {
                if (HidApi.singleton == null)
                {
                    HidApi.singleton = new HidApi();
                }

                return HidApi.singleton;
            }
        }
    }

    /// <summary>
    ///   Partial class grouping HID device enumeration.
    /// </summary>
    public partial class HidApi
    {
        /// <summary>
        ///   Return an enumerator for HID devices with a given vendor and product ID.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This wraps hid_enumerate and needs the library to be
        ///     initialized, which is why the method can't be static.
        ///   </para>
        /// </remarks>
        /// <returns><see cref="HidDeviceInfo"/> collection.</returns>
        /// <param name="vendorId">Vendor ID as <c>unsigned short</c>, 0 is a wildcard.</param>
        /// <param name="productId">Product ID as <c>unsigned short</c>, 0 is a wildcard.</param>
        [SuppressMessage(
                "Microsoft.Performance",
                "CA1822:MarkMembersAsStatic",
                Justification = "Instance method to make sure the hidapi library has been initialized.")]
        public IEnumerable<HidDeviceInfo> Enumerate(ushort vendorId, ushort productId)
        {
            return new HidDeviceCollection(vendorId, productId);
        }

        /// <summary>
        ///   Return an enumerator for all HID devices.
        /// </summary>
        /// <seealso cref="Enumerate(ushort, ushort)"/>
        /// <returns><see cref="HidDeviceInfo"/> collection.</returns>
        [SuppressMessage(
                "Microsoft.Performance",
                "CA1822:MarkMembersAsStatic",
                Justification = "Instance method to make sure the hidapi library has been initialized.")]
        public IEnumerable<HidDeviceInfo> Enumerate()
        {
            // 0 is the wildcard value.
            return new HidDeviceCollection(0, 0);
        }

        /// <summary>
        ///   IEnumerable wrapper for hid_enumerate.
        /// </summary>
        private class HidDeviceCollection : IEnumerable<HidDeviceInfo>
        {
            private ushort vendorId;
            private ushort productId;

            /// <summary>
            ///   Initializes a new instance of the <see cref="HidDeviceCollection"/> class.
            /// </summary>
            /// <param name="vendorId">Vendor ID.</param>
            /// <param name="productId">Prduct ID.</param>
            public HidDeviceCollection(ushort vendorId, ushort productId)
            {
                this.vendorId = vendorId;
                this.productId = productId;
            }

            /// <summary>
            ///   Implement IEnumerable.
            /// </summary>
            public IEnumerator<HidDeviceInfo> GetEnumerator()
            {
                return new HidDeviceEnumerator(this.vendorId, this.productId);
            }

            /// <summary>
            ///   Implement IEnumerable.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator1();
            }

            private IEnumerator GetEnumerator1()
            {
                return this.GetEnumerator();
            }

            /// <summary>
            ///   Enumerator for HID devices.
            /// </summary>
            /// <remarks>
            ///   <para>
            ///     Written following
            ///     https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1?view=netframework-4.8.
            ///   </para>
            /// </remarks>
            private class HidDeviceEnumerator : IDisposable, IEnumerator<HidDeviceInfo>
            {
                private IntPtr first = IntPtr.Zero;

                private IntPtr current = IntPtr.Zero;

                private bool pastEnd = false;

                /// <summary>
                ///   Initializes a new instance of the <see cref="HidDeviceEnumerator"/> class.
                /// </summary>
                public HidDeviceEnumerator(ushort vendorId, ushort productId)
                {
                    this.first = HidApi.NativeMethods.hid_enumerate(vendorId, productId);
                    this.current = IntPtr.Zero;
                    this.pastEnd = false;
                }

                /// <summary>
                ///   Finalizes an instance of the <see cref="HidDeviceEnumerator"/> class.
                /// </summary>
                /// <remarks>
                ///   <para>
                ///     Overrides default finalizer because <see cref="Dispose"/>
                ///     frees unmanaged resources.
                ///   </para>
                /// </remarks>
                ~HidDeviceEnumerator()
                {
                    this.Dispose();
                }

                /// <summary>
                ///   Gets current <see cref="HidDeviceInfo"/>.
                /// </summary>
                /// <remarks>
                ///   <para>
                ///     Implement IEnumerator.
                ///   </para>
                /// </remarks>
                public HidDeviceInfo Current
                {
                    get
                    {
                        if (this.pastEnd)
                        {
                            throw new InvalidOperationException();
                        }

                        var hid = this.UnMarshal(this.current); // can throw InvalidOperationException, too
                        return new HidDeviceInfo(
                            hid.path,
                            hid.vendor_id,
                            hid.product_id,
                            hid.serial_number,
                            hid.release_number,
                            hid.manufacturer_string,
                            hid.product_string,
                            hid.usage_page,
                            hid.usage,
                            hid.interface_number);
                    }
                }

                /// <summary>
                ///   Gets current <see cref="HidDeviceInfo"/>.
                /// </summary>
                /// <remarks>
                ///   <para>
                ///     Implement IEnumerator.
                ///   </para>
                /// </remarks>
                object IEnumerator.Current => this.Current1;

                private object Current1 => this.Current;

                /// <summary>
                ///   Implement IDisposable.
                /// </summary>
                public void Dispose()
                {
                    this.current = IntPtr.Zero;
                    this.pastEnd = false;
                    if (this.first != IntPtr.Zero)
                    {
                        HidApi.NativeMethods.hid_free_enumeration(this.first);
                    }

                    this.first = IntPtr.Zero;

                    // make CA1816 happy, personally I did not mind that Dispose
                    // would be called a second time as NO-OP:
                    GC.SuppressFinalize(this);
                }

                /// <summary>
                ///   Implement IEnumerator.
                /// </summary>
                public bool MoveNext()
                {
                    if (this.pastEnd)
                    {
                        return false;
                    }

                    if (this.current == IntPtr.Zero)
                    {
                        // starting to enumerate devices
                        if (this.first == IntPtr.Zero)
                        {
                            return false; // no devices at all
                        }
                        else
                        {
                            // move from before any device to first
                            this.current = this.first;
                            return true;
                        }
                    }
                    else
                    {
                        // inside enumeration, advance
                        var hid = this.UnMarshal(this.current);
                        this.current = hid.next;
                        this.pastEnd = this.current == IntPtr.Zero;
                        return !this.pastEnd;
                    }
                }

                /// <summary>
                ///   Implement IEnumerator.
                /// </summary>
                public void Reset()
                {
                    this.current = IntPtr.Zero;
                    this.pastEnd = false;
                }

                private hid_device_info UnMarshal(IntPtr p)
                {
                    if (p == IntPtr.Zero)
                    {
                        throw new InvalidOperationException();
                    }

                    return (hid_device_info)Marshal.PtrToStructure(p, typeof(hid_device_info));
                }

                /** hidapi info structure */
                [SuppressMessage(
                        "StyleCop.CSharp.NamingRules",
                        "SA1300:ElementMustBeginWithUpperCaseLetter",
                        Justification = "Definition matches hidapi source as closely as possible.")]
                [SuppressMessage(
                        "StyleCop.CSharp.NamingRules",
                        "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter",
                        Justification = "Definition matches hidapi source as closely as possible.")]
                [SuppressMessage(
                        "StyleCop.CSharp.NamingRules",
                        "SA1310:FieldNamesMustNotContainUnderscore",
                        Justification = "Definition matches hidapi source as closely as possible.")]
                [StructLayout(LayoutKind.Sequential)]
                private struct hid_device_info
                {
                    /** Platform-specific device path */
                    [MarshalAs(UnmanagedType.LPStr)]
                    public string path;
                    /** Device Vendor ID */
                    public ushort vendor_id;
                    /** Device Product ID */
                    public ushort product_id;
                    /** Serial Number */
                    [MarshalAs(UnmanagedType.LPWStr)]
                    public string serial_number;
                    /** Device Release Number in binary-coded decimal,
                        also known as Device Version Number */
                    public ushort release_number;
                    /** Manufacturer String */
                    [MarshalAs(UnmanagedType.LPWStr)]
                    public string manufacturer_string;
                    /** Product string */
                    [MarshalAs(UnmanagedType.LPWStr)]
                    public string product_string;
                    /** Usage Page for this Device/Interface
                        (Windows/Mac only). */
                    public ushort usage_page;
                    /** Usage for this Device/Interface
                        (Windows/Mac only).*/
                    public ushort usage;
                    /** The USB interface which this logical device
                        represents. Valid on both Linux implementations
                        in all cases, and valid on the Windows implementation
                        only if the device contains more than one interface. */
                    public int interface_number;

                    /** Pointer to the next device */
                    public IntPtr next;
                }
            }
        }

        private partial class NativeMethods
        {
            /// <returns><c>hid_device_info*</c>.</returns>
            /// <param name="vendor_id">Vendor ID as <c>unsigned short</c>.</param>
            /// <param name="product_id">Product ID as <c>unsigned short</c>.</param>
            [DllImport(HidApi.DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr hid_enumerate(ushort vendor_id, ushort product_id);

            /// <summary>
            ///   Cleans up resources allocated by <see cref="hid_enumerate"/>.
            /// </summary>
            /// <param name="devs"><c>struct hid_device_info*</c>.</param>
            [DllImport(HidApi.DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void hid_free_enumeration(IntPtr devs);
        }
    }

    /// <summary>
    ///   Partial class grouping HID device opening, reading/writing, closing.
    /// </summary>
    public partial class HidApi
    {
        /// <summary>
        ///   Open HID device for reading/writing.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This call wraps <see cref="NativeMethods.hid_open"/>.
        ///   </para>
        /// </remarks>
        /// <returns><see cref="IHidHandle"/>.</returns>
        /// <param name="vendorId">Vendor ID as <c>ushort</c>.</param>
        /// <param name="productId">Product ID as <c>ushort</c>.</param>
        /// <param name="serialNumber">Serial number as <c>string</c>.</param>
        [SuppressMessage(
                "Microsoft.Performance",
                "CA1822:MarkMembersAsStatic",
                Justification = "Using instance makes sure the hidapi library has been initialized.")]
        public IHidHandle Open(ushort vendorId, ushort productId, string serialNumber)
        {
            return new HidHandle(vendorId, productId, serialNumber);
        }

        /// <summary>
        ///   Open HID device for reading/writing.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     This call wraps <see cref="NativeMethods.hid_open_path"/>.
        ///   </para>
        /// </remarks>
        /// <returns><see cref="IHidHandle"/>.</returns>
        /// <param name="path">OS dependent path for device as <c>string</c>.</param>
        [SuppressMessage(
                "Microsoft.Performance",
                "CA1822:MarkMembersAsStatic",
                Justification = "Using instance makes sure the hidapi library has been initialized.")]
        public IHidHandle Open(string path)
        {
            return new HidHandle(path);
        }

        /// <summary>
        ///   Group HID specific methods.
        /// </summary>
        private partial class HidHandle : IHidHandle
        {
            /// <summary>
            ///   Query Manufacturer from HID device.
            /// </summary>
            /// <returns><c>string</c>.</returns>
            public string GetManufacturerString()
            {
                if (this.handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Cannot get manufacturer string before opening");
                }
                else
                {
                    var bufString = new StringBuilder(100);
                    var exitCode = NativeMethods.hid_get_manufacturer_string(
                        this.handle,
                        bufString,
                        Convert.ToUInt16(bufString.Capacity));
                    if (exitCode == 0)
                    {
                        return bufString.ToString();
                    }
                    else
                    {
                        throw new InvalidOperationException($"hid_get_manufacturer_string returned {exitCode}");
                    }
                }
            }

            /// <summary>
            ///   Query Product String from HID device.
            /// </summary>
            /// <returns><c>string</c>.</returns>
            public string GetProductString()
            {
                if (this.handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Cannot get product string before opening");
                }
                else
                {
                    var bufString = new StringBuilder(100);
                    var exitCode = NativeMethods.hid_get_product_string(
                        this.handle,
                        bufString,
                        Convert.ToUInt16(bufString.Capacity));
                    if (exitCode == 0)
                    {
                        return bufString.ToString();
                    }
                    else
                    {
                        throw new InvalidOperationException($"hid_get_product_string returned {exitCode}");
                    }
                }
            }

            /// <summary>
            ///   Query Serial Number from HID device.
            /// </summary>
            /// <returns><c>string</c>.</returns>
            public string GetSerialNumber()
            {
                if (this.handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Cannot get serial number before opening");
                }
                else
                {
                    var bufString = new StringBuilder(100);
                    var exitCode = NativeMethods.hid_get_serial_number_string(
                        this.handle,
                        bufString,
                        Convert.ToUInt16(bufString.Capacity));
                    if (exitCode == 0)
                    {
                        return bufString.ToString();
                    }
                    else
                    {
                        throw new InvalidOperationException($"hid_get_serial_number_string returned {exitCode}");
                    }
                }
            }

            /// <summary>
            ///   Wrapper for <see cref="NativeMethods.hid_read"/>.
            /// </summary>
            /// <returns><c>byte[]</c> of data read.</returns>
            /// <param name="maxBytes">how many bytes to read.</param>
            public byte[] Read(ushort maxBytes)
            {
                if (this.handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Can only read from open HidHandle");
                }
                else
                {
                    var buffer = new byte[maxBytes + 1];
                    var bytesRead = NativeMethods.hid_read(this.handle, buffer, maxBytes);
                    if (bytesRead < 0)
                    {
                        throw new InvalidOperationException(
                            $"Error code {bytesRead} trying to read {maxBytes} bytes");
                    }
                    else
                    {
                        Array.Resize(ref buffer, bytesRead);
                        return buffer;
                    }
                }
            }

            /// <summary>
            ///   Wrapper for <see cref="NativeMethods.hid_read_timeout"/>.
            /// </summary>
            /// <returns><c>byte[]</c> of data read.</returns>
            /// <param name="maxBytes">how many bytes to read.</param>
            /// <param name="timeout">timeout in seconds.</param>
            public byte[] Read(ushort maxBytes, double timeout)
            {
                if (this.handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Can only read from open HidHandle");
                }
                else
                {
                    var buffer = new byte[maxBytes + 1];
                    var bytesRead = NativeMethods.hid_read_timeout(
                        this.handle, buffer, maxBytes, Convert.ToInt32(1000.0 * timeout));
                    if (bytesRead < 0)
                    {
                        throw new InvalidOperationException(
                            $"Error code {bytesRead} trying to read {maxBytes} bytes");
                    }
                    else
                    {
                        Array.Resize(ref buffer, bytesRead);
                        return buffer;
                    }
                }
            }

            /// <summary>
            ///   Wrapper for <see cref="NativeMethods.hid_write"/>.
            /// </summary>
            /// <returns>how many bytes written.</returns>
            /// <param name="data"><c>byte[]</c> of bytes to write.</param>
            public int Write(byte[] data)
            {
                if (this.handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Can only write to open HidHandle");
                }
                else
                {
                    var bytesWritten = NativeMethods.hid_write(this.handle, data, Convert.ToUInt16(data.Length));
                    if (bytesWritten < data.Length)
                    {
                        throw new InvalidOperationException(
                            $"Wrote {bytesWritten}, expected to write {data.Length} bytes");
                    }
                    else
                    {
                        return bytesWritten;
                    }
                }
            }
        }

        /// <summary>
        ///   IDisposable Support.
        /// </summary>
        private partial class HidHandle : IHidHandle
        {
            private IntPtr handle = IntPtr.Zero;
            private bool disposedValue = false; // To detect redundant calls

            /// <summary>
            ///   Initializes a new instance of the <see cref="HidHandle"/> class.
            /// </summary>
            /// <remarks>
            ///   <para>
            ///     Open HID device with <see cref="NativeMethods.hid_open"/>.
            ///   </para>
            /// </remarks>
            /// <returns><see cref="IHidHandle"/>.</returns>
            /// <param name="vendorId">Vendor ID as <c>ushort</c>.</param>
            /// <param name="productId">Product ID as <c>ushort</c>.</param>
            /// <param name="serialNumber">Serial number as <c>string</c>.</param>
            public HidHandle(ushort vendorId, ushort productId, string serialNumber)
            {
                this.handle = NativeMethods.hid_open(vendorId, productId, serialNumber);
                if (this.handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(
                        $"Could not hid_open({vendorId:X}, {productId:X}, {Repr(serialNumber)})");
                }
            }

            /// <summary>
            ///   Initializes a new instance of the <see cref="HidHandle"/> class.
            /// </summary>
            /// <returns><see cref="IHidHandle"/>.</returns>
            /// <param name="path">
            ///   OS dependent path for device as <c>string</c>, <see cref="NativeMethods.hid_open_path"/>.
            /// </param>
            public HidHandle(string path)
            {
                this.handle = NativeMethods.hid_open_path(path);
                if (this.handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException($"Could not hid_open_path({Repr(path)})");
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            ~HidHandle()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                this.Dispose(false);
            }

            /// <summary>
            ///   This code added to correctly implement the disposable pattern.
            /// </summary>
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                this.Dispose(true);

                // uncommented the following line because the finalizer is overridden.
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposedValue)
                {
                    // no managed objects to free, only the hid_device handle
                    if (this.handle != IntPtr.Zero)
                    {
                        NativeMethods.hid_close(this.handle);
                        this.handle = IntPtr.Zero;
                    }

                    this.disposedValue = true;
                }
            }

            private static string Repr(string input)
            {
                if (input == null)
                {
                    return "null";
                }
                else
                {
                    return $"\"{input}\"";
                }
            }
        }

        private partial class NativeMethods
        {
            /// <summary>
            ///   Open HID device for reading/writing.
            /// </summary>
            /// <returns><c>hid_device*</c>.</returns>
            /// <param name="vendor_id">Vendor ID as <c>unsigned short</c>.</param>
            /// <param name="product_id">Product ID as <c>unsigned short</c>.</param>
            /// <param name="serial_number"><c>wchar_t*</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern IntPtr hid_open(ushort vendor_id, ushort product_id, [In] string serial_number);

            /// <summary>
            ///   Open HID device for reading/writing.
            /// </summary>
            /// <returns><c>hid_device*</c>.</returns>
            /// <param name="path"><c>char*</c>.</param>
            [DllImport(
                    DllFileName,
                    CallingConvention = CallingConvention.Cdecl,
                    CharSet = CharSet.Ansi,
                    BestFitMapping = false,
                    ThrowOnUnmappableChar = true)]
            public static extern IntPtr hid_open_path([In] string path);

            /// <summary>
            ///   Write <c>byte[]</c> array to HID device.
            /// </summary>
            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="data"><c>unsigned char*</c>.</param>
            /// <param name="length"><c>size_t-&gt;unsigned int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int hid_write(IntPtr device, [In] byte[] data, uint length);

            /// <summary>
            ///   Read <c>byte[]</c> array from HID device with a timeout.
            /// </summary>
            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="buf_data"><c>unsigned char*</c>.</param>
            /// <param name="length"><c>size_t-&gt;unsigned int</c>.</param>
            /// <param name="milliseconds"><c>int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int hid_read_timeout(IntPtr device, [Out] byte[] buf_data, uint length, int milliseconds);

            /// <summary>
            ///   Blocking read <c>byte[]</c> array from HID device.
            /// </summary>
            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="buf_data"><c>unsigned char*</c>.</param>
            /// <param name="length"><c>size_t-&gt;unsigned int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int hid_read(IntPtr device, [Out] byte[] buf_data, uint length);

            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="nonblock"><c>int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int hid_set_nonblocking(IntPtr device, int nonblock);

            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="data"><c>char*</c>.</param>
            /// <param name="length"><c>size_t-&gt;unsigned int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int hid_send_feature_report(IntPtr device, [In] byte[] data, uint length);

            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="buf_data"><c>unsigned char*</c>.</param>
            /// <param name="length"><c>size_t-&gt;unsigned int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern int hid_get_feature_report(IntPtr device, [Out] byte[] buf_data, uint length);

            /// <param name="device"><c>hid_device*</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
            public static extern void hid_close(IntPtr device);

            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="buf_string"><c>wchar_t*</c>.</param>
            /// <param name="length">String capacity (was <c>size_t</c>) <c>unsigned int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern int hid_get_manufacturer_string(IntPtr device, StringBuilder buf_string, uint length);

            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="buf_string"><c>wchar_t*</c>.</param>
            /// <param name="length">String capacity (was <c>size_t</c>) <c>unsigned int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern int hid_get_product_string(IntPtr device, StringBuilder buf_string, uint length);

            /// <returns>Status code <c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="buf_serial"><c>wchar_t*</c>.</param>
            /// <param name="maxlen">String capacity (was <c>size_t</c>) <c>unsigned int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern int hid_get_serial_number_string(IntPtr device, StringBuilder buf_serial, uint maxlen);

            /// <returns><c>int</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            /// <param name="string_index">String index as <c>int</c>.</param>
            /// <param name="buf_string"><c>wchar_t*</c> marshaled into a <c>StringBuilder</c>.</param>
            /// <param name="maxlen">String capacity (was <c>size_t</c>) <c>unsigned int</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            public static extern int hid_get_indexed_string(IntPtr device, int string_index, StringBuilder buf_string, uint maxlen);

            /// <returns><c>wchar_t*</c>.</returns>
            /// <param name="device"><c>hid_device*</c>.</param>
            [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
            public static extern IntPtr hid_error(IntPtr device);
        }
    }
}
