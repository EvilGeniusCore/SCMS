using System.Net;

namespace SCMS.Tests.Integration
{
    [Collection("Sequential")]
    public class AuthTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task AdminSettings_Unauthenticated_RedirectsToLogin()
        {
            var response = await _client.GetAsync("/admin/settings");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("portal-access", response.Headers.Location?.ToString() ?? "");
        }

        [Fact]
        public async Task AdminNavContent_Unauthenticated_RedirectsToLogin()
        {
            var response = await _client.GetAsync("/admin/navcontent");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("portal-access", response.Headers.Location?.ToString() ?? "");
        }

        [Fact]
        public async Task AdminSocialMedia_Unauthenticated_RedirectsToLogin()
        {
            var response = await _client.GetAsync("/admin/socialmedia");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("portal-access", response.Headers.Location?.ToString() ?? "");
        }

        [Fact]
        public async Task AdminUpload_Unauthenticated_RedirectsToLogin()
        {
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(new byte[] { 0 }), "file", "test.jpg");

            var response = await _client.PostAsync("/admin/upload/image", content);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
    }
}
