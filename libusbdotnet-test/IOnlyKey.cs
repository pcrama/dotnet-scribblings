// <copyright file="IOnlyKey.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace LibusbdotnetTest
{
    using System;

    internal interface IOnlyKey : IDisposable
    {
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
