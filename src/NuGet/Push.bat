@ECHO OFF
CD /D %~dp0

IF /i {%1}=={} GOTO :Usage
IF /i {%1}=={-h} GOTO :Usage
IF /i {%1}=={-help} GOTO :Usage

SET PKG=nupkg\DataBooster.PSWebApi.%1.nupkg
SET PKGPREPARED=true

FOR %%P IN (%PKG%) DO (
IF NOT EXIST %%P (
ECHO %%P does not exist!
SET PKGPREPARED=false
)
)

IF {%PKGPREPARED%}=={false} (
COLOR 0C
PAUSE
COLOR
GOTO :EOF
)

FOR %%P IN (%PKG%) DO ..\..\.nuget\NuGet.exe Push %%P

GOTO :EOF

:Usage
ECHO.
ECHO Usage:
ECHO     Push.bat version
ECHO.
ECHO Example:
ECHO     Push.bat 1.0.0.1
