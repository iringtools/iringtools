using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using log4net;
using org.iringtools.mapping;
using org.iringtools.library;
using org.iringtools.adapter.projection;

namespace org.iringtools.adapter
{
  public class DataTransferIndicesTask
  {
    private ManualResetEvent _doneEvent;
    private DtoProjectionEngine _projectionLayer;
    private IDataLayer _dataLayer;
    private GraphMap _graphMap;
    private DataFilter _filter;
    private int _pageSize;
    private int _startIndex;
    private DataTransferIndices _dataTransferIndices;

    public DataTransferIndicesTask(ManualResetEvent doneEvent, DtoProjectionEngine projectionLayer, IDataLayer dataLayer, 
      GraphMap graphMap, DataFilter filter, int pageSize, int startIndex)
    {
      _doneEvent = doneEvent;
      _projectionLayer = projectionLayer;
      _dataLayer = dataLayer;
      _graphMap = graphMap;
      _filter = filter;
      _pageSize = pageSize;
      _startIndex = startIndex;
    }

    public void ThreadPoolCallback(object threadContext)
    {
      int threadIndex = (int)threadContext;
      IList<IDataObject> dataObjects = _dataLayer.Get(_graphMap.dataObjectName, _filter, _pageSize, _startIndex);

      if (dataObjects != null)
      {
        _dataTransferIndices = _projectionLayer.GetDataTransferIndices(_graphMap, dataObjects, string.Empty);
      }

      _doneEvent.Set();
    }

    public DataTransferIndices DataTransferIndices
    {
      get { return _dataTransferIndices; }
    }
  }
}
