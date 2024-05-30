using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using UserProvider.Models;

namespace UserProvider.Functions
{
    public class UpdateUser(ILogger<UpdateUser> logger, DataContext context)
    {
        private readonly ILogger<UpdateUser> _logger = logger;
        private readonly DataContext _context = context;

        [Function("UpdateUser")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "user/{email}")] HttpRequest req, string email)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(body))
            {
                return new BadRequestResult();
            }

            UpdateUserRequest updateUserRequest = null!;
            try
            {
                updateUserRequest = JsonConvert.DeserializeObject<UpdateUserRequest>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<UpdateUserRequest> :: {ex.Message}");
                return new BadRequestResult();
            }

            if (updateUserRequest == null || string.IsNullOrEmpty(updateUserRequest.Email))
            {
                return new BadRequestResult();
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                return new NotFoundResult();
            }

            user.Email = updateUserRequest.Email;
            user.FirstName = updateUserRequest.FirstName;
            user.LastName = updateUserRequest.LastName;
            user.PhoneNumber = updateUserRequest.PhoneNumber;
            user.Bio = updateUserRequest.Bio;

            try
            {
                await _context.SaveChangesAsync();

                using var http = new HttpClient();
                StringContent content = new StringContent(JsonConvert.SerializeObject(new { Email = user.Email }), Encoding.UTF8, "application/json");
                var response = await http.PostAsync("https://silicon-newsletterprovider.azurewebsites.net/api/subscriber/updateuser", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"External update API call failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"http.PostAsync :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }

            return new OkObjectResult(user);
        }
    }
}
