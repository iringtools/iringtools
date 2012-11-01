@echo off
setlocal
set basedir=%~dp0
set javabasedir=%basedir%iRINGTools.ESBServices\
set deploymentdir=%basedir%Deployment\
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

echo Updating build script ...
cd %basedir%
svn update build.xml
pause

echo Building C# projects ...
msbuild build.xml /t:All /fileLogger /flp:errorsonly;logfile=msbuild.error.log /fileLogger /flp1:warningsonly;logfile=msbuild.warning.log
pause