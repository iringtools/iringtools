using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using org.ids_adi.qmxf;
using org.iringtools.refdata.federation;

namespace iRINGTools.Web.Models
{
  public interface IRefDataRepository
  {
    Federation GetFederation();

    RefDataEntities Search(string query);

    RefDataEntities Search(string query, int start, int limit);

    RefDataEntities SearchReset(string query);

    Entity GetClassLabel(string classId);

    Entities GetSuperClasses(string classId, Repository repository);

    Entities GetSubClasses(string classId, Repository repository);

    Entities GetSubClassesCount(string classId);

    Entities GetClassTemplatesCount(string classId);

    Entities GetClassTemplates(string classId);

    Entities GetClassMembers(string classId, Repository repository);

    QMXF GetClasses(string classId, Repository repository);

    QMXF GetTemplate(string id);
  }
}