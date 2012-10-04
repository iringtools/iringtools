Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Public Class JsonTreeNode

    Public Sub New()
        hidden = False
        iconCls = String.Empty
    End Sub

    Public Property [property]() As Dictionary(Of String, String)
        Get
            Return m_property
        End Get
        Set(ByVal value As Dictionary(Of String, String))
            m_property = Value
        End Set
    End Property
    Private m_property As Dictionary(Of String, String)
    Public Property id() As String
        Get
            Return m_id
        End Get
        Set(ByVal value As String)
            m_id = Value
        End Set
    End Property
    Private m_id As String
    Public Property identifier() As String
        Get
            Return m_identifier
        End Get
        Set(ByVal value As String)
            m_identifier = Value
        End Set
    End Property
    Private m_identifier As String
    Public Property text() As String
        Get
            Return m_text
        End Get
        Set(ByVal value As String)
            m_text = Value
        End Set
    End Property
    Private m_text As String
    Public Property icon() As String
        Get
            Return m_icon
        End Get
        Set(ByVal value As String)
            m_icon = Value
        End Set
    End Property
    Private m_icon As String
    Public Property leaf() As Boolean
        Get
            Return m_leaf
        End Get
        Set(ByVal value As Boolean)
            m_leaf = Value
        End Set
    End Property
    Private m_leaf As Boolean
    Public Property expanded() As Boolean
        Get
            Return m_expanded
        End Get
        Set(ByVal value As Boolean)
            m_expanded = Value
        End Set
    End Property
    Private m_expanded As Boolean
    Public Property hidden() As Boolean
        Get
            Return m_hidden
        End Get
        Set(ByVal value As Boolean)
            m_hidden = Value
        End Set
    End Property
    Private m_hidden As Boolean
    Public Property children() As List(Of JsonTreeNode)
        Get
            Return m_children
        End Get
        Set(ByVal value As List(Of JsonTreeNode))
            m_children = Value
        End Set
    End Property
    Private m_children As List(Of JsonTreeNode)
    Public Property type() As String
        Get
            Return m_type
        End Get
        Set(ByVal value As String)
            m_type = Value
        End Set
    End Property
    Private m_type As String
    Public Property nodeType() As String
        Get
            Return m_nodeType
        End Get
        Set(ByVal value As String)
            m_nodeType = Value
        End Set
    End Property
    Private m_nodeType As String
    Public Property checked() As Object
        Get
            Return m_checked
        End Get
        Set(ByVal value As Object)
            m_checked = Value
        End Set
    End Property
    Private m_checked As Object
    Public Property record() As Object
        Get
            Return m_record
        End Get
        Set(ByVal value As Object)
            m_record = Value
        End Set
    End Property
    Private m_record As Object
    Public Property properties() As Dictionary(Of String, String)
        Get
            Return m_properties
        End Get
        Set(ByVal value As Dictionary(Of String, String))
            m_properties = Value
        End Set
    End Property
    Private m_properties As Dictionary(Of String, String)
    Public Property iconCls() As String
        Get
            Return m_iconCls
        End Get
        Set(ByVal value As String)
            m_iconCls = Value
        End Set
    End Property
    Private m_iconCls As String

End Class
