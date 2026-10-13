Imports System.ComponentModel

Public Class DatalogUnlockItem
    Implements INotifyPropertyChanged

    Private _isSelected As Boolean
    Private _isUnlocked As Boolean

    Public Property DatalogId As Integer
    Public Property DatalogName As String

    Public ReadOnly Property DisplayName As String
        Get
            Return SpaceHavenEditor2.Localization.T(If(DatalogName, ""))
        End Get
    End Property

    Public Property IsSelected As Boolean
        Get
            Return _isSelected
        End Get
        Set(value As Boolean)
            If _isSelected = value Then Return
            _isSelected = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(IsSelected)))
        End Set
    End Property

    Public Property IsUnlocked As Boolean
        Get
            Return _isUnlocked
        End Get
        Set(value As Boolean)
            If _isUnlocked = value Then Return
            _isUnlocked = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(IsUnlocked)))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Status)))
        End Set
    End Property

    Public ReadOnly Property Status As String
        Get
            Return If(IsUnlocked, SpaceHavenEditor2.Localization.T("Unlocked"), SpaceHavenEditor2.Localization.T("Locked"))
        End Get
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class
