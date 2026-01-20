Imports System.ComponentModel

Public Class Resource
    Implements INotifyPropertyChanged
    Private _id As String
    Private _count As Integer
    Public Property Id As String
        Get
            Return _id
        End Get
        Set(value As String)
            _id = value
            OnPropertyChanged("Id")
        End Set
    End Property
    Public Property Count As Integer
        Get
            Return _count
        End Get
        Set(value As Integer)
            _count = value
            OnPropertyChanged("Count")
        End Set
    End Property
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class

Public Class CrewMember
    Implements INotifyPropertyChanged
    Private _id As String
    Private _name As String
    Private _shirtSet As String
    Private _shirtColorIndex As String
    Private _pantsSet As String
    Private _pantsColorIndex As String
    Public Property Id As String
        Get
            Return _id
        End Get
        Set(value As String)
            _id = value
            OnPropertyChanged("Id")
        End Set
    End Property
    Public Property Name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
            OnPropertyChanged("Name")
        End Set
    End Property
    Public Property ShirtSet As String
        Get
            Return _shirtSet
        End Get
        Set(value As String)
            _shirtSet = value
            OnPropertyChanged("ShirtSet")
        End Set
    End Property
    Public Property ShirtColorIndex As String
        Get
            Return _shirtColorIndex
        End Get
        Set(value As String)
            _shirtColorIndex = value
            OnPropertyChanged("ShirtColorIndex")
        End Set
    End Property
    Public Property PantsSet As String
        Get
            Return _pantsSet
        End Get
        Set(value As String)
            _pantsSet = value
            OnPropertyChanged("PantsSet")
        End Set
    End Property
    Public Property PantsColorIndex As String
        Get
            Return _pantsColorIndex
        End Get
        Set(value As String)
            _pantsColorIndex = value
            OnPropertyChanged("PantsColorIndex")
        End Set
    End Property
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
End Class
