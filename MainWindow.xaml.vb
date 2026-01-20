Imports MaterialDesignThemes.Wpf
Imports Microsoft.Win32
Imports System.Data
Imports System.IO
Imports System.Xml.Linq
Imports System.Collections.ObjectModel
Imports System.Collections.Generic
Imports System.Xml.XPath
Imports System.ComponentModel
Imports System.Windows.Threading
Imports Microsoft.VisualBasic.FileIO
Imports System.Windows.Automation
Imports System.Globalization ' Needed for CultureInfo
Imports System.Windows.Input ' Needed for Drag/Drop and Mouse events
Imports System.Windows.Data ' Needed for CollectionViewSource and PropertyGroupDescription
Imports System.Windows.Media ' Needed for VisualTreeHelper


Namespace SpaceHavenEditor2

    Class SpaceHavenEditor
        Inherits Window

        ' Static lists for ComboBox items
        Public Shared ReadOnly ShirtSets As New List(Of String) From {"2433", "2434"}
        Public Shared ReadOnly PantsSets As New List(Of String) From {"2433", "2434"}
        Public Shared ReadOnly ColorIndices As New List(Of String) From {"0", "1", "2", "3", "4", "5"}

        ' Color mapping for visual feedback (actual Space Haven colors from save file)
        Private Shared ReadOnly ColorMapping As New Dictionary(Of Integer, String) From {
            {0, "Default/White"}, {1, "Light Gray"}, {2, "Gray"}, {3, "Dark Gray"}, {4, "Black"},
            {5, "Light Blue"}, {6, "Blue"}, {7, "Dark Blue"}, {8, "Light Green"}, {9, "Green"},
            {10, "Dark Green"}, {11, "Light Red"}, {12, "Red"}, {13, "Dark Red"}, {14, "Light Yellow"},
            {15, "Yellow"}, {16, "Orange"}, {17, "Light Purple"}, {18, "Purple"}, {19, "Dark Purple"},
            {20, "Light Brown"}, {21, "Brown"}, {22, "Dark Brown"}, {23, "Pink"}, {24, "Cyan"},
            {25, "Teal"}, {26, "Maroon"}, {27, "Navy"}, {28, "Olive"}, {29, "Lime"},
            {30, "Coral"}, {31, "Gold"}, {32, "Silver"}, {33, "Bronze"}, {34, "Copper"},
            {35, "Beige"}, {36, "Tan"}, {37, "Khaki"}, {38, "Cream"}, {39, "Ivory"},
            {40, "Mint"}, {41, "Lavender"}
        }

        Private currentFilePath As String = ""
        Private xmlDoc As XDocument ' Keep using this for XML operations
        Private characters As New List(Of Character)
        Private ships As New List(Of Ship) ' Your existing list of Ship objects
        Private _removedShipsInfo As String = "" ' Store information about removed ships

        Private currentShipStorageContainers As New List(Of StorageContainer)()
        Private CurrentContainerItems As ObservableCollection(Of StorageItem)
        Private _relationshipsCurrentPage As Integer = 1
        Private ReadOnly _relationshipsPageSize As Integer = 15

        'User Settings
        Private _backupEnabled As Boolean = True

        ' Unsaved changes tracking
        Private _hasUnsavedChanges As Boolean = False

        ' Blueprint Manager variables
        Private sourceSavePath As String = String.Empty
        Private blueprintFilePath As String = String.Empty
        Private targetSavePath As String = String.Empty
        Private sourceXmlDoc As XDocument = Nothing
        Private targetXmlDoc As XDocument = Nothing ' Only load when needed for import
        Private blueprintNode As XElement = Nothing ' Loaded blueprint ship node

        ' Game Start Editor variables
        Private gameStartXmlDoc As XDocument = Nothing
        Private gameStartFilePath As String = String.Empty
        Private gameStartResourceItems As ObservableCollection(Of GameStartResourceItem) = Nothing
        Private gameStartItemItems As ObservableCollection(Of GameStartResourceItem) = Nothing
        Private gameStartHasUnsavedChanges As Boolean = False

        ' Class to represent a resource/item in the Game Start Editor DataGrid
        Public Class GameStartResourceItem
            Implements System.ComponentModel.INotifyPropertyChanged

            Private _elementId As Integer
            Private _name As String
            Private _quantity As Integer
            Private _isSelected As Boolean

            Public Property ElementId As Integer
                Get
                    Return _elementId
                End Get
                Set(value As Integer)
                    _elementId = value
                    OnPropertyChanged("ElementId")
                End Set
            End Property

            Public Property Name As String
                Get
                    Return _name
                End Get
                Set(value As String)
                    _name = value
                    OnPropertyChanged("Name")
                End Set
            End Property

            Public Property Quantity As Integer
                Get
                    Return _quantity
                End Get
                Set(value As Integer)
                    _quantity = value
                    OnPropertyChanged("Quantity")
                End Set
            End Property

            Public Property IsSelected As Boolean
                Get
                    Return _isSelected
                End Get
                Set(value As Boolean)
                    _isSelected = value
                    OnPropertyChanged("IsSelected")
                End Set
            End Property

            Public Event PropertyChanged As System.ComponentModel.PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

            Protected Sub OnPropertyChanged(propertyName As String)
                RaiseEvent PropertyChanged(Me, New System.ComponentModel.PropertyChangedEventArgs(propertyName))
            End Sub
        End Class

        ' Simple class to hold ship info for the Blueprint Manager ComboBox
        Public Class ShipInfo
            Public Property Sid As Integer
            Public Property Sname As String
            Public Overrides Function ToString() As String
                Return Sname ' Default display
            End Function
        End Class

        Public Property CrewMembers As ObservableCollection(Of CrewMember)
        Public Property Resources As ObservableCollection(Of Resource)



        Public Sub New()
            InitializeComponent()
            Try
                _backupEnabled = My.Settings.BackupOnOpen
            Catch ex As Exception
                ' Handle potential error reading settings file
                MessageBox.Show($"Error loading settings: {ex.Message}{vbCrLf}Backup on open defaulted to True.", "Settings Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                _backupEnabled = True
            End Try
            Resources = New ObservableCollection(Of Resource)()
            CrewMembers = New ObservableCollection(Of CrewMember)()
            PrepareGroupedStorageItems()
            InitializeGameStartEditor()
            DataContext = Me
            AddHandler Me.Closing, AddressOf MainWindow_Closing
        End Sub

        ' Initialize Game Start Editor
        Private Sub InitializeGameStartEditor()
            gameStartResourceItems = New ObservableCollection(Of GameStartResourceItem)()
            gameStartItemItems = New ObservableCollection(Of GameStartResourceItem)()
            dgGameStartResources.ItemsSource = gameStartResourceItems
            dgGameStartItems.ItemsSource = gameStartItemItems
            PopulateGameStartComboBoxes()
            UpdateGameStartScenarioFolderDisplay()
        End Sub

        ' Update scenario folder display
        Private Sub UpdateGameStartScenarioFolderDisplay()
            Try
                ' Check if scenario directory is set in settings
                Dim scenarioDir As String = My.Settings.DefaultScenarioDirectory
                If Not String.IsNullOrEmpty(scenarioDir) AndAlso Directory.Exists(scenarioDir) Then
                    txtGameStartScenarioFolder.Text = $"Scenario Folder: {scenarioDir}"
                Else
                    ' Try to get the default scenario directory
                    scenarioDir = GetGameStartScenarioDirectory()
                    If Not String.IsNullOrEmpty(scenarioDir) AndAlso Directory.Exists(scenarioDir) Then
                        txtGameStartScenarioFolder.Text = $"Scenario Folder: {scenarioDir}"
                    Else
                        txtGameStartScenarioFolder.Text = "Scenario Folder: Not set (click 'Set Scenario Folder' to configure)"
                    End If
                End If
            Catch ex As Exception
                txtGameStartScenarioFolder.Text = "Scenario Folder: Not set"
            End Try
        End Sub


        ' 2. MODIFY the SelectionChanged handler to CALL the helper method
        Private Sub cmb_ships_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedShip = TryCast(cmb_ships.SelectedItem, Ship)
            Dim selectedShipNode = GetSelectedShipNode()


            ' Call the helper method to handle the selection
            ProcessShipSelection(selectedShip)
        End Sub


        Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs)
            Try
                My.Settings.BackupOnOpen = _backupEnabled ' Store current value
                My.Settings.Save() ' Persist settings
            Catch ex As Exception
                MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error)
                ' Don't cancel closing, just report error
            End Try
        End Sub

        Private Sub ResetApplicationState()
            ' Clear Data Objects
            xmlDoc = Nothing
            currentFilePath = String.Empty
            ships?.Clear()          ' Use null-conditional ?. if ships might be Nothing initially
            characters?.Clear()     ' Use null-conditional ?.
            currentShipStorageContainers?.Clear() ' Use null-conditional ?.
            CurrentContainerItems?.Clear()    ' Use null-conditional ?.

            ' Clear UI Elements - Add Try/Catch around UI access in case window isn't fully loaded/ready
            Try
                ClearGlobalSettingsUI()
                cmb_ships.ItemsSource = Nothing
                ClearUI() ' Clears ship details, crew list, grids
                ClearStorageDisplay() ' Clears storage combo and grid
                ' Clear other UI elements if needed (e.g., status bar text)
                ' txtStatus.Text = "Ready"
            Catch uiEx As Exception
                ' Log or ignore minor UI clearing errors during reset
                Console.WriteLine($"Minor error during UI reset: {uiEx.Message}")
            End Try

            SetWindowTitle() ' Reset window title
        End Sub

        Private Sub SetWindowTitle(Optional filePath As String = "", Optional hasUnsavedChanges As Boolean? = Nothing)
            ' If hasUnsavedChanges not provided, use the tracked state
            If hasUnsavedChanges Is Nothing Then
                hasUnsavedChanges = _hasUnsavedChanges
            End If

            Dim baseTitle = "Moragar's Space Haven Save Editor"
            If String.IsNullOrEmpty(filePath) Then
                Me.Title = baseTitle
            Else
                Dim fileNameOnly As String = "game" ' Default if path parsing fails
                Try
                    ' Try to get the save game folder name (e.g., "MySave")
                    Dim saveFolder = Path.GetDirectoryName(filePath) ' ...\save
                    If Not String.IsNullOrEmpty(saveFolder) Then
                        Dim saveNameFolder = Path.GetDirectoryName(saveFolder) ' ...\MySave
                        If Not String.IsNullOrEmpty(saveNameFolder) Then
                            fileNameOnly = Path.GetFileName(saveNameFolder)
                        End If
                    End If
                Catch pathEx As Exception
                    Console.WriteLine($"Error parsing path for title: {pathEx.Message}")
                End Try
                Me.Title = $"{baseTitle} - [{fileNameOnly}]{If(hasUnsavedChanges, " *", "")}"
            End If
        End Sub

        ' Helper method to mark that changes have been made
        Private Sub MarkUnsavedChanges()
            _hasUnsavedChanges = True
            SetWindowTitle(currentFilePath)
            ' Show visual indicator
            Try
                If txtUnsavedIndicator IsNot Nothing Then
                    txtUnsavedIndicator.Visibility = Visibility.Visible
                Else
                    Debug.WriteLine("WARNING: txtUnsavedIndicator is Nothing!")
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error setting unsaved indicator visibility: {ex.Message}")
            End Try
        End Sub

        ' Helper method to clear unsaved changes flag
        Private Sub ClearUnsavedChanges()
            _hasUnsavedChanges = False
            SetWindowTitle(currentFilePath)
            ' Hide visual indicator
            Try
                If txtUnsavedIndicator IsNot Nothing Then
                    txtUnsavedIndicator.Visibility = Visibility.Collapsed
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error hiding unsaved indicator: {ex.Message}")
            End Try
        End Sub


        ' Handle "Open" menu click
        Private Sub OpenFileMenu_Click(sender As Object, e As RoutedEventArgs)

            ' Initialize the OpenFileDialog
            Dim openFileDialog As New OpenFileDialog With {
                .Filter = "Space Haven Save Files|game",
                .Title = "Open Space Haven Save File"
            }


            ' Set the initial directory to the saved default or a fallback
            Dim defaultDirectory As String = My.Settings.DefaultSaveDirectory
            If String.IsNullOrEmpty(defaultDirectory) OrElse Not Directory.Exists(defaultDirectory) Then
                defaultDirectory = GetDefaultSpaceHavenDirectory()
                My.Settings.DefaultSaveDirectory = defaultDirectory
                My.Settings.Save()
            End If
            openFileDialog.InitialDirectory = defaultDirectory


            If openFileDialog.ShowDialog() = True Then
                Dim selectedFilePath As String = openFileDialog.FileName
                Dim backupPerformedSuccessfully As Boolean = False

                ' --- Backup Logic (with error handling) ---
                If _backupEnabled Then
                    Try
                        Dim sourceDir = Path.GetDirectoryName(selectedFilePath) ' Should be the 'save' folder
                        If String.IsNullOrEmpty(sourceDir) OrElse Not Directory.Exists(sourceDir) Then Throw New DirectoryNotFoundException("Source save directory not found.")
                        Dim sourceName = Path.GetFileName(sourceDir) ' Should be 'save'
                        Dim parentDir = Path.GetDirectoryName(sourceDir) ' This should be the folder containing 'save' (e.g., YourSaveName)
                        If String.IsNullOrEmpty(parentDir) Then Throw New DirectoryNotFoundException("Parent directory (save name folder) not found.")
                        Dim saveGamesDir = Path.GetDirectoryName(parentDir) ' This should be the 'savegames' folder
                        If String.IsNullOrEmpty(saveGamesDir) Then Throw New DirectoryNotFoundException("Parent directory ('savegames') not found.")

                        Dim ts = Date.Now.ToString("yyyyMMdd_HHmmss")
                        Dim backupName = $"{Path.GetFileName(parentDir)}_{sourceName}_backup_{ts}"
                        Dim backupPath = Path.Combine(saveGamesDir, backupName)

                        FileSystem.CopyDirectory(sourceDir, backupPath, UIOption.OnlyErrorDialogs, UICancelOption.DoNothing)
                        backupPerformedSuccessfully = True
                    Catch ex As Exception
                        MessageBox.Show($"Backup failed: {ex.Message}{Environment.NewLine}Attempting to load original file anyway.", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                        backupPerformedSuccessfully = False
                    End Try
                End If

                ' --- Reset UI and Data before loading ---
                ResetApplicationState()
                ClearUnsavedChanges() ' Reset unsaved changes flag when loading new file

                ' --- Loading Logic (with extensive Try...Catch) ---
                Try
                    currentFilePath = selectedFilePath
                    _removedShipsInfo = "" ' Reset removed ships info for new file

                    ' Step 1: Load the XML document with enhanced error handling
                    Try
                        xmlDoc = XDocument.Load(currentFilePath, LoadOptions.None)
                    Catch xmlEx As System.Xml.XmlException
                        ' Try to load with more permissive settings first
                        Try
                            xmlDoc = XDocument.Load(currentFilePath, LoadOptions.PreserveWhitespace)
                        Catch xmlEx2 As System.Xml.XmlException
                            ' If that fails, try to clean the XML and load it
                            Try
                                Dim cleanedXml = CleanXmlFile(currentFilePath)
                                xmlDoc = XDocument.Parse(cleanedXml)
                            Catch cleanEx As Exception
                                ' If cleaning fails, try fixing EntityName issues
                                Try
                                    ' Create a backup before attempting fixes
                                    Try
                                        Dim backupPath = currentFilePath & ".backup." & DateTime.Now.ToString("yyyyMMdd_HHmmss")
                                        File.Copy(currentFilePath, backupPath)
                                    Catch backupEx As Exception
                                        ' Backup failed, but continue with fix attempt
                                    End Try

                                    Dim entityFixedXml = FixEntityNameIssues(currentFilePath)
                                    xmlDoc = XDocument.Parse(entityFixedXml)
                                    MessageBox.Show("The save file contained XML formatting issues that have been automatically fixed. The file should now load successfully.", "Auto-Fix Applied", MessageBoxButton.OK, MessageBoxImage.Information)
                                Catch entityFixEx As Exception
                                    ' If that fails, try removing problematic ships
                                    Try
                                        ' Pass the line number from the error to help identify the problematic section
                                        Dim shipCleanedXml = RemoveProblematicShips(currentFilePath, xmlEx.LineNumber, xmlEx.LinePosition)
                                        xmlDoc = XDocument.Parse(shipCleanedXml)

                                        Dim message As String = "The save file contained problematic ship data that has been automatically removed. The file should now load successfully."
                                        If Not String.IsNullOrEmpty(_removedShipsInfo) Then
                                            message += Environment.NewLine & Environment.NewLine & "Removed ships:" & Environment.NewLine & _removedShipsInfo
                                        End If

                                        MessageBox.Show(message, "Auto-Fix Applied", MessageBoxButton.OK, MessageBoxImage.Information)
                                    Catch shipCleanEx As Exception
                                        ' If all attempts fail, show the original error with detailed information
                                        Dim errorMessage As String = $"Error parsing the save file XML:{Environment.NewLine}[{xmlEx.GetType().Name}] {xmlEx.Message}{Environment.NewLine}{Environment.NewLine}File: {currentFilePath}{Environment.NewLine}{Environment.NewLine}The file might be corrupted or not a valid Space Haven save.{Environment.NewLine}{Environment.NewLine}Additional info: Line {xmlEx.LineNumber}, Position {xmlEx.LinePosition}"

                                        ' Add specific guidance based on the error type
                                        If xmlEx.Message.Contains("EntityName") Then
                                            errorMessage += Environment.NewLine & Environment.NewLine & "This appears to be an XML entity parsing error, often caused by:" & Environment.NewLine & "- Unescaped special characters (like &, <, >) in ship names or descriptions" & Environment.NewLine & "- Malformed XML in quest-related ships (like Haven Foundation ships)" & Environment.NewLine & "- Corrupted save file data"
                                        End If

                                        errorMessage += Environment.NewLine & Environment.NewLine & "Manual fix: You can try opening the save file in a text editor and removing the problematic ship section (look for <j> tags around the error line)."

                                        MessageBox.Show(errorMessage, "XML Load Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                        ResetApplicationState()
                                        Return
                                    End Try
                                End Try
                            End Try
                        End Try
                    Catch fileEx As Exception
                        MessageBox.Show($"Error reading the save file:{Environment.NewLine}[{fileEx.GetType().Name}] {fileEx.Message}{Environment.NewLine}{Environment.NewLine}File: {currentFilePath}", "File Read Error", MessageBoxButton.OK, MessageBoxImage.Error)
                        ResetApplicationState()
                        Return
                    End Try

                    ' Step 2: Basic Validation of loaded XML
                    If xmlDoc Is Nothing OrElse xmlDoc.Root Is Nothing OrElse xmlDoc.Root.Name <> "game" Then
                        MessageBox.Show("The loaded file is not a valid Space Haven save (missing root <game> element).", "Invalid File Structure", MessageBoxButton.OK, MessageBoxImage.Error)
                        ResetApplicationState()
                        Return
                    End If

                    ' Step 3: Load different data sections with individual Try...Catch
                    Dim loadErrors As New System.Text.StringBuilder()

                    ' --- Enhanced Error Reporting in Catch Blocks ---
                    Try : LoadGlobalSettings() : Catch ex As Exception : loadErrors.AppendLine($"- Error loading Global Settings: [{ex.GetType().Name}] {ex.Message}") : ClearGlobalSettingsUI() : End Try
                    Try : LoadShips() : Catch ex As Exception : loadErrors.AppendLine($"- Error loading Ships: [{ex.GetType().Name}] {ex.Message}") : cmb_ships.ItemsSource = Nothing : ClearUI() : ClearStorageDisplay() : End Try
                    Try : LoadCharacters() : Catch ex As Exception : loadErrors.AppendLine($"- Error loading Characters: [{ex.GetType().Name}] {ex.Message}") : lstCharacters.ItemsSource = Nothing : ClearDataGrids() : End Try

                    ' Step 4: Populate UI based on loaded data (if ships were loaded)
                    If ships IsNot Nothing AndAlso ships.Any() Then
                        Try : PrepareGroupedStorageItems() : Catch ex As Exception : loadErrors.AppendLine($"- Error preparing storage item list: [{ex.GetType().Name}] {ex.Message}") : End Try

                        If cmb_ships.Items.Count > 0 Then
                            cmb_ships.SelectedIndex = 0
                            ' Optional: Force processing if SelectionChanged isn't reliable on first load with single item
                            If cmb_ships.Items.Count = 1 Then
                                Dispatcher.BeginInvoke(New Action(Sub() ProcessShipSelection(TryCast(cmb_ships.SelectedItem, Ship))), System.Windows.Threading.DispatcherPriority.Background)
                            End If
                        Else
                            ClearUI() : ClearStorageDisplay()
                            loadErrors.AppendLine("- Warning: No <ship> elements found in the save file.")
                        End If
                    ElseIf loadErrors.Length = 0 Then ' Only report no ships if no other errors occurred
                        loadErrors.AppendLine("- No ships found in the save file.")
                    End If

                    ' Step 5: Report results
                    If loadErrors.Length > 0 Then
                        MessageBox.Show("Save file loaded, but some issues occurred during data reading:" & Environment.NewLine & loadErrors.ToString(), "Load Complete with Issues", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Else
                        MessageBox.Show("Save game loaded successfully!", "Load Complete", MessageBoxButton.OK, MessageBoxImage.Information)
                    End If

                    SetWindowTitle(currentFilePath) ' Update window title

                Catch ex As Exception
                    ' Catch any unexpected errors during the overall loading process
                    ' Show full exception details including stack trace for debugging
                    MessageBox.Show($"An unexpected critical error occurred while loading the save file:{Environment.NewLine}{ex.ToString()}", "Critical Load Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    ResetApplicationState()
                End Try
            End If
        End Sub



        Private Sub LoadGlobalSettings()
            If xmlDoc Is Nothing OrElse xmlDoc.Root Is Nothing Then Return ' Added Root check

            Try
                ' Load Player Credits
                Dim bankElement = xmlDoc.Root.Element("playerBank")
                Dim creditsValue As String = "0" ' Default
                If bankElement IsNot Nothing Then
                    Dim caAttribute = bankElement.Attribute("ca")
                    If caAttribute IsNot Nothing AndAlso Not String.IsNullOrEmpty(caAttribute.Value) Then
                        creditsValue = caAttribute.Value
                    End If
                End If
                txtPlayerCredits.Text = creditsValue

                ' Load Sandbox Mode (Using chkSandbox)
                Dim sandboxIsChecked As Boolean = False ' Default
                ' *** Check your actual XML: Is the element name 'difficulty' or 'diff'? Adjust below if needed. ***
                Dim settingsRootElement = xmlDoc.Root.Element("settings")
                If settingsRootElement IsNot Nothing Then
                    Dim diffElement = settingsRootElement.Element("diff") ' Assuming it's 'diff'
                    If diffElement IsNot Nothing Then
                        Dim sandboxAttribute = diffElement.Attribute("sandbox")
                        If sandboxAttribute IsNot Nothing Then
                            sandboxIsChecked = (sandboxAttribute.Value.ToLowerInvariant() = "true")
                        End If
                    End If
                End If
                chkSandbox.IsChecked = sandboxIsChecked

                ' Load Player Prestige Points
                Dim prestigePoints As Integer = 0 ' Default value
                Try
                    Dim questLines1 = xmlDoc.Root.Element("questLines")
                    If questLines1 IsNot Nothing Then
                        Dim questLines2 = questLines1.Element("questLines")
                        If questLines2 IsNot Nothing Then
                            Dim exodusFleetElement = questLines2.Elements("l").FirstOrDefault(Function(el)
                                                                                                  Dim typeAttr = el.Attribute("type")
                                                                                                  Return typeAttr IsNot Nothing AndAlso typeAttr.Value = "ExodusFleet"
                                                                                              End Function)
                            If exodusFleetElement IsNot Nothing Then
                                Dim prestigeAttr = exodusFleetElement.Attribute("playerPrestigePoints")
                                If prestigeAttr IsNot Nothing Then
                                    Integer.TryParse(prestigeAttr.Value, prestigePoints) ' TryParse handles errors, defaults to 0 if fail
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception
                    Console.WriteLine($"Error loading Prestige Points: {ex.Message}")
                    prestigePoints = 0 ' Ensure default on error
                End Try
                txtPrestigePoints.Text = prestigePoints.ToString()

            Catch ex As Exception
                MessageBox.Show($"Error loading global settings: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error)
                ' Reset UI elements to defaults on error
                txtPlayerCredits.Text = "0"
                chkSandbox.IsChecked = False
                txtPrestigePoints.Text = "0"
            End Try
        End Sub
        ' --- Click Handler for Update Global Settings Button ---
        Private Sub btnUpdateGlobalSettings_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse xmlDoc.Root Is Nothing Then ' Added Root check
                MessageBox.Show("No save file loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim settingsUpdated As Boolean = False
            Dim validationError As Boolean = False
            Dim errorMessages As New System.Text.StringBuilder()

            Try
                ' --- Validate and Update Player Credits (Memory) ---
                Dim bankElement = xmlDoc.Root.Element("playerBank")
                If bankElement IsNot Nothing Then
                    Dim creditsValue As Double
                    If Double.TryParse(txtPlayerCredits.Text, NumberStyles.Any, CultureInfo.InvariantCulture, creditsValue) Then
                        Dim currentCreditsStr As String = Nothing
                        Dim caAttr = bankElement.Attribute("ca")
                        If caAttr IsNot Nothing Then currentCreditsStr = caAttr.Value

                        Dim newCreditsStr = creditsValue.ToString(CultureInfo.InvariantCulture)
                        If currentCreditsStr <> newCreditsStr Then
                            bankElement.SetAttributeValue("ca", newCreditsStr)
                            settingsUpdated = True
                        End If
                    Else
                        errorMessages.AppendLine("- Invalid Credits value. Please enter a valid number.")
                        validationError = True
                    End If
                Else
                    ' If txtPlayerCredits.Text <> "0" Then errorMessages.AppendLine("- Cannot find <playerBank> node to update credits.")
                End If

                ' --- Validate and Update Prestige Points (Memory) ---
                Dim prestigePoints As Integer
                If Integer.TryParse(txtPrestigePoints.Text, prestigePoints) AndAlso prestigePoints >= 0 Then
                    Dim questLines1 = xmlDoc.Root.Element("questLines")
                    If questLines1 IsNot Nothing Then
                        Dim questLines2 = questLines1.Element("questLines")
                        If questLines2 IsNot Nothing Then
                            Dim exodusFleetElement = questLines2.Elements("l").FirstOrDefault(Function(el)
                                                                                                  Dim typeAttr = el.Attribute("type")
                                                                                                  Return typeAttr IsNot Nothing AndAlso typeAttr.Value = "ExodusFleet"
                                                                                              End Function)
                            If exodusFleetElement IsNot Nothing Then
                                Dim currentPrestigeStr As String = Nothing
                                Dim prestigeAttr = exodusFleetElement.Attribute("playerPrestigePoints")
                                If prestigeAttr IsNot Nothing Then currentPrestigeStr = prestigeAttr.Value

                                Dim newPrestigeStr = prestigePoints.ToString()
                                If currentPrestigeStr <> newPrestigeStr Then
                                    exodusFleetElement.SetAttributeValue("playerPrestigePoints", newPrestigeStr)
                                    settingsUpdated = True
                                End If
                            Else
                                ' If txtPrestigePoints.Text <> "0" Then errorMessages.AppendLine("- Cannot find ExodusFleet quest element to update prestige.")
                            End If
                        End If
                    End If
                Else
                    errorMessages.AppendLine("- Invalid Prestige Points value. Please enter a non-negative whole number.")
                    validationError = True
                End If

                ' --- Update Sandbox Mode (Memory) (Using chkSandbox) ---
                ' *** Check your actual XML: Is the element name 'difficulty' or 'diff'? Adjust below if needed. ***
                Dim settingsRootElement = xmlDoc.Root.Element("settings")
                If settingsRootElement IsNot Nothing Then
                    Dim diffElement = settingsRootElement.Element("diff") ' Assuming it's 'diff'
                    If diffElement IsNot Nothing Then
                        Dim currentSandbox As String = Nothing
                        Dim sandboxAttr = diffElement.Attribute("sandbox")
                        If sandboxAttr IsNot Nothing Then currentSandbox = sandboxAttr.Value.ToLowerInvariant()

                        Dim newSandbox = If(chkSandbox.IsChecked.GetValueOrDefault(), "true", "false")
                        If currentSandbox <> newSandbox Then
                            diffElement.SetAttributeValue("sandbox", newSandbox)
                            settingsUpdated = True
                        End If
                    Else
                        ' If chkSandbox.IsChecked.HasValue Then errorMessages.AppendLine("- Cannot find <settings>/<diff> node to update sandbox mode.")
                    End If
                End If

                ' --- Feedback ---
                If validationError Then
                    MessageBox.Show("Please correct the following errors:" & Environment.NewLine & errorMessages.ToString(),
                                 "Validation Failed", MessageBoxButton.OK, MessageBoxImage.Warning)
                ElseIf settingsUpdated Then
                    ' Mark as unsaved
                    MarkUnsavedChanges()
                    MessageBox.Show("Global settings updated in memory. Use File -> Save to make permanent.",
                                 "Globals Updated", MessageBoxButton.OK, MessageBoxImage.Information)
                Else
                    MessageBox.Show("No changes detected in global settings values.",
                                 "Info", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"Error updating global settings in memory: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' --- ADDED: Click Handler for Update Credits Button ---
        Private Sub btnUpdateCredits_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse xmlDoc.Root Is Nothing Then
                MessageBox.Show("No save file loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            Dim newCreditsString As String = txtPlayerCredits.Text
            Dim newCreditsValue As Integer ' Or Long if needed

            If Not Integer.TryParse(newCreditsString, newCreditsValue) OrElse newCreditsValue < 0 Then
                MessageBox.Show("Invalid credits value. Enter non-negative number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Try
                ' --- Find playerBank anywhere under the root ---
                Dim playerBankNode As XElement = xmlDoc.Root.Descendants("playerBank").FirstOrDefault()
                ' --- End Find ---

                If playerBankNode IsNot Nothing Then
                    ' Found it, just update the attribute
                    playerBankNode.SetAttributeValue("ca", newCreditsValue.ToString())
                    MessageBox.Show($"Player credits updated to {newCreditsValue} (in memory). Save the file to make changes permanent.", "Credits Updated", MessageBoxButton.OK, MessageBoxImage.Information)
                Else
                    ' Could not find it - inform user, don't create it automatically
                    MessageBox.Show("<playerBank> element not found in the save file. Cannot update credits automatically.", "XML Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    ' NOTE: If desired, code could be added here to create the <settings> and <playerBank> nodes
                    '       if they are missing entirely, but that's riskier if the structure is unknown.
                End If

            Catch ex As Exception
                MessageBox.Show($"Error updating credits in XML: {ex.Message}", "XML Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub



        Private Sub SettingsMenu_Click(sender As Object, e As RoutedEventArgs)
            Dim settingsWin As New SettingsWindow()
            settingsWin.Owner = Me
            settingsWin.SetInitialValue(_backupEnabled) ' Pass current backup setting
            settingsWin.SetInitialDirectory(My.Settings.DefaultSaveDirectory) ' Pass current directory setting
            Dim result = settingsWin.ShowDialog()

            If result.HasValue AndAlso result.Value = True Then
                ' User clicked OK, update the settings from the dialog
                _backupEnabled = settingsWin.BackupSetting
                My.Settings.DefaultSaveDirectory = settingsWin.DefaultDirectory
                My.Settings.Save() ' Save the settings immediately

                ' Notify the user of the changes
                MessageBox.Show(
            $"Backup on Open setting is now: {_backupEnabled}. " & vbCrLf &
            $"Default Save Directory is now: {My.Settings.DefaultSaveDirectory}. " & vbCrLf &
            "Changes take effect next time you open a file.",
            "Settings Updated",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        )
            End If
        End Sub

#Region "Blueprint Manager Logic"

        ' Helper function to get default save directory from settings for blueprint dialogs
        Private Function GetBlueprintDefaultDirectory() As String
            Try
                Dim defaultDir As String = My.Settings.DefaultSaveDirectory
                If Not String.IsNullOrEmpty(defaultDir) AndAlso Directory.Exists(defaultDir) Then
                    Return defaultDir
                End If
            Catch
                ' If settings fail, continue to fallback
            End Try
            Return GetDefaultSpaceHavenDirectory()
        End Function

        ' Handles selecting the source save file for export
        Private Sub btnSelectSourceSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSelectSourceSave.Click
            Debug.WriteLine(">>> btnSelectSourceSave_Click Entered")
            Dim ofd As New OpenFileDialog With {
                .Title = "Select Source Space Haven Save File ('game')",
                .Filter = "Space Haven Save|game;*.sav|All Files (*.*)|*.*",
                .CheckFileExists = True,
                .InitialDirectory = GetBlueprintDefaultDirectory()
            }
            Dim dlg = ofd.ShowDialog()
            If dlg <> True Then Return
            sourceSavePath = ofd.FileName
            txtSourceSavePath.Text = sourceSavePath
            txtSourceSavePath.ToolTip = sourceSavePath
            UpdateBlueprintStatus("Source save selected. Loading ships...")
            LoadShipsFromSource()
            CheckExportButtonState()
        End Sub

        ' Handles selecting the blueprint XML file for import
        Private Sub btnSelectBlueprintFile_Click(sender As Object, e As RoutedEventArgs) Handles btnSelectBlueprintFile.Click
            Dim ofd As New OpenFileDialog With {
                .Title = "Select Ship Blueprint File",
                .Filter = "Ship Blueprint XML (*.xml)|*.xml|All Files (*.*)|*.*",
                .CheckFileExists = True,
                .InitialDirectory = GetBlueprintDefaultDirectory()
            }
            Dim dlg = ofd.ShowDialog()
            If dlg <> True Then Return
            blueprintFilePath = ofd.FileName
            txtBlueprintFilePath.Text = blueprintFilePath
            txtBlueprintFilePath.ToolTip = blueprintFilePath
            UpdateBlueprintStatus("Blueprint file selected.")
            CheckImportButtonState()
        End Sub

        ' Handles selecting the target save file for import
        Private Sub btnSelectTargetSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSelectTargetSave.Click
            Dim ofd As New OpenFileDialog With {
               .Title = "Select Target Space Haven Save File ('game')",
               .Filter = "Space Haven Save|game;*.sav|All Files (*.*)|*.*",
               .CheckFileExists = True,
               .InitialDirectory = GetBlueprintDefaultDirectory()
           }
            Dim dlg = ofd.ShowDialog()
            If dlg <> True Then Return
            targetSavePath = ofd.FileName
            txtTargetSavePath.Text = targetSavePath
            txtTargetSavePath.ToolTip = targetSavePath
            UpdateBlueprintStatus("Target save file selected.")
            CheckImportButtonState()
        End Sub

        ' Loads ship names and IDs from the selected source save file
        Private Sub LoadShipsFromSource()
            If String.IsNullOrEmpty(sourceSavePath) Then
                Debug.WriteLine("LoadShipsFromSource: sourceSavePath is empty, exiting.")
                Return
            End If

            cmbShipsToExport.ItemsSource = Nothing
            cmbShipsToExport.IsEnabled = False
            sourceXmlDoc = Nothing
            Dim shipsFound As Integer = 0
            Dim shipsAdded As Integer = 0

            Try
                Debug.WriteLine($"LoadShipsFromSource: Attempting to load XML from: {sourceSavePath}")
                sourceXmlDoc = XDocument.Load(sourceSavePath)
                Debug.WriteLine("LoadShipsFromSource: XDocument.Load succeeded.")

                If sourceXmlDoc Is Nothing Then Throw New Exception("XDocument.Load returned Nothing.")
                If sourceXmlDoc.Root Is Nothing Then Throw New Exception("XML document has no Root element.")
                If sourceXmlDoc.Root.Name <> "game" Then Throw New Exception($"Invalid root element name: <{sourceXmlDoc.Root.Name}>. Expected <game>.")

                Dim ships = New List(Of ShipInfo)()
                Dim shipElements = sourceXmlDoc.Root.Descendants("ship").ToList()
                shipsFound = shipElements.Count
                Debug.WriteLine($"LoadShipsFromSource: Found {shipsFound} <ship> elements using Descendants.")

                For Each shipElement In shipElements
                    Dim sidAttr = shipElement.Attribute("sid")
                    Dim snameAttr = shipElement.Attribute("sname")
                    Dim sid As Integer = 0
                    Dim currentSidStr = If(sidAttr IsNot Nothing, sidAttr.Value, "NULL")
                    Dim currentSnameStr = If(snameAttr IsNot Nothing, snameAttr.Value, "NULL")

                    If sidAttr IsNot Nothing AndAlso Integer.TryParse(sidAttr.Value, sid) Then
                        ships.Add(New ShipInfo With {
                            .Sid = sid,
                            .Sname = If(snameAttr IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(snameAttr.Value), snameAttr.Value, $"Unnamed Ship (SID: {sid})")
                        })
                        shipsAdded += 1
                    Else
                        Debug.WriteLine($"LoadShipsFromSource: Skipped ship element. SID attribute missing or invalid: '{currentSidStr}'. Name attribute: '{currentSnameStr}'.")
                    End If
                Next

                Debug.WriteLine($"LoadShipsFromSource: Finished processing ship elements. Added {shipsAdded} ships to list.")

                cmbShipsToExport.ItemsSource = ships.OrderBy(Function(s) s.Sname).ToList()
                cmbShipsToExport.IsEnabled = ships.Any()
                If ships.Any() Then
                    cmbShipsToExport.SelectedIndex = 0
                    UpdateBlueprintStatus($"Ships loaded ({shipsAdded} found). Select ship and click Export.")
                Else
                    UpdateBlueprintStatus($"No valid ships found in source save file ({shipsFound} elements checked).")
                End If
                CheckExportButtonState()

            Catch ex As Exception
                MessageBox.Show($"Error loading ships from source save:{Environment.NewLine}{ex.ToString()}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error)
                sourceXmlDoc = Nothing
                cmbShipsToExport.ItemsSource = Nothing
                cmbShipsToExport.IsEnabled = False
                CheckExportButtonState()
                UpdateBlueprintStatus("Error loading source save.")
            End Try
        End Sub

        Private Sub CheckExportButtonState()
            btnExportBlueprint.IsEnabled = (sourceXmlDoc IsNot Nothing AndAlso cmbShipsToExport.SelectedItem IsNot Nothing)
        End Sub

        Private Sub cmbShipsToExport_SelectionChanged(sender As Object, e As System.Windows.Controls.SelectionChangedEventArgs) Handles cmbShipsToExport.SelectionChanged
            CheckExportButtonState()
        End Sub

        Private Sub btnExportBlueprint_Click(sender As Object, e As RoutedEventArgs) Handles btnExportBlueprint.Click
            Dim selectedShipInfo = TryCast(cmbShipsToExport.SelectedItem, ShipInfo)
            If selectedShipInfo Is Nothing Then
                MessageBox.Show("Please select a ship from the list.", "No Ship Selected", MessageBoxButton.OK, MessageBoxImage.Warning) : Return
            End If
            If sourceXmlDoc Is Nothing OrElse sourceXmlDoc.Root Is Nothing Then
                MessageBox.Show("Source save file is not loaded or is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) : Return
            End If

            Try
                UpdateBlueprintStatus($"Exporting blueprint for '{selectedShipInfo.Sname}'...")
                Dim originalShipElement = sourceXmlDoc.Root.Descendants("ship").FirstOrDefault(Function(s) s.Attribute("sid")?.Value = selectedShipInfo.Sid.ToString())
                If originalShipElement Is Nothing Then Throw New Exception($"Ship node with SID {selectedShipInfo.Sid} not found in source XML.")

                Dim blueprintNodeToSave As New XElement(originalShipElement)

                ' Always remove crew data on export
                blueprintNodeToSave.Element("characters")?.Remove()

                ' Keep inventory data in export - import will handle removing it if needed

                ' Export as blueprint (unbuilt) - set real="0"
                blueprintNodeToSave.SetAttributeValue("real", "0")

                Dim sfd As New Microsoft.Win32.SaveFileDialog() With {
                    .Title = "Save Ship Blueprint As",
                    .Filter = "Ship Blueprint XML (*.xml)|*.xml|All Files (*.*)|*.*",
                    .FileName = $"{selectedShipInfo.Sname}_Blueprint.xml",
                    .InitialDirectory = GetBlueprintDefaultDirectory()
                }

                If sfd.ShowDialog() = True Then
                    blueprintNodeToSave.Save(sfd.FileName)
                    MessageBox.Show($"Blueprint '{Path.GetFileName(sfd.FileName)}' saved successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information)
                    UpdateBlueprintStatus($"Blueprint '{Path.GetFileName(sfd.FileName)}' saved.")
                Else
                    UpdateBlueprintStatus("Export cancelled.")
                End If
            Catch ex As Exception
                MessageBox.Show($"Error exporting blueprint:{Environment.NewLine}{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
                UpdateBlueprintStatus("Export failed.")
            End Try
        End Sub

        Private Sub CheckImportButtonState()
            btnImportBlueprint.IsEnabled = (Not String.IsNullOrEmpty(blueprintFilePath) AndAlso File.Exists(blueprintFilePath) AndAlso
                                            Not String.IsNullOrEmpty(targetSavePath) AndAlso File.Exists(targetSavePath))
        End Sub

        Private Sub btnImportBlueprint_Click(sender As Object, e As RoutedEventArgs) Handles btnImportBlueprint.Click
            If String.IsNullOrEmpty(blueprintFilePath) OrElse Not File.Exists(blueprintFilePath) Then
                MessageBox.Show("Please select a valid blueprint XML file.", "Blueprint Missing", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            If String.IsNullOrEmpty(targetSavePath) OrElse Not File.Exists(targetSavePath) Then
                MessageBox.Show("Please select a valid target save 'game' file.", "Target Save Missing", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim result = MessageBox.Show(
        $"Import '{Path.GetFileName(blueprintFilePath)}' into '{Path.GetFileName(targetSavePath)}'?" & vbCrLf &
        "BACKUP YOUR SAVE FIRST! Do not have the game loaded." & vbCrLf & vbCrLf &
        "Proceed?",
        "Confirm Import", MessageBoxButton.YesNo, MessageBoxImage.Warning
    )
            If result = MessageBoxResult.No Then
                UpdateBlueprintStatus("Import cancelled.")
                Return
            End If

            Me.Cursor = System.Windows.Input.Cursors.Wait
            Try
                UpdateBlueprintStatus("Loading target save…")
                targetXmlDoc = XDocument.Load(targetSavePath)
                If targetXmlDoc.Root Is Nothing Then Throw New Exception("Target save file has no root element.")
                If targetXmlDoc.Root.Name <> "game" Then Throw New Exception("Invalid save file - root element is not <game>.")

                UpdateBlueprintStatus("Loading blueprint…")
                Dim bpDoc = XDocument.Load(blueprintFilePath)
                Dim loadedShip = bpDoc.Descendants("ship").FirstOrDefault()
                If loadedShip Is Nothing Then Throw New Exception("Blueprint has no <ship>.")
                blueprintNode = New XElement(loadedShip)

                ' If importing as blueprint, remove crew, inventory, and built structures (but keep planning grid)
                If chkImportAsBlueprint IsNot Nothing AndAlso chkImportAsBlueprint.IsChecked = True Then
                    ' Remove crew
                    blueprintNode.Element("characters")?.Remove()
                    For Each n In blueprintNode.Descendants().Where(Function(x)
                                                                        Return {"crew", "persons", "prePersons"}.Contains(x.Name.LocalName)
                                                                    End Function).ToList()
                        n.Remove()
                    Next
                    blueprintNode.SetAttributeValue("crew", "0")
                    blueprintNode.SetAttributeValue("cryoCrew", "0")

                    ' Remove inventory/items
                    For Each n In blueprintNode.Descendants().Where(Function(x)
                                                                        Return {"inventory", "item"}.Contains(x.Name.LocalName)
                                                                    End Function).ToList()
                        n.Remove()
                    Next
                    ' Remove inventory from feat elements
                    For Each featNode In blueprintNode.Descendants("feat")
                        featNode.Element("inv")?.Remove()
                    Next

                    ' Remove built structures (but keep planning grid - <e> elements without id)
                    ' Remove all built structure elements (<e> elements with id attribute - walls, floors, etc.)
                    ' NOTE: Keep <e> elements WITHOUT id (these are planning grid markers like <e m="-2">)
                    For Each eElement In blueprintNode.Descendants("e").Where(Function(elem) elem.Attribute("id") IsNot Nothing).ToList()
                        eElement.Remove()
                    Next
                    ' Remove feat elements (features/structures that are built)
                    For Each featElement In blueprintNode.Descendants("feat").ToList()
                        featElement.Remove()
                    Next
                End If

                ' --- 3) Assign new blueprint SID via <game idCounter> ---
                Dim gameRoot = targetXmlDoc.Root
                If gameRoot Is Nothing Then Throw New Exception("Target save file root element is Nothing.")
                Dim idCounterAttr = gameRoot.Attribute("idCounter")

                ' If idCounter is missing, initialize it based on existing ship SIDs or use a default
                Dim idCtr As Long
                If idCounterAttr Is Nothing Then
                    ' Find the highest existing ship SID to determine a safe starting point
                    Dim maxSid As Integer = 0
                    For Each ship In gameRoot.Descendants("ship")
                        Dim sidAttr = ship.Attribute("sid")
                        If sidAttr IsNot Nothing Then
                            Dim sid As Integer
                            If Integer.TryParse(sidAttr.Value, sid) AndAlso sid > maxSid Then
                                maxSid = sid
                            End If
                        End If
                    Next
                    ' Set idCounter to maxSid + 100 to ensure we have plenty of room
                    idCtr = Math.Max(maxSid + 100, 1000)
                    gameRoot.SetAttributeValue("idCounter", idCtr.ToString())
                    UpdateBlueprintStatus($"Initialized missing 'idCounter' attribute to {idCtr} based on existing ships.")
                Else
                    idCtr = CLng(idCounterAttr.Value)
                End If

                Dim newBlueprintSid = CInt(idCtr)
                gameRoot.SetAttributeValue("idCounter", (idCtr + 1).ToString())

                blueprintNode.SetAttributeValue("sid", newBlueprintSid.ToString())
                Dim baseName = blueprintNode.Attribute("sname")?.Value
                blueprintNode.SetAttributeValue("sname", $"{baseName} (Imported)")

                ' Set real attribute and idCnt based on import as blueprint checkbox
                ' If checked, import as blueprint (unbuilt) - real="0", idCnt="0"
                ' If NOT checked, import as built ship - real="1", preserve idCnt
                If chkImportAsBlueprint IsNot Nothing AndAlso chkImportAsBlueprint.IsChecked = True Then
                    blueprintNode.SetAttributeValue("real", "0")
                    blueprintNode.SetAttributeValue("idCnt", "0") ' No built structures in blueprint
                Else
                    blueprintNode.SetAttributeValue("real", "1")
                    ' Preserve idCnt if it exists, otherwise set to 0
                    If blueprintNode.Attribute("idCnt") Is Nothing Then
                        blueprintNode.SetAttributeValue("idCnt", "0")
                    End If
                End If

                Dim shipsEl = gameRoot.Element("ships")
                If shipsEl Is Nothing Then
                    shipsEl = New XElement("ships")
                    gameRoot.Add(shipsEl)
                End If
                shipsEl.Add(blueprintNode)

                Dim bpContainer = gameRoot.Element("blueprints")
                If bpContainer Is Nothing Then
                    bpContainer = New XElement("blueprints")
                    gameRoot.Add(bpContainer)
                End If
                bpContainer.Add(blueprintNode)

                ' --- 6) Allocate new instance ID via <starmap objectIdCounter> ---
                Dim starmap = gameRoot.Element("starmap")
                If starmap Is Nothing Then Throw New Exception("Target save file is missing the <starmap> element.")
                Dim objectIdCounterAttr = starmap.Attribute("objectIdCounter")

                ' If objectIdCounter is missing, initialize it to a safe default
                Dim objCtr As Long
                If objectIdCounterAttr Is Nothing Then
                    ' Use a default starting value for object IDs
                    objCtr = 1000
                    starmap.SetAttributeValue("objectIdCounter", objCtr.ToString())
                    UpdateBlueprintStatus($"Initialized missing 'objectIdCounter' attribute to {objCtr}.")
                Else
                    objCtr = CLng(objectIdCounterAttr.Value)
                End If

                Dim newInstanceId = CInt(objCtr)
                starmap.SetAttributeValue("objectIdCounter", (objCtr + 1).ToString())

                ' --- 7) Find player's system & first empty‐sector slot ---
                Dim gamedataEl = gameRoot.Element("gamedata")
                If gamedataEl Is Nothing Then Throw New Exception("Target save file is missing the <gamedata> element.")
                Dim playerSectorIdAttr = gamedataEl.Attribute("playerSectorId")

                ' If playerSectorId is missing, try to find a valid system ID or use a default
                Dim psid As Integer
                If playerSectorIdAttr Is Nothing Then
                    ' Try to find the first available system ID from starmap
                    Dim firstSystem = starmap.Element("systems")?.Elements("l").FirstOrDefault()
                    If firstSystem IsNot Nothing Then
                        Dim systemIdAttr = firstSystem.Attribute("systemId")
                        If systemIdAttr IsNot Nothing AndAlso Integer.TryParse(systemIdAttr.Value, psid) Then
                            ' Found a valid system ID, set it as playerSectorId
                            gamedataEl.SetAttributeValue("playerSectorId", psid.ToString())
                            UpdateBlueprintStatus($"Initialized missing 'playerSectorId' attribute to {psid} from first available system.")
                        Else
                            ' No valid system found, use default
                            psid = 0
                            gamedataEl.SetAttributeValue("playerSectorId", "0")
                            UpdateBlueprintStatus("Initialized missing 'playerSectorId' attribute to 0 (default).")
                        End If
                    Else
                        ' No systems found, use default
                        psid = 0
                        gamedataEl.SetAttributeValue("playerSectorId", "0")
                        UpdateBlueprintStatus("Initialized missing 'playerSectorId' attribute to 0 (default - no systems found).")
                    End If
                Else
                    psid = Integer.Parse(playerSectorIdAttr.Value)
                End If

                Dim systemsNode = starmap.Element("systems")
                If systemsNode Is Nothing Then Throw New Exception("Target save file is missing the <systems> element within <starmap>.")

                ' Pick your system or fall back to the first
                Dim chosenSystem = systemsNode.Elements("l") _
    .FirstOrDefault(Function(s) s.Attribute("systemId") IsNot Nothing AndAlso CInt(s.Attribute("systemId").Value) = psid)
                If chosenSystem Is Nothing Then
                    chosenSystem = systemsNode.Elements("l").FirstOrDefault()
                    If chosenSystem Is Nothing Then Throw New Exception("Target save file has no system elements in <starmap><systems>.")
                End If

                Dim emptySectorsEl = chosenSystem.Element("emptySectors")
                If emptySectorsEl Is Nothing Then Throw New Exception("Target save file is missing the <emptySectors> element in the chosen system.")
                Dim slot = emptySectorsEl.Elements("l").FirstOrDefault()
                If slot Is Nothing Then Throw New Exception("Target save file has no empty sector slots available.")

                ' Get slot coordinates once for reuse
                Dim slotXAttr = slot.Attribute("x")
                Dim slotYAttr = slot.Attribute("y")
                Dim slotX = If(slotXAttr IsNot Nothing, slotXAttr.Value, "0")
                Dim slotY = If(slotYAttr IsNot Nothing, slotYAttr.Value, "0")

                Dim fleet = slot.Element("fleet")
                If fleet Is Nothing Then
                    fleet = New XElement("fleet")
                    slot.Add(fleet)
                End If

                Dim playerF = fleet.Elements("f") _
                                   .FirstOrDefault(Function(fx) fx.Attribute("isPlayer")?.Value = "true")
                If playerF Is Nothing Then
                    Dim fallbackFac = targetXmlDoc.Root.Descendants("f") _
                                               .FirstOrDefault(Function(fx) fx.Attribute("isPlayer")?.Value = "true")? _
                                               .Attribute("factionId")?.Value
                    Dim fac = If(String.IsNullOrEmpty(fallbackFac), "461", fallbackFac)
                    playerF = New XElement("f",
                        New XAttribute("factionId", fac),
                        New XAttribute("isPlayer", "true"),
                        New XAttribute("id", "0"),
                        New XAttribute("x", slotX),
                        New XAttribute("y", slotY)
                    )
                    fleet.Add(playerF)
                End If
                Dim createdShips = playerF.Element("createdShips")

                If createdShips Is Nothing Then
                    createdShips = New XElement("createdShips")
                    playerF.Add(createdShips)
                End If

                UpdateBlueprintStatus("Placing imported ship…")
                Dim rnd = New Random()
                Dim instSeed = rnd.Next(Integer.MinValue, Integer.MaxValue)
                Dim shipX = slotX
                Dim shipY = slotY

                Dim newShip = New XElement("l",
            New XAttribute("seed", instSeed),
            New XAttribute("createdShipId", newInstanceId.ToString()),
            New XAttribute("slid", newBlueprintSid.ToString()),
            New XAttribute("created", "true"),
            New XAttribute("station", "false"),
            New XAttribute("shipDamagedNoFTL", "false"),
            New XAttribute("crew", If(chkImportAsBlueprint IsNot Nothing AndAlso chkImportAsBlueprint.IsChecked, "0", "0")),
            New XAttribute("cryoCrew", If(chkImportAsBlueprint IsNot Nothing AndAlso chkImportAsBlueprint.IsChecked, "0", "0")),
            New XAttribute("monsters", "0"),
            New XAttribute("bigMonsters", "0"),
            New XAttribute("hives", "0"),
            New XAttribute("infesters", "0"),
            New XAttribute("flybots", "0"),
            New XAttribute("walkers", "0"),
            New XAttribute("roboBase", "0"),
            New XAttribute("derelict", "false"),
            New XAttribute("addLoot", "false"),
            New XAttribute("inHyper", "false"),
            New XAttribute("sx", shipX),
            New XAttribute("sy", shipY),
            New XAttribute("shn", baseName)
        )
                createdShips.Add(newShip)

                Try
                    Dim bak = targetSavePath & ".bak"
                    File.Copy(targetSavePath, bak, True)
                    UpdateBlueprintStatus($"Backup created: {Path.GetFileName(bak)}")
                Catch ex As Exception
                    UpdateBlueprintStatus($"Warning: backup failed – {ex.Message}")
                End Try

                targetXmlDoc.Save(targetSavePath)
                UpdateBlueprintStatus($"Import complete: SID {newBlueprintSid}, Instance #{newInstanceId}.")
                MessageBox.Show("Ship imported successfully!", "Done", MessageBoxButton.OK, MessageBoxImage.Information)

            Catch ex As Exception
                MessageBox.Show($"Import Error:{vbCrLf}{ex.Message}{vbCrLf}{vbCrLf}Stack Trace:{vbCrLf}{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                UpdateBlueprintStatus($"Import failed: {ex.Message}")
                Debug.WriteLine($"Import Error: {ex.ToString()}")
            Finally
                Me.Cursor = System.Windows.Input.Cursors.Arrow
            End Try
        End Sub

        Private Sub UpdateBlueprintStatus(message As String)
            Me.Dispatcher.Invoke(Sub()
                                     txtBlueprintStatus.Text = message
                                 End Sub,
                                 DispatcherPriority.Background)
        End Sub

#End Region



        ' --- SIMPLIFIED SaveFileMenu_Click ---
        Private Sub SaveFileMenu_Click(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrEmpty(currentFilePath) OrElse xmlDoc Is Nothing Then
                MessageBox.Show("No file loaded to save.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Optional: Commit any pending grid edits if needed
            Try : dgvStorage.CommitEdit(DataGridEditingUnit.Row, True) : Catch : End Try
            Try : dgvRelationships.CommitEdit(DataGridEditingUnit.Row, True) : Catch : End Try

            Try
                ' Global settings should have been updated in memory via the 'Update Globals' button.
                ' Grid/List changes should be handled by their respective edit/add/delete handlers updating the xmlDoc in memory.

                ' Update character changes in XML before saving
                UpdateXmlWithCharacterChanges()

                ' Save the entire in-memory XML document to the file
                xmlDoc.Save(currentFilePath)
                ClearUnsavedChanges() ' Reset unsaved changes flag after successful save
                MessageBox.Show("File saved successfully!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information)

            Catch ex As Exception
                MessageBox.Show($"Error saving file: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub


        ' --- Add Save Menu Handler ---
        'Private Sub SaveFileMenu_Click(sender As Object, e As RoutedEventArgs)
        '    ' Force commit any pending grid edit before saving
        '    Try : dgvStorage.CommitEdit(DataGridEditingUnit.Row, True) : Catch : End Try

        '    If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(currentFilePath) Then MessageBox.Show("No file loaded.", "Warn", MessageBoxButton.OK, MessageBoxImage.Warning) : Return

        '    Dim settingsUpdated As Boolean = False ' Track if any global setting actually changed

        '    ' --- Actual Save Operation ---
        '    Try
        '        ' --- ADDED: Update Global Settings IN MEMORY before saving ---
        '        ' Update Credits
        '        Dim newCreditsString = txtPlayerCredits.Text : Dim newCreditsValue As Integer
        '        If Integer.TryParse(newCreditsString, newCreditsValue) AndAlso newCreditsValue >= 0 Then
        '            Dim playerBankNode = xmlDoc.Root.Descendants("playerBank").FirstOrDefault()
        '            If playerBankNode IsNot Nothing Then
        '                If playerBankNode.Attribute("ca")?.Value <> newCreditsValue.ToString() Then
        '                    playerBankNode.SetAttributeValue("ca", newCreditsValue.ToString())
        '                    settingsUpdated = True
        '                End If
        '            Else
        '                ' Optionally warn user that playerBank node wasn't found if they tried to edit
        '                If Not String.IsNullOrWhiteSpace(txtPlayerCredits.Text) Then ' Check if user actually typed something
        '                    MessageBox.Show("<playerBank> node not found. Credits cannot be saved.", "Save Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
        '                End If
        '            End If
        '        ElseIf Not String.IsNullOrWhiteSpace(txtPlayerCredits.Text) Then ' Check if user typed invalid data
        '            MessageBox.Show($"Invalid Credits value ('{newCreditsString}') entered. Credits not saved. Please correct and save again.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error)
        '            Return ' Stop the save if credits are invalid
        '        End If

        '        ' Update Sandbox
        '        Dim diffNode = xmlDoc.Root.Element("settings")?.Element("diff")
        '        If diffNode IsNot Nothing Then
        '            Dim currentSandboxValue = diffNode.Attribute("sandbox")?.Value
        '            Dim newSandboxValue = If(chkSandbox.IsChecked.GetValueOrDefault(False), "true", "false") ' Use GetValueOrDefault
        '            If currentSandboxValue <> newSandboxValue Then
        '                diffNode.SetAttributeValue("sandbox", newSandboxValue)
        '                settingsUpdated = True
        '            End If
        '        Else
        '            ' Optionally warn if user changed checkbox but diff node is missing
        '            ' If chkSandbox.IsChecked.HasValue Then ...
        '        End If
        '        ' --- End Update Global Settings ---


        '        UpdateXmlWithCharacterChanges() ' TODO: Implement saving character edits

        '        ' Save the entire in-memory XML document
        '        xmlDoc.Save(currentFilePath)

        '        ' Optionally provide combined feedback
        '        If settingsUpdated Then
        '            MessageBox.Show("File saved successfully (including updated global settings).", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
        '        Else
        '            MessageBox.Show("File saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information)
        '        End If

        '    Catch ex As Exception
        '        MessageBox.Show($"Error saving file: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error)
        '    End Try ' End Try block for saving

        'End Sub

        Private Sub ClearGlobalSettingsUI()
            txtPlayerCredits.Text = ""
            chkSandbox.IsChecked = False
        End Sub


        ' Update XML with character changes before saving
        Private Sub UpdateXmlWithCharacterChanges()
            If xmlDoc Is Nothing OrElse characters Is Nothing Then Return

            Try
                For Each characterToUpdate In characters
                    ' Find the character's <c> node in xmlDoc based on CharacterEntityId
                    Dim charNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = characterToUpdate.CharacterEntityId.ToString())
                    If charNode IsNot Nothing Then
                        Dim persElement = charNode.Element("pers")
                        If persElement IsNot Nothing Then
                            ' Update Attributes
                            Dim attrsElement = persElement.Element("attr")
                            If attrsElement IsNot Nothing Then
                                For Each localAttr In characterToUpdate.CharacterAttributes
                                    Dim attrNode = attrsElement.Elements("a").FirstOrDefault(Function(a) a.Attribute("id")?.Value = localAttr.Id.ToString())
                                    If attrNode IsNot Nothing Then
                                        attrNode.SetAttributeValue("points", localAttr.Value.ToString())
                                    End If
                                Next
                            End If

                            ' Update Skills (both level and mxn)
                            Dim skillsElement = persElement.Element("skills")
                            If skillsElement IsNot Nothing Then
                                For Each localSkill In characterToUpdate.CharacterSkills
                                    Dim skillNode = skillsElement.Elements("s").FirstOrDefault(Function(s) s.Attribute("sk")?.Value = localSkill.Id.ToString())
                                    If skillNode IsNot Nothing Then
                                        skillNode.SetAttributeValue("level", localSkill.Value.ToString())
                                        skillNode.SetAttributeValue("mxn", localSkill.MaxValue.ToString())
                                    End If
                                Next
                            End If

                            ' Update Traits (add/remove as needed)
                            Dim traitsElement = persElement.Element("traits")
                            If traitsElement IsNot Nothing Then
                                ' Remove all existing traits
                                traitsElement.Elements("t").Remove()
                                ' Add current traits
                                For Each localTrait In characterToUpdate.CharacterTraits
                                    Dim newTraitElement = New XElement("t")
                                    newTraitElement.SetAttributeValue("id", localTrait.Id.ToString())
                                    traitsElement.Add(newTraitElement)
                                Next
                            End If
                        End If
                    End If
                Next
            Catch ex As Exception
                MessageBox.Show($"Error updating character changes: {ex.Message}", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub LoadOwnerInfo(shipSid As Integer) ' Helper to load owner info
            Dim ownerText = "Owner: Unknown" ' Default text
            If xmlDoc IsNot Nothing Then
                Try
                    ' Find the specific ship element in the loaded XML
                    Dim shipElement = xmlDoc.Descendants("ship").FirstOrDefault(Function(s) s.Attribute("sid")?.Value = shipSid.ToString())
                    If shipElement IsNot Nothing Then
                        ' Find the <settings> element within this ship
                        Dim settingsElement = shipElement.Element("settings")
                        If settingsElement IsNot Nothing Then
                            ' Get the value of the "owner" attribute
                            Dim ownerValue As String = settingsElement.Attribute("owner")?.Value
                            If Not String.IsNullOrEmpty(ownerValue) Then
                                ' Check if the owner value indicates the player
                                ownerText = If(ownerValue.Equals("Player", StringComparison.OrdinalIgnoreCase),
                                       "Owner: Player",
                                       $"Owner: {ownerValue}") ' Display actual owner if not "Player"
                            Else
                                ownerText = "Owner: Not Specified" ' Owner attribute missing value
                            End If
                        Else
                            ownerText = "Owner: Settings Missing" ' <settings> tag missing in ship
                        End If
                    Else
                        ownerText = "Owner: Ship Node Error" ' Ship XML element not found (shouldn't happen if SID is valid)
                    End If
                Catch ex As Exception
                    ' Handle potential errors during XML parsing for this specific part
                    ownerText = "Owner: Load Error"
                    ' Optionally log the exception ex.Message
                End Try
            End If
            ' Update the label text
            lbl_owner.Text = ownerText
        End Sub

        Private Sub ProcessShipSelection(selectedShip As Ship)
            If selectedShip Is Nothing OrElse selectedShip.Sid = -1 Then
                ClearUI()
                ClearStorageDisplay()
                Return
            End If

            ' --- This is the logic moved from the original SelectionChanged handler ---
            ' Display ship size
            lbl_shipSize.Text = $"Size: {selectedShip.Sx}x{selectedShip.Sy}"
            Dim canvasWidth As Integer = selectedShip.Sx \ 28 ' Integer division
            Dim canvasHeight As Integer = selectedShip.Sy \ 28 ' Integer division
            lbl_CanvasSize.Text = $"Canvas Size: {canvasWidth} W x {canvasHeight} H squares"

            ' Load owner info
            LoadOwnerInfo(selectedShip.Sid)

            ' Load crew for the selected ship
            LoadCrewForShip(selectedShip.Sid)

            ' Load Storage Containers for the selected ship
            LoadStorageContainers(selectedShip.Sid)

            'Bulk Crew Add (Template Crew)
            PopulateTemplateCrewComboBox(selectedShip.Sid)

        End Sub



        ' Load ships into the ComboBox and internal list
        Private Sub LoadShips()
            If xmlDoc Is Nothing Then Return

            cmb_ships.ItemsSource = Nothing
            cmb_ships.Items.Clear()
            ships.Clear()
            Dim tempShipList As New List(Of Ship)()

            For Each shipXml In xmlDoc.Descendants("ship")
                Dim sid = 0 : Integer.TryParse(shipXml.Attribute("sid")?.Value, sid)
                If sid = 0 Then Continue For ' Skip if SID is invalid

                Dim sname = If(shipXml.Attribute("sname")?.Value, "Unnamed Ship")
                Dim sxVal = 0 : Integer.TryParse(shipXml.Attribute("sx")?.Value, sxVal)
                Dim syVal = 0 : Integer.TryParse(shipXml.Attribute("sy")?.Value, syVal)

                ' Check if ship with this SID already exists in the main 'ships' list before adding
                If Not ships.Any(Function(s) s.Sid = sid) Then
                    Dim newShip = New Ship() With {.Sid = sid, .Sname = sname, .Sx = sxVal, .Sy = syVal}
                    ships.Add(newShip) ' Add to the main list
                    tempShipList.Add(newShip) ' Add to the list for the ComboBox
                End If
            Next

            ' Bind the temporary list (sorted) to the ComboBox
            cmb_ships.ItemsSource = tempShipList.OrderBy(Function(s) s.Sname).ToList()
            cmb_ships.DisplayMemberPath = "Sname"
            cmb_ships.SelectedValuePath = "Sid"

            ' --- CHANGE IS HERE ---
            If tempShipList.Any() Then
                cmb_ships.SelectedIndex = 0 ' Select the first item

                ' *** If there's only one ship, manually trigger the processing ***
                If tempShipList.Count = 1 Then
                    Dim singleShip = TryCast(cmb_ships.SelectedItem, Ship)
                    If singleShip IsNot Nothing Then
                        ' Use Dispatcher to ensure UI is ready before processing
                        Dispatcher.BeginInvoke(New Action(Sub() ProcessShipSelection(singleShip)), System.Windows.Threading.DispatcherPriority.Background)
                    End If
                End If
                ' --- END OF CHANGE ---
            Else
                ' No ships found, clear related UI
                ClearUI()
                ClearStorageDisplay()
            End If
        End Sub

        ' Load characters into the internal list
        Private Sub LoadCharacters()
            characters.Clear() : If xmlDoc Is Nothing Then Return
            For Each shipXml In xmlDoc.Descendants("ship")
                Dim shipSid = 0 : Integer.TryParse(shipXml.Attribute("sid")?.Value, shipSid) : If shipSid = 0 Then Continue For
                Dim charactersElement = shipXml.Element("characters")
                If charactersElement IsNot Nothing Then
                    For Each cNode In charactersElement.Elements("c")
                        Dim charName = If(cNode.Attribute("name")?.Value, "Unknown")
                        Dim entId = 0 : Integer.TryParse(cNode.Attribute("entId")?.Value, entId)
                        If Not characters.Any(Function(ch) ch.CharacterEntityId = entId) Then
                            Dim character As New Character With {.CharacterName = charName, .CharacterEntityId = entId, .ShipSid = shipSid}
                            LoadSkillsTraitsAttrs(cNode, character) ' Use helper
                            characters.Add(character)
                        End If
                    Next
                End If
            Next
        End Sub
        ' Helper to load details for a character node
        Private Sub LoadSkillsTraitsAttrs(cNode As XElement, character As Character)
            Dim persNode = cNode.Element("pers")
            ' Load Skills
            Dim skillsEl = cNode.Element("pers")?.Element("skills")
            If skillsEl IsNot Nothing Then
                For Each sNode In skillsEl.Elements("s")
                    Dim skillId = 0 : Integer.TryParse(sNode.Attribute("sk")?.Value, skillId)
                    Dim skillLevel = 0 : Integer.TryParse(sNode.Attribute("level")?.Value, skillLevel)
                    Dim skillMax = 0 : Integer.TryParse(sNode.Attribute("mxn")?.Value, skillMax)
                    character.CharacterSkills.Add(New DataProp With {.Id = skillId, .Name = GetSkillNameById(skillId), .Value = skillLevel, .MaxValue = skillMax})
                Next
            End If
            ' Load Traits
            Dim traitsEl = cNode.Element("pers")?.Element("traits")
            If traitsEl IsNot Nothing Then
                For Each tNode In traitsEl.Elements("t")
                    Dim traitId = 0 : Integer.TryParse(tNode.Attribute("id")?.Value, traitId)
                    character.CharacterTraits.Add(New DataProp With {.Id = traitId, .Name = GetTraitNameById(traitId)})
                Next
            End If
            ' Load Attributes
            Dim attrsEl = cNode.Element("pers")?.Element("attr")
            If attrsEl IsNot Nothing Then
                For Each aNode In attrsEl.Elements("a")
                    Dim attrId = 0 : Integer.TryParse(aNode.Attribute("id")?.Value, attrId)
                    Dim attrPoints = 0 : Integer.TryParse(aNode.Attribute("points")?.Value, attrPoints)
                    character.CharacterAttributes.Add(New DataProp With {.Id = attrId, .Name = GetAttributeNameById(attrId), .Value = attrPoints})
                Next
            End If
            ' Load Conditions ---

            Dim conditionsEl = persNode.Element("conditions")
            If conditionsEl IsNot Nothing Then
                For Each conditionElement In conditionsEl.Elements("c") ' Use correct element name "c"
                    Dim condId As Integer = 0
                    If Integer.TryParse(conditionElement.Attribute("id")?.Value, condId) Then
                        ' --- Check if the ID exists in our dictionary ---
                        If IdCollection.ConditionsIDs.ContainsKey(condId) Then
                            ' Only add if the condition ID is known
                            Dim condName As String = IdCollection.ConditionsIDs(condId)
                            character.CharacterConditions.Add(New DataProp With {.Id = condId, .Name = condName})
                            ' Else: ID not in dictionary, so we simply do nothing (skip/hide it)
                        End If
                        ' --- End check ---
                    End If
                Next
            End If

            Dim socialityNode = persNode.Element("sociality")
            Dim relationshipsNode = socialityNode?.Element("relationships")
            If relationshipsNode IsNot Nothing Then
                For Each relElement In relationshipsNode.Elements("l")
                    Dim targetId As Integer = 0
                    Dim friendship As Integer = 0
                    Dim attraction As Integer = 0
                    Dim compatibility As Integer = 0

                    ' Safely parse attributes
                    Integer.TryParse(relElement.Attribute("targetId")?.Value, targetId)
                    Integer.TryParse(relElement.Attribute("friendship")?.Value, friendship)
                    Integer.TryParse(relElement.Attribute("attraction")?.Value, attraction)
                    Integer.TryParse(relElement.Attribute("compatibility")?.Value, compatibility)

                    If targetId <> 0 Then ' Only add if targetId is valid
                        ' Lookup target character name from the main 'characters' list
                        Dim targetCharacter = characters.FirstOrDefault(Function(c) c.CharacterEntityId = targetId)
                        Dim targetName As String = If(targetCharacter IsNot Nothing, targetCharacter.CharacterName, $"Unknown ID ({targetId})")

                        character.CharacterRelationships.Add(New RelationshipInfo(targetId, targetName, friendship, attraction, compatibility))
                    End If
                Next
            End If

            ' Load Uniform Data
            Dim colorsNode = cNode.Element("colors")
            If colorsNode IsNot Nothing Then
                ' Load shirtSet and pantsSet (the color set IDs)
                Dim shirtSetAttr = colorsNode.Attribute("shirtSet")
                character.ShirtColorIndex = If(shirtSetAttr IsNot Nothing, shirtSetAttr.Value, "")

                Dim pantsSetAttr = colorsNode.Attribute("pantsSet")
                character.PantsColorIndex = If(pantsSetAttr IsNot Nothing, pantsSetAttr.Value, "")

                ' Load sp (shirt color index) and sl (pants color index)
                Dim spAttr = colorsNode.Attribute("sp")
                If spAttr IsNot Nothing AndAlso Integer.TryParse(spAttr.Value, character.ShirtColorIndexValue) Then
                    ' Value loaded
                Else
                    character.ShirtColorIndexValue = 0
                End If

                Dim slAttr = colorsNode.Attribute("sl")
                If slAttr IsNot Nothing AndAlso Integer.TryParse(slAttr.Value, character.PantsColorIndexValue) Then
                    ' Value loaded
                Else
                    character.PantsColorIndexValue = 0
                End If

                Dim skinSetAttr = colorsNode.Attribute("skinSet")
                character.SkinSet = If(skinSetAttr IsNot Nothing, skinSetAttr.Value, "")

                Dim glovesOffAttr = colorsNode.Attribute("glovesOff")
                character.GlovesOff = If(glovesOffAttr IsNot Nothing, glovesOffAttr.Value = "true", False)

                Dim longSleeveAttr = colorsNode.Attribute("longSleeve")
                character.LongSleeve = If(longSleeveAttr IsNot Nothing, longSleeveAttr.Value = "true", False)
            End If
        End Sub

        ' Load crew list for the selected ship ID
        Private Sub LoadCrewForShip(shipSid As Integer)
            Dim shipCrew = characters.Where(Function(c) c.ShipSid = shipSid) _
                                 .OrderBy(Function(c) c.CharacterName) _
                                 .ToList()
            lstCharacters.ItemsSource = shipCrew
            If shipCrew IsNot Nothing Then
                txtCrewCount.Text = $"Total Crew: {shipCrew.Count}"
            Else
                txtCrewCount.Text = "Total Crew: 0"
            End If
            ClearDataGrids()
        End Sub

        ' Handle character selection change
        Private Sub lstCharacters_SelectedIndexChanged(sender As Object, e As SelectionChangedEventArgs)
            If lstCharacters.SelectedItem Is Nothing Then
                ClearDataGrids()
                Return
            End If
            Dim selectedCharacter = TryCast(lstCharacters.SelectedItem, Character)
            If selectedCharacter IsNot Nothing Then
                dgvAttributes.ItemsSource = New ObservableCollection(Of DataProp)(selectedCharacter.CharacterAttributes)
                dgvSkills.ItemsSource = New ObservableCollection(Of DataProp)(selectedCharacter.CharacterSkills.OrderBy(Function(s) s.Id))
                dgvTraits.ItemsSource = New ObservableCollection(Of DataProp)(selectedCharacter.CharacterTraits)
                lstConditions.ItemsSource = New ObservableCollection(Of DataProp)(selectedCharacter.CharacterConditions)
                _relationshipsCurrentPage = 1 ' Reset to first page
                LoadRelationshipsPage()

                PopulateAddTraitComboBox(selectedCharacter)
            End If
        End Sub


        Private Sub LoadRelationshipsPage()
            Dim selectedCharacter = TryCast(lstCharacters.SelectedItem, Character)
            If selectedCharacter Is Nothing OrElse selectedCharacter.CharacterRelationships Is Nothing Then
                dgvRelationships.ItemsSource = Nothing
                txtRelationshipPageInfo.Text = "Page 0 of 0"
                btnRelPrev.IsEnabled = False
                btnRelNext.IsEnabled = False
                Return
            End If

            Dim totalItems = selectedCharacter.CharacterRelationships.Count
            Dim totalPages = CInt(Math.Ceiling(CDbl(totalItems) / _relationshipsPageSize))
            If totalPages = 0 Then totalPages = 1 ' Show page 1 of 1 even if empty

            ' Clamp current page within valid range
            If _relationshipsCurrentPage < 1 Then _relationshipsCurrentPage = 1
            If _relationshipsCurrentPage > totalPages Then _relationshipsCurrentPage = totalPages

            ' Get the subset for the current page
            Dim pagedList = selectedCharacter.CharacterRelationships _
                                .OrderBy(Function(r) r.TargetName) _
                                .Skip((_relationshipsCurrentPage - 1) * _relationshipsPageSize) _
                                .Take(_relationshipsPageSize) _
                                .ToList()

            dgvRelationships.ItemsSource = New ObservableCollection(Of RelationshipInfo)(pagedList)

            ' Update paging info text and button states
            txtRelationshipPageInfo.Text = $"Page {_relationshipsCurrentPage} of {totalPages}"
            btnRelPrev.IsEnabled = (_relationshipsCurrentPage > 1)
            btnRelNext.IsEnabled = (_relationshipsCurrentPage < totalPages)
        End Sub

        Private Sub btnRelNext_Click(sender As Object, e As RoutedEventArgs)
            ' Calculate total pages again in case data changed (though unlikely without full reload here)
            Dim selectedCharacter = TryCast(lstCharacters.SelectedItem, Character)
            If selectedCharacter IsNot Nothing AndAlso selectedCharacter.CharacterRelationships IsNot Nothing Then
                Dim totalItems = selectedCharacter.CharacterRelationships.Count
                Dim totalPages = CInt(Math.Ceiling(CDbl(totalItems) / _relationshipsPageSize))
                If _relationshipsCurrentPage < totalPages Then
                    _relationshipsCurrentPage += 1
                    LoadRelationshipsPage()
                End If
            End If
        End Sub

        ' --- ADDED: Click Handlers for Paging Buttons ---
        Private Sub btnRelPrev_Click(sender As Object, e As RoutedEventArgs)
            If _relationshipsCurrentPage > 1 Then
                _relationshipsCurrentPage -= 1
                LoadRelationshipsPage()
            End If
        End Sub

        ''' <summary>
        ''' Loads storage containers based on <feat> elements having an "eatAllowed" attribute (value ignored)
        ''' and containing an <inv> descendant.
        ''' </summary>
        ''' <param name="shipSid">The SID of the ship to load containers for.</param>
        Private Sub LoadStorageContainers(shipSid As Integer)
            ClearStorageDisplay()
            currentShipStorageContainers.Clear()
            If xmlDoc Is Nothing Then Return

            Dim shipElement = xmlDoc.Descendants("ship").FirstOrDefault(Function(s) s.Attribute("sid")?.Value = shipSid.ToString())
            If shipElement Is Nothing Then Return

            ' --- LINQ: Find <feat> elements that HAVE eatAllowed attribute AND contain <inv> ---
            Dim storageFeatElementsQuery = From feat In shipElement.Descendants("feat")
                                           Where feat.Attribute("eatAllowed") IsNot Nothing AndAlso feat.Descendants("inv").Any()
                                           Select feat

            Dim storageFeatElements As List(Of XElement)
            Try
                storageFeatElements = storageFeatElementsQuery.ToList()
            Catch ex As Exception
                Return ' silently fail or log if needed
            End Try

            Dim containerIndex As Integer = 0
            For Each featElement As XElement In storageFeatElements
                Dim container As New StorageContainer(featElement, containerIndex)

                Dim invElement = featElement.Descendants("inv").FirstOrDefault()
                If invElement IsNot Nothing Then
                    For Each itemElement In invElement.Elements("s")
                        Dim itemId As Integer
                        Dim quantity As Integer
                        If Integer.TryParse(itemElement.Attribute("elementaryId")?.Value, itemId) AndAlso
                   Integer.TryParse(itemElement.Attribute("inStorage")?.Value, quantity) AndAlso quantity > 0 Then
                            container.Items.Add(New StorageItem(itemId, quantity))
                        End If
                    Next

                    If container.Items.Any() Then
                        currentShipStorageContainers.Add(container)
                    End If
                End If

                containerIndex += 1
            Next

            cmbStorageContainers.ItemsSource = currentShipStorageContainers.OrderBy(Function(c) c.DisplayName).ToList()
            If cmbStorageContainers.Items.Count > 0 Then
                cmbStorageContainers.SelectedIndex = 0
            Else
                dgvStorage.ItemsSource = Nothing
            End If
        End Sub








        Private Sub cmbStorageContainers_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            If cmbStorageContainers.SelectedItem Is Nothing Then
                dgvStorage.ItemsSource = Nothing
                CurrentContainerItems = Nothing
                ' txtContainerInfo.Text = "(Select Container)" ' Update handled by helper call
                UpdateContainerInfoText(Nothing) ' Call helper with Nothing
                Return
            End If

            Dim selectedContainer = TryCast(cmbStorageContainers.SelectedItem, StorageContainer)

            If selectedContainer IsNot Nothing Then
                CurrentContainerItems = New ObservableCollection(Of StorageItem)(selectedContainer.Items.OrderBy(Function(item) item.Name))
                dgvStorage.ItemsSource = CurrentContainerItems

                ' --- Use Helper Function ---
                UpdateContainerInfoText(CurrentContainerItems)
                ' --- End Change ---

            Else
                dgvStorage.ItemsSource = Nothing
                CurrentContainerItems = Nothing
                UpdateContainerInfoText(Nothing) ' Call helper with Nothing
            End If
        End Sub

        ' *** btnAddItem_Click - Uses FeatElement Reference ***
        'Private Sub btnAddItem_Click(sender As Object, e As RoutedEventArgs)
        '    If cmbStorageContainers.SelectedItem Is Nothing Then
        '        MessageBox.Show("Please select a storage container first.", "No Container Selected", MessageBoxButton.OK, MessageBoxImage.Warning) : Return
        '    End If
        '    If cmbAddItem.SelectedValue Is Nothing Then
        '        MessageBox.Show("Please select an item to add.", "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning) : Return
        '    End If
        '    If xmlDoc Is Nothing Then
        '        MessageBox.Show("Save file not loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) : Return
        '    End If

        '    Dim selectedContainer = TryCast(cmbStorageContainers.SelectedItem, StorageContainer)
        '    Dim itemId = CInt(cmbAddItem.SelectedValue)
        '    Dim quantity As Integer
        '    If Not Integer.TryParse(txtAddQuantity.Text, quantity) OrElse quantity <= 0 Then
        '        MessageBox.Show("Please enter a valid positive quantity.", "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning) : Return
        '    End If

        '    ' Get the target <feat> element directly from the selected container
        '    Dim targetFeatElement = selectedContainer.FeatElement
        '    If targetFeatElement Is Nothing Then
        '        MessageBox.Show("Error: Selected container object missing its XML element reference.", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error) : Return
        '    End If

        '    ' Find the <inv> element within this specific <feat> element
        '    Dim targetInv = targetFeatElement.Descendants("inv").FirstOrDefault()
        '    If targetInv Is Nothing Then
        '        MessageBox.Show($"Container '{selectedContainer.DisplayName}' lacks an <inv> node. Creating one.", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
        '        targetInv = New XElement("inv")
        '        targetFeatElement.Add(targetInv) ' Add it to the <feat> element
        '    End If

        '    ' Add/Update item within the targetInv
        '    Dim existingItemElement = targetInv.Elements("s").FirstOrDefault(Function(s) s.Attribute("elementaryId")?.Value = itemId.ToString())
        '    If existingItemElement IsNot Nothing Then
        '        Dim currentQuantity As Integer = 0 : Integer.TryParse(existingItemElement.Attribute("inStorage")?.Value, currentQuantity)
        '        existingItemElement.SetAttributeValue("inStorage", (currentQuantity + quantity).ToString())
        '        existingItemElement.SetAttributeValue("onTheWayIn", "0") : existingItemElement.SetAttributeValue("onTheWayOut", "0")
        '    Else
        '        Dim newItemElement = New XElement("s", New XAttribute("elementaryId", itemId.ToString()), New XAttribute("inStorage", quantity.ToString()), New XAttribute("onTheWayIn", "0"), New XAttribute("onTheWayOut", "0"))
        '        targetInv.Add(newItemElement)
        '    End If

        '    ' Update in-memory list and refresh UI
        '    Dim itemInMemory = selectedContainer.Items.FirstOrDefault(Function(i) i.ElementId = itemId)
        '    If itemInMemory IsNot Nothing Then
        '        itemInMemory.Quantity += quantity
        '    Else
        '        selectedContainer.Items.Add(New StorageItem(itemId, quantity)) ' Assumes StorageItem class exists
        '    End If
        '    dgvStorage.ItemsSource = Nothing ' Force refresh
        '    dgvStorage.ItemsSource = selectedContainer.Items.OrderBy(Function(item) item.Name).ToList()
        '    MessageBox.Show($"{quantity}x {CType(cmbAddItem.SelectedItem, KeyValuePair(Of Integer, String)).Value} added/updated in {selectedContainer.DisplayName} (in memory). Save the file to persist.", "Item Added/Updated", MessageBoxButton.OK, MessageBoxImage.Information)
        'End Sub


        ''' <summary>
        ''' Adds the selected item and quantity to the currently selected storage container.
        ''' </summary>
        Private Sub btnAddItem_Click(sender As Object, e As RoutedEventArgs)
            ' Check if a container is selected
            If cmbStorageContainers.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a storage container first.", "No Container Selected", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return ' Exit Sub if no container selected
            End If ' This End If is CORRECT

            ' Check if an item is selected in the Add Item dropdown
            If cmbAddItem.SelectedValue Is Nothing Then
                MessageBox.Show("Please select an item to add.", "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return ' Exit Sub if no item selected
            End If ' This End If is CORRECT

            ' Check if the XML document is loaded
            If xmlDoc Is Nothing Then
                MessageBox.Show("Save file not loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return ' Exit Sub if file not loaded
            End If ' This End If is CORRECT

            Dim selectedContainer = TryCast(cmbStorageContainers.SelectedItem, StorageContainer)
            Dim itemId As Integer = CInt(cmbAddItem.SelectedValue) ' Get the selected Item ID
            Dim itemName As String = IdCollection.DefaultStorageIDs(itemId) ' Get name for message box
            Dim quantity As Integer

            ' Validate the quantity input
            If Not Integer.TryParse(txtAddQuantity.Text, quantity) OrElse quantity <= 0 Then
                MessageBox.Show("Please enter a valid positive quantity.", "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return ' Exit Sub if quantity invalid
            End If ' This End If is CORRECT

            ' Get the target <feat> element directly from the selected container object
            Dim targetFeatElement As XElement = selectedContainer.FeatElement
            If targetFeatElement Is Nothing Then
                MessageBox.Show("Error: Selected container object is missing its internal XML element reference. Cannot modify.", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return ' Exit Sub if reference is broken
            End If ' This End If is CORRECT

            ' Find the <inv> element within this specific <feat> element
            Dim targetInv As XElement = targetFeatElement.Descendants("inv").FirstOrDefault()

            ' If <inv> doesn't exist within the <feat>, create it
            If targetInv Is Nothing Then
                MessageBox.Show($"Container '{selectedContainer.DisplayName}' lacks an <inv> node. Creating one.", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
                targetInv = New XElement("inv")
                targetFeatElement.Add(targetInv) ' Add it to the <feat> element
            End If

            ' --- Add/Update item logic within the specific targetInv ---
            Dim existingItemElement As XElement = targetInv.Elements("s").FirstOrDefault(Function(s) s.Attribute("elementaryId")?.Value = itemId.ToString())

            If existingItemElement IsNot Nothing Then
                ' Item already exists in this container, update quantity
                Dim currentQuantity As Integer = 0
                Integer.TryParse(existingItemElement.Attribute("inStorage")?.Value, currentQuantity) ' Get current amount
                existingItemElement.SetAttributeValue("inStorage", (currentQuantity + quantity).ToString()) ' Add new quantity
                ' Ensure other tracking attributes are reset (if they exist)
                If existingItemElement.Attribute("onTheWayIn") IsNot Nothing Then existingItemElement.SetAttributeValue("onTheWayIn", "0")
                If existingItemElement.Attribute("onTheWayOut") IsNot Nothing Then existingItemElement.SetAttributeValue("onTheWayOut", "0")
            Else
                ' Item doesn't exist, add a new <s> element
                Dim newItemElement As New XElement("s",
            New XAttribute("elementaryId", itemId.ToString()),
            New XAttribute("inStorage", quantity.ToString()),
            New XAttribute("onTheWayIn", "0"), ' Default values likely needed
            New XAttribute("onTheWayOut", "0")
        )
                targetInv.Add(newItemElement) ' Add the new element to the <inv> node
            End If

            ' --- Refresh the UI ---
            ' 1. Update the in-memory list for the container object
            Dim itemInMemory = selectedContainer.Items.FirstOrDefault(Function(i) i.ElementId = itemId)
            If itemInMemory IsNot Nothing Then
                itemInMemory.Quantity += quantity ' Update existing item in list
            Else
                selectedContainer.Items.Add(New StorageItem(itemId, quantity)) ' Add new item to list
            End If

            ' 2. Refresh the DataGrid by resetting its ItemsSource (simple refresh method)
            dgvStorage.ItemsSource = Nothing ' Clear binding
            dgvStorage.ItemsSource = selectedContainer.Items.OrderBy(Function(i) i.Name).ToList() ' Rebind sorted list

            ' Mark as unsaved
            MarkUnsavedChanges()
            MessageBox.Show($"{quantity}x {itemName} added/updated in {selectedContainer.DisplayName} (in memory). Use File > Save to make permanent.", "Item Added/Updated", MessageBoxButton.OK, MessageBoxImage.Information)
        End Sub

        ' *** btnDeleteItem_Click - Uses FeatElement Reference ***
        Private Sub btnDeleteItem_Click(sender As Object, e As RoutedEventArgs)
            If cmbStorageContainers.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a storage container first.", "No Container Selected", MessageBoxButton.OK, MessageBoxImage.Warning) : Return
            End If
            If dgvStorage.SelectedItem Is Nothing Then
                MessageBox.Show("Please select an item in the grid to delete.", "No Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning) : Return
            End If
            If xmlDoc Is Nothing Then
                MessageBox.Show("Save file not loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error) : Return
            End If

            Dim selectedContainer = TryCast(cmbStorageContainers.SelectedItem, StorageContainer)
            Dim selectedStorageItem = TryCast(dgvStorage.SelectedItem, StorageItem)
            Dim itemIdToDelete = selectedStorageItem.ElementId

            ' Get the target <feat> element directly
            Dim targetFeatElement = selectedContainer.FeatElement
            If targetFeatElement Is Nothing Then Return ' Should not happen

            Dim targetInv = targetFeatElement.Descendants("inv").FirstOrDefault()
            If targetInv Is Nothing Then Return ' Should not happen if items were loaded

            ' Find the specific <s> element for the item within this <inv>
            Dim itemElementToDelete = targetInv.Elements("s").FirstOrDefault(Function(s) s.Attribute("elementaryId")?.Value = itemIdToDelete.ToString())
            If itemElementToDelete Is Nothing Then
                MessageBox.Show($"Item {itemIdToDelete} not found in XML for {selectedContainer.DisplayName}.", "XML Error", MessageBoxButton.OK, MessageBoxImage.Error) : Return
            End If

            Dim confirmResult = MessageBox.Show($"Are you sure you want to delete ALL {selectedStorageItem.Quantity} x {selectedStorageItem.Name} from {selectedContainer.DisplayName}?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question)
            If confirmResult = MessageBoxResult.Yes Then
                itemElementToDelete.Remove() ' Remove from XML
                selectedContainer.Items.Remove(selectedStorageItem) ' Remove from memory
                dgvStorage.ItemsSource = Nothing : dgvStorage.ItemsSource = selectedContainer.Items.OrderBy(Function(item) item.Name).ToList() ' Refresh grid
                ' Mark as unsaved
                MarkUnsavedChanges()
                MessageBox.Show($"{selectedStorageItem.Name} removed from {selectedContainer.DisplayName} (in memory). Use File > Save to make permanent.", "Item Deleted", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        ' Helper Method to Clear Character Details DataGrids
        Private Sub ClearDataGrids()
            dgvAttributes.ItemsSource = Nothing
            dgvSkills.ItemsSource = Nothing
            dgvTraits.ItemsSource = Nothing
            lstConditions.ItemsSource = Nothing
            dgvRelationships.ItemsSource = Nothing ' Clear Relationships Grid
            txtRelationshipPageInfo.Text = "Page 0 of 0" ' Clear Paging Label
            btnRelPrev.IsEnabled = False
            btnRelNext.IsEnabled = False
            cmb_addTrait.ItemsSource = Nothing


        End Sub

        Private Sub UpdateXmlWithShipSize(shipToUpdate As Ship)
            ' Ensure xmlDoc and the ship object are valid
            If xmlDoc Is Nothing OrElse xmlDoc.Root Is Nothing OrElse shipToUpdate Is Nothing Then
                MessageBox.Show("Cannot update XML: XML document or ship data is missing.", "Internal Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            Try
                ' Find the specific <ship> element using its 'sid' attribute
                Dim shipElement = xmlDoc.Root.Descendants("ship").FirstOrDefault(
                                Function(x) x.Attribute("sid")?.Value = shipToUpdate.Sid.ToString()
                              )

                If shipElement IsNot Nothing Then
                    ' Found the ship element, update its 'sx' and 'sy' attributes
                    shipElement.SetAttributeValue("sx", shipToUpdate.Sx.ToString())
                    shipElement.SetAttributeValue("sy", shipToUpdate.Sy.ToString())

                    ' Mark as unsaved
                    MarkUnsavedChanges()

                Else
                    ' Ship element with the matching SID was not found in the XML
                    MessageBox.Show($"Could not find ship with SID {shipToUpdate.Sid} in the loaded XML file to update its size.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End If
            Catch ex As Exception
                ' Handle any errors during XML searching or attribute setting
                MessageBox.Show($"Error updating ship size in XML: {ex.Message}", "XML Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub


        ' Update ship size button click
        Private Sub btn_updateSize_Click(sender As Object, e As RoutedEventArgs)
            If cmb_ships.SelectedItem Is Nothing OrElse TryCast(cmb_ships.SelectedItem, Ship)?.Sid = -1 Then
                MessageBox.Show("Please select a valid ship first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            Dim selectedShip = CType(cmb_ships.SelectedItem, Ship)

            ' Pass current Sx and Sy to the constructor
            Dim updateWindow As New UpdateShipSizeWindow(selectedShip.Sx, selectedShip.Sy)
            updateWindow.Owner = Me ' Set owner

            ' Show the dialog and check the result
            If updateWindow.ShowDialog() = True Then
                ' If user clicked "Update" and validation passed...

                ' Get the CALCULATED Sx/Sy values back from the dialog's properties
                Dim newSx = updateWindow.ShipWidth
                Dim newSy = updateWindow.ShipHeight

                ' Update the Ship object in our internal list (optional but good practice)
                selectedShip.Sx = newSx
                selectedShip.Sy = newSy

                ' Update labels in main window UI
                lbl_shipSize.Text = $"Size: {selectedShip.Sx}x{selectedShip.Sy}"
                Dim canvasWidth As Integer = selectedShip.Sx \ 28 ' Use integer division
                Dim canvasHeight As Integer = selectedShip.Sy \ 28 ' Use integer division
                lbl_CanvasSize.Text = $"Canvas Size: {canvasWidth} W x {canvasHeight} H squares"

                ' *** CRUCIAL: Update the XML document in memory ***
                UpdateXmlWithShipSize(selectedShip)
            End If
        End Sub

        '' Update XML with Ship Sizes (Uses Descendants for robustness)
        'Private Sub UpdateXmlWithShips()
        '    If xmlDoc Is Nothing Then Return
        '    Dim shipsList = xmlDoc.Descendants("ship")
        '    If Not shipsList.Any Then Return ' No ships found
        '    For Each shipInMemory In ships
        '        Dim shipElement = shipsList.FirstOrDefault(Function(x) x.Attribute("sid")?.Value = shipInMemory.Sid.ToString())
        '        If shipElement IsNot Nothing Then
        '            shipElement.SetAttributeValue("sx", shipInMemory.Sx.ToString())
        '            shipElement.SetAttributeValue("sy", shipInMemory.Sy.ToString())
        '        End If
        '    Next
        '    Try
        '        xmlDoc.Save(currentFilePath)
        '        MessageBox.Show("Ship size updated in the XML.", "Success - Saved", MessageBoxButton.OK, MessageBoxImage.Information)
        '    Catch ex As Exception
        '        MessageBox.Show($"Error saving ship size update: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error)
        '    End Try
        'End Sub

        ' Exit menu click
        Private Sub ExitMenu_Click(sender As Object, e As RoutedEventArgs)
            Application.Current.Shutdown()
        End Sub

        ' Game Start Editor menu click - Switch to the tab
        Private Sub GameStartEditorMenu_Click(sender As Object, e As RoutedEventArgs)
            ' Find the TabControl and switch to Game Start Editor tab
            Dim tabControl = FindVisualChild(Of TabControl)(Me)
            If tabControl IsNot Nothing Then
                For Each tabItem As TabItem In tabControl.Items
                    If tabItem.Header.ToString() = "Game Start Editor" Then
                        tabControl.SelectedItem = tabItem
                        Exit For
                    End If
                Next
            End If
        End Sub

        ' Helper to find visual child (for finding TabControl)
        Private Function FindVisualChild(Of T As DependencyObject)(parent As DependencyObject) As T
            For i As Integer = 0 To VisualTreeHelper.GetChildrenCount(parent) - 1
                Dim child As DependencyObject = VisualTreeHelper.GetChild(parent, i)
                If TypeOf child Is T Then
                    Return DirectCast(child, T)
                End If
                Dim childOfChild As T = FindVisualChild(Of T)(child)
                If childOfChild IsNot Nothing Then
                    Return childOfChild
                End If
            Next
            Return Nothing
        End Function

        ' --- Helper functions for IDs ---
        Private Function GetAttributeNameById(id As Integer) As String
            Return If(IdCollection.DefaultAttributeIDs.ContainsKey(id), IdCollection.DefaultAttributeIDs(id), $"Unknown ({id})")
        End Function
        Private Function GetSkillNameById(id As Integer) As String
            Return If(IdCollection.DefaultSkillIDs.ContainsKey(id), IdCollection.DefaultSkillIDs(id), $"Unknown ({id})")
        End Function
        Private Function GetTraitNameById(id As Integer) As String
            Return If(IdCollection.DefaultTraitIDs.ContainsKey(id), IdCollection.DefaultTraitIDs(id), $"Unknown ({id})")
        End Function

        ' --- Populate Dropdowns ---

        Private Sub PopulateAddTraitComboBox(selectedCharacter As Character)
            If selectedCharacter Is Nothing Then
                cmb_addTrait.ItemsSource = Nothing
                Return
            End If

            ' Get all traits, sorted by name
            Dim allTraits = IdCollection.DefaultTraitIDs.OrderBy(Function(kvp) kvp.Value).ToList()

            If allTraits Is Nothing OrElse allTraits.Count = 0 Then
                Debug.WriteLine("No traits available in IdCollection.DefaultTraitIDs.")
                cmb_addTrait.ItemsSource = Nothing
                Return
            End If

            ' Get the traits the character already has
            Dim currentTraitIds As New HashSet(Of Integer)(selectedCharacter.CharacterTraits.Select(Function(t) CInt(t.Id)))

            ' Filter out traits the character already has
            Dim availableTraits = allTraits.Where(Function(t) Not currentTraitIds.Contains(t.Key)).ToList()

            ' Debugging
            Debug.WriteLine($"All Traits Count: {allTraits.Count}")
            Debug.WriteLine($"Current Traits Count: {currentTraitIds.Count}")
            Debug.WriteLine($"Available Traits Count: {availableTraits.Count}")

            ' Populate the dropdown
            cmb_addTrait.ItemsSource = availableTraits
            If availableTraits.Any() Then
                cmb_addTrait.SelectedIndex = 0
            Else
                cmb_addTrait.SelectedIndex = -1
            End If
        End Sub


        'Private Sub PopulateTraitDropdown()
        '    Dim sortedTraits = IdCollection.DefaultTraitIDs.OrderBy(Function(kvp) kvp.Value).ToList()
        '    cmb_addTrait.ItemsSource = sortedTraits
        '    'cmb_addTrait.ItemsSource = IdCollection.DefaultTraitIDs.ToList()
        '    cmb_addTrait.DisplayMemberPath = "Value"
        '    cmb_addTrait.SelectedValuePath = "Key"
        '    If cmb_addTrait.Items.Count > 0 Then cmb_addTrait.SelectedIndex = 0
        'End Sub
        'Private Sub PopulateStorageItemDropdown()
        'cmbAddItem.ItemsSource = IdCollection.DefaultStorageIDs.ToList()
        'cmbAddItem.DisplayMemberPath = "Value"
        'cmbAddItem.SelectedValuePath = "Key"
        'mbAddItem.Items.Count > 0 Then cmbAddItem.SelectedIndex = 0
        'End Sub

        ' --- Trait Add/Delete (Full versions) ---
        Private Sub btn_addTrait_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedCharacter As Character = TryCast(lstCharacters.SelectedItem, Character)
            If selectedCharacter Is Nothing Then
                MessageBox.Show("Please select a character first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            If cmb_addTrait.SelectedValue Is Nothing Then
                MessageBox.Show("Please select a trait to add.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            Dim selectedTraitId = CInt(cmb_addTrait.SelectedValue)
            Dim selectedTraitName = CType(cmb_addTrait.SelectedItem, KeyValuePair(Of Integer, String)).Value
            If Not selectedCharacter.CharacterTraits.Any(Function(t) t.Id = selectedTraitId) Then
                Dim newTrait As New DataProp With {.Id = selectedTraitId, .Name = selectedTraitName}
                selectedCharacter.CharacterTraits.Add(newTrait)
                dgvTraits.ItemsSource = Nothing
                dgvTraits.ItemsSource = New ObservableCollection(Of DataProp)(selectedCharacter.CharacterTraits)
                ' Mark as unsaved - XML will be updated in UpdateXmlWithCharacterChanges() when Save is called
                MarkUnsavedChanges()
                MessageBox.Show($"Trait '{selectedTraitName}' added to character (in memory). Use File > Save to make permanent.", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
            Else
                MessageBox.Show("Trait already exists for this character.", "Info", MessageBoxButton.OK, MessageBoxImage.Information)
            End If
        End Sub

        Private Sub btn_deleteTrait_Click(sender As Object, e As RoutedEventArgs)
            Dim selectedCharacter As Character = TryCast(lstCharacters.SelectedItem, Character)
            If selectedCharacter Is Nothing Then
                Debug.WriteLine("No character selected to delete trait.")
                Return
            End If

            Dim selectedTrait As DataProp = TryCast(dgvTraits.SelectedItem, DataProp)
            If selectedTrait Is Nothing Then
                Debug.WriteLine("No trait selected to delete.")
                Return
            End If

            ' Remove the trait from the character's traits
            selectedCharacter.CharacterTraits.Remove(selectedTrait)

            ' Update the XML
            Dim characterNode As XElement = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = selectedCharacter.CharacterEntityId.ToString())
            If characterNode IsNot Nothing Then
                Dim traitNode As XElement = characterNode.Element("pers")?.Element("traits")?.Elements("t").FirstOrDefault(Function(t) t.Attribute("id")?.Value = selectedTrait.Id.ToString())
                If traitNode IsNot Nothing Then
                    traitNode.Remove()
                End If
            End If

            ' Mark as unsaved
            MarkUnsavedChanges()

            ' Refresh the UI
            dgvTraits.ItemsSource = Nothing
            dgvTraits.ItemsSource = New ObservableCollection(Of DataProp)(selectedCharacter.CharacterTraits)
            PopulateAddTraitComboBox(selectedCharacter)
        End Sub

        ' --- Clear UI Helpers ---
        Private Sub ClearUI()
            lbl_owner.Text = "Owner: "
            lbl_shipSize.Text = "Size: "
            lbl_CanvasSize.Text = "Canvas Size: "
            lstCharacters.ItemsSource = Nothing
            ClearDataGrids()
            txtCrewCount.Text = "Total Crew: N/A"
        End Sub

        ' --- Set All Buttons (Full versions, save immediately) ---





        ' --- Storage Helpers ---
        Private Sub ClearStorageDisplay()
            cmbStorageContainers.ItemsSource = Nothing
            dgvStorage.ItemsSource = Nothing
            currentShipStorageContainers.Clear() ' Clear internal list too
            CurrentContainerItems = Nothing
            txtContainerInfo.Text = "(Select Container)"
        End Sub
        Private Sub ClearStorageGrid() ' Renamed from previous ambiguous name
            dgvStorage.ItemsSource = Nothing
        End Sub

        '--- TODO: Implement UpdateXmlWithCharacterChanges if you want grid edits saved via the Save menu ---

        Private Sub PrepareGroupedStorageItems()
            Dim categorizedItems As New List(Of CategorizedItem)()
            For Each kvp In IdCollection.DefaultStorageIDs.OrderBy(Function(x) x.Value)
                Dim category As String = GetCategoryForItem(kvp.Key, kvp.Value)
                categorizedItems.Add(New CategorizedItem(category, kvp.Value, kvp.Key))
            Next
            Dim sortedItems = categorizedItems.OrderBy(Function(item) item.Category).ThenBy(Function(item) item.ItemName).ToList()

            ' Create CollectionViewSource for grouping
            Dim cvs As New CollectionViewSource()
            cvs.Source = sortedItems
            If cvs.GroupDescriptions.Count = 0 Then
                cvs.GroupDescriptions.Add(New PropertyGroupDescription("Category"))
            End If

            ' --- SET ItemsSource Directly ---
            cmbAddItem.ItemsSource = cvs.View
            ' --- End Change ---

            ' Removed: GroupedStorageItemsView = cvs.View
            ' Removed: FlatStorageItems = sortedItems
            ' Removed: Debug MessageBox
        End Sub

        Private Function GetCategoryForItem(itemId As Integer, itemName As String) As String
            ' Prioritize specific IDs if needed
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

            ' Categorize by keywords in name (adjust keywords as needed)
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
               lowerName.Contains("energ") OrElse lowerName.Contains("hyperium") Then ' Energium, Energy Rod, Hyperfuel etc.
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

        'Private Sub dgvStorage_RowEditEnding(sender As Object, e As DataGridRowEditEndingEventArgs) Handles dgvStorage.RowEditEnding

        '    If e.EditAction <> DataGridEditAction.Commit Then Exit Sub

        '    ' Delay processing until after the row edit is fully committed to avoid reentrancy issues.
        '    Dispatcher.BeginInvoke(New Action(Sub()
        '                                          Dim editedItem As StorageItem = TryCast(e.Row.Item, StorageItem)
        '                                          If editedItem Is Nothing Then Exit Sub

        '                                          Dim newQuantity As Integer = editedItem.Quantity

        '                                          ' Validate the new quantity.
        '                                          If newQuantity < 0 Then
        '                                              MessageBox.Show("Invalid quantity. Please enter 0 or a positive number.", "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning)
        '                                              Exit Sub
        '                                          End If

        '                                          ' Ensure a storage container is selected.
        '                                          Dim selectedContainer As StorageContainer = TryCast(cmbStorageContainers.SelectedItem, StorageContainer)
        '                                          If selectedContainer Is Nothing Then
        '                                              MessageBox.Show("No storage container selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        '                                              Exit Sub
        '                                          End If

        '                                          ' Ensure the XML document is loaded.
        '                                          If xmlDoc Is Nothing Then
        '                                              MessageBox.Show("No file loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        '                                              Exit Sub
        '                                          End If

        '                                          ' Retrieve the corresponding XML <feat> element for the container.
        '                                          Dim targetFeatElement As XElement = selectedContainer.FeatElement
        '                                          If targetFeatElement Is Nothing Then
        '                                              MessageBox.Show("Selected container is missing its XML reference.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        '                                              Exit Sub
        '                                          End If

        '                                          ' Locate the <inv> node within the container.
        '                                          Dim targetInv As XElement = targetFeatElement.Descendants("inv").FirstOrDefault()
        '                                          If targetInv Is Nothing Then
        '                                              MessageBox.Show("Inventory node (<inv>) not found in the selected container.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
        '                                              Exit Sub
        '                                          End If

        '                                          ' Find the <s> element that matches the StorageItem's ElementId.
        '                                          Dim itemElement As XElement = targetInv.Elements("s").FirstOrDefault(Function(s) s.Attribute("elementaryId")?.Value = editedItem.ElementId.ToString())

        '                                          If newQuantity > 0 Then
        '                                              ' If the item does not exist in the XML, create it.
        '                                              If itemElement Is Nothing Then
        '                                                  itemElement = New XElement("s",
        '                                                                              New XAttribute("elementaryId", editedItem.ElementId.ToString()),
        '                                                                              New XAttribute("inStorage", newQuantity.ToString()),
        '                                                                              New XAttribute("onTheWayIn", "0"),
        '                                                                              New XAttribute("onTheWayOut", "0"))
        '                                                  targetInv.Add(itemElement)
        '                                              Else
        '                                                  ' Otherwise, update the existing XML element's quantity.
        '                                                  itemElement.SetAttributeValue("inStorage", newQuantity.ToString())
        '                                              End If
        '                                          Else
        '                                              ' If newQuantity is 0, remove the <s> element if it exists.
        '                                              If itemElement IsNot Nothing Then
        '                                                  itemElement.Remove()
        '                                              End If
        '                                          End If

        '                                      End Sub), System.Windows.Threading.DispatcherPriority.Background)


        'End Sub



        Private Sub dgvStorage_RowEditEnding(sender As Object, e As DataGridRowEditEndingEventArgs) Handles dgvStorage.RowEditEnding
            If e.EditAction <> DataGridEditAction.Commit Then Exit Sub

            Dispatcher.BeginInvoke(New Action(Sub()
                                                  Dim editedItem As StorageItem = TryCast(e.Row.Item, StorageItem)
                                                  If editedItem Is Nothing Then Exit Sub

                                                  Dim newQuantity As Integer = editedItem.Quantity ' Assumes binding updated this

                                                  If newQuantity < 0 Then
                                                      MessageBox.Show("Invalid quantity. Must be 0 or positive.", "Invalid Quantity", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                      ' TODO: Revert UI/Data change if possible
                                                      Exit Sub
                                                  End If

                                                  Dim selectedContainer As StorageContainer = TryCast(cmbStorageContainers.SelectedItem, StorageContainer)
                                                  If selectedContainer Is Nothing OrElse xmlDoc Is Nothing OrElse selectedContainer.FeatElement Is Nothing Then Exit Sub ' Add null checks

                                                  Dim targetFeatElement As XElement = selectedContainer.FeatElement
                                                  Dim targetInv As XElement = targetFeatElement.Descendants("inv").FirstOrDefault()
                                                  If targetInv Is Nothing Then Exit Sub ' Should exist if loaded

                                                  Dim itemElement As XElement = targetInv.Elements("s").FirstOrDefault(Function(s) s.Attribute("elementaryId")?.Value = editedItem.ElementId.ToString())

                                                  Try
                                                      If newQuantity > 0 Then
                                                          If itemElement Is Nothing Then
                                                              itemElement = New XElement("s", New XAttribute("elementaryId", editedItem.ElementId.ToString()), New XAttribute("inStorage", newQuantity.ToString()), New XAttribute("onTheWayIn", "0"), New XAttribute("onTheWayOut", "0"))
                                                              targetInv.Add(itemElement)
                                                          Else
                                                              itemElement.SetAttributeValue("inStorage", newQuantity.ToString())
                                                          End If
                                                      Else ' newQuantity is 0
                                                          If itemElement IsNot Nothing Then itemElement.Remove()
                                                      End If

                                                      ' Update the backing list (selectedContainer.Items) as well
                                                      Dim itemInBackingList = selectedContainer.Items.FirstOrDefault(Function(i) i.ElementId = editedItem.ElementId)
                                                      If newQuantity > 0 Then
                                                          If itemInBackingList Is Nothing Then selectedContainer.Items.Add(editedItem) Else itemInBackingList.Quantity = newQuantity
                                                      Else
                                                          If itemInBackingList IsNot Nothing Then selectedContainer.Items.Remove(itemInBackingList)
                                                          ' CurrentContainerItems updates visually when item removed via grid if bound correctly
                                                      End If

                                                      ' Mark as unsaved
                                                      MarkUnsavedChanges()

                                                      ' --- ADDED: Update total count label ---
                                                      UpdateContainerInfoText(CurrentContainerItems)
                                                      ' --- End Addition ---

                                                      ' Optional: Keep a success message?
                                                      ' MessageBox.Show($"Quantity for {editedItem.Name} updated to {newQuantity} (in memory). Save to persist.", "Qty Updated", MessageBoxButton.OK, MessageBoxImage.Information)

                                                  Catch ex As Exception
                                                      MessageBox.Show($"Error updating XML for item {editedItem.ElementId}: {ex.Message}", "XML Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                                      ' TODO: Revert UI changes if XML update fails?
                                                  End Try

                                              End Sub), DispatcherPriority.Background) ' End Dispatcher.BeginInvoke
        End Sub

        ' Handler for direct edits in Attributes DataGrid
        Private Sub dgvAttributes_RowEditEnding(sender As Object, e As DataGridRowEditEndingEventArgs) Handles dgvAttributes.RowEditEnding
            If e.EditAction <> DataGridEditAction.Commit Then Exit Sub

            Dispatcher.BeginInvoke(New Action(Sub()
                                                  Dim editedAttr As DataProp = TryCast(e.Row.Item, DataProp)
                                                  If editedAttr Is Nothing Then Exit Sub

                                                  Dim selectedChar = TryCast(lstCharacters.SelectedItem, Character)
                                                  If selectedChar Is Nothing OrElse xmlDoc Is Nothing Then Exit Sub

                                                  ' Update XML immediately
                                                  Dim charNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = selectedChar.CharacterEntityId.ToString())
                                                  If charNode IsNot Nothing Then
                                                      Dim attrsElement = charNode.Element("pers")?.Element("attr")
                                                      If attrsElement IsNot Nothing Then
                                                          Dim attrNode = attrsElement.Elements("a").FirstOrDefault(Function(a) a.Attribute("id")?.Value = editedAttr.Id.ToString())
                                                          If attrNode IsNot Nothing Then
                                                              attrNode.SetAttributeValue("points", editedAttr.Value.ToString())
                                                          End If
                                                      End If
                                                  End If

                                                  ' Mark as unsaved
                                                  MarkUnsavedChanges()

                                              End Sub), DispatcherPriority.Background)
        End Sub

        ' Handler for direct edits in Skills DataGrid
        Private Sub dgvSkills_RowEditEnding(sender As Object, e As DataGridRowEditEndingEventArgs) Handles dgvSkills.RowEditEnding
            If e.EditAction <> DataGridEditAction.Commit Then Exit Sub

            Dispatcher.BeginInvoke(New Action(Sub()
                                                  Dim editedSkill As DataProp = TryCast(e.Row.Item, DataProp)
                                                  If editedSkill Is Nothing Then Exit Sub

                                                  Dim selectedChar = TryCast(lstCharacters.SelectedItem, Character)
                                                  If selectedChar Is Nothing OrElse xmlDoc Is Nothing Then Exit Sub

                                                  ' Update XML immediately
                                                  Dim charNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = selectedChar.CharacterEntityId.ToString())
                                                  If charNode IsNot Nothing Then
                                                      Dim skillsElement = charNode.Element("pers")?.Element("skills")
                                                      If skillsElement IsNot Nothing Then
                                                          Dim skillNode = skillsElement.Elements("s").FirstOrDefault(Function(s) s.Attribute("sk")?.Value = editedSkill.Id.ToString())
                                                          If skillNode IsNot Nothing Then
                                                              skillNode.SetAttributeValue("level", editedSkill.Value.ToString())
                                                              skillNode.SetAttributeValue("mxn", editedSkill.MaxValue.ToString())
                                                          End If
                                                      End If
                                                  End If

                                                  ' Mark as unsaved
                                                  MarkUnsavedChanges()

                                              End Sub), DispatcherPriority.Background)
        End Sub





        Private Sub UpdateContainerInfoText(itemsToSum As IEnumerable(Of StorageItem))
            If itemsToSum IsNot Nothing Then
                Dim currentTotalQuantity As Integer = itemsToSum.Sum(Function(item) item.Quantity)
                txtContainerInfo.Text = $"Total Items: {currentTotalQuantity}"
            Else
                txtContainerInfo.Text = "(No Items)" ' Or "(Select Container)"
            End If
        End Sub

        ' --- ADD THIS NEW SUBROUTINE ---
        Private Sub HelpMenu_Click(sender As Object, e As RoutedEventArgs)
            Dim helpText As String = GenerateHelpText() ' Get the help text
            Dim helpWin As New HelpWindow(helpText)   ' Create the window, passing text
            helpWin.Owner = Me ' Make it owned by the main window
            helpWin.ShowDialog() ' Show it as a modal dialog
        End Sub

        ' --- ADD THIS NEW FUNCTION TO GENERATE THE HELP TEXT ---
        Private Function GenerateHelpText() As String
            Dim sb As New System.Text.StringBuilder()

            sb.AppendLine("Moragar's Space Haven Save Editor - Help & Instructions")
            sb.AppendLine("======================================================")
            sb.AppendLine() ' Blank line for spacing

            sb.AppendLine("*** DISCLAIMER ***")
            sb.AppendLine("Use this tool at your own risk. Editing save files can lead to unexpected")
            sb.AppendLine("issues or corrupted saves. The creator is not responsible for any damage")
            sb.AppendLine("to your save games, even if the backup feature is enabled.")
            sb.AppendLine("Always keep manual backups of important saves!")
            sb.AppendLine("******************")
            sb.AppendLine() ' Blank line for spacing


            sb.AppendLine("--- Getting Started ---")
            sb.AppendLine("- Three Editors in One: The application includes three separate editors accessible via tabs:")
            sb.AppendLine("  - Save Game Editor: Edit existing save games (default tab)")
            sb.AppendLine("  - Game Start Editor: Set difficulty settings for new game creation")
            sb.AppendLine("  - Blueprint Manager: Clone and transfer ship blueprints between save games")
            sb.AppendLine("- File -> Open: Use this to load your Space Haven save game.")
            sb.AppendLine("  - Navigate to your save game folder. The typical path is:")
            sb.AppendLine("    Steam\steamapps\common\SpaceHaven\savegames\[YourSaveGameName]\save\")
            sb.AppendLine("  - Select the file named 'game' (it usually has no file extension).")
            sb.AppendLine("  - NOTE: If you set a default save location in Settings, the file dialog will")
            sb.AppendLine("    automatically open to that directory each time you use File -> Open.")
            sb.AppendLine("- Backups: If enabled in Settings, a timestamped backup of your save folder")
            sb.AppendLine("    (e.g., '[YourSaveGameName]_backup_YYYYMMDDHHMMSS') will be created")
            sb.AppendLine("    automatically in the 'savegames' directory *each time* you open a file.")
            sb.AppendLine("    This is highly recommended to prevent data loss!")
            sb.AppendLine("- File -> Save: IMPORTANT! Click this after making edits to permanently write")
            sb.AppendLine("    your changes back to the 'game' file.")
            sb.AppendLine("- File -> Exit: Closes the editor. Any unsaved changes will be lost.")
            sb.AppendLine("- Edit -> Settings: Opens the Settings window where you can:")
            sb.AppendLine("    - Toggle automatic backups")
            sb.AppendLine("    - Set a default 'open' save location directory")
            sb.AppendLine("- Help -> Help / Instructions: Shows this information again.")
            sb.AppendLine("- Help -> About: Shows application version and credits.")
            sb.AppendLine()

            sb.AppendLine("--- Editing Your Save ---")
            sb.AppendLine("- Global Settings (Top Section):")
            sb.AppendLine("  - Player Credits: Enter the desired amount of credits.")
            sb.AppendLine("  - Sandbox Mode: Check or uncheck to enable/disable sandbox mode.")
            sb.AppendLine("  - Player Prestige Points: Enter the desired amount of Exodus Fleet Prestige Points")
            sb.AppendLine("  - NOTE: Changes here are applied to memory only when you click File -> Save.")
            sb.AppendLine("- Ship Selection (Middle Section):")
            sb.AppendLine("  - Use the dropdown to select the specific ship you want to view or edit.")
            sb.AppendLine("  - Basic info like Owner and Size is shown below the dropdown.")
            sb.AppendLine("  - Update Size Button: Allows changing the selected ship's dimensions (Sx, Sy values).")
            sb.AppendLine("      - The 'Canvas Size' shows the size in buildable grid squares (Sx/28 x Sy/28).")
            sb.AppendLine("      - Max recommended Canvas Size is 8W x 8H squares (Sx=224, Sy=224).")
            sb.AppendLine() ' Blank line for spacing
            sb.AppendLine("      - **WARNING:** Significantly increasing ship size might cause graphical glitches")
            sb.AppendLine("        or conflicts with other ships/stations in-game. Use with caution!")
            sb.AppendLine("      - Ship size changes save immediately to memory and require File -> Save.")
            sb.AppendLine("- Main Tabs (Crew / Storage):")
            sb.AppendLine("  - Select the 'Crew' or 'Storage' tab to edit details for the currently selected ship.")
            sb.AppendLine()

            sb.AppendLine("--- Crew Tab Details ---")
            sb.AppendLine("- Crew List (Left): Select a crew member. The list shows names; total count is above.")
            sb.AppendLine("- Create New Crew Member: Button below the list opens a window to add a new character.")
            sb.AppendLine("    You can customize their name, stats, and traits before creation.")
            sb.AppendLine("    - You can now pick the crew member that is being cloned (template user).")
            sb.AppendLine("    - NOTE: Right now, cloning a current crew member and assigning a new ID for it is")
            sb.AppendLine("      the most reliable way to make this work.")
            sb.AppendLine("- Bulk Operations (Menu -> Bulk):")
            sb.AppendLine("  - Create X amount of crew from a template crew member:")
            sb.AppendLine("    * Select a crew member to use as the template")
            sb.AppendLine("    * All crew members created this way will be the same as the template crew,")
            sb.AppendLine("      except for name and attributes")
            sb.AppendLine("    * Names are pulled from an AI-generated list of Sci-Fi names")
            sb.AppendLine("  - Bulk Attributes: Set X amount of attributes for all crew on the selected ship")
            sb.AppendLine("    to the same amount")
            sb.AppendLine("  - Bulk Skills: Set X amount of skills for all crew on the selected ship")
            sb.AppendLine("    to the same amount")
            sb.AppendLine("- Editing Tabs (Right - Attributes, Skills, Traits, Conditions, Relationships):")
            sb.AppendLine("  - Attributes/Skills: Double-click a cell in the 'Value' or 'Level' column to edit.")
            sb.AppendLine("      Type the new number and press Enter or click another row to confirm the change.")
            sb.AppendLine("      Use the 'Set All...' buttons for quick presets (these affect ALL characters")
            sb.AppendLine("      and save immediately to memory - use File -> Save to make permanent).")
            sb.AppendLine("  - Traits: Select a trait from the dropdown, click 'Add Trait'. To remove, select a trait")
            sb.AppendLine("      in the grid above, then click 'Delete Trait'.")
            sb.AppendLine("  - Conditions: Shows current status effects/injuries. Select one and click")
            sb.AppendLine("      'Delete Selected Condition' to remove it.")
            sb.AppendLine("  - Relationships: Shows how the selected character feels about others. Edit the")
            sb.AppendLine("      Friendship, Attraction, or Compatibility values by double-clicking the cell,")
            sb.AppendLine("      typing a new number, and pressing Enter or changing rows.")
            sb.AppendLine("  - NOTE: Adding crew, editing grids (Attributes, Skills, Relationships), or changing")
            sb.AppendLine("      Traits/Conditions requires using File -> Save to make permanent.")
            sb.AppendLine()

            sb.AppendLine("--- Storage Tab Details ---")
            sb.AppendLine("- Container Selection: Choose a specific storage container (e.g., 'Storage (Type: storageMedium) - 1')")
            sb.AppendLine("    from the dropdown. The 'Total Items' count for that container is shown next to it.")
            sb.AppendLine("- Item Grid: Shows items in the selected container.")
            sb.AppendLine("- Edit Quantity: Double-click a cell in the 'Quantity' column, type the new amount,")
            sb.AppendLine("    and press Enter or change row to confirm. Setting quantity to 0 removes the stack on save.")
            sb.AppendLine("- Add Item: Below the grid, select an item category, then the specific item. Enter the")
            sb.AppendLine("    desired quantity and click 'Add to Container'.")
            sb.AppendLine("- Delete Item Stack: Select a row in the grid and click 'Delete Selected'.")
            sb.AppendLine("- NOTE: All storage changes (adding, deleting, editing quantity) require File -> Save.")
            sb.AppendLine()

            sb.AppendLine("--- Game Start Editor ---")
            sb.AppendLine("- Accessible via the tab at the top of the editor")
            sb.AppendLine("- Allows you to set difficulty settings parameters at the start of new game creation")
            sb.AppendLine("- Step-by-step instructions:")
            sb.AppendLine("  1. Go through the start game process in Space Haven")
            sb.AppendLine("  2. Click 'Save Custom Game Template' in the difficulty start settings")
            sb.AppendLine("  3. Open that XML file using File -> Open in the Game Start Editor tab")
            sb.AppendLine("  4. Change the parameters as desired")
            sb.AppendLine("  5. Save your changes using File -> Save")
            sb.AppendLine()

            sb.AppendLine("--- Blueprint Manager ---")
            sb.AppendLine("- Accessible via the tab at the top of the editor")
            sb.AppendLine("- Allows you to clone a ship blueprint to transfer between save games")
            sb.AppendLine("- Export Options:")
            sb.AppendLine("  - Export with items: Includes the ship blueprint along with all items on the ship")
            sb.AppendLine("  - Export blueprint only: Just the ship blueprint structure without items")
            sb.AppendLine("- Step-by-step instructions:")
            sb.AppendLine("  1. Open your current save game in the Save Game Editor tab")
            sb.AppendLine("  2. Switch to the Blueprint Manager tab")
            sb.AppendLine("  3. Select the ship you want to export from the dropdown")
            sb.AppendLine("  4. Choose whether to export with items or blueprint only")
            sb.AppendLine("  5. Click 'Export Blueprint' and save the file")
            sb.AppendLine("  6. Open your new save game (or a different save) in the Save Game Editor tab")
            sb.AppendLine("  7. Switch back to the Blueprint Manager tab")
            sb.AppendLine("  8. Click 'Import Blueprint' and select the exported file")
            sb.AppendLine("  - WARNING: Be careful on import - if you have 2 big ships, you may run into space")
            sb.AppendLine("    issues in the game world.")
            sb.AppendLine()

            sb.AppendLine("--- Final Reminder ---")
            sb.AppendLine("- Don't forget File -> Save! Most edits only change the data in memory until saved.")
            sb.AppendLine("- Keep backups of your original save files just in case!")

            Return sb.ToString()
        End Function



        ' --- (Ensure all your other existing methods are still present) ---

        'Private Sub dgvStorage_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs)
        '    MessageBox.Show("DEBUG: dgvStorage_CellEditEnding START", "Debug", MessageBoxButton.OK, MessageBoxImage.Information) ' Start of handler

        '    If e.EditAction = DataGridEditAction.Cancel Then
        '        MessageBox.Show("DEBUG: EditAction was Cancel. Exiting.", "Debug", MessageBoxButton.OK, MessageBoxImage.Warning)
        '        Return ' Edit was cancelled by user (e.g., Esc key)
        '    End If

        '    Dim quantityColumn As DataGridTextColumn = TryCast(e.Column, DataGridTextColumn)
        '    If quantityColumn Is Nothing OrElse quantityColumn.Header?.ToString() <> "Quantity" Then
        '        Dim headerInfo As String = If(e.Column Is Nothing, "Column is Nothing", $"Header is '{If(e.Column?.Header?.ToString(), "NULL")}'")
        '        MessageBox.Show($"DEBUG EXIT: Edited column test failed. {headerInfo}", "Debug Exit", MessageBoxButton.OK, MessageBoxImage.Error)
        '        Return ' Exit because it's not the quantity column or column is null
        '    End If

        '    Dim editedItem As StorageItem = TryCast(e.Row?.Item, StorageItem)
        '    If editedItem Is Nothing Then
        '        Dim itemTypeInfo As String = If(e.Row?.Item Is Nothing, "Row.Item is Nothing", $"Row.Item type is {e.Row.Item.GetType().Name}")
        '        MessageBox.Show($"DEBUG EXIT: Failed to cast Row.Item to StorageItem. {itemTypeInfo}", "Debug Exit", MessageBoxButton.OK, MessageBoxImage.Error)
        '        Return ' Exit because the data item for the row is null or not a StorageItem
        '    End If

        '    Dim editingTextBox As TextBox = TryCast(e.EditingElement, TextBox)
        '    If editingTextBox Is Nothing Then
        '        Dim elementInfo As String = If(e.EditingElement Is Nothing, "EditingElement is Nothing", $"EditingElement type is {e.EditingElement.GetType().Name}")
        '        MessageBox.Show($"DEBUG EXIT: Failed to cast EditingElement to TextBox. {elementInfo}", "Debug Exit", MessageBoxButton.OK, MessageBoxImage.Error)
        '        Return ' Exit because the control used for editing isn't a TextBox
        '    End If

        '    MessageBox.Show("DEBUG: Initial checks passed (Column, Row Item, Edit Control). Proceeding to validation.", "Debug Progress", MessageBoxButton.OK, MessageBoxImage.Information)

        '    Dim newValueString As String = editingTextBox.Text
        '    Dim newQuantity As Integer

        '    If Not Integer.TryParse(newValueString, newQuantity) OrElse newQuantity < 0 Then
        '        MessageBox.Show($"DEBUG EXIT: Invalid quantity input ('{newValueString}'). Cancelling edit.", "Debug Validation", MessageBoxButton.OK, MessageBoxImage.Warning)
        '        e.Cancel = True
        '        editingTextBox.Text = editedItem.Quantity.ToString() ' Attempt to restore visually
        '        Return
        '    End If

        '    MessageBox.Show($"DEBUG: Input '{newValueString}' parsed as valid quantity {newQuantity}.", "Debug Validation", MessageBoxButton.OK, MessageBoxImage.Information)

        '    ' --- Start XML Update Logic with Try...Catch ---
        '    Dim xmlWasModified As Boolean = False
        '    Dim exceptionMessage As String = Nothing ' To store potential error message
        '    Dim targetFeatElement As XElement = Nothing ' Declare here to use in Tuple later

        '    Try
        '        If xmlDoc Is Nothing Then Throw New Exception("xmlDoc is Nothing")
        '        Dim selectedContainer = TryCast(cmbStorageContainers.SelectedItem, StorageContainer)
        '        If selectedContainer Is Nothing Then Throw New Exception("selectedContainer is Nothing")
        '        If selectedContainer.FeatElement Is Nothing Then Throw New Exception("selectedContainer.FeatElement is Nothing")

        '        targetFeatElement = selectedContainer.FeatElement ' Assign within Try block
        '        MessageBox.Show($"DEBUG: Trying to find <inv> within <feat> (Parent objId='{selectedContainer.ParentObjId}', Parent entId='{If(selectedContainer.ParentEntId.HasValue, selectedContainer.ParentEntId.Value.ToString(), "N/A")}')", "Debug Progress", MessageBoxButton.OK, MessageBoxImage.Information)
        '        Dim targetInv = targetFeatElement.Descendants("inv").FirstOrDefault()

        '        If targetInv Is Nothing Then Throw New Exception("Cannot find <inv> node within the selected container's <feat> element.")

        '        MessageBox.Show($"DEBUG: Found <inv> node. Trying to find <s> element with elementaryId='{editedItem.ElementId}'.", "Debug Progress", MessageBoxButton.OK, MessageBoxImage.Information)
        '        Dim itemElementToUpdate = targetInv.Elements("s").FirstOrDefault(Function(s) s.Attribute("elementaryId")?.Value = editedItem.ElementId.ToString())

        '        If itemElementToUpdate Is Nothing Then MessageBox.Show($"DEBUG: <s> element for Item ID {editedItem.ElementId} was NOT FOUND within the <inv> node.", "Debug XML Info", MessageBoxButton.OK, MessageBoxImage.Warning) Else MessageBox.Show($"DEBUG: Found existing <s> element for Item ID {editedItem.ElementId}.", "Debug XML Info", MessageBoxButton.OK, MessageBoxImage.Information)

        '        ' --- Perform XML Update ---
        '        If itemElementToUpdate Is Nothing AndAlso newQuantity > 0 Then
        '            MessageBox.Show($"DEBUG INFO: Item ID {editedItem.ElementId} not in XML, creating new <s>.", "Debug XML Action", MessageBoxButton.OK, MessageBoxImage.Information)
        '            itemElementToUpdate = New XElement("s", New XAttribute("elementaryId", editedItem.ElementId.ToString()), New XAttribute("inStorage", newQuantity.ToString()), New XAttribute("onTheWayIn", "0"), New XAttribute("onTheWayOut", "0"))
        '            targetInv.Add(itemElementToUpdate) : xmlWasModified = True
        '        ElseIf itemElementToUpdate IsNot Nothing Then
        '            If newQuantity > 0 Then
        '                Dim currentQtyXml = itemElementToUpdate.Attribute("inStorage")?.Value
        '                If currentQtyXml <> newQuantity.ToString() Then
        '                    MessageBox.Show($"DEBUG INFO: Updating XML 'inStorage' for Item ID {editedItem.ElementId} from '{currentQtyXml}' to '{newQuantity}'.", "Debug XML Action", MessageBoxButton.OK, MessageBoxImage.Information)
        '                    itemElementToUpdate.SetAttributeValue("inStorage", newQuantity.ToString()) : xmlWasModified = True
        '                Else
        '                    MessageBox.Show($"DEBUG INFO: XML 'inStorage' already matched {newQuantity}. No XML change.", "Debug XML Action", MessageBoxButton.OK, MessageBoxImage.Information)
        '                    xmlWasModified = False
        '                End If
        '            Else ' newQuantity is 0
        '                MessageBox.Show($"DEBUG INFO: Removing <s> element for Item ID {editedItem.ElementId} (quantity is 0).", "Debug XML Action", MessageBoxButton.OK, MessageBoxImage.Information)
        '                itemElementToUpdate.Remove() : xmlWasModified = True
        '            End If
        '        Else ' itemElement is Nothing and newQuantity is 0
        '            MessageBox.Show($"DEBUG INFO: Item ID {editedItem.ElementId} not in XML and new quantity is 0. No XML change.", "Debug XML Action", MessageBoxButton.OK, MessageBoxImage.Information)
        '            xmlWasModified = False
        '        End If

        '    Catch ex As Exception
        '        exceptionMessage = $"DEBUG EXCEPTION during XML Update: {ex.Message}"
        '        e.Cancel = True ' Cancel edit on error
        '    End Try

        '    ' --- Handle results ---
        '    If exceptionMessage IsNot Nothing Then
        '        MessageBox.Show(exceptionMessage, "Debug Error", MessageBoxButton.OK, MessageBoxImage.Error)
        '        TryCast(e.EditingElement, TextBox).Text = editedItem.Quantity.ToString() ' Attempt to restore visually
        '        Return ' Exit handler on error
        '    End If

        '    ' --- Update In-Memory Data (Only if XML update was successful or not needed) ---
        '    MessageBox.Show("DEBUG: Starting In-Memory Update.", "Debug Progress", MessageBoxButton.OK, MessageBoxImage.Information)
        '    Dim itemInMemoryList = selectedContainer.Items.FirstOrDefault(Function(i) i.ElementId = editedItem.ElementId)

        '    ' --- CORRECTED In-Memory Update Logic ---
        '    If newQuantity > 0 Then
        '        ' Always update the Quantity property of the item object from the grid event
        '        If editedItem IsNot Nothing Then
        '            editedItem.Quantity = newQuantity
        '        End If

        '        ' Check if the item exists in the underlying list (selectedContainer.Items)
        '        If itemInMemoryList IsNot Nothing Then
        '            ' It exists, just update its quantity
        '            itemInMemoryList.Quantity = newQuantity
        '        Else
        '            ' It doesn't exist in the list. This happens if it was just re-added to the XML.
        '            ' Add the editedItem (which now has the correct quantity) to the list.
        '            ' Make sure editedItem is valid before adding.
        '            If editedItem IsNot Nothing AndAlso xmlWasModified Then
        '                ' Check if really not in list before adding to avoid potential duplicates
        '                If Not selectedContainer.Items.Any(Function(i) i.ElementId = editedItem.ElementId) Then
        '                    selectedContainer.Items.Add(editedItem)
        '                    ' Also add to the ObservableCollection bound to the grid
        '                    If CurrentContainerItems IsNot Nothing AndAlso Not CurrentContainerItems.Any(Function(i) i.ElementId = editedItem.ElementId) Then
        '                        CurrentContainerItems.Add(editedItem)
        '                        ' Note: Adding might disrupt sorting. Re-sorting/refreshing might be needed here if order is critical.
        '                    End If
        '                End If
        '            End If
        '        End If
        '    Else ' newQuantity is 0
        '        ' Remove the item from the underlying list if it exists
        '        If itemInMemoryList IsNot Nothing Then
        '            selectedContainer.Items.Remove(itemInMemoryList)
        '        End If
        '        ' Remove the item from the ObservableCollection (bound to grid) if it exists
        '        If CurrentContainerItems IsNot Nothing AndAlso editedItem IsNot Nothing Then
        '            Dim itemInObservable = CurrentContainerItems.FirstOrDefault(Function(i) i.ElementId = editedItem.ElementId)
        '            If itemInObservable IsNot Nothing Then
        '                CurrentContainerItems.Remove(itemInObservable) ' This updates the grid visually
        '            End If
        '        End If
        '    End If
        '    ' --- End Corrected Block ---

        '    MessageBox.Show("DEBUG: Finished In-Memory Update.", "Debug Progress", MessageBoxButton.OK, MessageBoxImage.Information)

        '    ' --- Final Messages & Storing Debug Info ---
        '    Dim currentItemName = If(editedItem?.Name, "Unknown Item")
        '    MessageBox.Show($"Quantity for {currentItemName} updated to {newQuantity} (in memory). Save file to persist.", "Quantity Updated", MessageBoxButton.OK, MessageBoxImage.Information)

        '    If xmlWasModified AndAlso targetFeatElement IsNot Nothing Then ' Check targetFeatElement isn't nothing
        '        lastEditedStorageItemDetails = Tuple.Create(targetFeatElement, editedItem.ElementId.ToString(), newQuantity.ToString())
        '        MessageBox.Show($"DEBUG: Set lastEditedStorageItemDetails for Item {editedItem.ElementId}, New Qty {newQuantity}", "CellEditEnding Debug", MessageBoxButton.OK, MessageBoxImage.Asterisk)
        '    Else
        '        lastEditedStorageItemDetails = Nothing
        '        MessageBox.Show($"DEBUG: XML Was NOT Modified or Error occurred. lastEditedStorageItemDetails NOT set.", "CellEditEnding Debug", MessageBoxButton.OK, MessageBoxImage.Information)
        '    End If

        '    MessageBox.Show("DEBUG: dgvStorage_CellEditEnding END", "Debug", MessageBoxButton.OK, MessageBoxImage.Information)

        'End Sub


        ' --- Helper function to create a character XML node from scratch ---
        Private Function CreateCharacterNodeFromScratch(name As String, entId As String, attributes As IEnumerable(Of DataProp), skills As IEnumerable(Of DataProp), traits As IEnumerable(Of DataProp)) As XElement
            ' Create the main <c> element with name and entId attributes
            Dim characterNode As New XElement("c",
                New XAttribute("name", name),
                New XAttribute("entId", entId),
                New XAttribute("origName", name)) ' origName is often required

            ' Create <state> element with default attributes that the game expects
            ' The game needs these attributes to parse correctly - missing attributes cause NumberFormatException
            ' Based on the crash, the game tries to parse integers from state attributes
            Dim stateNode As New XElement("state",
                New XAttribute("x", "0"),
                New XAttribute("y", "0"),
                New XAttribute("z", "0"),
                New XAttribute("areaId", "0"))
            ' Note: bedLink is intentionally left out (null) as new characters don't have beds assigned
            characterNode.Add(stateNode)

            ' Create <pers> element with all sub-elements
            Dim persNode As New XElement("pers")

            ' Create <attr> element with all attributes
            Dim attrNode As New XElement("attr")
            For Each attr In attributes
                attrNode.Add(New XElement("a",
                    New XAttribute("id", attr.Id.ToString()),
                    New XAttribute("points", attr.Value.ToString())))
            Next
            persNode.Add(attrNode)

            ' Create <skills> element with all skills
            Dim skillsNode As New XElement("skills")
            For Each skill In skills
                skillsNode.Add(New XElement("s",
                    New XAttribute("sk", skill.Id.ToString()),
                    New XAttribute("level", skill.Value.ToString()),
                    New XAttribute("mxn", skill.Value.ToString()))) ' Set max known to same as level
            Next
            persNode.Add(skillsNode)

            ' Create <traits> element with all traits
            Dim traitsNode As New XElement("traits")
            For Each trait In traits
                traitsNode.Add(New XElement("t",
                    New XAttribute("id", trait.Id.ToString())))
            Next
            persNode.Add(traitsNode)

            ' Create empty <conditions> element
            persNode.Add(New XElement("conditions"))

            ' Create <sociality> element with empty <relationships>
            Dim socialityNode As New XElement("sociality")
            socialityNode.Add(New XElement("relationships"))
            persNode.Add(socialityNode)

            characterNode.Add(persNode)

            ' Create <colors> element with default uniform values
            ' Using common default values: shirtSet=142, pantsSet=143, sp=0, sl=0
            Dim colorsNode As New XElement("colors",
                New XAttribute("shirtSet", "142"),
                New XAttribute("pantsSet", "143"),
                New XAttribute("sp", "0"),
                New XAttribute("sl", "0"),
                New XAttribute("skinSet", ""),
                New XAttribute("glovesOff", "false"),
                New XAttribute("longSleeve", "false"))
            characterNode.Add(colorsNode)

            Return characterNode
        End Function

        ' --- ADDED: Handler for Create New Crew Button ---

        Private Sub btnAddNewCrew_Click(sender As Object, e As RoutedEventArgs)
            ' Ensure a ship is selected and XML is loaded
            If cmb_ships.SelectedItem Is Nothing OrElse TryCast(cmb_ships.SelectedItem, Ship)?.Sid = -1 Then
                MessageBox.Show("Please select a ship first before adding crew.", "No Ship Selected", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            If xmlDoc Is Nothing OrElse xmlDoc.Root Is Nothing Then
                MessageBox.Show("Please load a save file first.", "No File Loaded", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedShip = TryCast(cmb_ships.SelectedItem, Ship)
            Dim shipElement = xmlDoc.Descendants("ship").FirstOrDefault(Function(s) s.Attribute("sid")?.Value = selectedShip.Sid.ToString())
            Dim charactersNode = shipElement?.Element("characters")

            If shipElement Is Nothing OrElse charactersNode Is Nothing Then
                MessageBox.Show($"Could not find ship (SID:{selectedShip.Sid}) or its <characters> node in the XML.", "XML Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' --- Get available characters for template selection ---
            Dim availableCharacters = characters.Where(Function(c) c.ShipSid = selectedShip.Sid).ToList()
            If availableCharacters.Count = 0 Then
                MessageBox.Show($"The selected ship (SID:{selectedShip.Sid}) has no existing crew members to use as a template. Please add a crew member first.", "No Template Available", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' --- Show New Crew Window with available characters ---
            Dim newCrewWin As New NewCrewWindow(availableCharacters) With {.Owner = Me}
            Dim result = newCrewWin.ShowDialog()

            If result.HasValue AndAlso result.Value = True Then ' User clicked Create
                ' --- Get Data From Window ---
                Dim newName = newCrewWin.NewCrewName
                Dim newAttributes = newCrewWin.Attributes
                Dim newSkills = newCrewWin.Skills
                Dim newTraits = newCrewWin.Traits
                Dim templateCharacterId = newCrewWin.SelectedTemplateCharacterId

                Try
                    ' --- Get Template Character XML Node ---
                    Dim templateCharacterNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = templateCharacterId.ToString())
                    If templateCharacterNode Is Nothing Then
                        Throw New Exception($"Could not find template character with ID {templateCharacterId} in XML.")
                    End If

                    ' --- Get and Increment ID Counter ---
                    ' idCounter is in <masterData> element, not on root <game> element
                    Dim masterDataNode = xmlDoc.Root.Element("masterData")
                    If masterDataNode Is Nothing Then Throw New Exception("Cannot find <masterData> element in XML.")

                    Dim idCounterAttr = masterDataNode.Attribute("idCounter")
                    If idCounterAttr Is Nothing Then Throw New Exception("Cannot find 'idCounter' attribute on <masterData> element.")
                    Dim nextId As Long = 0
                    If Not Long.TryParse(idCounterAttr.Value, nextId) Then Throw New Exception($"Cannot parse 'idCounter' value '{idCounterAttr.Value}'.")
                    nextId += 1
                    Dim newEntIdStr = nextId.ToString()
                    idCounterAttr.Value = newEntIdStr
                    MessageBox.Show($"Assigned new Entity ID: {newEntIdStr}", "Info", MessageBoxButton.OK, MessageBoxImage.Information)

                    ' --- Clone Template and Modify with User's Data ---
                    Dim newCharacterNode As New XElement(templateCharacterNode) ' Deep copy to get all required structure

                    ' Update Basic Info
                    newCharacterNode.SetAttributeValue("name", newName)
                    newCharacterNode.SetAttributeValue("entId", newEntIdStr)
                    If newCharacterNode.Attribute("origName") IsNot Nothing Then
                        newCharacterNode.SetAttributeValue("origName", newName)
                    End If
                    newCharacterNode.Element("state")?.SetAttributeValue("bedLink", Nothing) ' Clear inherited bed link

                    ' Update Stats within <pers>
                    Dim persNode = newCharacterNode.Element("pers")
                    If persNode IsNot Nothing Then
                        ' Attributes
                        Dim attrNode = persNode.Element("attr")
                        If attrNode IsNot Nothing Then
                            For Each newAttr In newAttributes
                                Dim existingA = attrNode.Elements("a").FirstOrDefault(Function(a) a.Attribute("id")?.Value = newAttr.Id.ToString())
                                If existingA IsNot Nothing Then
                                    existingA.SetAttributeValue("points", newAttr.Value.ToString())
                                Else
                                    ' Add new attribute if it doesn't exist
                                    attrNode.Add(New XElement("a",
                                        New XAttribute("id", newAttr.Id.ToString()),
                                        New XAttribute("points", newAttr.Value.ToString())))
                                End If
                            Next
                        End If

                        ' Skills
                        Dim skillsNode = persNode.Element("skills")
                        If skillsNode IsNot Nothing Then
                            For Each newSkill In newSkills
                                Dim existingS = skillsNode.Elements("s").FirstOrDefault(Function(s) s.Attribute("sk")?.Value = newSkill.Id.ToString())
                                If existingS IsNot Nothing Then
                                    existingS.SetAttributeValue("level", newSkill.Value.ToString())
                                    existingS.SetAttributeValue("mxn", newSkill.Value.ToString())
                                Else
                                    ' Add new skill if it doesn't exist
                                    skillsNode.Add(New XElement("s",
                                        New XAttribute("sk", newSkill.Id.ToString()),
                                        New XAttribute("level", newSkill.Value.ToString()),
                                        New XAttribute("mxn", newSkill.Value.ToString())))
                                End If
                            Next
                        End If

                        ' Traits - replace all
                        Dim traitsNode = persNode.Element("traits")
                        If traitsNode IsNot Nothing Then
                            traitsNode.RemoveNodes() ' Clear existing template traits
                            For Each newTrait In newTraits
                                traitsNode.Add(New XElement("t", New XAttribute("id", newTrait.Id.ToString())))
                            Next
                        End If

                        ' Clear Conditions and Relationships
                        Dim conditionsNode = persNode.Element("conditions")
                        If conditionsNode IsNot Nothing Then conditionsNode.RemoveNodes()

                        Dim socialityNode = persNode.Element("sociality")
                        If socialityNode IsNot Nothing Then
                            Dim relationshipsNode = socialityNode.Element("relationships")
                            If relationshipsNode IsNot Nothing Then relationshipsNode.RemoveNodes()
                        End If
                    Else
                        Throw New Exception("Template <pers> node missing. Cannot create character.")
                    End If

                    ' Add New Character Node to XML Characters Node
                    charactersNode.Add(newCharacterNode)

                    ' Create and Add Character Object to In-Memory List
                    Dim newCharacter As New Character With {
                    .CharacterName = newName, .CharacterEntityId = CInt(newEntIdStr), .ShipSid = selectedShip.Sid,
                    .CharacterAttributes = newAttributes.Select(Function(dp) New DataProp With {.Id = dp.Id, .Name = dp.Name, .Value = dp.Value}).ToList(),
                    .CharacterSkills = newSkills.Select(Function(dp) New DataProp With {.Id = dp.Id, .Name = dp.Name, .Value = dp.Value}).ToList(),
                    .CharacterTraits = newTraits.Select(Function(dp) New DataProp With {.Id = dp.Id, .Name = dp.Name}).ToList(),
                    .CharacterConditions = New List(Of DataProp)(), ' Start with empty conditions list
                    .ShirtColorIndex = "142", ' Default shirt set
                    .PantsColorIndex = "143", ' Default pants set
                    .ShirtColorIndexValue = 0, ' Default shirt color index
                    .PantsColorIndexValue = 0, ' Default pants color index
                    .SkinSet = "" ' Empty skin set
                }
                    characters.Add(newCharacter)

                    ' Refresh UI
                    LoadCrewForShip(selectedShip.Sid)
                    ' Mark as unsaved
                    MarkUnsavedChanges()
                    MessageBox.Show($"Crew member '{newName}' added (in memory). Use File > Save to make permanent.", "Crew Added", MessageBoxButton.OK, MessageBoxImage.Information)

                Catch ex As Exception
                    MessageBox.Show($"Error creating crew: {ex.Message}", "Creation Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If ' End If DialogResult = True

        End Sub

        Private Sub btnDeleteCondition_Click(sender As Object, e As RoutedEventArgs)
            ' Get selected character and selected condition from UI
            Dim selectedCharacter = TryCast(lstCharacters.SelectedItem, Character)
            Dim selectedCondition = TryCast(lstConditions.SelectedItem, DataProp)

            If selectedCharacter Is Nothing Then
                MessageBox.Show("Please select a character first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            If selectedCondition Is Nothing Then
                MessageBox.Show("Please select a condition from the list to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            If xmlDoc Is Nothing Then
                MessageBox.Show("Save file not loaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Return
            End If

            ' Confirmation
            Dim confirmResult = MessageBox.Show($"Are you sure you want to remove the condition '{selectedCondition.Name}' from {selectedCharacter.CharacterName}?",
                                                 "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question)
            If confirmResult <> MessageBoxResult.Yes Then Return

            Try
                ' Find the character's node in XML
                Dim characterNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = selectedCharacter.CharacterEntityId.ToString())
                If characterNode Is Nothing Then Throw New Exception($"Character node with entId {selectedCharacter.CharacterEntityId} not found.")

                ' Find the conditions node
                Dim conditionsNode = characterNode.Element("pers")?.Element("conditions")
                If conditionsNode Is Nothing Then Throw New Exception("<pers><conditions> node not found for character.")

                ' Find the specific condition element to remove by ID
                Dim conditionElementToRemove = conditionsNode.Elements("c").FirstOrDefault(Function(c) c.Attribute("id")?.Value = selectedCondition.Id.ToString())

                If conditionElementToRemove IsNot Nothing Then
                    ' Remove from XML
                    conditionElementToRemove.Remove()

                    ' Remove from in-memory list
                    Dim conditionInMemory = selectedCharacter.CharacterConditions.FirstOrDefault(Function(c) c.Id = selectedCondition.Id)
                    If conditionInMemory IsNot Nothing Then
                        selectedCharacter.CharacterConditions.Remove(conditionInMemory)
                    End If

                    ' Mark as unsaved
                    MarkUnsavedChanges()

                    ' Refresh ListBox
                    lstConditions.ItemsSource = Nothing
                    lstConditions.ItemsSource = New ObservableCollection(Of DataProp)(selectedCharacter.CharacterConditions) ' Rebind to update UI

                    MessageBox.Show($"Condition '{selectedCondition.Name}' removed (in memory). Use File > Save to make permanent.", "Condition Removed", MessageBoxButton.OK, MessageBoxImage.Information)
                Else
                    MessageBox.Show($"Condition '{selectedCondition.Name}' (ID: {selectedCondition.Id}) not found in the character's XML <conditions> node. It might have already been removed.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning)
                    ' Optionally refresh list anyway in case memory was out of sync
                    lstConditions.ItemsSource = Nothing
                    lstConditions.ItemsSource = New ObservableCollection(Of DataProp)(selectedCharacter.CharacterConditions)
                End If

            Catch ex As Exception
                MessageBox.Show($"Error removing condition: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        Private Sub dgvRelationships_RowEditEnding(sender As Object, e As DataGridRowEditEndingEventArgs) Handles dgvRelationships.RowEditEnding
            If e.EditAction <> DataGridEditAction.Commit Then Exit Sub

            ' Use Dispatcher to ensure data object is updated by binding first
            Dispatcher.BeginInvoke(New Action(Sub()
                                                  Dim editedRel As RelationshipInfo = TryCast(e.Row.Item, RelationshipInfo)
                                                  If editedRel Is Nothing Then Exit Sub

                                                  Dim currentCharacter As Character = TryCast(lstCharacters.SelectedItem, Character)
                                                  If currentCharacter Is Nothing OrElse xmlDoc Is Nothing Then Exit Sub ' Need context

                                                  ' Optional: Validate new values (e.g., ensure they are within -100 to 100 or other game limits)
                                                  ' Example:
                                                  ' If editedRel.Friendship < -100 OrElse editedRel.Friendship > 100 Then ' Adjust range as needed
                                                  '      MessageBox.Show("Friendship value must be between -100 and 100.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                                                  '      ' TODO: Revert Change - This is tricky without full MVVM/Undo
                                                  '      Exit Sub
                                                  ' End If
                                                  ' (Add similar validation for Attraction, Compatibility if needed)

                                                  ' Find the XML nodes
                                                  Dim characterNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = currentCharacter.CharacterEntityId.ToString())
                                                  Dim relationshipsNode = characterNode?.Element("pers")?.Element("sociality")?.Element("relationships")

                                                  If relationshipsNode Is Nothing Then
                                                      MessageBox.Show("Could not find <relationships> node in XML.", "XML Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                                      Exit Sub
                                                  End If

                                                  ' Find the specific relationship <l> element
                                                  Dim relElement As XElement = relationshipsNode.Elements("l").FirstOrDefault(Function(l) l.Attribute("targetId")?.Value = editedRel.TargetId.ToString())

                                                  If relElement Is Nothing Then
                                                      MessageBox.Show($"Could not find relationship XML node for target ID {editedRel.TargetId}.", "XML Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                                      Exit Sub
                                                  End If

                                                  ' Update attributes in the in-memory XML
                                                  Try
                                                      relElement.SetAttributeValue("friendship", editedRel.Friendship.ToString())
                                                      relElement.SetAttributeValue("attraction", editedRel.Attraction.ToString())
                                                      relElement.SetAttributeValue("compatibility", editedRel.Compatibility.ToString())

                                                      ' Mark as unsaved
                                                      MarkUnsavedChanges()
                                                      MessageBox.Show($"Relationship with {editedRel.TargetName} updated (in memory). Use File > Save to make permanent.", "Relationship Updated", MessageBoxButton.OK, MessageBoxImage.Information)

                                                  Catch ex As Exception
                                                      MessageBox.Show($"Error updating relationship XML: {ex.Message}", "XML Update Error", MessageBoxButton.OK, MessageBoxImage.Error)
                                                  End Try

                                              End Sub), DispatcherPriority.Background)
        End Sub

        Private Sub AboutMenu_Click(sender As Object, e As RoutedEventArgs)
            ' Create an instance of the new About window
            Dim aboutWin As New AboutWindow()

            ' Set the owner to the main window (so it centers correctly)
            aboutWin.Owner = Me

            ' Show the window as a modal dialog (blocks interaction with main window)
            aboutWin.ShowDialog()
        End Sub


        Private Sub BtnBulkAddCrew_Click(sender As Object, e As RoutedEventArgs)
            ' Ensure XML is loaded
            If xmlDoc Is Nothing OrElse xmlDoc.Root Is Nothing Then
                lblBulkStatus.Text = "Please load a save file first."
                Return
            End If

            ' 1. Get the selected ship
            Dim selectedShip As Ship = TryCast(cmb_ships.SelectedItem, Ship)
            If selectedShip Is Nothing OrElse selectedShip.Sid = -1 Then
                lblBulkStatus.Text = "Please select a ship first."
                Return
            End If

            Dim selectedShipNode As XElement = xmlDoc.Descendants("ship").FirstOrDefault(Function(s) s.Attribute("sid")?.Value = selectedShip.Sid.ToString())
            If selectedShipNode Is Nothing Then
                lblBulkStatus.Text = $"Error: Could not find ship with SID {selectedShip.Sid} in XML."
                Return
            End If

            ' 2. Get the selected template crew member
            Dim templateCrewElement As XElement = GetSelectedTemplateCrewNode()
            If templateCrewElement Is Nothing Then
                lblBulkStatus.Text = "Please select a template crew member."
                Return
            End If

            ' 3. Get and validate the amount to add
            Dim amountToAdd As Integer
            If Not Integer.TryParse(txtBulkCrewAmount.Text, amountToAdd) OrElse amountToAdd <= 0 Then
                lblBulkStatus.Text = "Please enter a valid positive number to add."
                Return
            End If

            ' Disable button during processing
            btnBulkAddCrew.IsEnabled = False
            lblBulkStatus.Text = $"Adding {amountToAdd} crew..."

            Try
                ' 4. Get or create the characters node
                Dim charactersNode As XElement = selectedShipNode.Element("characters")
                If charactersNode Is Nothing Then
                    charactersNode = New XElement("characters")
                    selectedShipNode.Add(charactersNode)
                End If

                ' 5. Get and validate idCounter
                Dim idCounterAttr As XAttribute = xmlDoc.Root.Attribute("idCounter")
                If idCounterAttr Is Nothing Then
                    Throw New Exception("Could not find 'idCounter' attribute in XML root <game> element.")
                End If
                Dim currentId As Long
                If Not Long.TryParse(idCounterAttr.Value, currentId) Then
                    Throw New Exception($"Invalid 'idCounter' value: '{idCounterAttr.Value}'")
                End If

                ' 6. Get existing names for the selected ship to check for duplicates
                Dim existingNames As New HashSet(Of String)(characters.Where(Function(c) c.ShipSid = selectedShip.Sid).Select(Function(c) c.CharacterName))

                ' 7. Add crew members
                Dim crewAdded As Integer = 0
                Dim maxAttempts As Integer = amountToAdd * 10 ' Prevent infinite loop
                Dim attempts As Integer = 0

                While crewAdded < amountToAdd AndAlso attempts < maxAttempts
                    attempts += 1

                    ' Generate a random first name
                    Dim crewName As String = NameGenerator.GetRandomFirstName()

                    ' Check for duplicate name
                    If existingNames.Contains(crewName) Then
                        Continue While ' Name already exists, try another
                    End If

                    ' Add the unique name to the set
                    existingNames.Add(crewName)

                    currentId += 1
                    Dim newEntId As String = currentId.ToString()

                    ' Clone the template element
                    Dim newCrewElement As New XElement(templateCrewElement)

                    ' Update attributes
                    newCrewElement.SetAttributeValue("entId", newEntId)
                    newCrewElement.SetAttributeValue("name", crewName)
                    If newCrewElement.Attribute("origName") IsNot Nothing Then
                        newCrewElement.SetAttributeValue("origName", crewName)
                    End If

                    ' Clear bedLink and relationships to avoid conflicts
                    newCrewElement.Element("state")?.SetAttributeValue("bedLink", Nothing)
                    newCrewElement.Element("sociality")?.Element("relationships")?.Remove()

                    ' Add to XML
                    charactersNode.Add(newCrewElement)

                    ' Update in-memory characters list
                    Dim newCharacter As New Character With {
                .CharacterName = crewName,
                .CharacterEntityId = CInt(newEntId),
                .ShipSid = selectedShip.Sid
            }
                    LoadSkillsTraitsAttrs(newCrewElement, newCharacter)
                    characters.Add(newCharacter)

                    crewAdded += 1
                End While

                ' Update idCounter
                idCounterAttr.Value = currentId.ToString()

                ' Check if we added all requested crew
                If crewAdded < amountToAdd Then
                    lblBulkStatus.Text = $"Added {crewAdded} crew. Could not add all {amountToAdd} due to unique name constraints."
                Else
                    lblBulkStatus.Text = $"{amountToAdd} crew added successfully (in memory). Save file to persist."
                End If

                ' Refresh crew UI
                LoadCrewForShip(selectedShip.Sid)
                PopulateTemplateCrewComboBox(selectedShip.Sid)

                ' Mark as unsaved
                MarkUnsavedChanges()

            Catch ex As Exception
                lblBulkStatus.Text = $"Error adding crew: {ex.Message}"
                Debug.WriteLine($"Bulk Add Crew Error: {ex.ToString()}")
            Finally
                btnBulkAddCrew.IsEnabled = True
            End Try
        End Sub


        ' --- Placeholder for your actual XML document variable ---
        ' This needs to be accessible where the button click handler is defined
        'Private xmlDoc As XDocument ' = Your loaded save game XML

        ' --- Placeholder for getting selected ship's XML ---
        ' >>> REPLACE THIS WITH YOUR ACTUAL LOGIC <<<
        Private Function GetSelectedShipNode() As XElement
            ' Example: Return TryCast(cmbShips.SelectedItem, YourShipClass)?.XmlNode
            ' MessageBox.Show("GetSelectedShipNode() needs to be implemented!", "Placeholder", MessageBoxButton.OK, MessageBoxImage.Warning)
            ' For testing, you might temporarily return the first player ship:
            Return xmlDoc?.Root?.Element("ships")?.Elements("ship")?.FirstOrDefault(Function(s) s.Attribute("owner")?.Value = "Player")
            ' Return Nothing ' Return nothing until implemented
        End Function

        Private Function GetSelectedTemplateCrewNode() As XElement
            Dim selectedCharacter As Character = TryCast(cmbTemplateCrew.SelectedItem, Character)
            If selectedCharacter Is Nothing Then
                Return Nothing
            End If
            Debug.WriteLine($"Selected Template Crew ID: {selectedCharacter.CharacterEntityId}")
            Dim crewNode = xmlDoc?.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = selectedCharacter.CharacterEntityId.ToString())
            If crewNode Is Nothing Then
                Debug.WriteLine($"Could not find XML node for crew ID: {selectedCharacter.CharacterEntityId}")
            End If
            Return crewNode
        End Function

        ' --- You also need code to populate cmbTemplateCrew when a ship is selected ---

        Private Sub PopulateTemplateCrewComboBox(shipSid As Integer)
            Debug.WriteLine($"Populating Template Crew for Ship SID: {shipSid}")
            Debug.WriteLine($"Total Characters in List: {characters.Count}")
            Debug.WriteLine($"Character SIDs: {String.Join(", ", characters.Select(Function(c) $"ID:{c.CharacterEntityId}, ShipSid:{c.ShipSid}"))}")

            cmbTemplateCrew.ItemsSource = Nothing
            If shipSid = -1 Then
                lblBulkStatus.Text = "Please select a ship first."
                btnBulkAddCrew.IsEnabled = False
                Return
            End If

            Dim crewList = characters.Where(Function(c) c.ShipSid = shipSid).OrderBy(Function(c) c.CharacterName).ToList()
            Debug.WriteLine($"Crew Found for Ship {shipSid}: {crewList.Count}")

            If crewList.Count = 0 Then
                lblBulkStatus.Text = "No crew members found for this ship. Add a crew member first."
                btnBulkAddCrew.IsEnabled = False
                Return
            End If

            cmbTemplateCrew.ItemsSource = crewList
            Debug.WriteLine($"cmbTemplateCrew Items Count: {cmbTemplateCrew.Items.Count}")
            cmbTemplateCrew.DisplayMemberPath = "CharacterName"
            cmbTemplateCrew.SelectedValuePath = "CharacterEntityId"
            cmbTemplateCrew.Items.Refresh()

            If cmbTemplateCrew.Items.Count > 0 Then
                cmbTemplateCrew.SelectedIndex = 0
                btnBulkAddCrew.IsEnabled = True
                lblBulkStatus.Text = $"Template crew loaded ({crewList.Count} members). Ready to add crew."
            Else
                btnBulkAddCrew.IsEnabled = False
                lblBulkStatus.Text = "No crew members found for this ship. Add a crew member first."
            End If
        End Sub

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

        ' Method to clean problematic XML before parsing
        Private Function CleanXmlFile(filePath As String) As String
            Try
                ' Read the file content
                Dim xmlContent As String = File.ReadAllText(filePath)

                ' Common fixes for XML parsing issues

                ' 1. Remove any null characters that might cause issues
                xmlContent = xmlContent.Replace(Chr(0), "")

                ' 2. Fix common entity issues - replace problematic characters
                xmlContent = xmlContent.Replace("&", "&amp;")
                xmlContent = xmlContent.Replace("<", "&lt;")
                xmlContent = xmlContent.Replace(">", "&gt;")
                xmlContent = xmlContent.Replace("""", "&quot;")
                xmlContent = xmlContent.Replace("'", "&apos;")

                ' 3. But we need to restore the actual XML tags
                xmlContent = xmlContent.Replace("&lt;", "<")
                xmlContent = xmlContent.Replace("&gt;", ">")

                ' 4. Handle specific cases where & might be part of valid content
                ' This is a more sophisticated approach - we'll try to preserve valid XML structure
                Dim lines = xmlContent.Split(Environment.NewLine)
                Dim cleanedLines As New List(Of String)

                For Each line In lines
                    Dim cleanedLine = line

                    ' Skip lines that are just whitespace
                    If String.IsNullOrWhiteSpace(line) Then
                        cleanedLines.Add(line)
                        Continue For
                    End If

                    ' Try to preserve XML structure while fixing entities
                    ' Look for patterns that might indicate problematic content
                    If line.Contains("&") AndAlso Not line.Contains("&amp;") AndAlso Not line.Contains("&lt;") AndAlso Not line.Contains("&gt;") AndAlso Not line.Contains("&quot;") AndAlso Not line.Contains("&apos;") Then
                        ' This line has unescaped & characters that aren't part of valid XML entities
                        ' Replace them with &amp;
                        cleanedLine = System.Text.RegularExpressions.Regex.Replace(line, "&(?!amp;|lt;|gt;|quot;|apos;)", "&amp;")
                    End If

                    cleanedLines.Add(cleanedLine)
                Next

                Return String.Join(Environment.NewLine, cleanedLines)

            Catch ex As Exception
                ' If cleaning fails, throw the original exception
                Throw New Exception($"Failed to clean XML file: {ex.Message}", ex)
            End Try
        End Function

        ' Method to remove problematic ship sections from XML
        ' Optional parameters: errorLine and errorPosition to help identify the problematic section
        Private Function RemoveProblematicShips(filePath As String, Optional errorLine As Integer = -1, Optional errorPosition As Integer = -1) As String
            Try
                Dim xmlContent As String = File.ReadAllText(filePath)

                ' Look for ship sections that might be problematic
                ' Based on user reports, these are often Haven Foundation ships or quest-related ships
                Dim lines = xmlContent.Split({Environment.NewLine, vbCrLf, vbLf}, StringSplitOptions.None)
                Dim cleanedLines As New List(Of String)
                Dim skipMode As Boolean = False
                Dim shipDepth As Integer = 0
                Dim removedShips As New List(Of String)
                Dim currentJStart As Integer = -1

                For i As Integer = 0 To lines.Length - 1
                    Dim line = lines(i)
                    Dim lineNum = i + 1 ' Line numbers are 1-based

                    ' If we have an error line number, try to find the <j> block that contains it
                    If errorLine > 0 AndAlso lineNum <= errorLine AndAlso currentJStart = -1 Then
                        ' Look backwards from the error line to find the opening <j> tag
                        If lineNum <= errorLine AndAlso lineNum >= errorLine - 50 Then
                            If line.Trim().StartsWith("<j ") Then
                                currentJStart = i
                                ' Mark this section for removal
                                skipMode = True
                                shipDepth = 0
                                removedShips.Add($"Ship/Station at line {lineNum} (near error location)")
                            End If
                        End If
                    End If

                    ' Check if this line starts a ship/station section (<j> tags are stations/structures that can contain ships)
                    If line.Trim().StartsWith("<j ") OrElse line.Trim().StartsWith("<j>") Then
                        Dim isProblematic = False
                        Dim shipInfo As String = ""

                        ' Extract ship/station ID for reporting
                        Dim sidMatch = System.Text.RegularExpressions.Regex.Match(line, "sid=""([^""]+)""")
                        Dim idMatch = System.Text.RegularExpressions.Regex.Match(line, "id=""([^""]+)""")
                        If sidMatch.Success Then
                            shipInfo = $"Ship/Station ID: {sidMatch.Groups(1).Value} (Line {lineNum})"
                        ElseIf idMatch.Success Then
                            shipInfo = $"Structure ID: {idMatch.Groups(1).Value} (Line {lineNum})"
                        Else
                            shipInfo = $"Ship/Station at line {lineNum}"
                        End If

                        ' Look for indicators of Haven Foundation or quest ships
                        ' Check for faction ID 4144 (Haven Foundation)
                        If line.Contains("factionId=""4144""") OrElse line.Contains("factionId='4144'") Then
                            isProblematic = True
                            shipInfo += " (Haven Foundation - Faction ID 4144)"
                        End If

                        ' Check for keywords
                        If line.Contains("Haven") OrElse line.Contains("Foundation") OrElse
                           line.Contains("Exodus") OrElse line.Contains("ExodusFleet") OrElse
                           line.Contains("Fleet") OrElse line.Contains("missionType") Then
                            isProblematic = True
                            shipInfo += " (Haven Foundation/Quest Ship)"
                        End If

                        ' Also check the next few lines for more context (look deeper for nested content)
                        Dim contextLines = Math.Min(50, lines.Length - i - 1)
                        For j As Integer = 1 To contextLines
                            If i + j < lines.Length Then
                                Dim nextLine = lines(i + j)

                                ' Check for Haven Foundation indicators
                                If nextLine.Contains("Haven") OrElse nextLine.Contains("Foundation") OrElse
                                   nextLine.Contains("Exodus") OrElse nextLine.Contains("ExodusFleet") OrElse
                                   nextLine.Contains("Fleet") OrElse nextLine.Contains("quest") OrElse
                                   nextLine.Contains("factionId=""4144""") OrElse nextLine.Contains("factionId='4144'") OrElse
                                   nextLine.Contains("missionType") OrElse nextLine.Contains("type=""ExodusFleet""") Then
                                    isProblematic = True
                                    If String.IsNullOrEmpty(shipInfo) OrElse Not shipInfo.Contains("Haven") Then
                                        shipInfo = If(String.IsNullOrEmpty(shipInfo), "Quest-related Ship", shipInfo & " (Quest-related)")
                                    End If
                                    Exit For
                                End If

                                ' If we've gone too far into the structure, stop looking
                                If nextLine.Trim().StartsWith("</j>") Then
                                    Exit For
                                End If
                            End If
                        Next

                        ' If we're near an error line, mark as problematic
                        If errorLine > 0 AndAlso Math.Abs(lineNum - errorLine) < 20 Then
                            isProblematic = True
                            shipInfo += $" (Near error at line {errorLine})"
                        End If

                        If isProblematic Then
                            ' Skip this entire ship/station section
                            skipMode = True
                            shipDepth = 0
                            currentJStart = i
                            If Not String.IsNullOrEmpty(shipInfo) Then
                                removedShips.Add(shipInfo)
                            End If
                        End If
                    End If

                    If skipMode Then
                        ' Count opening and closing tags to know when to stop skipping
                        Dim trimmedLine = line.Trim()
                        If trimmedLine.StartsWith("<j") AndAlso Not trimmedLine.EndsWith("/>") Then
                            ' Self-closing <j/> tags don't count
                            If trimmedLine.StartsWith("<j ") OrElse trimmedLine.StartsWith("<j>") Then
                                shipDepth += 1
                            End If
                        ElseIf trimmedLine.StartsWith("</j>") Then
                            shipDepth -= 1
                            If shipDepth <= 0 Then
                                skipMode = False
                                currentJStart = -1
                            End If
                        End If
                        Continue For
                    End If

                    cleanedLines.Add(line)
                Next

                ' Store information about removed ships for user notification
                If removedShips.Count > 0 Then
                    _removedShipsInfo = String.Join(Environment.NewLine, removedShips)
                End If

                Return String.Join(Environment.NewLine, cleanedLines)

            Catch ex As Exception
                ' If this fails, return the original content
                Return File.ReadAllText(filePath)
            End Try
        End Function

        ' Method to fix specific XML issues that cause EntityName errors
        Private Function FixEntityNameIssues(filePath As String) As String
            Try
                Dim xmlContent As String = File.ReadAllText(filePath)

                ' Common fixes for EntityName errors
                ' EntityName errors typically occur when the parser encounters &something; where 'something' is not a valid entity

                ' 1. Fix invalid entity references like &something; (where something is not a valid entity)
                ' This pattern matches & followed by letters/numbers and a semicolon, but not valid entities
                xmlContent = System.Text.RegularExpressions.Regex.Replace(xmlContent,
                    "&([a-zA-Z][a-zA-Z0-9]{1,31});(?!amp;|lt;|gt;|quot;|apos;|#\d+;|#x[0-9a-fA-F]+;)",
                    "&amp;$1;",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)

                ' 2. Fix unescaped ampersands in attribute values (but preserve valid entities)
                ' Match & that's not part of a valid entity in attribute values
                ' This is a simpler approach - just escape ampersands in attribute values that aren't valid entities
                xmlContent = System.Text.RegularExpressions.Regex.Replace(xmlContent,
                    "=""([^""]*?)&([^""&]*)""",
                    "=""$1&amp;$2""")

                ' 3. Fix unescaped ampersands at the end of lines or before closing tags
                xmlContent = System.Text.RegularExpressions.Regex.Replace(xmlContent, "&(\s*[>\r\n])", "&amp;$1")

                ' 4. Fix unescaped ampersands in text content (but not in valid XML entities)
                xmlContent = System.Text.RegularExpressions.Regex.Replace(xmlContent, "&(?!amp;|lt;|gt;|quot;|apos;|#\d+;|#x[0-9a-fA-F]+;)", "&amp;")

                ' 4. Fix any remaining problematic characters
                xmlContent = xmlContent.Replace(Chr(0), "") ' Remove null characters
                xmlContent = xmlContent.Replace(Chr(1), "") ' Remove control characters
                xmlContent = xmlContent.Replace(Chr(2), "")
                xmlContent = xmlContent.Replace(Chr(3), "")
                xmlContent = xmlContent.Replace(Chr(4), "")
                xmlContent = xmlContent.Replace(Chr(5), "")
                xmlContent = xmlContent.Replace(Chr(6), "")
                xmlContent = xmlContent.Replace(Chr(7), "")
                xmlContent = xmlContent.Replace(Chr(8), "")
                xmlContent = xmlContent.Replace(Chr(11), "")
                xmlContent = xmlContent.Replace(Chr(12), "")
                xmlContent = xmlContent.Replace(Chr(14), "")
                xmlContent = xmlContent.Replace(Chr(15), "")
                xmlContent = xmlContent.Replace(Chr(16), "")
                xmlContent = xmlContent.Replace(Chr(17), "")
                xmlContent = xmlContent.Replace(Chr(18), "")
                xmlContent = xmlContent.Replace(Chr(19), "")
                xmlContent = xmlContent.Replace(Chr(20), "")

                ' 5. Fix specific issues that might cause EntityName errors
                ' Look for patterns like &something; where 'something' is not a valid entity
                xmlContent = System.Text.RegularExpressions.Regex.Replace(xmlContent, "&([a-zA-Z0-9]+);(?!amp;|lt;|gt;|quot;|apos;)", "&amp;$1;")

                ' 6. Fix any remaining unescaped special characters in attribute values
                xmlContent = System.Text.RegularExpressions.Regex.Replace(xmlContent, "=""([^""]*?)&([^""]*?)""", "=""$1&amp;$2""")

                Return xmlContent

            Catch ex As Exception
                ' If this fails, return the original content
                Return File.ReadAllText(filePath)
            End Try
        End Function

        ' --- New Attribute Management Methods ---

        Private Sub BulkTabControl_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim tabControl = CType(sender, TabControl)
            If tabControl.SelectedIndex = 0 Then
                ' Bulk Crew tab
                bulkCrewPanel.Visibility = Visibility.Visible
                bulkStatsPanel.Visibility = Visibility.Collapsed
            ElseIf tabControl.SelectedIndex = 1 Then
                ' Bulk Character Stats tab (combined attributes and skills)
                bulkCrewPanel.Visibility = Visibility.Collapsed
                bulkStatsPanel.Visibility = Visibility.Visible
            End If
        End Sub

        Private Sub btnSetCharacterAttributes_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(currentFilePath) Then
                MessageBox.Show("Load a save file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If lstCharacters.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a character first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim attributeValue As Integer
            If Not Integer.TryParse(txtAttributeValue.Text, attributeValue) OrElse attributeValue < 1 OrElse attributeValue > 10 Then
                MessageBox.Show("Please enter a valid attribute value between 1 and 10.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedChar = CType(lstCharacters.SelectedItem, Character)

            ' Update in-memory data for selected character only
            For Each attrib In selectedChar.CharacterAttributes
                attrib.Value = attributeValue
            Next

            ' Refresh grid for selected character
            dgvAttributes.ItemsSource = Nothing
            dgvAttributes.ItemsSource = New ObservableCollection(Of DataProp)(selectedChar.CharacterAttributes)

            ' Update XML for selected character only
            Dim charNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = selectedChar.CharacterEntityId.ToString())
            If charNode IsNot Nothing Then
                Dim attrElement = charNode.Element("pers")?.Element("attr")
                If attrElement IsNot Nothing Then
                    For Each aNode In attrElement.Elements("a")
                        aNode.SetAttributeValue("points", attributeValue.ToString())
                    Next
                End If
            End If

            ' Mark as unsaved (changes are in memory, will be saved when user clicks File > Save)
            MarkUnsavedChanges()
        End Sub

        Private Sub btnBulkSetAttributes_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(currentFilePath) Then
                MessageBox.Show("Load a save file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim attributeValue As Integer
            If Not Integer.TryParse(txtBulkAttributeValue.Text, attributeValue) OrElse attributeValue < 1 OrElse attributeValue > 10 Then
                MessageBox.Show("Please enter a valid attribute value between 1 and 10.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Update in-memory data for all characters
            For Each character In characters
                For Each attrib In character.CharacterAttributes
                    attrib.Value = attributeValue
                Next
            Next

            ' Refresh grid for selected character if any
            If lstCharacters.SelectedItem IsNot Nothing Then
                Dim selectedChar = CType(lstCharacters.SelectedItem, Character)
                dgvAttributes.ItemsSource = Nothing
                dgvAttributes.ItemsSource = New ObservableCollection(Of DataProp)(selectedChar.CharacterAttributes)
            End If

            ' Update XML for all characters
            Dim shipNodes = xmlDoc.Descendants("ship")
            If Not shipNodes.Any() Then Return

            For Each shipXml In shipNodes
                Dim charactersElement = shipXml.Element("characters")
                If charactersElement IsNot Nothing Then
                    For Each cNode In charactersElement.Elements("c")
                        Dim attrElement = cNode.Element("pers")?.Element("attr")
                        If attrElement IsNot Nothing Then
                            For Each aNode In attrElement.Elements("a")
                                aNode.SetAttributeValue("points", attributeValue.ToString())
                            Next
                        End If
                    Next
                End If
            Next

            ' Mark as unsaved (changes are in memory, will be saved when user clicks File > Save)
            MarkUnsavedChanges()
            lblBulkAttributeStatus.Text = $"All attributes set to {attributeValue} for every character."
        End Sub

        Private Sub btnSetCharacterSkills_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(currentFilePath) Then
                MessageBox.Show("Load a save file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If lstCharacters.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a character first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim skillValue As Integer
            If Not Integer.TryParse(txtSkillValue.Text, skillValue) OrElse skillValue < 1 OrElse skillValue > 10 Then
                MessageBox.Show("Please enter a valid skill value between 1 and 10.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedChar = CType(lstCharacters.SelectedItem, Character)

            ' Update in-memory data for selected character only
            For Each skill In selectedChar.CharacterSkills
                skill.Value = skillValue
            Next

            ' Refresh grid for selected character
            dgvSkills.ItemsSource = Nothing
            dgvSkills.ItemsSource = New ObservableCollection(Of DataProp)(selectedChar.CharacterSkills)

            ' Update XML for selected character only
            Dim charNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = selectedChar.CharacterEntityId.ToString())
            If charNode IsNot Nothing Then
                Dim skillsElement = charNode.Element("pers")?.Element("skills")
                If skillsElement IsNot Nothing Then
                    For Each sNode In skillsElement.Elements("s")
                        sNode.SetAttributeValue("level", skillValue.ToString())
                    Next
                End If
            End If

            ' Mark as unsaved (changes are in memory, will be saved when user clicks File > Save)
            MarkUnsavedChanges()
        End Sub

        Private Sub btnSetCharacterMaxSkills_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(currentFilePath) Then
                MessageBox.Show("Load a save file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            If lstCharacters.SelectedItem Is Nothing Then
                MessageBox.Show("Please select a character first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim skillMaxValue As Integer
            If Not Integer.TryParse(txtSkillMaxValue.Text, skillMaxValue) OrElse skillMaxValue < 1 OrElse skillMaxValue > 10 Then
                MessageBox.Show("Please enter a valid max skill value between 1 and 10.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedChar = CType(lstCharacters.SelectedItem, Character)

            ' Update in-memory data for selected character only
            For Each skill In selectedChar.CharacterSkills
                skill.MaxValue = skillMaxValue
            Next

            ' Refresh grid for selected character
            dgvSkills.ItemsSource = Nothing
            dgvSkills.ItemsSource = New ObservableCollection(Of DataProp)(selectedChar.CharacterSkills)

            ' Update XML for selected character only
            Dim charNode = xmlDoc.Descendants("c").FirstOrDefault(Function(c) c.Attribute("entId")?.Value = selectedChar.CharacterEntityId.ToString())
            If charNode IsNot Nothing Then
                Dim skillsElement = charNode.Element("pers")?.Element("skills")
                If skillsElement IsNot Nothing Then
                    For Each sNode In skillsElement.Elements("s")
                        sNode.SetAttributeValue("mxn", skillMaxValue.ToString())
                    Next
                End If
            End If

            ' Mark as unsaved (changes are in memory, will be saved when user clicks File > Save)
            MarkUnsavedChanges()
        End Sub

        Private Sub btnBulkSetSkills_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(currentFilePath) Then
                MessageBox.Show("Load a save file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim skillValue As Integer
            If Not Integer.TryParse(txtBulkSkillValue.Text, skillValue) OrElse skillValue < 1 OrElse skillValue > 10 Then
                MessageBox.Show("Please enter a valid skill value between 1 and 10.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Update in-memory data for all characters
            For Each character In characters
                For Each skill In character.CharacterSkills
                    skill.Value = skillValue
                Next
            Next

            ' Refresh grid for selected character if any
            If lstCharacters.SelectedItem IsNot Nothing Then
                Dim selectedChar = CType(lstCharacters.SelectedItem, Character)
                dgvSkills.ItemsSource = Nothing
                dgvSkills.ItemsSource = New ObservableCollection(Of DataProp)(selectedChar.CharacterSkills)
            End If

            ' Update XML for all characters
            Dim shipNodes = xmlDoc.Descendants("ship")
            If Not shipNodes.Any() Then Return

            For Each shipXml In shipNodes
                Dim charactersElement = shipXml.Element("characters")
                If charactersElement IsNot Nothing Then
                    For Each cNode In charactersElement.Elements("c")
                        Dim skillsElement = cNode.Element("pers")?.Element("skills")
                        If skillsElement IsNot Nothing Then
                            For Each sNode In skillsElement.Elements("s")
                                sNode.SetAttributeValue("level", skillValue.ToString())
                            Next
                        End If
                    Next
                End If
            Next

            ' Mark as unsaved (changes are in memory, will be saved when user clicks File > Save)
            MarkUnsavedChanges()
            lblBulkSkillStatus.Text = $"All current skills set to {skillValue} for every character."
        End Sub

        Private Sub btnBulkSetMaxSkills_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(currentFilePath) Then
                MessageBox.Show("Load a save file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim skillMaxValue As Integer
            If Not Integer.TryParse(txtBulkSkillMaxValue.Text, skillMaxValue) OrElse skillMaxValue < 1 OrElse skillMaxValue > 10 Then
                MessageBox.Show("Please enter a valid max skill value between 1 and 10.", "Invalid Value", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Update in-memory data for all characters
            For Each character In characters
                For Each skill In character.CharacterSkills
                    skill.MaxValue = skillMaxValue
                Next
            Next

            ' Refresh grid for selected character if any
            If lstCharacters.SelectedItem IsNot Nothing Then
                Dim selectedChar = CType(lstCharacters.SelectedItem, Character)
                dgvSkills.ItemsSource = Nothing
                dgvSkills.ItemsSource = New ObservableCollection(Of DataProp)(selectedChar.CharacterSkills)
            End If

            ' Update XML for all characters
            Dim shipNodes = xmlDoc.Descendants("ship")
            If Not shipNodes.Any() Then Return

            For Each shipXml In shipNodes
                Dim charactersElement = shipXml.Element("characters")
                If charactersElement IsNot Nothing Then
                    For Each cNode In charactersElement.Elements("c")
                        Dim skillsElement = cNode.Element("pers")?.Element("skills")
                        If skillsElement IsNot Nothing Then
                            For Each sNode In skillsElement.Elements("s")
                                sNode.SetAttributeValue("mxn", skillMaxValue.ToString())
                            Next
                        End If
                    Next
                End If
            Next

            ' Mark as unsaved (changes are in memory, will be saved when user clicks File > Save)
            MarkUnsavedChanges()
            lblBulkSkillStatus.Text = $"All max skills set to {skillMaxValue} for every character."
        End Sub

        Private Sub btnBulkSetSkillsMax_Click(sender As Object, e As RoutedEventArgs)
            If xmlDoc Is Nothing OrElse String.IsNullOrEmpty(currentFilePath) Then
                MessageBox.Show("Load a save file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Update in-memory data for all characters
            For Each character In characters
                For Each skill In character.CharacterSkills
                    skill.Value = skill.MaxValue ' Set current level to max level
                Next
            Next

            ' Refresh grid for selected character if any
            If lstCharacters.SelectedItem IsNot Nothing Then
                Dim selectedChar = CType(lstCharacters.SelectedItem, Character)
                dgvSkills.ItemsSource = Nothing
                dgvSkills.ItemsSource = New ObservableCollection(Of DataProp)(selectedChar.CharacterSkills)
            End If

            ' Update XML for all characters
            Dim shipNodes = xmlDoc.Descendants("ship")
            If Not shipNodes.Any() Then Return

            For Each shipXml In shipNodes
                Dim charactersElement = shipXml.Element("characters")
                If charactersElement IsNot Nothing Then
                    For Each cNode In charactersElement.Elements("c")
                        Dim skillsElement = cNode.Element("pers")?.Element("skills")
                        If skillsElement IsNot Nothing Then
                            For Each sNode In skillsElement.Elements("s")
                                Dim currentMax = sNode.Attribute("mxn")?.Value
                                If Not String.IsNullOrEmpty(currentMax) Then
                                    sNode.SetAttributeValue("level", currentMax) ' Set level to current max
                                End If
                            Next
                        End If
                    Next
                End If
            Next

            ' Mark as unsaved (changes are in memory, will be saved when user clicks File > Save)
            MarkUnsavedChanges()
            lblBulkSkillStatus.Text = "All skills set to their maximum level for every character."
        End Sub

        ' ===== UNIFORM EDITING METHODS =====


#Region "Game Start Editor Methods"

        ' Populate the ComboBoxes with categorized items
        Private Sub PopulateGameStartComboBoxes()
            ' Populate Resources ComboBox (EXCLUDE items that belong in Items tab)
            Dim categorizedResources As New List(Of CategorizedItem)()
            For Each kvp In IdCollection.DefaultStorageIDs.OrderBy(Function(x) x.Value)
                Dim category As String = GetGameStartCategoryForItem(kvp.Key, kvp.Value)
                ' EXCLUDE: Weapons, Armor/Apparel, Medical, Equipment, Remote Control, and Breaching Charge
                If category <> "Weapons" AndAlso category <> "Armor/Apparel" AndAlso category <> "Medical" AndAlso
                   category <> "Equipment" AndAlso kvp.Key <> 3386 AndAlso kvp.Key <> 4040 Then ' Exclude Remote Control (3386) and Breaching Charge (4040)
                    categorizedResources.Add(New CategorizedItem(category, kvp.Value, kvp.Key))
                End If
            Next
            Dim sortedResources = categorizedResources.OrderBy(Function(item) item.Category).ThenBy(Function(item) item.ItemName).ToList()

            Dim cvsResources As New CollectionViewSource()
            cvsResources.Source = sortedResources
            If cvsResources.GroupDescriptions.Count = 0 Then
                cvsResources.GroupDescriptions.Add(New PropertyGroupDescription("Category"))
            End If
            cmbGameStartAddResource.ItemsSource = cvsResources.View

            ' Populate Items ComboBox (ONLY weapons, armor, medical, equipment, remote control, and breaching charge)
            Dim categorizedItems As New List(Of CategorizedItem)()
            For Each kvp In IdCollection.DefaultStorageIDs.OrderBy(Function(x) x.Value)
                Dim category As String = GetGameStartCategoryForItem(kvp.Key, kvp.Value)
                ' Filter to ONLY include: Weapons, Armor/Apparel, Medical, Equipment, Remote Control, and Breaching Charge
                If category = "Weapons" OrElse category = "Armor/Apparel" OrElse category = "Medical" OrElse
                   category = "Equipment" OrElse (kvp.Key = 3386) OrElse (kvp.Key = 4040) Then ' Remote Control (3386), Breaching Charge (4040)
                    categorizedItems.Add(New CategorizedItem(category, kvp.Value, kvp.Key))
                End If
            Next
            Dim sortedItems = categorizedItems.OrderBy(Function(item) item.Category).ThenBy(Function(item) item.ItemName).ToList()

            Dim cvsItems As New CollectionViewSource()
            cvsItems.Source = sortedItems
            If cvsItems.GroupDescriptions.Count = 0 Then
                cvsItems.GroupDescriptions.Add(New PropertyGroupDescription("Category"))
            End If
            cmbGameStartAddItem.ItemsSource = cvsItems.View
        End Sub

        ' Get category for item (same logic as GameStartEditorWindow)
        Private Function GetGameStartCategoryForItem(itemId As Integer, itemName As String) As String
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

        ' Get scenario directory path
        Private Function GetGameStartScenarioDirectory() As String
            Try
                ' First try to use saved scenario directory setting
                Dim scenarioDir As String = My.Settings.DefaultScenarioDirectory
                If Not String.IsNullOrEmpty(scenarioDir) AndAlso Directory.Exists(scenarioDir) Then
                    Return scenarioDir
                End If

                ' Try to find scenario directory based on save directory
                Dim saveDir As String = My.Settings.DefaultSaveDirectory
                If Not String.IsNullOrEmpty(saveDir) AndAlso Directory.Exists(saveDir) Then
                    ' Scenario folder is typically in the same parent directory as saves
                    Dim parentDir As String = Directory.GetParent(saveDir).FullName
                    Dim scenarioPath As String = Path.Combine(parentDir, "scenario")
                    If Directory.Exists(scenarioPath) Then
                        Return scenarioPath
                    End If
                End If

                ' Try to find default Space Haven scenario directory
                Dim localAppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                Dim gamePath As String = Path.Combine(localAppData, "Low", "Bugbyte Oy", "Space Haven", "scenario")
                If Directory.Exists(gamePath) Then
                    Return gamePath
                End If

                ' Fallback to My Documents
                Return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            Catch ex As Exception
                Return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            End Try
        End Function

        ' Set Scenario Folder button click handler
        Private Sub btnGameStartSetScenarioFolder_Click(sender As Object, e As RoutedEventArgs)
            Dim folderDialog As New System.Windows.Forms.FolderBrowserDialog() With {
                .Description = "Select the Scenario folder where your custom difficulty game files are stored",
                .ShowNewFolderButton = False
            }

            ' Set initial directory
            Dim currentScenarioDir As String = GetGameStartScenarioDirectory()
            If Directory.Exists(currentScenarioDir) Then
                folderDialog.SelectedPath = currentScenarioDir
            End If

            If folderDialog.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Try
                    My.Settings.DefaultScenarioDirectory = folderDialog.SelectedPath
                    My.Settings.Save()
                    UpdateGameStartScenarioFolderDisplay()
                    MessageBox.Show($"Scenario folder set to:{vbCrLf}{folderDialog.SelectedPath}", "Scenario Folder Set", MessageBoxButton.OK, MessageBoxImage.Information)
                Catch ex As Exception
                    MessageBox.Show($"Error saving scenario folder setting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                End Try
            End If
        End Sub

        ' Load File button click handler
        Private Sub btnGameStartLoadFile_Click(sender As Object, e As RoutedEventArgs)
            Dim openFileDialog As New Microsoft.Win32.OpenFileDialog() With {
            .Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            .Title = "Select Game Start XML File"
        }

            ' Set initial directory to scenario folder
            Try
                Dim initialDir As String = GetGameStartScenarioDirectory()
                openFileDialog.InitialDirectory = initialDir
            Catch ex As Exception
                ' If settings fail, use default
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            End Try

            If openFileDialog.ShowDialog() = True Then
                gameStartFilePath = openFileDialog.FileName
                txtGameStartFilePath.Text = gameStartFilePath
                LoadGameStartXmlFile()
            End If
        End Sub

        ' Load and parse the XML file
        Private Sub LoadGameStartXmlFile()
            Try
                gameStartXmlDoc = XDocument.Load(gameStartFilePath)
                gameStartResourceItems.Clear()
                gameStartItemItems.Clear()

                ' Game Start files have a <diff> root element, then <playerShips>
                Dim diffElement As XElement = Nothing
                If gameStartXmlDoc.Root IsNot Nothing Then
                    If gameStartXmlDoc.Root.Name = "diff" Then
                        diffElement = gameStartXmlDoc.Root
                    Else
                        diffElement = gameStartXmlDoc.Root.Element("diff")
                        If diffElement Is Nothing Then
                            diffElement = gameStartXmlDoc.Root
                        End If
                    End If
                End If

                ' Find the playerShips element
                Dim playerShipsElement = diffElement?.Element("playerShips")
                If playerShipsElement Is Nothing Then
                    MessageBox.Show("Invalid XML structure: 'playerShips' element not found. This may not be a valid Game Start file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get the first ship (l element)
                Dim shipElement = playerShipsElement.Element("l")
                If shipElement Is Nothing Then
                    MessageBox.Show("Invalid XML structure: No ship found in 'playerShips'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Load settings from diff element
                If diffElement IsNot Nothing Then
                    Dim enemiesAttr = diffElement.Attribute("enemiesEnabled")
                    If enemiesAttr IsNot Nothing Then
                        chkGameStartEnemiesEnabled.IsChecked = Boolean.Parse(enemiesAttr.Value)
                    End If

                    Dim friendsAttr = diffElement.Attribute("friendsEnabled")
                    If friendsAttr IsNot Nothing Then
                        chkGameStartFriendsEnabled.IsChecked = Boolean.Parse(friendsAttr.Value)
                    End If

                    Dim loversAttr = diffElement.Attribute("loversEnabled")
                    If loversAttr IsNot Nothing Then
                        chkGameStartLoversEnabled.IsChecked = Boolean.Parse(loversAttr.Value)
                    End If

                    Dim sandboxAttr = diffElement.Attribute("sandbox")
                    If sandboxAttr IsNot Nothing Then
                        chkGameStartSandbox.IsChecked = Boolean.Parse(sandboxAttr.Value)
                    End If
                End If

                ' Load crew and resource settings from ship element
                Dim crewAttr = shipElement.Attribute("crew")
                If crewAttr IsNot Nothing Then
                    txtGameStartCrew.Text = crewAttr.Value
                End If

                Dim maxCrewAttr = shipElement.Attribute("maxCrew")
                If maxCrewAttr IsNot Nothing Then
                    txtGameStartMaxCrew.Text = maxCrewAttr.Value
                End If

                Dim maxResourcesAttr = shipElement.Attribute("maxResources")
                If maxResourcesAttr IsNot Nothing Then
                    txtGameStartMaxResources.Text = maxResourcesAttr.Value
                End If

                Dim maxItemsAttr = shipElement.Attribute("maxItems")
                If maxItemsAttr IsNot Nothing Then
                    txtGameStartMaxItems.Text = maxItemsAttr.Value
                End If

                Dim hangarsPopulatedAttr = shipElement.Attribute("hangarsPopulated")
                If hangarsPopulatedAttr IsNot Nothing Then
                    chkGameStartHangarsPopulated.IsChecked = Boolean.Parse(hangarsPopulatedAttr.Value)
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
                                Dim itemName As String = GetGameStartItemName(elementId)
                                gameStartResourceItems.Add(New GameStartResourceItem With {
                                .ElementId = elementId,
                                .Name = itemName,
                                .Quantity = quantity,
                                .IsSelected = False
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
                                Dim itemName As String = GetGameStartItemName(elementId)
                                gameStartItemItems.Add(New GameStartResourceItem With {
                                .ElementId = elementId,
                                .Name = itemName,
                                .Quantity = quantity,
                                .IsSelected = False
                            })
                            End If
                        End If
                    Next
                End If

                ' Enable UI components
                gameStartSettingsCard.IsEnabled = True
                btnGameStartSaveChanges.IsEnabled = True
                ClearGameStartUnsavedChanges() ' Reset unsaved changes flag when loading new file

                ' Validate limits after loading
                Dim validation = ValidateAllLimits()
                Dim loadMessage = $"File loaded successfully. Found {gameStartResourceItems.Count} resource(s) and {gameStartItemItems.Count} item(s)."

                If Not validation.Item1 Then
                    ' Limits exceeded - disable save button and show warning
                    btnGameStartSaveChanges.IsEnabled = False
                    MessageBox.Show($"{loadMessage}{vbCrLf}{vbCrLf}⚠ WARNING: Limits Exceeded!{vbCrLf}{vbCrLf}{validation.Item2}{vbCrLf}{vbCrLf}You must fix this before saving. Increase the limits or reduce quantities.",
                                    "File Loaded with Warnings", MessageBoxButton.OK, MessageBoxImage.Warning)
                Else
                    MessageBox.Show(loadMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information)
                End If

            Catch ex As Exception
                MessageBox.Show($"Error loading XML file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

        ' Get item name from IdCollection
        Private Function GetGameStartItemName(elementId As Integer) As String
            If IdCollection.DefaultStorageIDs.ContainsKey(elementId) Then
                Return IdCollection.DefaultStorageIDs(elementId)
            Else
                Return $"Unknown Item ({elementId})"
            End If
        End Function

        ' Calculate total quantity of resources
        Private Function GetTotalResourcesQuantity() As Integer
            If gameStartResourceItems Is Nothing Then Return 0
            Return gameStartResourceItems.Sum(Function(i) i.Quantity)
        End Function

        ' Calculate total quantity of items
        Private Function GetTotalItemsQuantity() As Integer
            If gameStartItemItems Is Nothing Then Return 0
            Return gameStartItemItems.Sum(Function(i) i.Quantity)
        End Function

        ' Get max resources limit
        Private Function GetMaxResourcesLimit() As Integer
            Dim maxResources As Integer
            If Integer.TryParse(txtGameStartMaxResources.Text, maxResources) Then
                Return maxResources
            End If
            Return 0
        End Function

        ' Get max items limit
        Private Function GetMaxItemsLimit() As Integer
            Dim maxItems As Integer
            If Integer.TryParse(txtGameStartMaxItems.Text, maxItems) Then
                Return maxItems
            End If
            Return 0
        End Function

        ' Validate resources limit
        Private Function ValidateResourcesLimit(additionalQuantity As Integer) As Tuple(Of Boolean, String)
            Dim currentTotal = GetTotalResourcesQuantity()
            Dim maxLimit = GetMaxResourcesLimit()
            Dim newTotal = currentTotal + additionalQuantity

            If maxLimit > 0 AndAlso newTotal > maxLimit Then
                Return New Tuple(Of Boolean, String)(False, $"Total resources would exceed the maximum limit of {maxLimit}. Current total: {currentTotal}, Adding: {additionalQuantity}, New total: {newTotal}. Please increase the 'Max Resources' limit or reduce quantities.")
            End If
            Return New Tuple(Of Boolean, String)(True, "")
        End Function

        ' Validate items limit
        Private Function ValidateItemsLimit(additionalQuantity As Integer) As Tuple(Of Boolean, String)
            Dim currentTotal = GetTotalItemsQuantity()
            Dim maxLimit = GetMaxItemsLimit()
            Dim newTotal = currentTotal + additionalQuantity

            If maxLimit > 0 AndAlso newTotal > maxLimit Then
                Return New Tuple(Of Boolean, String)(False, $"Total items would exceed the maximum limit of {maxLimit}. Current total: {currentTotal}, Adding: {additionalQuantity}, New total: {newTotal}. Please increase the 'Max Items' limit or reduce quantities.")
            End If
            Return New Tuple(Of Boolean, String)(True, "")
        End Function

        ' Check if current totals exceed limits (for save validation)
        Private Function ValidateAllLimits() As Tuple(Of Boolean, String)
            Dim resourcesTotal = GetTotalResourcesQuantity()
            Dim itemsTotal = GetTotalItemsQuantity()
            Dim maxResources = GetMaxResourcesLimit()
            Dim maxItems = GetMaxItemsLimit()

            Dim errors As New List(Of String)

            If maxResources > 0 AndAlso resourcesTotal > maxResources Then
                errors.Add($"Resources total ({resourcesTotal}) exceeds maximum limit ({maxResources}). Please increase 'Max Resources' or reduce quantities.")
            End If

            If maxItems > 0 AndAlso itemsTotal > maxItems Then
                errors.Add($"Items total ({itemsTotal}) exceeds maximum limit ({maxItems}). Please increase 'Max Items' or reduce quantities.")
            End If

            If errors.Count > 0 Then
                Return New Tuple(Of Boolean, String)(False, String.Join(vbCrLf & vbCrLf, errors))
            End If

            Return New Tuple(Of Boolean, String)(True, "")
        End Function

        ' Add Resource button click handler
        Private Sub btnGameStartAddResource_Click(sender As Object, e As RoutedEventArgs)
            If cmbGameStartAddResource.SelectedValue Is Nothing Then
                MessageBox.Show("Please select an item from the dropdown.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim elementId As Integer
            If Not Integer.TryParse(cmbGameStartAddResource.SelectedValue.ToString(), elementId) Then
                MessageBox.Show("Invalid item selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedItem = TryCast(cmbGameStartAddResource.SelectedItem, CategorizedItem)
            Dim itemName As String = If(selectedItem IsNot Nothing, selectedItem.ItemName, GetGameStartItemName(elementId))

            ' Check if item already exists
            Dim existingItem = gameStartResourceItems.FirstOrDefault(Function(r) r.ElementId = elementId)
            If existingItem IsNot Nothing Then
                Dim result = MessageBox.Show($"Item '{itemName}' already exists with quantity {existingItem.Quantity}. Do you want to update the quantity?",
                                       "Item Exists", MessageBoxButton.YesNo, MessageBoxImage.Question)
                If result = MessageBoxResult.Yes Then
                    Dim newQuantity As Integer
                    If Integer.TryParse(txtGameStartAddResourceQuantity.Text, newQuantity) AndAlso newQuantity > 0 Then
                        ' Calculate the difference in quantity
                        Dim quantityDifference = newQuantity - existingItem.Quantity
                        ' Validate the new total
                        Dim validation = ValidateResourcesLimit(quantityDifference)
                        If Not validation.Item1 Then
                            MessageBox.Show(validation.Item2, "Resource Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning)
                            Return
                        End If
                        existingItem.Quantity = newQuantity
                        MarkGameStartUnsavedChanges()
                    Else
                        MessageBox.Show("Please enter a valid quantity (positive integer).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                End If
                Return
            End If

            ' Add new item
            Dim quantity As Integer
            If Integer.TryParse(txtGameStartAddResourceQuantity.Text, quantity) AndAlso quantity > 0 Then
                ' Validate the limit before adding
                Dim validation = ValidateResourcesLimit(quantity)
                If Not validation.Item1 Then
                    MessageBox.Show(validation.Item2, "Resource Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                gameStartResourceItems.Add(New GameStartResourceItem With {
                .ElementId = elementId,
                .Name = itemName,
                .Quantity = quantity,
                .IsSelected = False
            })
                txtGameStartAddResourceQuantity.Text = ""
                cmbGameStartAddResource.SelectedValue = Nothing
                MarkGameStartUnsavedChanges()
            Else
                MessageBox.Show("Please enter a valid quantity (positive integer).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub

        ' Add Item button click handler
        Private Sub btnGameStartAddItem_Click(sender As Object, e As RoutedEventArgs)
            If cmbGameStartAddItem.SelectedValue Is Nothing Then
                MessageBox.Show("Please select an item from the dropdown.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim elementId As Integer
            If Not Integer.TryParse(cmbGameStartAddItem.SelectedValue.ToString(), elementId) Then
                MessageBox.Show("Invalid item selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim selectedItem = TryCast(cmbGameStartAddItem.SelectedItem, CategorizedItem)
            Dim itemName As String = If(selectedItem IsNot Nothing, selectedItem.ItemName, GetGameStartItemName(elementId))

            ' Check if item already exists
            Dim existingItem = gameStartItemItems.FirstOrDefault(Function(r) r.ElementId = elementId)
            If existingItem IsNot Nothing Then
                Dim result = MessageBox.Show($"Item '{itemName}' already exists with quantity {existingItem.Quantity}. Do you want to update the quantity?",
                                       "Item Exists", MessageBoxButton.YesNo, MessageBoxImage.Question)
                If result = MessageBoxResult.Yes Then
                    Dim newQuantity As Integer
                    If Integer.TryParse(txtGameStartAddItemQuantity.Text, newQuantity) AndAlso newQuantity > 0 Then
                        ' Calculate the difference in quantity
                        Dim quantityDifference = newQuantity - existingItem.Quantity
                        ' Validate the new total
                        Dim validation = ValidateItemsLimit(quantityDifference)
                        If Not validation.Item1 Then
                            MessageBox.Show(validation.Item2, "Item Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning)
                            Return
                        End If
                        existingItem.Quantity = newQuantity
                        MarkGameStartUnsavedChanges()
                    Else
                        MessageBox.Show("Please enter a valid quantity (positive integer).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                End If
                Return
            End If

            ' Add new item
            Dim quantity As Integer
            If Integer.TryParse(txtGameStartAddItemQuantity.Text, quantity) AndAlso quantity > 0 Then
                ' Validate the limit before adding
                Dim validation = ValidateItemsLimit(quantity)
                If Not validation.Item1 Then
                    MessageBox.Show(validation.Item2, "Item Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning)
                    Return
                End If

                gameStartItemItems.Add(New GameStartResourceItem With {
                .ElementId = elementId,
                .Name = itemName,
                .Quantity = quantity,
                .IsSelected = False
            })
                txtGameStartAddItemQuantity.Text = ""
                cmbGameStartAddItem.SelectedValue = Nothing
                MarkGameStartUnsavedChanges()
            Else
                MessageBox.Show("Please enter a valid quantity (positive integer).", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Sub

        ' Delete Selected Resources button click handler
        Private Sub btnGameStartDeleteSelectedResources_Click(sender As Object, e As RoutedEventArgs)
            ' Get all items where IsSelected is True
            Dim selectedItems = gameStartResourceItems.Where(Function(i) i.IsSelected = True).ToList()
            If selectedItems.Count = 0 Then
                MessageBox.Show("Please select one or more items to delete by checking their boxes.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim itemNames = String.Join(", ", selectedItems.Select(Function(i) i.Name))
            Dim result = MessageBox.Show($"Are you sure you want to delete {selectedItems.Count} selected item(s)?{vbCrLf}{itemNames}",
                                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question)
            If result = MessageBoxResult.Yes Then
                ' Remove items from the collection
                For Each item In selectedItems
                    gameStartResourceItems.Remove(item)
                Next
                MarkGameStartUnsavedChanges()
                UpdateGameStartSaveButtonState() ' Check if limits are now valid after deletion
            End If
        End Sub

        ' Delete Selected Items button click handler
        Private Sub btnGameStartDeleteSelectedItems_Click(sender As Object, e As RoutedEventArgs)
            ' Get all items where IsSelected is True
            Dim selectedItems = gameStartItemItems.Where(Function(i) i.IsSelected = True).ToList()
            If selectedItems.Count = 0 Then
                MessageBox.Show("Please select one or more items to delete by checking their boxes.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Dim itemNames = String.Join(", ", selectedItems.Select(Function(i) i.Name))
            Dim result = MessageBox.Show($"Are you sure you want to delete {selectedItems.Count} selected item(s)?{vbCrLf}{itemNames}",
                                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question)
            If result = MessageBoxResult.Yes Then
                ' Remove items from the collection
                For Each item In selectedItems
                    gameStartItemItems.Remove(item)
                Next
                MarkGameStartUnsavedChanges()
                UpdateGameStartSaveButtonState() ' Check if limits are now valid after deletion
            End If
        End Sub

        ' DataGrid selection changed handlers
        Private Sub dgGameStartResources_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Optional functionality
        End Sub

        Private Sub dgGameStartItems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            ' Optional functionality
        End Sub

        ' CheckBox event handlers to ensure IsSelected is updated
        Private Sub GameStartCheckBox_Checked(sender As Object, e As RoutedEventArgs)
            Dim checkBox = TryCast(sender, CheckBox)
            If checkBox IsNot Nothing Then
                Dim item = TryCast(checkBox.DataContext, GameStartResourceItem)
                If item IsNot Nothing Then
                    item.IsSelected = True
                End If
            End If
        End Sub

        Private Sub GameStartCheckBox_Unchecked(sender As Object, e As RoutedEventArgs)
            Dim checkBox = TryCast(sender, CheckBox)
            If checkBox IsNot Nothing Then
                Dim item = TryCast(checkBox.DataContext, GameStartResourceItem)
                If item IsNot Nothing Then
                    item.IsSelected = False
                End If
            End If
        End Sub

        ' Preview mouse down handlers to allow checkbox clicks without selecting rows
        Private Sub dgGameStartResources_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
            Dim dep = TryCast(e.OriginalSource, DependencyObject)
            If dep IsNot Nothing Then
                ' Walk up the visual tree to find if we clicked on a checkbox
                While dep IsNot Nothing
                    If TypeOf dep Is CheckBox Then
                        ' Let the checkbox handle the click - don't select the row or handle the event
                        e.Handled = False ' Don't prevent the checkbox from handling it
                        Return
                    End If
                    dep = VisualTreeHelper.GetParent(dep)
                End While
            End If
        End Sub

        Private Sub dgGameStartItems_PreviewMouseDown(sender As Object, e As MouseButtonEventArgs)
            Dim dep = TryCast(e.OriginalSource, DependencyObject)
            If dep IsNot Nothing Then
                ' Walk up the visual tree to find if we clicked on a checkbox
                While dep IsNot Nothing
                    If TypeOf dep Is CheckBox Then
                        ' Let the checkbox handle the click - don't select the row or handle the event
                        e.Handled = False ' Don't prevent the checkbox from handling it
                        Return
                    End If
                    dep = VisualTreeHelper.GetParent(dep)
                End While
            End If
        End Sub

        ' DataGrid cell edit ending handlers to track quantity changes
        Private Sub dgGameStartResources_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs)
            ' Validate quantity changes
            If e.Column.Header.ToString() = "Quantity" Then
                Dim item = TryCast(e.Row.Item, GameStartResourceItem)
                If item IsNot Nothing Then
                    Dim textBox = TryCast(e.EditingElement, TextBox)
                    If textBox IsNot Nothing Then
                        Dim newQuantity As Integer
                        If Integer.TryParse(textBox.Text, newQuantity) AndAlso newQuantity > 0 Then
                            ' Calculate the difference
                            Dim oldQuantity = item.Quantity
                            Dim quantityDifference = newQuantity - oldQuantity

                            ' Validate the new total
                            Dim validation = ValidateResourcesLimit(quantityDifference)
                            If Not validation.Item1 Then
                                MessageBox.Show(validation.Item2, "Resource Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning)
                                e.Cancel = True ' Cancel the edit
                                item.Quantity = oldQuantity ' Revert to old value
                                Return
                            End If
                        End If
                    End If
                End If
            End If
            MarkGameStartUnsavedChanges()
            UpdateGameStartSaveButtonState() ' Update save button state after edit
        End Sub

        Private Sub dgGameStartItems_CellEditEnding(sender As Object, e As DataGridCellEditEndingEventArgs)
            ' Validate quantity changes
            If e.Column.Header.ToString() = "Quantity" Then
                Dim item = TryCast(e.Row.Item, GameStartResourceItem)
                If item IsNot Nothing Then
                    Dim textBox = TryCast(e.EditingElement, TextBox)
                    If textBox IsNot Nothing Then
                        Dim newQuantity As Integer
                        If Integer.TryParse(textBox.Text, newQuantity) AndAlso newQuantity > 0 Then
                            ' Calculate the difference
                            Dim oldQuantity = item.Quantity
                            Dim quantityDifference = newQuantity - oldQuantity

                            ' Validate the new total
                            Dim validation = ValidateItemsLimit(quantityDifference)
                            If Not validation.Item1 Then
                                MessageBox.Show(validation.Item2, "Item Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Warning)
                                e.Cancel = True ' Cancel the edit
                                item.Quantity = oldQuantity ' Revert to old value
                                Return
                            End If
                        End If
                    End If
                End If
            End If
            MarkGameStartUnsavedChanges()
            UpdateGameStartSaveButtonState() ' Update save button state after edit
        End Sub

        ' Checkbox change handlers for settings
        Private Sub chkGameStartEnemiesEnabled_Checked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartEnemiesEnabled_Unchecked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartFriendsEnabled_Checked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartFriendsEnabled_Unchecked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartLoversEnabled_Checked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartLoversEnabled_Unchecked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartSandbox_Checked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartSandbox_Unchecked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartHangarsPopulated_Checked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub chkGameStartHangarsPopulated_Unchecked(sender As Object, e As RoutedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        ' TextBox change handlers for settings
        Private Sub txtGameStartCrew_TextChanged(sender As Object, e As TextChangedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub txtGameStartMaxCrew_TextChanged(sender As Object, e As TextChangedEventArgs)
            If gameStartSettingsCard.IsEnabled Then MarkGameStartUnsavedChanges()
        End Sub

        Private Sub txtGameStartMaxResources_TextChanged(sender As Object, e As TextChangedEventArgs)
            If gameStartSettingsCard.IsEnabled Then
                MarkGameStartUnsavedChanges()
                ' Re-enable save button if limits are now valid
                UpdateGameStartSaveButtonState()
            End If
        End Sub

        Private Sub txtGameStartMaxItems_TextChanged(sender As Object, e As TextChangedEventArgs)
            If gameStartSettingsCard.IsEnabled Then
                MarkGameStartUnsavedChanges()
                ' Re-enable save button if limits are now valid
                UpdateGameStartSaveButtonState()
            End If
        End Sub

        ' Update save button state based on limit validation
        Private Sub UpdateGameStartSaveButtonState()
            If btnGameStartSaveChanges IsNot Nothing Then
                Dim validation = ValidateAllLimits()
                btnGameStartSaveChanges.IsEnabled = validation.Item1
            End If
        End Sub

        ' Mark unsaved changes and show banner
        Private Sub MarkGameStartUnsavedChanges()
            gameStartHasUnsavedChanges = True
            Try
                If txtGameStartUnsavedIndicator IsNot Nothing Then
                    txtGameStartUnsavedIndicator.Visibility = Visibility.Visible
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error showing unsaved indicator: {ex.Message}")
            End Try
        End Sub

        ' Clear unsaved changes flag and hide banner
        Private Sub ClearGameStartUnsavedChanges()
            gameStartHasUnsavedChanges = False
            Try
                If txtGameStartUnsavedIndicator IsNot Nothing Then
                    txtGameStartUnsavedIndicator.Visibility = Visibility.Collapsed
                End If
            Catch ex As Exception
                Debug.WriteLine($"Error hiding unsaved indicator: {ex.Message}")
            End Try
        End Sub

        ' Save Changes button click handler
        Private Sub btnGameStartSaveChanges_Click(sender As Object, e As RoutedEventArgs)
            If gameStartXmlDoc Is Nothing OrElse String.IsNullOrEmpty(gameStartFilePath) Then
                MessageBox.Show("No file loaded to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' Validate limits before saving
            Dim validation = ValidateAllLimits()
            If Not validation.Item1 Then
                MessageBox.Show($"Cannot save: Limits exceeded!{vbCrLf}{vbCrLf}{validation.Item2}{vbCrLf}{vbCrLf}Please increase the limits or reduce quantities before saving.",
                                "Cannot Save", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            Try
                ' Game Start files have a <diff> root element, then <playerShips>
                Dim diffElement As XElement = Nothing
                If gameStartXmlDoc.Root IsNot Nothing Then
                    If gameStartXmlDoc.Root.Name = "diff" Then
                        diffElement = gameStartXmlDoc.Root
                    Else
                        diffElement = gameStartXmlDoc.Root.Element("diff")
                        If diffElement Is Nothing Then
                            diffElement = gameStartXmlDoc.Root
                        End If
                    End If
                End If

                ' Find the playerShips element
                Dim playerShipsElement = diffElement?.Element("playerShips")
                If playerShipsElement Is Nothing Then
                    MessageBox.Show("Invalid XML structure: 'playerShips' element not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Get the first ship (l element)
                Dim shipElement = playerShipsElement.Element("l")
                If shipElement Is Nothing Then
                    MessageBox.Show("Invalid XML structure: No ship found in 'playerShips'.", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    Return
                End If

                ' Save settings to diff element
                If diffElement IsNot Nothing Then
                    diffElement.SetAttributeValue("enemiesEnabled", chkGameStartEnemiesEnabled.IsChecked.ToString().ToLower())
                    diffElement.SetAttributeValue("friendsEnabled", chkGameStartFriendsEnabled.IsChecked.ToString().ToLower())
                    diffElement.SetAttributeValue("loversEnabled", chkGameStartLoversEnabled.IsChecked.ToString().ToLower())
                    diffElement.SetAttributeValue("sandbox", chkGameStartSandbox.IsChecked.ToString().ToLower())
                End If

                ' Save crew and resource settings to ship element
                shipElement.SetAttributeValue("crew", txtGameStartCrew.Text)
                shipElement.SetAttributeValue("maxCrew", txtGameStartMaxCrew.Text)
                shipElement.SetAttributeValue("maxResources", txtGameStartMaxResources.Text)
                shipElement.SetAttributeValue("maxItems", txtGameStartMaxItems.Text)
                shipElement.SetAttributeValue("hangarsPopulated", chkGameStartHangarsPopulated.IsChecked.ToString().ToLower())

                ' Clear and save resources
                Dim resourcesElement = shipElement.Element("resources")
                If resourcesElement IsNot Nothing Then
                    resourcesElement.RemoveAll()
                Else
                    resourcesElement = New XElement("resources")
                    shipElement.Add(resourcesElement)
                End If

                For Each resourceItem In gameStartResourceItems
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

                For Each itemItem In gameStartItemItems
                    Dim itemNode = New XElement("l",
                    New XAttribute("elementId", itemItem.ElementId.ToString()),
                    New XAttribute("howMuch", itemItem.Quantity.ToString()))
                    itemsElement.Add(itemNode)
                Next

                ' Save the XML file
                gameStartXmlDoc.Save(gameStartFilePath)
                ClearGameStartUnsavedChanges() ' Reset unsaved changes flag after successful save
                MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information)

            Catch ex As Exception
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
        End Sub

#End Region

    End Class

End Namespace