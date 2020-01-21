// <copyright file="SetOnce.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

/* Moved into a gist for usage in smallcliutils (see
 * https://github.com/pcrama/scoop-buckets/)
 * https://gist.github.com/pcrama/a0480922ba7e4a0082c50a97335011f0
 */

namespace RunAsFromFile
{
    /// <summary>Hold a value of type <typeparamref name="T"/> that can be set
    /// at most once: afterwards, any attempts to set it are silently
    /// ignored.</summary> <typeparam name="T">type of value to
    /// hold.</typeparam>
    internal class SetOnce<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetOnce{T}"/> class with unset value.
        /// </summary>
        public SetOnce()
        {
            this.IsSet = false;
            this.Value = default(T);
        }

        /// <summary>Initializes a new instance of the <see
        /// cref="SetOnce{T}"/> class with value already set to <paramref
        /// name="v"/>.</summary><param name="v">initial value.</param>
        public SetOnce(T v)
        {
            this.IsSet = true;
            this.Value = v;
        }

        /// <summary>
        ///   Gets a value indicating whether value has not been set yet.
        /// </summary>
        public bool NotSet
        {
            get { return !this.IsSet; }
            private set { this.IsSet = !value; }
        }

        /// <summary>
        ///   Gets a value indicating whether value has been set already.
        /// </summary>
        public bool IsSet { get; private set; }

        /// <summary>
        ///   Gets value that was set.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        ///   Set value to <paramref name="v"/> if it wasn't already set.
        /// </summary>
        /// <param name="v">new value to set if no value was set before.</param>
        /// <returns>this so that calls can be cained.</returns>
        public SetOnce<T> Set(T v)
        {
            if (this.NotSet)
            {
                this.IsSet = true;
                this.Value = v;
            }

            return this;
        }
    }
}
