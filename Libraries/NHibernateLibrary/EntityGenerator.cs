// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;
using System.Xml;
using Microsoft.CSharp;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;

namespace org.iringtools.nhibernate
{
  public class EntityGenerator
  {
    private List<string> NHIBERNATE_ASSEMBLIES = new List<string>() 
    {
      "NHibernate.dll",     
      "NHibernate.ByteCode.Castle.dll",
      "Iesi.Collections.dll",
    };

    private static readonly ILog _logger = LogManager.GetLogger(typeof(EntityGenerator));

    private NHibernateSettings _settings = null;
    private StringBuilder _mappingBuilder = null;
    private XmlTextWriter _mappingWriter = null;
    private List<DataObject> _dataObjects = null;
    private IndentedTextWriter _dataObjectWriter = null;
    private StringBuilder _dataObjectBuilder = null;

    public EntityGenerator(NHibernateSettings settings)
    {
      _settings = settings;
    }

    public Response Generate(string compilerVersion, DatabaseDictionary dbSchema, string projectName, string applicationName)
    {
      Response response = new Response();
      Status status = new Status();

      if (dbSchema.dataObjects != null)
      {
        _dataObjects = dbSchema.dataObjects;

        try
        {
          status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

          Directory.CreateDirectory(_settings["AppDataPath"]);

          _mappingBuilder = new StringBuilder();
          _mappingWriter = new XmlTextWriter(new StringWriter(_mappingBuilder));
          _mappingWriter.Formatting = Formatting.Indented;

          _mappingWriter.WriteStartElement("hibernate-mapping", "urn:nhibernate-mapping-2.2");
          _mappingWriter.WriteAttributeString("default-lazy", "true");

          _dataObjectBuilder = new StringBuilder();
          _dataObjectWriter = new IndentedTextWriter(new StringWriter(_dataObjectBuilder), "  ");

          _dataObjectWriter.WriteLine(Utility.GeneratedCodeProlog);
          _dataObjectWriter.WriteLine("using System;");
          _dataObjectWriter.WriteLine("using System.Globalization;");
          _dataObjectWriter.WriteLine("using System.Collections.Generic;");
          _dataObjectWriter.WriteLine("using Iesi.Collections.Generic;");
          _dataObjectWriter.WriteLine("using org.iringtools.library;");
          
          foreach (DataObject dataObject in dbSchema.dataObjects)
          {
            _dataObjectWriter.WriteLine(); 
            _dataObjectWriter.WriteLine("namespace {0}", dataObject.objectNamespace);
            _dataObjectWriter.Write("{"); // begin namespace block
            _dataObjectWriter.Indent++;

            dataObject.objectNamespace = dataObject.objectNamespace;

            CreateNHibernateDataObjectMap(dataObject);

            _dataObjectWriter.Indent--;
            _dataObjectWriter.WriteLine("}"); // end namespace block    
          }            

          _mappingWriter.WriteEndElement(); // end hibernate-mapping element
          _mappingWriter.Close();

          string mappingXml = _mappingBuilder.ToString();
          string sourceCode = _dataObjectBuilder.ToString();

          #region Compile entities
          Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
          compilerOptions.Add("CompilerVersion", compilerVersion);

          CompilerParameters parameters = new CompilerParameters();
          parameters.GenerateExecutable = false;
          parameters.ReferencedAssemblies.Add("System.dll");
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "Iesi.Collections.dll");
          parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + "iRINGLibrary.dll");
          NHIBERNATE_ASSEMBLIES.ForEach(assembly => parameters.ReferencedAssemblies.Add(_settings["BinaryPath"] + assembly));
          
          Utility.Compile(compilerOptions, parameters, new string[] { sourceCode });
          #endregion Compile entities

          #region Writing memory data to disk
          string hibernateConfig = CreateConfiguration(
            (Provider)Enum.Parse(typeof(Provider), dbSchema.Provider, true), 
            dbSchema.ConnectionString, dbSchema.SchemaName);

          Utility.WriteString(hibernateConfig, _settings["AppDataPath"] + "nh-configuration." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(mappingXml, _settings["AppDataPath"] + "nh-mapping." + projectName + "." + applicationName + ".xml", Encoding.UTF8);
          Utility.WriteString(sourceCode, _settings["AppCodePath"] + "Model." + projectName + "." + applicationName + ".cs", Encoding.ASCII);
          
          DataDictionary dataDictionary = CreateDataDictionary(dbSchema.dataObjects);
          dataDictionary.dataVersion = dbSchema.dataVersion;
          dataDictionary.enableSearch = dbSchema.enableSearch;
          dataDictionary.enableSummary = dbSchema.enableSummary;          

          Utility.Write<DataDictionary>(dataDictionary, _settings["AppDataPath"] + "DataDictionary." + projectName + "." + applicationName + ".xml");
          #endregion

          status.Messages.Add("Entities of [" + projectName + "." + applicationName + "] generated successfully.");
        }
        catch (Exception ex)
        {
          throw new Exception("Error generating application entities " + ex);

          //no need to status, thrown exception will be statused above.
        }
      }

