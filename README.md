# PS-WebApi
-- -- -- -- -- -- -- -- -- -- -- -- Let any PowerShell Script or command-line process serve as WebAPI.

PSWebApi is a simple library for turning PowerShell Scripts or batch/executable files into ASP.NET Web APIs (RESTful Services) out of the box.

## Overview

Similar to the sister repository [DbWebApi](https://github.com/DataBooster/DbWebApi), any http client can invoke **managed** PowerShell scripts, batch files and command-line executables through the PS-WebApi as the following,

**PowerShell**:
- `http://base-uri`/**ps.json**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as JSON)*
- `http://base-uri`/**ps.xml**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as XML)*
- `http://base-uri`/**ps.csv**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as CSV)*
- `http://base-uri`/**ps.html**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as HTML)*
- `http://base-uri`/**ps.string**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(returns the result object as plain text)*
- `http://base-uri`/**ps.null**/*ps-scripts/test-args.ps1?p1=1&=arg...*  
*(discards the result and returns empty)*

**Batch**:
- `http://base-uri`/**cmd**/*bat-scripts/test-args.bat?a1=1&=arg...*  
*(captures the StdOut and returns it as plain text)*

**Executable**:
- `http://base-uri`/**cmd**/*exe-programs/test-args.exe?a1=1&=arg...*  
*(captures the StdOut and returns it as plain text)*

Often times, in some intranet applications, some functional requirements can be quickly implemented by succinct PowerShell scripts or command line scripts. However, for some integratability reason, this practice was out of sight from most existing projects.

The PS-WebApi is coming out for the seamless integration between script-based modules and other modules in the network. So, Scripting Guys! Do not feel lonely anymore. Make good use of scripting, can reduce the complexity of the whole system development and reduce the developing effort significantly.

## Server Setup

[PSWebApi.OwinSample](https://github.com/DataBooster/PS-WebApi/tree/master/sample/PSWebApi.OwinSample) is a Quick Start Example, it can be used as a scaffold for customizing your own PS-WebApi service.

The main controller [PSWebApiController](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/Controllers/PSWebApiController.cs) shows how to use the two main extension methods `this.InvokePowerShell(script.LocalFullPath(), allParameters)` and `this.InvokeCmd(script.LocalFullPath(), allArguments, ConfigHelper.CmdTimeoutSeconds)` brought from nugget package https://www.nuget.org/packages/DataBooster.PSWebApi. You can leave this controller as it is if no need to enhance at present.

The only one configuration item that must be customized is the **ScriptRoot** in the [Web.config](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/Web.config). The **ScriptRoot** item is used to indicate a physical location for hosting all the PowerShell Scripts and batch/executable files, those files can be organized under any deep subdirectories. For example,
``` XML
<configuration>
  <appSettings>
    <add key="ScriptRoot" value="D:\user-script-root\" />
  </appSettings>
</configuration>
```
Then, any command-line program _(main entry is .exe, .bat or .ps1)_ been copied into (under) the **ScriptRoot** directory (`D:\user-script-root\`) becomes Web API _(RESTful service)_ out of the box. The request URL should be `http://base-uri/.../relative_path_from_ScriptRoot` _(Note that all back-slashes `\` in physical path need to be replaced by forward-slashes `/` in the URL, and the `...` part depends on your URL routing policy defined in [Startup.cs](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/Startup.cs))_.

You should have an enterprise authorization (access control) service to be integrated with the PS-WebApi service in your organization. The join-point is reserved in [CustomAuthorizeAttribute.cs](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/CustomAuthorizeAttribute.cs):
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
`CheckPrivilege(string script, string user)`:
- `script` parameter will receive above `relative_path_from_ScriptRoot`;
- `user` parameter will be the authenticated user name of current HTTP client.

## HTTP Client
#### Request-URI

If there is a PowerShell script "D:\user-script-root\Dept1\ps\test\demo1.ps1", the HTTP client should call it by URL like `http://base-uri/ps.json`/__Dept1/ps/test/demo1.ps1__?p1=1&=arg....  
If there is a batch file "D:\user-script-root\Dept1\bat\test\demo2.bat", the HTTP client should call it by URL like `http://base-uri/cmd`/__Dept1/bat/test/demo2.bat__?=1&=arg....  
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

    The response body presents the text in the standard output (stdout) stream if the program runs successfully with a ***0*** *ExitCode* and the standard error (stderr) stream is **empty**; Otherwise, the response body presents the text in stderr or exception messages with a *HTTP 500 Internal Error* header.

#### Parameters and Arguments
Parameters and arguments can be carried through either URI query-string or message body, or both.  
JSON is the only one recognizable media-type in the request body, please always include the "**Content-Type: application/json**" or "Content-Type: text/json" in your request header if the request contains a body.  
If both request body and query-string are supplied, the query-string parts (parameters and arguments) will be taken into the command line first, then append the body parts (parameters and arguments).

##### PowerShell
1. From URI query-string

    All the name/value pairs in query-string are passed to PowerShell. Empty name and duplicate names are acceptable in query-string. Empty-named parameters will be converted to arguments. For the following Uri example:

    ```
    test-args.ps1?np1=0.618&np1=2.71828&=arg1...&=arg2...
    ```
    **==>**
    ```
    test-args.ps1 `
    -np1 "0.618" `
    -np1 "2.71828" `
    "arg1..." `
    "arg2..."
    ```
    *Get through Uri query-string, all the values are always string type.*

2. From Body  
For the moment, PowerShell parameters/arguments can only accept a JSON object *(which can be deserialized into a Dictionary<string,object>)* from the request body. Any invalid JSON will be ignored without warning.
    - To add an argument (unnamed-parameter), please use an empty string or any-length white-space string as the key-name of a key/value pair.  
To have multiple arguments, please use different number of white-space as the key-name of key/value pairs.
    - To add multiple named-parameters with the same parameter name, please add different number of white-space before or after the actual parameter name as the key-name of key/value pairs, since a dictionary object cannot carry duplicate keys. When the server side receives the dictionary object, it will **trim** all leading and trailing white-space from the key-name before it pass to PowerShell. But the value parts will always remain as it is (**no trim**).
    - If a value is null and its key is null, empty or white-space; that key/value pair will be discarded together.

    For the following JSON example:
    ``` JSON
    {
        "named_param1": 3.14,
        "": "This is an argument (unnamed-parameter)",
        "np2": "Value 1 of 3",
        "np2 ": "Value 2 of 3",
        " np2 ": "Value 3 of 3",
        " ": " This is another argument "
    }
    ```
    **==>**
    ```
    test-args.ps1 `
    -named_param1 3.14 `
    "This is an argument (unnamed-parameter)" `
    -np2 "Value 1 of 3" `
    -np2 "Value 2 of 3" `
    -np2 "Value 3 of 3" `
    " This is another argument "
    ```

##### Batch/Executable
Essentially, there is no concept of named-parameter in batch/executable world. Command line can only supply string-typed arguments. There are some different treatments on arguments from PowerShell.

1. From URI query-string  
Each name/value pair will be split into two arguments. For examples:
    - `test-args.bat?/n1=value1`  is equivalent to `test-args.bat?=/n1&=value1`  
(*both result to the same command line:* `test-args.bat /n1 value1`)
    - `test-args.bat?-n2=value2`  is equivalent to `test-args.bat?=-n2&=value2`  
(*both result to the same command line:* `test-args.bat -n2 value2`)

2. From Body  
Three kinds of JSON data can be accepted as command line arguments. Each request can use one of a kind in the message body:
    - JSON Object *(`Dictionary<string,object>`)*  
Similar to URI query-string, JSON Object is an unordered set of name/value pairs. Each name/value pair basically will be split into two arguments. All leading and trailing white-space will be trimmed from the name. If the name becomes empty, it won't be added into the command line as an argument. But the value part won't be trimmed. Any type other than string will be converted into a string by **ToString()** method *(null value will become an empty string "")*. A null value will be discarded only when its name is null, empty or white-space.  
For Example,
        ``` JSON
        {
            "/n1": 3.14,
            "-n2": "Value 1 of 2",
            "-n2 ": "Value2of2",
            " ": null,
            "": " This is the last argument "
        }
        ```
        **==>**
        ```
        test-args.bat /n1 3.14 -n2 "Value 1 of 3" -n2 Value2of3 " This is the last argument "
        ```

    - JSON Array  
Using a JSON Array is most straightforward way, each item in the array will be passed to command line as arguments (no trim); null value will become an empty string "" (enclosed by double quotes).

        For example,
        ``` JSON
        [
            "/n1",
            3.14,
            "-n2",
            "Value 1 of 3",
            "-n2",
            "Value2of3",
            " This is the last argument "
        ]
        ```
        ==>
        ```
        test-args.bat /n1 3.14 -n2 "Value 1 of 3" -n2 Value2of3 " This is the last argument "
        ```

    - Single JSON Value  
When there is only one item (single value) in the array, the single JSON value can be put in the body directly, rather than wrapped in an array *(surrounded by square brackets)*.

        For example,
        ``` JSON
        "single value argument"
        ```
        *is equivalent to*
        ``` JSON
        [
            "single value argument"
        ]
        ```
        both ==>
        ```
        test-args.bat "single value argument"
        ```

- ***Escaping and Quoting***  
Since there is no unified standard to escape and quote a Windows command line argument, it needs to be fully controlled by your own customized PSWebApi service *(please see a reference implementation [CmdArgumentResolver.cs](https://github.com/DataBooster/PS-WebApi/blob/master/sample/PSWebApi.OwinSample/CmdArgumentResolver.cs) in the sample project)*. The PSWebApi library only offers two common methods for escaping and quoting:
    - CmdArgumentsBuilder.**QuoteExeArgument**  
For most Microsoft C/C++/C# console applications (follow the rules described in https://msdn.microsoft.com/en-us/library/17w5ykft.aspx or https://msdn.microsoft.com/en-us/library/a1y7w461.aspx):
        1. A string will remain unchanged if it can be interpreted (by [CommandLineToArgvW](https://msdn.microsoft.com/en-us/library/windows/desktop/bb776391.aspx)) as a single argument;  
For examples,  
    `3.14`  
    `A_string_without_white_space`  
    `"A string already surrounded by double quotation marks"`  
    `/name_without_white_space:"value with spaces"`  
        2. Otherwise, if a string would be interpreted as multiple broken arguments or a string contains any unclosed double-quotation mark *(non-literal)*, then the original string will be surrounded by an extra pair of double-quotation marks at the outermost layer, and all the originally nested quotes will be escaped using the `\"` as **literal** double-quotation marks.  
For examples,  
    `She's 5'5" tall.`  
    will be encoded/escaped as  
    `"She's 5'5\" tall."`

        A test batch [test-args.exe](https://github.com/DataBooster/PS-WebApi/tree/master/sample/Test-Args) *(build release mode to `..\PSWebApi.OwinSample\user-script-root\exe-apps\`)* can be used to give it a try.

    - CmdArgumentsBuilder.**QuoteBatArgument**  
For batch files:
        1. A string will remain unchanged if it can be interpreted (by Windows command-line parser *CMD.EXE*) as a single argument;
        2. Otherwise, if a string would be interpreted as multiple broken arguments or a string contains any unclosed double-quotation mark (non-literal), then the original string will be surrounded by an extra pair of double-quotation marks at the outermost layer, and each originally nested double-quotation mark will be escaped as double double-quotation marks.  
For examples,  
    `She's 5'5" tall.`  
    will be encoded/escaped as  
    `"She's 5'5"" tall."`

## Conventions
#### HTTP status of PowerShell response
- If any errors or exceptions occur, whether **Terminating Error** or **Non-Terminating Error** *(such as: write-error)*; A HTTP status 500 *(InternalServerError)* will be sent to the client.
- If the request script has been successfully processed, but the returned result-object is null or empty; A HTTP status 204 *(NoContent)* will be sent to the client.
- If the request script has been successfully processed and there is something in the returned result-object; A HTTP status 200 *(**OK**)* will be sent to the client.

#### HTTP status of Batch/Executable response
- If any exceptions occur or the standard error *(**stderr**)* stream contains any message; A HTTP status 500 *(InternalServerError)* will be sent to the client.
- If the request process has exited successfully *(by an ExitCode value of 0)* and the **stderr** is empty, but the standard output (**stdout**) stream is empty; A HTTP status 204 *(NoContent)* will be sent to the client.
- If the request process has exited successfully *(by an ExitCode value of 0)* and the **stderr** is empty, but the **stdout** stream has some content; A HTTP status 200 *(**OK**)* will be sent to the client.
- A custom field "Exit-Code" is added into the response header to carry the exit code value of the associated process.

## NuGet
#### Server library package
- [PowerShell-WebApi](https://www.nuget.org/packages/DataBooster.PSWebApi)

#### Client  
Most of [DbWebApi Client Packages](https://github.com/DataBooster/DbWebApi#clients-1) can be reused in PSWebApi client.  
For example, a .Net Client:
``` CSharp
using DataBooster.DbWebApi.Client;
```
``` CSharp
DbWebApiClient client = new DbWebApiClient("http://localhost:1608/ps.json/");
//  client.HttpMethod = HttpMethod.Get;    // Default is POST

// Synchronous call. If need asynchronous call, please use ExecAsJsonAsync(..) instead.
JObject data = client.ExecAsJson("ps-scripts/test-args.ps1",
    new {
        inDate = new DateTime(2015, 3, 16)
        //, ... other input parameters, if any.
    });
```
In the same way,  
`ExecAsXml(...)` or *`ExecAsXmlAsync(...)`* can be used for XML response;  
`ExecAsString(...)` or *`ExecAsStringAsync(...)`* can be used for any kind of plain text response;  
`ExecAsStream(...)` or *`ExecAsStreamAsync(...)`* can be used for saving response body as a file.


***

Welcome all feedback through the [Issues](https://github.com/DataBooster/PS-WebApi/issues) or [Discussions](https://pswebapi.codeplex.com/discussions).
