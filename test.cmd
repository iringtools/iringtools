@echo off
setlocal
set basedir=%~dp0
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

echo Running C# tests ...
cd %basedir%

msbuild build.xml /t:Test
pause