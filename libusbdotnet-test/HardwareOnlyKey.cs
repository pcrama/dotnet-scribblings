// <copyright file="HardwareOnlyKey.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;

    public class HardwareOnlyKey : IOnlyKey
    {
        private bool disposed = true;

        public HardwareOnlyKey()
        {
            if (HidApi.hid_init() != 0)
            {
                throw new SystemException("Failed to initialize hidapi");
            }
            else
            {
                Console.WriteLine("hid_init returned 0");
            }

            this.disposed = false;

            // ... find HID device here
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~HardwareOnlyKey()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        public string[] SlotNames()
        {
            string[] notImplemented = { "not", "implemented", "yet" };
            return notImplemented;
        }

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

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                if (HidApi.hid_exit() < 0)
                {
                    Console.WriteLine("Failed to hid_exit()");
                }

                // Note disposing has been done.
                this.disposed = true;
            }
        }
    }
}
