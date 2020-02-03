// <copyright file="IOnlyKey.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;

    /// <summary>
    ///   Interface for all operations that must be supported by an OnlyKey.
    /// </summary>
    internal interface IOnlyKey : IDisposable
    {
        /// <summary>
        ///   Return array of OnlyKey slot names.
        /// </summary>
        /// <returns>Array of slot names.  Empty slots are <c>null</c>.</returns>
        public string[] SlotNames();
    }

    internal class TestOnlyKey : IOnlyKey
    {
        private string[] slots;

        public TestOnlyKey(string[] slots)
        {
            this.slots = slots;
        }

        public string[] SlotNames() => this.slots;

        public void Dispose()
        {
            // Nothing to dispose here, really
        }
    }
}
