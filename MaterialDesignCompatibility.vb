' Legacy source files still import this namespace, but no Material Design types are used.
' Keeping the empty namespace avoids shipping the unused URL-bearing assemblies.
Namespace Global.MaterialDesignThemes
    Namespace Wpf
        Friend NotInheritable Class LocalCompatibilityMarker
            Private Sub New()
            End Sub
        End Class
    End Namespace
End Namespace
