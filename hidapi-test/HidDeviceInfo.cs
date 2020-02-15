// <copyright file="HidDeviceInfo.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace HidapiTest
{
    /// <summary>
    ///   Data object with read-only information about HID device.
    /// </summary>
    public class HidDeviceInfo
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="HidDeviceInfo"/> class.
        /// </summary>
        /// <param name="path">Platform-specific device path as <c>string</c>.</param>
        /// <param name="vendorId">Device Vendor ID as <c>ushort</c>.</param>
        /// <param name="productId">Device Product ID as <c>ushort</c>.</param>
        /// <param name="serialNumber">Serial Number as <c>string</c>.</param>
        /// <param name="releaseNumber">Device Release Number in binary-coded decimal <c>ushort</c>.</param>
        /// <param name="manufacturerString">Manufacturer String as <c>string</c>.</param>
        /// <param name="productString">Product string as <c>string</c>.</param>
        /// <param name="usagePage">Usage Page for this Device/Interface (Windows/Mac only) as <c>ushort</c>.</param>
        /// <param name="usage">Usage for this Device/Interface (Windows/Mac only) as <c>ushort</c>.</param>
        /// <param name="interfaceNumber">The USB interface which this logical device as <c>int</c>.</param>
        public HidDeviceInfo(
            string path,
            ushort vendorId,
            ushort productId,
            string serialNumber,
            ushort releaseNumber,
            string manufacturerString,
            string productString,
            ushort usagePage,
            ushort usage,
            int interfaceNumber)
        {
            this.Path = path;
            this.VendorId = vendorId;
            this.ProductId = productId;
            this.SerialNumber = serialNumber;
            this.ReleaseNumber = releaseNumber;
            this.ManufacturerString = manufacturerString;
            this.ProductString = productString;
            this.UsagePage = usagePage;
            this.Usage = usage;
            this.InterfaceNumber = interfaceNumber;
        }

        /// <summary>
        ///   Gets platform-specific device path.
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///   Gets Device Vendor ID.
        /// </summary>
        public ushort VendorId { get; }

        /// <summary>
        ///   Gets Device Product ID.
        /// </summary>
        public ushort ProductId { get; }

        /// <summary>
        ///   Gets Serial Number.
        /// </summary>
        public string SerialNumber { get; }

        /// <summary>
        ///   Gets Device Release Number in binary-coded decimal, also known as
        ///   Device Version Number.
        /// </summary>
        public ushort ReleaseNumber { get; }

        /// <summary>
        ///   Gets Manufacturer String.
        /// </summary>
        public string ManufacturerString { get; }

        /// <summary>
        ///   Gets Product string.
        /// </summary>
        public string ProductString { get; }

        /// <summary>
        ///   Gets Usage Page for this Device/Interface (Windows/Mac only).
        /// </summary>
        public ushort UsagePage { get; }

        /// <summary>
        ///   Gets Usage for this Device/Interface (Windows/Mac only).
        /// </summary>
        public ushort Usage { get; }

        /// <summary>
        ///   Gets the USB interface which this logical device
        ///   represents. Valid on both Linux implementations
        ///   in all cases, and valid on the Windows implementation
        ///   only if the device contains more than one interface.
        /// </summary>
        public int InterfaceNumber { get; }
    }
}
