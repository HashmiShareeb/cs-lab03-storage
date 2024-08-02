using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using lab03_storage.Models;
using System.Data.SqlClient;

namespace MCT.Function
{
    public static class GetRegistrations
    {
        [FunctionName("GetRegistrations")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/registrations")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {
                string connectionString = Environment.GetEnvironmentVariable("connectionString");

                List<Registrations> registrations = new List<Registrations>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT * FROM Registrations";
                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            Registrations registration = new Registrations();
                            {
                                registration.RegistationId = reader.GetGuid(0);
                                registration.lastName = reader.GetString(1);
                                registration.firstName = reader.GetString(2);
                                registration.Email = reader.GetString(3);
                                registration.ZipCode = reader.GetString(4);
                                registration.Age = reader.GetInt32(5);
                                registration.IsFirstTimer = reader.GetBoolean(6);
                            }

                            registrations.Add(registration);
                        }
                    }
                }

                return new OkObjectResult(registrations);

            }
            catch (System.Exception ex)
            {

                log.LogError(ex, "Error getting registrations");
                return new BadRequestObjectResult(500);
            }

        }
    }
}
