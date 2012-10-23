//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//     Runtime Version:2.0.50727.3074
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Collections.Generic;
using Iesi.Collections.Generic;
using org.iringtools.library;

namespace org.iringtools.adapter.datalayer.proj_12345_000.ABC
{
  public class EQUIPMENT : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TAG
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String INTERNAL_TAG { get; set; }
    public virtual String ID { get; set; }
    public virtual String AREA { get; set; }
    public virtual String TRAINNUMBER { get; set; }
    public virtual String EQTYPE { get; set; }
    public virtual String EQPPREFIX { get; set; }
    public virtual String EQSEQNO { get; set; }
    public virtual String EQPSUFF { get; set; }
    public virtual String EQUIPDESC1 { get; set; }
    public virtual String EQUIPDESC2 { get; set; }
    public virtual String CONSTTYPE { get; set; }
    public virtual String EWP { get; set; }
    public virtual String USER1 { get; set; }
    public virtual String USER2 { get; set; }
    public virtual String USER3 { get; set; }
    public virtual String TAGSTATUS { get; set; }
    public virtual String COMMODITY { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "TAG": return TAG;
        case "INTERNAL_TAG": return INTERNAL_TAG;
        case "ID": return ID;
        case "AREA": return AREA;
        case "TRAINNUMBER": return TRAINNUMBER;
        case "EQTYPE": return EQTYPE;
        case "EQPPREFIX": return EQPPREFIX;
        case "EQSEQNO": return EQSEQNO;
        case "EQPSUFF": return EQPSUFF;
        case "EQUIPDESC1": return EQUIPDESC1;
        case "EQUIPDESC2": return EQUIPDESC2;
        case "CONSTTYPE": return CONSTTYPE;
        case "EWP": return EWP;
        case "USER1": return USER1;
        case "USER2": return USER2;
        case "USER3": return USER3;
        case "TAGSTATUS": return TAGSTATUS;
        case "COMMODITY": return COMMODITY;
        default: throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "Id":
          Id = Convert.ToString(value);
          break;
        case "TAG":
          TAG = Convert.ToString(value);
          break;
        case "INTERNAL_TAG":
          INTERNAL_TAG = Convert.ToString(value);
          break;
        case "ID":
          ID = Convert.ToString(value);
          break;
        case "AREA":
          AREA = Convert.ToString(value);
          break;
        case "TRAINNUMBER":
          TRAINNUMBER = Convert.ToString(value);
          break;
        case "EQTYPE":
          EQTYPE = Convert.ToString(value);
          break;
        case "EQPPREFIX":
          EQPPREFIX = Convert.ToString(value);
          break;
        case "EQSEQNO":
          EQSEQNO = Convert.ToString(value);
          break;
        case "EQPSUFF":
          EQPSUFF = Convert.ToString(value);
          break;
        case "EQUIPDESC1":
          EQUIPDESC1 = Convert.ToString(value);
          break;
        case "EQUIPDESC2":
          EQUIPDESC2 = Convert.ToString(value);
          break;
        case "CONSTTYPE":
          CONSTTYPE = Convert.ToString(value);
          break;
        case "EWP":
          EWP = Convert.ToString(value);
          break;
        case "USER1":
          USER1 = Convert.ToString(value);
          break;
        case "USER2":
          USER2 = Convert.ToString(value);
          break;
        case "USER3":
          USER3 = Convert.ToString(value);
          break;
        case "TAGSTATUS":
          TAGSTATUS = Convert.ToString(value);
          break;
        case "COMMODITY":
          COMMODITY = Convert.ToString(value);
          break;
        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      switch (relatedObjectType)
      {
        default:
          throw new Exception("Related object [" + relatedObjectType + "] does not exist.");
      }
    }
  }
}

