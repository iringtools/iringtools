@echo off
call "%VS100COMNTOOLS%\vsvars32.bat"

svcutil /dconly ^
  /namespace:http://www.iringtools.org/dxfr/manifest,org.iringtools.dxfr.manifest ^
  /namespace:http://www.iringtools.org/mapping,org.iringtools.mapping ^
	/out:Manifest.cs ./dxfr/manifest.xsd ./mapping/mapping.xsd