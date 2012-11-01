using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
using System.Xml.Linq;
using IDS_ADI.InformationModel;
using org.ids_adi.qmxf;
using org.iringtools.utility;
using ExtremeML.Packaging;
using ExtremeML.Spreadsheet;
using System.Text.RegularExpressions;
using ExtremeML.Spreadsheet.Address;
using org.iringtools.library;

namespace QMXFGenerator
{
  class Program
  {
    #region Private Members
    private static string _excelFilePath = String.Empty;
    private static string _qmxfFilePath = String.Empty;
    private static string _processedFilePath = String.Empty;
    private static string _proxyHost = String.Empty;
    private static string _proxyPort = String.Empty;
    private static string _proxyCredentials = String.Empty;
    private static string _idsADICredentials = String.Empty;
    private static string _classRegistryBase = String.Empty;
    private static string _templateRegistryBase = String.Empty;
    private static string _targetRepository = string.Empty;
    private static string _updateRun = string.Empty;
    private static SpreadsheetDocumentWrapper document = null;

    private static WorksheetPartWrapper _classWorksheet = null;
    private static WorksheetPartWrapper _classSpecializationWorksheet = null;
    private static WorksheetPartWrapper _baseTemplateWorksheet = null;
    private static WorksheetPartWrapper _classificationWorksheet = null;
    private static WebHttpClient _refdataClient = null;
    private static string _refdataServiceUri = null;
    private static List<ArrayList> _classes = new List<ArrayList>();
    private static List<ArrayList> _classSpecializations = new List<ArrayList>();
    private static List<ArrayList> _classifications = new List<ArrayList>();
    private static List<ArrayList> _baseTemplates = new List<ArrayList>();
    private static List<ArrayList> _siTemplates = new List<ArrayList>();
    private static List<ArrayList> _roles = new List<ArrayList>();
    #endregion