namespace org.iringtools.adapter.datalayer.proj_12345_000.ABC
{
  public class INSTRUMENTS : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TAG
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String KEYTAG { get; set; }
    public virtual String TAG_NO { get; set; }
    public virtual String TAG_CODE { get; set; }
    public virtual String ASSOC_EQ { get; set; }
    public virtual String IAREA { get; set; }
    public virtual String ITRAIN { get; set; }
    public virtual String ITYP { get; set; }
    public virtual String INUM { get; set; }
    public virtual String ISUFFIX { get; set; }
    public virtual String MODIFIER1 { get; set; }
    public virtual String MODIFIER2 { get; set; }
    public virtual String MODIFIER3 { get; set; }
    public virtual String MODIFIER4 { get; set; }
    public virtual String STD_DETAIL { get; set; }
    public virtual String DESCRIPT { get; set; }
    public virtual String TAG_TYPE { get; set; }
    public virtual String CONST_TYPE { get; set; }
    public virtual String COMP_ID { get; set; }
    public virtual String PROJ_STAT { get; set; }
    public virtual String PID_NO { get; set; }
    public virtual String LINE_NO { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "KEYTAG": return KEYTAG;
        case "TAG": return TAG;
        case "TAG_NO": return TAG_NO;
        case "TAG_CODE": return TAG_CODE;
        case "ASSOC_EQ": return ASSOC_EQ;
        case "IAREA": return IAREA;
        case "ITRAIN": return ITRAIN;
        case "ITYP": return ITYP;
        case "INUM": return INUM;
        case "ISUFFIX": return ISUFFIX;
        case "MODIFIER1": return MODIFIER1;
        case "MODIFIER2": return MODIFIER2;
        case "MODIFIER3": return MODIFIER3;
        case "MODIFIER4": return MODIFIER4;
        case "STD_DETAIL": return STD_DETAIL;
        case "DESCRIPT": return DESCRIPT;
        case "TAG_TYPE": return TAG_TYPE;
        case "CONST_TYPE": return CONST_TYPE;
        case "COMP_ID": return COMP_ID;
        case "PROJ_STAT": return PROJ_STAT;
        case "PID_NO": return PID_NO;
        case "LINE_NO": return LINE_NO;
        default: throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "Id":
          Id = Convert.ToString(value);
          break;
        case "KEYTAG":
          KEYTAG = Convert.ToString(value);
          break;
        case "TAG":
          TAG = Convert.ToString(value);
          break;
        case "TAG_NO":
          TAG_NO = Convert.ToString(value);
          break;
        case "TAG_CODE":
          TAG_CODE = Convert.ToString(value);
          break;
        case "ASSOC_EQ":
          ASSOC_EQ = Convert.ToString(value);
          break;
        case "IAREA":
          IAREA = Convert.ToString(value);
          break;
        case "ITRAIN":
          ITRAIN = Convert.ToString(value);
          break;
        case "ITYP":
          ITYP = Convert.ToString(value);
          break;
        case "INUM":
          INUM = Convert.ToString(value);
          break;
        case "ISUFFIX":
          ISUFFIX = Convert.ToString(value);
          break;
        case "MODIFIER1":
          MODIFIER1 = Convert.ToString(value);
          break;
        case "MODIFIER2":
          MODIFIER2 = Convert.ToString(value);
          break;
        case "MODIFIER3":
          MODIFIER3 = Convert.ToString(value);
          break;
        case "MODIFIER4":
          MODIFIER4 = Convert.ToString(value);
          break;
        case "STD_DETAIL":
          STD_DETAIL = Convert.ToString(value);
          break;
        case "DESCRIPT":
          DESCRIPT = Convert.ToString(value);
          break;
        case "TAG_TYPE":
          TAG_TYPE = Convert.ToString(value);
          break;
        case "CONST_TYPE":
          CONST_TYPE = Convert.ToString(value);
          break;
        case "COMP_ID":
          COMP_ID = Convert.ToString(value);
          break;
        case "PROJ_STAT":
          PROJ_STAT = Convert.ToString(value);
          break;
        case "PID_NO":
          PID_NO = Convert.ToString(value);
          break;
        case "LINE_NO":
          LINE_NO = Convert.ToString(value);
          break;
        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      switch (relatedObjectType)
      {
        default:
          throw new Exception("Related object [" + relatedObjectType + "] does not exist.");
      }
    }
  }
}

