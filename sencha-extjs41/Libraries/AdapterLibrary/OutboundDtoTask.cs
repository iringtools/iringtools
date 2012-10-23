using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using org.iringtools.mapping;
using org.iringtools.library;
using org.iringtools.adapter.projection;
using System.Xml.Linq;
using Microsoft.ServiceModel.Web;
using org.iringtools.refdata.federation;

namespace org.iringtools.adapter
{
  public class OutboundDtoTask
  {
    private ManualResetEvent _doneEvent;
    private DtoProjectionEngine _projectionLayer;
    private IDataLayer _dataLayer;
    private GraphMap _graphMap;
    private List<string> _identifiers;
    private DataTransferObjects _dataTransferObjects;

    public OutboundDtoTask(ManualResetEvent doneEvent, DtoProjectionEngine projectionLayer, IDataLayer dataLayer,
      GraphMap graphMap, List<string> identifiers)
    {
      _doneEvent = doneEvent;
      _projectionLayer = projectionLayer;
      _dataLayer = dataLayer;
      _graphMap = graphMap;
      _identifiers = identifiers;
    }

    public void ThreadPoolCallback(object threadContext)
    {
      int threadIndex = (int)threadContext;

      if (_identifiers != null && _identifiers.Count > 0)
      {
        IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, _identifiers);

        if (dataObjects != null)
        {
          XDocument dtoDoc = _projectionLayer.ToXml(_graphMap.name, ref dataObjects);

          if (dtoDoc != null && dtoDoc.Root != null)
          {
            _dataTransferObjects = SerializationExtensions.ToObject<DataTransferObjects>(dtoDoc.Root);
          }
        }
      }

      _doneEvent.Set();
    }

    public DataTransferObjects DataTransferObjects
    {
      get { return _dataTransferObjects; }
    }
  }
}
