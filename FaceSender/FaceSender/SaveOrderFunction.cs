
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FaceSender
{
    using Microsoft.WindowsAzure.Storage.Table;

    public static class SaveOrderFunction
    {
        [FunctionName("SaveOrderFunction")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, 
            [Table("Orders", Connection = "OrdersConnectionString")]ICollector<OrderDTO> orderTable,
            TraceWriter log)
        {
            OrderDTO data;
            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                data = JsonConvert.DeserializeObject<OrderDTO>(requestBody);
                data.PartitionKey = System.DateTime.UtcNow.DayOfYear.ToString();
                data.RowKey = data.FileName;
                orderTable.Add(data);
            }
            catch (System.Exception)
            {
                return new BadRequestObjectResult("Failed");
            }
            return new OkObjectResult("Order proceeded");
        }
    }

    public class OrderDTO: TableEntity
    {
        public string Email { get; set; }
        public string FileName { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
