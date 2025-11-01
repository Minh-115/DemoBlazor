using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client.Configuration;

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
        try
        {
            // Cho phép request chạy tiếp
            return await continuation(request, context);
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
                "Unhandled exception in gRPC method {Method}. Exception={ExceptionType}",
                methodName,
                ex.GetType().Name
            );

            throw new Exception(
                "Đã xảy ra lỗi hệ thống"
            );
        }
    }
}
