using System.Runtime.Serialization;
using System.ComponentModel;

namespace org.iringtools.library
{
	[DataContract(Name = "Field", Namespace = "http://www.iringtools.org/library")]
	public class Field
	{
		[DataMember(Name = "name", IsRequired = true, Order = 0)]
		public string name { get; set; }  // header

    [DataMember(Name = "header", IsRequired = true, Order = 1)]
    public string header { get; set; }

		[DataMember(Name = "dataIndex", IsRequired = true, Order = 2)]
		public string dataIndex { get; set; }			

		[DataMember(Name = "sortable", IsRequired = true, Order = 3)]
		public bool sortable = true;

    [DataMember(Name = "type", IsRequired = true, Order = 4)]
    public string type { get; set; }  // data type	

    [DataMember(Name = "filterable", IsRequired = true, Order = 5)]
    public bool filterable = true;

    //[DataMember(Name = "keytype", IsRequired = true, Order = 5)]
    //public string keytype = null;

    public void setSortable(bool sortable)
    {
      this.sortable = sortable;
    }

    public bool getSortable()
    {
      return sortable;
    } 

    //public void setKeytype(string keytype)
    //{
    //  this.keytype = keytype;
    //}

    //public string getKeytype()
    //{
    //  return keytype;
    //}

    //public void setFixed(bool fix)
    //{
    //  this.fix = fix;
    //}
  
    //public bool getFixed()
    //{
    //  return fix;
    //}

    //public void setFilterable(bool filterable)
    //{
    //  this.filterable = filterable;
    //}

    //public bool getFilterable()
    //{
    //  return filterable;
    //}

		
	}
}