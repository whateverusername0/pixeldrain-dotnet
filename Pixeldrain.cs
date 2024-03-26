using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Pixeldrain
{
    // copyright ME at https://github.com/whateverusername0/pixeldrain-dotnet
    public class PixeldrainClient : IDisposable
    {
        public string APIKey { get; private set; }
        private readonly HttpClient _httpClient;
        private const string _endpoint = "https://pixeldrain.com/api";
        public ILogger Log { get; private set; }

        public PixeldrainClient(string apiKey = "")
        {
            APIKey = apiKey;
            _httpClient = new HttpClient();
            Log = new NullLogger();
        }
        public PixeldrainClient(ILogger log, string apiKey = "") : this(apiKey)
        {
            Log = log;
        }

        /// <summary>
        /// Uploads a file to pixeldrain and returns it's id.
        /// </summary>
        /// <param name="Path">Full path to the file you want to upload.</param>
        /// <returns>The id to the file on pixeldrain.</returns>
        /// <exception cref="PixeldrainHttpException"></exception>
        public async Task<string> UploadFile(string Path)
        {
            var content = new MultipartContent { new ByteArrayContent(File.ReadAllBytes(Path)) };
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("file")
            { FileName = Path.Split('\\')[Path.Split('\\').Length - 1] };

            var req = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{_endpoint}/file/{Path.Split('\\')[Path.Split('\\').Length - 1]}"),
                Method = HttpMethod.Put,
                Content = content,
            };
            var res = await _httpClient.SendAsync(req);

            var resObj = JObject.Parse(await res.Content.ReadAsStringAsync());
            if (!res.IsSuccessStatusCode) throw new PixeldrainHttpException(resObj);

            var id = resObj["id"].ToString();
            Log.LogDebug($"Successfully uploaded {Path} and recieved an ID of {id}.");
            return id;
        }

        /// <summary>
        /// Downloads a file from pixeldrain and returns a byte array containing the file.
        /// </summary>
        /// <param name="ID">The id of a file located on pixeldrain.</param>
        /// <param name="Path">Full path to where you want to save it. e.g. C:\Path1\file.txt</param>
        /// <returns>A byte array containing the file.</returns>
        /// <exception cref="PixeldrainHttpException"></exception>
        public async Task<byte[]> DownloadFile(string ID, string Path = "")
        {
            var req = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{_endpoint}/file/{ID}"),
                Method = HttpMethod.Get,
            };
            var res = await _httpClient.SendAsync(req);

            if (!res.IsSuccessStatusCode) throw new PixeldrainHttpException(JObject.Parse(await res.Content.ReadAsStringAsync()));
            var retVal = await res.Content.ReadAsByteArrayAsync();

            if (!string.IsNullOrWhiteSpace(Path))
                try { File.WriteAllBytes(Path, retVal); }
                catch (Exception e) { Log.LogError($"{e.Message} : {e.InnerException}"); }

            Log.LogDebug($"Successfully downloaded file {ID}{(!string.IsNullOrWhiteSpace(Path) ? $" and probably saved it to {Path}" : "")}.");
            return retVal;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>
    /// Occurs when there's an <see cref="HttpRequestException"/> or something.
    /// </summary>
    [Serializable] public class PixeldrainHttpException : Exception
    {
        public PixeldrainHttpException(JObject responseObject)
        { new Exception($"{responseObject["value"]} : {responseObject["message"]}"); }
    }

    /// <summary>
    /// This exists as a default logger for <see cref="PixeldrainClient"/>.
    /// It literally does absolutely nothing and exists just to avoid logger exceptions.
    /// </summary>
    public class NullLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {}
    }
}