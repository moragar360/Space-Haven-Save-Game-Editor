Imports System.Collections.ObjectModel

Public Class GlobalScheduleDefinition
    Public Property ScheduleId As Integer
    Public Property ScheduleName As String
    Public Property Slots As New ObservableCollection(Of CrewScheduleSlot)

    Public ReadOnly Property DisplayName As String
        Get
            If String.IsNullOrWhiteSpace(ScheduleName) Then
                Return $"Global Schedule {ScheduleId}"
            End If
            Return $"{ScheduleName} (Schedule {ScheduleId})"
        End Get
    End Property
End Class
