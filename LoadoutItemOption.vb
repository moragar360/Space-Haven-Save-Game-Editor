Public Class LoadoutItemOption
    Public Property ItemId As Integer
    Public Property ItemName As String

    Public ReadOnly Property DisplayName As String
        Get
            Return If(ItemId = 0, "None", $"{ItemName} ({ItemId})")
        End Get
    End Property
End Class
