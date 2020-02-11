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
    internal class HidApi
    {
        // On windows for system installed: hidapi.dll
        // On linux for system installed: "libhidapi-hidraw" or "libhidapi-libusb"
        // unfortunately there is no way simple to automatically
        // find the library on all platforms because of different
        // naming conventions.
        // Just use hidapi and expect users to supply it in same folder as .exe
        // For development purposes, I downloaded the release from
        // https://github.com/libusb/hidapi/releases/download/hidapi-0.9.0/hidapi-win.zip
        // and unzip -j ~/Downloads/hidapi-win.zip hidapi-win/x64/hidapi.dll
        // next to this source file
        public const string DllFileName = "hidapi";

        /// <returns>Status code <c>int</c>.</returns>
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_init();

        /// <returns>Status code <c>int</c>.</returns>
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_exit();

        /// <returns><c>hid_device_info*</c>.</returns>
        /// <param name="vendor_id">Vendor ID as <c>unsigned short</c>.</param>
        /// <param name="product_id">Product ID as <c>unsigned short</c>.</param>
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr hid_enumerate(ushort vendor_id, ushort product_id);

        /// <param name="devs"><c>struct hid_device_info*</c>.</param>
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void hid_free_enumeration(IntPtr devs);

        /// <returns><c>hid_device*</c>.</returns>
        /// <param name="vendor_id">Vendor ID as <c>unsigned short</c>.</param>
        /// <param name="product_id">Product ID as <c>unsigned short</c>.</param>
        /// <param name="serial_number"><c>wchar_t*</c>.</param>
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern IntPtr hid_open(ushort vendor_id, ushort product_id, [In] string serial_number);

        /* I do not intend to use this DLL function and can't find an easy way
         * to fix the warning CA2101: Specify marshaling for P/Invoke string
         * arguments, so I simply comment out that function.

        /// <returns><c>hid_device*</c>.</returns>
        /// <param name="path"><c>char*</c>.</param>
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr hid_open_path([In] string path);
        */

        /// <returns>Status code <c>int</c>.</returns>
        /// <param name="device"><c>hid_device*</c>.</param>
        /// <param name="data"><c>unsigned char*</c>.</param>
        /// <param name="length"><c>size_t-&gt;unsigned int</c>.</param>
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_write(IntPtr device, [In] byte[] data, uint length);

        /// <returns>Status code <c>int</c>.</returns>
        /// <param name="device"><c>hid_device*</c>.</param>
        /// <param name="buf_data"><c>unsigned char*</c>.</param>
        /// <param name="length"><c>size_t-&gt;unsigned int</c>.</param>
        /// <param name="milliseconds"><c>int</c>.</param>
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_read_timeout(IntPtr device, [Out] byte[] buf_data, uint length, int milliseconds);

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

        public class HidDeviceInfo
        {
            public HidDeviceInfo(
                string path,
                ushort vendor_id,
                ushort product_id,
                string serial_number,
                ushort release_number,
                string manufacturer_string,
                string product_string,
                ushort usage_page,
                ushort usage,
                int interface_number)
            {
                this.path = path;
                this.vendor_id = vendor_id;
                this.product_id = product_id;
                this.serial_number = serial_number;
                this.release_number = release_number;
                this.manufacturer_string = manufacturer_string;
                this.product_string = product_string;
                this.usage_page = usage_page;
                this.usage = usage;
                this.interface_number = interface_number;
            }

            /** Platform-specific device path */
            public string path { get; }
            /** Device Vendor ID */
            public ushort vendor_id { get; }
            /** Device Product ID */
            public ushort product_id { get; }
            /** Serial Number */
            public string serial_number { get; }
            /** Device Release Number in binary-coded decimal,
                also known as Device Version Number */
            public ushort release_number { get; }
            /** Manufacturer String */
            public string manufacturer_string { get; }
            /** Product string */
            public string product_string { get; }
            /** Usage Page for this Device/Interface
                (Windows/Mac only). */
            public ushort usage_page { get; }
            /** Usage for this Device/Interface
                (Windows/Mac only).*/
            public ushort usage { get; }
            /** The USB interface which this logical device
                represents. Valid on both Linux implementations
                in all cases, and valid on the Windows implementation
                only if the device contains more than one interface. */
            public int interface_number { get; }
        }

        public class HidDeviceEnumerable : IEnumerable<HidDeviceInfo>
        {
            private ushort vendor_id;
            private ushort product_id;

            public HidDeviceEnumerable(ushort vendor_id, ushort product_id)
            {
                this.vendor_id = vendor_id;
                this.product_id = product_id;
            }

            public IEnumerator<HidDeviceInfo> GetEnumerator()
            {
                return new HidDeviceEnumerator(this.vendor_id, this.product_id);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator1();
            }

            private IEnumerator GetEnumerator1()
            {
                return this.GetEnumerator();
            }
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
        public class HidDeviceEnumerator : IDisposable, IEnumerator<HidDeviceInfo>
        {
            private IntPtr first = IntPtr.Zero;

            private IntPtr current = IntPtr.Zero;

            private bool pastEnd = false;

            public HidDeviceEnumerator(ushort vendor_id, ushort product_id)
            {
                this.first = hid_enumerate(vendor_id, product_id);
                this.current = IntPtr.Zero;
                this.pastEnd = false;
            }

            ~HidDeviceEnumerator()
            {
                this.Dispose();
            }

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

            object IEnumerator.Current => this.Current1;

            private object Current1 => this.Current;

            public void Dispose()
            {
                if (this.first != IntPtr.Zero)
                {
                    this.current = IntPtr.Zero;
                    this.pastEnd = false;
                    hid_free_enumeration(this.first);
                    this.first = IntPtr.Zero;
                }

                // make CA1816 happy, personally I did not mind that Dispose
                // would be called a second time as NO-OP:
                GC.SuppressFinalize(this);
            }

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
}
