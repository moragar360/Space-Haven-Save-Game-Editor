Public Class DataProp
    Public Property Id As Integer
    Public Property Name As String
    Public Property Value As Integer

    Public Property MaxValue As Integer

    Public ReadOnly Property DisplayName As String
        Get
            Return SpaceHavenEditor2.Localization.T(If(Name, ""))
        End Get
    End Property
End Class
