using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SCMS.Data;

namespace SCMS.Tests.Integration
{
    [Collection("Sequential")]
    public class PageRenderingTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public PageRenderingTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task HomePage_ReturnsSuccessAndContent()
        {
            var response = await _client.GetAsync("/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Welcome", content); // From HasData seed
        }

        [Fact]
        public async Task HomePage_DefaultSlug_ReturnsHome()
        {
            var response = await _client.GetAsync("/home");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Welcome", content);
        }

        [Fact]
        public async Task NonExistentPage_Returns404()
        {
            var response = await _client.GetAsync("/this-page-does-not-exist");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task HomePage_ContainsHtmlStructure()
        {
            var response = await _client.GetAsync("/");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Contains("<html", content);
            Assert.Contains("</html>", content);
        }

        [Fact]
        public async Task PortalAccessPage_ReturnsLogin()
        {
            var response = await _client.GetAsync("/portal-access");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Login", content);
        }
    }
}
