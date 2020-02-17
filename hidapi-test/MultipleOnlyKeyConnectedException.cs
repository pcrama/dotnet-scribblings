// <copyright file="MultipleOnlyKeyConnectedException.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace HidapiTest
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    ///   Multiple OnlyKey connected to USB.
    /// </summary>
    public class MultipleOnlyKeyConnectedException : OnlyKeyException
    {
        private string[] paths;

        /// <summary>
        ///   Initializes a new instance of the <see cref="MultipleOnlyKeyConnectedException"/> class.
        /// </summary>
        /// <param name="paths">Platform-specific device paths in <c>string[]</c>.</param>
        public MultipleOnlyKeyConnectedException(string[] paths)
        : base("Multiple OnlyKey devices connected, cannot select 1! by myself.")
        {
            this.paths = paths;
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="MultipleOnlyKeyConnectedException"/> class.
        /// </summary>
        public MultipleOnlyKeyConnectedException()
        : this(Array.Empty<string>())
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="MultipleOnlyKeyConnectedException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        public MultipleOnlyKeyConnectedException(string message)
        : base(message)
        {
            this.paths = Array.Empty<string>();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="MultipleOnlyKeyConnectedException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        /// <param name="innerException">inner exception.</param>
        public MultipleOnlyKeyConnectedException(string message, Exception innerException)
        : base(message, innerException)
        {
            this.paths = Array.Empty<string>();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="MultipleOnlyKeyConnectedException"/> class.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
        protected MultipleOnlyKeyConnectedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }

        /// <summary>
        ///   Gets paths of all HID devices detected to be OnlyKey devices.
        /// </summary>
        public IReadOnlyCollection<string> Paths { get => this.paths; }
}
}
