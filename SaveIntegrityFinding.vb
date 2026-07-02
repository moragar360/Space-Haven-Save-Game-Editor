Imports System.ComponentModel
Imports System.Xml.Linq

Public Enum SaveIntegritySeverity
    Information
    Warning
    Critical
End Enum

Public Enum SaveIntegrityRepairKind
    None
    RemoveElements
    ClampRelationshipValues
    RaiseCounter
End Enum

Public Class SaveIntegrityFinding
    Implements INotifyPropertyChanged

    Private _isSelected As Boolean

    Public Property Severity As SaveIntegritySeverity
    Public Property Category As String
    Public Property AffectedObject As String
    Public Property Description As String
    Public Property RepairDescription As String
    Public Property RepairKind As SaveIntegrityRepairKind
    Public Property SourceElement As XElement
    Public Property ElementsToRemove As New List(Of XElement)
    Public Property CounterAttributeName As String
    Public Property CounterValue As Long

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

    Public ReadOnly Property SeverityText As String
        Get
            Return Severity.ToString()
        End Get
    End Property

    Public ReadOnly Property CanRepair As Boolean
        Get
            Return RepairKind <> SaveIntegrityRepairKind.None
        End Get
    End Property

    Public ReadOnly Property RepairText As String
        Get
            Return If(CanRepair, RepairDescription, "Review manually")
        End Get
    End Property

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class
