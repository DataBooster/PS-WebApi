@ECHO OFF
CD /D %~dp0

IF NOT EXIST nupkg MKDIR nupkg
..\..\.nuget\NuGet.exe pack ..\DataBooster.PSWebApi\DataBooster.PSWebApi.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg
