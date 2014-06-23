NetBIX
======

A .NET oBIX Client, written in C# as a portable library.

NetBIX supports connecting to, reading and writing information from oBIX XML over HTTP servers (See https://www.oasis-open.org/committees/download.php/38212/oBIX-1-1-spec-wd06.pdf).  NetBIX contains extension code for most oBIX-compatible .NET data types for easy conversion to and from oBIX types.  NetBIX comes in a PCL for easy deployment to mobile devices.

NetBIX is heavily integrated into, and depends on, LINQ-to-XML (XElement) types.

## Features

* .NET Portable class-library
* Async aware APIs
* oBIX:Read, oBIX:Write, and oBIX:Invoke support
* oBIX:Batch support

## Namespaces

* **NetBIX.oBIX.Client** - Root Namespace, contains the classes required to open and manipulate an oBIX connection
** **NetBIX.oBIX.Client.Framework** - Contains classses responsible for the running of the oBIX engine
** **NetBIX.oBIX.Client.Extensions** - Contains the oBIX data-type conversion extensions

## Getting started

All public oBIX APIs that rely on I/O to an oBIX server will return an `ObixResult` in `Client.Framework` namespace if the method has no return, or an `ObixResult<TResult>` with `TResult` being the type of the returned object accessible through the `.Result` property of the returned `ObixResult<>`. 

**Connecting to an oBIX Server**

```cs
ObixResult connectResult;
XmlObixClient obixClient = new XmlObixClient(new Uri("http://obix.server/obix"));

connectResult = obixClient.Connect();
if (connectResult == ObixResult.kObixClientSuccess) {
  Console.WriteLine("Connection to the oBIX server succeeded!");
} else {
  Console.WriteLine("Connection to the oBIX server failed: {0}", ObixResult.Message(connectResult));
}

```


## Limitations

At the time of writing, only oBIX:XML over HTTP is supported, although more bindings are possible.
