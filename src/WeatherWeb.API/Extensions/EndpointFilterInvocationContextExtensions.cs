namespace WeatherWeb.Extensions;

public static class EndpointFilterInvocationContextExtensions
{
    public static T? GetService<T>(this EndpointFilterInvocationContext context)
    {
        return context.HttpContext.RequestServices.GetService<T>();
    }

    public static T GetRequiredService<T>(this EndpointFilterInvocationContext context)
        where T : notnull
    {
        return context.HttpContext.RequestServices.GetRequiredService<T>();
    }   
}
