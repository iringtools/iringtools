using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace org.iringtools.utility
{
  public class Properties
  {
    private IDictionary<string, string> _dictionary = new Dictionary<string, string>();
    
    public void Load(string filePath)
    {
      foreach (string line in File.ReadAllLines(filePath))
      {
        if ((!string.IsNullOrEmpty(line)) &&
            (!line.StartsWith(";")) &&
            (!line.StartsWith("#")) &&
            (!line.StartsWith("'")) &&
            (line.Contains('=')))
        {
          int index = line.IndexOf('=');
          string key = line.Substring(0, index).Trim();
          string value = line.Substring(index + 1).Trim();

          if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
              (value.StartsWith("'") && value.EndsWith("'")))
          {
            value = value.Substring(1, value.Length - 2);
          }
          _dictionary.Add(key, value);
        }
      }
    }

    public string this[string name]
    {
      get
      {
        return _dictionary[name];
      }
      set
      {
        _dictionary[name] = value;
      }
    }

    public int Count
    {
      get { return _dictionary.Count; }
    }

    public ICollection<string> Keys
    {
      get { return _dictionary.Keys; }
    }

    public ICollection<string> Values
    {
      get { return _dictionary.Values; }
    }

    public bool ContainsKey(string key)
    {
      return _dictionary.ContainsKey(key);
    }
  }
}
