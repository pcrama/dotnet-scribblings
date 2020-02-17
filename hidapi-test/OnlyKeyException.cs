// <copyright file="OnlyKeyException.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace HidapiTest
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   Base class for all exceptions thrown by <see cref="HardwareOnlyKey"/>.
    /// </summary>
    public class OnlyKeyException : Exception
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="OnlyKeyException"/> class.
        /// </summary>
        public OnlyKeyException()
        : base("OnlyKey related exception")
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="OnlyKeyException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        public OnlyKeyException(string message)
        : base(message)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="OnlyKeyException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        /// <param name="innerException">inner exception.</param>
        public OnlyKeyException(string message, Exception innerException)
        : base(message, innerException)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="OnlyKeyException"/> class.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
        protected OnlyKeyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}
