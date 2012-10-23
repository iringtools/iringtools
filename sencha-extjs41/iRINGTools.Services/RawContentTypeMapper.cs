using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Channels;

namespace org.iringtools.adapter
{
  public class RawContentTypeMapper : WebContentTypeMapper
  {
    public override WebContentFormat
                 GetMessageFormatForContentType(string contentType)
    {
      return WebContentFormat.Raw;
    }
  }
}