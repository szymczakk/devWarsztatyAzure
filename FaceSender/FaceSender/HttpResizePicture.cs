
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FaceSender
{
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Jpeg;

    public static class HttpResizePicture
    {
        [FunctionName("HttpResizePicture")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req,
            [Blob("photos", FileAccess.Read, Connection = "OrdersConnectionString")]CloudBlobContainer photosContainer,
            [Blob("doneorders/{rand-guid}", FileAccess.ReadWrite, Connection = "OrdersConnectionString")] ICloudBlob resizedPhotoCloudBlob,
            TraceWriter log)
        {
            var pictureResizeRequest = await GetResizeRequest(req);
            var photoStream = await GetSourcePhotoStream(photosContainer, pictureResizeRequest.FileName);
            SetAttachmentAsContentDisposition(resizedPhotoCloudBlob, pictureResizeRequest);

            var image = Image.Load(photoStream);
            image.Mutate(e => e.Resize(pictureResizeRequest.Width, pictureResizeRequest.Width));

            using (var resizedPhotoStream = new MemoryStream())
            {
                image.Save(resizedPhotoStream, new JpegEncoder());
                resizedPhotoStream.Seek(0, SeekOrigin.Begin);
                await resizedPhotoCloudBlob.UploadFromStreamAsync(resizedPhotoStream);
            }

            return new JsonResult(new {FileName = resizedPhotoCloudBlob.Name});
        }

        private static void SetAttachmentAsContentDisposition(ICloudBlob resizedPhotoCloudBlob, PictureResizeRequest pictureResizeRequest)
        {
            resizedPhotoCloudBlob.Properties.ContentDisposition =
                $"attachment; filename={pictureResizeRequest.Width}x{pictureResizeRequest.Height}.jpeg";
        }

        private static async Task<Stream> GetSourcePhotoStream(CloudBlobContainer photosContainer, string fileName)
        {
            var photoBlob = await photosContainer.GetBlobReferenceFromServerAsync(fileName);
            var photoStream = await photoBlob.OpenReadAsync(AccessCondition.GenerateEmptyCondition(),
                new BlobRequestOptions(), new OperationContext());
            return photoStream;
        }

        private static async Task<PictureResizeRequest> GetResizeRequest(HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var pictureResizeRequest = JsonConvert.DeserializeObject<PictureResizeRequest>(requestBody);
            return pictureResizeRequest;
        }
    }

    internal class PictureResizeRequest
    {
        public string FileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
