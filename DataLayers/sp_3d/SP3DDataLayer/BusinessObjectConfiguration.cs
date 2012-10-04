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

using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using org.iringtools.library;
using System.Linq;

namespace iringtools.sdk.sp3ddatalayer
{
  [DataContract(Name = "businessObjectConfiguration", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessObjectConfiguration
  {
    public BusinessObjectConfiguration()
    {
      businessObjects = new List<BusinessObject>();
    }

    [DataMember(Order = 0)]
    public List<BusinessObject> businessObjects { get; set; }

    public BusinessObject GetBusinessObject(string name)
    {
      BusinessObject BusinessObject = null;
      BusinessObject = this.businessObjects.FirstOrDefault<BusinessObject>(o => o.objectName.ToLower() == name.ToLower());
      return BusinessObject;
    }

    public BusinessObject GetTableObject(string name)
    {
      BusinessObject BusinessObject = null;
      BusinessObject = this.businessObjects.FirstOrDefault<BusinessObject>(o => (o.objectName).ToLower() == name.ToLower());
      return BusinessObject;
    }
  }

  [DataContract(Name = "businessInterface", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessInterface
  {
    public BusinessInterface()
    {
      businessProperties = new List<BusinessProperty>();
    }

    [DataMember(IsRequired = true, Order = 0)]
    public string interfaceName { get; set; }

    [DataMember(IsRequired = true, Order = 2)]
    public string tableName { get; set; }

    [DataMember(IsRequired = true, Order = 3)]
    public List<BusinessProperty> businessProperties { get; set; }

    public bool deleteProperty(BusinessProperty businessProperty)
    {
      foreach (BusinessProperty property in businessProperties)
      {
        if (businessProperty == property)
        {
          businessProperties.Remove(businessProperty);
          break;
        }
      }
      return true;
    }
  }

  [DataContract(Name = "businessObject", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessObject
  {
    public BusinessObject()
    {
      businessKeyProperties = new List<BusinessKeyProperty>();
      businessInterfaces = new List<BusinessInterface>();
      businessRelationships = new List<BusinessRelationship>();
    }

    [DataMember(IsRequired = true, Order = 0)]
    public string objectName { get; set; }

    [DataMember(IsRequired = true, Order = 1)]
    public List<BusinessKeyProperty> businessKeyProperties { get; set; }

    [DataMember(IsRequired = false, Order = 2, EmitDefaultValue = false)]
    public List<BusinessInterface> businessInterfaces { get; set; }

    [DataMember(IsRequired = false, Order = 3, EmitDefaultValue = false)]
    public List<BusinessRelationship> businessRelationships { get; set; }

    [DataMember(IsRequired = false, Order = 4, EmitDefaultValue = false)]
    public bool isReadOnly { get; set; }

    [DataMember(IsRequired = false, Order = 5, EmitDefaultValue = false)]
    public string description { get; set; }

    [DataMember(IsRequired = false, Order = 6, EmitDefaultValue = false)]
    public DataFilter dataFilter { get; set; }

    public bool isKeyProperty(string propertyName)
    {
      foreach (BusinessKeyProperty keyProperty in businessKeyProperties)
      {
        if (keyProperty.keyPropertyName.ToLower() == propertyName.ToLower())
          return true;
      }

      return false;
    }

    public BusinessProperty getKeyProperty(string keyPropertyName, string interfaceName)
    {
      foreach (BusinessInterface businessInterface in businessInterfaces)
      {
        if (businessInterface.interfaceName.ToLower() == interfaceName.ToLower())
        {
          if (businessInterface.businessProperties.Count > 0)
          {
            foreach (BusinessProperty businessProperty in businessInterface.businessProperties)
            {
              if (businessProperty != null)
                if (businessProperty.propertyName.ToLower() == keyPropertyName.ToLower())
                  return businessProperty;
            }
          }
          else
            return null;
        }
      }

      return null;
    }

    public bool deleteProperty(BusinessProperty dataProperty)
    {
      foreach (BusinessInterface businessInterface in businessInterfaces)
      {
        foreach (BusinessProperty property in businessInterface.businessProperties)
        {
          if (dataProperty == property)
          {
            businessInterface.businessProperties.Remove(dataProperty);
            break;
          }
        }
      }

      foreach (BusinessKeyProperty keyProperty in businessKeyProperties)
      {
        if (keyProperty.keyPropertyName.ToLower() == dataProperty.propertyName.ToLower())
        {
          businessKeyProperties.Remove(keyProperty);
          break;
        }
      }
      return true;
    }

    public bool addKeyProperty(BusinessProperty keyProperty, string interfaceName)
    {
      foreach (BusinessInterface businessInterface in businessInterfaces)
      {
        if (businessInterface.interfaceName.ToLower() == interfaceName.ToLower())
        {
          businessInterface.businessProperties.Add(keyProperty);
        }
        return false;
      }

      this.businessKeyProperties.Add(new BusinessKeyProperty { keyPropertyName = keyProperty.propertyName });
      return true;
    }
  }

  [DataContract(Name = "businessKeyProperty", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessKeyProperty
  {
    [DataMember(IsRequired = true, Order = 0)]
    public string keyPropertyName { get; set; }    
  }

  [DataContract(Name = "businessProperty", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessProperty
  {
    [DataMember(IsRequired = true, Order = 0)]
    public string propertyName { get; set; }

    [DataMember(IsRequired = false, Order = 1, EmitDefaultValue = false)]
    public string dataType { get; set; }

    [DataMember(IsRequired = false, Order = 2, EmitDefaultValue = false)]
    public bool isNullable { get; set; }

    [DataMember(IsRequired = false, Order = 3, EmitDefaultValue = false)]
    public bool isReadOnly { get; set; }

    [DataMember(IsRequired = false, Order = 4, EmitDefaultValue = false)]
    public string description { get; set; }

    [DataMember(IsRequired = false, Order = 5, EmitDefaultValue = false)]
    public string dbColumn { get; set; }
  }

  [DataContract(Name = "businessRelationship", Namespace = "http://www.iringtools.sdk/sp3ddatalayer")]
  public class BusinessRelationship
  {
    public BusinessRelationship()
    {
      this.businessRelatedInterfaces = new List<BusinessInterface>();
    }

    [DataMember(Order = 0, Name = "businessRelatedInterfaces", IsRequired = true)]
    public List<BusinessInterface> businessRelatedInterfaces { get; set; }

    [DataMember(Order = 1, Name = "relatedObjectName", IsRequired = true)]
    public string relatedObjectName { get; set; }

    [DataMember(Order = 2, Name = "relationshipName", IsRequired = true)]
    public string relationshipName { get; set; }
  }

}
