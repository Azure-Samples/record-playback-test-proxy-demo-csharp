// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------

using Azure.Data.Tables;
using Azure.Core.Pipeline;
using Test.Proxy.Transport;

// Beginning of app code.
class CosmosDBTablesTestProxy
{    
    static async Task Main()
    {
        //=====================================================================//
        // Test proxy prologue. The following code is necessary to configure   //
        // the test proxy, as well as to start the record or playback process. // 
        //=====================================================================//
        // Load environment variables from the local .env file
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, "..\\..\\..\\.env");
        Console.WriteLine(dotenv);
        ParseDotEnvFile.ParseDotEnvFile.Load(dotenv);

        var tpv = new TestProxyVariables();
        var tpm = new TestProxyMethods();
        var use_proxy = Environment.GetEnvironmentVariable("USE_PROXY");
        var table_options = new TableClientOptions();

        // Override the http transport via TableClientOptions when using the test proxy.
        // If not using the proxy, the default client http transport will be used.
        if (use_proxy == "true")
        {
            tpv.host = Environment.GetEnvironmentVariable("PROXY_HOST")!;
            tpv.port = Int32.Parse(Environment.GetEnvironmentVariable("PROXY_PORT")!);
            tpv.mode = Environment.GetEnvironmentVariable("PROXY_MODE");
            await tpm.StartTestProxy(tpv);
            table_options.Transport = new TestProxyTransport(new HttpClientTransport(TestProxyVariables._httpClient),
                                                             tpv.host, tpv.port, tpv.RecordingId!, tpv.mode!);
        }

        //=========================================================================================//
        // End of test proxy prologue. Original test code starts here. Everything after this point //
        // represents an app interacting with the Azure Table Storage service.                     //
        //=========================================================================================//

        TableServiceClient tableServiceClient = new TableServiceClient(Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING"), table_options);

        // New instance of TableClient class referencing the server-side table
        TableClient tableClient = tableServiceClient.GetTableClient(
            tableName: "adventureworks"
        );
        await tableClient.CreateIfNotExistsAsync();

        // Create new item using composite key constructor
        var prod1 = new Product()
        {
            RowKey = "68719518388",
            PartitionKey = "gear-surf-surfboards",
            Name = "Ocean Surfboard",
            Quantity = 8,
            Sale = true
        };

        // Add new item to server-side table
        await tableClient.AddEntityAsync<Product>(prod1);

        // Read a single item from container
        var product = await tableClient.GetEntityAsync<Product>(
            rowKey: "68719518388",
            partitionKey: "gear-surf-surfboards"
        );
        Console.WriteLine("Single product:");
        Console.WriteLine(product.Value.Name);

        // Read multiple items from container
        var prod2 = new Product()
        {
            RowKey = "68719518390",
            PartitionKey = "gear-surf-surfboards",
            Name = "Sand Surfboard",
            Quantity = 5,
            Sale = false
        };

        await tableClient.AddEntityAsync<Product>(prod2);

        var products = tableClient.Query<Product>(x => x.PartitionKey == "gear-surf-surfboards");

        Console.WriteLine("Multiple products:");
        foreach (var item in products)
        {
            Console.WriteLine(item.Name);
        }

        object deleteVal = tableClient.Delete();

        //=============================================================================//
        // Test proxy epilogue - necessary to stop the test proxy. Note that if you do //
        // not stop the test proxy after recording, your recording WILL NOT be saved!  //
        //=============================================================================//

        if (use_proxy == "true")
        {
            await tpm.StopTestProxy(tpv);
        }
    }
}