@echo off
setlocal
rem set basedir=%~dp0
rem set deploymentdir=%basedir%Deployment\
set "PATH=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319;%PATH%"

rem echo Updating build script ...
rem cd %basedir%
rem svn update build.xml
rem pause

echo Building VB projects ...
msbuild bamboo.xml /t:All
pause

