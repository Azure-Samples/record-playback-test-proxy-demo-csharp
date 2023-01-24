Software testing is a crucial step in the software development process,
and testing software on cloud services like Azure can be expensive.

Here at Microsoft, we've developed a lightweight test proxy that
allows us to record app interactions with Azure and play them back on
demand, reducing our testing costs significantly. we're now excited to
share this tool with the broader Azure development community and invite
you to try it out for yourself. 

This repository contains a sample project that demonstrates integration
of the record and playback test proxy with an app that interacts with 
the Azure Cosmos DB Table Storage service.
The included recording file is provided for illustration purposes only,
it can't be used to play back the test since the resources associated
with it no longer exist in Azure. 

This project is intended to be a demo that goes with the following Azure
SDK blog post:
<blog post link TBD>

The test proxy is compatible with all four major languages and can be
easily installed using the standard dotnet tool installation process as
described in the blog post. To use it, you\'ll need to be able to reroute
your app requests to the test proxy via modifications to the request
headers.
