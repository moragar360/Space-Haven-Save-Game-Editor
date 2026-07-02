Imports System.Windows.Input ' Required for MouseButtonEventArgs
Imports System.Windows.Documents ' Required for Inlines (Run, Bold, LineBreak)
Imports System.Windows.Media ' Required for Brushes


Namespace SpaceHavenEditor2



    Public Class HelpWindow
        Inherits Window

        ' Constructor that accepts the help text string
        Public Sub New(helpText As String)
            InitializeComponent()
            BuildAndDisplayHelpDocument(helpText) ' Call the new method
        End Sub

        Private Sub BuildAndDisplayHelpDocument(helpText As String)
            Dim flowDoc As New FlowDocument()
            flowDoc.FontFamily = New FontFamily("Segoe UI")
            flowDoc.FontSize = 13
            flowDoc.Foreground = New SolidColorBrush(Color.FromRgb(&HE9, &HE4, &HDC))
            ' Removed fixed LineHeight, let Paragraph/List spacing handle it

            Dim lines = helpText.Split({Environment.NewLine}, StringSplitOptions.None)
            Dim currentList As List = Nothing ' Track if we are inside a list
            Dim isInsideDisclaimer As Boolean = False ' Track if we are processing disclaimer body

            For Each line As String In lines
                Dim trimmedLine = line.Trim()

                ' --- Handle Blank Lines ---
                If String.IsNullOrWhiteSpace(trimmedLine) Then
                    ' End the current list if we were in one before the blank line
                    If currentList IsNot Nothing Then
                        flowDoc.Blocks.Add(currentList)
                        currentList = Nothing
                    End If
                    isInsideDisclaimer = False ' Blank line ends disclaimer section
                    ' Don't add empty paragraphs for blank lines, let margins handle spacing
                    Continue For
                End If

                ' --- Handle Headers and Disclaimer Start/End ---
                If trimmedLine.StartsWith("===") AndAlso trimmedLine.EndsWith("===") Then ' Main Title
                    If currentList IsNot Nothing Then flowDoc.Blocks.Add(currentList) : currentList = Nothing : isInsideDisclaimer = False
                    Dim p As New Paragraph(New Bold(New Run(trimmedLine.Trim("= ")))) With {
                        .FontSize = 22,
                        .FontWeight = FontWeights.Bold, ' Explicitly Bold
                        .Margin = New Thickness(0, 0, 0, 15) ' More bottom margin
                    }
                    flowDoc.Blocks.Add(p)
                ElseIf trimmedLine.StartsWith("---") AndAlso trimmedLine.EndsWith("---") Then ' Section Header
                    If currentList IsNot Nothing Then flowDoc.Blocks.Add(currentList) : currentList = Nothing : isInsideDisclaimer = False
                    Dim p As New Paragraph(New Bold(New Run(trimmedLine.Trim("- ")))) With {
                        .FontSize = 16,
                        .FontWeight = FontWeights.SemiBold,
                        .Margin = New Thickness(0, 15, 0, 8) ' More top/bottom margin
                    }
                    flowDoc.Blocks.Add(p)
                ElseIf trimmedLine.StartsWith("***") AndAlso trimmedLine.EndsWith("***") Then ' Disclaimer Header/Footer
                    If currentList IsNot Nothing Then flowDoc.Blocks.Add(currentList) : currentList = Nothing
                    Dim p As New Paragraph(New Bold(New Run(trimmedLine.Trim("* ")))) With {
                        .Foreground = Brushes.DarkRed,
                        .TextAlignment = TextAlignment.Center,
                        .Margin = New Thickness(0, 5, 0, 5) ' Reduced margin slightly
                    }
                    flowDoc.Blocks.Add(p)
                    ' If it's the start disclaimer, set the flag
                    isInsideDisclaimer = (trimmedLine = "*** DISCLAIMER ***")

                    ' --- Handle Bullet Points ---
                ElseIf trimmedLine.StartsWith("- ") Then ' Main bullet point
                    isInsideDisclaimer = False ' Bullets end disclaimer section
                    If currentList Is Nothing Then
                        currentList = New List() With {
                            .MarkerStyle = TextMarkerStyle.Disc,
                            .Padding = New Thickness(25, 0, 0, 0), ' Increased Indentation slightly
                            .Margin = New Thickness(0, 0, 0, 5) ' Space below list
                        }
                    End If
                    Dim listItemParagraph As New Paragraph(New Run(trimmedLine.TrimStart("- ")))
                    listItemParagraph.Margin = New Thickness(0, 0, 0, 3) ' Small margin below each list item text
                    currentList.ListItems.Add(New ListItem(listItemParagraph))

                    ' --- Handle WARNING ---
                ElseIf line.Contains("**WARNING:**") Then
                    isInsideDisclaimer = False
                    If currentList IsNot Nothing Then flowDoc.Blocks.Add(currentList) : currentList = Nothing
                    Dim p As New Paragraph() With {.Margin = New Thickness(10, 5, 0, 5)}
                    Dim warningBold = New Bold(New Run("WARNING:")) With {.Foreground = Brushes.OrangeRed}
                    Dim warningText = New Run(line.Replace("**WARNING:**", "").Trim()) ' Use Trim()
                    p.Inlines.Add(warningBold)
                    p.Inlines.Add(New Run(" ")) ' Add space after bold part
                    p.Inlines.Add(warningText)
                    flowDoc.Blocks.Add(p)

                    ' --- Handle Normal Text (Including Disclaimer Body) ---
                Else
                    If currentList IsNot Nothing Then
                        flowDoc.Blocks.Add(currentList) ' Finish the list before normal text
                        currentList = Nothing
                    End If
                    ' Create paragraph, center if it's part of the disclaimer
                    Dim p As New Paragraph(New Run(line)) ' Use original line to preserve leading spaces if any (except for disclaimer)
                    If isInsideDisclaimer Then
                        p.TextAlignment = TextAlignment.Center
                        p.Margin = New Thickness(0, 0, 0, 0) ' No extra margin between disclaimer lines
                    Else
                        p.Margin = New Thickness(0, 0, 0, 5) ' Standard bottom margin for normal text
                    End If
                    flowDoc.Blocks.Add(p)
                End If
            Next

            ' Add the last list if it wasn't closed
            If currentList IsNot Nothing Then
                flowDoc.Blocks.Add(currentList)
            End If

            ' Assign the created FlowDocument to the viewer
            fdViewer.Document = flowDoc
        End Sub


        ' Handler to allow dragging the borderless window
        Private Sub TitleBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            If e.ButtonState = MouseButtonState.Pressed Then
                Me.DragMove()
            End If
        End Sub

        ' Handler for the Close button
        Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
            Me.Close() ' Close the help window
        End Sub

    End Class




End Namespace
