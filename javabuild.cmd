@echo off
setlocal
set basedir=%~dp0
set javabasedir=%basedir%iRINGTools.ESBServices\
set deploymentdir=%basedir%Deployment\

echo Updating build script ...
cd %javabasedir%
svn update build.xml
pause

echo Building Java projects ...
cd %javabasedir%
call ant
if %ERRORLEVEL% equ 0 copy /y %javabasedir%dist\*.* %deploymentdir%
pause