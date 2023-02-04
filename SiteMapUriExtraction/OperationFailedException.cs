// Copyright Mark J. van Wijk 2023

using System.Runtime.Serialization;

namespace SiteMapUriExtractor {

    /// <summary>
    /// Generic exception for all kinds of failed activities
    /// </summary>
    public class OperationFailedException : Exception {

        /// <summary>
        /// <see cref="Exception(string?)"/>
        /// </summary>
        public OperationFailedException(string message) : base(message) {
        }

        /// <summary>
        /// <see cref="Exception(string?, Exception?)"/>
        /// </summary>
        public OperationFailedException(string message, Exception exception) : base(message, exception) { }

        /// <summary>
        /// <see cref="Exception(SerializationInfo, StreamingContext)"/>
        /// </summary>
        protected OperationFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
