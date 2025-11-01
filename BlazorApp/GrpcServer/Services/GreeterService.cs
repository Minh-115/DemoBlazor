using Grpc.Core;
namespace GrpcServer.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            //DoWork();
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new RpcException(
                    new Status(StatusCode.InvalidArgument, "Name is required")
                );
            }

            if (!context.GetHttpContext().User.Identity?.IsAuthenticated ?? false)
            {
                throw new RpcException(
                    new Status(StatusCode.Unauthenticated, "Bạn chưa đăng nhập")
                );
            }

            return Task.FromResult(new HelloReply
            {
                Message = "Hello from GRPC " + request.Name
            });
        }

        private void DoWork()
        {
            throw new InvalidOperationException("Bug trong DoWork");
        }

    }
}
