using Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UserProvider.Functions
{
    public class GetOneUser(ILogger<GetOneUser> logger, DataContext context)
    {
        private readonly ILogger<GetOneUser> _logger = logger;
        private readonly DataContext _context = context;

        [Function("GetOneUser")]
        public async Task <IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "edituser")] HttpRequest req)
        {
            string id = req.Query["id"];

            if (string.IsNullOrEmpty(id))
            {
                return new BadRequestObjectResult("Please pass an id on the query string");
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (user != null)
                {
                    return new OkObjectResult(user);
                }
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetOneSubscriber :: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
