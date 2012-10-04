using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace org.iringtools.sdk.objects.widgets
{
  //This namesapce is different for example purposes...
  [CollectionDataContract(Name = "widgets", Namespace = "http://www.bechtel.com/ipix/api/widgets", ItemName = "widget")]
  public class Widgets : List<Widget>
  {
    public Widgets()
    { }

    public Widgets(IList<Widget> list)
    {
      foreach (Widget widget in list)
      {
        Widget item = new Widget
        {
          Id = widget.Id,
          Name = widget.Name,
        };

        Add(item);
      }
    }
  }

  [DataContract(Name = "widget", Namespace = "http://www.bechtel.com/ipix/api/widgets")]
  public class Widget
  {
    [DataMember(Name = "id", EmitDefaultValue = false)]
    public int Id { get; set; }

    [DataMember(Name = "name", EmitDefaultValue = false)]
    public string Name { get; set; }

    [DataMember(Name = "description", EmitDefaultValue = false)]
    public string Description { get; set; }

    [DataMember(Name = "length", EmitDefaultValue = false)]
    public double Length { get; set; }

    [DataMember(Name = "width", EmitDefaultValue = false)]
    public double Width { get; set; }

    [DataMember(Name = "height", EmitDefaultValue = false)]
    public double Height { get; set; }

    [DataMember(Name = "weight", EmitDefaultValue = false)]
    public double Weight { get; set; }

    [DataMember(Name = "length_uom", EmitDefaultValue = false)]
    public LengthUOM LengthUOM { get; set; }

    [DataMember(Name = "weight_uom", EmitDefaultValue = false)]
    public WeightUOM WeightUOM { get; set; }

    [DataMember(Name = "material", EmitDefaultValue = false)]
    public string Material { get; set; }

    [DataMember(Name = "color", EmitDefaultValue = false)]
    public Color Color { get; set; }
  }

  public enum LengthUOM
  {
    meter,
    inch,
    feet,
    milimeter,
  }

  public enum WeightUOM
  {
    pounds,
    grams,
    tons,
    kilograms,
    metricTons,
  }

  public enum Color
  {
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Violet,
    Black,
    White,
    Gray,
  }
}