namespace org.iringtools.adapter.datalayer.proj_12345_000.ABC
{
  public class LINES : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TAG
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String ID { get; set; }
    public virtual String AREA { get; set; }
    public virtual String TRAINNUMBER { get; set; }
    public virtual String SPEC { get; set; }
    public virtual String SYSTEM { get; set; }
    public virtual String LINENO { get; set; }
    public virtual Single? NOMDIAMETER { get; set; }
    public virtual String INSULATIONTYPE { get; set; }
    public virtual String HTRACED { get; set; }
    public virtual String CONSTTYPE { get; set; }
    public virtual String DESPRESSURE { get; set; }
    public virtual String TESTPRESSURE { get; set; }
    public virtual String PWHT { get; set; }
    public virtual String TESTMEDIA { get; set; }
    public virtual String MATLTYPE { get; set; }
    public virtual String NDT { get; set; }
    public virtual String NDE { get; set; }
    public virtual String PIPECLASS { get; set; }
    public virtual String PIDNUMBER { get; set; }
    public virtual String DESTEMPERATURE { get; set; }
    public virtual String PAINTSYSTEM { get; set; }
    public virtual String DESIGNCODE { get; set; }
    public virtual String COLOURCODE { get; set; }
    public virtual String EWP { get; set; }
    public virtual String USER1 { get; set; }
    public virtual String TAGSTATUS { get; set; }
    public virtual String FULLLINE { get; set; }
    public virtual String UOM_NOMDIAMETER { get; set; }
    public virtual String UOM_DESPRESSURE { get; set; }
    public virtual String UOM_DESTEMPERATURE { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "TAG": return TAG;
        case "ID": return ID;
        case "AREA": return AREA;
        case "TRAINNUMBER": return TRAINNUMBER;
        case "SPEC": return SPEC;
        case "SYSTEM": return SYSTEM;
        case "LINENO": return LINENO;
        case "NOMDIAMETER": return NOMDIAMETER;
        case "INSULATIONTYPE": return INSULATIONTYPE;
        case "HTRACED": return HTRACED;
        case "CONSTTYPE": return CONSTTYPE;
        case "DESPRESSURE": return DESPRESSURE;
        case "TESTPRESSURE": return TESTPRESSURE;
        case "PWHT": return PWHT;
        case "TESTMEDIA": return TESTMEDIA;
        case "MATLTYPE": return MATLTYPE;
        case "NDT": return NDT;
        case "NDE": return NDE;
        case "PIPECLASS": return PIPECLASS;
        case "PIDNUMBER": return PIDNUMBER;
        case "DESTEMPERATURE": return DESTEMPERATURE;
        case "PAINTSYSTEM": return PAINTSYSTEM;
        case "DESIGNCODE": return DESIGNCODE;
        case "COLOURCODE": return COLOURCODE;
        case "EWP": return EWP;
        case "USER1": return USER1;
        case "TAGSTATUS": return TAGSTATUS;
        case "FULLLINE": return FULLLINE;
        case "UOM_NOMDIAMETER": return UOM_NOMDIAMETER;
        case "UOM_DESPRESSURE": return UOM_DESPRESSURE;
        case "UOM_DESTEMPERATURE": return UOM_DESTEMPERATURE;
        default: throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "Id":
          Id = Convert.ToString(value);
          break;
        case "TAG":
          TAG = Convert.ToString(value);
          break;
        case "ID":
          ID = Convert.ToString(value);
          break;
        case "AREA":
          AREA = Convert.ToString(value);
          break;
        case "TRAINNUMBER":
          TRAINNUMBER = Convert.ToString(value);
          break;
        case "SPEC":
          SPEC = Convert.ToString(value);
          break;
        case "SYSTEM":
          SYSTEM = Convert.ToString(value);
          break;
        case "LINENO":
          LINENO = Convert.ToString(value);
          break;
        case "NOMDIAMETER":
          NOMDIAMETER = Single.Parse((String)value, NumberStyles.Any);
          break;
        case "INSULATIONTYPE":
          INSULATIONTYPE = Convert.ToString(value);
          break;
        case "HTRACED":
          HTRACED = Convert.ToString(value);
          break;
        case "CONSTTYPE":
          CONSTTYPE = Convert.ToString(value);
          break;
        case "DESPRESSURE":
          DESPRESSURE = Convert.ToString(value);
          break;
        case "TESTPRESSURE":
          TESTPRESSURE = Convert.ToString(value);
          break;
        case "PWHT":
          PWHT = Convert.ToString(value);
          break;
        case "TESTMEDIA":
          TESTMEDIA = Convert.ToString(value);
          break;
        case "MATLTYPE":
          MATLTYPE = Convert.ToString(value);
          break;
        case "NDT":
          NDT = Convert.ToString(value);
          break;
        case "NDE":
          NDE = Convert.ToString(value);
          break;
        case "PIPECLASS":
          PIPECLASS = Convert.ToString(value);
          break;
        case "PIDNUMBER":
          PIDNUMBER = Convert.ToString(value);
          break;
        case "DESTEMPERATURE":
          DESTEMPERATURE = Convert.ToString(value);
          break;
        case "PAINTSYSTEM":
          PAINTSYSTEM = Convert.ToString(value);
          break;
        case "DESIGNCODE":
          DESIGNCODE = Convert.ToString(value);
          break;
        case "COLOURCODE":
          COLOURCODE = Convert.ToString(value);
          break;
        case "EWP":
          EWP = Convert.ToString(value);
          break;
        case "USER1":
          USER1 = Convert.ToString(value);
          break;
        case "TAGSTATUS":
          TAGSTATUS = Convert.ToString(value);
          break;
        case "FULLLINE":
          FULLLINE = Convert.ToString(value);
          break;
        case "UOM_NOMDIAMETER":
          UOM_NOMDIAMETER = Convert.ToString(value);
          break;
        case "UOM_DESPRESSURE":
          UOM_DESPRESSURE = Convert.ToString(value);
          break;
        case "UOM_DESTEMPERATURE":
          UOM_DESTEMPERATURE = Convert.ToString(value);
          break;
        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      switch (relatedObjectType)
      {
        default:
          throw new Exception("Related object [" + relatedObjectType + "] does not exist.");
      }
    }
  }
}

