using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Linq.Expressions;
using org.iringtools.utility;
using System.Reflection;
using Ciloci.Flee;

namespace org.iringtools.sdk.objects.widgets
{
  public class WidgetProvider
  {
    private Widgets _repository = null;
    private string _fileName = @".\WidgetsStore.xml";

    public WidgetProvider()
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

      _repository = Initialize();
    }

    public Widget CreateWidget(Widget widget)
    {
      var ids = from w in _repository
                select w.Id;

      int newId = ids.Max() + 1;

      widget.Id = newId;

      _repository.Add(widget);
      return widget;
    }

    public Widget ReadWidget(int identifier)
    {
      var widgets = from widget in _repository
                    where widget.Id == identifier
                    select widget;

      return widgets.FirstOrDefault();
    }

    public Widgets SearchWidgets(string query)
    {
      List<Filter> filters = new List<Filter>();

      Filter name = new Filter
      {
        AttributeName = "Name",
        RelationalOperator = "like",
        Value = query,
      };
      filters.Add(name);

      Filter description = new Filter
      {
        Logical = "or",
        AttributeName = "Description",
        RelationalOperator = "like",
        Value = query,
      };
      filters.Add(description);

      Filter material = new Filter
      {
        Logical = "or",
        AttributeName = "Material",
        RelationalOperator = "like",
        Value = query,
      };
      filters.Add(material);
      // for database applications ,the following function has to be called to build filters.
      // Currently it is difficult to implement search functionality on Enum types

      //filters = BuildSearchFiltersForResources("Widgets", query);
      return ReadWidgets(filters);
    }

    public Widgets ReadWidgets(List<Filter> filters)
    {
      Widgets widgets = _repository;

      string linqExpression = filters.ToLinqExpression<Widget>("o");

      if (linqExpression != String.Empty)
      {
        ExpressionContext context = new ExpressionContext();
        context.Variables.DefineVariable("o", typeof(Widget));

        for (int i = 0; i < widgets.Count; i++)
        {
          context.Variables["o"] = widgets[i];
          var expression = context.CompileGeneric<bool>(linqExpression);
          try
          {
            if (!expression.Evaluate())
            {
              widgets.RemoveAt(i--);
            }
          }
          catch { } //If Expression fails to eval for item ignore item.
        }
      }

      return widgets;
    }

    public int UpdateWidgets(Widgets widgets)
    {
      try
      {
        foreach (Widget widget in widgets)
        {
          Widget existingWidget = (from w in _repository
                                   where w.Id == widget.Id
                                   select w).FirstOrDefault();

          if (existingWidget != null)
          {
            _repository.Remove(existingWidget);
            _repository.Add(widget);
          }
          else
          {
            CreateWidget(widget);
          }
        }

        Save();

        return 0;
      }
      catch (Exception ex)
      {
        return 1;
      }
    }

    public int DeleteWidgets(int identifier)
    {
      try
      {
        Widget existingWidget = (from w in _repository
                                 where w.Id == identifier
                                 select w).FirstOrDefault();

        _repository.Remove(existingWidget);

        Save();

        return 0;
      }
      catch (Exception ex)
      {
        return 1;
      }
    }

    public int DeleteWidgets(Filter filter)
    {
      return 0;
    }

    private Widgets Initialize()
    {
      Widgets widgets = new Widgets();

      if (File.Exists(_fileName))
      {
        widgets = Utility.Read<Widgets>(_fileName, true);
      }
      else
      {
        widgets = new Widgets
        {
          new Widget
          {
            Id = 1,
            Name = "Thing1",
            Description = "Sample Object 1",
            Color = Color.Orange,
            Material = "Oak Wood",
            Length = 3.14,
            Height = 4.0,
            Width = 5.25,
            LengthUOM = LengthUOM.inch,
            Weight = 10,
            WeightUOM = WeightUOM.pounds
          },
          new Widget
          {
            Id = 2,
            Name = "Thing2",
            Description = "Sample Object 2",
            Color = Color.Blue,
            Material = "Polyoxymethylene",
            Length = 6.14,
            Height = 10.0,
            Width = 19.25,
            LengthUOM = LengthUOM.milimeter,
            Weight = 15,
            WeightUOM = WeightUOM.kilograms
          },
          new Widget
          {
            Id = 3,
            Name = "Thing3",
            Description = "Sample Object 3",
            Color = Color.Red,
            Material = "Maple Wood",
            Length = 8.14,
            Height = 12.0,
            Width = 25.25,
            LengthUOM = LengthUOM.feet,
            Weight = 150,
            WeightUOM = WeightUOM.pounds
          },
        };

        Utility.Write<Widgets>(widgets, _fileName, true);
      }

      return widgets;
    }

    private void Save()
    {
      Utility.Write<Widgets>(_repository, _fileName, true);
    }
  }
}
