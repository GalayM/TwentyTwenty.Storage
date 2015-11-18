﻿using Microsoft.WindowsAzure.Storage.Blob;
using Xunit;

namespace TwentyTwenty.Storage.Azure.Test
{
    [Trait("Category", "Azure")]
    public sealed class UpdateTests : BlobTestBase
    {
        public UpdateTests(StorageFixture fixture)
            :base(fixture) { }

        [Fact]
        public async void Test_Container_Permissions_Elevated_On_Save()
        {
            var containerName = GetRandomContainerName();
            var containerRef = _client.GetContainerReference(containerName);

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Off, null, null);

            _provider.SaveBlobStream(containerName, GenerateRandomName(), GenerateRandomBlobStream(), new BlobProperties { Security = BlobSecurity.Public });

            Assert.True(await containerRef.ExistsAsync());
            Assert.Equal(BlobContainerPublicAccessType.Blob, (await containerRef.GetPermissionsAsync()).PublicAccess);
        }

        [Fact]
        public async void Test_Container_Permissions_Elevated_On_Update()
        {
            var containerName = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var containerRef = _client.GetContainerReference(containerName);
            var blobRef = containerRef.GetBlockBlobReference(blobName);
            var data = GenerateRandomBlobStream();

            await containerRef.CreateAsync(BlobContainerPublicAccessType.Off, null, null);

            await blobRef.UploadFromStreamAsync(data);            
            _provider.UpdateBlobProperties(containerName, blobName, new BlobProperties { Security = BlobSecurity.Public });

            Assert.Equal(BlobContainerPublicAccessType.Blob, (await containerRef.GetPermissionsAsync()).PublicAccess);
        }

        [Fact]
        public async void Test_Blob_Properties_Updated()
        {
            var container = GetRandomContainerName();
            var blobName = GenerateRandomName();
            var contentType = "image/jpg";
            var data = GenerateRandomBlobStream();

            var containerRef = _client.GetContainerReference(container);
            var blobRef = containerRef.GetBlockBlobReference(blobName);

            await containerRef.CreateAsync();
            blobRef.Properties.ContentType = "image/png";

            await blobRef.UploadFromStreamAsync(data);
            _provider.UpdateBlobProperties(container, blobName, new BlobProperties { ContentType = contentType });

            await blobRef.FetchAttributesAsync();
            Assert.Equal(contentType, blobRef.Properties.ContentType);
        }
    }
}