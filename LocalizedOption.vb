Namespace SpaceHavenEditor2
    Public Class LocalizedOption
        Public Sub New(value As String)
            Me.Value = value
        End Sub

        Public Property Value As String

        Public ReadOnly Property Display As String
            Get
                Return Localization.T(Value)
            End Get
        End Property
    End Class

    Public Class ScheduleActivityOption
        Public Sub New(key As Integer, englishName As String)
            Me.Key = key
            Me.EnglishName = englishName
        End Sub

        Public Property Key As Integer
        Public Property EnglishName As String

        Public ReadOnly Property Value As String
            Get
                Return Localization.T(EnglishName)
            End Get
        End Property
    End Class

    Public Class IdLabel
        Public Sub New(key As Integer, value As String)
            Me.Key = key
            Me.Value = value
        End Sub

        Public Property Key As Integer
        Public Property Value As String

        Public ReadOnly Property Display As String
            Get
                Return Localization.T(Value)
            End Get
        End Property
    End Class
End Namespace
