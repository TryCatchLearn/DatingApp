using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        // Only log activity for authenticated requests
        if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        var userId = resultContext.HttpContext.User.GetUserId();
        if (!userId.HasValue) return;

        var uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

        var user = await uow.UserRepository.GetUserByIdAsync(userId.Value);
        if (user == null) return;

        user.LastActive = DateTime.UtcNow;

        await uow.Complete();
    }
}
