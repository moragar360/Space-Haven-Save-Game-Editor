Imports System.Windows.Input ' Required for MouseButtonEventArgs
Namespace SpaceHavenEditor2
    Public Class AboutWindow
    Inherits Window

    Public Sub New()
        InitializeComponent()
        AddHandler Loaded, Sub(sender, e) ApplyCredits()
        AddHandler Localization.LanguageChanged, AddressOf ApplyCredits
        AddHandler Closed, Sub(sender, e) RemoveHandler Localization.LanguageChanged, AddressOf ApplyCredits
    End Sub

    Private Sub ApplyCredits()
        Title = Localization.T("About Space Haven Editor")
        txtProductName.Text = Localization.T("Moragar's Space Haven Save Game Editor")
        txtAuthor.Text = Localization.T("Created by Moragar")
        txtLocalization.Text = Localization.T("Localization by GuguguBear")
        txtVersion.Text = Localization.T("v2.1 - Last Updated - 07/02/2026")
        btnClose.Content = Localization.T("OK")
    End Sub

    ' Handler to allow dragging the borderless window
    Private Sub TitleBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
        If e.ButtonState = MouseButtonState.Pressed Then
            Me.DragMove()
        End If
    End Sub

    ' Handler for the OK button
    Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
        Me.DialogResult = True ' Or just Me.Close()
    End Sub

End Class


End Namespace