﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Owin.Hosting;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Owin;

namespace System.Web.Http.OwinHost
{
    public class IntegrationTest
    {
        [Fact]
        public void SimpleGet_Works()
        {
            using (WebApplication.Start<IntegrationTest>(url: "http://localhost:50232/vroot"))
            {
                HttpClient client = new HttpClient();

                var response = client.GetAsync("http://localhost:50232/vroot/HelloWorld").Result;

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Hello from OWIN\"", response.Content.ReadAsStringAsync().Result);
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public void SimplePost_Works()
        {
            using (WebApplication.Start<IntegrationTest>(url: "http://localhost:50232/vroot"))
            {
                HttpClient client = new HttpClient();
                var content = new StringContent("\"Echo this\"", Encoding.UTF8, "application/json");

                var response = client.PostAsync("http://localhost:50232/vroot/Echo", content).Result;

                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("\"Echo this\"", response.Content.ReadAsStringAsync().Result);
                Assert.Null(response.Headers.TransferEncodingChunked);
            }
        }

        [Fact]
        public void GetThatThrowsDuringSerializations_RespondsWith500()
        {
            using (WebApplication.Start<IntegrationTest>(url: "http://localhost:50232/vroot"))
            {
                HttpClient client = new HttpClient();

                var response = client.GetAsync("http://localhost:50232/vroot/Error").Result;

                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                JObject json = Assert.IsType<JObject>(JToken.Parse(response.Content.ReadAsStringAsync().Result));
                JToken exceptionMessage;
                Assert.True(json.TryGetValue("ExceptionMessage", out exceptionMessage));
            }
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.Routes.MapHttpRoute("Default", "{controller}");
            appBuilder.UseWebApi(config);
        }
    }

    public class HelloWorldController : ApiController
    {
        public string Get()
        {
            return "Hello from OWIN";
        }
    }

    public class EchoController : ApiController
    {
        public string Post([FromBody] string s)
        {
            return s;
        }
    }

    public class ErrorController : ApiController
    {
        public ExceptionThrower Get()
        {
            return new ExceptionThrower();
        }

        public class ExceptionThrower
        {
            public string Throws
            {
                get
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
