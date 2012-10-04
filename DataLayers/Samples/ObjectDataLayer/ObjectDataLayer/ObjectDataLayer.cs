using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.sdk.objects.widgets;

namespace org.iringtools.sdk.objects
{
  public class ObjectDataLayer : BaseDataLayer
  {
    WidgetProvider _widgetProvider = null;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ObjectDataLayer));

    //NOTE: This is required to deliver settings to constructor.
    //NOTE: Other objects could be requested on an as needed basis.
    [Inject]
    public ObjectDataLayer(AdapterSettings settings)
      : base(settings)
    {
      _settings = settings;

      _widgetProvider = new WidgetProvider();
    }

    public override DataDictionary GetDictionary()
    {
      DataDictionary dataDictionary = new DataDictionary();

      try
      {
        List<DataObject> dataObjects = new List<DataObject>();

        DataObject widget = new DataObject
        {
          objectName = "Widget",
          keyDelimeter = "_",
        };

        List<KeyProperty> keyProperties = new List<KeyProperty>
      {
        new KeyProperty
        {
          keyPropertyName = "Id",
        },
      };

        widget.keyProperties = keyProperties;

        List<DataProperty> dataProperties = new List<DataProperty>
      {
        new DataProperty
        {
          propertyName = "Id",
          keyType = KeyType.unassigned,
          dataLength = 32,
          numberOfDecimals = 0,
          dataType = DataType.Int32,
        },
        new DataProperty
        {
          propertyName = "Name",
          dataLength = 32,
          dataType = DataType.String,
          showOnIndex = true,
        },
        new DataProperty
        {
          propertyName = "Description",
          dataLength = 256,
          dataType = DataType.String,
        },
        new DataProperty
        {
          propertyName = "Length",
          dataLength = 32,
          numberOfDecimals = 2,
          dataType = DataType.Double,
        },
        new DataProperty
        {
          propertyName = "Width",
          dataLength = 32,
          numberOfDecimals = 2,
          dataType = DataType.Double,
        },
        new DataProperty
        {
          propertyName = "Height",
          dataLength = 32,
          numberOfDecimals = 2,
          dataType = DataType.Double,
        },
        new DataProperty
        {
          propertyName = "Weight",
          dataLength = 32,
          numberOfDecimals = 2,
          dataType = DataType.Double,
        },
        new DataProperty
        {
          propertyName = "LengthUOM",
          dataLength = 32,
          dataType = DataType.String,
        },
        new DataProperty
        {
          propertyName = "WeightUOM",
          dataLength = 32,
          dataType = DataType.String,
        },
        new DataProperty
        {
          propertyName = "Material",
          dataLength = 128,
          dataType = DataType.String,
        },
        new DataProperty
        {
          propertyName = "Color",
          dataLength = 32,
          dataType = DataType.String,
        },
      };

        widget.dataProperties = dataProperties;

        dataObjects.Add(widget);

        dataDictionary.dataObjects = dataObjects;
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while getting DataDictionary: {0}", ex.ToString());
        throw new Exception("Error while getting DataDictionary.", ex);
      }

      return dataDictionary;
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      _dataObjects = new List<IDataObject>();

      try
      {
        switch (objectType.ToUpper())
        {
          case "WIDGET":

            foreach (string identifier in identifiers)
            {
              int id = 0;
              Int32.TryParse(identifier, out id);

              Widget widget = _widgetProvider.ReadWidget(id);

              IDataObject dataObject = FormDataObject(widget);

              _dataObjects.Add(dataObject);
            }
            break;

          default:
            throw new Exception("Invalid object type provided");
        }
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while getting list of specified data objects of type [{0}]: {1}", objectType, ex);
        throw new Exception("Error while getting list of specified data objects of type [" + objectType + "].", ex);
      }

      return _dataObjects;
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      List<IDataObject> page = new List<IDataObject>();

      try
      {
        //This is in the base class
        //loads the current object definition into _dataObjectDefinition.
        //needed by FormFilterList
        LoadDataDictionary(objectType);

        List<Filter> filterList = null;
        if (filter != null)
          filterList = FormFilterList(filter);

        _dataObjects = new List<IDataObject>();

        switch (objectType.ToUpper())
        {
          case "WIDGET":

            List<Widget> widgets = _widgetProvider.ReadWidgets(filterList);

            foreach (Widget widget in widgets)
            {
              IDataObject dataObject = FormDataObject(widget);

              _dataObjects.Add(dataObject);
            }
            break;

          default:
            throw new Exception("Invalid object type provided");
        }

        long count = _dataObjects.Count();
        if (pageSize > count)
          pageSize = (int)count;

        page = _dataObjects.GetRange(startIndex, pageSize);
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while getting a filtered page of data objects of type [{0}]: {1}", objectType, ex);
        throw new Exception("Error while getting a filtered page of data objects of type [" + objectType + "].", ex);
      }

      return page;
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      List<string> identifiers = new List<string>();

      try
      {
        //NOTE: pageSize of 0 indicates that all rows should be returned.
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue("Id"));
        }
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while getting a filtered list of identifiers of type [{0}]: {1}", objectType, ex);
        throw new Exception("Error while getting a filtered list of identifiers of type [" + objectType + "].", ex);
      }

      return identifiers;
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      string objectType = String.Empty;

      if (dataObjects == null || dataObjects.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Data object list provided was empty.");
        response.Append(status);
        return response;
      }

      try
      {
        objectType = ((GenericDataObject)dataObjects.FirstOrDefault()).ObjectType;

        switch (objectType.ToUpper())
        {
          case "WIDGET":
            foreach (IDataObject dataObject in dataObjects)
            {
              Status status = new Status();

              Widget widget = FormWidget(dataObject);
              string identifier = widget.Id.ToString();
              status.Identifier = identifier;

              int result = _widgetProvider.UpdateWidgets(new Widgets { widget });

              string message = String.Empty;
              if (result == 0)
              {
                message = String.Format(
                  "Data object [{0}] posted successfully.",
                  identifier
                );
              }
              else
              {
                message = String.Format(
                  "Error while posting data object [{0}].",
                  identifier
                );
              }

              status.Messages.Add(message);

              response.Append(status);
            }
            break;

          default:
            throw new Exception("Invalid object type provided");
        }
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while processing a list of data objects of type [{0}]: {1}", objectType, ex);
        throw new Exception("Error while processing a list of data objects of type [" + objectType + "].", ex);
      }

      return response;
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();

      if (identifiers == null || identifiers.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to delete.");
        response.Append(status);
        return response;
      }

      try
      {
        foreach (string identifier in identifiers)
        {
          Status status = new Status();
          status.Identifier = identifier;

          int id = 0;
          Int32.TryParse(identifier, out id);

          int result = _widgetProvider.DeleteWidgets(id);

          string message = String.Empty;
          if (result == 0)
          {
            message = String.Format(
              "DataObject [{0}] deleted successfully.",
              identifier
            );
          }
          else
          {
            message = String.Format(
              "Error while deleting dataObject [{0}].",
              identifier
            );
          }

          status.Messages.Add(message);


          response.Append(status);
        }

      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while deleting a list of data objects of type [{0}]: {1}", objectType, ex);
        throw new Exception("Error while deleting a list of data objects of type [" + objectType + "].", ex);
      }

      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    #region Private Marshalling Methods
    private IDataObject FormDataObject(Widget widget)
    {
      IDataObject dataObject = new GenericDataObject();

      try
      {
        dataObject.SetPropertyValue("Id", widget.Id);
        dataObject.SetPropertyValue("Name", widget.Name);
        dataObject.SetPropertyValue("Description", widget.Description);
        dataObject.SetPropertyValue("Length", widget.Length);
        dataObject.SetPropertyValue("Width", widget.Width);
        dataObject.SetPropertyValue("Height", widget.Height);
        dataObject.SetPropertyValue("Weight", widget.Weight);
        dataObject.SetPropertyValue("LengthUOM", widget.LengthUOM);
        dataObject.SetPropertyValue("WeightUOM", widget.WeightUOM);
        dataObject.SetPropertyValue("Material", widget.Material);
        dataObject.SetPropertyValue("Color", widget.Color);
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while marshalling a widget into a data object: {1}", ex);
        throw new Exception("Error while marshalling a widget into a data object.", ex);
      }

      return dataObject;
    }

    private Widget FormWidget(IDataObject dataObject)
    {
      Widget widget = new Widget();

      try
      {
        if (dataObject.GetPropertyValue("Id") != null)
        {
          string identifier = dataObject.GetPropertyValue("Id").ToString();
          int id = 0;
          Int32.TryParse(identifier, out id);
          widget.Id = id;
        }

        if (dataObject.GetPropertyValue("Name") != null)
        {
          widget.Name = dataObject.GetPropertyValue("Name").ToString();
        }

        if (dataObject.GetPropertyValue("Description") != null)
        {
          widget.Description = dataObject.GetPropertyValue("Description").ToString();
        }

        if (dataObject.GetPropertyValue("Material") != null)
        {
          widget.Material = dataObject.GetPropertyValue("Material").ToString();
        }

        if (dataObject.GetPropertyValue("Length") != null)
        {
          string lengthValue = dataObject.GetPropertyValue("Length").ToString();
          double length = 0;
          Double.TryParse(lengthValue, out length);
          widget.Length = length;
        }

        if (dataObject.GetPropertyValue("Width") != null)
        {
          string widthValue = dataObject.GetPropertyValue("Width").ToString();
          double width = 0;
          Double.TryParse(widthValue, out width);
          widget.Width = width;
        }

        if (dataObject.GetPropertyValue("Height") != null)
        {
          string heightValue = dataObject.GetPropertyValue("Height").ToString();
          double height = 0;
          Double.TryParse(heightValue, out height);
          widget.Height = height;
        }

        if (dataObject.GetPropertyValue("Weight") != null)
        {
          string weightValue = dataObject.GetPropertyValue("Weight").ToString();
          double weight = 0;
          Double.TryParse(weightValue, out weight);
          widget.Weight = weight;
        }

        if (dataObject.GetPropertyValue("LengthUOM") != null)
        {
          string lengthUOMValue = dataObject.GetPropertyValue("LengthUOM").ToString();
          LengthUOM lengthUOM = LengthUOM.feet;
          Enum.TryParse<LengthUOM>(lengthUOMValue, out lengthUOM);
          widget.LengthUOM = lengthUOM;
        }

        if (dataObject.GetPropertyValue("WeightUOM") != null)
        {
          string weightUOMValue = dataObject.GetPropertyValue("WeightUOM").ToString();
          WeightUOM weightUOM = WeightUOM.grams;
          Enum.TryParse<WeightUOM>(weightUOMValue, out weightUOM);
          widget.WeightUOM = weightUOM;
        }

        if (dataObject.GetPropertyValue("Color") != null)
        {
          string colorValue = dataObject.GetPropertyValue("Color").ToString();
          Color color = Color.Black;
          Enum.TryParse<Color>(colorValue, out color);
          widget.Color = color;
        }
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while marshalling a data object into a widget: {1}", ex);
        throw new Exception("Error while marshalling a data object into a widget.", ex);
      }

      return widget;
    }

    private List<Filter> FormFilterList(DataFilter dataFilter)
    {
      List<Filter> filterList = new List<Filter>();

      try
      {
        foreach (org.iringtools.library.Expression expression in dataFilter.Expressions)
        {
          Filter filter = new Filter();

          string propertyName = expression.PropertyName;
          DataProperty dataProperty = (from dp in _dataObjectDefinition.dataProperties
                                       where dp.propertyName.ToUpper() == propertyName.ToUpper()
                                       select dp).FirstOrDefault();

          bool isString = (dataProperty.dataType == DataType.String || dataProperty.dataType == DataType.Char);

          filter.AttributeName = propertyName;

          if (expression.RelationalOperator == RelationalOperator.StartsWith)
          {
            if (!isString) throw new Exception("StartsWith operator used with non-string property");

            filter.RelationalOperator = "like";
            filter.Value = "\"" + expression.Values.FirstOrDefault() + "\"";
          }
          else if (expression.RelationalOperator == RelationalOperator.EndsWith)
          {
            if (!isString) throw new Exception("EndsWith operator used with non-string property");

            filter.RelationalOperator = "like";
            filter.Value = "\"" + expression.Values.FirstOrDefault() + "\"";
          }
          else if (expression.RelationalOperator == RelationalOperator.Contains)
          {
            if (!isString) throw new Exception("Contains operator used with non-string property");

            filter.RelationalOperator = "like";
            filter.Value = "\"" + expression.Values.FirstOrDefault() + "\"";
          }
          else if (expression.RelationalOperator == RelationalOperator.In)
          {
            filter.RelationalOperator = expression.RelationalOperator.ToString();
            string values = String.Empty;
            int valueIndex = 1;
            int valueCount = expression.Values.Count();
            foreach (string value in expression.Values)
            {
              if (isString)
              {
                if (valueIndex == valueCount)
                  values += "\"" + value + "\"";
                else
                  values += "\"" + value + "\", ";
              }
              else
              {
                if (valueIndex == valueCount)
                  values += value;
                else
                  values += value + ", ";
              }

              valueIndex++;
            }

            filter.Value = values;
          }
          else
          {
            filter.RelationalOperator = expression.RelationalOperator.ToString();
              
            if (isString)
              filter.Value = "\"" + expression.Values.FirstOrDefault() + "\"";
            else
              filter.Value = expression.Values.FirstOrDefault();
          }

          if (expression.LogicalOperator != LogicalOperator.None)
            filter.Logical = expression.LogicalOperator.ToString();

          filterList.Add(filter);
        }
      }
      catch (Exception ex)
      {
        _logger.ErrorFormat("Error while marshalling a data filter into a filter list: {1}", ex);
        throw new Exception("Error while marshalling a data filter into a filter list.", ex);
      }

      return filterList;
    }
    #endregion

  }
}
