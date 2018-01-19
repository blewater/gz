using System;
using AngleSharp;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;

public static void Run(string myQueueItem, TraceWriter log)
{
    var configuration = Configuration.Default.WithDefaultLoader().WithCookies();
    var context = BrowsingContext.New(configuration);    
    log.Info($"angle sharp context is created? {context != null}");
    log.Info($"C# Queue trigger function processed: {myQueueItem}");
}
