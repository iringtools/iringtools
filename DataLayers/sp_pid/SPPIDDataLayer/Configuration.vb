Imports System.Collections.Generic
Imports System.Xml.Serialization
Imports System.Runtime.Serialization
Imports System.Linq
Imports System.Text



<DataContract(Name:="document")> _
Public Class SPPIDConfiguration
    <DataMember(Name:="siteConnectionString", Order:=0)> _
    Public Property SiteConnectionString() As String
        Get
            Return m_SiteConnectionString
        End Get
        Set(value As String)
            m_SiteConnectionString = value
        End Set
    End Property
    Private m_SiteConnectionString As String

    <DataMember(Name:="plantConnectionString", Order:=1)> _
    Public Property PlantConnectionString() As String
        Get
            Return m_PlantConnectionString
        End Get
        Set(value As String)
            m_PlantConnectionString = value
        End Set
    End Property
    Private m_PlantConnectionString As String
    <DataMember(Name:="plantDataDicConnectionString", Order:=2)> _
    Public Property PlantDataDicConnectionString() As String
        Get
            Return m_PlantDataDicConnectionString
        End Get
        Set(value As String)
            m_PlantDataDicConnectionString = value
        End Set
    End Property
    Private m_PlantDataDicConnectionString As String

    <DataMember(Name:="pidConnectionString", Order:=3)> _
    Public Property PIDConnectionString() As String
        Get
            Return m_PIDConnectionString
        End Get
        Set(value As String)
            m_PIDConnectionString = value
        End Set
    End Property
    Private m_PIDConnectionString As String

    <DataMember(Name:="pidDataDicConnectionString", Order:=4)> _
    Public Property PIDDataDicConnectionString() As String
        Get
            Return m_PIDDataDicConnectionString
        End Get
        Set(value As String)
            m_PIDDataDicConnectionString = value
        End Set
    End Property
    Private m_PIDDataDicConnectionString As String


    <DataMember(Name:="stagingConnectionString", Order:=5)> _
    Public Property StagingConnectionString() As String
        Get
            Return m_StagingConnectionString
        End Get
        Set(value As String)
            m_StagingConnectionString = value
        End Set
    End Property
    Private m_StagingConnectionString As String

    <DataMember(Name:="provider", Order:=2)> _
    Public Property Provider() As String
        Get
            Return m_Provider
        End Get
        Set(value As String)
            m_Provider = value
        End Set
    End Property
    Private m_Provider As String





End Class



