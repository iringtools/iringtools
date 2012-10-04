Imports System.Collections.Generic
Imports System.Linq
Imports System.Web


Public Class JsonContainer(Of T)
    Public Property items() As T
        Get
            Return m_items
        End Get
        Set(value As T)
            m_items = value
        End Set
    End Property
    Private m_items As T
    Public Property message() As String
        Get
            Return m_message
        End Get
        Set(value As String)
            m_message = value
        End Set
    End Property
    Private m_message As String
    Public Property success() As [Boolean]
        Get
            Return m_success
        End Get
        Set(value As [Boolean])
            m_success = value
        End Set
    End Property
    Private m_success As [Boolean]
    Public Property total() As Integer
        Get
            Return m_total
        End Get
        Set(value As Integer)
            m_total = value
        End Set
    End Property
    Private m_total As Integer
    Public Property errors() As String
        Get
            Return m_errors
        End Get
        Set(value As String)
            m_errors = value
        End Set
    End Property
    Private m_errors As String
End Class

