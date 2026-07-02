Public Class Ship
    Public Property Sid As Integer
    Public Property Sname As String
    Public Property Sx As Integer
    Public Property Sy As Integer
    Public Property Owner As String
    Public Property State As String
    Public Property IsStation As Boolean
    Public Property IsStationSaveAnchor As Boolean
    Public Property IsPlayerOwned As Boolean
    Public Property IsDerelict As Boolean
    Public Property StructureType As String
    Public Property StationType As String
    Public Property Rotation As String
    Public Property StationMaterials As String
    Public Property StationOptions As String
    Public Property CrewCount As Integer
    Public Property StorageContainerCount As Integer

    Public ReadOnly Property DisplayName As String
        Get
            If Sid = -1 Then Return "-- Select Ship --"
            Return $"[{StructureType}] {Sname}"
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return DisplayName
    End Function

    Public Property StorageItems As New List(Of StorageItem)
End Class
