using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.iringtools.library
{
  [CollectionDataContract(Name = "contexts", Namespace = "http://www.iringtools.org/library", ItemName = "context")]
  public class Contexts : List<Context>
  {
  }

  /// <summary>
  /// This class represents a node in the Project.xml file with
  /// specific attention paid to the parent/child relationship between projects and applications
  /// </summary>
  [DataContract(Name = "context", Namespace = "http://www.iringtools.org/library")]
  public class Context
  {
    /// <summary>
    /// The name of the project
    /// </summary>
    /// <returns>a string</returns>
    [DataMember(Name = "name", Order = 0)]
    public string Name { get; set; }

    /// <summary>
    /// The description of the project
    /// </summary>
    /// <returns>a string</returns>
    [DataMember(Name = "description", Order = 1, EmitDefaultValue = false)]
    public string Description { get; set; }
  }
}
