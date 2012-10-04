Imports System
Imports System.Data
Imports System.Data.Common
Namespace DataUtils

    Public Class DataReaderAdapter
        Inherits DbDataAdapter

        Public Function FillFromReader(ByVal dataTable As DataTable, ByVal dataReader As IDataReader) As Integer
            Return Me.Fill(dataTable, dataReader)
        End Function

        Protected Overloads Overrides Function CreateRowUpdatedEvent(ByVal dataRow As DataRow, ByVal command As IDbCommand, ByVal statementType As StatementType, ByVal tableMapping As DataTableMapping) As RowUpdatedEventArgs
            Return Nothing
        End Function

        Protected Overloads Overrides Function CreateRowUpdatingEvent(ByVal dataRow As DataRow, ByVal command As IDbCommand, ByVal statementType As StatementType, ByVal tableMapping As DataTableMapping) As RowUpdatingEventArgs
            Return Nothing
        End Function

        Protected Overloads Overrides Sub OnRowUpdated(ByVal value As RowUpdatedEventArgs)
        End Sub

        Protected Overloads Overrides Sub OnRowUpdating(ByVal value As RowUpdatingEventArgs)
        End Sub
    End Class
End Namespace