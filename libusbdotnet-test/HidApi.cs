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

        /// Return Type: int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_init();

        /// Return Type: int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_exit();

        /// Return Type: hid_device_info*
        ///vendor_id: unsigned short
        ///product_id: unsigned short
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr hid_enumerate(ushort vendor_id, ushort product_id);

        /// Return Type: void
        ///devs: struct hid_device_info*
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void hid_free_enumeration(IntPtr devs);

        /// Return Type: hid_device*
        ///vendor_id: unsigned short
        ///product_id: unsigned short
        ///serial_number: wchar_t*
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr hid_open(ushort vendor_id, ushort product_id, [In] string serial_number);

        /// Return Type: hid_device*
        ///path: char*
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr hid_open_path([In] string path);

        /// Return Type: int
        ///device: hid_device*
        ///data: unsigned char*
        ///length: size_t->unsigned int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_write(IntPtr device, [In] byte[] data, uint length);

        /// Return Type: int
        ///dev: hid_device*
        ///data: unsigned char*
        ///length: size_t->unsigned int
        ///milliseconds: int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_read_timeout(IntPtr device, [Out] byte[] buf_data, uint length, int milliseconds);

        /// Return Type: int
        ///device: hid_device*
        ///data: unsigned char*
        ///length: size_t->unsigned int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_read(IntPtr device, [Out] byte[] buf_data, uint length);

        /// Return Type: int
        ///device: hid_device*
        ///nonblock: int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_set_nonblocking(IntPtr device, int nonblock);

        /// Return Type: int
        ///device: hid_device*
        ///data: char*
        ///length: size_t->unsigned int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_send_feature_report(IntPtr device, [In] byte[] data, uint length);

        /// Return Type: int
        ///device: hid_device*
        ///data: unsigned char*
        ///length: size_t->unsigned int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int hid_get_feature_report(IntPtr device, [Out] byte[] buf_data, uint length);

        /// Return Type: void
        ///device: hid_device*
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void hid_close(IntPtr device);

        /// Return Type: int
        ///device: hid_device*
        ///string: wchar_t*
        ///maxlen: size_t->unsigned int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int hid_get_manufacturer_string(IntPtr device, StringBuilder buf_string, uint length);

        /// Return Type: int
        ///device: hid_device*
        ///string: wchar_t*
        ///maxlen: size_t->unsigned int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int hid_get_product_string(IntPtr device, StringBuilder buf_string, uint length);

        /// Return Type: int
        ///device: hid_device*
        ///string: wchar_t*
        ///maxlen: size_t->unsigned int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int hid_get_serial_number_string(IntPtr device, StringBuilder buf_serial, uint maxlen);

        /// Return Type: int
        ///device: hid_device*
        ///string_index: int
        ///string: wchar_t*
        ///maxlen: size_t->unsigned int
        [DllImport(DllFileName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern int hid_get_indexed_string(IntPtr device, int string_index, StringBuilder buf_string, uint maxlen);

        /// Return Type: wchar_t*
        ///device: hid_device*
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
