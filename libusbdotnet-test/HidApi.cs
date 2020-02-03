// <copyright file="HidApi.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;
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

        /** hidapi info structure */
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct hid_device_info
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
