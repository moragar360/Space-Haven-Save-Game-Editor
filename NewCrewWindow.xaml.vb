Imports System.Collections.ObjectModel
Imports System.Windows.Data
Imports System.Windows.Input


Namespace SpaceHavenEditor2

    Class NewCrewWindow
        Inherits Window

        Public Property NewCrewName As String = "New Recruit"
        Public Attributes As List(Of DataProp)
        Public Skills As List(Of DataProp)
        Public Traits As ObservableCollection(Of DataProp)
        Public Property SelectedTemplateCharacterId As Integer = -1
        Private AvailableTraits As List(Of KeyValuePair(Of Integer, String))

        Public Sub New(availableCharacters As List(Of Character))
            InitializeComponent()
            
            ' Populate template character ComboBox
            If availableCharacters IsNot Nothing AndAlso availableCharacters.Count > 0 Then
                cmbTemplateCharacter.ItemsSource = availableCharacters
                cmbTemplateCharacter.DisplayMemberPath = "CharacterName"
                cmbTemplateCharacter.SelectedValuePath = "CharacterEntityId"
                cmbTemplateCharacter.SelectedIndex = 0 ' Select first by default
            Else
                cmbTemplateCharacter.IsEnabled = False
            End If

            Attributes = IdCollection.DefaultAttributeIDs _
                         .Select(Function(kvp) New DataProp With {.Id = kvp.Key, .Name = kvp.Value, .Value = 1}) _
                         .OrderBy(Function(dp) dp.Name).ToList()
            Skills = IdCollection.DefaultSkillIDs _
                  .Select(Function(kvp) New DataProp With {.Id = kvp.Key, .Name = kvp.Value, .Value = 0}) _
                  .OrderBy(Function(dp) dp.Id).ToList()
            Traits = New ObservableCollection(Of DataProp)()

            dgvNewAttributes.ItemsSource = Attributes
            dgvNewSkills.ItemsSource = Skills

            AvailableTraits = IdCollection.DefaultTraitIDs.OrderBy(Function(kvp) kvp.Value).ToList()
            cmbAvailableTraits.ItemsSource = AvailableTraits
            If cmbAvailableTraits.Items.Count > 0 Then cmbAvailableTraits.SelectedIndex = 0

            lstNewTraits.ItemsSource = Traits

            txtNewCrewName.Text = NewCrewName
            txtNewCrewName.Focus()
            txtNewCrewName.SelectAll()
        End Sub

        ' --- ADDED: Set All Attributes Handler ---
        Private Sub btnSetNewAttributes_Click(sender As Object, e As RoutedEventArgs)
            For Each attr In Attributes
                attr.Value = 5
            Next
            ' Refresh the grid to show changes
            dgvNewAttributes.ItemsSource = Nothing
            dgvNewAttributes.ItemsSource = Attributes
            ' Or if Attributes was an ObservableCollection<DataProp> and DataProp implemented INotifyPropertyChanged:
            ' dgvNewAttributes.Items.Refresh()
        End Sub

        ' --- ADD THIS HANDLER FOR DRAGGING ---
        Private Sub TitleBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            If e.ButtonState = MouseButtonState.Pressed Then
                Me.DragMove()
            End If
        End Sub

        ' --- ADDED: Set All Skills Handler ---
        Private Sub btnSetNewSkills_Click(sender As Object, e As RoutedEventArgs)
            For Each skill In Skills
                skill.Value = 8
            Next
            ' Refresh the grid to show changes
            dgvNewSkills.ItemsSource = Nothing
            dgvNewSkills.ItemsSource = Skills
            ' Or if Skills was an ObservableCollection<DataProp> and DataProp implemented INotifyPropertyChanged:
            ' dgvNewSkills.Items.Refresh()
        End Sub


        Private Sub btnAddNewTrait_Click(sender As Object, e As RoutedEventArgs)
            If cmbAvailableTraits.SelectedValue IsNot Nothing Then
                Dim selectedTraitId = CInt(cmbAvailableTraits.SelectedValue)
                If Not Traits.Any(Function(t) t.Id = selectedTraitId) Then
                    Dim selectedTraitName = CType(cmbAvailableTraits.SelectedItem, KeyValuePair(Of Integer, String)).Value
                    Traits.Add(New DataProp With {.Id = selectedTraitId, .Name = selectedTraitName})
                End If
            End If
        End Sub

        Private Sub btnRemoveNewTrait_Click(sender As Object, e As RoutedEventArgs)
            If lstNewTraits.SelectedItem IsNot Nothing Then
                Dim traitToRemove = CType(lstNewTraits.SelectedItem, DataProp)
                Traits.Remove(traitToRemove)
            End If
        End Sub

        Private Sub btnCreateCrew_Click(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(txtNewCrewName.Text) Then
                MessageBox.Show("Please enter a name.", "Name Required", MessageBoxButton.OK, MessageBoxImage.Warning) : Return
            End If

            ' Validate template character is selected
            If cmbTemplateCharacter.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a template character to copy structure from.", "Template Required", MessageBoxButton.OK, MessageBoxImage.Warning) : Return
            End If

            ' Basic validation for numeric values in grids (more robust validation could be added)
            Try
                For Each item In DirectCast(dgvNewAttributes.ItemsSource, List(Of DataProp))
                    If item.Value < 0 Then Throw New Exception($"Attribute '{item.Name}' has invalid value.")
                Next
                For Each item In DirectCast(dgvNewSkills.ItemsSource, List(Of DataProp))
                    If item.Value < 0 Then Throw New Exception($"Skill '{item.Name}' has invalid value.")
                Next
            Catch ex As Exception
                MessageBox.Show($"Validation Error: {ex.Message} Values must be non-negative numbers.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End Try

            NewCrewName = txtNewCrewName.Text
            SelectedTemplateCharacterId = CInt(cmbTemplateCharacter.SelectedValue)
            ' Attributes/Skills/Traits lists are already updated via the UI/buttons
            Me.DialogResult = True
        End Sub

    End Class

End Namespace