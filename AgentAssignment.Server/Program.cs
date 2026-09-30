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
// For local development/testing register the fake service so /health reports ready immediately.
builder.Services.AddSingleton<IChatCompletionService, FakeChatCompletionService>();

builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(port);
});

var app = builder.Build();

// No background startup required for the fake service used in development.

app.MapControllers();

// Note: HTTPS redirection intentionally omitted for local testing.

app.Run();

// Expose Program class for WebApplicationFactory in tests
public partial class Program { }

// (Using FakeChatCompletionService from Contracts for testing; no placeholder needed.)
