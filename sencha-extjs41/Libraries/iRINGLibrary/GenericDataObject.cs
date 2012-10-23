using System;
using System.Collections.Generic;
using org.iringtools.library;

namespace org.iringtools.library
{
  public class GenericDataObject : IDataObject
  {
    protected IDictionary<string, object> _dictionary = null;

    public GenericDataObject()
    {
      _dictionary = new Dictionary<string, object>();
    }

    public GenericDataObject(IDictionary<string, object> dict)
    {
      _dictionary = dict;
    }

    public IDictionary<string, object> Dictionary
    {
      get
      {
        return _dictionary;
      }
    }

    public object GetPropertyValue(string propertyName)
    {
      if (_dictionary.ContainsKey(propertyName) && _dictionary[propertyName] != null)
        return _dictionary[propertyName];

      return null;
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      _dictionary[propertyName] = value;
    }

    public string ObjectType { get; set; }
  }
}