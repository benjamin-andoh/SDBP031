using AgentAssignment.Server.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Bind config for Server:Port and Server:ModelAlias
var configuration = builder.Configuration;
var port = configuration.GetValue<int?>("Server:Port") ?? 5080;
var modelAlias = configuration.GetValue<string?>("Server:ModelAlias") ?? "qwen2.5-0.5b";

// Register the chat completion service. We avoid a compile-time dependency on FoundryLocalChatCompletionService
// because it pulls in environment-specific packages. If that type is available at runtime, we'll try to create it
// via reflection, otherwise a simple not-ready placeholder will be registered (tests replace this with a fake).
// builder.Services.AddSingleton<IChatCompletionService>(sp =>
// {
//     // Try to find the FoundryLocal type by name
//     var type = Type.GetType("AgentAssignment.Server.Contracts.FoundryLocalChatCompletionService, AgentAssignment.Server");
//     if (type is not null)
//     {
//         try
//         {
//             var instance = Activator.CreateInstance(type, modelAlias) as IChatCompletionService;
//             if (instance is not null) return instance;
//         }
//         catch
//         {
//             // fall through to placeholder
//         }
//     }

//     // Placeholder implementation: not ready and will throw if used. Tests replace this registration.
//     return new NotReadyChatCompletionService();
// });

builder.Services.AddSingleton<FoundryLocalChatCompletionService>(
    _ => new FoundryLocalChatCompletionService(modelAlias));
builder.Services.AddSingleton<IChatCompletionService>(
    sp => sp.GetRequiredService<FoundryLocalChatCompletionService>());

builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port);
});

var app = builder.Build();

// Using the Fake service means we do not start Foundry; /health will report ready immediately.

app.MapControllers();

// Note: HTTPS redirection intentionally omitted for local testing.

app.Run();

// Expose Program class for WebApplicationFactory in tests
public partial class Program { }

internal sealed class NotReadyChatCompletionService : IChatCompletionService
{
    public bool IsReady => false;

    public Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Local model not available in this environment.");
}
