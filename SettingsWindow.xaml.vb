Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Diagnostics
Imports Ookii.Dialogs.Wpf

Namespace SpaceHavenEditor2

    Class SettingsWindow
        Inherits Window

        ' Public property to get the final setting value for backup
        Public ReadOnly Property BackupSetting As Boolean
            Get
                Return chkBackup.IsChecked.GetValueOrDefault(False)
            End Get
        End Property

        ' Public property to get the final setting value for the default directory
        Public ReadOnly Property DefaultDirectory As String
            Get
                Return txtDefaultDirectory.Text
            End Get
        End Property

        Public Sub New()
            InitializeComponent()

            ' Load the saved default directory or set a fallback
            Dim savedDirectory As String = My.Settings.DefaultSaveDirectory
            If String.IsNullOrEmpty(savedDirectory) OrElse Not Directory.Exists(savedDirectory) Then
                txtDefaultDirectory.Text = GetDefaultSpaceHavenDirectory()
            Else
                txtDefaultDirectory.Text = savedDirectory
            End If

            ' Load the backup setting
            chkBackup.IsChecked = My.Settings.BackupOnOpen
        End Sub

        ' Method for MainWindow to set the initial value for the backup setting
        Public Sub SetInitialValue(currentValue As Boolean)
            chkBackup.IsChecked = currentValue
        End Sub

        ' Method to set the initial value for the default directory
        Public Sub SetInitialDirectory(currentValue As String)
            If String.IsNullOrEmpty(currentValue) OrElse Not Directory.Exists(currentValue) Then
                txtDefaultDirectory.Text = GetDefaultSpaceHavenDirectory()
            Else
                txtDefaultDirectory.Text = currentValue
            End If
        End Sub

        ' Debug window loading to check button visibility
        Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
            Debug.WriteLine($"SettingsWindow Loaded. btnBrowseDirectory Visibility: {btnBrowseDirectory.Visibility}, IsEnabled: {btnBrowseDirectory.IsEnabled}")
            Debug.WriteLine($"btnBrowseDirectory Width: {btnBrowseDirectory.ActualWidth}, Height: {btnBrowseDirectory.ActualHeight}")
            Debug.WriteLine($"btnBrowseDirectory Content: {btnBrowseDirectory.Content}, Style: {If(btnBrowseDirectory.Style Is Nothing, "None", "Set")}")
            If btnBrowseDirectory.Parent IsNot Nothing Then
                Debug.WriteLine($"btnBrowseDirectory Parent: {btnBrowseDirectory.Parent.GetType().Name}")
            End If
        End Sub

        ' Browse Button Click Handler
        Private Sub btnBrowseDirectory_Click(sender As Object, e As RoutedEventArgs)
            Debug.WriteLine("btnBrowseDirectory_Click triggered")
            ' Determine the starting path
            Dim startPath As String = If(Directory.Exists(txtDefaultDirectory.Text),
                                       txtDefaultDirectory.Text,
                                       GetDefaultSpaceHavenDirectory())

            ' Instantiate the WPF folder picker
            Dim dlg As New VistaFolderBrowserDialog With {
                .Description = "Select your Space Haven save-game folder",
                .SelectedPath = startPath,
                .UseDescriptionForTitle = True
            }

            ' Show dialog and process result
            Dim result As Boolean? = dlg.ShowDialog()
            If result = True AndAlso Directory.Exists(dlg.SelectedPath) Then
                txtDefaultDirectory.Text = dlg.SelectedPath
            Else
                ' Revert to a valid directory if cancelled or invalid
                Dim saved As String = My.Settings.DefaultSaveDirectory
                txtDefaultDirectory.Text = If(Directory.Exists(saved),
                                           saved,
                                           GetDefaultSpaceHavenDirectory())
            End If
        End Sub

        ' OK Button Click Handler
        Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs)
            ' Save backup setting
            My.Settings.BackupOnOpen = chkBackup.IsChecked.GetValueOrDefault(False)

            ' Save default directory if valid, otherwise use fallback
            If Directory.Exists(txtDefaultDirectory.Text) Then
                My.Settings.DefaultSaveDirectory = txtDefaultDirectory.Text
            Else
                My.Settings.DefaultSaveDirectory = GetDefaultSpaceHavenDirectory()
                txtDefaultDirectory.Text = My.Settings.DefaultSaveDirectory
                MessageBox.Show("The selected directory is invalid. Reverted to default Space Haven save directory.",
                              "Invalid Directory",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning)
            End If

            My.Settings.Save()
            Me.DialogResult = True
            Me.Close()
        End Sub

        ' Cancel Button Click Handler
        Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs)
            Me.DialogResult = False
            Me.Close()
        End Sub

        ' Get the default Space Haven save directory or a fallback
        Private Function GetDefaultSpaceHavenDirectory() As String
            ' Try Space Haven's default save path
            Dim localAppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            Dim spaceHavenPath As String = Path.Combine(localAppData, "Low", "Bugbyte Oy", "Space Haven", "Saves")
            If Directory.Exists(spaceHavenPath) Then
                Return spaceHavenPath
            End If

            ' Fallback to My Documents
            Dim myDocuments As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            If Directory.Exists(myDocuments) Then
                Return myDocuments
            End If

            ' Ultimate fallback: current directory
            Return Environment.CurrentDirectory
        End Function

    End Class

End Namespace