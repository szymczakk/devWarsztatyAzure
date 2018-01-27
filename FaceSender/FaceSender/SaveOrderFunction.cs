
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FaceSender
{
    public static class SaveOrderFunction
    {
        [FunctionName("SaveOrderFunction")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            OrderDTO data;
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                data = JsonConvert.DeserializeObject<OrderDTO>(requestBody);
            }
            catch (System.Exception)
            {
                return new BadRequestObjectResult("Failed");
            }
            return new OkObjectResult("Order proceeded");
        }
    }

    public class OrderDTO
    {
        public string Email { get; set; }
        public string FileName { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
