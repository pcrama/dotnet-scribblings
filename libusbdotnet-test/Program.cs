// <copyright file="Program.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    // See https://stackoverflow.com/a/957544 for how to access linked list
    using System;

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
            HidDeviceInfo onlykey = null;
            try
            {
                onlykey = FindSoleOnlyKey(hidApi);
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Oops: {e.Message}");
            }

            if (onlykey != null)
            {
                Console.WriteLine(
                    "path = {0}\n  vendor_id={1:X4} product_id={2:X4}\n  serial_number={3}\n  manufacturer={4}\n  product={5}\n  usage_page={6:X4}  usage={7:X4}\n  interface_number={8}",
                    onlykey.Path,
                    onlykey.VendorId,
                    onlykey.ProductId,
                    onlykey.SerialNumber,
                    onlykey.ManufacturerString,
                    onlykey.ProductString,
                    onlykey.UsagePage,
                    onlykey.Usage,
                    onlykey.InterfaceNumber);
                using (var hidDevice = hidApi.Open(onlykey.Path))
                {
                    Console.WriteLine(
                        "hidApi.Open({0}) ->\n  m={1}\n  p={2}\n  s={3}",
                        onlykey.Path,
                        hidDevice.GetManufacturerString(),
                        hidDevice.GetProductString(),
                        hidDevice.GetSerialNumber());
                    Console.WriteLine("---");
                    const byte OKGETLABELS = 0xe5;
                    var w = hidDevice.Write(
                        new byte[] { 0x0, 0xff, 0xff, 0xff, 0xff, OKGETLABELS, });
                    Console.WriteLine($"Wrote {w} bytes");
                    var r = hidDevice.Read(32);
                    Console.Write($"Read {r.Length} bytes");
                    foreach (var b in r)
                    {
                        if ((32 < b) && (b < 128))
                        {
                            Console.Write($" {Convert.ToChar(b)}");
                        }
                        else
                        {
                            Console.Write($" <{b:X2}>");
                        }
                    }

                    Console.WriteLine();
                }
            }

            DoSomethingWithHid();
            using (var onlyKey = new HardwareOnlyKey(hidApi))
            {
                foreach (var s in onlyKey.SlotNames())
                {
                    Console.WriteLine(s);
                }
            }
        }

        /// <summary>
        ///   Look for 1! OnlyKey in HID devices.
        /// </summary>
        /// <remarks>
        ///   <para>
        ///     Implemented close to the logic in
        ///     https://github.com/trustcrypto/python-onlykey/blob/a5b5b0787bf8450593a66e27f1bbcc2f2e82ded9/onlykey/client.py#L168.
        ///   </para>
        /// </remarks>
        private static HidDeviceInfo FindSoleOnlyKey(HidApi hidApi)
        {
            const string onlykeySerialNumber = "1000000000";
            var devices = new System.Collections.Generic.List<HidDeviceInfo>();
            foreach (var devInfo in hidApi.Enumerate())
            {
                foreach (var vpp in deviceIds)
                {
                    if ((devInfo.VendorId == vpp.VendorId) && (devInfo.ProductId == vpp.ProductId))
                    {
                        if (devInfo.SerialNumber == onlykeySerialNumber)
                        {
                            if ((devInfo.UsagePage == 0xffab) || (devInfo.InterfaceNumber == 2))
                            {
                                devices.Add(devInfo);
                            }
                        }
                        else
                        {
                            if ((devInfo.UsagePage == 0xf1d0) || (devInfo.InterfaceNumber == 1))
                            {
                                devices.Add(devInfo);
                            }
                        }
                    }
                }
            }

            switch (devices.Count)
            {
                case 0:
                    throw new InvalidOperationException("No OnlyKey found");
                case 1:
                    return devices[0];
                default:
                    throw new InvalidOperationException(
                        $"{devices.Count} OnlyKey devices found, can not decide which one to take");
            }
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
