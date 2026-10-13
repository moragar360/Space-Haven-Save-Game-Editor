Public Class LoadoutItemOption
    Public Property ItemId As Integer
    Public Property ItemName As String

    Public ReadOnly Property DisplayName As String
        Get
            Return If(ItemId = 0, SpaceHavenEditor2.Localization.T("None"), String.Format("{0} ({1})", SpaceHavenEditor2.Localization.T(ItemName), ItemId))
        End Get
    End Property
End Class
