Public Class CrewRecoveryVital
    Public Property VitalName As String
    Public Property CurrentValue As String
    Public Property LongTermValue As String
    Public Property Details As String

    Public ReadOnly Property DisplayVital As String
        Get
            Return SpaceHavenEditor2.Localization.T(If(VitalName, ""))
        End Get
    End Property

    Public ReadOnly Property DisplayDetails As String
        Get
            If String.IsNullOrEmpty(Details) Then Return ""
            Const reservePrefix = "Reserve: "
            If Details.StartsWith(reservePrefix) Then
                Return SpaceHavenEditor2.Localization.F("Reserve: {0}", Details.Substring(reservePrefix.Length))
            End If
            Return SpaceHavenEditor2.Localization.T(Details)
        End Get
    End Property
End Class
