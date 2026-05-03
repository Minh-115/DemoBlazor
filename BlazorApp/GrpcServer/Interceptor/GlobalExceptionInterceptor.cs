using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client.Configuration;
using System.Text.Json;

public class GlobalExceptionInterceptor : Interceptor
{
    private readonly ILogger<GlobalExceptionInterceptor> _logger;

    public GlobalExceptionInterceptor(ILogger<GlobalExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var methodName = context.Method;
        var requestType = typeof(TRequest).Name;

        try
        {
            // Log request vào
            _logger.LogInformation(
                "gRPC Request => Method: {Method}, RequestType: {RequestType}, Payload: {Payload}",
                methodName,
                requestType,
                SafeSerialize(request)
            );

            var response = await continuation(request, context);

            // Log response ra
            _logger.LogInformation(
                "gRPC Response <= Method: {Method}, ResponseType: {ResponseType}, Payload: {Payload}",
                methodName,
                typeof(TResponse).Name,
                SafeSerialize(response)
            );

            return response;
        } 
        catch (RpcException ex)
        {
            _logger.LogWarning(ex,
                "gRPC error in method {Method}. StatusCode={StatusCode}, Detail={Detail}",
                methodName,
                ex.StatusCode,
                ex.Status.Detail
            );

            throw; // giữ nguyên RPC exception
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Loi gRPC ham {Method}. Exception={ExceptionType}, Message={Message}",
                methodName,
                ex.GetType().Name,
                ex.Message
            );

            throw new RpcException(
                new Status(
                    StatusCode.Internal,
                    ex.Message
                )
            );
        }
    }
    private static string SafeSerialize<T>(T obj)
    {
        if (obj == null)
        {
            return "null";
        }

        try
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch
        {
            return $"[Cannot serialize type: {typeof(T).Name}]";
        }
    }
}
