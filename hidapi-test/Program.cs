// <copyright file="Program.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace HidapiTest
{
    // See https://stackoverflow.com/a/957544 for how to access linked list
    using System;
    using System.Text;

    internal static class Program
    {
        private static VendorProductIdPair[] deviceIds =
        {
            new VendorProductIdPair(vendor: 0x16c0, product: 0x0486),
            new VendorProductIdPair(vendor: 0x1d50, product: 0x60fc),
        };

        public static void Main()
        {
            var hidApi = HidApi.Library;
            Console.WriteLine("Success opening HidApi");
            using (var onlykey = new HardwareOnlyKey(hidApi))
            {
                var di = onlykey?.DeviceInfo;
                if (di != null)
                {
                    Console.WriteLine(
                        "path = {0}\n  vendor_id={1:X4} product_id={2:X4}\n  serial_number={3}\n  manufacturer={4}\n  product={5}\n  usage_page={6:X4}  usage={7:X4}\n  interface_number={8}",
                        di.Path,
                        di.VendorId,
                        di.ProductId,
                        di.SerialNumber,
                        di.ManufacturerString,
                        di.ProductString,
                        di.UsagePage,
                        di.Usage,
                        di.InterfaceNumber);
                }

                Console.WriteLine("---");
                foreach (var s in onlykey.SlotLabels())
                {
                    Console.WriteLine(s == null ? "<null>" : ("'" + s + "'"));
                }
            }

            DoSomethingWithHid();
        }

        private static void DoSomethingWithHid()
        {
            IHidHandle hid1 = null;
            try
            {
                try
                {
                    hid1 = HidApi.Library.Open(deviceIds[0].VendorId, deviceIds[0].ProductId, null);
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"Could not open once ({e.Message}), retrying");
                    try
                    {
                        hid1 = HidApi.Library.Open(deviceIds[1].VendorId, deviceIds[1].ProductId, null);
                    }
                    catch (InvalidOperationException f)
                    {
                        Console.WriteLine($"No device found ({f.Message}), sorry.");
                        return;
                    }
                }

                if (hid1 == null)
                {
                    Console.WriteLine("No device found, sorry, should not be reached.");
                    return;
                }

                try
                {
                    Console.WriteLine("Manufacturer = {0}", hid1.GetManufacturerString());
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"Couldn't get manufacturer info: {e.Message}");
                }

                try
                {
                    Console.WriteLine("Product = {0}", hid1.GetProductString());
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"Couldn't get product info: {e.Message}");
                }

                try
                {
                    Console.WriteLine("Serial_number = {0}", hid1.GetSerialNumber());
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"Couldn't get serial number: {e.Message}");
                }
            }
            finally
            {
                // NB: normally I should be using the `using' keyword and the
                // hid1.Dispose would be implicit...
                if (hid1 != null)
                {
                    hid1.Dispose();
                    hid1 = null;
                    Console.WriteLine("Closed device");
                }
            }
        }

        private class VendorProductIdPair
        {
            public VendorProductIdPair(ushort vendor, ushort product)
            {
                this.VendorId = vendor;
                this.ProductId = product;
            }

            public ushort VendorId { get; private set; }

            public ushort ProductId { get; private set; }
        }
    }
}
