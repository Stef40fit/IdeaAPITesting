using IdeaAPITesting.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IdeaAPITesting
{
    public class IdeaAPITests
    {
        private RestClient client;
        private const string BASEURL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";
        private const string EMAIL = "stefan+idea@gmail.com";
        private const string PASSWORD = "123789";

        private static string LastIdeaId;
        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(EMAIL, PASSWORD);
            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
           RestClient authClient = new RestClient(BASEURL);
            var request = new RestRequest("/api/User/Authentication");
            request.AddJsonBody(new {email, password});

            var response = authClient.Execute(request, Method.Post);
            if (response.StatusCode == HttpStatusCode.OK) 
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if(string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Tokrn is null or empty.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Unexpected response type{response.StatusCode} with{response.Content}");
            }



        }

        [Test, Order(1)]
        public void CreateNewIdia_WithCorrectData_ShouldeSucceed()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "TestTitle",
                Description = "TestDescription"
            };
            //Act
            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(requestData);
            var response = client.Execute(request, Method.Post);
            var resposeData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(resposeData.Msg, Is.EqualTo("Successfully created!"));

        }


        [Test, Order(2)]
        public void GetAllIdeas_ShouldeReturnAllIdeas()
        {
            //Arrange
            
            //Act
            var request = new RestRequest("/api/Idea/All");
            
            var response = client.Execute(request, Method.Get);
            var resposeDataArray = JsonSerializer.Deserialize<ApiResponseDTO[]>(response.Content);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(resposeDataArray.Length, Is.GreaterThan(0));

            LastIdeaId = resposeDataArray[resposeDataArray.Length - 1].IdeaId;
        }

        [Test, Order(3)]
        public void EditeLastIdea_WithCorrectData_ShouldeSucceed()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "EditedTestTitle",
                Description = "Edited Test Description"
            };
            //Act
            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", LastIdeaId);
            request.AddJsonBody(requestData);
            var response = client.Execute(request, Method.Put);
            var resposeEditData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(resposeEditData.Msg, Is.EqualTo("Edited successfully"));

        }

        [Test, Order(4)]
        public void DeleteLastIdea_ShouldeSucceed()
        {
            //Arrange
            
            //Act
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", LastIdeaId);            
            var response = client.Execute(request, Method.Delete);
            
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("The idea is deleted!"));

        }

        [Test, Order(5)]
        public void CreateNewIdia_WithoutCorrectData_ShouldeFail()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "TestTitle"
                
            };
            //Act
            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(requestData);
            var response = client.Execute(request, Method.Post);
            var resposeData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
           

        }

        [Test, Order(6)]
        public void EditeNonExistingIdea_ShouldeFail()
        {
            //Arrange
            var requestData = new IdeaDTO()
            {
                Title = "EditedTestTitle",
                Description = "Edited Test Description"
            };
            //Act
            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", "12345");
            request.AddJsonBody(requestData);
            var response = client.Execute(request, Method.Put);
            //var resposeEditData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));

        }

        [Test, Order(7)]
        public void DeleteNonExistingIdea_ShouldeFail()
        {
            //Arrange

            //Act
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", "112233");
            var response = client.Execute(request, Method.Delete);

            //Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));

        }
    }
}