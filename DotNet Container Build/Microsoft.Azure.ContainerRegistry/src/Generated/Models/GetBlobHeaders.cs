// <auto-generated>
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.ContainerRegistry.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Defines headers for GetBlob operation.
    /// </summary>
    public partial class GetBlobHeaders
    {
        /// <summary>
        /// Initializes a new instance of the GetBlobHeaders class.
        /// </summary>
        public GetBlobHeaders()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the GetBlobHeaders class.
        /// </summary>
        /// <param name="contentLength">The length of the requested blob
        /// content.</param>
        /// <param name="dockerContentDigest">Digest of the targeted content
        /// for the request.</param>
        /// <param name="location">The location where the layer should be
        /// accessible.</param>
        public GetBlobHeaders(long? contentLength = default(long?), string dockerContentDigest = default(string), string location = default(string))
        {
            ContentLength = contentLength;
            DockerContentDigest = dockerContentDigest;
            Location = location;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the length of the requested blob content.
        /// </summary>
        [JsonProperty(PropertyName = "Content-Length")]
        public long? ContentLength { get; set; }

        /// <summary>
        /// Gets or sets digest of the targeted content for the request.
        /// </summary>
        [JsonProperty(PropertyName = "Docker-Content-Digest")]
        public string DockerContentDigest { get; set; }

        /// <summary>
        /// Gets or sets the location where the layer should be accessible.
        /// </summary>
        [JsonProperty(PropertyName = "Location")]
        public string Location { get; set; }

    }
}
