﻿// Copyright Mark J. van Wijk 2023

namespace SiteMapUriExtractor {

    /// <summary>
    /// Cache retention/reuse policy
    /// </summary>
    public enum RetentionPolicy {
        /// <summary>
        /// Null, undefined value
        /// </summary>
        None = 0,

        /// <summary>
        /// Check header
        /// </summary>
        Header,

        /// <summary>
        /// If cached contents is older as one day, check via header, otherwise use cache
        /// </summary>
        Day,

        /// <summary>
        /// If cached contents is older as one week, check via header, otherwise use cache
        /// </summary>
        Week

    }
}