      response.Append(status);
      return response;
    }

    // Remove table names and column names from database dictionary
    private DataDictionary CreateDataDictionary(List<DataObject> dataObjects)
    {
      /*
      foreach (DataObject dataObject in dataObjects)
      {        
        dataObject.tableName = null;

        foreach (DataProperty dataProperty in dataObject.dataProperties)
          dataProperty.columnName = null;
      }
      */

      return new DataDictionary { dataObjects = dataObjects };
    }

    private void CreateNHibernateDataObjectMap(DataObject dataObject)
    {
      string keyClassName = dataObject.objectName + "Id";

      _mappingWriter.WriteStartElement("class");
      _mappingWriter.WriteAttributeString("name", dataObject.objectNamespace + "." + dataObject.objectName + ", " + _settings["ExecutingAssemblyName"]);
      _mappingWriter.WriteAttributeString("table", "\"" + dataObject.tableName + "\"");

      #region Create composite key
      if (dataObject.keyProperties.Count > 1)
      {
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("[Serializable]");
        _dataObjectWriter.WriteLine("public class {0}", keyClassName);
        _dataObjectWriter.WriteLine("{"); // begin composite key class
        _dataObjectWriter.Indent++;

        _mappingWriter.WriteStartElement("composite-id");
        _mappingWriter.WriteAttributeString("name", "Id");
        _mappingWriter.WriteAttributeString("class", dataObject.objectNamespace + "." + keyClassName + ", " + _settings["ExecutingAssemblyName"]);

        foreach (KeyProperty keyName in dataObject.keyProperties)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(keyName.keyPropertyName);

          if (keyProperty != null)
          {
            _dataObjectWriter.WriteLine("public {0} {1} {{ get; set; }}", keyProperty.dataType, keyProperty.propertyName);

            _mappingWriter.WriteStartElement("key-property");
            _mappingWriter.WriteAttributeString("name", keyProperty.propertyName);
            _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
            _mappingWriter.WriteEndElement(); // end key-property
          }
        }

        _dataObjectWriter.WriteLine("public override bool Equals(object obj)"); // start Equals method
        _dataObjectWriter.WriteLine("{");

        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("bool equals = false;");
        _dataObjectWriter.WriteLine("if (obj != null)");
        _dataObjectWriter.WriteLine("{");

        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties[i].keyPropertyName);

          string keyName = String.IsNullOrEmpty(keyProperty.propertyName) ? keyProperty.columnName : keyProperty.propertyName;

          if (i == 0)
          {
            _dataObjectWriter.Indent++;
            _dataObjectWriter.Write("equals = (");
          }
          else
          {
            _dataObjectWriter.Write(" && ");
          }

