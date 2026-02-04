Imports MaterialDesignThemes.Wpf
Imports System.Windows.Input ' Required for MouseButtonEventArgs

Namespace SpaceHavenEditor2



    Public Class UpdateShipSizeWindow
        Inherits Window

        ' Public properties to return the CALCULATED Sx/Sy values
        Public Property ShipWidth As Integer  ' This will hold Sx (squares * 28)
        Public Property ShipHeight As Integer ' This will hold Sy (squares * 28)

        ' Optional: Store the current size in squares if needed for display
        Private currentWidthSquares As Integer
        Private currentHeightSquares As Integer

        ' Modified constructor to potentially accept current Sx/Sy
        Public Sub New(Optional currentSx As Integer = 0, Optional currentSy As Integer = 0)
            InitializeComponent()

            ' Calculate current size in squares (minimum 1)
            ' Use Math.Ceiling if you want fractional squares to round up, Round for nearest.
            ' Ensure division by floating point number (28.0) for accuracy before rounding/ceiling.
            currentWidthSquares = Math.Max(1, CInt(Math.Round(currentSx / 28.0)))
            currentHeightSquares = Math.Max(1, CInt(Math.Round(currentSy / 28.0)))

            ' Display current size in squares in the text boxes
            txtWidth.Text = currentWidthSquares.ToString()
            txtHeight.Text = currentHeightSquares.ToString()

            txtWidth.Focus() ' Set focus to the first input box
            txtWidth.SelectAll()
        End Sub

        ' Handler to allow dragging the borderless window
        Private Sub TitleBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            If e.ButtonState = MouseButtonState.Pressed Then
                Me.DragMove()
            End If
        End Sub

        ' Handler for the Update button - VALIDATES SQUARES (1-10) and CALCULATES Sx/Sy
        Private Sub Update_Click(sender As Object, e As RoutedEventArgs)
            Dim squaresWidth As Integer
            Dim squaresHeight As Integer

            ' Validate Width (Squares)
            If Not Integer.TryParse(txtWidth.Text, squaresWidth) OrElse squaresWidth < 1 OrElse squaresWidth > 10 Then
                MessageBox.Show("Please enter a whole number between 1 and 10 for Width (Squares).", "Invalid Width", MessageBoxButton.OK, MessageBoxImage.Warning)
                txtWidth.Focus()
                txtWidth.SelectAll()
                Return ' Stop processing if invalid
            End If

            ' Validate Height (Squares)
            If Not Integer.TryParse(txtHeight.Text, squaresHeight) OrElse squaresHeight < 1 OrElse squaresHeight > 10 Then
                MessageBox.Show("Please enter a whole number between 1 and 10 for Height (Squares).", "Invalid Height", MessageBoxButton.OK, MessageBoxImage.Warning)
                txtHeight.Focus()
                txtHeight.SelectAll()
                Return ' Stop processing if invalid
            End If

            ' Validation passed, CALCULATE Sx/Sy and store in properties
            ShipWidth = squaresWidth * 28
            ShipHeight = squaresHeight * 28

            ' Set DialogResult to True to indicate success and close the window
            Me.DialogResult = True
        End Sub

        ' No explicit Cancel_Click needed if IsCancel="True" is set on the button in XAML

    End Class


End Namespace