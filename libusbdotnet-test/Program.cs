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
        private class VendorProductIdPair {
            public int VendorId { get; private set; }
            public int ProductId { get; private set; }
            public VendorProductIdPair(int vendor, int product)
            {
                this.VendorId = vendor;
                this.ProductId = product;
            }
        }

        private static VendorProductIdPair[] DeviceIds = {
            new VendorProductIdPair(vendor: 0x16c0, product: 0x0486),
            new VendorProductIdPair(vendor: 0x1d50, product: 0x60fc)
        };

        private static bool IdentifyOnlyKey(IUsbDevice d)
        {
            foreach (var p in DeviceIds)
            {
                if ((p.VendorId == d.VendorId) && (p.ProductId == d.ProductId))
                {
                    var info = d.Info;
                    return info.SerialNumber == "1000000000";
                }
            }
            return false;
        }

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
                var selectedDevice = usbDeviceCollection.FirstOrDefault(IdentifyOnlyKey);
                if (selectedDevice == null)
                {
                    Console.WriteLine("Ohoh, selectedDevice is null");
                    return;
                }
                else
                {
                    Console.WriteLine(
                        "selectedDevice PID={0:X4} VID={1:X4} found",
                        selectedDevice.ProductId,
                        selectedDevice.VendorId);
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
