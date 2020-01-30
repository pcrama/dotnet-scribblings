// <copyright file="NativeMethods.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        [DllImport("Assemblies/rawhid.dll")]
        public static extern int rawhid_open(int max, int vid, int pid, int usage_page, int usage);

        [DllImport("Assemblies/rawhid.dll")]
        public static extern int rawhid_recv(
            int num,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] out byte[] buf,
            int len,
            int timeout);

        [DllImport("Assemblies/rawhid.dll")]
        public static extern int rawhid_send(
            int num,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)] in byte[] buf,
            int len,
            int timeout);

        [DllImport("Assemblies/rawhid.dll")]
        public static extern void rawhid_close(int num);

        public static bool OpenHid()
        {
            try
            {
                int r = rawhid_open(1, 0x16c0, 0x0486, -1, -1);
                if (r <= 0)
                {
                    Console.WriteLine("Retrying...");
                    r = rawhid_open(1, 0x1d50, 0x60fc, -1, -1);
                    if (r <= 0)
                    {
                        Console.WriteLine("no rawhid device found\n");
                        return false;
                    }
                }

                Console.WriteLine($"found {r} rawhid device\n");
                return true;
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }

            return false;
        }
    }
}
