# PS-WebApi
-- -- -- -- -- -- -- -- -- -- -- -- Let PowerShell Script serve as WebAPI.

PSWebApi is a simple library for building ASP.NET Web APIs (RESTful Services) by PowerShell Scripts or batch/executable files out of the box.

Similar to the sister repository [DbWebApi](https://github.com/DataBooster/DbWebApi), any managed http client can invoke PowerShell scripts, batch files and executables through the PS-WebApi as the following,

**PowerShell**:
- `http://`localhost:1608/**ps.json**/ps-scripts/test-args.ps1?p1=1&=arg...  
*(returns the result object as JSON)*
- `http://`localhost:1608/**ps.xml**/ps-scripts/test-args.ps1?p1=1&=arg...  
*(returns the result object as XML)*
- `http://`localhost:1608/**ps.csv**/ps-scripts/test-args.ps1?p1=1&=arg...  
*(returns the result object as CSV)*
- `http://`localhost:1608/**ps.html**/ps-scripts/test-args.ps1?p1=1&=arg...  
*(returns the result object as HTML)*
- `http://`localhost:1608/**ps.string**/ps-scripts/test-args.ps1?p1=1&=arg...  
*(returns the result object as plain text)*
- `http://`localhost:1608/**ps.null**/ps-scripts/test-args.ps1?p1=1&=arg...  
*(discards the result and returns empty)*

**Batch**:
- `http://`localhost:1608/**cmd**/bat-scripts/test-args.bat?a1=1&=arg...  
*(captures the StdOut and returns the content as plain text)*

**Executable**:
- `http://`localhost:1608/**cmd**/exe-programs/test-args.exe?a1=1&=arg...  
*(captures the StdOut and returns the content as plain text)*

Often times, in some intranet applications, some functional requirements can be quickly implemented by succinct PowerShell scripts or command line scripts. However, for some integratability reason, this practice was out of sight from most existing projects.

The PS-WebApi is coming out for the seamless integration between script-based modules and other modules. So, Scripting Guys! Do not feel lonely anymore. Make good use of scripting, can reduce the complexity of the whole system development and reduce the developing effort significantly.
