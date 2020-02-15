// <copyright file="IHidHandle.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace HidapiTest
{
    using System;

    /// <summary>
    ///   Interface describing features of HID devices opened with
    ///   <see cref="HidApi.NativeMethods.hid_open"/> or
    ///   <see cref="HidApi.NativeMethods.hid_open_path"/>.
    /// </summary>
    public interface IHidHandle : IDisposable
    {
        /// <summary>
        ///   Blocking write <c>byte[]</c> to HID device.
        /// </summary>
        /// <returns>Number of bytes written.</returns>
        /// <param name="data"><c>byte[]</c> to send to HID device.</param>
        public int Write(byte[] data);

        /// <summary>
        ///   Blocking read <c>byte[]</c> from HID device.
        /// </summary>
        /// <returns>Number of bytes read.</returns>
        /// <param name="maxBytes">how many bytes to read.</param>
        public byte[] Read(ushort maxBytes);

        /// <summary>
        ///   Blocking read <c>byte[]</c> from HID device with a timeout.
        /// </summary>
        /// <returns>Number of bytes read.</returns>
        /// <param name="maxBytes">how many bytes to read.</param>
        /// <param name="timeout">how many seconds to wait.</param>
        public byte[] Read(ushort maxBytes, double timeout);

        /// <summary>
        ///   Query Manufacturer from HID device.
        /// </summary>
        /// <returns><c>string</c>.</returns>
        public string GetManufacturerString();

        /// <summary>
        ///   Query Product String from HID device.
        /// </summary>
        /// <returns><c>string</c>.</returns>
        public string GetProductString();

        /// <summary>
        ///   Query Serial Number from HID device.
        /// </summary>
        /// <returns><c>string</c>.</returns>
        public string GetSerialNumber();
    }
}
