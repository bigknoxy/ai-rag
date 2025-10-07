using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Net.Http;

namespace Tests.Unit;

public class TestRequestLoggingMiddleware
{
    [Fact]
    public async Task Middleware_AllowsRequestThrough()
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging();
            })
            .Configure(app =>
            {
                app.UseMiddleware<AiRag.Api.Services.RequestLoggingMiddleware>();
                app.Run(async context =>
                {
                    await context.Response.WriteAsync("ok");
                });
            });

        using var server = new TestServer(builder);
        using var client = server.CreateClient();

        var resp = await client.GetAsync("/");
        var body = await resp.Content.ReadAsStringAsync();
        Assert.True(resp.IsSuccessStatusCode);
        Assert.Equal("ok", body);
    }
}
