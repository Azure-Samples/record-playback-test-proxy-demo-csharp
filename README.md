Testing software deployed to cloud services like Azure can incur significant
costs when provisioning and maintaining the services needed for testing.

Here at Microsoft, we've developed a lightweight test proxy that
allows us to record app interactions with Azure and play them back on
demand, reducing our testing costs significantly. We're now excited to
share this tool with the broader Azure development community and invite
you to try it out for yourself.

This repository contains a sample project that demonstrates integration
of the test-proxy with an app that interacts with the Azure Cosmos DB Table
Storage service.

You must have the test proxy installed and running before starting the test.

To install the proxy:

[Install .NET 6.0 or higher](https://dotnet.microsoft.com/download)

Install the test-proxy:

```bash
dotnet tool update azure.sdk.tools.testproxy --global --add-source https://pkgs.dev.azure.com/azure-sdk/public/_packaging/azure-sdk-for-net/nuget/v3/index.json --version "1.0.0-dev*"
```

After installing the test-proxy, run it in a terminal or cmd window by typing the command 'test-proxy'.

Once the test-proxy is installed, before running the project contained within, ensure that the following environment variables are set:

- COSMOS_CONNECTION_STRING
- USE_PROXY
- PROXY_HOST
- PROXY_PORT
- PROXY_MODE

The included recording file is provided for illustration purposes only,
it can't be used to play back the test since the resources associated
with it no longer exist in Azure.

This project is intended to be a demo that goes with the following [Azure
SDK blog post](https://devblogs.microsoft.com/azure-sdk/level-up-your-cloud-testing-game-with-the-azure-sdk-test-proxy/)

The test proxy is compatible with any language that communicates using HTTP traffic. The sole limitation is that
To use it, you must be able to reroute your app requests to the test proxy via modifications to the request headers
priot to actually invoking the response.
