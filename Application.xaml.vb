Class Application

    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        SpaceHavenEditor2.Localization.Initialize()
        MyBase.OnStartup(e)
    End Sub

End Class
