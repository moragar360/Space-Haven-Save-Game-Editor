Imports System.IO
Imports System.Xml.Linq
Imports Microsoft.Win32 ' For File Dialogs
Imports System.Windows.Input ' For MouseButtonEventArgs
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows ' Required for MessageBoxButton, MessageBoxImage etc.
Imports System.Windows.Threading ' Required for Dispatcher
Imports System.Diagnostics ' Required for Debug.WriteLine

' *** Make sure this Namespace matches your project's root namespace ***
Namespace SpaceHavenEditor2

    Public Class BlueprintManagerWindow
        Inherits Window

        ' Class-level variables to store paths and loaded data
        Private sourceSavePath As String = String.Empty
        Private blueprintFilePath As String = String.Empty
        Private targetSavePath As String = String.Empty

        Private sourceXmlDoc As XDocument = Nothing
        Private targetXmlDoc As XDocument = Nothing ' Only load when needed for import
        Private blueprintNode As XElement = Nothing ' Loaded blueprint ship node

        ' Simple class to hold ship info for the ComboBox
        Public Class ShipInfo
            Public Property Sid As Integer
            Public Property Sname As String
            Public Overrides Function ToString() As String
                Return Sname ' Default display
            End Function
        End Class

        Public Sub New()
            InitializeComponent() ' This links XAML controls to this code
            UpdateStatus("Ready. Select source save file to begin export.")
        End Sub

#Region "Window Management"

        ' Allows dragging the borderless window via the title bar area
        Private Sub TitleBar_MouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs)
            If e.ButtonState = MouseButtonState.Pressed Then
                Me.DragMove()
            End If
        End Sub

        ' Handles the custom close button
        Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
            Me.Close()
        End Sub

#End Region

#Region "File Selection Handlers"

        ' Helper function to get default save directory from settings
        Private Function GetDefaultSaveDirectory() As String
            Try
                Dim defaultDir As String = My.Settings.DefaultSaveDirectory
                If Not String.IsNullOrEmpty(defaultDir) AndAlso Directory.Exists(defaultDir) Then
                    Return defaultDir
                End If
            Catch
                ' If settings fail, continue to fallback
            End Try
            ' Fallback to default Space Haven directory
            Dim localAppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            Dim spaceHavenPath As String = Path.Combine(localAppData, "Low", "Bugbyte Oy", "Space Haven", "Saves")
            If Directory.Exists(spaceHavenPath) Then
                Return spaceHavenPath
            End If
            Return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        End Function

        ' Handles selecting the source save file for export
        Private Sub btnSelectSourceSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSelectSourceSave.Click
            Debug.WriteLine(">>> btnSelectSourceSave_Click Entered") ' DEBUG MESSAGE
            Dim ofd As New OpenFileDialog With {
                .Title = "Select Source Space Haven Save File ('game')",
                .Filter = "Space Haven Save|game;*.sav|All Files (*.*)|*.*", ' Allow game or .sav
                .CheckFileExists = True,
                .InitialDirectory = GetDefaultSaveDirectory()
            }
            Dim dlg = ofd.ShowDialog()
            If dlg <> True Then Return
            sourceSavePath = ofd.FileName
                ' Update the UI to show the selected path
                txtSourceSavePath.Text = sourceSavePath
                txtSourceSavePath.ToolTip = sourceSavePath ' Show full path on hover
                UpdateStatus("Source save selected. Loading ships...")
                LoadShipsFromSource() ' Load ships into the ComboBox
                CheckExportButtonState() ' Update button enabled state

        End Sub

        ' Handles selecting the blueprint XML file for import
        Private Sub btnSelectBlueprintFile_Click(sender As Object, e As RoutedEventArgs) Handles btnSelectBlueprintFile.Click
            Dim ofd As New OpenFileDialog With {
                .Title = "Select Ship Blueprint File",
                .Filter = "Ship Blueprint XML (*.xml)|*.xml|All Files (*.*)|*.*",
                .CheckFileExists = True,
                .InitialDirectory = GetDefaultSaveDirectory()
            }
            Dim dlg = ofd.ShowDialog()
            If dlg <> True Then Return
            blueprintFilePath = ofd.FileName
                ' Update the UI
                txtBlueprintFilePath.Text = blueprintFilePath
                txtBlueprintFilePath.ToolTip = blueprintFilePath
                UpdateStatus("Blueprint file selected.")
                CheckImportButtonState() ' Update button enabled state

        End Sub

        ' Handles selecting the target save file for import
        Private Sub btnSelectTargetSave_Click(sender As Object, e As RoutedEventArgs) Handles btnSelectTargetSave.Click
            Dim ofd As New OpenFileDialog With {
               .Title = "Select Target Space Haven Save File ('game')",
               .Filter = "Space Haven Save|game;*.sav|All Files (*.*)|*.*", ' Allow game or .sav
               .CheckFileExists = True,
               .InitialDirectory = GetDefaultSaveDirectory()
           }
            Dim dlg = ofd.ShowDialog()
            If dlg <> True Then Return
            targetSavePath = ofd.FileName
                ' Update the UI
                txtTargetSavePath.Text = targetSavePath ' Corrected target control name
                txtTargetSavePath.ToolTip = targetSavePath
                UpdateStatus("Target save file selected.")
                CheckImportButtonState() ' Update button enabled state

        End Sub

