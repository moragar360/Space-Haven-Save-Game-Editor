' Helper class to hold item details including category
Public Class CategorizedItem
    Private _category As String
    Private _itemName As String

    Public Property Category As String
        Get
            Return SpaceHavenEditor2.Localization.T(_category)
        End Get
        Set(value As String)
            _category = value
        End Set
    End Property

    Public Property ItemName As String
        Get
            Return SpaceHavenEditor2.Localization.T(_itemName)
        End Get
        Set(value As String)
            _itemName = value
        End Set
    End Property

    Public Property ItemId As Integer

    Public Sub New(cat As String, name As String, id As Integer)
        Category = cat
        ItemName = name
        ItemId = id
    End Sub
End Class