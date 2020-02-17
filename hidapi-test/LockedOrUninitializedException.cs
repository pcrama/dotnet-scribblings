// <copyright file="LockedOrUninitializedException.cs" company="Philippe Crama">
// Copyright (c) Philippe Crama. All rights reserved.
// Licensed under the GPLv3 license. See LICENSE file for full license information.
// </copyright>
namespace HidapiTest
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   Thrown when an I/O operation with an OnlyKey failed because the
    ///   OnlyKey is locked or has not been configured yet.
    /// </summary>
    public class LockedOrUninitializedException : OnlyKeyException
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="LockedOrUninitializedException"/> class.
        /// </summary>
        public LockedOrUninitializedException()
        : this("OnlyKey is locked or not initialized yet.")
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="LockedOrUninitializedException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        public LockedOrUninitializedException(string message)
        : base(message)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="LockedOrUninitializedException"/> class.
        /// </summary>
        /// <param name="message">error message.</param>
        /// <param name="innerException">inner exception.</param>
        public LockedOrUninitializedException(string message, Exception innerException)
        : base(message, innerException)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="LockedOrUninitializedException"/> class.
        /// </summary>
        /// <param name="info">serialization info.</param>
        /// <param name="context">streaming context.</param>
        protected LockedOrUninitializedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}
