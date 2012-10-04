
Public Class TableProperties

#Region " Private Variables "

    Private _name As String
    Private _schema As String
    Private _schemaType As String
    Private _alias As String
    Private _columns As Dictionary(Of String, ColumnProperties)
    ' string is the column name (no schema or table name)
    ' because of this, the column collection is not designed to stand on it's own - it must belong to a table

#End Region

#Region " Properties "

    Public ReadOnly Property Name As String
        Get
            Return _Name
        End Get
    End Property

    Public ReadOnly Property Schema As String
        Get
            Return _schema
        End Get
    End Property

    Public ReadOnly Property SchemaType As String
        Get
            Return _schemaType
        End Get
    End Property

    Public ReadOnly Property QueryAlias As String
        Get
            Return _alias
        End Get
    End Property

    Public ReadOnly Property Columns As Dictionary(Of String, ColumnProperties)
        Get
            Return _columns
        End Get
    End Property

#End Region

#Region " Constructors "

    Public Sub New(Name As String, Schema As String, SchemaType As String, _
                   QueryAlias As String, Columns As Dictionary(Of String, ColumnProperties))

        _name = Name
        _schema = Schema
        _schemaType = SchemaType
        _alias = QueryAlias
        _columns = Columns

    End Sub

#End Region


End Class

Public Class ColumnProperties

#Region " Private Variables "

    ' for integer values, -1 indicates null
    Private _name As String
    Private _tableAlias As String
    Private _isNullable As Boolean
    Private _dataType As SqlDbType
    Private _charMaxLength As Integer
    Private _numericPrecision As Integer
    Private _numericPrecisionRadix As Integer
    Private _numericScale As Integer
    Private _dateTimePrecision As Integer
    Private _characterSetName As String    ' this is generally either 'Unicode' or null
    Private _collationName As String        ' string in the format layed of for the COLLATION_NAME field in information_schema.columns in SQL Server

#End Region

#Region " Properties "

    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property TableAlias As String
        Get
            Return _tableAlias
        End Get
    End Property

    Public ReadOnly Property IsNullable As Boolean
        Get
            Return _isNullable
        End Get
    End Property

    Public ReadOnly Property DataType As SqlDbType
        Get
            Return _dataType
        End Get
    End Property

    Public ReadOnly Property CharacterMaxLength As Integer
        Get
            Return _charMaxLength
        End Get
    End Property

    Public ReadOnly Property NumericPrecision As Integer
        Get
            Return _numericPrecision
        End Get
    End Property

    Public ReadOnly Property NumericPrecisionRadix As Integer
        Get
            Return _numericPrecisionRadix
        End Get
    End Property

    Public ReadOnly Property NumericScale As Integer
        Get
            Return _numericScale
        End Get
    End Property

    Public ReadOnly Property DateTimePrecision As Integer
        Get
            Return _dateTimePrecision
        End Get
    End Property

    Public ReadOnly Property CharacterSetName As String
        Get
            Return _characterSetName
        End Get
    End Property

    Public ReadOnly Property CollationName As String
        Get
            Return _collationName
        End Get
    End Property

#End Region

#Region " Constructors "

    Public Sub New(Name As String, TableAlias As String, IsNullable As String, DataType As SqlDbType, CharacterMaxLength As String, _
                   NumericPrecision As Integer, NumericPrecisionRadix As Integer, NumericScale As Integer, _
                   DateTimePrecision As Integer, CharacterSetName As String, CollationName As String)

        _name = Name
        _tableAlias = TableAlias
        _isNullable = IsNullable
        _dataType = DataType
        _charMaxLength = CharacterMaxLength
        _numericPrecision = NumericPrecision
        _numericPrecisionRadix = NumericPrecisionRadix
        _numericScale = NumericScale
        _dateTimePrecision = DateTimePrecision
        _characterSetName = CharacterSetName
        _collationName = CollationName


    End Sub

#End Region


End Class

Public Class SQLSchemaX

    Public Property DatabaseName As String

    Private _Tables As Dictionary(Of String, TableProperties)


End Class
