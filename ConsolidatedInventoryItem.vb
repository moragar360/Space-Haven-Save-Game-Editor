Public Class ConsolidatedInventoryItem
    Public Property ElementId As Integer
    Public Property TotalQuantity As Integer
    Public Property ContainerCount As Integer
    Public Property Locations As String

    Public ReadOnly Property Name As String
        Get
            If IdCollection.DefaultStorageIDs.ContainsKey(ElementId) Then
                Return IdCollection.DefaultStorageIDs(ElementId)
            End If
            Return $"Unknown Item ({ElementId})"
        End Get
    End Property
End Class
