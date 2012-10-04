Imports System.Collections.Generic
Imports org.iringtools.library


Namespace com.example
    Public Class Equipment
        Implements IDataObject

        Public Overridable Property SP_ID() As [String]
            Get
                Return m_SP_ID
            End Get
            Set(value As [String])
                m_SP_ID = value
            End Set
        End Property
        Private m_SP_ID As [String]
        Public Overridable Property ADAPTER_PARENTTAG() As [String]
            Get
                Return m_ADAPTER_PARENTTAG
            End Get
            Set(value As [String])
                m_ADAPTER_PARENTTAG = value
            End Set
        End Property
        Private m_ADAPTER_PARENTTAG As [String]
        Public Overridable Property DRAWING_DATECREATED() As DateTime
            Get
                Return m_DRAWING_DATECREATED
            End Get
            Set(value As DateTime)
                m_DRAWING_DATECREATED = value
            End Set
        End Property
        Private m_DRAWING_DATECREATED As DateTime
        Public Overridable Property DRAWING_DESCRIPTION() As [String]
            Get
                Return m_DRAWING_DESCRIPTION
            End Get
            Set(value As [String])
                m_DRAWING_DESCRIPTION = value
            End Set
        End Property
        Private m_DRAWING_DESCRIPTION As [String]
        Public Overridable Property DRAWING_DOCUMENTCATEGORY() As [String]
            Get
                Return m_DRAWING_DOCUMENTCATEGORY
            End Get
            Set(value As [String])
                m_DRAWING_DOCUMENTCATEGORY = value
            End Set
        End Property
        Private m_DRAWING_DOCUMENTCATEGORY As [String]
        Public Overridable Property DRAWING_DOCUMENTTYPE() As [String]
            Get
                Return m_DRAWING_DOCUMENTTYPE
            End Get
            Set(value As [String])
                m_DRAWING_DOCUMENTTYPE = value
            End Set
        End Property
        Private m_DRAWING_DOCUMENTTYPE As [String]
        Public Overridable Property DRAWING_DRAWINGNUMBER() As [String]
            Get
                Return m_DRAWING_DRAWINGNUMBER
            End Get
            Set(value As [String])
                m_DRAWING_DRAWINGNUMBER = value
            End Set
        End Property
        Private m_DRAWING_DRAWINGNUMBER As [String]
        Public Overridable Property DRAWING_ITEMSTATUS() As [String]
            Get
                Return m_DRAWING_ITEMSTATUS
            End Get
            Set(value As [String])
                m_DRAWING_ITEMSTATUS = value
            End Set
        End Property
        Private m_DRAWING_ITEMSTATUS As [String]
        Public Overridable Property DRAWING_NAME() As [String]
            Get
                Return m_DRAWING_NAME
            End Get
            Set(value As [String])
                m_DRAWING_NAME = value
            End Set
        End Property
        Private m_DRAWING_NAME As [String]
        Public Overridable Property DRAWING_PATH() As [String]
            Get
                Return m_DRAWING_PATH
            End Get
            Set(value As [String])
                m_DRAWING_PATH = value
            End Set
        End Property
        Private m_DRAWING_PATH As [String]
        Public Overridable Property DRAWING_REVISION() As [String]
            Get
                Return m_DRAWING_REVISION
            End Get
            Set(value As [String])
                m_DRAWING_REVISION = value
            End Set
        End Property
        Private m_DRAWING_REVISION As [String]
        Public Overridable Property DRAWING_TEMPLATE() As [String]
            Get
                Return m_DRAWING_TEMPLATE
            End Get
            Set(value As [String])
                m_DRAWING_TEMPLATE = value
            End Set
        End Property
        Private m_DRAWING_TEMPLATE As [String]
        Public Overridable Property DRAWING_TITLE() As [String]
            Get
                Return m_DRAWING_TITLE
            End Get
            Set(value As [String])
                m_DRAWING_TITLE = value
            End Set
        End Property
        Private m_DRAWING_TITLE As [String]
        Public Overridable Property DRAWING_VERSION() As [String]
            Get
                Return m_DRAWING_VERSION
            End Get
            Set(value As [String])
                m_DRAWING_VERSION = value
            End Set
        End Property
        Private m_DRAWING_VERSION As [String]
        Public Overridable Property REPRESENTATION_INSTOCKPILE() As [String]
            Get
                Return m_REPRESENTATION_INSTOCKPILE
            End Get
            Set(value As [String])
                m_REPRESENTATION_INSTOCKPILE = value
            End Set
        End Property
        Private m_REPRESENTATION_INSTOCKPILE As [String]
        Public Overridable Property SYMBOL_FILENAME() As [String]
            Get
                Return m_SYMBOL_FILENAME
            End Get
            Set(value As [String])
                m_SYMBOL_FILENAME = value
            End Set
        End Property
        Private m_SYMBOL_FILENAME As [String]
        Public Overridable Property SYMBOL_XCOORDINATE() As [Double]
            Get
                Return m_SYMBOL_XCOORDINATE
            End Get
            Set(value As [Double])
                m_SYMBOL_XCOORDINATE = value
            End Set
        End Property
        Private m_SYMBOL_XCOORDINATE As Double
        Public Overridable Property SYMBOL_YCOORDINATE() As [Double]
            Get
                Return m_SYMBOL_YCOORDINATE
            End Get
            Set(value As [Double])
                m_SYMBOL_YCOORDINATE = value
            End Set
        End Property
        Private m_SYMBOL_YCOORDINATE As Double
        Public Overridable Property EQUIPMENTOTHER_ABSORBEDDUTY() As [String]
            Get
                Return m_EQUIPMENTOTHER_ABSORBEDDUTY
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_ABSORBEDDUTY = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_ABSORBEDDUTY As [String]
        Public Overridable Property EQUIPMENTOTHER_AREA() As [String]
            Get
                Return m_EQUIPMENTOTHER_AREA
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_AREA = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_AREA As [String]
        Public Overridable Property EQUIPMENTOTHER_EQUIPMENTORIENTATION() As [String]
            Get
                Return m_EQUIPMENTOTHER_EQUIPMENTORIENTATION
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_EQUIPMENTORIENTATION = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_EQUIPMENTORIENTATION As [String]
        Public Overridable Property EQUIPMENTOTHER_RATEDDUTY() As [String]
            Get
                Return m_EQUIPMENTOTHER_RATEDDUTY
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_RATEDDUTY = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_RATEDDUTY As [String]
        Public Overridable Property EQUIPMENTOTHER_REVISION_APPROVALTIMESTAMP() As DateTime
            Get
                Return m_EQUIPMENTOTHER_REVISION_APPROVALTIMESTAMP
            End Get
            Set(value As DateTime)
                m_EQUIPMENTOTHER_REVISION_APPROVALTIMESTAMP = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_REVISION_APPROVALTIMESTAMP As DateTime
        Public Overridable Property EQUIPMENTOTHER_REVISION_APPROVEDBY() As [String]
            Get
                Return m_EQUIPMENTOTHER_REVISION_APPROVEDBY
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_REVISION_APPROVEDBY = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_REVISION_APPROVEDBY As [String]
        Public Overridable Property EQUIPMENTOTHER_REVISION_RESPONSIBILITY() As [String]
            Get
                Return m_EQUIPMENTOTHER_REVISION_RESPONSIBILITY
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_REVISION_RESPONSIBILITY = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_REVISION_RESPONSIBILITY As [String]
        Public Overridable Property EQUIPMENTOTHER_REVISION_REVISIONNUMBER() As [String]
            Get
                Return m_EQUIPMENTOTHER_REVISION_REVISIONNUMBER
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_REVISION_REVISIONNUMBER = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_REVISION_REVISIONNUMBER As [String]
        Public Overridable Property EQUIPMENTOTHER_REVISION_STATUSTIMESTAMP() As DateTime
            Get
                Return m_EQUIPMENTOTHER_REVISION_STATUSTIMESTAMP
            End Get
            Set(value As DateTime)
                m_EQUIPMENTOTHER_REVISION_STATUSTIMESTAMP = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_REVISION_STATUSTIMESTAMP As DateTime
        Public Overridable Property EQUIPMENTOTHER_REVISION_STATUSTYPE() As [String]
            Get
                Return m_EQUIPMENTOTHER_REVISION_STATUSTYPE
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_REVISION_STATUSTYPE = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_REVISION_STATUSTYPE As [String]
        Public Overridable Property EQUIPMENTOTHER_REVISION_STATUSVALUE() As [String]
            Get
                Return m_EQUIPMENTOTHER_REVISION_STATUSVALUE
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_REVISION_STATUSVALUE = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_REVISION_STATUSVALUE As [String]
        Public Overridable Property EQUIPMENTOTHER_REVISION_TEXT() As [String]
            Get
                Return m_EQUIPMENTOTHER_REVISION_TEXT
            End Get
            Set(value As [String])
                m_EQUIPMENTOTHER_REVISION_TEXT = value
            End Set
        End Property
        Private m_EQUIPMENTOTHER_REVISION_TEXT As [String]
        Public Overridable Property EXCHANGER_ABSORBEDDUTY() As [String]
            Get
                Return m_EXCHANGER_ABSORBEDDUTY
            End Get
            Set(value As [String])
                m_EXCHANGER_ABSORBEDDUTY = value
            End Set
        End Property
        Private m_EXCHANGER_ABSORBEDDUTY As [String]
        Public Overridable Property EXCHANGER_ABSORBEDDUTYUOM() As [String]
            Get
                Return m_EXCHANGER_ABSORBEDDUTYUOM
            End Get
            Set(value As [String])
                m_EXCHANGER_ABSORBEDDUTYUOM = value
            End Set
        End Property
        Private m_EXCHANGER_ABSORBEDDUTYUOM As [String]
        Public Overloads Property EXCHANGER_CLEANINGREQMTSTUBE() As [String]
            Get
                Return m_EXCHANGER_CLEANINGREQMTSTUBE
            End Get
            Set(value As [String])
                m_EXCHANGER_CLEANINGREQMTSTUBE = value
            End Set
        End Property
        Private m_EXCHANGER_CLEANINGREQMTSTUBE As [String]
        Public Property EXCHANGER_COATINGREQMTSTUBE() As [String]
            Get
                Return m_EXCHANGER_COATINGREQMTSTUBE
            End Get
            Set(value As [String])
                m_EXCHANGER_COATINGREQMTSTUBE = value
            End Set
        End Property
        Private m_EXCHANGER_COATINGREQMTSTUBE As [String]
        Public Overridable Property EXCHANGER_CORROSIONALLOWANCETUBE() As [String]
            Get
                Return m_EXCHANGER_CORROSIONALLOWANCETUBE
            End Get
            Set(value As [String])
                m_EXCHANGER_CORROSIONALLOWANCETUBE = value
            End Set
        End Property
        Private m_EXCHANGER_CORROSIONALLOWANCETUBE As [String]
        Public Overridable Property EXCHANGER_DRAFTTYPE() As [String]
            Get
                Return m_EXCHANGER_DRAFTTYPE
            End Get
            Set(value As [String])
                m_EXCHANGER_DRAFTTYPE = value
            End Set
        End Property
        Private m_EXCHANGER_DRAFTTYPE As [String]
        Public Overridable Property EXCHANGER_EQUIPMENTORIENTATION() As [String]
            Get
                Return m_EXCHANGER_EQUIPMENTORIENTATION
            End Get
            Set(value As [String])
                m_EXCHANGER_EQUIPMENTORIENTATION = value
            End Set
        End Property
        Private m_EXCHANGER_EQUIPMENTORIENTATION As [String]
        Public Overridable Property EXCHANGER_HEATTRANSFERAREAPERUNIT() As [String]
            Get
                Return m_EXCHANGER_HEATTRANSFERAREAPERUNIT
            End Get
            Set(value As [String])
                m_EXCHANGER_HEATTRANSFERAREAPERUNIT = value
            End Set
        End Property
        Private m_EXCHANGER_HEATTRANSFERAREAPERUNIT As [String]
        Public Overridable Property EXCHANGER_HEATTRANSFERRATING() As [String]
            Get
                Return m_EXCHANGER_HEATTRANSFERRATING
            End Get
            Set(value As [String])
                m_EXCHANGER_HEATTRANSFERRATING = value
            End Set
        End Property
        Private m_EXCHANGER_HEATTRANSFERRATING As [String]
        Public Overridable Property EXCHANGER_INSULATIONSPECTUBE() As [String]
            Get
                Return m_EXCHANGER_INSULATIONSPECTUBE
            End Get
            Set(value As [String])
                m_EXCHANGER_INSULATIONSPECTUBE = value
            End Set
        End Property
        Private m_EXCHANGER_INSULATIONSPECTUBE As [String]
        Public Overridable Property EXCHANGER_MATERIALOFCONSTCLASSTUBE() As [String]
            Get
                Return m_EXCHANGER_MATERIALOFCONSTCLASSTUBE
            End Get
            Set(value As [String])
                m_EXCHANGER_MATERIALOFCONSTCLASSTUBE = value
            End Set
        End Property
        Private m_EXCHANGER_MATERIALOFCONSTCLASSTUBE As [String]
        Public Overridable Property EXCHANGER_MOTORPOWERPERFAN() As [String]
            Get
                Return m_EXCHANGER_MOTORPOWERPERFAN
            End Get
            Set(value As [String])
                m_EXCHANGER_MOTORPOWERPERFAN = value
            End Set
        End Property
        Private m_EXCHANGER_MOTORPOWERPERFAN As [String]
        Public Overridable Property EXCHANGER_NUMBEROFBAYS() As [String]
            Get
                Return m_EXCHANGER_NUMBEROFBAYS
            End Get
            Set(value As [String])
                m_EXCHANGER_NUMBEROFBAYS = value
            End Set
        End Property
        Private m_EXCHANGER_NUMBEROFBAYS As [String]
        Public Overridable Property EXCHANGER_NUMBEROFBUNDLES() As [String]
            Get
                Return m_EXCHANGER_NUMBEROFBUNDLES
            End Get
            Set(value As [String])
                m_EXCHANGER_NUMBEROFBUNDLES = value
            End Set
        End Property
        Private m_EXCHANGER_NUMBEROFBUNDLES As [String]
        Public Overridable Property EXCHANGER_NUMBEROFFANS() As [String]
            Get
                Return m_EXCHANGER_NUMBEROFFANS
            End Get
            Set(value As [String])
                m_EXCHANGER_NUMBEROFFANS = value
            End Set
        End Property
        Private m_EXCHANGER_NUMBEROFFANS As [String]
        Public Overridable Property EXCHANGER_NUMBEROFTUBES() As [String]
            Get
                Return m_EXCHANGER_NUMBEROFTUBES
            End Get
            Set(value As [String])
                m_EXCHANGER_NUMBEROFTUBES = value
            End Set
        End Property
        Private m_EXCHANGER_NUMBEROFTUBES As [String]
        Public Overridable Property EXCHANGER_PIPINGMATERIALSCLASSTUBE() As [String]
            Get
                Return m_EXCHANGER_PIPINGMATERIALSCLASSTUBE
            End Get
            Set(value As [String])
                m_EXCHANGER_PIPINGMATERIALSCLASSTUBE = value
            End Set
        End Property
        Private m_EXCHANGER_PIPINGMATERIALSCLASSTUBE As [String]
        Public Overridable Property EXCHANGER_POWERABSORBEDPERFAN() As [String]
            Get
                Return m_EXCHANGER_POWERABSORBEDPERFAN
            End Get
            Set(value As [String])
                m_EXCHANGER_POWERABSORBEDPERFAN = value
            End Set
        End Property
        Private m_EXCHANGER_POWERABSORBEDPERFAN As [String]
        Public Overridable Property EXCHANGER_RATEDDUTY() As [String]
            Get
                Return m_EXCHANGER_RATEDDUTY
            End Get
            Set(value As [String])
                m_EXCHANGER_RATEDDUTY = value
            End Set
        End Property
        Private m_EXCHANGER_RATEDDUTY As [String]
        Public Overridable Property EXCHANGER_RATEDDUTYUOM() As [String]
            Get
                Return m_EXCHANGER_RATEDDUTYUOM
            End Get
            Set(value As [String])
                m_EXCHANGER_RATEDDUTYUOM = value
            End Set
        End Property
        Private m_EXCHANGER_RATEDDUTYUOM As [String]
        Public Overridable Property EXCHANGER_REVISION_APPROVALTIMESTAMP() As DateTime
            Get
                Return m_EXCHANGER_REVISION_APPROVALTIMESTAMP
            End Get
            Set(value As DateTime)
                m_EXCHANGER_REVISION_APPROVALTIMESTAMP = value
            End Set
        End Property
        Private m_EXCHANGER_REVISION_APPROVALTIMESTAMP As DateTime
        Public Overridable Property EXCHANGER_REVISION_APPROVEDBY() As [String]
            Get
                Return m_EXCHANGER_REVISION_APPROVEDBY
            End Get
            Set(value As [String])
                m_EXCHANGER_REVISION_APPROVEDBY = value
            End Set
        End Property
        Private m_EXCHANGER_REVISION_APPROVEDBY As [String]
        Public Overridable Property EXCHANGER_REVISION_RESPONSIBILITY() As [String]
            Get
                Return m_EXCHANGER_REVISION_RESPONSIBILITY
            End Get
            Set(value As [String])
                m_EXCHANGER_REVISION_RESPONSIBILITY = value
            End Set
        End Property
        Private m_EXCHANGER_REVISION_RESPONSIBILITY As [String]
        Public Overridable Property EXCHANGER_REVISION_REVISIONNUMBER() As [String]
            Get
                Return m_EXCHANGER_REVISION_REVISIONNUMBER
            End Get
            Set(value As [String])
                m_EXCHANGER_REVISION_REVISIONNUMBER = value
            End Set
        End Property
        Private m_EXCHANGER_REVISION_REVISIONNUMBER As [String]
        Public Overridable Property EXCHANGER_REVISION_STATUSTIMESTAMP() As DateTime
            Get
                Return m_EXCHANGER_REVISION_STATUSTIMESTAMP
            End Get
            Set(value As DateTime)
                m_EXCHANGER_REVISION_STATUSTIMESTAMP = value
            End Set
        End Property
        Private m_EXCHANGER_REVISION_STATUSTIMESTAMP As DateTime
        Public Overridable Property EXCHANGER_REVISION_STATUSTYPE() As [String]
            Get
                Return m_EXCHANGER_REVISION_STATUSTYPE
            End Get
            Set(value As [String])
                m_EXCHANGER_REVISION_STATUSTYPE = value
            End Set
        End Property
        Private m_EXCHANGER_REVISION_STATUSTYPE As [String]
        Public Overridable Property EXCHANGER_REVISION_STATUSVALUE() As [String]
            Get
                Return m_EXCHANGER_REVISION_STATUSVALUE
            End Get
            Set(value As [String])
                m_EXCHANGER_REVISION_STATUSVALUE = value
            End Set
        End Property
        Private m_EXCHANGER_REVISION_STATUSVALUE As [String]
        Public Overridable Property EXCHANGER_REVISION_TEXT() As [String]
            Get
                Return m_EXCHANGER_REVISION_TEXT
            End Get
            Set(value As [String])
                m_EXCHANGER_REVISION_TEXT = value
            End Set
        End Property
        Private m_EXCHANGER_REVISION_TEXT As [String]
        Public Overridable Property EXCHANGER_SHELLDIAMETER() As [String]
            Get
                Return m_EXCHANGER_SHELLDIAMETER
            End Get
            Set(value As [String])
                m_EXCHANGER_SHELLDIAMETER = value
            End Set
        End Property
        Private m_EXCHANGER_SHELLDIAMETER As [String]
        Public Overridable Property EXCHANGER_SHELLDIAMETERUOM() As [String]
            Get
                Return m_EXCHANGER_SHELLDIAMETERUOM
            End Get
            Set(value As [String])
                m_EXCHANGER_SHELLDIAMETERUOM = value
            End Set
        End Property
        Private m_EXCHANGER_SHELLDIAMETERUOM As [String]
        Public Overridable Property EXCHANGER_TEMA_DESIGNATION() As [String]
            Get
                Return m_EXCHANGER_TEMA_DESIGNATION
            End Get
            Set(value As [String])
                m_EXCHANGER_TEMA_DESIGNATION = value
            End Set
        End Property
        Private m_EXCHANGER_TEMA_DESIGNATION As [String]
        Public Overridable Property EXCHANGER_TUBELENGTH() As [String]
            Get
                Return m_EXCHANGER_TUBELENGTH
            End Get
            Set(value As [String])
                m_EXCHANGER_TUBELENGTH = value
            End Set
        End Property
        Private m_EXCHANGER_TUBELENGTH As [String]
        Public Overridable Property EXCHANGER_TYPEOFLOUVERS() As [String]
            Get
                Return m_EXCHANGER_TYPEOFLOUVERS
            End Get
            Set(value As [String])
                m_EXCHANGER_TYPEOFLOUVERS = value
            End Set
        End Property
        Private m_EXCHANGER_TYPEOFLOUVERS As [String]
        Public Overridable Property EXCHANGER_UNITWIDTH() As [String]
            Get
                Return m_EXCHANGER_UNITWIDTH
            End Get
            Set(value As [String])
                m_EXCHANGER_UNITWIDTH = value
            End Set
        End Property
        Private m_EXCHANGER_UNITWIDTH As [String]
        Public Overridable Property MECHANICAL_CWPIPINGPLAN() As [String]
            Get
                Return m_MECHANICAL_CWPIPINGPLAN
            End Get
            Set(value As [String])
                m_MECHANICAL_CWPIPINGPLAN = value
            End Set
        End Property
        Private m_MECHANICAL_CWPIPINGPLAN As [String]
        Public Overridable Property MECHANICAL_DIFFERENTIALPRESSURE() As [String]
            Get
                Return m_MECHANICAL_DIFFERENTIALPRESSURE
            End Get
            Set(value As [String])
                m_MECHANICAL_DIFFERENTIALPRESSURE = value
            End Set
        End Property
        Private m_MECHANICAL_DIFFERENTIALPRESSURE As [String]
        Public Overridable Property MECHANICAL_DIFFERENTIALPRESSUREUOM() As [String]
            Get
                Return m_MECHANICAL_DIFFERENTIALPRESSUREUOM
            End Get
            Set(value As [String])
                m_MECHANICAL_DIFFERENTIALPRESSUREUOM = value
            End Set
        End Property
        Private m_MECHANICAL_DIFFERENTIALPRESSUREUOM As [String]
        Public Overridable Property MECHANICAL_DRIVERRATEDPOWER() As [String]
            Get
                Return m_MECHANICAL_DRIVERRATEDPOWER
            End Get
            Set(value As [String])
                m_MECHANICAL_DRIVERRATEDPOWER = value
            End Set
        End Property
        Private m_MECHANICAL_DRIVERRATEDPOWER As [String]
        Public Overridable Property MECHANICAL_DRIVERRATEDPOWERUOM() As [String]
            Get
                Return m_MECHANICAL_DRIVERRATEDPOWERUOM
            End Get
            Set(value As [String])
                m_MECHANICAL_DRIVERRATEDPOWERUOM = value
            End Set
        End Property
        Private m_MECHANICAL_DRIVERRATEDPOWERUOM As [String]
        Public Overridable Property MECHANICAL_ELECTRICALREQMT() As [String]
            Get
                Return m_MECHANICAL_ELECTRICALREQMT
            End Get
            Set(value As [String])
                m_MECHANICAL_ELECTRICALREQMT = value
            End Set
        End Property
        Private m_MECHANICAL_ELECTRICALREQMT As [String]
        Public Overridable Property MECHANICAL_MATERIALOFCONSTCLASSINTERNAL() As [String]
            Get
                Return m_MECHANICAL_MATERIALOFCONSTCLASSINTERNAL
            End Get
            Set(value As [String])
                m_MECHANICAL_MATERIALOFCONSTCLASSINTERNAL = value
            End Set
        End Property
        Private m_MECHANICAL_MATERIALOFCONSTCLASSINTERNAL As [String]
        Public Overridable Property MECHANICAL_MECHRATING() As [String]
            Get
                Return m_MECHANICAL_MECHRATING
            End Get
            Set(value As [String])
                m_MECHANICAL_MECHRATING = value
            End Set
        End Property
        Private m_MECHANICAL_MECHRATING As [String]
        Public Overridable Property MECHANICAL_POWERABSORBED() As [String]
            Get
                Return m_MECHANICAL_POWERABSORBED
            End Get
            Set(value As [String])
                m_MECHANICAL_POWERABSORBED = value
            End Set
        End Property
        Private m_MECHANICAL_POWERABSORBED As [String]
        Public Overridable Property MECHANICAL_POWERCONSUMPTION() As [String]
            Get
                Return m_MECHANICAL_POWERCONSUMPTION
            End Get
            Set(value As [String])
                m_MECHANICAL_POWERCONSUMPTION = value
            End Set
        End Property
        Private m_MECHANICAL_POWERCONSUMPTION As [String]
        Public Overridable Property MECHANICAL_RATEDCAPACITY() As [String]
            Get
                Return m_MECHANICAL_RATEDCAPACITY
            End Get
            Set(value As [String])
                m_MECHANICAL_RATEDCAPACITY = value
            End Set
        End Property
        Private m_MECHANICAL_RATEDCAPACITY As [String]
        Public Overridable Property MECHANICAL_RATEDCAPACITYUOM() As [String]
            Get
                Return m_MECHANICAL_RATEDCAPACITYUOM
            End Get
            Set(value As [String])
                m_MECHANICAL_RATEDCAPACITYUOM = value
            End Set
        End Property
        Private m_MECHANICAL_RATEDCAPACITYUOM As [String]
        Public Overridable Property MECHANICAL_RATEDDISCHARGEPRESSURE() As [String]
            Get
                Return m_MECHANICAL_RATEDDISCHARGEPRESSURE
            End Get
            Set(value As [String])
                m_MECHANICAL_RATEDDISCHARGEPRESSURE = value
            End Set
        End Property
        Private m_MECHANICAL_RATEDDISCHARGEPRESSURE As [String]
        Public Overridable Property MECHANICAL_RATEDSUCTIONPRESSURE() As [String]
            Get
                Return m_MECHANICAL_RATEDSUCTIONPRESSURE
            End Get
            Set(value As [String])
                m_MECHANICAL_RATEDSUCTIONPRESSURE = value
            End Set
        End Property
        Private m_MECHANICAL_RATEDSUCTIONPRESSURE As [String]
        Public Overridable Property MECHANICAL_REVISION_APPROVALTIMESTAMP() As DateTime
            Get
                Return m_MECHANICAL_REVISION_APPROVALTIMESTAMP
            End Get
            Set(value As DateTime)
                m_MECHANICAL_REVISION_APPROVALTIMESTAMP = value
            End Set
        End Property
        Private m_MECHANICAL_REVISION_APPROVALTIMESTAMP As DateTime
        Public Overridable Property MECHANICAL_REVISION_APPROVEDBY() As [String]
            Get
                Return m_MECHANICAL_REVISION_APPROVEDBY
            End Get
            Set(value As [String])
                m_MECHANICAL_REVISION_APPROVEDBY = value
            End Set
        End Property
        Private m_MECHANICAL_REVISION_APPROVEDBY As [String]
        Public Overridable Property MECHANICAL_REVISION_RESPONSIBILITY() As [String]
            Get
                Return m_MECHANICAL_REVISION_RESPONSIBILITY
            End Get
            Set(value As [String])
                m_MECHANICAL_REVISION_RESPONSIBILITY = value
            End Set
        End Property
        Private m_MECHANICAL_REVISION_RESPONSIBILITY As [String]
        Public Overridable Property MECHANICAL_REVISION_REVISIONNUMBER() As [String]
            Get
                Return m_MECHANICAL_REVISION_REVISIONNUMBER
            End Get
            Set(value As [String])
                m_MECHANICAL_REVISION_REVISIONNUMBER = value
            End Set
        End Property
        Private m_MECHANICAL_REVISION_REVISIONNUMBER As [String]
        Public Overridable Property MECHANICAL_REVISION_STATUSTIMESTAMP() As DateTime
            Get
                Return m_MECHANICAL_REVISION_STATUSTIMESTAMP
            End Get
            Set(value As DateTime)
                m_MECHANICAL_REVISION_STATUSTIMESTAMP = value
            End Set
        End Property
        Private m_MECHANICAL_REVISION_STATUSTIMESTAMP As DateTime
        Public Overridable Property MECHANICAL_REVISION_STATUSTYPE() As [String]
            Get
                Return m_MECHANICAL_REVISION_STATUSTYPE
            End Get
            Set(value As [String])
                m_MECHANICAL_REVISION_STATUSTYPE = value
            End Set
        End Property
        Private m_MECHANICAL_REVISION_STATUSTYPE As [String]
        Public Overridable Property MECHANICAL_REVISION_STATUSVALUE() As [String]
            Get
                Return m_MECHANICAL_REVISION_STATUSVALUE
            End Get
            Set(value As [String])
                m_MECHANICAL_REVISION_STATUSVALUE = value
            End Set
        End Property
        Private m_MECHANICAL_REVISION_STATUSVALUE As [String]
        Public Overridable Property MECHANICAL_REVISION_TEXT() As [String]
            Get
                Return m_MECHANICAL_REVISION_TEXT
            End Get
            Set(value As [String])
                m_MECHANICAL_REVISION_TEXT = value
            End Set
        End Property
        Private m_MECHANICAL_REVISION_TEXT As [String]
        Public Overridable Property MECHANICAL_SEALPIPINGPLAN() As [String]
            Get
                Return m_MECHANICAL_SEALPIPINGPLAN
            End Get
            Set(value As [String])
                m_MECHANICAL_SEALPIPINGPLAN = value
            End Set
        End Property
        Private m_MECHANICAL_SEALPIPINGPLAN As [String]
        Public Overridable Property MECHANICAL_TYPEOFDRIVER() As [String]
            Get
                Return m_MECHANICAL_TYPEOFDRIVER
            End Get
            Set(value As [String])
                m_MECHANICAL_TYPEOFDRIVER = value
            End Set
        End Property
        Private m_MECHANICAL_TYPEOFDRIVER As [String]
        Public Overridable Property VESSEL_DIAMETERINTERNAL() As [String]
            Get
                Return m_VESSEL_DIAMETERINTERNAL
            End Get
            Set(value As [String])
                m_VESSEL_DIAMETERINTERNAL = value
            End Set
        End Property
        Private m_VESSEL_DIAMETERINTERNAL As [String]
        Public Overridable Property VESSEL_EQUIPMENTORIENTATION() As [String]
            Get
                Return m_VESSEL_EQUIPMENTORIENTATION
            End Get
            Set(value As [String])
                m_VESSEL_EQUIPMENTORIENTATION = value
            End Set
        End Property
        Private m_VESSEL_EQUIPMENTORIENTATION As [String]
        Public Overridable Property VESSEL_LENGTHTANTOTAN() As [String]
            Get
                Return m_VESSEL_LENGTHTANTOTAN
            End Get
            Set(value As [String])
                m_VESSEL_LENGTHTANTOTAN = value
            End Set
        End Property
        Private m_VESSEL_LENGTHTANTOTAN As [String]
        Public Overridable Property VESSEL_LENGTHTANTOTANUOM() As [String]
            Get
                Return m_VESSEL_LENGTHTANTOTANUOM
            End Get
            Set(value As [String])
                m_VESSEL_LENGTHTANTOTANUOM = value
            End Set
        End Property
        Private m_VESSEL_LENGTHTANTOTANUOM As [String]
        Public Overridable Property VESSEL_LEVELREFERENCE() As [String]
            Get
                Return m_VESSEL_LEVELREFERENCE
            End Get
            Set(value As [String])
                m_VESSEL_LEVELREFERENCE = value
            End Set
        End Property
        Private m_VESSEL_LEVELREFERENCE As [String]
        Public Overridable Property VESSEL_LEVELREFERENCEUOM() As [String]
            Get
                Return m_VESSEL_LEVELREFERENCEUOM
            End Get
            Set(value As [String])
                m_VESSEL_LEVELREFERENCEUOM = value
            End Set
        End Property
        Private m_VESSEL_LEVELREFERENCEUOM As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELHIGH() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELHIGH
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELHIGH = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELHIGH As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELHIGHUOM() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELHIGHUOM
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELHIGHUOM = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELHIGHUOM As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELHIGHHIGH() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELHIGHHIGH
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELHIGHHIGH = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELHIGHHIGH As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELHIGHHIGHUOM() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELHIGHHIGHUOM
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELHIGHHIGHUOM = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELHIGHHIGHUOM As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELLOW() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELLOW
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELLOW = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELLOW As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELLOWUOM() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELLOWUOM
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELLOWUOM = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELLOWUOM As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELLOWLOW() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELLOWLOW
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELLOWLOW = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELLOWLOW As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELLOWLOWUOM() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELLOWLOWUOM
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELLOWLOWUOM = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELLOWLOWUOM As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELNORMAL() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELNORMAL
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELNORMAL = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELNORMAL As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELNORMALUOM() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELNORMALUOM
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELNORMALUOM = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELNORMALUOM As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELOVERFLOW() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELOVERFLOW
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELOVERFLOW = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELOVERFLOW As [String]
        Public Overridable Property VESSEL_LIQUIDLEVELOVERFLOWUOM() As [String]
            Get
                Return m_VESSEL_LIQUIDLEVELOVERFLOWUOM
            End Get
            Set(value As [String])
                m_VESSEL_LIQUIDLEVELOVERFLOWUOM = value
            End Set
        End Property
        Private m_VESSEL_LIQUIDLEVELOVERFLOWUOM As [String]
        Public Overridable Property VESSEL_MATERIALOFCONSTCLASSINTERNAL() As [String]
            Get
                Return m_VESSEL_MATERIALOFCONSTCLASSINTERNAL
            End Get
            Set(value As [String])
                m_VESSEL_MATERIALOFCONSTCLASSINTERNAL = value
            End Set
        End Property
        Private m_VESSEL_MATERIALOFCONSTCLASSINTERNAL As [String]
        Public Overridable Property VESSEL_REVISION_APPROVALTIMESTAMP() As DateTime
            Get
                Return m_VESSEL_REVISION_APPROVALTIMESTAMP
            End Get
            Set(value As DateTime)
                m_VESSEL_REVISION_APPROVALTIMESTAMP = value
            End Set
        End Property
        Private m_VESSEL_REVISION_APPROVALTIMESTAMP As DateTime
        Public Overridable Property VESSEL_REVISION_APPROVEDBY() As [String]
            Get
                Return m_VESSEL_REVISION_APPROVEDBY
            End Get
            Set(value As [String])
                m_VESSEL_REVISION_APPROVEDBY = value
            End Set
        End Property
        Private m_VESSEL_REVISION_APPROVEDBY As [String]
        Public Overridable Property VESSEL_REVISION_RESPONSIBILITY() As [String]
            Get
                Return m_VESSEL_REVISION_RESPONSIBILITY
            End Get
            Set(value As [String])
                m_VESSEL_REVISION_RESPONSIBILITY = value
            End Set
        End Property
        Private m_VESSEL_REVISION_RESPONSIBILITY As [String]
        Public Overridable Property VESSEL_REVISION_REVISIONNUMBER() As [String]
            Get
                Return m_VESSEL_REVISION_REVISIONNUMBER
            End Get
            Set(value As [String])
                m_VESSEL_REVISION_REVISIONNUMBER = value
            End Set
        End Property
        Private m_VESSEL_REVISION_REVISIONNUMBER As [String]
        Public Overridable Property VESSEL_REVISION_STATUSTIMESTAMP() As DateTime
            Get
                Return m_VESSEL_REVISION_STATUSTIMESTAMP
            End Get
            Set(value As DateTime)
                m_VESSEL_REVISION_STATUSTIMESTAMP = value
            End Set
        End Property
        Private m_VESSEL_REVISION_STATUSTIMESTAMP As DateTime
        Public Overridable Property VESSEL_REVISION_STATUSTYPE() As [String]
            Get
                Return m_VESSEL_REVISION_STATUSTYPE
            End Get
            Set(value As [String])
                m_VESSEL_REVISION_STATUSTYPE = value
            End Set
        End Property
        Private m_VESSEL_REVISION_STATUSTYPE As [String]
        Public Overridable Property VESSEL_REVISION_STATUSVALUE() As [String]
            Get
                Return m_VESSEL_REVISION_STATUSVALUE
            End Get
            Set(value As [String])
                m_VESSEL_REVISION_STATUSVALUE = value
            End Set
        End Property
        Private m_VESSEL_REVISION_STATUSVALUE As [String]
        Public Overridable Property VESSEL_REVISION_TEXT() As [String]
            Get
                Return m_VESSEL_REVISION_TEXT
            End Get
            Set(value As [String])
                m_VESSEL_REVISION_TEXT = value
            End Set
        End Property
        Private m_VESSEL_REVISION_TEXT As [String]
        Public Overridable Property VESSEL_VOLUMERATING() As [String]
            Get
                Return m_VESSEL_VOLUMERATING
            End Get
            Set(value As [String])
                m_VESSEL_VOLUMERATING = value
            End Set
        End Property
        Private m_VESSEL_VOLUMERATING As [String]
        Public Overridable Property VESSEL_VOLUMERATINGUOM() As [String]
            Get
                Return m_VESSEL_VOLUMERATINGUOM
            End Get
            Set(value As [String])
                m_VESSEL_VOLUMERATINGUOM = value
            End Set
        End Property
        Private m_VESSEL_VOLUMERATINGUOM As [String]
        Public Overridable Property [CLASS]() As [String]
            Get
                Return m_CLASS
            End Get
            Set(value As [String])
                m_CLASS = value
            End Set
        End Property
        Private m_CLASS As [String]
        Public Overridable Property CLEANINGREQMTS() As [String]
            Get
                Return m_CLEANINGREQMTS
            End Get
            Set(value As [String])
                m_CLEANINGREQMTS = value
            End Set
        End Property
        Private m_CLEANINGREQMTS As [String]
        Public Overridable Property COATINGREQMTS() As [String]
            Get
                Return m_COATINGREQMTS
            End Get
            Set(value As [String])
                m_COATINGREQMTS = value
            End Set
        End Property
        Private m_COATINGREQMTS As [String]
        Public Overridable Property CONSTRUCTIONBY() As [String]
            Get
                Return m_CONSTRUCTIONBY
            End Get
            Set(value As [String])
                m_CONSTRUCTIONBY = value
            End Set
        End Property
        Private m_CONSTRUCTIONBY As [String]
        Public Overridable Property CONSTRUCTIONSTATUS() As [String]
            Get
                Return m_CONSTRUCTIONSTATUS
            End Get
            Set(value As [String])
                m_CONSTRUCTIONSTATUS = value
            End Set
        End Property
        Private m_CONSTRUCTIONSTATUS As [String]
        Public Overridable Property CORROSIONALLOWANCE() As [String]
            Get
                Return m_CORROSIONALLOWANCE
            End Get
            Set(value As [String])
                m_CORROSIONALLOWANCE = value
            End Set
        End Property
        Private m_CORROSIONALLOWANCE As [String]
        Public Overridable Property DESCRIPTION() As [String]
            Get
                Return m_DESCRIPTION
            End Get
            Set(value As [String])
                m_DESCRIPTION = value
            End Set
        End Property
        Private m_DESCRIPTION As [String]
        Public Overridable Property DESIGNBY() As [String]
            Get
                Return m_DESIGNBY
            End Get
            Set(value As [String])
                m_DESIGNBY = value
            End Set
        End Property
        Private m_DESIGNBY As [String]
        Public Overridable Property ERPASSETNO() As [String]
            Get
                Return m_ERPASSETNO
            End Get
            Set(value As [String])
                m_ERPASSETNO = value
            End Set
        End Property
        Private m_ERPASSETNO As [String]
        Public Overridable Property EQUIPMENTSUBCLASS() As [String]
            Get
                Return m_EQUIPMENTSUBCLASS
            End Get
            Set(value As [String])
                m_EQUIPMENTSUBCLASS = value
            End Set
        End Property
        Private m_EQUIPMENTSUBCLASS As [String]
        Public Overridable Property EQUIPMENTTYPE() As [String]
            Get
                Return m_EQUIPMENTTYPE
            End Get
            Set(value As [String])
                m_EQUIPMENTTYPE = value
            End Set
        End Property
        Private m_EQUIPMENTTYPE As [String]
        Public Overridable Property FABRICATIONCATEGORY() As [String]
            Get
                Return m_FABRICATIONCATEGORY
            End Get
            Set(value As [String])
                m_FABRICATIONCATEGORY = value
            End Set
        End Property
        Private m_FABRICATIONCATEGORY As [String]
        Public Overridable Property HTRACEMEDIUM() As [String]
            Get
                Return m_HTRACEMEDIUM
            End Get
            Set(value As [String])
                m_HTRACEMEDIUM = value
            End Set
        End Property
        Private m_HTRACEMEDIUM As [String]
        Public Overridable Property HTRACEMEDIUMTEMP() As [String]
            Get
                Return m_HTRACEMEDIUMTEMP
            End Get
            Set(value As [String])
                m_HTRACEMEDIUMTEMP = value
            End Set
        End Property
        Private m_HTRACEMEDIUMTEMP As [String]
        Public Overridable Property HTRACEREQMT() As [String]
            Get
                Return m_HTRACEREQMT
            End Get
            Set(value As [String])
                m_HTRACEREQMT = value
            End Set
        End Property
        Private m_HTRACEREQMT As [String]
        Public Overridable Property HEIGHT() As [String]
            Get
                Return m_HEIGHT
            End Get
            Set(value As [String])
                m_HEIGHT = value
            End Set
        End Property
        Private m_HEIGHT As [String]
        Public Overridable Property HEIGHTUOM() As [String]
            Get
                Return m_HEIGHTUOM
            End Get
            Set(value As [String])
                m_HEIGHTUOM = value
            End Set
        End Property
        Private m_HEIGHTUOM As [String]
        Public Overridable Property HOLDSTATUS_APPROVALTIMESTAMP() As DateTime
            Get
                Return m_HOLDSTATUS_APPROVALTIMESTAMP
            End Get
            Set(value As DateTime)
                m_HOLDSTATUS_APPROVALTIMESTAMP = value
            End Set
        End Property
        Private m_HOLDSTATUS_APPROVALTIMESTAMP As DateTime
        Public Overridable Property HOLDSTATUS_APPROVEDBY() As [String]
            Get
                Return m_HOLDSTATUS_APPROVEDBY
            End Get
            Set(value As [String])
                m_HOLDSTATUS_APPROVEDBY = value
            End Set
        End Property
        Private m_HOLDSTATUS_APPROVEDBY As [String]
        Public Overridable Property HOLDSTATUS_RESPONSIBILITY() As [String]
            Get
                Return m_HOLDSTATUS_RESPONSIBILITY
            End Get
            Set(value As [String])
                m_HOLDSTATUS_RESPONSIBILITY = value
            End Set
        End Property
        Private m_HOLDSTATUS_RESPONSIBILITY As [String]
        Public Overridable Property HOLDSTATUS_REVISIONNUMBER() As [String]
            Get
                Return m_HOLDSTATUS_REVISIONNUMBER
            End Get
            Set(value As [String])
                m_HOLDSTATUS_REVISIONNUMBER = value
            End Set
        End Property
        Private m_HOLDSTATUS_REVISIONNUMBER As [String]
        Public Overridable Property HOLDSTATUS_STATUSTIMESTAMP() As DateTime
            Get
                Return m_HOLDSTATUS_STATUSTIMESTAMP
            End Get
            Set(value As DateTime)
                m_HOLDSTATUS_STATUSTIMESTAMP = value
            End Set
        End Property
        Private m_HOLDSTATUS_STATUSTIMESTAMP As DateTime
        Public Overridable Property HOLDSTATUS_STATUSTYPE() As [String]
            Get
                Return m_HOLDSTATUS_STATUSTYPE
            End Get
            Set(value As [String])
                m_HOLDSTATUS_STATUSTYPE = value
            End Set
        End Property
        Private m_HOLDSTATUS_STATUSTYPE As [String]
        Public Overridable Property HOLDSTATUS_STATUSVALUE() As [String]
            Get
                Return m_HOLDSTATUS_STATUSVALUE
            End Get
            Set(value As [String])
                m_HOLDSTATUS_STATUSVALUE = value
            End Set
        End Property
        Private m_HOLDSTATUS_STATUSVALUE As [String]
        Public Overridable Property HOLDSTATUS_TEXT() As [String]
            Get
                Return m_HOLDSTATUS_TEXT
            End Get
            Set(value As [String])
                m_HOLDSTATUS_TEXT = value
            End Set
        End Property
        Private m_HOLDSTATUS_TEXT As [String]
        Public Overridable Property HOLDSTATUS_UPDATECOUNT() As Int32
            Get
                Return m_HOLDSTATUS_UPDATECOUNT
            End Get
            Set(value As Int32)
                m_HOLDSTATUS_UPDATECOUNT = value
            End Set
        End Property
        Private m_HOLDSTATUS_UPDATECOUNT As Int32
        Public Overridable Property INSULDENSITY() As [String]
            Get
                Return m_INSULDENSITY
            End Get
            Set(value As [String])
                m_INSULDENSITY = value
            End Set
        End Property
        Private m_INSULDENSITY As [String]
        Public Overridable Property INSULPURPOSE() As [String]
            Get
                Return m_INSULPURPOSE
            End Get
            Set(value As [String])
                m_INSULPURPOSE = value
            End Set
        End Property
        Private m_INSULPURPOSE As [String]
        Public Overridable Property INSULTEMP() As [String]
            Get
                Return m_INSULTEMP
            End Get
            Set(value As [String])
                m_INSULTEMP = value
            End Set
        End Property
        Private m_INSULTEMP As [String]
        Public Overridable Property INSULTHICK() As [String]
            Get
                Return m_INSULTHICK
            End Get
            Set(value As [String])
                m_INSULTHICK = value
            End Set
        End Property
        Private m_INSULTHICK As [String]
        Public Overridable Property INSULTHICKUOM() As [String]
            Get
                Return m_INSULTHICKUOM
            End Get
            Set(value As [String])
                m_INSULTHICKUOM = value
            End Set
        End Property
        Private m_INSULTHICKUOM As [String]
        Public Overridable Property INSULTYPE() As [String]
            Get
                Return m_INSULTYPE
            End Get
            Set(value As [String])
                m_INSULTYPE = value
            End Set
        End Property
        Private m_INSULTYPE As [String]
        Public Overridable Property INSULATIONSPEC() As [String]
            Get
                Return m_INSULATIONSPEC
            End Get
            Set(value As [String])
                m_INSULATIONSPEC = value
            End Set
        End Property
        Private m_INSULATIONSPEC As [String]
        Public Overridable Property INSULATIONTHKSOURCE() As [String]
            Get
                Return m_INSULATIONTHKSOURCE
            End Get
            Set(value As [String])
                m_INSULATIONTHKSOURCE = value
            End Set
        End Property
        Private m_INSULATIONTHKSOURCE As [String]
        Public Overridable Property INVENTORYTAG() As [String]
            Get
                Return m_INVENTORYTAG
            End Get
            Set(value As [String])
                m_INVENTORYTAG = value
            End Set
        End Property
        Private m_INVENTORYTAG As [String]
        Public Overridable Property ISBULKITEM() As [String]
            Get
                Return m_ISBULKITEM
            End Get
            Set(value As [String])
                m_ISBULKITEM = value
            End Set
        End Property
        Private m_ISBULKITEM As [String]
        Public Overridable Property ISUNCHECKED() As [String]
            Get
                Return m_ISUNCHECKED
            End Get
            Set(value As [String])
                m_ISUNCHECKED = value
            End Set
        End Property
        Private m_ISUNCHECKED As [String]
        Public Overridable Property ITEMSTATUS() As [String]
            Get
                Return m_ITEMSTATUS
            End Get
            Set(value As [String])
                m_ITEMSTATUS = value
            End Set
        End Property
        Private m_ITEMSTATUS As [String]
        Public Overridable Property ITEMTAG() As [String]
            Get
                Return m_ITEMTAG
            End Get
            Set(value As [String])
                m_ITEMTAG = value
            End Set
        End Property
        Private m_ITEMTAG As [String]
        Public Overridable Property ITEMTYPENAME() As [String]
            Get
                Return m_ITEMTYPENAME
            End Get
            Set(value As [String])
                m_ITEMTYPENAME = value
            End Set
        End Property
        Private m_ITEMTYPENAME As [String]
        Public Overridable Property MATERIALOFCONSTCLASS() As [String]
            Get
                Return m_MATERIALOFCONSTCLASS
            End Get
            Set(value As [String])
                m_MATERIALOFCONSTCLASS = value
            End Set
        End Property
        Private m_MATERIALOFCONSTCLASS As [String]
        Public Overridable Property MODELITEMTYPE() As [String]
            Get
                Return m_MODELITEMTYPE
            End Get
            Set(value As [String])
                m_MODELITEMTYPE = value
            End Set
        End Property
        Private m_MODELITEMTYPE As [String]
        Public Overridable Property NAME() As [String]
            Get
                Return m_NAME
            End Get
            Set(value As [String])
                m_NAME = value
            End Set
        End Property
        Private m_NAME As [String]
        Public Overridable Property PARTOFTYPE() As [String]
            Get
                Return m_PARTOFTYPE
            End Get
            Set(value As [String])
                m_PARTOFTYPE = value
            End Set
        End Property
        Private m_PARTOFTYPE As [String]
        Public Overridable Property PIPINGMATERIALSCLASS() As [String]
            Get
                Return m_PIPINGMATERIALSCLASS
            End Get
            Set(value As [String])
                m_PIPINGMATERIALSCLASS = value
            End Set
        End Property
        Private m_PIPINGMATERIALSCLASS As [String]
        Public Overridable Property PLANTITEMTYPE() As [String]
            Get
                Return m_PLANTITEMTYPE
            End Get
            Set(value As [String])
                m_PLANTITEMTYPE = value
            End Set
        End Property
        Private m_PLANTITEMTYPE As [String]
        Public Overridable Property REQUISITIONBY() As [String]
            Get
                Return m_REQUISITIONBY
            End Get
            Set(value As [String])
                m_REQUISITIONBY = value
            End Set
        End Property
        Private m_REQUISITIONBY As [String]
        Public Overridable Property REQUISITIONNO() As [String]
            Get
                Return m_REQUISITIONNO
            End Get
            Set(value As [String])
                m_REQUISITIONNO = value
            End Set
        End Property
        Private m_REQUISITIONNO As [String]
        Public Overridable Property SLOPE() As [String]
            Get
                Return m_SLOPE
            End Get
            Set(value As [String])
                m_SLOPE = value
            End Set
        End Property
        Private m_SLOPE As [String]
        Public Overridable Property SLOPERISE() As [String]
            Get
                Return m_SLOPERISE
            End Get
            Set(value As [String])
                m_SLOPERISE = value
            End Set
        End Property
        Private m_SLOPERISE As [String]
        Public Overridable Property SLOPERUN() As [String]
            Get
                Return m_SLOPERUN
            End Get
            Set(value As [String])
                m_SLOPERUN = value
            End Set
        End Property
        Private m_SLOPERUN As [String]
        Public Overridable Property STEAMOUTPRESSURE() As [String]
            Get
                Return m_STEAMOUTPRESSURE
            End Get
            Set(value As [String])
                m_STEAMOUTPRESSURE = value
            End Set
        End Property
        Private m_STEAMOUTPRESSURE As [String]
        Public Overridable Property STEAMOUTREQMT() As [String]
            Get
                Return m_STEAMOUTREQMT
            End Get
            Set(value As [String])
                m_STEAMOUTREQMT = value
            End Set
        End Property
        Private m_STEAMOUTREQMT As [String]
        Public Overridable Property STEAMOUTTEMPERATURE() As [String]
            Get
                Return m_STEAMOUTTEMPERATURE
            End Get
            Set(value As [String])
                m_STEAMOUTTEMPERATURE = value
            End Set
        End Property
        Private m_STEAMOUTTEMPERATURE As [String]
        Public Overridable Property STRESSRELIEFREQMT() As [String]
            Get
                Return m_STRESSRELIEFREQMT
            End Get
            Set(value As [String])
                m_STRESSRELIEFREQMT = value
            End Set
        End Property
        Private m_STRESSRELIEFREQMT As [String]
        Public Overridable Property SUPPLYBY() As [String]
            Get
                Return m_SUPPLYBY
            End Get
            Set(value As [String])
                m_SUPPLYBY = value
            End Set
        End Property
        Private m_SUPPLYBY As [String]
        Public Overridable Property TAGPREFIX() As [String]
            Get
                Return m_TAGPREFIX
            End Get
            Set(value As [String])
                m_TAGPREFIX = value
            End Set
        End Property
        Private m_TAGPREFIX As [String]
        Public Overridable Property TAGREQDFLAG() As [String]
            Get
                Return m_TAGREQDFLAG
            End Get
            Set(value As [String])
                m_TAGREQDFLAG = value
            End Set
        End Property
        Private m_TAGREQDFLAG As [String]
        Public Overridable Property TAGSEQUENCENO() As [String]
            Get
                Return m_TAGSEQUENCENO
            End Get
            Set(value As [String])
                m_TAGSEQUENCENO = value
            End Set
        End Property
        Private m_TAGSEQUENCENO As [String]
        Public Overridable Property TAGSUFFIX() As [String]
            Get
                Return m_TAGSUFFIX
            End Get
            Set(value As [String])
                m_TAGSUFFIX = value
            End Set
        End Property
        Private m_TAGSUFFIX As [String]
        Public Overridable Property TRIMSPEC() As [String]
            Get
                Return m_TRIMSPEC
            End Get
            Set(value As [String])
                m_TRIMSPEC = value
            End Set
        End Property
        Private m_TRIMSPEC As [String]
        Public Overridable Property UPDATECOUNT() As Int32
            Get
                Return m_UPDATECOUNT
            End Get
            Set(value As Int32)
                m_UPDATECOUNT = value
            End Set
        End Property
        Private m_UPDATECOUNT As Int32
        Public Overridable Property AABBCC_CODE() As [String]
            Get
                Return m_AABBCC_CODE
            End Get
            Set(value As [String])
                m_AABBCC_CODE = value
            End Set
        End Property
        Private m_AABBCC_CODE As [String]

        Public Function GetPropertyValue(propertyName As [String]) As Object Implements IDataObject.GetPropertyValue

            Select Case propertyName
                Case "SP_ID"
                    Return SP_ID
                Case "ADAPTER_PARENTTAG"
                    Return ADAPTER_PARENTTAG
                Case "DRAWING_DATECREATED"
                    Return DRAWING_DATECREATED
                Case "DRAWING_DESCRIPTION"
                    Return DRAWING_DESCRIPTION
                Case "DRAWING_DOCUMENTCATEGORY"
                    Return DRAWING_DOCUMENTCATEGORY
                Case "DRAWING_DOCUMENTTYPE"
                    Return DRAWING_DOCUMENTTYPE
                Case "DRAWING_DRAWINGNUMBER"
                    Return DRAWING_DRAWINGNUMBER
                Case "DRAWING_ITEMSTATUS"
                    Return DRAWING_ITEMSTATUS
                Case "DRAWING_NAME"
                    Return DRAWING_NAME
                Case "DRAWING_PATH"
                    Return DRAWING_PATH
                Case "DRAWING_REVISION"
                    Return DRAWING_REVISION
                Case "DRAWING_TEMPLATE"
                    Return DRAWING_TEMPLATE
                Case "DRAWING_TITLE"
                    Return DRAWING_TITLE
                Case "DRAWING_VERSION"
                    Return DRAWING_VERSION
                Case "REPRESENTATION_INSTOCKPILE"
                    Return REPRESENTATION_INSTOCKPILE
                Case "SYMBOL_FILENAME"
                    Return SYMBOL_FILENAME
                Case "SYMBOL_XCOORDINATE"
                    Return SYMBOL_XCOORDINATE
                Case "SYMBOL_YCOORDINATE"
                    Return SYMBOL_YCOORDINATE
                Case "EQUIPMENTOTHER_ABSORBEDDUTY"
                    Return EQUIPMENTOTHER_ABSORBEDDUTY
                Case "EQUIPMENTOTHER_AREA"
                    Return EQUIPMENTOTHER_AREA
                Case "EQUIPMENTOTHER_EQUIPMENTORIENTATION"
                    Return EQUIPMENTOTHER_EQUIPMENTORIENTATION
                Case "EQUIPMENTOTHER_RATEDDUTY"
                    Return EQUIPMENTOTHER_RATEDDUTY
                Case "EQUIPMENTOTHER_REVISION_APPROVALTIMESTAMP"
                    Return EQUIPMENTOTHER_REVISION_APPROVALTIMESTAMP
                Case "EQUIPMENTOTHER_REVISION_APPROVEDBY"
                    Return EQUIPMENTOTHER_REVISION_APPROVEDBY
                Case "EQUIPMENTOTHER_REVISION_RESPONSIBILITY"
                    Return EQUIPMENTOTHER_REVISION_RESPONSIBILITY
                Case "EQUIPMENTOTHER_REVISION_REVISIONNUMBER"
                    Return EQUIPMENTOTHER_REVISION_REVISIONNUMBER
                Case "EQUIPMENTOTHER_REVISION_STATUSTIMESTAMP"
                    Return EQUIPMENTOTHER_REVISION_STATUSTIMESTAMP
                Case "EQUIPMENTOTHER_REVISION_STATUSTYPE"
                    Return EQUIPMENTOTHER_REVISION_STATUSTYPE
                Case "EQUIPMENTOTHER_REVISION_STATUSVALUE"
                    Return EQUIPMENTOTHER_REVISION_STATUSVALUE
                Case "EQUIPMENTOTHER_REVISION_TEXT"
                    Return EQUIPMENTOTHER_REVISION_TEXT
                Case "EXCHANGER_ABSORBEDDUTY"
                    Return EXCHANGER_ABSORBEDDUTY
                Case "EXCHANGER_ABSORBEDDUTYUOM"
                    Return EXCHANGER_ABSORBEDDUTYUOM
                Case "EXCHANGER_CLEANINGREQMTSTUBE"
                    Return EXCHANGER_CLEANINGREQMTSTUBE
                Case "EXCHANGER_COATINGREQMTSTUBE"
                    Return EXCHANGER_COATINGREQMTSTUBE
                Case "EXCHANGER_CORROSIONALLOWANCETUBE"
                    Return EXCHANGER_CORROSIONALLOWANCETUBE
                Case "EXCHANGER_DRAFTTYPE"
                    Return EXCHANGER_DRAFTTYPE
                Case "EXCHANGER_EQUIPMENTORIENTATION"
                    Return EXCHANGER_EQUIPMENTORIENTATION
                Case "EXCHANGER_HEATTRANSFERAREAPERUNIT"
                    Return EXCHANGER_HEATTRANSFERAREAPERUNIT
                Case "EXCHANGER_HEATTRANSFERRATING"
                    Return EXCHANGER_HEATTRANSFERRATING
                Case "EXCHANGER_INSULATIONSPECTUBE"
                    Return EXCHANGER_INSULATIONSPECTUBE
                Case "EXCHANGER_MATERIALOFCONSTCLASSTUBE"
                    Return EXCHANGER_MATERIALOFCONSTCLASSTUBE
                Case "EXCHANGER_MOTORPOWERPERFAN"
                    Return EXCHANGER_MOTORPOWERPERFAN
                Case "EXCHANGER_NUMBEROFBAYS"
                    Return EXCHANGER_NUMBEROFBAYS
                Case "EXCHANGER_NUMBEROFBUNDLES"
                    Return EXCHANGER_NUMBEROFBUNDLES
                Case "EXCHANGER_NUMBEROFFANS"
                    Return EXCHANGER_NUMBEROFFANS
                Case "EXCHANGER_NUMBEROFTUBES"
                    Return EXCHANGER_NUMBEROFTUBES
                Case "EXCHANGER_PIPINGMATERIALSCLASSTUBE"
                    Return EXCHANGER_PIPINGMATERIALSCLASSTUBE
                Case "EXCHANGER_POWERABSORBEDPERFAN"
                    Return EXCHANGER_POWERABSORBEDPERFAN
                Case "EXCHANGER_RATEDDUTY"
                    Return EXCHANGER_RATEDDUTY
                Case "EXCHANGER_RATEDDUTYUOM"
                    Return EXCHANGER_RATEDDUTYUOM
                Case "EXCHANGER_REVISION_APPROVALTIMESTAMP"
                    Return EXCHANGER_REVISION_APPROVALTIMESTAMP
                Case "EXCHANGER_REVISION_APPROVEDBY"
                    Return EXCHANGER_REVISION_APPROVEDBY
                Case "EXCHANGER_REVISION_RESPONSIBILITY"
                    Return EXCHANGER_REVISION_RESPONSIBILITY
                Case "EXCHANGER_REVISION_REVISIONNUMBER"
                    Return EXCHANGER_REVISION_REVISIONNUMBER
                Case "EXCHANGER_REVISION_STATUSTIMESTAMP"
                    Return EXCHANGER_REVISION_STATUSTIMESTAMP
                Case "EXCHANGER_REVISION_STATUSTYPE"
                    Return EXCHANGER_REVISION_STATUSTYPE
                Case "EXCHANGER_REVISION_STATUSVALUE"
                    Return EXCHANGER_REVISION_STATUSVALUE
                Case "EXCHANGER_REVISION_TEXT"
                    Return EXCHANGER_REVISION_TEXT
                Case "EXCHANGER_SHELLDIAMETER"
                    Return EXCHANGER_SHELLDIAMETER
                Case "EXCHANGER_SHELLDIAMETERUOM"
                    Return EXCHANGER_SHELLDIAMETERUOM
                Case "EXCHANGER_TEMA_DESIGNATION"
                    Return EXCHANGER_TEMA_DESIGNATION
                Case "EXCHANGER_TUBELENGTH"
                    Return EXCHANGER_TUBELENGTH
                Case "EXCHANGER_TYPEOFLOUVERS"
                    Return EXCHANGER_TYPEOFLOUVERS
                Case "EXCHANGER_UNITWIDTH"
                    Return EXCHANGER_UNITWIDTH
                Case "MECHANICAL_CWPIPINGPLAN"
                    Return MECHANICAL_CWPIPINGPLAN
                Case "MECHANICAL_DIFFERENTIALPRESSURE"
                    Return MECHANICAL_DIFFERENTIALPRESSURE
                Case "MECHANICAL_DIFFERENTIALPRESSUREUOM"
                    Return MECHANICAL_DIFFERENTIALPRESSUREUOM
                Case "MECHANICAL_DRIVERRATEDPOWER"
                    Return MECHANICAL_DRIVERRATEDPOWER
                Case "MECHANICAL_DRIVERRATEDPOWERUOM"
                    Return MECHANICAL_DRIVERRATEDPOWERUOM
                Case "MECHANICAL_ELECTRICALREQMT"
                    Return MECHANICAL_ELECTRICALREQMT
                Case "MECHANICAL_MATERIALOFCONSTCLASSINTERNAL"
                    Return MECHANICAL_MATERIALOFCONSTCLASSINTERNAL
                Case "MECHANICAL_MECHRATING"
                    Return MECHANICAL_MECHRATING
                Case "MECHANICAL_POWERABSORBED"
                    Return MECHANICAL_POWERABSORBED
                Case "MECHANICAL_POWERCONSUMPTION"
                    Return MECHANICAL_POWERCONSUMPTION
                Case "MECHANICAL_RATEDCAPACITY"
                    Return MECHANICAL_RATEDCAPACITY
                Case "MECHANICAL_RATEDCAPACITYUOM"
                    Return MECHANICAL_RATEDCAPACITYUOM
                Case "MECHANICAL_RATEDDISCHARGEPRESSURE"
                    Return MECHANICAL_RATEDDISCHARGEPRESSURE
                Case "MECHANICAL_RATEDSUCTIONPRESSURE"
                    Return MECHANICAL_RATEDSUCTIONPRESSURE
                Case "MECHANICAL_REVISION_APPROVALTIMESTAMP"
                    Return MECHANICAL_REVISION_APPROVALTIMESTAMP
                Case "MECHANICAL_REVISION_APPROVEDBY"
                    Return MECHANICAL_REVISION_APPROVEDBY
                Case "MECHANICAL_REVISION_RESPONSIBILITY"
                    Return MECHANICAL_REVISION_RESPONSIBILITY
                Case "MECHANICAL_REVISION_REVISIONNUMBER"
                    Return MECHANICAL_REVISION_REVISIONNUMBER
                Case "MECHANICAL_REVISION_STATUSTIMESTAMP"
                    Return MECHANICAL_REVISION_STATUSTIMESTAMP
                Case "MECHANICAL_REVISION_STATUSTYPE"
                    Return MECHANICAL_REVISION_STATUSTYPE
                Case "MECHANICAL_REVISION_STATUSVALUE"
                    Return MECHANICAL_REVISION_STATUSVALUE
                Case "MECHANICAL_REVISION_TEXT"
                    Return MECHANICAL_REVISION_TEXT
                Case "MECHANICAL_SEALPIPINGPLAN"
                    Return MECHANICAL_SEALPIPINGPLAN
                Case "MECHANICAL_TYPEOFDRIVER"
                    Return MECHANICAL_TYPEOFDRIVER
                Case "VESSEL_DIAMETERINTERNAL"
                    Return VESSEL_DIAMETERINTERNAL
                Case "VESSEL_EQUIPMENTORIENTATION"
                    Return VESSEL_EQUIPMENTORIENTATION
                Case "VESSEL_LENGTHTANTOTAN"
                    Return VESSEL_LENGTHTANTOTAN
                Case "VESSEL_LENGTHTANTOTANUOM"
                    Return VESSEL_LENGTHTANTOTANUOM
                Case "VESSEL_LEVELREFERENCE"
                    Return VESSEL_LEVELREFERENCE
                Case "VESSEL_LEVELREFERENCEUOM"
                    Return VESSEL_LEVELREFERENCEUOM
                Case "VESSEL_LIQUIDLEVELHIGH"
                    Return VESSEL_LIQUIDLEVELHIGH
                Case "VESSEL_LIQUIDLEVELHIGHUOM"
                    Return VESSEL_LIQUIDLEVELHIGHUOM
                Case "VESSEL_LIQUIDLEVELHIGHHIGH"
                    Return VESSEL_LIQUIDLEVELHIGHHIGH
                Case "VESSEL_LIQUIDLEVELHIGHHIGHUOM"
                    Return VESSEL_LIQUIDLEVELHIGHHIGHUOM
                Case "VESSEL_LIQUIDLEVELLOW"
                    Return VESSEL_LIQUIDLEVELLOW
                Case "VESSEL_LIQUIDLEVELLOWUOM"
                    Return VESSEL_LIQUIDLEVELLOWUOM
                Case "VESSEL_LIQUIDLEVELLOWLOW"
                    Return VESSEL_LIQUIDLEVELLOWLOW
                Case "VESSEL_LIQUIDLEVELLOWLOWUOM"
                    Return VESSEL_LIQUIDLEVELLOWLOWUOM
                Case "VESSEL_LIQUIDLEVELNORMAL"
                    Return VESSEL_LIQUIDLEVELNORMAL
                Case "VESSEL_LIQUIDLEVELNORMALUOM"
                    Return VESSEL_LIQUIDLEVELNORMALUOM
                Case "VESSEL_LIQUIDLEVELOVERFLOW"
                    Return VESSEL_LIQUIDLEVELOVERFLOW
                Case "VESSEL_LIQUIDLEVELOVERFLOWUOM"
                    Return VESSEL_LIQUIDLEVELOVERFLOWUOM
                Case "VESSEL_MATERIALOFCONSTCLASSINTERNAL"
                    Return VESSEL_MATERIALOFCONSTCLASSINTERNAL
                Case "VESSEL_REVISION_APPROVALTIMESTAMP"
                    Return VESSEL_REVISION_APPROVALTIMESTAMP
                Case "VESSEL_REVISION_APPROVEDBY"
                    Return VESSEL_REVISION_APPROVEDBY
                Case "VESSEL_REVISION_RESPONSIBILITY"
                    Return VESSEL_REVISION_RESPONSIBILITY
                Case "VESSEL_REVISION_REVISIONNUMBER"
                    Return VESSEL_REVISION_REVISIONNUMBER
                Case "VESSEL_REVISION_STATUSTIMESTAMP"
                    Return VESSEL_REVISION_STATUSTIMESTAMP
                Case "VESSEL_REVISION_STATUSTYPE"
                    Return VESSEL_REVISION_STATUSTYPE
                Case "VESSEL_REVISION_STATUSVALUE"
                    Return VESSEL_REVISION_STATUSVALUE
                Case "VESSEL_REVISION_TEXT"
                    Return VESSEL_REVISION_TEXT
                Case "VESSEL_VOLUMERATING"
                    Return VESSEL_VOLUMERATING
                Case "VESSEL_VOLUMERATINGUOM"
                    Return VESSEL_VOLUMERATINGUOM
                Case "CLASS"
                    Return [CLASS]
                Case "CLEANINGREQMTS"
                    Return CLEANINGREQMTS
                Case "COATINGREQMTS"
                    Return COATINGREQMTS
                Case "CONSTRUCTIONBY"
                    Return CONSTRUCTIONBY
                Case "CONSTRUCTIONSTATUS"
                    Return CONSTRUCTIONSTATUS
                Case "CORROSIONALLOWANCE"
                    Return CORROSIONALLOWANCE
                Case "DESCRIPTION"
                    Return DESCRIPTION
                Case "DESIGNBY"
                    Return DESIGNBY
                Case "ERPASSETNO"
                    Return ERPASSETNO
                Case "EQUIPMENTSUBCLASS"
                    Return EQUIPMENTSUBCLASS
                Case "EQUIPMENTTYPE"
                    Return EQUIPMENTTYPE
                Case "FABRICATIONCATEGORY"
                    Return FABRICATIONCATEGORY
                Case "HTRACEMEDIUM"
                    Return HTRACEMEDIUM
                Case "HTRACEMEDIUMTEMP"
                    Return HTRACEMEDIUMTEMP
                Case "HTRACEREQMT"
                    Return HTRACEREQMT
                Case "HEIGHT"
                    Return HEIGHT
                Case "HEIGHTUOM"
                    Return HEIGHTUOM
                Case "HOLDSTATUS_APPROVALTIMESTAMP"
                    Return HOLDSTATUS_APPROVALTIMESTAMP
                Case "HOLDSTATUS_APPROVEDBY"
                    Return HOLDSTATUS_APPROVEDBY
                Case "HOLDSTATUS_RESPONSIBILITY"
                    Return HOLDSTATUS_RESPONSIBILITY
                Case "HOLDSTATUS_REVISIONNUMBER"
                    Return HOLDSTATUS_REVISIONNUMBER
                Case "HOLDSTATUS_STATUSTIMESTAMP"
                    Return HOLDSTATUS_STATUSTIMESTAMP
                Case "HOLDSTATUS_STATUSTYPE"
                    Return HOLDSTATUS_STATUSTYPE
                Case "HOLDSTATUS_STATUSVALUE"
                    Return HOLDSTATUS_STATUSVALUE
                Case "HOLDSTATUS_TEXT"
                    Return HOLDSTATUS_TEXT
                Case "HOLDSTATUS_UPDATECOUNT"
                    Return HOLDSTATUS_UPDATECOUNT
                Case "INSULDENSITY"
                    Return INSULDENSITY
                Case "INSULPURPOSE"
                    Return INSULPURPOSE
                Case "INSULTEMP"
                    Return INSULTEMP
                Case "INSULTHICK"
                    Return INSULTHICK
                Case "INSULTHICKUOM"
                    Return INSULTHICKUOM
                Case "INSULTYPE"
                    Return INSULTYPE
                Case "INSULATIONSPEC"
                    Return INSULATIONSPEC
                Case "INSULATIONTHKSOURCE"
                    Return INSULATIONTHKSOURCE
                Case "INVENTORYTAG"
                    Return INVENTORYTAG
                Case "ISBULKITEM"
                    Return ISBULKITEM
                Case "ISUNCHECKED"
                    Return ISUNCHECKED
                Case "ITEMSTATUS"
                    Return ITEMSTATUS
                Case "ITEMTAG"
                    Return ITEMTAG
                Case "ITEMTYPENAME"
                    Return ITEMTYPENAME
                Case "MATERIALOFCONSTCLASS"
                    Return MATERIALOFCONSTCLASS
                Case "MODELITEMTYPE"
                    Return MODELITEMTYPE
                Case "NAME"
                    Return NAME
                Case "PARTOFTYPE"
                    Return PARTOFTYPE
                Case "PIPINGMATERIALSCLASS"
                    Return PIPINGMATERIALSCLASS
                Case "PLANTITEMTYPE"
                    Return PLANTITEMTYPE
                Case "REQUISITIONBY"
                    Return REQUISITIONBY
                Case "REQUISITIONNO"
                    Return REQUISITIONNO
                Case "SLOPE"
                    Return SLOPE
                Case "SLOPERISE"
                    Return SLOPERISE
                Case "SLOPERUN"
                    Return SLOPERUN
                Case "STEAMOUTPRESSURE"
                    Return STEAMOUTPRESSURE
                Case "STEAMOUTREQMT"
                    Return STEAMOUTREQMT
                Case "STEAMOUTTEMPERATURE"
                    Return STEAMOUTTEMPERATURE
                Case "STRESSRELIEFREQMT"
                    Return STRESSRELIEFREQMT
                Case "SUPPLYBY"
                    Return SUPPLYBY
                Case "TAGPREFIX"
                    Return TAGPREFIX
                Case "TAGREQDFLAG"
                    Return TAGREQDFLAG
                Case "TAGSEQUENCENO"
                    Return TAGSEQUENCENO
                Case "TAGSUFFIX"
                    Return TAGSUFFIX
                Case "TRIMSPEC"
                    Return TRIMSPEC
                Case "UPDATECOUNT"
                    Return UPDATECOUNT
                Case "AABBCC_CODE"
                    Return AABBCC_CODE
                Case Else
                    Return Nothing
            End Select


        End Function

        Public Overridable Sub SetPropertyValue(propertyName As String, value As Object) Implements IDataObject.SetPropertyValue
            Select Case propertyName
                Case "SP_ID"
                    If value IsNot Nothing Then
                        SP_ID = Convert.ToString(value)
                    End If
                    Exit Select
                Case "ADAPTER_PARENTTAG"
                    If value IsNot Nothing Then
                        ADAPTER_PARENTTAG = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_DATECREATED"
                    If value IsNot Nothing Then
                        DRAWING_DATECREATED = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_DESCRIPTION"
                    If value IsNot Nothing Then
                        DRAWING_DESCRIPTION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_DOCUMENTCATEGORY"
                    If value IsNot Nothing Then
                        DRAWING_DOCUMENTCATEGORY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_DOCUMENTTYPE"
                    If value IsNot Nothing Then
                        DRAWING_DOCUMENTTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_DRAWINGNUMBER"
                    If value IsNot Nothing Then
                        DRAWING_DRAWINGNUMBER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_ITEMSTATUS"
                    If value IsNot Nothing Then
                        DRAWING_ITEMSTATUS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_NAME"
                    If value IsNot Nothing Then
                        DRAWING_NAME = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_PATH"
                    If value IsNot Nothing Then
                        DRAWING_PATH = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_REVISION"
                    If value IsNot Nothing Then
                        DRAWING_REVISION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_TEMPLATE"
                    If value IsNot Nothing Then
                        DRAWING_TEMPLATE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_TITLE"
                    If value IsNot Nothing Then
                        DRAWING_TITLE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DRAWING_VERSION"
                    If value IsNot Nothing Then
                        DRAWING_VERSION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "REPRESENTATION_INSTOCKPILE"
                    If value IsNot Nothing Then
                        REPRESENTATION_INSTOCKPILE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "SYMBOL_FILENAME"
                    If value IsNot Nothing Then
                        SYMBOL_FILENAME = Convert.ToString(value)
                    End If
                    Exit Select
                Case "SYMBOL_XCOORDINATE"
                    If value IsNot Nothing Then
                        SYMBOL_XCOORDINATE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "SYMBOL_YCOORDINATE"
                    If value IsNot Nothing Then
                        SYMBOL_YCOORDINATE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_ABSORBEDDUTY"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_ABSORBEDDUTY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_AREA"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_AREA = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_EQUIPMENTORIENTATION"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_EQUIPMENTORIENTATION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_RATEDDUTY"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_RATEDDUTY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_REVISION_APPROVALTIMESTAMP"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_REVISION_APPROVALTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_REVISION_APPROVEDBY"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_REVISION_APPROVEDBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_REVISION_RESPONSIBILITY"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_REVISION_RESPONSIBILITY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_REVISION_REVISIONNUMBER"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_REVISION_REVISIONNUMBER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_REVISION_STATUSTIMESTAMP"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_REVISION_STATUSTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_REVISION_STATUSTYPE"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_REVISION_STATUSTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_REVISION_STATUSVALUE"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_REVISION_STATUSVALUE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTOTHER_REVISION_TEXT"
                    If value IsNot Nothing Then
                        EQUIPMENTOTHER_REVISION_TEXT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_ABSORBEDDUTY"
                    If value IsNot Nothing Then
                        EXCHANGER_ABSORBEDDUTY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_ABSORBEDDUTYUOM"
                    If value IsNot Nothing Then
                        EXCHANGER_ABSORBEDDUTYUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_CLEANINGREQMTSTUBE"
                    If value IsNot Nothing Then
                        EXCHANGER_CLEANINGREQMTSTUBE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_COATINGREQMTSTUBE"
                    If value IsNot Nothing Then
                        EXCHANGER_COATINGREQMTSTUBE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_CORROSIONALLOWANCETUBE"
                    If value IsNot Nothing Then
                        EXCHANGER_CORROSIONALLOWANCETUBE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_DRAFTTYPE"
                    If value IsNot Nothing Then
                        EXCHANGER_DRAFTTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_EQUIPMENTORIENTATION"
                    If value IsNot Nothing Then
                        EXCHANGER_EQUIPMENTORIENTATION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_HEATTRANSFERAREAPERUNIT"
                    If value IsNot Nothing Then
                        EXCHANGER_HEATTRANSFERAREAPERUNIT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_HEATTRANSFERRATING"
                    If value IsNot Nothing Then
                        EXCHANGER_HEATTRANSFERRATING = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_INSULATIONSPECTUBE"
                    If value IsNot Nothing Then
                        EXCHANGER_INSULATIONSPECTUBE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_MATERIALOFCONSTCLASSTUBE"
                    If value IsNot Nothing Then
                        EXCHANGER_MATERIALOFCONSTCLASSTUBE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_MOTORPOWERPERFAN"
                    If value IsNot Nothing Then
                        EXCHANGER_MOTORPOWERPERFAN = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_NUMBEROFBAYS"
                    If value IsNot Nothing Then
                        EXCHANGER_NUMBEROFBAYS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_NUMBEROFBUNDLES"
                    If value IsNot Nothing Then
                        EXCHANGER_NUMBEROFBUNDLES = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_NUMBEROFFANS"
                    If value IsNot Nothing Then
                        EXCHANGER_NUMBEROFFANS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_NUMBEROFTUBES"
                    If value IsNot Nothing Then
                        EXCHANGER_NUMBEROFTUBES = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_PIPINGMATERIALSCLASSTUBE"
                    If value IsNot Nothing Then
                        EXCHANGER_PIPINGMATERIALSCLASSTUBE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_POWERABSORBEDPERFAN"
                    If value IsNot Nothing Then
                        EXCHANGER_POWERABSORBEDPERFAN = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_RATEDDUTY"
                    If value IsNot Nothing Then
                        EXCHANGER_RATEDDUTY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_RATEDDUTYUOM"
                    If value IsNot Nothing Then
                        EXCHANGER_RATEDDUTYUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_REVISION_APPROVALTIMESTAMP"
                    If value IsNot Nothing Then
                        EXCHANGER_REVISION_APPROVALTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_REVISION_APPROVEDBY"
                    If value IsNot Nothing Then
                        EXCHANGER_REVISION_APPROVEDBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_REVISION_RESPONSIBILITY"
                    If value IsNot Nothing Then
                        EXCHANGER_REVISION_RESPONSIBILITY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_REVISION_REVISIONNUMBER"
                    If value IsNot Nothing Then
                        EXCHANGER_REVISION_REVISIONNUMBER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_REVISION_STATUSTIMESTAMP"
                    If value IsNot Nothing Then
                        EXCHANGER_REVISION_STATUSTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_REVISION_STATUSTYPE"
                    If value IsNot Nothing Then
                        EXCHANGER_REVISION_STATUSTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_REVISION_STATUSVALUE"
                    If value IsNot Nothing Then
                        EXCHANGER_REVISION_STATUSVALUE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_REVISION_TEXT"
                    If value IsNot Nothing Then
                        EXCHANGER_REVISION_TEXT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_SHELLDIAMETER"
                    If value IsNot Nothing Then
                        EXCHANGER_SHELLDIAMETER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_SHELLDIAMETERUOM"
                    If value IsNot Nothing Then
                        EXCHANGER_SHELLDIAMETERUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_TEMA_DESIGNATION"
                    If value IsNot Nothing Then
                        EXCHANGER_TEMA_DESIGNATION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_TUBELENGTH"
                    If value IsNot Nothing Then
                        EXCHANGER_TUBELENGTH = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_TYPEOFLOUVERS"
                    If value IsNot Nothing Then
                        EXCHANGER_TYPEOFLOUVERS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EXCHANGER_UNITWIDTH"
                    If value IsNot Nothing Then
                        EXCHANGER_UNITWIDTH = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_CWPIPINGPLAN"
                    If value IsNot Nothing Then
                        MECHANICAL_CWPIPINGPLAN = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_DIFFERENTIALPRESSURE"
                    If value IsNot Nothing Then
                        MECHANICAL_DIFFERENTIALPRESSURE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_DIFFERENTIALPRESSUREUOM"
                    If value IsNot Nothing Then
                        MECHANICAL_DIFFERENTIALPRESSUREUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_DRIVERRATEDPOWER"
                    If value IsNot Nothing Then
                        MECHANICAL_DRIVERRATEDPOWER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_DRIVERRATEDPOWERUOM"
                    If value IsNot Nothing Then
                        MECHANICAL_DRIVERRATEDPOWERUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_ELECTRICALREQMT"
                    If value IsNot Nothing Then
                        MECHANICAL_ELECTRICALREQMT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_MATERIALOFCONSTCLASSINTERNAL"
                    If value IsNot Nothing Then
                        MECHANICAL_MATERIALOFCONSTCLASSINTERNAL = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_MECHRATING"
                    If value IsNot Nothing Then
                        MECHANICAL_MECHRATING = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_POWERABSORBED"
                    If value IsNot Nothing Then
                        MECHANICAL_POWERABSORBED = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_POWERCONSUMPTION"
                    If value IsNot Nothing Then
                        MECHANICAL_POWERCONSUMPTION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_RATEDCAPACITY"
                    If value IsNot Nothing Then
                        MECHANICAL_RATEDCAPACITY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_RATEDCAPACITYUOM"
                    If value IsNot Nothing Then
                        MECHANICAL_RATEDCAPACITYUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_RATEDDISCHARGEPRESSURE"
                    If value IsNot Nothing Then
                        MECHANICAL_RATEDDISCHARGEPRESSURE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_RATEDSUCTIONPRESSURE"
                    If value IsNot Nothing Then
                        MECHANICAL_RATEDSUCTIONPRESSURE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_REVISION_APPROVALTIMESTAMP"
                    If value IsNot Nothing Then
                        MECHANICAL_REVISION_APPROVALTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_REVISION_APPROVEDBY"
                    If value IsNot Nothing Then
                        MECHANICAL_REVISION_APPROVEDBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_REVISION_RESPONSIBILITY"
                    If value IsNot Nothing Then
                        MECHANICAL_REVISION_RESPONSIBILITY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_REVISION_REVISIONNUMBER"
                    If value IsNot Nothing Then
                        MECHANICAL_REVISION_REVISIONNUMBER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_REVISION_STATUSTIMESTAMP"
                    If value IsNot Nothing Then
                        MECHANICAL_REVISION_STATUSTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_REVISION_STATUSTYPE"
                    If value IsNot Nothing Then
                        MECHANICAL_REVISION_STATUSTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_REVISION_STATUSVALUE"
                    If value IsNot Nothing Then
                        MECHANICAL_REVISION_STATUSVALUE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_REVISION_TEXT"
                    If value IsNot Nothing Then
                        MECHANICAL_REVISION_TEXT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_SEALPIPINGPLAN"
                    If value IsNot Nothing Then
                        MECHANICAL_SEALPIPINGPLAN = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MECHANICAL_TYPEOFDRIVER"
                    If value IsNot Nothing Then
                        MECHANICAL_TYPEOFDRIVER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_DIAMETERINTERNAL"
                    If value IsNot Nothing Then
                        VESSEL_DIAMETERINTERNAL = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_EQUIPMENTORIENTATION"
                    If value IsNot Nothing Then
                        VESSEL_EQUIPMENTORIENTATION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LENGTHTANTOTAN"
                    If value IsNot Nothing Then
                        VESSEL_LENGTHTANTOTAN = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LENGTHTANTOTANUOM"
                    If value IsNot Nothing Then
                        VESSEL_LENGTHTANTOTANUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LEVELREFERENCE"
                    If value IsNot Nothing Then
                        VESSEL_LEVELREFERENCE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LEVELREFERENCEUOM"
                    If value IsNot Nothing Then
                        VESSEL_LEVELREFERENCEUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELHIGH"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELHIGH = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELHIGHUOM"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELHIGHUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELHIGHHIGH"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELHIGHHIGH = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELHIGHHIGHUOM"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELHIGHHIGHUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELLOW"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELLOW = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELLOWUOM"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELLOWUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELLOWLOW"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELLOWLOW = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELLOWLOWUOM"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELLOWLOWUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELNORMAL"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELNORMAL = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELNORMALUOM"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELNORMALUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELOVERFLOW"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELOVERFLOW = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_LIQUIDLEVELOVERFLOWUOM"
                    If value IsNot Nothing Then
                        VESSEL_LIQUIDLEVELOVERFLOWUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_MATERIALOFCONSTCLASSINTERNAL"
                    If value IsNot Nothing Then
                        VESSEL_MATERIALOFCONSTCLASSINTERNAL = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_REVISION_APPROVALTIMESTAMP"
                    If value IsNot Nothing Then
                        VESSEL_REVISION_APPROVALTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_REVISION_APPROVEDBY"
                    If value IsNot Nothing Then
                        VESSEL_REVISION_APPROVEDBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_REVISION_RESPONSIBILITY"
                    If value IsNot Nothing Then
                        VESSEL_REVISION_RESPONSIBILITY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_REVISION_REVISIONNUMBER"
                    If value IsNot Nothing Then
                        VESSEL_REVISION_REVISIONNUMBER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_REVISION_STATUSTIMESTAMP"
                    If value IsNot Nothing Then
                        VESSEL_REVISION_STATUSTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_REVISION_STATUSTYPE"
                    If value IsNot Nothing Then
                        VESSEL_REVISION_STATUSTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_REVISION_STATUSVALUE"
                    If value IsNot Nothing Then
                        VESSEL_REVISION_STATUSVALUE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_REVISION_TEXT"
                    If value IsNot Nothing Then
                        VESSEL_REVISION_TEXT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_VOLUMERATING"
                    If value IsNot Nothing Then
                        VESSEL_VOLUMERATING = Convert.ToString(value)
                    End If
                    Exit Select
                Case "VESSEL_VOLUMERATINGUOM"
                    If value IsNot Nothing Then
                        VESSEL_VOLUMERATINGUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "CLASS"
                    If value IsNot Nothing Then
                        [CLASS] = Convert.ToString(value)
                    End If
                    Exit Select
                Case "CLEANINGREQMTS"
                    If value IsNot Nothing Then
                        CLEANINGREQMTS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "COATINGREQMTS"
                    If value IsNot Nothing Then
                        COATINGREQMTS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "CONSTRUCTIONBY"
                    If value IsNot Nothing Then
                        CONSTRUCTIONBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "CONSTRUCTIONSTATUS"
                    If value IsNot Nothing Then
                        CONSTRUCTIONSTATUS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "CORROSIONALLOWANCE"
                    If value IsNot Nothing Then
                        CORROSIONALLOWANCE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DESCRIPTION"
                    If value IsNot Nothing Then
                        DESCRIPTION = Convert.ToString(value)
                    End If
                    Exit Select
                Case "DESIGNBY"
                    If value IsNot Nothing Then
                        DESIGNBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "ERPASSETNO"
                    If value IsNot Nothing Then
                        ERPASSETNO = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTSUBCLASS"
                    If value IsNot Nothing Then
                        EQUIPMENTSUBCLASS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "EQUIPMENTTYPE"
                    If value IsNot Nothing Then
                        EQUIPMENTTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "FABRICATIONCATEGORY"
                    If value IsNot Nothing Then
                        FABRICATIONCATEGORY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HTRACEMEDIUM"
                    If value IsNot Nothing Then
                        HTRACEMEDIUM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HTRACEMEDIUMTEMP"
                    If value IsNot Nothing Then
                        HTRACEMEDIUMTEMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HTRACEREQMT"
                    If value IsNot Nothing Then
                        HTRACEREQMT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HEIGHT"
                    If value IsNot Nothing Then
                        HEIGHT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HEIGHTUOM"
                    If value IsNot Nothing Then
                        HEIGHTUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_APPROVALTIMESTAMP"
                    If value IsNot Nothing Then
                        HOLDSTATUS_APPROVALTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_APPROVEDBY"
                    If value IsNot Nothing Then
                        HOLDSTATUS_APPROVEDBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_RESPONSIBILITY"
                    If value IsNot Nothing Then
                        HOLDSTATUS_RESPONSIBILITY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_REVISIONNUMBER"
                    If value IsNot Nothing Then
                        HOLDSTATUS_REVISIONNUMBER = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_STATUSTIMESTAMP"
                    If value IsNot Nothing Then
                        HOLDSTATUS_STATUSTIMESTAMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_STATUSTYPE"
                    If value IsNot Nothing Then
                        HOLDSTATUS_STATUSTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_STATUSVALUE"
                    If value IsNot Nothing Then
                        HOLDSTATUS_STATUSVALUE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_TEXT"
                    If value IsNot Nothing Then
                        HOLDSTATUS_TEXT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "HOLDSTATUS_UPDATECOUNT"
                    If value IsNot Nothing Then
                        HOLDSTATUS_UPDATECOUNT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INSULDENSITY"
                    If value IsNot Nothing Then
                        INSULDENSITY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INSULPURPOSE"
                    If value IsNot Nothing Then
                        INSULPURPOSE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INSULTEMP"
                    If value IsNot Nothing Then
                        INSULTEMP = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INSULTHICK"
                    If value IsNot Nothing Then
                        INSULTHICK = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INSULTHICKUOM"
                    If value IsNot Nothing Then
                        INSULTHICKUOM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INSULTYPE"
                    If value IsNot Nothing Then
                        INSULTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INSULATIONSPEC"
                    If value IsNot Nothing Then
                        INSULATIONSPEC = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INSULATIONTHKSOURCE"
                    If value IsNot Nothing Then
                        INSULATIONTHKSOURCE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "INVENTORYTAG"
                    If value IsNot Nothing Then
                        INVENTORYTAG = Convert.ToString(value)
                    End If
                    Exit Select
                Case "ISBULKITEM"
                    If value IsNot Nothing Then
                        ISBULKITEM = Convert.ToString(value)
                    End If
                    Exit Select
                Case "ISUNCHECKED"
                    If value IsNot Nothing Then
                        ISUNCHECKED = Convert.ToString(value)
                    End If
                    Exit Select
                Case "ITEMSTATUS"
                    If value IsNot Nothing Then
                        ITEMSTATUS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "ITEMTAG"
                    If value IsNot Nothing Then
                        ITEMTAG = Convert.ToString(value)
                    End If
                    Exit Select
                Case "ITEMTYPENAME"
                    If value IsNot Nothing Then
                        ITEMTYPENAME = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MATERIALOFCONSTCLASS"
                    If value IsNot Nothing Then
                        MATERIALOFCONSTCLASS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "MODELITEMTYPE"
                    If value IsNot Nothing Then
                        MODELITEMTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "NAME"
                    If value IsNot Nothing Then
                        NAME = Convert.ToString(value)
                    End If
                    Exit Select
                Case "PARTOFTYPE"
                    If value IsNot Nothing Then
                        PARTOFTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "PIPINGMATERIALSCLASS"
                    If value IsNot Nothing Then
                        PIPINGMATERIALSCLASS = Convert.ToString(value)
                    End If
                    Exit Select
                Case "PLANTITEMTYPE"
                    If value IsNot Nothing Then
                        PLANTITEMTYPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "REQUISITIONBY"
                    If value IsNot Nothing Then
                        REQUISITIONBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "REQUISITIONNO"
                    If value IsNot Nothing Then
                        REQUISITIONNO = Convert.ToString(value)
                    End If
                    Exit Select
                Case "SLOPE"
                    If value IsNot Nothing Then
                        SLOPE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "SLOPERISE"
                    If value IsNot Nothing Then
                        SLOPERISE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "SLOPERUN"
                    If value IsNot Nothing Then
                        SLOPERUN = Convert.ToString(value)
                    End If
                    Exit Select
                Case "STEAMOUTPRESSURE"
                    If value IsNot Nothing Then
                        STEAMOUTPRESSURE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "STEAMOUTREQMT"
                    If value IsNot Nothing Then
                        STEAMOUTREQMT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "STEAMOUTTEMPERATURE"
                    If value IsNot Nothing Then
                        STEAMOUTTEMPERATURE = Convert.ToString(value)
                    End If
                    Exit Select
                Case "STRESSRELIEFREQMT"
                    If value IsNot Nothing Then
                        STRESSRELIEFREQMT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "SUPPLYBY"
                    If value IsNot Nothing Then
                        SUPPLYBY = Convert.ToString(value)
                    End If
                    Exit Select
                Case "TAGPREFIX"
                    If value IsNot Nothing Then
                        TAGPREFIX = Convert.ToString(value)
                    End If
                    Exit Select
                Case "TAGREQDFLAG"
                    If value IsNot Nothing Then
                        TAGREQDFLAG = Convert.ToString(value)
                    End If
                    Exit Select
                Case "TAGSEQUENCENO"
                    If value IsNot Nothing Then
                        TAGSEQUENCENO = Convert.ToString(value)
                    End If
                    Exit Select
                Case "TAGSUFFIX"
                    If value IsNot Nothing Then
                        TAGSUFFIX = Convert.ToString(value)
                    End If
                    Exit Select
                Case "TRIMSPEC"
                    If value IsNot Nothing Then
                        TRIMSPEC = Convert.ToString(value)
                    End If
                    Exit Select
                Case "UPDATECOUNT"
                    If value IsNot Nothing Then
                        UPDATECOUNT = Convert.ToString(value)
                    End If
                    Exit Select
                Case "AABBCC_CODE"
                    If value IsNot Nothing Then
                        AABBCC_CODE = Convert.ToString(value)
                    End If
                    Exit Select
            End Select
        End Sub
    End Class
End Namespace

