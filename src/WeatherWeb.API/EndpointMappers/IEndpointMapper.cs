namespace WeatherWeb.EndpointMappers;

/// <summary>
/// Can be used to mark all endpoint mappers in order to discover them and map them at startup through reflection or simply to have a common interface for all mappers in order to map them manually.
/// </summary>
public interface IEndpointMapper
{
    void Map(IEndpointRouteBuilder endpointRouteBuilder);
}