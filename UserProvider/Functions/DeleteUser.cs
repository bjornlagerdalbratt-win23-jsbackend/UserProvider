using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using UserProvider.Models;

namespace UserProvider.Functions;

public class DeleteUser(ILogger<DeleteUser> logger, DataContext context)
{
    private readonly ILogger<DeleteUser> _logger = logger;
    private readonly DataContext _context = context;

    [Function("DeleteUser")]
    public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "delete")] HttpRequest req)
    {
        string body;

        try
        {
            using var reader = new StreamReader(req.Body);
            body = await reader.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamReader :: {ex.Message}");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        if (!string.IsNullOrEmpty(body))
        {
            DeleteUserRequest deleteUserRequest;
            try
            {
                deleteUserRequest = JsonConvert.DeserializeObject<DeleteUserRequest>(body)!;
            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<UnsubscribeRequest> :: {ex.Message}");
                return new BadRequestResult();
            }

            if (deleteUserRequest != null && !string.IsNullOrEmpty(deleteUserRequest.Email))
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == deleteUserRequest.Email);
                if (user != null)
                {
                    _context.Users.Remove(user);

                    try
                    {
                        await _context.SaveChangesAsync();

                        using var http = new HttpClient();
                        StringContent content = new StringContent(JsonConvert.SerializeObject(new { Email = user.Email }), Encoding.UTF8, "application/json");
                        var response = await http.PostAsync("https://silicon-newsletterprovider.azurewebsites.net/api/removeuser", content);

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError($"External unsubscribe API call failed with status code {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"http.PostAsync :: {ex.Message}");
                        return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                    }

                    return new OkResult();
                }
                else
                {
                    return new NotFoundResult();
                }
            }
        }
        return new BadRequestResult();
    }
}
