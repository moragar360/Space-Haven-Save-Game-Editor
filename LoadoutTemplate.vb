Public Class LoadoutTemplate
    Public Property Name As String
    Public Property Headgear As Integer
    Public Property Armor As Integer
    Public Property Primary As Integer
    Public Property Attachment As Integer
    Public Property Secondary As Integer
    Public Property Pocket1 As Integer
    Public Property Pocket2 As Integer
    Public Property Pocket3 As Integer
    Public Property BestQualityArmor As Boolean = True
    Public Property BestQualityPrimary As Boolean = True

    Public Function Clone(Optional newName As String = Nothing) As LoadoutTemplate
        Return New LoadoutTemplate With {
            .Name = If(newName, Name),
            .Headgear = Headgear,
            .Armor = Armor,
            .Primary = Primary,
            .Attachment = Attachment,
            .Secondary = Secondary,
            .Pocket1 = Pocket1,
            .Pocket2 = Pocket2,
            .Pocket3 = Pocket3,
            .BestQualityArmor = BestQualityArmor,
            .BestQualityPrimary = BestQualityPrimary
        }
    End Function

    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class
