using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rapr.Tests
{
    [TestClass]
    public class HttpClientExtensionsTests
    {
        [TestMethod]
        public void FailedDownloadIsNotSavedAsTheFile()
        {
            using (var client = new HttpClient(new FixedResponseHandler(HttpStatusCode.NotFound, "<html>Not Found</html>")))
            using (var destination = new MemoryStream())
            {
                var exception = Assert.ThrowsExactly<AggregateException>(
                    () => client.DownloadAsync(new Uri("https://github.com/missing.zip"), destination).Wait());

                Assert.IsInstanceOfType<HttpRequestException>(exception.InnerException);
                Assert.AreEqual(0, destination.Length);
            }
        }

        [TestMethod]
        public void SuccessfulDownloadIsCopied()
        {
            using (var client = new HttpClient(new FixedResponseHandler(HttpStatusCode.OK, "zip contents")))
            using (var destination = new MemoryStream())
            {
                client.DownloadAsync(new Uri("https://github.com/release.zip"), destination).Wait();

                Assert.AreEqual("zip contents", Encoding.UTF8.GetString(destination.ToArray()));
            }
        }

        private sealed class FixedResponseHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode statusCode;
            private readonly string content;

            public FixedResponseHandler(HttpStatusCode statusCode, string content)
            {
                this.statusCode = statusCode;
                this.content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(this.statusCode)
                {
                    Content = new StringContent(this.content),
                });
            }
        }
    }
}
