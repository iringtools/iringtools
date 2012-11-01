using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Configuration;
using SysConfig = System.Configuration;
using System.Collections;
using log4net;


public static class ConfigFile
{
	private static readonly ILog _logger = LogManager.GetLogger(typeof(ConfigFile));
    
    public const string AppSettings = "appSettings";


    public static string GetConfigString(this string filePath)
    {
        IDictionary<string, string> configList = filePath.GetSection(ConfigFile.AppSettings);

        return configList.GetConfigString();
    }

    public static string GetConfigString(this IDictionary<string, string> configCol)
    {
        StringBuilder retVal = new StringBuilder();
        foreach (KeyValuePair<string, string> record in configCol)
        {
            if (retVal.Length > 0)
                retVal.Append("~");
            retVal.AppendFormat("{0}#{1}", record.Key, record.Value);
        }
        return retVal.ToString();
    }

    public static IDictionary<string, string> GetSection(this string fullFilePath, string section)
    {
        return GetSection(fullFilePath, section, new Dictionary<string, string>());
    }

    public static IDictionary<string, string> GetSection(this string fullFilePath, string section, IDictionary<string, string> appendList)
    {
        IDictionary<string, string> retList = new Dictionary<string, string>();
        string filePath = fullFilePath;
        System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
        NameValueCollection dataCol = new NameValueCollection();
        try
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = filePath;
            SysConfig.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            string xmlString = config.GetSection(section).SectionInformation.GetRawXml();
            xmlDoc.LoadXml(xmlString);

            System.Xml.XmlNode nodeList = xmlDoc.ChildNodes[0];
            foreach (System.Xml.XmlNode node in nodeList)
                retList.Add(node.Attributes[0].Value, node.Attributes[1].Value);
        }
        catch (Exception ex) {
					_logger.Error("Error in GetSection: " + ex);
				}

        // If an append list is provided then add it to the list we are returning
        if (appendList != null && appendList.Count > 0)
        {
            foreach (KeyValuePair<string, string> record in appendList)
                retList.Add(record.Key, record.Value);
        }
        return retList;
    }

    public static string GetSection(this string fullFilePath, string section, string key)
    {
        string filePath = fullFilePath;
        System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
        NameValueCollection dataCol = new NameValueCollection();
        try
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = filePath;
            SysConfig.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            string xmlString = config.GetSection(section).SectionInformation.GetRawXml();
            xmlDoc.LoadXml(xmlString);

            System.Xml.XmlNode nodeList = xmlDoc.ChildNodes[0];
            foreach (System.Xml.XmlNode node in nodeList)
                if (node.Attributes[0].Value.ToLower() == key.ToLower())
                    return node.Attributes[1].Value;
        }
        catch (Exception ex){
					_logger.Error("Error in GetSection: " + ex);
				}

        return "";
    }
}