    static void Main(string[] args)
    {

      try
      {
        {
          QMXF qmxf = new QMXF();
          if (Initialize(args))
          {
            using (document = SpreadsheetDocumentWrapper.Open(_excelFilePath))
            {
              _refdataClient = new WebHttpClient(_refdataServiceUri);
              _classWorksheet = GetWorksheet(document, "Class");
              _classSpecializationWorksheet = GetWorksheet(document, "Class Specialization");
              Console.WriteLine("Processing Classes...");
              qmxf.classDefinitions = ProcessClass(_classWorksheet, _classSpecializationWorksheet);
              
              _classificationWorksheet = GetWorksheet(document, "Classification");

              Console.WriteLine("Processing Classifications...");
              ProcessClassDefinitions(_classificationWorksheet, qmxf.classDefinitions);

              _baseTemplateWorksheet = GetWorksheet(document, "Base Template");

              Console.WriteLine("Processing Base Templates...");
              qmxf.templateDefinitions = ProcessBaseTemplate(_baseTemplateWorksheet);
             
              WorksheetPartWrapper specializedIndividualTemplateWorksheet = GetWorksheet(document, "Specialized Individual Template");

              Console.WriteLine("Processing Specialized Individual Templates...");
              qmxf.templateQualifications = ProcessSpecializedIndividualTemplates(specializedIndividualTemplateWorksheet);

              specializedIndividualTemplateWorksheet = null;
              _baseTemplateWorksheet = null;
              _classSpecializationWorksheet = null;
              _classWorksheet = null;
            }
            ///Post Classes and Templates individually to refdataService
            var error = false;
            if (!string.IsNullOrEmpty(_updateRun))
            {
              foreach (var cls in qmxf.classDefinitions)
              {
                try
                {
                  if (!CheckUri(cls.identifier))
                  {
                    Utility.WriteString("Cannot Post Example namespace " + cls.identifier + "\n", "error.log", true);
                    continue;
                  }
                  var q = new QMXF { targetRepository = _targetRepository };
                  q.classDefinitions.Add(cls);
                  Response resp = _refdataClient.Post<QMXF, Response>("/classes", q, true);
                  if (resp.Level == StatusLevel.Error)
                  {
                    Console.WriteLine("Error posting class: " + cls.name[0].value);
                    Utility.WriteString("Error posting class: " + cls.name[0].value + "\n", "error.log", true);
                  }
                  else
                    Console.WriteLine("Success: posted class: " + cls.name[0].value);
                }
                catch (Exception)
                {
                  Utility.WriteString("Error posting class: " + cls.name[0].value + "\n", "error.log", true);
                }
              }
              ///Post baseTemplates
              foreach (var t in qmxf.templateDefinitions)
              {
                try
                {
                  if (!CheckUri(t.identifier))
                  {
                    error = true;
                    Utility.WriteString("Cannot Post Example namespace " + t.identifier + "\n", "error.log", true);
                  }

                  foreach (var r in t.roleDefinition)
                  {
                    if (string.IsNullOrEmpty(r.range))
                    {
                      Utility.WriteString("\n" + r.identifier + " do not have range defined \n", "error.log", true);
                      Console.WriteLine("error in template " + t.identifier + " see : error.log");
                      error = true;
                    }
                    else if (!CheckUri(r.identifier))
                    {
                      Utility.WriteString("Cannot Post Example namespace " + r.identifier + "\n", "error.log", true);
                      error = true;
                    }

                  }
                  if (error)
                  {
                    error = false;
                    break;
                  }
                  var q = new QMXF { targetRepository = _targetRepository };
                  q.templateDefinitions.Add(t);
                  Response resp = _refdataClient.Post<QMXF, Response>("/templates", q, true);
                  if (resp.Level == StatusLevel.Error)
                  {
                    Console.WriteLine("Error posting baseTemplate: " + t.name[0].value);
                    Utility.WriteString("Error posting baseTemplate: " + t.name[0].value + "\n", "error.log", true);
                  }
                  else
                    Console.WriteLine("Success: posted baseTemplate: " + t.name[0].value);
                }
                catch (Exception)
                {
                  Utility.WriteString("Error posting baseTemplate: " + t.name[0].value + "\n", "error.log", true);
                }
              }
              ///Post Specialised templates
              foreach (var t in qmxf.templateQualifications)
              {
                try
                {
                  if (!CheckUri(t.identifier))
                  {
                    Utility.WriteString("Cannot Post Example namespace " + t.identifier + "\n", "error.log", true);
                    error = true;
                    continue;
                  }
                  foreach (var r in t.roleQualification)
                  {
                    if (string.IsNullOrEmpty(r.range))
                    {
                      Utility.WriteString("\n" + r.identifier + " do not have range defined \n", "error.log", true);
                      Console.WriteLine("error in template " + t.identifier + " see : error.log");
                      error = true;
                    }
                  }
                  if (error)
                  {
                    error = false;
                    break;
                  }
                  var q = new QMXF { targetRepository = _targetRepository };
                  q.templateQualifications.Add(t);
                  Response resp = _refdataClient.Post<QMXF, Response>("/templates", q, true);
                  if (resp.Level == StatusLevel.Error)
                  {
                    Console.WriteLine("Error posting specializedTemplate: " + t.name[0].value);
                    Utility.WriteString("Error posting specializedTemplate: " + t.name[0].value + "\n", "error.log", true);
                  }
                  else
                    Console.WriteLine("Success: posted specializedTemplate: " + t.name[0].value);
                }
                catch (Exception)
                {
                  Utility.WriteString("Error posting specializedTemplate: " + t.name[0].value + "\n", "error.log", true);
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Utility.WriteString("\n" + ex.ToString() + "\n", "error.log", true);
        Console.WriteLine("Failure: See log file: error.log");
      }

      Console.ReadKey();
    }

    private static bool CheckUri(string uri)
    {
      if(uri.Contains("example"))
        return false;
      else
        return true;
    }

    private static WorksheetPartWrapper GetWorksheet(SpreadsheetDocumentWrapper document, string sheetName)
    {
      return document.WorkbookPart.WorksheetParts[sheetName];
    }

    private string GetCellValue(WorksheetPartWrapper part, int startCol, int startRow)
    {

      var row = part.Worksheet.SheetData.Rows.FirstOrDefault(c => c.RowIndex == startRow);
      return row.GetCell(startCol, false).CellValue.Value;
    }

    private static void ProcessClassDefinitions(WorksheetPartWrapper _classificationWorksheet, List<ClassDefinition> list)
    {
      try
      {
        _classifications = MarshallToList(_classificationWorksheet);
        foreach (var c in _classifications)
        {
          var query = from cls in _classes
                      where cls[(int)ClassColumns.Label].ToString().Trim().Equals(c[(int)ClassificationColumns.Classified].ToString())
                      select cls[(int)ClassColumns.ID];
          var cl = list.SingleOrDefault(l => l.name[0].value.Equals(c[(int)ClassificationColumns.Class].ToString()));
          if (cl != null && query != null && query.Count() > 0)
          {
            cl.classification.Add(new Classification
            {
              label = c[(int)ClassificationColumns.Classified].ToString(),
              lang = "en",
              reference = query.FirstOrDefault().ToString()
            });
          }
        }
      }
      catch (Exception)
      {
      }
    }
    private static bool Initialize(string[] args)
    {
      try
      {
        if (args.Length < 2)
        {
          _excelFilePath = System.Configuration.ConfigurationManager.AppSettings["ExcelFilePath"];
          _targetRepository = System.Configuration.ConfigurationManager.AppSettings["TargetRepositoryName"];
          _refdataServiceUri = System.Configuration.ConfigurationManager.AppSettings["RefdataServiceUri"];
          _updateRun = System.Configuration.ConfigurationManager.AppSettings["UpdateRun"];
        }
        else
        {
          _excelFilePath = args[0];

        }

        if (_excelFilePath == String.Empty)
        {
          Console.WriteLine("Usage: \n");
          Console.WriteLine("   qmxfgen.exe <excel file> <output file>");
          return false;
        }
        _proxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
        _proxyPort = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
        _proxyCredentials = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];
        _idsADICredentials = System.Configuration.ConfigurationManager.AppSettings["IDSADICredentialToken"];

        bool useTestRegistry = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["UseTestRegistry"]);

        if (useTestRegistry)
        {
          string testRegistryBase = System.Configuration.ConfigurationManager.AppSettings["TestRegistryBase"];

          _classRegistryBase = testRegistryBase;
          _templateRegistryBase = testRegistryBase;
        }
        else
        {
          _classRegistryBase = System.Configuration.ConfigurationManager.AppSettings["ClassRegistryBase"];
          _templateRegistryBase = System.Configuration.ConfigurationManager.AppSettings["TemplateRegistryBase"];
        }

        return true;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\nError Initializing \n" + ex.ToString() + "\n", "error.log", true);
        throw ex;
      }
    }

    private static List<ClassDefinition> ProcessClass(WorksheetPartWrapper classPart, WorksheetPartWrapper specializationPart)
    {
      int rowIndex = 0;
      int idx = 0;
      try
      {
        _classes = MarshallToList(classPart);
        _classSpecializations = MarshallToList(specializationPart);

        List<ClassDefinition> classDefinitions = new List<ClassDefinition>();
        foreach (ArrayList row in _classes)
        {
          object load = row[(int)ClassColumns.Load];
          rowIndex = Convert.ToInt32(row[row.Count - 1]);
          if (load != null && load.ToString().Trim() != String.Empty && load.ToString() != "Load")
          {            
            object identifier = row[(int)ClassColumns.ID];
            object label = row[(int)ClassColumns.Label];
            object description = row[(int)ClassColumns.Description];
            object entityType = row[(int)ClassColumns.EntityType];

            ClassDefinition classDefinition = new ClassDefinition();

            string name = String.Empty;
            if (label != null)
            {
              name = label.ToString();

              if (name != String.Empty)
              {
                QMXFName englishUSName = new QMXFName
                {
                  lang = "en",
                  value = name,
                };

                classDefinition.name = new List<QMXFName>
                { 
                  englishUSName 
                };
              }
            }

            if (identifier == null || identifier.ToString() == String.Empty)
            {
              identifier = GenerateID(_classRegistryBase, name);
              //write to the in-memory list
              _classes[idx][(int)ClassColumns.ID] = identifier;
              //write to the sheet, but offset counters for 1-based array
              classPart.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int)ClassColumns.ID), identifier);
            }

            classDefinition.identifier = identifier.ToString();
            if (description != null && description.ToString() != String.Empty)
            {
              Description englishUSDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };
              classDefinition.description = new List<Description> 
              {
                englishUSDescription,
              };
            }
            string ent = entityType.ToString();
            if (!string.IsNullOrEmpty(ent))
            {
              classDefinition.entityType = new EntityType
              {
                reference = ent
              };
            }

            List<Specialization> classSpecialization = ProcessClassSpecialization(name);

            if (classSpecialization.Count > 0)
              classDefinition.specialization = classSpecialization;

            load = String.Empty;
            idx++;
            if (!string.IsNullOrEmpty(ent)) /// must have entity type
              classDefinitions.Add(classDefinition);
          }
        }
        Console.WriteLine("  processed " + idx + " classes.");
        return classDefinitions;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\nError Processing Class \n Worksheet: " + classPart +
                            "\t Row: " + idx + " \n" + ex.ToString() + "\n", "error.log");
        throw ex;
      }
    }

       private static string GenerateID(string registryBase, string name)
    {
      try
      {
        string identifier = String.Empty;


        if (!string.IsNullOrEmpty(registryBase))
          return string.Format("{0}R{1}", registryBase, Guid.NewGuid().ToString().Replace("_", "").Replace("-", "").ToUpper());
        else
        {
           Utility.WriteString("Failed to create id for "+ name , "error.log");
          throw new Exception("CreateIdsAdiId: Failed to create id ");

        }
      
      }
      catch (Exception ex)
      {
        Utility.WriteString("Error Generating ID\n" + ex.ToString() + "\n", "error.log");
        throw ex;
      }
    }

    private static List<Specialization> ProcessClassSpecialization(string className)
    {
      try
      {
        List<Specialization> classSpecializations = new List<Specialization>();

        //Find the class specializations
        var specializationList = from specialization in _classSpecializations
                                 where Convert.ToString(specialization[(int)ClassSpecializationColumns.Superclass]) == className
                                 select specialization;
        //Get their details from the Class List
        List<ArrayList> superclasses = new List<ArrayList>();

        foreach (ArrayList specialization in specializationList)
        {
          object subclass = specialization[(int)ClassSpecializationColumns.Subclass];
          var query = from @class in _classes
                      where Convert
                         .ToString(@class[(int)ClassColumns.Label])
                         .Trim() == subclass.ToString().Trim()
                      select @class;

          if (query.Count() > 0 && query.FirstOrDefault().Count > 0)
          {
            superclasses.Add(query.FirstOrDefault());
          }
          else
          {
            Utility.WriteString("\n " + subclass.ToString() + " Was Not Found in Class List", "error.log", true);
          }
        }

        //Use the details for each to create the Specializations and add to List to return
        foreach (ArrayList superClassRow in superclasses)
        {
          object superclassIdentifier = superClassRow[(int)ClassColumns.ID];
          object superclassName = superClassRow[(int)ClassColumns.Label];

          if (superclassIdentifier != null && superclassName != null &&
              superclassIdentifier.ToString() != String.Empty)
          {
            Specialization specialization = new Specialization
            {
              label = superclassName.ToString(),
              reference = superclassIdentifier.ToString().Trim(),
            };

            classSpecializations.Add(specialization);
          }
        }
        return classSpecializations;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\nError Processing Class Specialization \n" +
                            "Worksheet: " + _classSpecializationWorksheet + " \n" + ex.ToString() + "\n", "error.log", true);
        throw ex;
      }
    }

    private static List<TemplateDefinition> ProcessBaseTemplate(WorksheetPartWrapper part)
    {
      int rowIndex = 0;
      int idx = 0;
      try
      {
        _baseTemplates = MarshallToList(part);
        List<TemplateDefinition> templateDefinitions = new List<TemplateDefinition>();
        foreach (ArrayList row in _baseTemplates)
        {
          rowIndex = Convert.ToInt32(row[row.Count - 1]);
          object load = row[(int)TemplateColumns.Load];
          if (load != null && load.ToString().Trim() != String.Empty && load.ToString() != "Load")
          {
            object templateIdentifier = row[(int)TemplateColumns.ID];
            object templateName = row[(int)TemplateColumns.Name];
            object description = row[(int)TemplateColumns.Description];
            TemplateDefinition templateDefinition = new TemplateDefinition();
            string name = String.Empty;
            if (templateName != null)
            {
              name = templateName.ToString();
              if (name != String.Empty)
              {
                QMXFName englishUSName = new QMXFName
                {
                  lang = "en",
                  value = name,
                };
                templateDefinition.name = new List<QMXFName>
                                { 
                                  englishUSName 
                                };
              }
            }
            if (templateIdentifier == null || templateIdentifier.ToString() == String.Empty)
            {
              templateIdentifier = GenerateID(_templateRegistryBase, name);
              //write to the in-memory list
              foreach (var b in _baseTemplates)
              {
                if (Convert.ToInt32(b[b.Count - 1]) == rowIndex)
                {
                  b[(int)TemplateColumns.ID] = templateIdentifier;
                }
              }
              //write to the sheet, but offset counters for 1-based array
              part.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int)TemplateColumns.ID), templateIdentifier);
            }
            templateDefinition.identifier = templateIdentifier.ToString().Trim();
            if (description != null && description.ToString() != String.Empty)
            {
              Description englishUSDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };
              templateDefinition.description = new List<Description> 
                            {
                                englishUSDescription,
                            };
            }
            templateDefinition.roleDefinition = ProcessRoleDefinition(templateDefinition.name.FirstOrDefault().value, row, Convert.ToInt32(row[row.Count - 1]), part);
            load = String.Empty;
            templateDefinitions.Add(templateDefinition);
            idx++;
          }
          
        }
        Console.WriteLine("  processed " + idx + " base templates.");
        return templateDefinitions;
      }
      catch (Exception e)
      {
        Utility.WriteString("\nError Processing Template \n Worksheet: " + part.Name + "\tRow: "
                             + idx + " \n" + e.ToString() + "\n", "error.log", true);
        throw e;
      }
    }

    private static List<RoleDefinition> ProcessRoleDefinition(string templateName, ArrayList row, int rowIndex, WorksheetPartWrapper part)
    {
      try
      {
        int idx = 0;
        List<RoleDefinition> roleDefinitions = new List<RoleDefinition>();
        for (int roleIndex = 0; roleIndex <= (int)RoleColumns.Max - 1; roleIndex++)
        {
          int roleOffset = (int)TemplateColumns.Roles + ((int)RoleColumns.Count * roleIndex);
          object identifier = row[(int)RoleColumns.ID + roleOffset];
          object label = row[(int)RoleColumns.Name + roleOffset];
          object description = row[(int)RoleColumns.Description + roleOffset];
          object type = row[(int)RoleColumns.Type + roleOffset];

          if (label != null && label.ToString().Trim() != String.Empty)
          {
            string name = label.ToString();
            RoleDefinition roleDefinition = new RoleDefinition();

            QMXFName englishUSName = new QMXFName
            {
              lang = "en",
              value = name,
            };

            roleDefinition.name = new List<QMXFName>
            { 
              englishUSName 
            };

            if (identifier == null || identifier.ToString() == String.Empty)
            {
              identifier = GenerateID(_templateRegistryBase, name);

              //write to the in-memory list
              _baseTemplates[idx][(int)RoleColumns.ID + roleOffset] = identifier;

              //write to the sheet, but offset counters for 1-based array
              part.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int)RoleColumns.ID + roleOffset), identifier);
            }
            roleDefinition.identifier = identifier.ToString();

            if (description != null && description.ToString() != String.Empty)
            {
              Description englishUSDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };
              roleDefinition.description = englishUSDescription;
            }
            
            if (type != null && type.ToString() != String.Empty)
            {
              var query = from clss in _classes
                          where Convert.ToString(clss[(int)ClassColumns.Label].ToString().ToUpper()) == type.ToString().ToUpper()
                          select clss;
              if (query.FirstOrDefault() != null & query.FirstOrDefault()[(int)ClassColumns.Label].ToString().Trim().Equals(type.ToString()))
              {
                roleDefinition.range = query.FirstOrDefault()[(int)ClassColumns.ID].ToString().Trim();
              }
              else
              {
                Utility.WriteString("\n " + type.ToString() + " Was Not Found in Class List While Processing Role Definition", "error.log", true);
              }
            }
            else
            {
              Utility.WriteString("\nType Was Not Set for Role Definition \"" + englishUSName.value + "\" on template \"" + templateName + "\".", "error.log", true);
            }
            roleDefinitions.Add(roleDefinition);
          }
        }
        return roleDefinitions;
      }
      catch (Exception e)
      {
        Utility.WriteString("\nError Processing Role \n Row: " + rowIndex + " \n" + e.ToString() + "\n", "error.log", true);
        throw e;
      }
    }

    private static List<TemplateQualification> ProcessSpecializedIndividualTemplates(WorksheetPartWrapper part)
    {
      int rowIndex = 0;
      int idx = 0;
      try
      {
        _siTemplates = MarshallToList(part);
        List<TemplateQualification> templateQualifications = new List<TemplateQualification>();
        foreach (ArrayList row in _siTemplates)
        {
          rowIndex = Convert.ToInt32(row[row.Count - 1]);
          object load = row[(int)TemplateColumns.Load];

          if (load != null && load.ToString().Trim() != String.Empty && load.ToString() != "Load")
          {
            object templateIdentifier = row[(int)TemplateColumns.ID];
            object templateName = row[(int)TemplateColumns.Name];
            object description = row[(int)TemplateColumns.Description];
            object parentTemplate = row[(int)TemplateColumns.ParentTemplate];
            TemplateQualification templateQualification = new TemplateQualification();
            string name = String.Empty;
            if (templateName != null)
            {
              name = templateName.ToString();
              if (name != String.Empty)
              {
                QMXFName englishUSName = new QMXFName
                {
                  lang = "en",
                  value = name,
                };
                templateQualification.name = new List<QMXFName>
                    { 
                      englishUSName 
                    };
              }
            }
            if (templateIdentifier == null || templateIdentifier.ToString() == String.Empty)
            {
              templateIdentifier = GenerateID(_templateRegistryBase, name);
              //write to the in-memory list
              _siTemplates[rowIndex - 1][(int)TemplateColumns.ID] = templateIdentifier;
              //write to the sheet, but offset counters for 1-based array
              part.Worksheet.SetCellValue(new GridReference(rowIndex - 1, (int)TemplateColumns.ID), templateIdentifier);
            }
            templateQualification.identifier = templateIdentifier.ToString().Trim();
            if (description != null && description.ToString() != String.Empty)
            {
              Description englishUSDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };
              templateQualification.description = new List<Description> 
                  {
                    englishUSDescription,
                  };
            }
            if (parentTemplate != null && parentTemplate.ToString() != String.Empty)
            {
              var query = from template in _baseTemplates
                          where Convert.ToString(template[(int)TemplateColumns.Name]) == parentTemplate.ToString()
                          select template;

              ArrayList parentRow = query.FirstOrDefault();
              if (parentRow != null)
              {
                object templateQualifiesId = parentRow[(int)TemplateColumns.ID];
                if (templateQualifiesId == null)
                {
                  Utility.WriteString("Template Qualification \"" + templateQualification.identifier + "\" qualifies ID not found.\n", "error.log", true);
                }
                templateQualification.qualifies = (templateQualifiesId ?? "").ToString().Trim();
                templateQualification.roleQualification = ProcessRoleQualification(templateQualification.name.FirstOrDefault().value, row, parentRow, rowIndex, part);
              }
              else
              {
                Utility.WriteString(parentTemplate.ToString() + " Was Not Found in Template List While Processing Specialized Templates.\n", "error.log", true);
              }
            }
            load = String.Empty;
            idx++;
            if (templateQualification.roleQualification.Count > 0)
            {
              templateQualifications.Add(templateQualification);
            }
            else
              Utility.WriteString("Template Qualification \"" + templateQualification.identifier + "\" RoleQualifications failed.\n", "error.log", true);
          }
          
        }
        Console.WriteLine("  processed " + idx + " Specialized templates.");
        return templateQualifications;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\nError Processing Individual Template \n" +
                            "Worksheet: " + part.Name + "\tRow: " + idx +
                            " \n" + ex.ToString() + "\n", "error.log", true);
        throw ex;
      }
    }

    private static List<RoleQualification> ProcessRoleQualification(string templateName, ArrayList row, ArrayList parentRow, int rowIndex, WorksheetPartWrapper part) 
    {
      int roleIndex = 0;
      
      try
      {
        List<RoleQualification> roleQualifications = new List<RoleQualification>();

        for (roleIndex = 0; roleIndex <= (int)RoleColumns.Max - 1; roleIndex++)
        {
          int roleOffset = (int)TemplateColumns.Roles + ((int)RoleColumns.Count * roleIndex);
          object identifier = parentRow[(int)RoleColumns.ID + roleOffset];
          object label = row[(int)RoleColumns.Name + roleOffset];
          object description = row[(int)RoleColumns.Description + roleOffset];
          object type = row[(int)RoleColumns.Type + roleOffset];
          object value = row[(int)RoleColumns.Value + roleOffset];
          object parentRole = parentRow[(int)RoleColumns.ID + roleOffset];

          if (label != null && label.ToString().Trim() != String.Empty)
          {
            string name = label.ToString();

            if (parentRole == null)
            {
              Utility.WriteString("Error Processing Role Qualification: Role \"" + name + "\" at index " + roleIndex + " on template \"" + templateName + "\" not found.\n", "error.log", true);
              continue;
            }

            RoleQualification roleQualification = new RoleQualification();
            roleQualification.identifier = identifier.ToString();

            QMXFName englishUSName = new QMXFName
            {
              lang = "en",
              value = name,
            };

            roleQualification.name = new List<QMXFName>
            { 
              englishUSName 
            };

            if (description != null && description.ToString() != String.Empty)
            {
              Description englishUSDescription = new Description
              {
                lang = "en",
                value = description.ToString(),
              };

              roleQualification.description = new List<Description>
              {
                englishUSDescription
              };
            }

            roleQualification.qualifies = (parentRole ?? "").ToString().Trim();

            if (type != null && type.ToString() != String.Empty)
            {
              var query = from @class in _classes
                          where Convert.ToString(@class[(int)ClassColumns.Label]).Trim() == type.ToString().Trim()
                          select @class;

              if (query.FirstOrDefault() != null)
              {
                object classId = query.FirstOrDefault()[(int)ClassColumns.ID];
                if (classId != null)
                {
                  roleQualification.range = classId.ToString().Trim();
                }
                else
                {
                  Utility.WriteString("\n " + type.ToString() + " Does not have an id in Class List While Processing Role Qualification", "error.log", true);
                }
              }
              else
              {
                Utility.WriteString("\n " + type.ToString() + " Was Not Found in Class List While Processing Role Qualification", "error.log", true);
              }
            }
            else if (value != null && value.ToString() != String.Empty)
            {
              var query = from @class in _classes
                          where Convert.ToString(@class[(int)ClassColumns.Label]) == value.ToString()
                          select @class;

              if (query.FirstOrDefault() != null)
              {
                roleQualification.value = new QMXFValue
                {
                  reference = query.FirstOrDefault()[(int)ClassColumns.ID].ToString().Trim(),
                };
              }
            }
            else
            {
              Utility.WriteString("\nType/Value Was Not Set for Role Qualification \"" + englishUSName.value + "\" on template \"" + templateName + "\".", "error.log", true);
            }
            roleQualifications.Add(roleQualification);
          }
        }

        return roleQualifications;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\nError Processing Role Qualification \n" +
                            "\nRow: " + roleIndex + " \n" + ex.ToString() + "\n", "error.log", true);
        throw ex;
      }
    }

    private static List<ArrayList> MarshallToList(WorksheetPartWrapper part)
    {
      try
      {
        string vals = string.Empty;
        List<ArrayList> table = new List<ArrayList>();
        ArrayList rw;
        foreach (var row in part.Worksheet.SheetData.Rows)
        {
          var value = row.GetCellValue<string>(0);

          rw = new ArrayList();
          for (int i = 0; i <= row.Worksheet.ColumnSets[0].Columns.Count; i++)
          {
            if (row.GetCellValue<string>(i) != null)
            {
              vals = row.GetCellValue<string>(i).Trim();
            }
            else
            {
              vals = string.Empty;
            }
            rw.Add(vals);
          }

          if (rw.Count > 0)
          {
            rw.Add(row.RowIndex.Value.ToString());
            table.Add(rw);
          }
        }
        return table;
      }
      catch (Exception ex)
      {
        Utility.WriteString("\n" + ex.ToString() + "\n", "error.log", true);
        throw ex;
      }
    }
  }
}