@echo off
call "%VS100COMNTOOLS%\vsvars32.bat"
cd %~dp0%
taskkill /IM WebDev.WebServer40.exe
start /B WebDev.WebServer40 /port:54321 /path:%~dp0%iRINGTools.Services
start /B WebDev.WebServer40 /port:12345 /path:%~dp0%iRINGTools.Applications