using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using lab03_storage.Models;
using Azure.Data.Tables;
using System.Collections.Generic;

namespace MCT.Function
{
    public static class RegistrationManagementNoSql
    {
        [FunctionName("AddRegistrationManagementNoSql")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v2/registrations")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                Registrations reg = JsonConvert.DeserializeObject<Registrations>(requestBody);

                // Read environment variables
                string accountName = Environment.GetEnvironmentVariable("AccountName"); //!storage name 
                string accountKey = Environment.GetEnvironmentVariable("StorageAccountKey"); //!storage account key not connectionstring!
                string storageUri = Environment.GetEnvironmentVariable("StorageUri"); //! tbl uri


                //reg.RegistationId = Guid.NewGuid();

                string partitionKey = "Registrations"; //?dont make the table urself code does it
                string rowKey = Guid.NewGuid().ToString();

                // Initialize TableClient
                var tableClient = new TableClient(
                    new Uri(storageUri),
                    "Registrations",
                    new TableSharedKeyCredential(accountName, accountKey));
                await tableClient.CreateIfNotExistsAsync();

                // Create a new TableEntity
                var entity = new TableEntity(partitionKey, rowKey)
                {
                {"Age" ,reg.Age},
                {"FirstName" ,reg.firstName},
                {"LastName" ,reg.lastName},
                {"Email" ,reg.Email},
                {"Zipcode" ,reg.ZipCode},
                {"IsFirstTimer" ,reg.IsFirstTimer.ToString()}
              };

                // Add the entity to the table
                await tableClient.AddEntityAsync(entity);
                return new OkObjectResult(reg);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error adding registrations");
                return new StatusCodeResult(500);
            }
        }

    }
}
