namespace Dipchik;

public static class WebUtils
{
    public static string? UserId(this HttpContext context)
        => context.User.Identity?.Name;
}