// <copyright file="IOnlyKey.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace HidapiTest
{
    using System;

    /// <summary>
    ///   Interface for all operations that must be supported by an OnlyKey.
    /// </summary>
    internal interface IOnlyKey : IDisposable
    {
        /// <summary>
        ///   Return array of OnlyKey key labels.
        /// </summary>
        /// <returns>Array of key labels.  Empty keys are <c>""</c>.</returns>
        public string[] KeyLabels();

        /// <summary>
        ///   Return array of OnlyKey slot labels.
        /// </summary>
        /// <returns>Array of slot labels.  Empty slots are <c>""</c>.</returns>
        public string[] SlotLabels();
    }

    internal class TestOnlyKey : IOnlyKey
    {
        private string[] keys;
        private string[] slots;

        public TestOnlyKey(string[] keys, string[] slots)
        {
            this.keys = keys;
            this.slots = slots;
        }

        public string[] KeyLabels() => this.keys;

        public string[] SlotLabels() => this.slots;

        public void Dispose()
        {
            // Nothing to dispose here, really
        }
    }
}
