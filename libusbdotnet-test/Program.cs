// <copyright file="Program.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;
    using System.Linq;

    using LibUsbDotNet;
    using LibUsbDotNet.LibUsb;
    using LibUsbDotNet.Main;

    internal static class Program
    {
        // My mouse's product ID:
        private const int ProductId = 0xc077;

        // My mouse's vendor ID:
        private const int VendorId = 0x046d;

        public static void Main(string[] args)
        {
            using (var context = new UsbContext())
            {
                context.SetDebugLevel(LogLevel.Info);

                // Get a list of all connected devices
                var usbDeviceCollection = context.List();
                if (usbDeviceCollection == null)
                {
                    Console.WriteLine("Ohoh, usbDeviceCollection=null");
                }
                else
                {
                    Console.WriteLine("Found {0} devices", usbDeviceCollection.Count);
                    foreach (var u in usbDeviceCollection)
                    {
                        Console.Write("  P={0:X4} V={1:X4} ", u.ProductId, u.VendorId);
                        var opened = u.TryOpen();
                        if (opened)
                        {
                            Console.WriteLine("is a '{0}' by '{1}'", u.Info.Product, u.Info.Manufacturer);
                            u.Close();
                        }
                        else
                        {
                            Console.WriteLine("ohoh");
                        }
                    }
                }

                // Narrow down the device by vendor and pid
                var selectedDevice = usbDeviceCollection.FirstOrDefault(d => d.ProductId == ProductId && d.VendorId == VendorId);
                if (selectedDevice == null)
                {
                    Console.WriteLine("Ohoh, selectedDevice PID={0:X4} VID={1:X4} is null", ProductId, VendorId);
                }
                else
                {
                    Console.WriteLine("selectedDevice PID={0:X4} VID={1:X4} found", ProductId, VendorId);
                }

                // Open the device
                selectedDevice.Open();

                // Get the first config number of the interface
                selectedDevice.ClaimInterface(selectedDevice.Configs[0].Interfaces[0].Number);

                // Open up the endpoints
                var writeEndpoint = selectedDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                var readEnpoint = selectedDevice.OpenEndpointReader(ReadEndpointID.Ep01);

                // Create a buffer with some data in it
                var buffer = new byte[64];
                buffer[0] = 0x3f;
                buffer[1] = 0x23;
                buffer[2] = 0x23;

                // Write three bytes
                writeEndpoint.Write(buffer, 3000, out var bytesWritten);

                var readBuffer = new byte[64];

                // Read some data
                readEnpoint.Read(readBuffer, 3000, out var readBytes);

                Console.WriteLine("{0} bytes written, {1} bytes read", bytesWritten, readBytes);
            }
        }
    }
}
