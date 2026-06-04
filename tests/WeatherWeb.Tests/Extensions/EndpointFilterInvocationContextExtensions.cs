using Microsoft.AspNetCore.Http;
using Moq;

namespace WeatherWeb.Tests.Extensions;

internal static class EndpointFilterInvocationContextExtensions
{
    public static Mock<EndpointFilterInvocationContext> AddArgumentToMock<T>(this Mock<EndpointFilterInvocationContext> mockFilter, T instance, int index = 0)
    {
        mockFilter.Setup(filter => filter.GetArgument<T>(index)).Returns(instance);
        return mockFilter;
    }

    public static Mock<EndpointFilterInvocationContext> AddServiceToMock<T>(this Mock<EndpointFilterInvocationContext> endpointFilter, T instance)
    {
        endpointFilter.Setup(f => f.HttpContext.RequestServices.GetService(typeof(T))).Returns(instance);
        return endpointFilter;
    }
}
