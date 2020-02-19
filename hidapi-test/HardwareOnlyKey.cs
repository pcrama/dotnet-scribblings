// <copyright file="HardwareOnlyKey.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace HidapiTest
{
    using System;
    using System.Text;

    /// <summary>
    ///   Implement <see cref="IOnlyKey"/> interface for a hardware OnlyKey device.
    /// </summary>
    public class HardwareOnlyKey : IOnlyKey
    {
        private const byte OKGETLABELS = 0xe5;

        private static VendorProductIdPair[] deviceIds =
        {
            new VendorProductIdPair(vendor: 0x16c0, product: 0x0486),
            new VendorProductIdPair(vendor: 0x1d50, product: 0x60fc),
        };

        private bool disposed = true;
        private HidApi hidApi = null;
        private HidDeviceInfo deviceInfo = null;
        private IHidHandle device = null;

        /// <summary>
        ///   Initializes a new instance of the <see cref="HardwareOnlyKey"/> class.
        /// </summary>
        /// <param name="hidApi"><see cref="HidApi"/> instance wrapping the hidapi library.</param>
        public HardwareOnlyKey(HidApi hidApi)
        {
            if (hidApi == null)
            {
                throw new ArgumentNullException(nameof(hidApi));
            }

            this.hidApi = hidApi;
            this.disposed = false;
            this.deviceInfo = this.FindSoleOnlyKey();
            this.device = this.hidApi.Open(this.deviceInfo.Path);
        }

        /// <summary>
        ///   Finalizes an instance of the <see cref="HardwareOnlyKey"/> class.
        ///   Use C# destructor syntax for finalization code.
        ///   This destructor will run only if the Dispose method
        ///   does not get called.
        ///   It gives your base class the opportunity to finalize.
        ///   Do not provide destructors in types derived from this class.
        /// </summary>
        ~HardwareOnlyKey()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        /// <summary>
        ///   Gets the information about the device.
        /// </summary>
        public HidDeviceInfo DeviceInfo { get => this.deviceInfo; }

        /// <summary>
        ///   Return array of OnlyKey key labels.
        /// </summary>
        /// <returns>Array of key labels.  Empty keys are <c>""</c>.</returns>
        public string[] KeyLabels()
        {
            var keys = new string[33];
            for (var idx = 0; idx < keys.Length; ++idx)
            {
                keys[idx] = null;
            }

            this.SendMessage(OKGETLABELS, 107);
            for (var idx = 0; idx < keys.Length; ++idx)
            {
                var r = this.device.Read(32, 1);
                var s = FromCString(r);

                // TODO: find out what an uninitialized OnlyKey answers
                if (s == "INITIALIZED")
                {
                    throw new LockedOrUninitializedException(s);
                }

                if (
                    (r.Length >= 2)
#pragma warning disable SA1131 // This way of writing the condition emphasizes the range check
                    && (25 <= r[0]) && (r[0] <= 57)
#pragma warning restore SA1131 // Use readable conditions
                    && (r[0] - 25 == idx)
                    && (r[1] == Convert.ToByte('|')))
                {
                    keys[idx] = FromCString(r, 2);
                }
                else
                {
                    throw new OnlyKeyException(
                        $"Error parsing OnlyKey response {s}");
                }
            }

            return keys;
        }

        /// <summary>
        ///   Return array of OnlyKey slot labels.
        /// </summary>
        /// <returns>Array of slot labels.  Empty slots are <c>""</c>.</returns>
        public string[] SlotLabels()
        {
            var slots = new string[12];
            for (var idx = 0; idx < slots.Length; ++idx)
            {
                slots[idx] = null;
            }

            // TODO: leading 0x0 is only needed on Windows, not on Linux
            // var w = this.device.Write(
            //     new byte[] { 0x0, 0xff, 0xff, 0xff, 0xff, OKGETLABELS, });
            this.SendMessage(OKGETLABELS);
            while (true)
            {
                var r = this.device.Read(32, 1);
                var s = FromCString(r);

                // TODO: find out what an uninitialized OnlyKey answers
                if (s == "INITIALIZED")
                {
                    throw new LockedOrUninitializedException(s);
                }

                // Go BCD to binary representation
                if (r[0] >= 0x10)
                {
                    r[0] -= 6;
                }

                if (
                    (r.Length >= 2)
#pragma warning disable SA1131 // This way of writing the condition emphasizes the range check
                    && (1 <= r[0]) && (r[0] <= 12)
#pragma warning restore SA1131 // Use readable conditions
                    && (r[1] == Convert.ToByte('|')))
                {
                    slots[r[0] - 1] = FromCString(r, 2);
                    if (r[0] == slots.Length)
                    {
                        return slots;
                    }
                }
                else
                {
                    throw new OnlyKeyException(
                        $"Error parsing OnlyKey response {s}");
                }
            }
        }

        /// <summary>
        ///   Implement IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Dispose(bool disposing) executes in two distinct scenarios.
        ///   If disposing equals true, the method has been called directly
        ///   or indirectly by a user's code. Managed and unmanaged resources
        ///   can be disposed.
        ///   If disposing equals false, the method has been called by the
        ///   runtime from inside the finalizer and you should not reference
        ///   other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">
        ///   <c>true</c> when called from <see cref="HardwareOnlyKey.Dispose()"/>,
        ///   i.e. when used with the <c>IDisposable</c> interface.  <c>false</c>
        ///   when called from destructor.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (this.device != null)
                    {
                        this.device.Dispose();
                    }
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                this.hidApi = null;

                // Note disposing has been done.
                this.disposed = true;
            }
        }

        private static string FromCString(byte[] d)
        {
            return FromCString(d, 0);
        }

        private static string FromCString(byte[] d, int startIdx)
        {
            return FromCString(d, startIdx, Encoding.ASCII);
        }

        private static string FromCString(byte[] d, int startIdx, Encoding encoding)
        {
            var zero = Array.IndexOf<byte>(d, 0);
#pragma warning disable SA1131 // This way of writing the condition emphasizes the range check
            if ((0 <= zero) && (zero < d.Length))
#pragma warning restore SA1131 // Use readable conditions
            {
                return encoding.GetString(d, startIdx, zero - startIdx);
            }
            else
            {
                return encoding.GetString(d, startIdx, d.Length - startIdx);
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
        private HidDeviceInfo FindSoleOnlyKey()
        {
            const string onlykeySerialNumber = "1000000000";
            var devices = new System.Collections.Generic.List<HidDeviceInfo>();
            foreach (var devInfo in this.hidApi.Enumerate())
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
                    var paths = new string[devices.Count];
                    var idx = 0;
                    foreach (var d in devices)
                    {
                        paths[idx++] = d.Path;
                    }

                    throw new MultipleOnlyKeyConnectedException(paths);
            }
        }

        private void SendMessage(byte? message, byte? slotId = null)
        {
            // TODO: leading 0x0 is only needed on Windows, not on Linux
            const int maxBufferLen = 64 + 1; // leading 0
            var buffer = new byte[maxBufferLen];
            var bufferLen = 0;
            buffer[bufferLen++] = 0x0;
            for (var idx = 0; idx < 4; ++idx)
            {
                buffer[bufferLen++] = 0xff;
            }

            if (message is byte messageVal)
            {
                buffer[bufferLen++] = messageVal;
                if (slotId is byte slotIdVal)
                {
                    buffer[bufferLen++] = slotIdVal;
                }
            }
            else
            {
                if (slotId is byte slotIdVal)
                {
                    throw new OnlyKeyException(
                        $"When slotId={slotIdVal} is not null, message should not be null either");
                }
            }

            Array.Resize(ref buffer, bufferLen);
            var w = this.device.Write(buffer);
            if (w != maxBufferLen)
            {
                throw new OnlyKeyException(
                    $"Wanted to write {maxBufferLen} bytes but wrote {w}");
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