          _dataObjectWriter.Write("this.{0} == (({1})obj).{0}", keyName, keyClassName);
        }

        _dataObjectWriter.WriteLine(");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.WriteLine("return equals;");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end Equals method

        _dataObjectWriter.WriteLine("public override int GetHashCode()"); // start GetHashCode method
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("int _hashCode = 0;");

        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties[i].keyPropertyName);

          string keyName = String.IsNullOrEmpty(keyProperty.propertyName) ? keyProperty.columnName : keyProperty.propertyName;

          _dataObjectWriter.WriteLine("_hashCode += {0}.GetHashCode();", keyName);
        }

        _dataObjectWriter.WriteLine("return _hashCode;");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end GetHashCode method

        _dataObjectWriter.WriteLine("public override string ToString()"); // start ToString method
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("string _idString = String.Empty;");

        for (int i = 0; i < dataObject.keyProperties.Count; i++)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties[i].keyPropertyName);
          string keyName = String.IsNullOrEmpty(keyProperty.propertyName) ? keyProperty.columnName : keyProperty.propertyName;

          _dataObjectWriter.WriteLine("_idString += {0}.ToString();", keyName);
        }

        _dataObjectWriter.WriteLine("return _idString;");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end ToString method

        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end composite key class

        _mappingWriter.WriteEndElement(); // end composite-id class element
      }
      #endregion Create composite key

      _dataObjectWriter.WriteLine();
      _dataObjectWriter.WriteLine("public class {0} : IDataObject", dataObject.objectName);
      _dataObjectWriter.WriteLine("{"); // begin class block
      _dataObjectWriter.Indent++;

      if (dataObject.keyProperties.Count > 1)
      {
        _dataObjectWriter.WriteLine("public {0}()", dataObject.objectName);
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("Id = new {0}Id();", dataObject.objectName);
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.WriteLine("public virtual {0} Id {{ get; set; }}", keyClassName);

        foreach (KeyProperty keyName in dataObject.keyProperties)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(keyName.keyPropertyName);

          _dataObjectWriter.WriteLine("public virtual {0} {1}", keyProperty.dataType, keyProperty.propertyName);
          _dataObjectWriter.WriteLine("{");
          _dataObjectWriter.Indent++;
          _dataObjectWriter.WriteLine("get {{ return Id.{0}; }}", keyProperty.propertyName);
          _dataObjectWriter.WriteLine("set {{ Id.{0} = value; }}", keyProperty.propertyName);
          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}");

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", keyProperty.propertyName);
          _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
          _mappingWriter.WriteAttributeString("update", "false");
          _mappingWriter.WriteAttributeString("insert", "false");
          _mappingWriter.WriteEndElement();
        }
      }
      else if (dataObject.keyProperties.Count == 1)
      {
        DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties.First().keyPropertyName);

        _dataObjectWriter.WriteLine("public virtual {0} Id {{ get; set; }}", keyProperty.dataType);

        _mappingWriter.WriteStartElement("id");
        _mappingWriter.WriteAttributeString("name", "Id");
        _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
        _mappingWriter.WriteStartElement("generator");
        _mappingWriter.WriteAttributeString("class", keyProperty.keyType.ToString());
        _mappingWriter.WriteEndElement(); // end generator element
        _mappingWriter.WriteEndElement(); // end id element

        if (keyProperty.keyType == KeyType.assigned)
        {
          _dataObjectWriter.WriteLine("public virtual {0} {1}", keyProperty.dataType, keyProperty.propertyName);
          _dataObjectWriter.WriteLine("{");
          _dataObjectWriter.Indent++;
          _dataObjectWriter.WriteLine("get { return Id; }");
          _dataObjectWriter.WriteLine("set { Id = value; }");
          _dataObjectWriter.Indent--;
          _dataObjectWriter.WriteLine("}");

          _mappingWriter.WriteStartElement("property");
          _mappingWriter.WriteAttributeString("name", keyProperty.propertyName);
          _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
          _mappingWriter.WriteAttributeString("update", "false");
          _mappingWriter.WriteAttributeString("insert", "false");
          _mappingWriter.WriteEndElement(); // end property element          
        }
      }

      #region Process relationships
      //if (dataObject.dataRelationships != null)
      //{
      //  foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
      //  {
      //    DataObject relatedDataObject = GetDataObject(dataRelationship.relatedObjectName);

      //    switch (dataRelationship.relationshipType)
      //    {
      //      case RelationshipType.OneToOne:

              //DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties.First().keyPropertyName);

              /*
              _dataObjectWriter.WriteLine("public virtual {0} Id {{ get; set; }}", keyProperty.dataType);

              _mappingWriter.WriteStartElement("id");
              _mappingWriter.WriteAttributeString("name", "Id");
              _mappingWriter.WriteAttributeString("column", "\"" + keyProperty.columnName + "\"");
              _mappingWriter.WriteStartElement("generator");
              _mappingWriter.WriteAttributeString("class", keyProperty.keyType.ToString());
              _mappingWriter.WriteStartElement("param");
              _mappingWriter.WriteAttributeString("name", "property");
              _mappingWriter.WriteString(dataRelationship.relatedObjectName);
              _mappingWriter.WriteEndElement(); // end param element
              _mappingWriter.WriteEndElement(); // end generator element
              _mappingWriter.WriteEndElement(); // end id element
              */

              //_mappingWriter.WriteStartElement("one-to-one");
              //_mappingWriter.WriteAttributeString("name", dataRelationship.relationshipName);
              //_mappingWriter.WriteAttributeString("class", _namespace + "." + dataRelationship.relatedObjectName + ", " + _settings["ExecutingAssemblyName"]);
              //_mappingWriter.WriteAttributeString("cascade", "save-update");

              /*
              if (oneToOneRelationship.isKeySource)
              {
                _mappingWriter.WriteAttributeString("cascade", "save-update");
              }
              else
              {
                _mappingWriter.WriteAttributeString("constrained", "true");
              }
              */

              //_dataObjectWriter.WriteLine("public virtual {0} {1} {{ get; set; }}", dataRelationship.relatedObjectName, dataRelationship.relationshipName);
              //_mappingWriter.WriteEndElement(); // end one-to-one element

            //  break;

            //case RelationshipType.OneToMany:

              //if (dataRelationship.propertyMaps.Count > 0)
              //{
              //  _dataObjectWriter.WriteLine("public virtual Iesi.Collections.Generic.ISet<{0}> {1} {{ get; set; }}", dataRelationship.relatedObjectName, dataRelationship.relationshipName);
              //  _mappingWriter.WriteStartElement("set");
              //  _mappingWriter.WriteAttributeString("name", dataRelationship.relationshipName);
              //  _mappingWriter.WriteAttributeString("table", relatedDataObject.tableName);
              //  _mappingWriter.WriteAttributeString("inverse", "true");
              //  _mappingWriter.WriteAttributeString("cascade", "all-delete-orphan");

              //  if (dataRelationship.propertyMaps.Count == 1)
              //  {
              //    _mappingWriter.WriteStartElement("key");
              //    _mappingWriter.WriteAttributeString("column", "\"" + GetColumnName(relatedDataObject, dataRelationship.propertyMaps.First().relatedPropertyName) + "\"");
              //    _mappingWriter.WriteEndElement(); // end key
              //  }
              //  else 
              //  {
              //    _mappingWriter.WriteStartElement("key");
              //    foreach (PropertyMap propertyMap in dataRelationship.propertyMaps)
              //    {
              //      _mappingWriter.WriteStartElement("column");
              //      _mappingWriter.WriteAttributeString("name", "\"" + GetColumnName(relatedDataObject, propertyMap.relatedPropertyName) + "\"");
              //      _mappingWriter.WriteEndElement(); // end column
              //    }
              //    _mappingWriter.WriteEndElement(); // end key
              //  }

              //  _mappingWriter.WriteStartElement("one-to-many");
              //  _mappingWriter.WriteAttributeString("class", _namespace + "." + dataRelationship.relatedObjectName + ", " + _settings["ExecutingAssemblyName"]);
              //  _mappingWriter.WriteEndElement(); // one-to-many
              //  _mappingWriter.WriteEndElement(); // end set element
              //}
      //        break;
      //    }
      //  }
      //}
      #endregion Process relationships

      #region Process columns
      if (dataObject.dataProperties != null)
      {
        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          if (!dataObject.isKeyProperty(dataProperty.propertyName))
          {
            bool isNullableType = (dataProperty.dataType != DataType.String && dataProperty.isNullable == true);
            if (isNullableType)
            {
              _dataObjectWriter.WriteLine("public virtual {0}? {1} {{ get; set; }}", dataProperty.dataType, dataProperty.propertyName);
            }
            else
            {
              _dataObjectWriter.WriteLine("public virtual {0} {1} {{ get; set; }}", dataProperty.dataType, dataProperty.propertyName);
            }  

            _mappingWriter.WriteStartElement("property");
            _mappingWriter.WriteAttributeString("name", dataProperty.propertyName);
            _mappingWriter.WriteAttributeString("column", "\"" + dataProperty.columnName + "\"");
            _mappingWriter.WriteEndElement(); // end property element
          }
        }

        // Implements GetPropertyValue of IDataObject
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("public virtual object GetPropertyValue(string propertyName)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++; _dataObjectWriter.WriteLine("switch (propertyName)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("case \"Id\": return Id;");

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          _dataObjectWriter.WriteLine("case \"{0}\": return {0};", dataProperty.propertyName);
        }

        _dataObjectWriter.WriteLine("default: throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");

        // Implements SetPropertyValue of IDataObject
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine("public virtual void SetPropertyValue(string propertyName, object value)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("switch (propertyName)");
        _dataObjectWriter.Write("{");
        _dataObjectWriter.Indent++;

        if (dataObject.keyProperties.Count == 1)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(dataObject.keyProperties.First().keyPropertyName);
          DataType keyDataType = keyProperty.dataType;

          if (IsNumeric(keyDataType))
          {
            _dataObjectWriter.WriteLine(@"
        case ""Id"":
          Id = {0}.Parse((String)value, NumberStyles.Any);
          break;", keyDataType);
          }
          else
          {
            _dataObjectWriter.WriteLine(@"
        case ""Id"":
          Id = Convert.To{0}(value);
          break;", keyDataType);
          }
        }
        else if (dataObject.keyProperties.Count > 1)
        {
          _dataObjectWriter.WriteLine(@"
        case ""Id"":
          Id = ({0}Id)value;
          break;", dataObject.objectName);
        }
        
        /*
        foreach (KeyProperty keyName in dataObject.keyProperties)
        {
          DataProperty keyProperty = dataObject.getKeyProperty(keyName.keyPropertyName);

          _dataObjectWriter.WriteLine("case \"{0}\":", keyProperty.propertyName);
          _dataObjectWriter.Indent++;

          bool isDataPropertyNullable = (keyProperty.dataType == DataType.String || keyProperty.isNullable == true);
          if (isDataPropertyNullable)
          {
            _dataObjectWriter.WriteLine("if (value != null) {0} = Convert.To{1}(value);", keyProperty.propertyName, keyProperty.dataType);
          }
          else
          {
            _dataObjectWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", keyProperty.propertyName, keyProperty.dataType);
          }

          _dataObjectWriter.WriteLine("break;");
          _dataObjectWriter.Indent--;
        }*/

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          _dataObjectWriter.WriteLine("case \"{0}\":", dataProperty.propertyName);
          _dataObjectWriter.Indent++;

          bool dataPropertyIsNullable = (dataProperty.dataType == DataType.String || dataProperty.isNullable == true);
          if (dataPropertyIsNullable)
          {
            if (IsNumeric(dataProperty.dataType))
            {
              _dataObjectWriter.WriteLine("{0} = {1}.Parse((String)value, NumberStyles.Any);", dataProperty.propertyName, dataProperty.dataType);
            }
            else
            {
              _dataObjectWriter.WriteLine("{0} = Convert.To{1}(value);", dataProperty.propertyName, dataProperty.dataType);
            }
          }
          else
          {
            _dataObjectWriter.WriteLine("{0} = (value != null) ? Convert.To{1}(value) : default({1});", dataProperty.propertyName, dataProperty.dataType);
          }

          _dataObjectWriter.WriteLine("break;");
          _dataObjectWriter.Indent--;
        }

        _dataObjectWriter.WriteLine("default:");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
      #endregion Process columns

        #region generate GetRelatedObjects method
        _dataObjectWriter.WriteLine();
        _dataObjectWriter.WriteLine(@"public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("switch (relatedObjectType)");
        _dataObjectWriter.WriteLine("{");
        _dataObjectWriter.Indent++;

        //foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
        //{
        //  _dataObjectWriter.WriteLine("case \"{0}\":", dataRelationship.relationshipName);
        //  _dataObjectWriter.Indent++;

        //  if (dataRelationship.relationshipType == RelationshipType.OneToOne)
        //  {
        //    _dataObjectWriter.WriteLine(@"return new List<IDataObject>{{{0}}};", dataRelationship.relationshipName);
        //  }
        //  else if (dataRelationship.relationshipType == RelationshipType.OneToMany)
        //  {
        //    _dataObjectWriter.WriteLine(@"IList<IDataObject> relatedObjects = new List<IDataObject>();");
        //    _dataObjectWriter.WriteLine(@"foreach ({0} relatedObject in {1}) relatedObjects.Add(relatedObject);", dataRelationship.relatedObjectName, dataRelationship.relationshipName);
        //    _dataObjectWriter.WriteLine(@"return relatedObjects;");
        //  }

        //  _dataObjectWriter.Indent--;
        //}

        _dataObjectWriter.WriteLine("default:");
        _dataObjectWriter.Indent++;
        _dataObjectWriter.WriteLine("throw new Exception(\"Related object [\" + relatedObjectType + \"] does not exist.\");");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}");
        #endregion

        _dataObjectWriter.Indent--;
        _dataObjectWriter.WriteLine("}"); // end class block
        _mappingWriter.WriteEndElement(); // end class element
      }
    }

    private string CreateConfiguration(Provider provider, string connectionString, String defaultSchema)
    {
      string driver = String.Empty;
      string dialect = String.Empty;
            
      try
      {
        string dbProvider = provider.ToString();

        if (dbProvider.ToUpper().Contains("MSSQL"))
        {
          driver = "NHibernate.Driver.SqlClientDriver";
        }
        else if (dbProvider.ToUpper().Contains("MYSQL"))
        {
          driver = "NHibernate.Driver.MySqlDataDriver";
        }
        else if (dbProvider.ToUpper().Contains("ORACLE"))
        {
          driver = "NHibernate.Driver.OracleClientDriver";
        }
        else
          throw new Exception(string.Format("Database provider {0} is not supported", dbProvider));

        dialect = "NHibernate.Dialect." + dbProvider + "Dialect";

        StringBuilder configBuilder = new StringBuilder();
        XmlTextWriter configWriter = new XmlTextWriter(new StringWriter(configBuilder));

        configWriter.Formatting = Formatting.Indented;
        configWriter.WriteStartElement("configuration");
        configWriter.WriteStartElement("hibernate-configuration", "urn:nhibernate-configuration-2.2");
        configWriter.WriteStartElement("session-factory");
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "connection.provider");
        configWriter.WriteString("NHibernate.Connection.DriverConnectionProvider");
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "connection.driver_class");
        configWriter.WriteString(driver);
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "connection.connection_string");
        configWriter.WriteString(EncryptionUtility.Encrypt(connectionString));
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "proxyfactory.factory_class");
        configWriter.WriteString("NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle");
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "default_schema");
        configWriter.WriteString(defaultSchema);
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "dialect");
        configWriter.WriteString(dialect);
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteStartElement("property");
        configWriter.WriteAttributeString("name", "show_sql");
        configWriter.WriteString("true");
        configWriter.WriteEndElement(); // end property element
        configWriter.WriteEndElement(); // end session-factory element
        configWriter.WriteEndElement(); // end hibernate-configuration element
        configWriter.WriteEndElement(); // end configuration element
        configWriter.Close();

        return configBuilder.ToString();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    private DataObject GetDataObject(string dataObjectName)
    {
      foreach (DataObject dataObject in _dataObjects)
      {
        if (dataObject.objectName.ToLower() == dataObjectName.ToLower())
          return dataObject;
      }

      return null;
    }

    private string GetColumnName(DataObject dataObject, string propertyName)
    {
      foreach (DataProperty property in dataObject.dataProperties)
      {
        if (property.propertyName.ToLower() == propertyName.ToLower())
          return property.columnName;
      }

      return String.Empty;
    }

    private bool IsNumeric(DataType dataType)
    {
      if (dataType == DataType.Int32 ||
          dataType == DataType.Decimal ||
          dataType == DataType.Double ||
          dataType == DataType.Single ||
          dataType == DataType.Int16 ||
          dataType == DataType.Int64 ||
          dataType == DataType.Byte)
      {
        return true;
      }

      return false;
    }
  }
}