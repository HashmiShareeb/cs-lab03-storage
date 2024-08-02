using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using lab03_storage.Models;

namespace MCT.Function
{
    public static class AddRegistrations
    {
        [FunctionName("AddRegistrations")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/registrations")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                //convert JSON to model + new registration object aanmaken
                var newRegistration = JsonConvert.DeserializeObject<Registrations>(requestBody); //convertereen van model in JSON
                //registratie object met randmom id 
                newRegistration.RegistationId = Guid.NewGuid();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "INSERT INTO Registrations (RegistrationId, LastName, FirstName, Email, ZipCode, Age, IsFirstTimer) VALUES (@RegistrationId, @LastName, @FirstName, @Email, @ZipCode, @Age, @IsFirstTimer)";
                        command.Parameters.AddWithValue("@RegistrationId", newRegistration.RegistationId);
                        command.Parameters.AddWithValue("@LastName", newRegistration.lastName);
                        command.Parameters.AddWithValue("@FirstName", newRegistration.firstName);
                        command.Parameters.AddWithValue("@Email", newRegistration.Email);
                        command.Parameters.AddWithValue("@ZipCode", newRegistration.ZipCode);
                        command.Parameters.AddWithValue("@Age", newRegistration.Age);
                        command.Parameters.AddWithValue("@IsFirstTimer", newRegistration.IsFirstTimer);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return new OkObjectResult(newRegistration);
            }
            catch (System.Exception ex)
            {

                log.LogError(ex, "Error adding registrations");
                return new BadRequestObjectResult(500);
            }
        }
    }
}
