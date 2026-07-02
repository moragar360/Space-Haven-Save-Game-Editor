Imports System.Xml.Linq

Public Class FactionReputationInfo
    Public Property FactionName As String
    Public Property Relationship As Integer
    Public Property Stance As String
    Public Property Patience As Integer
    Public Property AccessTrade As Boolean
    Public Property AccessShip As Boolean
    Public Property AccessVision As Boolean
    Public Property AccessServices As Boolean
    Public Property AccessHire As Boolean
    Public Property SettlementDebt As Integer
    Public Property SourceElement As XElement
End Class
