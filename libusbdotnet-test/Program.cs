// <copyright file="Program.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;
    using System.Text;

    internal static class Program
    {
        private static VendorProductIdPair[] deviceIds =
        {
            new VendorProductIdPair(vendor: 0x16c0, product: 0x0486),
            new VendorProductIdPair(vendor: 0x1d50, product: 0x60fc),
        };

        public static void Main(string[] args)
        {
            string[] testSlots = { "a", "b" };
            using (var onlyKey = new HardwareOnlyKey())
            {
                foreach (var s in onlyKey.SlotNames())
                {
                    Console.WriteLine(s);
                }
            }

            if (HidApi.hid_init() == 0)
            {
                Console.WriteLine("Success opening HidApi");
                try
                {
                    DoSomethingWithHid();
                }
                finally
                {
                    if (HidApi.hid_exit() == 0)
                    {
                        Console.WriteLine("Success closing HidApi");
                    }
                    else
                    {
                        Console.WriteLine("Could not exit HidApi properly");
                    }
                }
            }
            else
            {
                Console.WriteLine("Abysmal failure");
            }
        }

        private static void DoSomethingWithHid()
        {
            var hid1 = HidApi.hid_open(deviceIds[0].VendorId, deviceIds[0].ProductId, null);
            if (hid1 == IntPtr.Zero)
            {
                Console.WriteLine("Could not open once, retrying");
                hid1 = HidApi.hid_open(deviceIds[1].VendorId, deviceIds[1].ProductId, null);
            }

            if (hid1 == IntPtr.Zero)
            {
                Console.WriteLine("No device found, sorry");
                return;
            }

            var manufacturer = new StringBuilder(100);
            if (HidApi.hid_get_manufacturer_string(hid1, manufacturer, Convert.ToUInt16(manufacturer.Capacity)) == 0)
            {
                Console.WriteLine("Manufacturer = {0}", manufacturer);
            }
            else
            {
                Console.WriteLine("Couldn't get manufacturer info");
            }

            var product = new StringBuilder(100);
            if (HidApi.hid_get_product_string(hid1, product, Convert.ToUInt16(product.Capacity)) == 0)
            {
                Console.WriteLine("Product = {0}", product);
            }
            else
            {
                Console.WriteLine("Couldn't get product info");
            }

            var serial_number = new StringBuilder(100);
            if (HidApi.hid_get_serial_number_string(hid1, serial_number, Convert.ToUInt16(serial_number.Capacity)) == 0)
            {
                Console.WriteLine("Serial_number = {0}", serial_number);
            }
            else
            {
                Console.WriteLine("Couldn't get serial_number info");
            }

            HidApi.hid_close(hid1);
            Console.WriteLine("Closed device");
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
