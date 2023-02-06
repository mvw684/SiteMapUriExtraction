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

        /// <summary>
        /// If the task failed throw an exception
        /// </summary>
        public static void ThrowIfFailed(string verb, Uri uri, Task<HttpResponseMessage> task) {
            if (!task.IsCompleted) {
                task.Wait();
            }
            if (!task.IsCompletedSuccessfully) {
                var exception = task.Exception;
                if (exception is null) {
                    throw new OperationFailedException($"{verb} {uri}: {task.Status }");
                } else {
                    throw new OperationFailedException($"{verb} {uri}: {exception.Message}", exception);
                }
            }
            var result = task.Result;
            if (!result.IsSuccessStatusCode) {
                throw new OperationFailedException($"{verb} {uri}: {result.StatusCode} {result.ReasonPhrase}");
            }
        }
    }
}
