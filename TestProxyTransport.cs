// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

using Azure.Core.Pipeline;
using Azure.Core;
using System.Text.Json;
using System.Text;

// This is an example integration with the Azure record/playback test proxy,
// which requires a custom implementation of the http pipeline transport used
// by the Azure SDK service clients.
// This implementation assumes the test-proxy is already running.
// Your test framework should start and stop the test-proxy process as needed.

namespace Test.Proxy.Transport
{
    // Derived from HttpPipeLineTransport, TestProxyTransport provides a <summary>
    //  custom implementations of the abstract methods defined in the base class
    //  and described above in the HTTP Transport section of this article. These
    //  custom implementations allow us to intercept and reroute app traffic sent
    //  between an app and Azure to the test proxy.
    public class TestProxyTransport : HttpPipelineTransport
    {
        // _transport is used only to POST messages to the test proxy,
        // in order to start and stop the recording or playback process.
        private readonly HttpPipelineTransport _transport;
        // _host will point to 'localhost' since the test proxy is running locally.
        private readonly string _host;
        // _port will be set to 5001 since that is the port the test proxy automatically binds to.
        private readonly int? _port;
        // _recordingId will contain a unique string provided by the test proxy
        // when a recording is first started.
        private readonly string _recordingId;
        // _mode defines whether the proxy should operate in 'record' or 'playback' mode.
        private readonly string _mode;
      
        // Constructor for our custom http transport.
        public TestProxyTransport(HttpPipelineTransport transport, string host, int? port, string recordingId, string mode)
        {
            _transport = transport;
            _host = host;
            _port = port;
            _recordingId = recordingId;
            _mode = mode;
        }

        // CreateRequest() is called when an http request
        // is made by the service client.
        public override Request CreateRequest()
        {
            return _transport.CreateRequest();
        }

        // Process() and ProcessAsync() are called to service
        // http requests. These methods can be used to inject custom code
        // that modifies an http request, which is how we reroute traffic
        // to the proxy. Rerouting is done by 'stashing' the original request
        // in a request header and changing the reqeuested URI address to
        // the address of the test proxy (localhost:5001 by default).
        // The proxy reads the original request URI out of the header and
        // saves it in a JSON-formatted recording file (if in record mode),
        // or reads it from the JSON recording file (if in playback mode).
        // It will then either forward the request to the internet (record
        // mode) or forward the relevant response to your app (playback mode).
        public override void Process(HttpMessage message)
        {
            RedirectToTestProxy(message);
            _transport.Process(message);
        }

        public override ValueTask ProcessAsync(HttpMessage message)
        {
            RedirectToTestProxy(message);
            return _transport.ProcessAsync(message);
        }

        private void RedirectToTestProxy(HttpMessage message)
        {
            message.Request.Headers.Add("x-recording-id", _recordingId);
            message.Request.Headers.Add("x-recording-mode", _mode);

            var baseUri = new RequestUriBuilder()
            {
                Scheme = message.Request.Uri.Scheme,
                Host = message.Request.Uri.Host,
                Port = message.Request.Uri.Port,
            };
            message.Request.Headers.Add("x-recording-upstream-base-uri", baseUri.ToString());

            message.Request.Uri.Host = _host;
            if (_port.HasValue)
            {
                message.Request.Uri.Port = _port.Value;
            }
        }
    }

    // TestProxyVariables class	encapsulates variables that store values
    // related to the test proxy, such as connection host (localhost),
    // connection port (5001), and mode (record/playback).
    public class TestProxyVariables
    {
        public string? host { get; set; }
        public int? port { get; set; }
        public string? mode { get; set; }
        public string? RecordingId { get; set; }
        public string CurrentRecordingPath
        {
            get
            {
                return Path.Join(System.Environment.CurrentDirectory + "\\..\\..\\..\\recordings", $"{GetType().Assembly.GetName().Name}.json");
            }
        }
        // Maintain an http client for POST-ing to the test proxy to start and stop recording.
        // For your test client, you can either maintain the lack of certificate validation (the test-proxy
        // is making real HTTPS calls, so if your actual api call is having cert issues, those will still surface.
        public static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });
    }

    // Methods to start and stop a running test proxy processing traffic between your app and Azure.
    public class TestProxyMethods
    {
        // StartTextProxy() will initiate a record or playback session by POST-ing a request
        // to a running instance of the test proxy. The test proxy will return a recording ID
        // value in the response header, which we pull out and save as 'x-recording-id'.
        public async Task StartTestProxy(TestProxyVariables tpv)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, $"https://{tpv.host}:{tpv.port}/{tpv.mode}/start");
            message.Content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, string>()
            {
                { "x-recording-file", tpv.CurrentRecordingPath }
            }), Encoding.UTF8, "application/json");

            var response = await TestProxyVariables._httpClient.SendAsync(message);
            tpv.RecordingId = response.Headers.GetValues("x-recording-id").Single();
        }

        // StopTextProxy() instructs the test proxy to stop recording or stop playback,
        // depending on the mode it is running in. The instruction to stop is made by
        // POST-ing a request to a running instance of the test proxy. We pass in the recording
        // ID and a directive to save the recording (when recording is running).
        // 
        // **Note that if you skip this step your recording WILL NOT be saved.**
        public async Task StopTestProxy(TestProxyVariables tpv)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, $"https://{tpv.host}:{tpv.port}/{tpv.mode}/stop");
            message.Headers.Add("x-recording-id", tpv.RecordingId);
            message.Headers.Add("x-recording-save", bool.TrueString);

            await TestProxyVariables._httpClient.SendAsync(message);
        }
    }
}
