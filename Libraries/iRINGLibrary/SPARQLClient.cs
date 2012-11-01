using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Web;
using log4net;
using org.w3.sparql_results;
using org.iringtools.utility;

namespace org.iringtools.library
{
  public class SPARQLClient
  {
		private static readonly ILog _logger = LogManager.GetLogger(typeof(SPARQLClient));
    public static SPARQLResults PostQuery(string baseUri, string sparql, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials)
    {
      try
      {
        SPARQLResults sparqlResults = null;

        string message = "query=" + HttpUtility.UrlEncode(sparql);

        WebHttpClient webClient = new WebHttpClient(baseUri, targetCredentials.GetNetworkCredential(), proxyCredentials.GetWebProxy());
        sparqlResults = webClient.PostMessage<SPARQLResults>("", message, false);

        return sparqlResults;
      }
      catch (Exception exception)
      {
				_logger.Error("Error in PostQuery: " + exception);
        throw exception;
      }
    }

    public static void PostQueryAsMultipartMessage(string baseUri, string sparql, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials)
    {
      try
      {
        string result = string.Empty;
        MultiPartMessage requestMessage = new MultiPartMessage
        {
          name = "update",
          type = MultipartMessageType.FormData,
          message = sparql,
        };

        List<MultiPartMessage> requestMessages = new List<MultiPartMessage>
        {
          requestMessage
        };

        WebHttpClient webClient = new WebHttpClient(baseUri, targetCredentials.GetNetworkCredential(), proxyCredentials.GetWebProxy());

        webClient.PostMultipartMessage("", requestMessages);
      }
      catch (Exception exception)
      {
				_logger.Error("Error in PostQueryAsMultipartMessage: " + exception);
				throw exception;
      }
    }

    public static SPARQLResults Query(string baseUri, string sparql, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials)
    {
      try
      {
        SPARQLResults sparqlResults = null;

        string relativeUri = "?query=" + HttpUtility.UrlEncode(sparql);

        WebHttpClient webClient = new WebHttpClient(baseUri, targetCredentials.GetNetworkCredential(), proxyCredentials.GetWebProxy());
        sparqlResults = webClient.Get<SPARQLResults>(relativeUri, false);

        return sparqlResults;
      }
      catch (Exception exception)
      {
				_logger.Error("Error in Query: " + exception);
				throw exception;
      }
    }
    public static string Update(string baseUri, string sparql, WebCredentials targetCredentials, WebProxyCredentials proxyCredentials)
    {
      try
      {
        string message = String.Empty;
        string relativeUri = "?update=" + HttpUtility.UrlEncode(sparql);
        WebHttpClient webClient = new WebHttpClient(baseUri, targetCredentials.GetNetworkCredential(), proxyCredentials.GetWebProxy());
        message = webClient.GetMessage(relativeUri);
        return message;
      }
      catch (Exception exception)
      {
				_logger.Error("Error in Update: " + exception);
				throw exception;
      }
    }
  }
}
