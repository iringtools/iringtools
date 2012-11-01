<%@ Page Language="C#" %>

<% 
    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(typeof(org.iringtools.library.VersionInfo));
    string major = assembly.GetName().Version.Major.ToString();
    string minor = assembly.GetName().Version.Minor.ToString();
    string build = assembly.GetName().Version.Build.ToString();
    string revision = assembly.GetName().Version.Revision.ToString();
%>

<html xmlns="http://www.w3.org/1999/xhtml">

<head>
<meta content="text/html; charset=utf-8" http-equiv="Content-Type" />
<title>Untitled 1</title>
</head>

<body>
  <div>
	<div>
            <a href="http://iringug.org/wiki/index.php?title=IRINGTools" target="_blank">
            <img src="Content/img/iringtools-logo.png"/>
            </a><div style="font-family:Arial;font-size:10pt;padding-left:165px;position:relative; top:-17px;">Version <%=major%>.<%=minor%>.<%=build%>.<%=revision%></div>
            </div>
	</div>

    <div>
      <p style="font-family:Arial;font-size:11pt; text-align:justify; ">
      The Adapter Manager is a tool for managing the configuration of the iRINGTools Adapter.
      <br/>The tool provides the ability to configure scopes and applications. It allows configuration of datalayers, and mapping of dataObjects to the federated model.</p>
   </div>
    <br/><br/><br/><br/>
	<div style="height: 32px;text-align:justify;">
            <p style="font-family:Arial;font-size:8pt;">
            Copyright © 2009 - 2012, iringug.org All rights reserved.
            <a href="http://iringug.org" target="_blank"><img src="Content/img/iringug-logo.png" style="float: right" />
            </a></p>
	</div>

	<div>
            <p style="font-family:Arial;font-size:8pt;text-align:justify;">Redistribution and use in
            source and binary forms, with or without modification, are permitted
            provided that the following conditions are met: </p>
	</div>
	<div>
            <p style="font-family:Arial;font-size:8pt;text-align:justify;">Redistributions of source code
            must retain the above copyright notice, this list of conditions and the
            following disclaimer. </p>
	</div>
	<div>
            <p style="font-family:Arial;font-size:8pt;text-align:justify;">Redistributions in binary form
            must reproduce the above copyright notice, this list of conditions and
            the following disclaimer in the documentation and/or other materials provided with the distribution.
            </p>
	</div>
	<div>
            <p style="font-family:Arial;font-size:8pt;text-align:justify;">Neither the name of
            iringug.org nor the names of its contributors may be used to endorse or
            promote products derived from this software without specific
            prior written permission. </p>
	</div>	
	<div>
            <p style="font-family:Arial;font-size:8pt;text-align:justify;">THIS SOFTWARE IS PROVIDED BY
            THE COPYRIGHT HOLDERS AND CONTRIBUTORS &quot;AS IS&quot; AND ANY EXPRESS OR
            IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
            AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
            THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
            INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
            NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
            USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
            ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
            (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
            THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
            </p>
	</div>
</body>

</html>
