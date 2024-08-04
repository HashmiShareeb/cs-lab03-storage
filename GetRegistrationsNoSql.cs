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
using System.Collections.Generic;
using Azure.Data.Tables;

namespace MCT.Function
{
    public static class GetRegistrationsNoSql
    {
        [FunctionName("GetRegistrationsNoSql")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v2/registrations")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string accountName = Environment.GetEnvironmentVariable("AccountName");
                string accountKey = Environment.GetEnvironmentVariable("StorageAccountKey");
                string storageUri = Environment.GetEnvironmentVariable("StorageUri");

                // if (string.IsNullOrEmpty(accountName) || string.IsNullOrEmpty(accountKey) || string.IsNullOrEmpty(storageUri))
                // {
                //     log.LogError("One or more environment variables are not set.");
                //     return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                // }

                var tableClient = new TableClient(new Uri(storageUri), "Registrations", new TableSharedKeyCredential(accountName, accountKey));
                var queryResults = new List<TableEntity>();

                await foreach (TableEntity entity in tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq 'Registrations'"))
                {
                    queryResults.Add(entity);
                }

                var registrations = new List<Registrations>();
                //!fetch the data from the query results and check if the entity contains the key
                foreach (var entity in queryResults) //?iterate/loop over queryresults
                {
                    //add fetched results into the list
                    registrations.Add(new Registrations
                    {
                        RegistationId = Guid.Parse(entity.RowKey), //*parse the rowkey to guid
                        //*check if enitity contains the key and convert it to string if not its null
                        lastName = entity.ContainsKey("LastName") ? entity["LastName"].ToString() : null,
                        firstName = entity.ContainsKey("FirstName") ? entity["FirstName"].ToString() : null,
                        Email = entity.ContainsKey("Email") ? entity["Email"].ToString() : null,
                        ZipCode = entity.ContainsKey("ZipCode") ? entity["ZipCode"].ToString() : null,
                        Age = entity.ContainsKey("Age") ? Convert.ToInt32(entity["Age"]) : 0,
                        IsFirstTimer = entity.ContainsKey("IsFirstTimer") && Convert.ToBoolean(entity["IsFirstTimer"])
                    });
                }

                return new OkObjectResult(registrations);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error fetching registrations");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
