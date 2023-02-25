using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Graph;
using multitenantAuth1.Model;
using System.Text.Json;

namespace multitenantAuth1.Controllers
{
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
       
        private readonly IConfiguration Configuration;
        private readonly ILogger _logger;

        public NotificationController( IConfiguration configuration, ILogger<NotificationController> logger)
        {
            Configuration = configuration;
            _logger = logger;

        }

        [HttpPost("postSubscription")]

        public async Task<ActionResult<string>> Post([FromQuery] string? validationToken = null)
        {
            // handle validation
            if (!string.IsNullOrEmpty(validationToken))
            {
                Console.WriteLine($"Received Token: '{validationToken}'");
                return Ok(validationToken);
            }


            ResponseModel responseModel = new ResponseModel();
            try
            {


              using CosmosClient client = new(
              accountEndpoint: Configuration["COSMOS_ENDPOINT"],
              authKeyOrResourceToken: Configuration["COSMOS_KEY"],
               new CosmosClientOptions()
               {
                   SerializerOptions = new CosmosSerializationOptions()
                   {
                       PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                   }

               }
              );

                Database database = client.GetDatabase(id: "ToDoList");
                Console.WriteLine($"New database:\t{database.Id}");
                _logger.LogInformation($"New database:\t{database.Id}");
                // Container reference with creation if it does not alredy exist
                Container container = await database.CreateContainerIfNotExistsAsync(
                    id: "calls",
                    partitionKeyPath: "/id",
                    throughput: 400
                );
                Console.WriteLine($"New container:\t{container.Id}");
                // handle notifications
                using (StreamReader reader = new StreamReader(Request.Body))
                {
                    string content = await reader.ReadToEndAsync();

                    Console.WriteLine(content);
                    _logger.LogInformation("About page visited at {DT}", DateTime.UtcNow.ToLongTimeString());
                    _logger.LogInformation($"Received content: '{content}'");

                    var notifications = JsonSerializer.Deserialize<ChangeNotificationCollection>(content);

                    if (notifications != null)
                    {
                        foreach (var notification in notifications.Value)
                        {
                            string val = notification.ResourceData.AdditionalData["id"]
                                .ToString();
                            notification.Id = val;
                            Console.WriteLine($"Received notification: '{notification.Resource}', {notification.ResourceData.AdditionalData["id"]}");
                            _logger.LogInformation($"Received notification: '{notification.Resource}', {notification.ResourceData.AdditionalData["id"]}");
                            var createdItem = await container.CreateItemAsync(
                              item: notification,
                              partitionKey: new PartitionKey(val)
                          );
                        }
                    }
                    _logger.LogInformation("after foreach loop");
                    responseModel.StatusCode = 200;
                    responseModel.Data = content;
                    responseModel.Message = "Success";
                }

                return Ok();
            }

            catch (Exception ex)
            {

                responseModel.StatusCode = 500;
                responseModel.Message = "Something went wrong";
                responseModel.Data = "Internal server error " + ex;
                return Ok(responseModel);
            }
        }
    }
}
