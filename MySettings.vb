Imports System.IO
Imports System.Text.Json
Imports Microsoft.VisualBasic.HideModuleNameAttribute

Namespace My
    ''' <summary>
    ''' User settings for this fork. The upstream repository referenced a generated
    ''' My.Settings designer that was not included in the published source.
    ''' </summary>
    Public Class EditorSettings
        Public Property BackupOnOpen As Boolean = True
        Public Property DefaultSaveDirectory As String = ""
        Public Property DefaultScenarioDirectory As String = ""
        Public Property Language As String = ""

        Private Shared _default As EditorSettings

        Public Shared ReadOnly Property [Default] As EditorSettings
            Get
                If _default Is Nothing Then _default = Load()
                Return _default
            End Get
        End Property

        Public Sub Save()
            Dim settingsFile = SettingsPath()
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(settingsFile))
            File.WriteAllText(settingsFile, JsonSerializer.Serialize(Me, New JsonSerializerOptions With {.WriteIndented = True}))
        End Sub

        Private Shared Function Load() As EditorSettings
            Try
                Dim path = SettingsPath()
                If File.Exists(path) Then
                    Dim loaded = JsonSerializer.Deserialize(Of EditorSettings)(File.ReadAllText(path))
                    If loaded IsNot Nothing Then Return loaded
                End If
            Catch
            End Try
            Return New EditorSettings()
        End Function

        Private Shared Function SettingsPath() As String
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "SpaceHavenSaveEditor",
                                "settings.json")
        End Function
    End Class

    <HideModuleName>
    Friend Module MySettingsProperty
        Friend ReadOnly Property Settings As EditorSettings
            Get
                Return EditorSettings.Default
            End Get
        End Property
    End Module
End Namespace
