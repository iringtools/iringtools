using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using org.iringtools.mapping;
using org.iringtools.library;
using org.iringtools.adapter.projection;
using org.iringtools.refdata.federation;

namespace org.iringtools.adapter
{
  public class DataTransferObjectsTask
  {
    private ManualResetEvent _doneEvent;
    private DtoProjectionEngine _projectionLayer;
    private IDataLayer _dataLayer;
    private GraphMap _graphMap;
    private DataTransferObjects _dataTransferObjects;
    private Response _response;

    public DataTransferObjectsTask(ManualResetEvent doneEvent, DtoProjectionEngine projectionLayer, IDataLayer dataLayer,
      GraphMap graphMap, DataTransferObjects dataTransferObjects)
    {
      _doneEvent = doneEvent;
      _projectionLayer = projectionLayer;
      _dataLayer = dataLayer;
      _graphMap = graphMap;
      _dataTransferObjects = dataTransferObjects;
    }

    public void ThreadPoolCallback(object threadContext)
    {
      int threadIndex = (int)threadContext;

      if (_dataTransferObjects != null)
      {
        IList<IDataObject> dataObjects = _projectionLayer.ToDataObjects(_graphMap, ref _dataTransferObjects);

        if (dataObjects != null)
        {
          _response = _dataLayer.Post(dataObjects);
        }
      }

      _doneEvent.Set();
    }

    public Response Response
    {
      get { return _response; }
    }
  }
}
