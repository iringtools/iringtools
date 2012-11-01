@echo off
setlocal
set basedir=%~dp0
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

echo Updating build script ...
cd %basedir%
svn update build.xml
pause

echo Deploying DLLs to SDK projects ...
msbuild build.xml /t:DeployToSDK /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
pause