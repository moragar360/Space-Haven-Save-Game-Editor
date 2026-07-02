Public Class CrewScheduleSlot
    Public Property HourIndex As Integer
    Public Property Activity As Integer

    Public ReadOnly Property HourLabel As String
        Get
            Return $"{HourIndex:00}:00 - {(HourIndex + 1) Mod 24:00}:00"
        End Get
    End Property
End Class