namespace org.iringtools.adapter.datalayer.proj_12345_000.ABC
{
  public class VALVES : IDataObject
  {
    public virtual String Id { get; set; }
    public virtual String TAG_NO
    {
      get { return Id; }
      set { Id = value; }
    }
    public virtual String KEYTAG { get; set; }
    public virtual String VAREA { get; set; }
    public virtual String VTYP { get; set; }
    public virtual String VTRAIN { get; set; }
    public virtual String VNUM { get; set; }
    public virtual String VSUFFIX { get; set; }
    public virtual String TAG_TYPE { get; set; }
    public virtual String CONST_TYPE { get; set; }
    public virtual String COMP_ID { get; set; }
    public virtual String VSIZE { get; set; }
    public virtual String UOM_VSIZE { get; set; }
    public virtual String VSPEC_TYPE { get; set; }
    public virtual String VSPEC_NUM { get; set; }
    public virtual String VPRESRATE { get; set; }
    public virtual String VCONDITION { get; set; }
    public virtual String PID_NO { get; set; }
    public virtual String PROJ_STAT { get; set; }
    
    public virtual object GetPropertyValue(string propertyName)
    {
      switch (propertyName)
      {
        case "Id": return Id;
        case "KEYTAG": return KEYTAG;
        case "TAG_NO": return TAG_NO;
        case "VAREA": return VAREA;
        case "VTYP": return VTYP;
        case "VTRAIN": return VTRAIN;
        case "VNUM": return VNUM;
        case "VSUFFIX": return VSUFFIX;
        case "TAG_TYPE": return TAG_TYPE;
        case "CONST_TYPE": return CONST_TYPE;
        case "COMP_ID": return COMP_ID;
        case "VSIZE": return VSIZE;
        case "UOM_VSIZE": return UOM_VSIZE;
        case "VSPEC_TYPE": return VSPEC_TYPE;
        case "VSPEC_NUM": return VSPEC_NUM;
        case "VPRESRATE": return VPRESRATE;
        case "VCONDITION": return VCONDITION;
        case "PID_NO": return PID_NO;
        case "PROJ_STAT": return PROJ_STAT;
        default: throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual void SetPropertyValue(string propertyName, object value)
    {
      switch (propertyName)
      {
        case "Id":
          Id = Convert.ToString(value);
          break;
        case "KEYTAG":
          KEYTAG = Convert.ToString(value);
          break;
        case "TAG_NO":
          TAG_NO = Convert.ToString(value);
          break;
        case "VAREA":
          VAREA = Convert.ToString(value);
          break;
        case "VTYP":
          VTYP = Convert.ToString(value);
          break;
        case "VTRAIN":
          VTRAIN = Convert.ToString(value);
          break;
        case "VNUM":
          VNUM = Convert.ToString(value);
          break;
        case "VSUFFIX":
          VSUFFIX = Convert.ToString(value);
          break;
        case "TAG_TYPE":
          TAG_TYPE = Convert.ToString(value);
          break;
        case "CONST_TYPE":
          CONST_TYPE = Convert.ToString(value);
          break;
        case "COMP_ID":
          COMP_ID = Convert.ToString(value);
          break;
        case "VSIZE":
          VSIZE = Convert.ToString(value);
          break;
        case "UOM_VSIZE":
          UOM_VSIZE = Convert.ToString(value);
          break;
        case "VSPEC_TYPE":
          VSPEC_TYPE = Convert.ToString(value);
          break;
        case "VSPEC_NUM":
          VSPEC_NUM = Convert.ToString(value);
          break;
        case "VPRESRATE":
          VPRESRATE = Convert.ToString(value);
          break;
        case "VCONDITION":
          VCONDITION = Convert.ToString(value);
          break;
        case "PID_NO":
          PID_NO = Convert.ToString(value);
          break;
        case "PROJ_STAT":
          PROJ_STAT = Convert.ToString(value);
          break;
        default:
          throw new Exception("Property [" + propertyName + "] does not exist.");
      }
    }
    
    public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      switch (relatedObjectType)
      {
        default:
          throw new Exception("Related object [" + relatedObjectType + "] does not exist.");
      }
    }
  }
}
