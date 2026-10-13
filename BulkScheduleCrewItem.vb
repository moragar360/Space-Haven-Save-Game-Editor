Public Class BulkScheduleCrewItem
    Public Property IsSelected As Boolean
    Public Property Crew As Character
    Public Property ScheduleDisplayName As String

    Public ReadOnly Property DisplayName As String
        Get
            Return Crew?.CharacterName
        End Get
    End Property

    Public ReadOnly Property CurrentSchedule As String
        Get
            If Crew Is Nothing Then Return SpaceHavenEditor2.Localization.T("Unknown")
            If Crew.UsesGlobalSchedule Then
                If Not String.IsNullOrWhiteSpace(ScheduleDisplayName) Then Return ScheduleDisplayName
                Return SpaceHavenEditor2.Localization.F("Global Schedule {0}", Crew.GlobalScheduleId)
            End If
            Return SpaceHavenEditor2.Localization.T("Custom Schedule")
        End Get
    End Property
End Class
