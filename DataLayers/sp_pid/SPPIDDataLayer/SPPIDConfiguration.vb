'''<remarks/>
<System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225"), _
 System.Diagnostics.DebuggerStepThroughAttribute(), _
 System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True), _
 System.Xml.Serialization.XmlRootAttribute([Namespace]:="", IsNullable:=False)> _
Partial Public Class configuration

  Private commoditiesField() As configurationCommodity

  '''<remarks/>
  <System.Xml.Serialization.XmlArrayItemAttribute("commodity", IsNullable:=False)> _
  Public Property commodities() As configurationCommodity()
    Get
      Return Me.commoditiesField
    End Get
    Set(value As configurationCommodity())
      Me.commoditiesField = Value
    End Set
  End Property
End Class
'''<remarks/>
<System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225"), _
 System.Diagnostics.DebuggerStepThroughAttribute(), _
 System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True)> _
Partial Public Class configurationCommodity

  Private attributesField() As configurationCommodityAttribute

  Private nameField As String

  '''<remarks/>
  <System.Xml.Serialization.XmlArrayItemAttribute("attribute", IsNullable:=False)> _
  Public Property attributes() As configurationCommodityAttribute()
    Get
      Return Me.attributesField
    End Get
    Set(value As configurationCommodityAttribute())
      Me.attributesField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property name() As String
    Get
      Return Me.nameField
    End Get
    Set(value As String)
      Me.nameField = Value
    End Set
  End Property
End Class
'''<remarks/>
<System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.225"), _
 System.Diagnostics.DebuggerStepThroughAttribute(), _
 System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True)> _
Partial Public Class configurationCommodityAttribute

  Private nameField As String

  Private displayNameField As String

  Private nativeNameField As String

  Private datatypeField As String

  Private lengthField As UShort

  Private lengthFieldSpecified As Boolean

  Private isKeyField As String

  Private readonlyField As String

  Private appSourceField As String

  Private subclassField As String

  Private parseField As String

  Private receiveScopeField As String

  Private symbolKeyField As String

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property name() As String
    Get
      Return Me.nameField
    End Get
    Set(value As String)
      Me.nameField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property displayName() As String
    Get
      Return Me.displayNameField
    End Get
    Set(value As String)
      Me.displayNameField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property nativeName() As String
    Get
      Return Me.nativeNameField
    End Get
    Set(value As String)
      Me.nativeNameField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property datatype() As String
    Get
      Return Me.datatypeField
    End Get
    Set(value As String)
      Me.datatypeField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property length() As UShort
    Get
      Return Me.lengthField
    End Get
    Set(value As UShort)
      Me.lengthField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlIgnoreAttribute()> _
  Public Property lengthSpecified() As Boolean
    Get
      Return Me.lengthFieldSpecified
    End Get
    Set(value As Boolean)
      Me.lengthFieldSpecified = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property isKey() As String
    Get
      Return Me.isKeyField
    End Get
    Set(value As String)
      Me.isKeyField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property [readonly]() As String
    Get
      Return Me.readonlyField
    End Get
    Set(value As String)
      Me.readonlyField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property appSource() As String
    Get
      Return Me.appSourceField
    End Get
    Set(value As String)
      Me.appSourceField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property subclass() As String
    Get
      Return Me.subclassField
    End Get
    Set(value As String)
      Me.subclassField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property parse() As String
    Get
      Return Me.parseField
    End Get
    Set(value As String)
      Me.parseField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property receiveScope() As String
    Get
      Return Me.receiveScopeField
    End Get
    Set(value As String)
      Me.receiveScopeField = Value
    End Set
  End Property

  '''<remarks/>
  <System.Xml.Serialization.XmlAttributeAttribute()> _
  Public Property symbolKey() As String
    Get
      Return Me.symbolKeyField
    End Get
    Set(value As String)
      Me.symbolKeyField = Value
    End Set
  End Property
End Class



