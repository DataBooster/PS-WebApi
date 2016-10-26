# PS-WebApi
-- -- -- -- -- -- -- -- -- -- -- -- Let PowerShell Script serve as WebAPI.

PSWebApi is a simple library for building ASP.NET Web APIs (RESTful Services) by PowerShell Scripts or batch/executable files out of the box.

## Overview

Similar to the sister repository [DbWebApi](https://github.com/DataBooster/DbWebApi), any managed http client can invoke PowerShell scripts, batch files and executables through the PS-WebApi as the following,

**PowerShell**:
- `http://`localhost:1608/**ps.json**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as JSON)*
- `http://`localhost:1608/**ps.xml**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as XML)*
- `http://`localhost:1608/**ps.csv**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as CSV)*
- `http://`localhost:1608/**ps.html**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as HTML)*
- `http://`localhost:1608/**ps.string**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as plain text)*
- `http://`localhost:1608/**ps.null**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(discards the result and returns empty)*

**Batch**:
- `http://`localhost:1608/**cmd**/*bat-scripts/test-args.bat?a1=1&=arg...*  
*(captures the StdOut and returns the content as plain text)*

**Executable**:
- `http://`localhost:1608/**cmd**/*exe-programs/test-args.exe?a1=1&=arg...*  
*(captures the StdOut and returns the content as plain text)*

Often times, in some intranet applications, some functional requirements can be quickly implemented by succinct PowerShell scripts or command line scripts. However, for some integratability reason, this practice was out of sight from most existing projects.

The PS-WebApi is coming out for the seamless integration between script-based modules and other modules in the network. So, Scripting Guys! Do not feel lonely anymore. Make good use of scripting, can reduce the complexity of the whole system development and reduce the developing effort significantly.

## Service Setup

[PSWebApi.OwinSample](https://github.com/DataBooster/PS-WebApi/tree/master/sample/PSWebApi.OwinSample) is a Quick Start Example, it can be used as a scaffold for customizing your own PS-WebApi service.

The main controller [PSWebApiController](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/Controllers/PSWebApiController.cs) shows how to use the two main extension methods `this.InvokePowerShell(script.LocalFullPath(), allParameters)` and `this.InvokeCmd(script.LocalFullPath(), allArguments, ConfigHelper.CmdTimeoutSeconds)` brought from nugget package https://www.nuget.org/packages/DataBooster.PSWebApi. You can leave this controller as it is if no need to enhance at present.

An important thing needs to be done by yourself is authorization, please don't forget to plug in your own enterprise's permission check implementation in [CustomAuthorizeAttribute.cs](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/CustomAuthorizeAttribute.cs):
``` CSharp
    public partial class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        ...

        private bool CheckPrivilege(string script, string user)
        {
            // TO DO, please implement your own authorization logic here.

            return true;	// If allow permission
            return false;	// If deny permission
        }
    }
```
The only one configuration item that must be customized is the **ScriptRoot** in the [Web.config](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/Web.config). The **ScriptRoot** item is used to indicate a physical location for hosting all the PowerShell Scripts and batch/executable files, those files can be organized under any deep subdirectories. For example,
``` XML
<configuration>
  <appSettings>
    <add key="ScriptRoot" value="D:\scripts-root\" />
  </appSettings>
</configuration>
```

## HTTP Client
#### Request-URI

If there is a PowerShell script "D:\scripts-root\Dept1\ps\test\demo1.ps1", the HTTP client should call it by URL like `http://localhost:1608/ps.json`/__Dept1/ps/test/demo1.ps1__?p1=1&=arg....  
If there is a batch file "D:\scripts-root\Dept1\bat\test\demo2.bat", the HTTP client should call it by URL like `http://localhost:1608/cmd`/__Dept1/bat/test/demo2.bat__?=1&=arg....  
Calling executable file follows the same pattern as batch file.

#### Response MediaType
##### PowerShell
PowerShell has built-in support for JSON, XML, CSV, HTML and plain text media types. The desired content-type (media type) of a response can be required by Request-URI or the *Accept* request-header field. Request-URI has a higher priority.

1. The Request-URI way depends on how you register your URL Routing. As an example, the OwinSample [Startup.cs](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/Startup.cs) leads to following patterns:

    - `http://BaseUrl`/**ps.json**/*script-path.ps1*   ==> Media-type: **application/json**
    - `http://BaseUrl`/**ps.xml**/*script-path.ps1*    ==> Media-type: **application/xml**
    - `http://BaseUrl`/**ps.csv**/*script-path.ps1*    ==> Media-type: **text/csv**
    - `http://BaseUrl`/**ps.html**/*script-path.ps1*   ==> Media-type: **text/html**
    - `http://BaseUrl`/**ps.string**/*script-path.ps1* ==> Media-type: **text/plain**
    - `http://BaseUrl`/**ps.null**/*script-path.ps1*   ==> HTTP 204 - NoContent

2. The ***Accept*** request-header can also be used to specify certain media types which are desired for the response. Use the URL `http://BaseUrl`/**ps**/script-path.ps1 with following request-header:

    - Accept: application/json  
or  
Accept: text/json
    - Accept: application/xml  
or  
Accept: text/xml
    - Accept: text/csv  
or  
Accept: application/csv
    - Accept: text/html  
or  
Accept: application/xhtml
    - Accept: text/plain
    - Accept: application/null

##### Batch/Executable
- `http://BaseUrl`/**cmd**/*batch-or-executable-path.bat*

    Since neither Batch-Program nor Console-Application would return an object, the response content-type is always text/plain.

    The response body presents the text in the standard output (stdout) stream if the program runs successfully with a ***0*** *ExitCode* and the standard error (stderr) stream is **empty**; Otherwise, the response body presents the text in stderr or exception messages with a HTTP 500 Internal Error header.

#### Parameters and Arguments
Parameters and arguments can be carried through either request message body or URI query-string, or both.  
JSON is the only one recognizable media-type in the request body, please always include the "**Content-Type: application/json**" or "Content-Type: text/json" in your request header if the request contains a body.  
If both request body and query-string are supplied, the body parts (parameters and arguments) will be taken into the command line first, then append the query-string parts (parameters and arguments).

##### PowerShell

##### Batch/Executable
