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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Linq;
using org.iringtools.library;
using org.iringtools.dxfr.manifest;
using org.iringtools.mapping;

namespace org.iringtools.adapter
{
  [DataContract(Namespace = "http://www.iringtools.org/dxfr/dto", Name = "dataTransferObjects")]
  public class DataTransferObjects
  {
    public DataTransferObjects()
    {
      DataTransferObjectList = new List<DataTransferObject>();
    }

    [DataMember(Name = "scopeName", Order = 0, EmitDefaultValue = false)]
    public string ScopeName { get; set; }

    [DataMember(Name = "appName", Order = 1, EmitDefaultValue = false)]
    public string AppName { get; set; }

    [DataMember(Name = "dataTransferObjectList", Order = 2)]
    public List<DataTransferObject> DataTransferObjectList { get; set; }

    [DataMember(Name = "version", Order = 3, EmitDefaultValue = false)]
    public string Version { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/dto", Name = "dataTransferObject")]
  public class DataTransferObject
  {
    public DataTransferObject()
    {
      classObjects = new List<ClassObject>();
    }

    [DataMember(Order = 0)]
    public string identifier { get; set; }

    [DataMember(Order = 1)]
    public List<ClassObject> classObjects { get; set; }

    [DataMember(Order = 2, EmitDefaultValue = false)]
    public TransferType transferType { get; set; }

    public ClassObject GetClassObject(string classId)
    {
      foreach (ClassObject classObject in classObjects)
      {
        if (classObject.classId == classId)
        {
          return classObject;
        }
      }

      return null;
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/dto", Name = "classObject")]
  public class ClassObject
  {
    public ClassObject()
    {
      templateObjects = new List<TemplateObject>();
    }

    [DataMember(Order = 0)]
    public string classId { get; set; }

    [DataMember(Order = 1)]
    public string name { get; set; }

    [DataMember(Order = 2)]
    public string identifier { get; set; }

    [DataMember(Order = 3)]
    public List<TemplateObject> templateObjects { get; set; }

    [DataMember(Order = 4, EmitDefaultValue = false)]
    public TransferType transferType { get; set; }

    public TemplateObject GetTemplateObject(TemplateMap templateMap)
    {
      TemplateObject theTemplateObject = null;

      if (templateObjects != null)
      {
        foreach (TemplateObject templateObject in templateObjects)
        {
          if (templateObject.templateId == templateMap.id)
          {
            int roleIdsMatchedCount = 0;

            foreach (RoleMap roleMap in templateMap.roleMaps)
            {
              foreach (RoleObject roleObject in templateObject.roleObjects)
              {
                if (roleObject.roleId == roleMap.id)
                {
                  if (roleMap.type == RoleType.Reference && roleMap.classMap == null && roleMap.value == roleObject.value)
                    return templateObject;

                  roleIdsMatchedCount++;
                  break;
                }
              }
            }

            if (roleIdsMatchedCount == templateMap.roleMaps.Count)
            {
              theTemplateObject = templateObject;
            }
          }
        }
      }

      return theTemplateObject;
    }
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/dto", Name = "templateObject")]
  public class TemplateObject
  {
    public TemplateObject()
    {
      roleObjects = new List<RoleObject>();
    }

    [DataMember(Order = 0)]
    public string templateId { get; set; }

    [DataMember(Order = 1)]
    public string name { get; set; }

    [DataMember(Order = 2)]
    public List<RoleObject> roleObjects { get; set; }

    [DataMember(Order = 3, EmitDefaultValue = false)]
    public TransferType transferType { get; set; }
  }

  [DataContract(Namespace = "http://www.iringtools.org/dxfr/dto", Name = "roleObject")]
  public class RoleObject
  {
    [DataMember(Order = 0)]
    public RoleType type { get; set; }

    [DataMember(Order = 1)]
    public string roleId { get; set; }

    [DataMember(Order = 2)]
    public string name { get; set; }

    [DataMember(Order = 3, EmitDefaultValue = false)]
    public string dataType { get; set; }

    [DataMember(Order = 4, EmitDefaultValue = false)]
    public string oldValue { get; set; }

    [DataMember(Order = 5, EmitDefaultValue = false)]
    public string value { get; set; }

    [DataMember(Order = 6, EmitDefaultValue = false)]
    public string relatedClassId { get; set; }

    [DataMember(Order = 7, EmitDefaultValue = false)]
    public string relatedClassName { get; set; }

    [DataMember(Order = 8, EmitDefaultValue = false)]
    public bool hasValueMap { get; set; }
    
    [DataMember(Order = 9, EmitDefaultValue = false)]
    public RoleValues values { get; set; }

    [DataMember(Order = 10, EmitDefaultValue = false)]
    public RoleValues oldValues { get; set; }
  }

  [DataContract]
  public enum TransferType
  {
    [EnumMember]
    Sync,
    [EnumMember]
    Add,
    [EnumMember]
    Change,
    [EnumMember]
    Delete,
  }

  [CollectionDataContract(Name = "values", Namespace = "http://www.iringtools.org/dxfr/dto", ItemName = "value")]
  public class RoleValues : List<string> {}
}
