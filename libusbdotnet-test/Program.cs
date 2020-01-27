// <copyright file="Program.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;

    internal static class Program
    {

        private static VendorProductIdPair[] DeviceIds = {
            new VendorProductIdPair(vendor: 0x16c0, product: 0x0486),
            new VendorProductIdPair(vendor: 0x1d50, product: 0x60fc)
        };

        private class VendorProductIdPair {
            public int VendorId { get; private set; }
            public int ProductId { get; private set; }
            public VendorProductIdPair(int vendor, int product)
            {
                this.VendorId = vendor;
                this.ProductId = product;
            }
        }

        public static void Main(string[] args)
        {
            if (NativeMethods.OpenHid())
            {
                Console.WriteLine("Success");
            }
            else
            {
                Console.WriteLine("Abysmal failure");
            }
        }
    }
}
