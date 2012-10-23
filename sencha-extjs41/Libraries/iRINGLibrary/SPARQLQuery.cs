using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.library
{

    public class SPARQLRole
    {
        public String Role { get; set; }
        public Object Id { get; set; }
        public Object newId { get; set; }
    }

    public class SPARQLTemplate
    {
        public int Idx { get; set; }
        public String Prefix { get; set; }

        public String TemplateName { get; set; }

        public String ClassRole { get; set; }
        public String ClassId { get; set; }

        public List<SPARQLRole> Roles { get; set; }

        public SPARQLTemplate()
        {
            this.Prefix = "t";
            this.Idx = 0;

            this.TemplateName = String.Empty;

            this.ClassRole = String.Empty;
            this.ClassId = String.Empty;

            this.Roles = new List<SPARQLRole>();
        }

        public SPARQLRole Add(SPARQLRole role)
        {
            var items = from query in this.Roles
                        where query.Role == role.Role
                        select query;

            if (items.Count<SPARQLRole>() == 0)
            {
                this.Roles.Add(role);
                return role;
            }
            else
            {
                return items.First<SPARQLRole>();
            }
        }

        public SPARQLRole addRole(string role, object id)
        {
            return this.addRole(role, id, id);
        }

        public SPARQLRole addRole(string role, object id, object newId)
        {
            SPARQLRole roleObj = new SPARQLRole() { Role = role, Id = id, newId = newId };
            return this.Add(roleObj);
        }

        public String getNode()
        {
            return getNode(false);
        }

        public String getNode(Boolean blank)
        {
            if (blank)
            {
                return String.Format("_:{0}{1}", Prefix, Idx);
            }
            else
            {
                return String.Format("?{0}{1}", Prefix, Idx);
            }
        }

        public string getINSERTTEMPORAL_SPARQL()
        {
            StringBuilder sparql = new StringBuilder();

            foreach (SPARQLRole role in this.Roles)
            {
                if (role.Role == "p7tpl:valEndTime")
                {
                    sparql.AppendLine(String.Format("{0} {1} {2} .", this.getNode(), role.Role, role.newId));
                }
            }

            return sparql.ToString();
        }

        public string getINSERT_SPARQL()
        {
            StringBuilder sparql = new StringBuilder();

            sparql.AppendLine(String.Format("{0} a {1} .", this.getNode(true), "owl:Thing"));
            sparql.AppendLine(String.Format("{0} a {1} .", this.getNode(true), TemplateName));
            sparql.AppendLine(String.Format("{0} {1} {2} .", this.getNode(true), ClassRole, ClassId));

            foreach (SPARQLRole role in this.Roles)
            {
                sparql.AppendLine(String.Format("{0} {1} {2} .", this.getNode(true), role.Role, role.newId));
            }

            return sparql.ToString();
        }

        public string getWHERE_SPARQL()
        {
            StringBuilder sparql = new StringBuilder();

            sparql.AppendLine(String.Format("{0} a {1} .", this.getNode(), "owl:Thing"));
            sparql.AppendLine(String.Format("{0} a {1} .", this.getNode(), TemplateName));
            sparql.AppendLine(String.Format("{0} {1} {2} .", this.getNode(), ClassRole, ClassId));

            foreach (SPARQLRole role in this.Roles)
            {
                if (role.Role != "p7tpl:valEndTime")
                {
                    sparql.AppendLine(String.Format("{0} {1} {2} .", this.getNode(), role.Role, role.Id));
                }
                else
                {
                    sparql.Append("OPTIONAL { ");
                    sparql.Append(String.Format("{0} {1} {2}", this.getNode(), role.Role, role.Id));
                    sparql.AppendLine(" }");

                    sparql.Append("FILTER(!bound(");
                    sparql.Append(String.Format("{0}", role.Id));
                    sparql.AppendLine("))");
                }

            }
            return sparql.ToString();
        }

        public string getDELETEWHERE_SPARQL()
        {
            StringBuilder sparql = new StringBuilder();
            sparql.AppendLine(String.Format("?subject ?predicate {0} .", this.Roles[0].Id));
            sparql.AppendLine("FILTER (?predicate != p7tpl:R99011248051)");
            sparql.AppendLine("OPTIONAL { ?subject p7tpl:valEndTime ?endDateTime }");
            sparql.AppendLine("FILTER (!bound(?endDateTime))");
            return sparql.ToString();
        }
    }

    public class SPARQLClassification
      : SPARQLTemplate
    {
        public SPARQLClassification()
        {
            this.Prefix = "c";
            this.Idx = 0;

            this.TemplateName = "p7tpl:R63638239485";

            this.ClassRole = "p7tpl:R55055340393";
            this.ClassId = String.Empty;

        }
    }

    public class SPARQLPrefix
    {
        public SPARQLPrefix()
        {
            this.Label = "";
            this.Uri = "";
            this.objectType = ObjectType.Unknown;
            this.isMappable = false;

        }

        public enum ObjectType
        {
            Class,
            Template,
            Role,
            Unknown
        }

        public string Label { get; set; }
        public string Uri { get; set; }
        public ObjectType objectType { get; set; }
        public bool isMappable { get; set; }

        public string GetPrefix()
        {
            return String.Format("PREFIX {0}: <{1}>", this.Label, this.Uri);
        }

    }

    public class SPARQLQuery
    {
        public SPARQLQuery()
        {
            Prefixes = new List<SPARQLPrefix>();
            Templates = new List<SPARQLTemplate>();
            Variables = new List<String>();
            Sources = new List<String>();
            Type = SPARQLQueryType.SELECT;

            this.Prefixes.Add(new SPARQLPrefix() { Label = @"dm", Uri = @"http://dm.rdlfacade.org/data#", isMappable = false, objectType = SPARQLPrefix.ObjectType.Unknown });
            this.Prefixes.Add(new SPARQLPrefix() { Label = @"rdl", Uri = @"http://rdl.rdlfacade.org/data#", isMappable = false, objectType = SPARQLPrefix.ObjectType.Class });
            this.Prefixes.Add(new SPARQLPrefix() { Label = @"tpl", Uri = @"http://tpl.rdlfacade.org/data#", isMappable = false, objectType = SPARQLPrefix.ObjectType.Template });
            this.Prefixes.Add(new SPARQLPrefix() { Label = @"xsd", Uri = @"http://www.w3.org/2001/XMLSchema#", isMappable = true, objectType = SPARQLPrefix.ObjectType.Unknown });
            this.Prefixes.Add(new SPARQLPrefix() { Label = @"eg", Uri = @"http://www.example.com/data#", isMappable = false, objectType = SPARQLPrefix.ObjectType.Unknown });
            this.Prefixes.Add(new SPARQLPrefix() { Label = @"owl", Uri = @"http://www.w3.org/2002/07/owl#", isMappable = false, objectType = SPARQLPrefix.ObjectType.Unknown });
            this.Prefixes.Add(new SPARQLPrefix() { Label = @"p7tpl", Uri = @"http://tpl.rdlfacade.org/data#", isMappable = false, objectType = SPARQLPrefix.ObjectType.Unknown });
        }

        public SPARQLQuery(SPARQLQueryType type)
            : this()
        {
            Type = type;
        }

        public List<SPARQLPrefix> Prefixes { get; set; }
        public List<SPARQLTemplate> Templates { get; set; }
        public List<String> Variables { get; set; }
        public List<String> Sources { get; set; }
        public SPARQLQueryType Type = SPARQLQueryType.SELECT;

        public String getLITERAL_SPARQL(Object value)
        {
            return this.getLITERAL_SPARQL(value, value.GetType());
        }

        public String getLITERAL_SPARQL(Object value, Type type)
        {
            return this.getLITERAL_SPARQL(value, type.GetType().Name);
        }

        public String getLITERAL_SPARQL(Object value, String type)
        {
            if (String.IsNullOrEmpty(type))
                type = "String";

            if (!type.Contains(':'))
                type = "xsd:" + type;

            string str = string.Empty;
            if (value == null)
                str = "";
            else
                str = value.ToString();

            return String.Format("\"{0}\"^^{1}", str, type);
        }

        public String getPREFIX_URI(String uri)
        {
            if (uri != string.Empty)
            {
                if (uri.Contains('#'))
                {
                    String[] token = uri.Split('#');
                    String url = token[0] + "#";
                    String identifier = token[1];

                    var items = from query in this.Prefixes
                                where query.Uri == url
                                select query;

                    if (items.Count() > 0)
                    {
                        foreach (SPARQLPrefix prefix in items)
                        {
                            return String.Format("{0}:{1}", prefix.Label, identifier);
                        }
                    }
                    else
                    {
                        return "<" + uri + ">";
                    }
                }
                else
                {
                    return uri;
                }
            }
            return String.Empty;
        }

        public Boolean addPrefix(SPARQLPrefix prefix)
        {
            var items = from query in this.Prefixes
                        where query.Label == prefix.Label
                        select query;

            if (items.Count<SPARQLPrefix>() == 0)
            {
                this.Prefixes.Add(prefix);
            }
            return true;
        }

        public Boolean addVariable(String variable)
        {
            if (variable.StartsWith("?"))
            {
                if (!this.Variables.Contains(variable))
                {
                    this.Variables.Add(variable);
                }
            }

            return true;
        }

        public Boolean addSource(String source)
        {
            if (!this.Sources.Contains(source))
            {
                this.Sources.Add(source);
            }

            return true;
        }

        public SPARQLTemplate addTemplate(SPARQLTemplate template)
        {
            //var items = from query in this.Templates
            //            where query.TemplateName == template.TemplateName
            //            select query;

            //if (items.Count<SPARQLTemplate>() == 0)
            //{
            int idx = this.Templates.Count<SPARQLTemplate>();
            template.Idx = ++idx;
            this.Templates.Add(template);
            return template;
            //}
            //else
            //{
            //  return items.First<SPARQLTemplate>();
            //}      
        }

        public SPARQLTemplate addTemplate(String templateName, String classRole, String classId, String role, String id)
        {
            return addTemplate(templateName, classRole, classId, role, id, id);
        }

        public SPARQLTemplate addTemplate(String templateName, String classRole, String classId)
        {
            SPARQLTemplate template = new SPARQLTemplate();

            template.TemplateName = templateName;
            template.ClassRole = classRole;
            template.ClassId = getPREFIX_URI(classId);

            return this.addTemplate(template);
        }

        public SPARQLTemplate addTemplate(String templateName, String classRole, String classId, String role, String id, String newId)
        {
            SPARQLTemplate template = this.addTemplate(templateName, classRole, classId);
            template.addRole(role, getPREFIX_URI(id), getPREFIX_URI(newId));
            return template;
        }

        public SPARQLClassification addClassification(String classId, String id)
        {
            SPARQLClassification classification = new SPARQLClassification();
            classification.ClassId = classId;
            classification.addRole("p7tpl:R99011248051", id);

            this.addTemplate(classification);

            return classification;
        }

        public Boolean Merge(SPARQLQuery query)
        {
            foreach (SPARQLPrefix prefix in query.Prefixes)
                this.addPrefix(prefix);

            foreach (String variable in query.Variables)
                this.addVariable(variable);

            foreach (SPARQLTemplate template in query.Templates)
                this.addTemplate(template);

            return true;
        }

        public string getSPARQL()
        {
            return getSPARQL(Type);
        }

        public string getSPARQL(SPARQLQueryType queryType)
        {
            StringBuilder query = new StringBuilder();

            foreach (SPARQLPrefix prefix in this.Prefixes)
            {
                query.AppendLine(prefix.GetPrefix());
            }

            switch (queryType)
            {
                case SPARQLQueryType.SELECTDISTINCT:
                    query.AppendLine("SELECT DISTINCT");
                    foreach (string variable in this.Variables)
                    {
                        query.AppendLine(variable);
                    }
                    foreach (string source in this.Sources)
                    {
                        query.AppendLine(String.Format("FROM <{0}>", source));
                    }
                    query.AppendLine("WHERE ");
                    query.AppendLine("{");

                    foreach (SPARQLTemplate template in this.Templates)
                    {
                        query.Append(template.getWHERE_SPARQL());
                    }
                    query.AppendLine("}");
                    break;

                case SPARQLQueryType.SELECT:
                    query.AppendLine("SELECT");
                    foreach (string variable in this.Variables)
                    {
                        query.AppendLine(variable);
                    }
                    foreach (string source in this.Sources)
                    {
                        query.AppendLine(String.Format("FROM <{0}>", source));
                    }
                    query.AppendLine("WHERE ");
                    query.AppendLine("{");

                    foreach (SPARQLTemplate template in this.Templates)
                    {
                        query.Append(template.getWHERE_SPARQL());
                    }
                    query.AppendLine("}");
                    break;

                case SPARQLQueryType.INSERTTEMPORAL:
                    query.AppendLine("INSERT {");
                    foreach (SPARQLTemplate template in this.Templates)
                    {
                        query.Append(template.getINSERTTEMPORAL_SPARQL());
                    }
                    query.AppendLine("}");
                    query.AppendLine("WHERE ");
                    query.AppendLine("{");

                    foreach (SPARQLTemplate template in this.Templates)
                    {
                        query.Append(template.getWHERE_SPARQL());
                    }
                    query.AppendLine("}");
                    break;

                case SPARQLQueryType.SELECTFORDELETE:
                    query.AppendLine("SELECT");
                    foreach (string variable in this.Variables)
                    {
                        query.AppendLine(variable);
                    }
                    query.AppendLine("WHERE ");
                    query.AppendLine("{");

                    foreach (SPARQLTemplate template in this.Templates)
                    {
                        query.Append(template.getDELETEWHERE_SPARQL());
                    }
                    query.AppendLine("}");
                    break;

                case SPARQLQueryType.INSERT:
                    query.AppendLine("INSERT {");
                    foreach (SPARQLTemplate template in this.Templates)
                    {
                        query.Append(template.getINSERT_SPARQL());
                    }
                    query.AppendLine("}");
                    return query.ToString();
            }


            return query.ToString();
        }
    }

    public enum SPARQLQueryType
    {
        SELECT,
        SELECTDISTINCT,
        INSERT,
        INSERTTEMPORAL,
        SELECTFORDELETE

    }

}
