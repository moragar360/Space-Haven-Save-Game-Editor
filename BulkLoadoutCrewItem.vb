Public Class BulkLoadoutCrewItem
    Public Property IsSelected As Boolean
    Public Property Crew As Character
    Public Property CurrentLoadout As String

    Public ReadOnly Property DisplayName As String
        Get
            Return Crew?.CharacterName
        End Get
    End Property
End Class
