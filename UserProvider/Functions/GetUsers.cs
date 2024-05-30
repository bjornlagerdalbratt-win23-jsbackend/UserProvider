using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UserProvider.Functions
{
    public class GetUsers(ILogger<GetUsers> logger, DataContext context)
    {
        private readonly ILogger<GetUsers> _logger = logger;
        private readonly DataContext _context = context;

        [Function("GetUsers")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                if (users.Count != 0)
                {
                    return new OkObjectResult(users);
                }

                return new NotFoundResult();
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetUsers :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
