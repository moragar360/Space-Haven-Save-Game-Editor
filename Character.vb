Imports System.Data
Imports System.ComponentModel

Public Class Character
    Implements INotifyPropertyChanged

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private _shirtColorIndex As String
    Private _pantsColorIndex As String
    Private _shirtColorIndexValue As Integer
    Private _pantsColorIndexValue As Integer

    Public Property CharacterName As String
    Public Property CharacterEntityId As Integer
    Public Property ShipSid As Integer
    Public Property CharacterStats As New List(Of DataProp)
    Public Property CharacterAttributes As New List(Of DataProp)
    Public Property CharacterSkills As New List(Of DataProp)
    Public Property CharacterTraits As New List(Of DataProp)

    Public Property CharacterConditions As New List(Of DataProp)

    Public Property CharacterRelationships As New List(Of RelationshipInfo)
    
    ' Uniform properties with change notifications
    Public Property ShirtColorIndex As String  ' shirtSet value
        Get
            Return _shirtColorIndex
        End Get
        Set(value As String)
            If _shirtColorIndex <> value Then
                _shirtColorIndex = value
                OnPropertyChanged(NameOf(ShirtColorIndex))
                OnPropertyChanged(NameOf(ShirtColorName))
            End If
        End Set
    End Property
    
    Public Property PantsColorIndex As String  ' pantsSet value
        Get
            Return _pantsColorIndex
        End Get
        Set(value As String)
            If _pantsColorIndex <> value Then
                _pantsColorIndex = value
                OnPropertyChanged(NameOf(PantsColorIndex))
                OnPropertyChanged(NameOf(PantsColorName))
            End If
        End Set
    End Property
    
    Public Property ShirtColorIndexValue As Integer  ' sp value (index within shirt set)
        Get
            Return _shirtColorIndexValue
        End Get
        Set(value As Integer)
            If _shirtColorIndexValue <> value Then
                _shirtColorIndexValue = value
                OnPropertyChanged(NameOf(ShirtColorIndexValue))
                OnPropertyChanged(NameOf(ShirtColorName))
            End If
        End Set
    End Property
    
    Public Property PantsColorIndexValue As Integer  ' sl value (index within pants set)
        Get
            Return _pantsColorIndexValue
        End Get
        Set(value As Integer)
            If _pantsColorIndexValue <> value Then
                _pantsColorIndexValue = value
                OnPropertyChanged(NameOf(PantsColorIndexValue))
                OnPropertyChanged(NameOf(PantsColorName))
            End If
        End Set
    End Property
    
    Public Property SkinSet As String
    Public Property GlovesOff As Boolean
    Public Property LongSleeve As Boolean
    
    ' Computed properties for display (now with texture number mapping applied)
    Public ReadOnly Property ShirtColorName As String
        Get
            Dim setId As Integer
            If Integer.TryParse(ShirtColorIndex, setId) Then
                Return IdCollection.GetColorName(setId, ShirtColorIndexValue)
            End If
            Return "Unknown"
        End Get
    End Property
    
    Public ReadOnly Property PantsColorName As String
        Get
            Dim setId As Integer
            If Integer.TryParse(PantsColorIndex, setId) Then
                Return IdCollection.GetColorName(setId, PantsColorIndexValue)
            End If
            Return "Unknown"
        End Get
    End Property
    
    Protected Sub OnPropertyChanged(propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
    
    Public Sub New()
        CharacterAttributes = New List(Of DataProp)
        CharacterSkills = New List(Of DataProp)
        CharacterTraits = New List(Of DataProp)
        CharacterConditions = New List(Of DataProp)
        CharacterRelationships = New List(Of RelationshipInfo)

    End Sub

End Class
