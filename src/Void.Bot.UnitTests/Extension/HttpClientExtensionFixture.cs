using System.Net;
using System.Net.Http.Json;
using Shouldly;
using Void.Bot.Core.Extension;

namespace Void.Bot.UnitTest.Extension;

public class HttpClientExtensionFixture
{
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    public async Task EmptyJsonResponseShouldReturnDefault(HttpStatusCode statusCode)
    {
        using var response = new HttpResponseMessage(statusCode)
        {
            Content = new ByteArrayContent([])
        };

        var result = await HttpClientExtension.ReadResponseContentAsAsync<TestResponse>(
            response, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task EmptyJsonResponseShouldReturnDefaultWithoutContentLength()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new UnknownLengthEmptyContent()
        };

        var result = await HttpClientExtension.ReadResponseContentAsAsync<TestResponse>(
            response, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task EmptyJsonResponseShouldReturnDefaultForNonSeekableStream()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new NonSeekableEmptyContent()
        };

        var result = await HttpClientExtension.ReadResponseContentAsAsync<TestResponse>(
            response, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task WhitespaceJsonResponseShouldReturnDefault()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(" \r\n\t")
        };

        var result = await HttpClientExtension.ReadResponseContentAsAsync<TestResponse>(
            response, CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task JsonResponseShouldStillDeserializeNormally()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new TestResponse { Value = "ok" })
        };

        var result = await HttpClientExtension.ReadResponseContentAsAsync<TestResponse>(
            response, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Value.ShouldBe("ok");
    }

    private sealed class TestResponse
    {
        public string Value { get; set; } = string.Empty;
    }

    private class UnknownLengthEmptyContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    private sealed class NonSeekableEmptyContent : UnknownLengthEmptyContent
    {
        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            return Task.FromResult<Stream>(new NonSeekableEmptyStream());
        }
    }

    private sealed class NonSeekableEmptyStream : MemoryStream
    {
        public override bool CanSeek => false;

        public override long Position
        {
            get => base.Position;
            set => throw new NotSupportedException();
        }
    }
}
