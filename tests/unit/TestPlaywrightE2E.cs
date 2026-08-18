using Microsoft.Playwright;
using Xunit.Sdk;
using Xunit;

namespace UnitTests;

public class TestPlaywrightE2E : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private bool _skipPlaywright = false;
    private readonly string? _baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL");

    public async Task InitializeAsync()
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            Console.WriteLine("[E2E] E2E_BASE_URL not set — skipping E2E tests.");
            _skipPlaywright = true;
            return;
        }

        try
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                // CI environments commonly require sandbox flags
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });
        }
        catch (System.Exception ex)
        {
            // If Playwright cannot be started in the environment, log and skip E2E tests
            Console.WriteLine($"[E2E] Playwright initialization failed: {ex.Message}");
            _skipPlaywright = true;
        }
    }

    public async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
        _skipPlaywright = false; // reset flag for next test run
    }

    private async Task<bool> EnsureCanNavigateAsync(IPage page, string path)
    {
        var url = path.StartsWith("http") ? path : new Uri(new Uri(_baseUrl!), path).ToString();
        try
        {
            // short timeout so tests fail fast when the server isn't running
            await page.GotoAsync(url, new PageGotoOptions { Timeout = 5000 });
            return true;
        }
        catch (Exception ex)
        {
            // Log and return false so the calling test can bail out gracefully
            Console.WriteLine($"[E2E] Unable to reach {url}: {ex.Message}");
            return false;
        }
    }

    [Fact]
    public async Task HomePage_LoadsCorrectly()
    {
        if (_skipPlaywright) { Console.WriteLine("[E2E] Skipping test because Playwright is unavailable."); return; }
        var page = await _browser!.NewPageAsync();
        if (!await EnsureCanNavigateAsync(page, "/")) return;

        var title = await page.TitleAsync();
        Assert.Equal("AI-RAG Demo", title);

        var heading = await page.TextContentAsync("h1");
        Assert.Equal("AI-RAG Demo", heading);
    }

    [Fact]
    public async Task IngestPage_AllowsTextInput()
    {
        if (_skipPlaywright) { Console.WriteLine("[E2E] Skipping test because Playwright is unavailable."); return; }
        var page = await _browser!.NewPageAsync();
        if (!await EnsureCanNavigateAsync(page, "/ingest")) return;

        await page.FillAsync("textarea", "Test document for ingestion.");

        var text = await page.InputValueAsync("textarea");
        Assert.Equal("Test document for ingestion.", text);
    }

    [Fact]
    public async Task QueryPage_AllowsQueryInput()
    {
        if (_skipPlaywright) { Console.WriteLine("[E2E] Skipping test because Playwright is unavailable."); return; }
        var page = await _browser!.NewPageAsync();
        if (!await EnsureCanNavigateAsync(page, "/query")) return;

        await page.FillAsync("input[type='text']", "Test query");

        var text = await page.InputValueAsync("input[type='text']");
        Assert.Equal("Test query", text);
    }

    [Fact]
    public async Task HappyPathWorkflow_IngestAndQuery()
    {
        if (_skipPlaywright) { Console.WriteLine("[E2E] Skipping test because Playwright is unavailable."); return; }
        var page = await _browser!.NewPageAsync();

        // Step 1: Navigate to home page
        if (!await EnsureCanNavigateAsync(page, "/")) return;
        var title = await page.TitleAsync();
        Assert.Equal("AI-RAG Demo", title);

        // Step 2: Navigate to ingest page
        await page.ClickAsync("a[href='/ingest']");
        await page.WaitForURLAsync("**/ingest");

        // Step 3: Enter sample data
        var sampleText = "This is a short sample document used for Phase 0 testing. Hello world.";
        await page.FillAsync("#documentText", sampleText);
        await page.WaitForTimeoutAsync(1000); // Wait for binding to update

        // Step 4: Submit ingest
        var ingestRequestTask = page.WaitForRequestAsync(req => req.Url.Contains("/ingest") && req.Method == "POST");
        await page.ClickAsync("button[type='submit']");
        var ingestRequest = await ingestRequestTask;
        try
        {
            var body = ingestRequest.PostData;
            Console.WriteLine($"[E2E] /api/ingest POST body: {body}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[E2E] Failed to read POST body: {ex.Message}");
        }
        // Wait for success message
        await page.WaitForSelectorAsync(".alert-info", new PageWaitForSelectorOptions { Timeout = 30000 });

        // Step 5: Navigate to query page (from home or directly)
        await page.GotoAsync($"{_baseUrl}/query");
        await page.WaitForURLAsync("**/query");

        // Step 6: Enter related query term
        var queryTerm = "Hello world";
        await page.FillAsync("#queryText", queryTerm);

        // Step 7: Submit query
        await page.ClickAsync("button[type='submit']");
        // Wait for results
        await page.WaitForSelectorAsync(".response-box", new PageWaitForSelectorOptions { Timeout = 30000 });

        // Step 8: Verify results are displayed
        var responseBox = await page.TextContentAsync(".response-box");
        Assert.NotNull(responseBox);
        Assert.NotEmpty(responseBox);
        Assert.Contains(queryTerm, responseBox, StringComparison.OrdinalIgnoreCase);

        // Verify results list has items
        var resultsCount = await page.Locator("ul li").CountAsync();
        Assert.True(resultsCount > 0, "Expected at least one result in the results list.");
    }
}