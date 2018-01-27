
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FaceSender
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    public static class HttpGetSharedAccessSignatureForBlob
    {
        [FunctionName("HttpGetSharedAccessSignatureForBlob")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, 
            [Blob("doneorders", FileAccess.Read, Connection = "OrdersConnectionString")]CloudBlobContainer photosContainer,
            TraceWriter log)
        {
            string fileName = req.Query["fileName"];
            if (string.IsNullOrEmpty(fileName))
            {
                return new BadRequestResult();
            }

            var photoBlob = await photosContainer.GetBlobReferenceFromServerAsync(fileName);
            var photoUri = GetBlobSasUri(photoBlob);
            return new JsonResult(new {PhotoUri = photoUri});
        }

        private static string GetBlobSasUri(ICloudBlob photoBlob)
        {
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddHours(-1);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read;

            var sasToken = photoBlob.GetSharedAccessSignature(sasConstraints);

            return photoBlob.Uri + sasToken;
        }
    }
}
