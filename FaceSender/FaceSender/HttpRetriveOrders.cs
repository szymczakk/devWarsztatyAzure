
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace FaceSender
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Table;

    public static class HttpRetriveOrders
    {
        [FunctionName("HttpRetriveOrders")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, 
           [Table("Orders", Connection = "OrdersConnectionString")]CloudTable ordersTable,
           TraceWriter log)
        {
            string fileName = req.Query["fileName"];
            if (string.IsNullOrEmpty(fileName))
            {
                return new BadRequestResult();
            }

            TableQuery<OrderDTO> query = new TableQuery<OrderDTO>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, fileName));
            TableQuerySegment<OrderDTO> tableQueryResult = await ordersTable.ExecuteQuerySegmentedAsync(query, null);
            var resultList = tableQueryResult.Results;

            if (resultList.Any())
            {
                var firstElement = resultList.First();

                return new JsonResult(new
                {
                    firstElement.Email,
                    firstElement.FileName,
                    firstElement.Height,
                    firstElement.Width
                });
            }

            return new NotFoundResult();
        }
    }
}
