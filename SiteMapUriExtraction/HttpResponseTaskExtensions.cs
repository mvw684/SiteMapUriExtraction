// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {
    internal static class HttpResponseTaskExtensions {

        /// <summary>
        /// If the task failed throw an exception
        /// </summary>
        public static void ThrowIfTaskFailed(this Task<HttpResponseMessage> task, string verb, Uri uri) {
            if (!task.IsCompleted) {
                task.Wait();
            }
            if (!task.IsCompletedSuccessfully) {
                var exception = task.Exception;
                if (exception is null) {
                    throw new OperationFailedException($"{verb} {uri}: {task.Status}");
                } else {
                    throw new OperationFailedException($"{verb} {uri}: {exception.Message}", exception);
                }
            }
        }

        /// <summary>
        /// If the task or the request failed throw an exception
        /// </summary>
        public static void ThrowIfRequestFailed(this Task<HttpResponseMessage> task, string verb, Uri uri) {
            task.ThrowIfTaskFailed(verb, uri);
            var result = task.Result;
            if (!result.IsSuccessStatusCode) {
                throw new OperationFailedException($"{verb} {uri}: {result.StatusCode} {result.ReasonPhrase}");
            }
        }
    }
}
