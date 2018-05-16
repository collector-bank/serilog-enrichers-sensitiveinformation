[![Build status](https://ci.appveyor.com/api/projects/status/e02riadn068cgwn6/branch/master?svg=true)](https://ci.appveyor.com/project/CollectorHeimdal/serilog-enricher-sensitiveinformation/branch/master)
# Sensitive Information Enricher

This repository includes the Serilog enricher SensitiveInformationEnricher, that will extract out and isolate all sensitive information in a log message.

DISCLAIMER: It is VERY important that you read to the very end of this readme if you intend to use this library, for it is at the end where you will learn how this will actually make us able to retain your log messages for longer than 3 months.

## Step 1 - Enricher

Add the enricher:

```csharp
var logger = new LoggerConfiguration()
  // Your other enrichers. Make sure the sensitive information enricher is configured LAST.
             .Enrich.With(new SensitiveInformationEnricher())
             .WriteTo....
             .CreateLogger();
```

Note: The constructor of the enricher can be given a list of blacklisted property names. This blacklist is the only way to mark information created by other enrichers as sensitive, which is why you should always add this enricher last. So for example say that you are using the HttpRequestClientHostIPEnricher from the SerilogWeb.Classic package then that enricher will add a very sensitive data point to your log event with the name HttpRequestClientHostIP. to handle that, simply add it to the blacklist:

```csharp
var logger = new LoggerConfiguration()
             .Enrich.With<HttpRequestClientHostIPEnricher>()
             .Enrich.With(new SensitiveInformationEnricher("HttpRequestClientHostIP"))
             .WriteTo....
             .CreateLogger();
```
This blacklist is already pre-populated with all the values of the [SensitiveInformationType](https://github.com/collector-bank/serilog-enricher-sensitiveinformation/blob/master/src/Collector.Serilog.SensitiveInformation/SensitiveInformationType.cs) enum.

## Step 2 - Destructuring policies

Add destructuring polices to mark information as sensitive. There are 3 ways to do this at the moment:

#### Step 2.1 - AsSensitive

```csharp
var logger = new LoggerConfiguration()
            .Destructure.AsSensitive<MyClass>(stringify: false)
            .Enrich.With(new SensitiveInformationEnricher())
            .WriteTo....
            .CreateLogger();
```
This destructuring policy marks the given class as sensitive. A parameter can be given to tell the policy whether the object should be logged as-is (with all its properties destructured), or if the object should be stringified - i.e have its .ToString() method called.

#### Step 2.2 - HasSensitiveProperties

```csharp
var logger = new LoggerConfiguration()
            .Destructure.HasSensitiveProperties<MyClass>(
                myclass => myclass.Prop1, 
                myclass => myclass.Prop2)
            .Enrich.With(new SensitiveInformationEnricher())
            .WriteTo....
            .CreateLogger();
```

This destructuring policy will make sure that whenever anyone logs an object of the type MyClass then the given properties are marked as sensitive, while all other properties of the object will be logged as usual.

#### Step 2.3 - HasNonSensitiveProperties

```csharp
var logger = new LoggerConfiguration()
            .Destructure.HasNonSensitiveProperties<MyClass>(
                myclass => myclass.Prop3, 
                myclass => myclass.Prop4)
            .Enrich.With(new SensitiveInformationEnricher())
            .WriteTo....
            .CreateLogger();
```

This destructuring policy will make sure that whenever anyone logs an object of the type MyClass then the given properties are logged as usual, while all other properties of the object will be logged as sensitive.

#### Step 2.4 - AsSensitiveByTransforming

```csharp
var logger = new LoggerConfiguration()
            .Destructure.AsSensitiveByTransforming<MyClass>(myclass => new 
                                                            {
                                                                Prop1 = myclass.SomeProp,
                                                                Prop2 = myclass.OtherProp 
                                                            })
            .Enrich.With(new SensitiveInformationEnricher())
            .WriteTo....
            .CreateLogger();
```
This destructuring policy will apply a transform to any object of type MyClass the same way as the standard .ByTransforming<>() policy, but it will also mark the result as sensitive.

## Step 3 - When logging

First of all, never EVER mix sensitive information in the message template of a log message. This library will not help you with that. So simply don't.

#### Step 3.1 - ForContext & WithSensitiveInformation

When logging it is instead recommended to use the .ForContext() method:

```csharp
var mc = new MyClass();
logger.ForContext("Prop1", "RegularValue")
      .ForContext("Prop2", mc, destructureObjects: true)
      .ForContext("Prop3", new { Name1 = "value", Name2 = mc} , destructureObjects: true)
      .ForContext("Prop4", new object[] { "value", mc} , destructureObjects: true)
      .Information("Test");
```
This is perfectly fine, and will be handled by the library, given that there exist a destructuring policy for MyClass.

To explicitly tell the library that a context should be treated as sensitive, then instead of the .ForContext() method use the .WithSensitiveInformation() method, like so:

```csharp
var mc = new MyClass();
logger.WithSensitiveInformation("Prop1", "RegularValue")
      .WithSensitiveInformation("Prop2", mc)
      .WithSensitiveInformation("Prop3", new { Name1 = "value", Name2 = mc})
      .WithSensitiveInformation("Prop4", new object[] { "value", mc})
      .Information("Test");
```
This will treat all the data points given to it as sensitive, so even if there might be some non-sensitive data in the MyClass class, it will still be treated as sensitive.

#### Step 3.2 - LogContext & SensitiveLogContext

Another good way to add properties to you log messages is using the LogContext.PushProperty() method:

```csharp
var mc = new MyClass();
using (LogContext.PushProperty("Prop1", "RegularValue"))
using (LogContext.PushProperty("Prop2", mc, destructureObjects: true))
using (LogContext.PushProperty("Prop3", new { Name1 = "value", Name2 = mc} , true))
using (LogContext.PushProperty("Prop4", new object[] { "value", mc} , destructureObjects: true))
    logger.Information("Test");
```
This is also perfectly fine, and will be handled by the library, given that there exist a destructuring policy for MyClass.

To explicitly tell the library that a static context should be treated as sensitive, instead use the SensitiveLogContext.PushProperty() method:

```csharp
var mc = new MyClass();
using (SensitiveLogContext.PushProperty("Prop1", "RegularValue"))
using (SensitiveLogContext.PushProperty("Prop2", mc,))
using (SensitiveLogContext.PushProperty("Prop3", new { Name1 = "value", Name2 = mc }))
using (SensitiveLogContext.PushProperty("Prop4", new object[] { "value", mc }))
    logger.Information("Test");
```
This will treat all the data points given to it as sensitive, so even if there might be some non-sensitive data in the MyClass class, it will still be treated as sensitive.

## Step 4 - Retaining you log messages

Now to the most important part. All of the things we have been doing so far have only made the sensitive information gather under a common property (called \_\_sensitiveInfo). But to make the team responsible for the log storage be able to know what log entries that can safely be modified, and what log entries needs to be completely removed then they will look for a property called \_\_sensitiveInfoHasBeenReviewed. If a log message does not have this property then it will be removed after a period of time. If  instead the review property is present then after that same period of time the \_\_sensitiveInfo property will be removed, but the rest of the information will be retained.

To add this review property, you need to call the .MarkAsReviewedRegardingSensitiveInformation() method:

```csharp
logger.MarkAsReviewedRegardingSensitiveInformation()
      .Information("Test");
```
If you have reviewed all log statements in a class then you can instead apply this on the constructor level:

```csharp
public class MyProcess
{
    private readonly ILogger _logger;
    
    public MyProcess(ILogger logger)
    {
    	_logger = logger.ForContext<MyProcess>()
                        .MarkAsReviewedRegardingSensitiveInformation();
    }
}
```
Likewise, if you have reviewed all log messages from a third party library, then you can apply it before giving a logger instance to the library, as an example, if you are using the Collector.Common.Restclient package, then you can do like so:

```csharp
var apiClient = new ApiClientBuilder()
    .ConfigureFromAppSettings()
    .WithLogger(Log.Logger.MarkAsReviewedRegardingSensitiveInformation())
    .Build();
```
DISCLAIMER: Do note that doing this to third party libraries might be dangerous since they can change with newer versions. The safest packages to apply this to are packages which themselves depend on this package, and therefor they have taken responsibility themselves of marking things as sensitive.
