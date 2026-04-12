using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Android.Net;

namespace Facesofnaija.Helpers.Utils
{
    /// <summary>
    /// Custom HTTP handler that forces HTTP for local development with XAMPP
    /// Converts HTTPS requests to 10.0.2.2 to HTTP since XAMPP doesn't have SSL enabled
    /// </summary>
    public class LocalHttpHandler : AndroidMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // If the request is HTTPS to 10.0.2.2 (emulator localhost), convert to HTTP
                if (request.RequestUri.Scheme == "https" && 
                    (request.RequestUri.Host == "10.0.2.2" || request.RequestUri.Host == "localhost"))
                {
                    var httpUri = new UriBuilder(request.RequestUri)
                    {
                        Scheme = "http",
                        Port = 80 // Explicitly set HTTP port
                    };
                    request.RequestUri = httpUri.Uri;
                    
                    Console.WriteLine($"[LocalHttpHandler] Converted HTTPS to HTTP: {request.RequestUri}");
                }

                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalHttpHandler] Error: {ex.Message}");
                throw;
            }
        }
    }
}
