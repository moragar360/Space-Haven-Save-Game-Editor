Imports System.Xml.Linq
Imports System.Collections.ObjectModel
Imports Microsoft.Win32
Imports System.IO
Imports System.Windows.Data
Imports System.Diagnostics

Namespace SpaceHavenEditor2

    Class GameStartEditorWindow
        Inherits Window

        ' Class to represent a resource/item in the DataGrid
        Public Class ResourceItem
            Public Property ElementId As Integer
            Public Property Name As String
            Public Property Quantity As Integer
        End Class

        Private xmlDoc As XDocument
        Private filePath As String
        Private resourceItems As ObservableCollection(Of ResourceItem)
        Private itemItems As ObservableCollection(Of ResourceItem) ' Separate collection for items
        Private _hasUnsavedChanges As Boolean = False
        Private isStationMode As Boolean = False ' True for playerCrafts (station), False for playerShips (ship)
        Private currentContainerElement As XElement = Nothing ' The current <l> element we're editing

        Public Sub New()
            InitializeComponent()
            resourceItems = New ObservableCollection(Of ResourceItem)()
            itemItems = New ObservableCollection(Of ResourceItem)()
            dgResources.ItemsSource = resourceItems
            dgItems.ItemsSource = itemItems
            PopulateComboBoxes()
        End Sub

        ' Populate the ComboBoxes with categorized items
        Private Sub PopulateComboBoxes()
            ' Populate Resources ComboBox (EXCLUDE items that belong in Items tab)
            Dim categorizedResources As New List(Of CategorizedItem)()
            For Each kvp In IdCollection.DefaultStorageIDs.OrderBy(Function(x) x.Value)
                Dim category As String = GetCategoryForItem(kvp.Key, kvp.Value)
                ' EXCLUDE: Weapons, Armor/Apparel, Medical, Remote Control, and Breaching Charge
                If category <> "Weapons" AndAlso category <> "Armor/Apparel" AndAlso category <> "Medical" AndAlso
                   kvp.Key <> 3386 AndAlso kvp.Key <> 4040 Then ' Exclude Remote Control (3386) and Breaching Charge (4040)
                    categorizedResources.Add(New CategorizedItem(category, kvp.Value, kvp.Key))
                End If
            Next
            Dim sortedResources = categorizedResources.OrderBy(Function(item) item.Category).ThenBy(Function(item) item.ItemName).ToList()

            Dim cvsResources As New CollectionViewSource()
            cvsResources.Source = sortedResources
            If cvsResources.GroupDescriptions.Count = 0 Then
                cvsResources.GroupDescriptions.Add(New PropertyGroupDescription("Category"))
            End If
            cmbAddResource.ItemsSource = cvsResources.View

            ' Populate Items ComboBox (ONLY weapons, armor, medical, remote control, and breaching charge)
            Dim categorizedItems As New List(Of CategorizedItem)()
            For Each kvp In IdCollection.DefaultStorageIDs.OrderBy(Function(x) x.Value)
                Dim category As String = GetCategoryForItem(kvp.Key, kvp.Value)
                ' Filter to ONLY include: Weapons, Armor/Apparel, Medical, Remote Control, and Breaching Charge
                If category = "Weapons" OrElse category = "Armor/Apparel" OrElse category = "Medical" OrElse
                   (kvp.Key = 3386) OrElse (kvp.Key = 4040) Then ' Remote Control (3386), Breaching Charge (4040)
                    categorizedItems.Add(New CategorizedItem(category, kvp.Value, kvp.Key))
                End If
            Next
            Dim sortedItems = categorizedItems.OrderBy(Function(item) item.Category).ThenBy(Function(item) item.ItemName).ToList()

            Dim cvsItems As New CollectionViewSource()
            cvsItems.Source = sortedItems
            If cvsItems.GroupDescriptions.Count = 0 Then
                cvsItems.GroupDescriptions.Add(New PropertyGroupDescription("Category"))
            End If
            cmbAddItem.ItemsSource = cvsItems.View
        End Sub

        ' Get category for item (same logic as MainWindow)
        Private Function GetCategoryForItem(itemId As Integer, itemName As String) As String
            Select Case itemId
                Case 725, 728, 729, 760, 1152, 3069, 3070, 3071, 3072, 3961, 3962
                    Return "Weapons"
                Case 2715, 1926
                    Return "Ammo"
                Case 3960, 3967, 3968, 3969, 4076 ' Weapon Attachments
                    Return "Attachments"
                Case 3384 ' Armor
                    Return "Armor/Apparel"
                Case 3388, 4065 ' Oxygen/Suit stuff
                    Return "Equipment"
            End Select

            ' Categorize by keywords in name
            Dim lowerName = itemName.ToLower()
            If lowerName.Contains("food") OrElse lowerName.Contains("meat") OrElse lowerName.Contains("vegetables") OrElse
               lowerName.Contains("fruits") OrElse lowerName.Contains("nuts and seeds") OrElse lowerName.Contains("alcohol") Then
                Return "Food & Drink"
            End If
            If lowerName.Contains("medical") OrElse lowerName.Contains("fluid") OrElse lowerName.Contains("bandage") OrElse
               lowerName.Contains("painkillers") OrElse lowerName.Contains("stimulant") OrElse lowerName.Contains("wound dressing") Then
                Return "Medical"
            End If
            If lowerName.Contains("scrap") OrElse lowerName.Contains("rubble") Then
                Return "Scrap & Waste"
            End If
            If lowerName.Contains("block") OrElse lowerName.Contains("plates") Then
                Return "Building Blocks"
            End If
            If lowerName.Contains("component") OrElse lowerName.Contains("parts") Then
                Return "Components"
            End If
            If lowerName.Contains("rod") OrElse lowerName.Contains("cell") OrElse lowerName.Contains("fuel") OrElse
               lowerName.Contains("energ") OrElse lowerName.Contains("hyperium") Then
                Return "Energy & Fuel"
            End If
            If lowerName.Contains("ore") OrElse lowerName.Contains("metals") OrElse lowerName.Contains("carbon") OrElse
               lowerName.Contains("ice") OrElse lowerName.Contains("water") OrElse lowerName.Contains("chemicals") OrElse
               lowerName.Contains("plastics") OrElse lowerName.Contains("fibers") OrElse lowerName.Contains("fabrics") OrElse
               lowerName.Contains("bio matter") Then
                Return "Raw Materials"
            End If
            If lowerName.Contains("corpse") OrElse lowerName.Contains("organs") Then
                Return "Biological"
            End If
            If lowerName.Contains("fertilizer") OrElse lowerName.Contains("grain") Then
                Return "Farming"
            End If

            ' Default category if none matched
            Return "Miscellaneous"
        End Function

        ' Load File button click handler
        Private Sub btnLoadFile_Click(sender As Object, e As RoutedEventArgs)
            Dim openFileDialog As New OpenFileDialog() With {
                .Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                .Title = "Select Game Start XML File"
            }

            ' Set initial directory from settings, or use default Space Haven path
            Try
                Dim initialDir As String = Global.SpaceHavenEditor2.My.Settings.Default.DefaultSaveDirectory
                If String.IsNullOrEmpty(initialDir) OrElse Not Directory.Exists(initialDir) Then
                    ' Try to find the game directory (typically one level up from saves)
                    Dim localAppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    Dim gamePath As String = Path.Combine(localAppData, "Low", "Bugbyte Oy", "Space Haven")
                    If Directory.Exists(gamePath) Then
                        initialDir = gamePath
                    Else
                        initialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    End If
                End If
                openFileDialog.InitialDirectory = initialDir
            Catch ex As Exception
                ' If settings fail, use default
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            End Try

            If openFileDialog.ShowDialog() = True Then
                filePath = openFileDialog.FileName
                txtFilePath.Text = filePath
                LoadXmlFile()
            End If
        End Sub

        ' Load and parse the XML file
        Private Sub LoadXmlFile()
            Try
                xmlDoc = XDocument.Load(filePath)
                resourceItems.Clear()
                itemItems.Clear()

                ' Game Start files have a <diff> root element, then <playerShips> or <playerCrafts>
                Dim diffElement As XElement = Nothing
                If xmlDoc.Root IsNot Nothing Then
                    If xmlDoc.Root.Name = "diff" Then
                        diffElement = xmlDoc.Root
                    Else
                        diffElement = xmlDoc.Root.Element("diff")
                        If diffElement Is Nothing Then
                            diffElement = xmlDoc.Root
                        End If
                    End If
                End If

                ' Check for both playerShips (ship mode) and playerCrafts (station mode)
                Dim playerShipsElement = diffElement?.Element("playerShips")
                Dim playerCraftsElement = diffElement?.Element("playerCrafts")
                Dim shipElement As XElement = Nothing
                isStationMode = False
                currentContainerElement = Nothing

                ' Check if playerShips exists AND has child elements (not empty)
                Dim hasPlayerShips = playerShipsElement IsNot Nothing AndAlso playerShipsElement.Elements("l").Any()

                If hasPlayerShips Then
                    ' Ship mode - get the first ship (l element)
                    shipElement = playerShipsElement.Element("l")
                    If shipElement Is Nothing Then
                        MessageBox.Show("Invalid XML structure: No ship found in 'playerShips'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        Return
                    End If
                    currentContainerElement = shipElement
                ElseIf playerCraftsElement IsNot Nothing Then
                    ' Station mode - find the craft/container with resources or items
                    isStationMode = True
                    ' Look for the <l> element that has resources or items, or has maxResources/maxItems > 0
                    For Each craftElement In playerCraftsElement.Elements("l")
                        Dim hasResources = craftElement.Element("resources") IsNot Nothing AndAlso craftElement.Element("resources").Elements("l").Any()
                        Dim hasItems = craftElement.Element("items") IsNot Nothing AndAlso craftElement.Element("items").Elements("l").Any()
                        Dim craftMaxResourcesAttr = craftElement.Attribute("maxResources")
                        Dim craftMaxItemsAttr = craftElement.Attribute("maxItems")
                        Dim maxResources As Integer = 0
                        Dim maxItems As Integer = 0
                        If craftMaxResourcesAttr IsNot Nothing Then Integer.TryParse(craftMaxResourcesAttr.Value, maxResources)
                        If craftMaxItemsAttr IsNot Nothing Then Integer.TryParse(craftMaxItemsAttr.Value, maxItems)

                        If hasResources OrElse hasItems OrElse (maxResources > 0 AndAlso maxItems > 0) Then
                            shipElement = craftElement
                            currentContainerElement = craftElement
                            Exit For
                        End If
                    Next
                    ' If no craft with resources/items found, use the first one
                    If shipElement Is Nothing Then
                        shipElement = playerCraftsElement.Element("l")
                        If shipElement Is Nothing Then
                            MessageBox.Show("Invalid XML structure: No craft found in 'playerCrafts'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                            Return
                        End If
                        currentContainerElement = shipElement
                    End If
                Else
                    MessageBox.Show("Invalid XML structure: Neither 'playerShips' nor 'playerCrafts' element found. This may not be a valid Game Start file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Load settings from diff element
                If diffElement IsNot Nothing Then
                    Dim enemiesAttr = diffElement.Attribute("enemiesEnabled")
                    If enemiesAttr IsNot Nothing Then
                        chkEnemiesEnabled.IsChecked = Boolean.Parse(enemiesAttr.Value)
                    End If

                    Dim friendsAttr = diffElement.Attribute("friendsEnabled")
                    If friendsAttr IsNot Nothing Then
                        chkFriendsEnabled.IsChecked = Boolean.Parse(friendsAttr.Value)
                    End If

                    Dim loversAttr = diffElement.Attribute("loversEnabled")
                    If loversAttr IsNot Nothing Then
                        chkLoversEnabled.IsChecked = Boolean.Parse(loversAttr.Value)
                    End If

                    Dim sandboxAttr = diffElement.Attribute("sandbox")
                    If sandboxAttr IsNot Nothing Then
                        chkSandbox.IsChecked = Boolean.Parse(sandboxAttr.Value)
                    End If
                End If

                ' Load crew and resource settings from ship element
                Dim crewAttr = shipElement.Attribute("crew")
                If crewAttr IsNot Nothing Then
                    txtCrew.Text = crewAttr.Value
                End If

                Dim maxCrewAttr = shipElement.Attribute("maxCrew")
                If maxCrewAttr IsNot Nothing Then
                    txtMaxCrew.Text = maxCrewAttr.Value
                End If

                Dim maxResourcesAttr = shipElement.Attribute("maxResources")
                If maxResourcesAttr IsNot Nothing Then
                    txtMaxResources.Text = maxResourcesAttr.Value
                End If

                Dim maxItemsAttr = shipElement.Attribute("maxItems")
                If maxItemsAttr IsNot Nothing Then
                    txtMaxItems.Text = maxItemsAttr.Value
                End If

                ' hangarsPopulated only exists for ship mode, not station mode
                If Not isStationMode Then
                    Dim hangarsPopulatedAttr = shipElement.Attribute("hangarsPopulated")
                    If hangarsPopulatedAttr IsNot Nothing Then
                        chkHangarsPopulated.IsChecked = Boolean.Parse(hangarsPopulatedAttr.Value)
                    End If
                Else
                    ' Station mode doesn't have hangarsPopulated, so disable/hide the checkbox
                    chkHangarsPopulated.IsChecked = False
                End If

                ' Load resources
                Dim resourcesElement = shipElement.Element("resources")
                If resourcesElement IsNot Nothing Then
                    For Each resourceNode In resourcesElement.Elements("l")
                        Dim elementIdAttr = resourceNode.Attribute("elementId")
                        Dim howMuchAttr = resourceNode.Attribute("howMuch")

                        If elementIdAttr IsNot Nothing AndAlso howMuchAttr IsNot Nothing Then
                            Dim elementId As Integer
                            Dim quantity As Integer

                            If Integer.TryParse(elementIdAttr.Value, elementId) AndAlso Integer.TryParse(howMuchAttr.Value, quantity) Then
                                Dim itemName As String = GetItemName(elementId)
                                resourceItems.Add(New ResourceItem With {
                                    .ElementId = elementId,
                                    .Name = itemName,
                                    .Quantity = quantity
                                })
                            End If
                        End If
                    Next
                End If

                ' Load items (separate from resources)
                Dim itemsElement = shipElement.Element("items")
                If itemsElement IsNot Nothing Then
                    For Each itemNode In itemsElement.Elements("l")
                        Dim elementIdAttr = itemNode.Attribute("elementId")
                        Dim howMuchAttr = itemNode.Attribute("howMuch")

                        If elementIdAttr IsNot Nothing AndAlso howMuchAttr IsNot Nothing Then
                            Dim elementId As Integer
                            Dim quantity As Integer

                            If Integer.TryParse(elementIdAttr.Value, elementId) AndAlso Integer.TryParse(howMuchAttr.Value, quantity) Then
                                Dim itemName As String = GetItemName(elementId)
                                itemItems.Add(New ResourceItem With {
                                    .ElementId = elementId,
                                    .Name = itemName,
                                    .Quantity = quantity
                                })
                            End If
                        End If
                    Next
                End If

                ' Enable UI components
                settingsCard.IsEnabled = True
                btnSaveChanges.IsEnabled = True
                ClearUnsavedChanges() ' Reset unsaved changes flag when loading new file
                MessageBox.Show($"File loaded successfully. Found {resourceItems.Count} resource(s) and {itemItems.Count} item(s).", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            Catch ex As Exception
                MessageBox.Show($"Error loading XML file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Get item name from IdCollection
        Private Function GetItemName(elementId As Integer) As String
            If IdCollection.DefaultStorageIDs.ContainsKey(elementId) Then
                Return IdCollection.DefaultStorageIDs(elementId)
            Else
                Return $"Unknown Item ({elementId})"
            End If
        End Function

        ' Add Resource button click handler
        Private Sub btnAddResource_Click(sender As Object, e As RoutedEventArgs)
            If cmbAddResource.SelectedValue Is Nothing Then
                MessageBox.Show("Please select an item from the dropdown.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim elementId As Integer
            If Not Integer.TryParse(cmbAddResource.SelectedValue.ToString(), elementId) Then
                MessageBox.Show("Invalid item selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedItem = TryCast(cmbAddResource.SelectedItem, CategorizedItem)
            Dim itemName As String = If(selectedItem IsNot Nothing, selectedItem.ItemName, GetItemName(elementId))

            ' Check if item already exists
            Dim existingItem = resourceItems.FirstOrDefault(Function(r) r.ElementId = elementId)
            If existingItem IsNot Nothing Then
                Dim result = MessageBox.Show($"Item '{itemName}' already exists with quantity {existingItem.Quantity}. Do you want to update the quantity?", 
                                           "Item Exists", MessageBoxButton.YesNo, MessageBoxImage.Question)
                If result = MessageBoxResult.Yes Then
                    Dim newQuantity As Integer
                    If Integer.TryParse(txtAddResourceQuantity.Text, newQuantity) AndAlso newQuantity > 0 Then
                        existingItem.Quantity = newQuantity
                        MarkUnsavedChanges()
                    Else
                        MessageBox.Show("Please enter a valid quantity (positive integer).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                End If
                Return
            End If

            ' Add new item
            Dim quantity As Integer
            If Integer.TryParse(txtAddResourceQuantity.Text, quantity) AndAlso quantity > 0 Then
                resourceItems.Add(New ResourceItem With {
                    .ElementId = elementId,
                    .Name = itemName,
                    .Quantity = quantity
                })
                txtAddResourceQuantity.Text = ""
                cmbAddResource.SelectedValue = Nothing
                MarkUnsavedChanges()
            Else
                MessageBox.Show("Please enter a valid quantity (positive integer).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub

        ' Add Item button click handler
        Private Sub btnAddItem_Click(sender As Object, e As RoutedEventArgs)
            If cmbAddItem.SelectedValue Is Nothing Then
                MessageBox.Show("Please select an item from the dropdown.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim elementId As Integer
            If Not Integer.TryParse(cmbAddItem.SelectedValue.ToString(), elementId) Then
                MessageBox.Show("Invalid item selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedItem = TryCast(cmbAddItem.SelectedItem, CategorizedItem)
            Dim itemName As String = If(selectedItem IsNot Nothing, selectedItem.ItemName, GetItemName(elementId))

            ' Check if item already exists
            Dim existingItem = itemItems.FirstOrDefault(Function(r) r.ElementId = elementId)
            If existingItem IsNot Nothing Then
                Dim result = MessageBox.Show($"Item '{itemName}' already exists with quantity {existingItem.Quantity}. Do you want to update the quantity?", 
                                           "Item Exists", MessageBoxButton.YesNo, MessageBoxImage.Question)
                If result = MessageBoxResult.Yes Then
                    Dim newQuantity As Integer
                    If Integer.TryParse(txtAddItemQuantity.Text, newQuantity) AndAlso newQuantity > 0 Then
                        existingItem.Quantity = newQuantity
                        MarkUnsavedChanges()
                    Else
                        MessageBox.Show("Please enter a valid quantity (positive integer).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                End If
                Return
            End If

            ' Add new item
            Dim quantity As Integer
            If Integer.TryParse(txtAddItemQuantity.Text, quantity) AndAlso quantity > 0 Then
                itemItems.Add(New ResourceItem With {
                    .ElementId = elementId,
                    .Name = itemName,
                    .Quantity = quantity
                })
                txtAddItemQuantity.Text = ""
                cmbAddItem.SelectedValue = Nothing
                MarkUnsavedChanges()
            Else
                MessageBox.Show("Please enter a valid quantity (positive integer).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub

        ' Delete Selected Resources button click handler
        Private Sub btnDeleteSelectedResources_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedItems = dgResources.SelectedItems.Cast(Of ResourceItem)().ToList()
            If selectedItems.Count = 0 Then
                MessageBox.Show("Please select one or more items to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim itemNames = String.Join(", ", selectedItems.Select(Function(i) i.Name))
            Dim result = MessageBox.Show($"Are you sure you want to delete {selectedItems.Count} selected item(s)?{vbCrLf}{itemNames}", 
                                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question)
            If result = MessageBoxResult.Yes Then
                For Each item In selectedItems
                    resourceItems.Remove(item)
                Next
                MarkUnsavedChanges()
            End If
        End Sub

        ' Delete Selected Items button click handler
        Private Sub btnDeleteSelectedItems_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedItems = dgItems.SelectedItems.Cast(Of ResourceItem)().ToList()
            If selectedItems.Count = 0 Then
                MessageBox.Show("Please select one or more items to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim itemNames = String.Join(", ", selectedItems.Select(Function(i) i.Name))
            Dim result = MessageBox.Show($"Are you sure you want to delete {selectedItems.Count} selected item(s)?{vbCrLf}{itemNames}", 
                                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question)
            If result = MessageBoxResult.Yes Then
                For Each item In selectedItems
                    itemItems.Remove(item)
                Next
                MarkUnsavedChanges()
            End If
        End Sub

        ' DataGrid selection changed handlers
        Private Sub dgResources_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Optional functionality
        End Sub

        Private Sub dgItems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Optional functionality
        End Sub

        ' DataGrid cell edit ending handlers to track quantity changes
        Private Sub dgResources_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs)
            MarkUnsavedChanges()
        End Sub

        Private Sub dgItems_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs)
            MarkUnsavedChanges()
        End Sub

        ' Checkbox change handlers for settings
        Private Sub chkEnemiesEnabled_Checked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkEnemiesEnabled_Unchecked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkFriendsEnabled_Checked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkFriendsEnabled_Unchecked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkLoversEnabled_Checked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkLoversEnabled_Unchecked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkSandbox_Checked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkSandbox_Unchecked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkHangarsPopulated_Checked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub chkHangarsPopulated_Unchecked(sender As Object, e As RoutedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        ' TextBox change handlers for settings
        Private Sub txtCrew_TextChanged(sender As Object, e As TextChangedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub txtMaxCrew_TextChanged(sender As Object, e As TextChangedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub txtMaxResources_TextChanged(sender As Object, e As TextChangedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        Private Sub txtMaxItems_TextChanged(sender As Object, e As TextChangedEventArgs)
            If settingsCard.IsEnabled Then MarkUnsavedChanges()
        End Sub

        ' Mark unsaved changes and show banner
        Private Sub MarkUnsavedChanges()
            _hasUnsavedChanges = True
            Try
                If txtUnsavedIndicator IsNot Nothing Then
                    txtUnsavedIndicator.Visibility = Visibility.Visible
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error showing unsaved indicator: {ex.Message}")
            End Try
        End Sub

        ' Clear unsaved changes flag and hide banner
        Private Sub ClearUnsavedChanges()
            _hasUnsavedChanges = False
            Try
                If txtUnsavedIndicator IsNot Nothing Then
                    txtUnsavedIndicator.Visibility = Visibility.Collapsed
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error hiding unsaved indicator: {ex.Message}")
            End Try
        End Sub

        ' Save Changes button click handler
        Private Sub btnSaveChanges_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(filePath) Then
                MessageBox.Show("No file loaded to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Try
                ' Game Start files have a <diff> root element, then <playerShips> or <playerCrafts>
                Dim diffElement As XElement = Nothing
                If xmlDoc.Root IsNot Nothing Then
                    If xmlDoc.Root.Name = "diff" Then
                        diffElement = xmlDoc.Root
                    Else
                        diffElement = xmlDoc.Root.Element("diff")
                        If diffElement Is Nothing Then
                            diffElement = xmlDoc.Root
                        End If
                    End If
                End If

                ' Check for both playerShips (ship mode) and playerCrafts (station mode)
                Dim playerShipsElement = diffElement?.Element("playerShips")
                Dim playerCraftsElement = diffElement?.Element("playerCrafts")
                Dim shipElement As XElement = Nothing

                ' Check if playerShips exists AND has child elements (not empty)
                Dim hasPlayerShips = playerShipsElement IsNot Nothing AndAlso playerShipsElement.Elements("l").Any()

                If hasPlayerShips Then
                    ' Ship mode - get the first ship (l element)
                    shipElement = playerShipsElement.Element("l")
                    If shipElement Is Nothing Then
                        MessageBox.Show("Invalid XML structure: No ship found in 'playerShips'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        Return
                    End If
                ElseIf playerCraftsElement IsNot Nothing Then
                    ' Station mode - use the stored container element
                    If currentContainerElement IsNot Nothing Then
                        shipElement = currentContainerElement
                    Else
                        ' Fallback: find the craft/container with resources or items
                        For Each craftElement In playerCraftsElement.Elements("l")
                            Dim hasResources = craftElement.Element("resources") IsNot Nothing AndAlso craftElement.Element("resources").Elements("l").Any()
                            Dim hasItems = craftElement.Element("items") IsNot Nothing AndAlso craftElement.Element("items").Elements("l").Any()
                            Dim craftMaxResourcesAttr = craftElement.Attribute("maxResources")
                            Dim craftMaxItemsAttr = craftElement.Attribute("maxItems")
                            Dim maxResources As Integer = 0
                            Dim maxItems As Integer = 0
                            If craftMaxResourcesAttr IsNot Nothing Then Integer.TryParse(craftMaxResourcesAttr.Value, maxResources)
                            If craftMaxItemsAttr IsNot Nothing Then Integer.TryParse(craftMaxItemsAttr.Value, maxItems)

                            If hasResources OrElse hasItems OrElse (maxResources > 0 AndAlso maxItems > 0) Then
                                shipElement = craftElement
                                Exit For
                            End If
                        Next
                        ' If no craft with resources/items found, use the first one
                        If shipElement Is Nothing Then
                            shipElement = playerCraftsElement.Element("l")
                            If shipElement Is Nothing Then
                                MessageBox.Show("Invalid XML structure: No craft found in 'playerCrafts'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                Return
                            End If
                        End If
                    End If
                Else
                    MessageBox.Show("Invalid XML structure: Neither 'playerShips' nor 'playerCrafts' element found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Save settings to diff element
                If diffElement IsNot Nothing Then
                    diffElement.SetAttributeValue("enemiesEnabled", chkEnemiesEnabled.IsChecked.ToString().ToLower())
                    diffElement.SetAttributeValue("friendsEnabled", chkFriendsEnabled.IsChecked.ToString().ToLower())
                    diffElement.SetAttributeValue("loversEnabled", chkLoversEnabled.IsChecked.ToString().ToLower())
                    diffElement.SetAttributeValue("sandbox", chkSandbox.IsChecked.ToString().ToLower())
                End If

                ' Save crew and resource settings to ship/craft element
                shipElement.SetAttributeValue("crew", txtCrew.Text)
                shipElement.SetAttributeValue("maxCrew", txtMaxCrew.Text)
                shipElement.SetAttributeValue("maxResources", txtMaxResources.Text)
                shipElement.SetAttributeValue("maxItems", txtMaxItems.Text)
                ' hangarsPopulated only exists for ship mode, not station mode
                If Not isStationMode Then
                    shipElement.SetAttributeValue("hangarsPopulated", chkHangarsPopulated.IsChecked.ToString().ToLower())
                End If

                ' Clear and save resources
                Dim resourcesElement = shipElement.Element("resources")
                If resourcesElement IsNot Nothing Then
                    resourcesElement.RemoveAll()
                Else
                    resourcesElement = New XElement("resources")
                    shipElement.Add(resourcesElement)
                End If

                For Each resourceItem In resourceItems
                    Dim resourceNode = New XElement("l",
                        New XAttribute("elementId", resourceItem.ElementId.ToString()),
                        New XAttribute("howMuch", resourceItem.Quantity.ToString()))
                    resourcesElement.Add(resourceNode)
                Next

                ' Clear and save items (separate from resources)
                Dim itemsElement = shipElement.Element("items")
                If itemsElement IsNot Nothing Then
                    itemsElement.RemoveAll()
                Else
                    itemsElement = New XElement("items")
                    shipElement.Add(itemsElement)
                End If

                For Each itemItem In itemItems
                    Dim itemNode = New XElement("l",
                        New XAttribute("elementId", itemItem.ElementId.ToString()),
                        New XAttribute("howMuch", itemItem.Quantity.ToString()))
                    itemsElement.Add(itemNode)
                Next

                ' Save the XML file
                xmlDoc.Save(filePath)
                ClearUnsavedChanges() ' Reset unsaved changes flag after successful save
                MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            Catch ex As Exception
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

    End Class

End Namespace