#End Region

#Region "Export Logic"

        ' Loads ship names and IDs from the selected source save file
        Private Sub LoadShipsFromSource()
            If String.IsNullOrEmpty(sourceSavePath) Then
                Debug.WriteLine("LoadShipsFromSource: sourceSavePath is empty, exiting.")
                Return
            End If

            cmbShipsToExport.ItemsSource = Nothing ' Clear previous items
            cmbShipsToExport.IsEnabled = False
            sourceXmlDoc = Nothing ' Clear previous doc
            Dim shipsFound As Integer = 0
            Dim shipsAdded As Integer = 0

            Try
                Debug.WriteLine($"LoadShipsFromSource: Attempting to load XML from: {sourceSavePath}")
                sourceXmlDoc = XDocument.Load(sourceSavePath)
                Debug.WriteLine("LoadShipsFromSource: XDocument.Load succeeded.")

                ' Basic validation of the loaded file
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
                    UpdateStatus($"Ships loaded ({shipsAdded} found). Select ship and click Export.")
                Else
                    UpdateStatus($"No valid ships found in source save file ({shipsFound} elements checked).")
                End If
                CheckExportButtonState()

            Catch ex As Exception
                MessageBox.Show($"Error loading ships from source save:{Environment.NewLine}{ex.ToString()}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error)
                sourceXmlDoc = Nothing
                cmbShipsToExport.ItemsSource = Nothing
                cmbShipsToExport.IsEnabled = False
                CheckExportButtonState()
                UpdateStatus("Error loading source save.")
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

            ' Check which export mode radio button is selected
            Dim exportAsBlueprint As Boolean = chkExportAsBlueprint.IsChecked.GetValueOrDefault(True)

            Try
                UpdateStatus($"Exporting blueprint for '{selectedShipInfo.Sname}'...")
                Dim originalShipElement = sourceXmlDoc.Root.Descendants("ship").FirstOrDefault(Function(s) s.Attribute("sid")?.Value = selectedShipInfo.Sid.ToString())
                If originalShipElement Is Nothing Then Throw New Exception($"Ship node with SID {selectedShipInfo.Sid} not found in source XML.")

                Dim blueprintNodeToSave As New XElement(originalShipElement)

                ' Always remove crew data on export
                blueprintNodeToSave.Element("characters")?.Remove()
                
                ' Keep inventory data in export - import will handle removing it if needed
                
                ' Set 'real' attribute based on export mode
                If exportAsBlueprint Then
                    ' Export as blueprint (unbuilt) - set real="0"
                    blueprintNodeToSave.SetAttributeValue("real", "0")
                Else
                    ' Export as built ship - preserve the original real attribute value
                    ' If it doesn't exist, set it to "1" (built)
                    If blueprintNodeToSave.Attribute("real") Is Nothing Then
                        blueprintNodeToSave.SetAttributeValue("real", "1")
                    End If
                End If

                Dim sfd As New SaveFileDialog() With {
                    .Title = "Save Ship Blueprint As",
                    .Filter = "Ship Blueprint XML (*.xml)|*.xml|All Files (*.*)|*.*",
                    .FileName = $"{selectedShipInfo.Sname}_Blueprint.xml",
                    .InitialDirectory = GetDefaultSaveDirectory()
                }

                If sfd.ShowDialog() = True Then
                    blueprintNodeToSave.Save(sfd.FileName)
                    MessageBox.Show($"Blueprint '{Path.GetFileName(sfd.FileName)}' saved successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information)
                    UpdateStatus($"Blueprint '{Path.GetFileName(sfd.FileName)}' saved.")
                Else
                    UpdateStatus("Export cancelled.")
                End If
            Catch ex As Exception
                MessageBox.Show($"Error exporting blueprint:{Environment.NewLine}{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error)
                UpdateStatus("Export failed.")
            End Try
        End Sub
#End Region

#Region "Import Logic"

        Private Sub CheckImportButtonState()
            btnImportBlueprint.IsEnabled = (Not String.IsNullOrEmpty(blueprintFilePath) AndAlso File.Exists(blueprintFilePath) AndAlso
                                            Not String.IsNullOrEmpty(targetSavePath) AndAlso File.Exists(targetSavePath))
        End Sub

        Private Sub btnImportBlueprint_Click(sender As Object, e As RoutedEventArgs) Handles btnImportBlueprint.Click
            ' --- 0) Validate inputs ---
            If String.IsNullOrEmpty(blueprintFilePath) OrElse Not File.Exists(blueprintFilePath) Then
                MessageBox.Show("Please select a valid blueprint XML file.", "Blueprint Missing", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If
            If String.IsNullOrEmpty(targetSavePath) OrElse Not File.Exists(targetSavePath) Then
                MessageBox.Show("Please select a valid target save 'game' file.", "Target Save Missing", MessageBoxButton.OK, MessageBoxImage.Warning)
                Return
            End If

            ' --- 1) Confirm --- 
            Dim result = MessageBox.Show(
        $"Import '{Path.GetFileName(blueprintFilePath)}' into '{Path.GetFileName(targetSavePath)}'?" & vbCrLf &
        "BACKUP YOUR SAVE FIRST! Do not have the game loaded." & vbCrLf & vbCrLf &
        "Proceed?",
        "Confirm Import", MessageBoxButton.YesNo, MessageBoxImage.Warning
    )
            If result = MessageBoxResult.No Then
                UpdateStatus("Import cancelled.")
                Return
            End If

            Me.Cursor = Cursors.Wait
            Try
                ' --- 2) Load docs ---
                UpdateStatus("Loading target save…")
                targetXmlDoc = XDocument.Load(targetSavePath)
                If targetXmlDoc.Root Is Nothing Then Throw New Exception("Target save file has no root element.")
                If targetXmlDoc.Root.Name <> "game" Then Throw New Exception("Invalid save file - root element is not <game>.")

                UpdateStatus("Loading blueprint…")
                Dim bpDoc = XDocument.Load(blueprintFilePath)
                Dim loadedShip = bpDoc.Descendants("ship").FirstOrDefault()
                If loadedShip Is Nothing Then Throw New Exception("Blueprint has no <ship>.")
                blueprintNode = New XElement(loadedShip)  ' clone

                ' --- 2a) If importing as blueprint, remove crew, inventory, and built structures (but keep planning grid) ---
                If chkImportAsBlueprint.IsChecked = True Then
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
                    UpdateStatus($"Initialized missing 'idCounter' attribute to {idCtr} based on existing ships.")
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
                If chkImportAsBlueprint.IsChecked = True Then
                    blueprintNode.SetAttributeValue("real", "0")
                    blueprintNode.SetAttributeValue("idCnt", "0") ' No built structures in blueprint
                Else
                    blueprintNode.SetAttributeValue("real", "1")
                    ' Preserve idCnt if it exists, otherwise set to 0
                    If blueprintNode.Attribute("idCnt") Is Nothing Then
                        blueprintNode.SetAttributeValue("idCnt", "0")
                    End If
                End If

                ' --- 4) Insert under <ships> so the game loads it on startup ---
                Dim shipsEl = gameRoot.Element("ships")
                If shipsEl Is Nothing Then
                    shipsEl = New XElement("ships")
                    gameRoot.Add(shipsEl)
                End If
                shipsEl.Add(blueprintNode)

                ' --- 5) Insert under <blueprints> for future imports ---
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
                    UpdateStatus($"Initialized missing 'objectIdCounter' attribute to {objCtr}.")
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
                            UpdateStatus($"Initialized missing 'playerSectorId' attribute to {psid} from first available system.")
                        Else
                            ' No valid system found, use default
                            psid = 0
                            gamedataEl.SetAttributeValue("playerSectorId", "0")
                            UpdateStatus("Initialized missing 'playerSectorId' attribute to 0 (default).")
                        End If
                    Else
                        ' No systems found, use default
                        psid = 0
                        gamedataEl.SetAttributeValue("playerSectorId", "0")
                        UpdateStatus("Initialized missing 'playerSectorId' attribute to 0 (default - no systems found).")
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

                ' Now find or create the <f isPlayer="true"> inside it
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

                ' --- 9) Build & add the instance <l> with slid→blueprint SID ---
                UpdateStatus("Placing imported ship…")
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
            New XAttribute("crew", If(chkImportAsBlueprint.IsChecked, "0", "0")),
            New XAttribute("cryoCrew", If(chkImportAsBlueprint.IsChecked, "0", "0")),
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

                        ' --- 10) Backup & Save ---
                        Try
                            Dim bak = targetSavePath & ".bak"
                            File.Copy(targetSavePath, bak, True)
                            UpdateStatus($"Backup created: {Path.GetFileName(bak)}")
                        Catch ex As Exception
                            UpdateStatus($"Warning: backup failed – {ex.Message}")
                        End Try

                        targetXmlDoc.Save(targetSavePath)
                        UpdateStatus($"Import complete: SID {newBlueprintSid}, Instance #{newInstanceId}.")
                        MessageBox.Show("Ship imported successfully!", "Done", MessageBoxButton.OK, MessageBoxImage.Information)

    Catch ex As Exception
                MessageBox.Show($"Import Error:{vbCrLf}{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                UpdateStatus("Import failed.")
            Finally
                Me.Cursor = Cursors.Arrow
            End Try
        End Sub




        Private Sub RemapEntityIDsRecursive(element As XElement, ByRef currentIdCounter As Long, idMap As Dictionary(Of String, String))
            Dim entIdAttr = element.Attribute("entId")
            If entIdAttr IsNot Nothing Then
                Dim oldId = entIdAttr.Value
                currentIdCounter += 1
                Dim newId = currentIdCounter.ToString()
                entIdAttr.Value = newId
                If Not String.IsNullOrEmpty(oldId) AndAlso Not idMap.ContainsKey(oldId) Then idMap.Add(oldId, newId)
            End If
            Dim attributesToRemap = {"bedLink", "targetId", "owner", "link", "doorLink", "controllerId"}
            For Each attrName In attributesToRemap
                Dim refAttr = element.Attribute(attrName)
                If refAttr IsNot Nothing AndAlso idMap.ContainsKey(refAttr.Value) Then refAttr.Value = idMap(refAttr.Value)
            Next
            For Each child In element.Elements() : RemapEntityIDsRecursive(child, currentIdCounter, idMap) : Next
        End Sub
#End Region

#Region "Utility"
        Private Sub UpdateStatus(message As String)
            Me.Dispatcher.Invoke(Sub()
                                     txtStatus.Text = message
                                 End Sub,
                                 DispatcherPriority.Background)
        End Sub



#End Region

    End Class ' End of BlueprintManagerWindow Class
End Namespace ' *** Make sure this matches your project's root namespace ***

