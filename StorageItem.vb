Public Class StorageItem
    Public Property ElementId As Integer
    Public Property Quantity As Integer

    Public ReadOnly Property Name As String
        Get
            If IdCollection.DefaultStorageIDs.ContainsKey(ElementId) Then
                Return SpaceHavenEditor2.Localization.T(IdCollection.DefaultStorageIDs(ElementId))
            Else
                Return SpaceHavenEditor2.Localization.F("Unknown Item ({0})", ElementId)
            End If
        End Get
    End Property

    ' Optional: Constructor for convenience
    Public Sub New(id As Integer, qty As Integer)
        ElementId = id
        Quantity = qty
    End Sub
End Class
