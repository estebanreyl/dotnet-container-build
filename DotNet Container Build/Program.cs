﻿using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Azure.ContainerRegistry;
using Microsoft.Azure.ContainerRegistry.Models;
using System.IO;
using System.Threading.Tasks;
using Konsole;
using System.Collections.Generic;

namespace DotNet_Container_Build
{
    /// <summary>
    /// Sample application demonstrating the ability to download all layers for an image and use them to 
    /// create a new Repository.
    /// </summary>
    class Program
    {
        const string Username = "csharpsdkblobtest";
        const string Password = "YIn=nAEVom8eRkZPT4wEQ4PL5O4oSiiX";
        const string Registry = "csharpsdkblobtest.azurecr.io";
        const string RepoOrigin = "jenkins";
        const string RepoOutput = "jenkins9";
        const string OutputTag = "latest";
        const int MaxParallel = 4;

        static void Main()
        {
            int timeoutInMilliseconds = 1500000;
            CancellationToken ct = new CancellationTokenSource(timeoutInMilliseconds).Token;
            AzureContainerRegistryClient client = LoginBasic(ct);
            BuildImageInRepoAfterDownload(RepoOrigin, RepoOutput, OutputTag, client, ct).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Example Credentials provisioning (Using Basic Authentication)
        /// </summary>
        private static AzureContainerRegistryClient LoginBasic(CancellationToken ct)
        {
            AcrClientCredentials credentials = new AcrClientCredentials(AcrClientCredentials.LoginMode.Basic, Registry, Username, Password, ct);
            AzureContainerRegistryClient client = new AzureContainerRegistryClient(credentials)
            {
                LoginUri = "https://csharpsdkblobtest.azurecr.io"
            };
            return client;
        }

        /// <summary>
        /// Uploads a specified image layer by layer from another local repository (Within the specific registry)
        /// </summary>
        private static async Task BuildImageInRepoAfterDownload(string origin, string output, string outputTag, AzureContainerRegistryClient client, CancellationToken ct)
        {
            V2Manifest manifest = (V2Manifest)await client.GetManifestAsync(origin, outputTag, "application/vnd.docker.distribution.manifest.v2+json", ct);

            var listOfActions = new List<Action>();

            // Acquire and upload all layers
            for (int i = 0; i < manifest.Layers.Count; i++)
            {
                var cur = i;
                listOfActions.Add(() =>
                {
                    var progress = new ProgressBar(3);
                    progress.Refresh(0, "Starting");
                    var layer = client.GetBlobAsync(origin, manifest.Layers[cur].Digest).GetAwaiter().GetResult();
                    progress.Next("Downloading " + manifest.Layers[cur].Digest + " layer from " + origin);
                    string digestLayer = UploadLayer(layer, output, client).GetAwaiter().GetResult();
                    progress.Next("Uploading " + manifest.Layers[cur].Digest + " layer to " + output);
                    manifest.Layers[cur].Digest = digestLayer;
                    progress.Next("Uploaded " + manifest.Layers[cur].Digest + " layer to " + output);
                });
            }

            // Acquire config Blob
            listOfActions.Add(() =>
            {
                var progress = new ProgressBar(3);
                progress.Next("Downloading config blob from " + origin);
                var configBlob = client.GetBlobAsync(origin, manifest.Config.Digest).GetAwaiter().GetResult();
                progress.Next("Uploading config blob to " + output);
                string digestConfig = UploadLayer(configBlob, output, client).GetAwaiter().GetResult();
                progress.Next("Uploaded config blob to " + output);
                manifest.Config.Digest = digestConfig;
            });

            var options = new ParallelOptions { MaxDegreeOfParallelism = MaxParallel };
            Parallel.Invoke(options, listOfActions.ToArray());

            Console.WriteLine("Pushing new manifest to " + output + ":" + outputTag);
            await client.CreateManifestAsync(output, outputTag, manifest, ct);

            Console.WriteLine("Successfully created " + output + ":" + outputTag);
        }

        /// <summary>
        /// Upload a layer using the nextLink properties internally. Very clean and simple to use overall.
        /// </summary>
        private static async Task<string> UploadLayer(Stream blob, string repo, AzureContainerRegistryClient client)
        {
            // Make copy to obtain the ability to rewind the stream
            Stream cpy = new MemoryStream();
            blob.CopyTo(cpy);
            cpy.Position = 0;

            string digest = ComputeDigest(cpy);
            cpy.Position = 0;

            var uploadInfo = await client.StartEmptyResumableBlobUploadAsync(repo);
            var uploadedLayer = await client.UploadBlobContentFromNextAsync(cpy, uploadInfo.Location.Substring(1));
            var uploadedLayerEnd = await client.EndBlobUploadFromNextAsync(digest, uploadedLayer.Location.Substring(1));
            return digest;
        }

        struct BlobData
        {
            public bool noUpload;
            public string state;
        }

        /// <summary>
        /// Computes a digest for a particular layer from its stream
        /// </summary>
        private static string ComputeDigest(Stream s)
        {
            s.Position = 0;
            StringBuilder sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.Unicode;
                Byte[] result = hash.ComputeHash(s);

                foreach (Byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            return "sha256:" + sb.ToString();

        }
    }
}
