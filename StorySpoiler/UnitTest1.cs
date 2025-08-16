using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoiler.Mobdel;
using System.Net;
using System.Text.Json;


namespace StorySpoiler
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string storySpoilerId;
        private static string responseMsg;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("emarinova", "qwerty");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
           
            return json.GetProperty("accessToken").GetString();

        }

        [Test,Order(1)]
        public void CreateNewStorySpoiler_ShouldReturnCreated()
        {
            var storySpoiler = new
            {
                Title = "New Story Spoiler",
                Description = "New Spoiler",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storySpoiler);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            storySpoilerId = json.GetProperty("storyId").GetString() ?? string.Empty;

            Assert.That(storySpoilerId, Is.Not.Null.And.Not.Empty, "ID should not be null or empty.");

        }

        [Test, Order(2)]

        public void EditSpoilerTitle_SouldReturnOk()
        {
            var editRequest = new StoryDTO
            {
                Title = "Edit Title",
                Description = "Edit Description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{storySpoilerId}", Method.Put);

            request.AddJsonBody(editRequest);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            responseMsg = json.GetProperty("msg").GetString() ?? string.Empty;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseMsg, Is.EqualTo("Successfully edited"));
        }


        [Test,Order(3)]

        public void GetAllStorySpoilers_ShouldReturnlist()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var stories = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(stories, Is.Not.Empty);
        }

        [Test,Order(4)]

        public void DeleteStorySpoiler_ShouldReturnOK()
        {
            var request = new RestRequest($"/api/Story/Delete/{storySpoilerId}", Method.Delete);
            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            responseMsg = json.GetProperty("msg").GetString() ?? string.Empty;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseMsg, Is.EqualTo("Deleted successfully!"));
        }

        [Test,Order(5)]

        public void CreateStorySpoiler_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var storySpoiler = new
            {
                Title = "",
                Description = "",
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storySpoiler);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test,Order(6)]

        public void EdiNonexistingStorySpoiler_ShouldReturnNotFound()
        {
            string fakeID = "4312";
            var editRequest = new StoryDTO
            {
                Title = "Edit Title",
                Description = "Edit Description",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{fakeID}", Method.Put);

            request.AddJsonBody(editRequest);

            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            responseMsg = json.GetProperty("msg").GetString() ?? string.Empty;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(responseMsg, Is.EqualTo("No spoilers..."));
          
        }

        [Test, Order(7)]

        public void DeleteNonexistingStorySpoiler_ShouldReturnBadRequest()
        {
            string fakeID = "4312";
            var request = new RestRequest($"/api/Story/Delete/{fakeID}", Method.Delete);
            var response = client.Execute(request);
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            responseMsg = json.GetProperty("msg").GetString() ?? string.Empty;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseMsg, Is.EqualTo("Unable to delete this story spoiler!"));

        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}