Imports System.IO
Imports System.Xml.Linq

Public Module BlueprintConverter
    Private Const PlannedMaterialId As String = "-2"
    Private ReadOnly StructuralMaterialIds As New HashSet(Of String)(StringComparer.Ordinal) From {
        "25", "31", "38", "43", "44", "46", "47", "48", "115", "122",
        "206", "423", "424", "425", "426", "428", "438", "905", "1144",
        "1149", "1711", "1713", "1794", "2755", "2757", "2758", "2759",
        "2760", "2762", "2763", "2764", "2765", "2767", "2768", "2769",
        "2770", "2771", "2772", "2861", "2862", "2863", "2864", "2866",
        "3029", "3031"
    }

    Public Function CreateUnbuiltBlueprint(sourceShip As XElement) As XElement
        If sourceShip Is Nothing Then Throw New ArgumentNullException(NameOf(sourceShip))

        Dim blueprint = New XElement(sourceShip)

        For Each containerName In {"characters", "robots", "monsters", "items",
                                   "nonStorables", "blueprints"}
            Dim container = blueprint.Element(containerName)
            If container Is Nothing Then
                blueprint.Add(New XElement(containerName))
            Else
                container.RemoveNodes()
            End If
        Next

        blueprint.SetAttributeValue("crew", "0")
        blueprint.SetAttributeValue("cryoCrew", "0")
        blueprint.SetAttributeValue("real", "0")
        blueprint.SetAttributeValue("idCnt", "0")
        blueprint.Element("gasWarnings")?.Remove()
        ResetRuntimeState(blueprint)

        Dim roof = blueprint.Element("roof")
        Dim originalWidth = ParsePositiveDimension(blueprint.Attribute("sx"), "sx")
        Dim originalHeight = ParsePositiveDimension(blueprint.Attribute("sy"), "sy")

        ' Preserve a deliberately resized roof canvas. When it is larger than the
        ' current built grid, anchor the converted design at the far/top edge.
        Dim allCoordinateCells = blueprint.Elements("e").
            Concat(If(roof Is Nothing, Enumerable.Empty(Of XElement)(), roof.Elements("e"))).
            ToList()
        Dim occupiedWidth = GetOccupiedDimension(allCoordinateCells, "x")
        Dim occupiedHeight = GetOccupiedDimension(allCoordinateCells, "y")
        Dim storedRoofWidth = ParseOptionalPositiveDimension(roof?.Attribute("sx"))
        Dim storedRoofHeight = ParseOptionalPositiveDimension(roof?.Attribute("sy"))
        Dim width = Math.Max(Math.Max(originalWidth, storedRoofWidth), occupiedWidth)
        Dim height = Math.Max(Math.Max(originalHeight, storedRoofHeight), occupiedHeight)
        Dim roofCells = If(roof Is Nothing,
                           Enumerable.Empty(Of XElement)(),
                           roof.Elements("e"))
        Dim designWidth = GetOccupiedDimension(roofCells, "x")
        Dim designHeight = GetOccupiedDimension(roofCells, "y")
        If designWidth <= 0 Then designWidth = occupiedWidth
        If designHeight <= 0 Then designHeight = occupiedHeight
        Dim coordinateOffsetX = Math.Max(0, width - designWidth)
        Dim coordinateOffsetY = Math.Max(0, height - designHeight)
        blueprint.SetAttributeValue("sx", width)
        blueprint.SetAttributeValue("sy", height)
        If roof IsNot Nothing Then
            roof.SetAttributeValue("sx", width)
            roof.SetAttributeValue("sy", height)
            roof.SetAttributeValue("shiftX", "0")
            roof.SetAttributeValue("shiftY", "0")
        End If

        ' Preserve already-converted planned structure so export followed by import
        ' is idempotent. Convert built hull, wall, and door variants generically.
        Dim sourceStructuralCells = blueprint.Elements("e").
            Where(Function(cell)
                      Dim material = cell.Attribute("m")?.Value
                      Dim isExistingPlan = material = PlannedMaterialId AndAlso
                                           cell.Attribute("hd")?.Value = "4" AndAlso
                                           cell.Attribute("sh")?.Value = "0"
                      Dim isKnownStructure = StructuralMaterialIds.Contains(material)
                      Dim isLeafStructure = material <> PlannedMaterialId AndAlso
                                            cell.Attribute("sh")?.Value = "32" AndAlso
                                            Not cell.Elements().Any()
                      Return isExistingPlan OrElse isKnownStructure OrElse isLeafStructure
                  End Function).
            ToList()

        ' Ship-level planned cells also include the canvas perimeter. Other m=-2
        ' masks belong to the source sector and must not be copied to another save.
        Dim plannedShipCells As New List(Of XElement)
        Dim occupiedCoordinates As New HashSet(Of String)(StringComparer.Ordinal)
        For x = 0 To width - 1
            plannedShipCells.Add(CreatePerimeterCell(x, 0))
            occupiedCoordinates.Add($"{x},0")
            If height > 1 Then plannedShipCells.Add(CreatePerimeterCell(x, height - 1))
            If height > 1 Then occupiedCoordinates.Add($"{x},{height - 1}")
        Next
        For y = 1 To height - 2
            plannedShipCells.Add(CreatePerimeterCell(0, y))
            occupiedCoordinates.Add($"0,{y}")
            If width > 1 Then plannedShipCells.Add(CreatePerimeterCell(width - 1, y))
            If width > 1 Then occupiedCoordinates.Add($"{width - 1},{y}")
        Next

        For Each sourceCell In sourceStructuralCells
            Dim x As Integer
            Dim y As Integer
            If Not Integer.TryParse(sourceCell.Attribute("x")?.Value, x) OrElse
               Not Integer.TryParse(sourceCell.Attribute("y")?.Value, y) Then
                Continue For
            End If
            x += coordinateOffsetX
            y += coordinateOffsetY
            If x < 0 OrElse x >= width OrElse y < 0 OrElse y >= height Then Continue For

            If occupiedCoordinates.Add($"{x},{y}") Then
                plannedShipCells.Add(CreatePlannedStructureCell(x, y))
            End If
        Next

        blueprint.Elements("e").Remove()
        blueprint.AddFirst(plannedShipCells)

        If roof IsNot Nothing Then
            Dim fallbackColor = roof.Elements("e").
                Select(Function(cell) cell.Attribute("col")?.Value).
                FirstOrDefault(Function(color) Not String.IsNullOrWhiteSpace(color))
            If String.IsNullOrWhiteSpace(fallbackColor) Then fallbackColor = "6e75ff"

            Dim plannedRoofCells = roof.Elements("e").
                Select(Function(cell)
                           Dim x As Integer
                           Dim y As Integer
                           If Not Integer.TryParse(cell.Attribute("x")?.Value, x) OrElse
                              Not Integer.TryParse(cell.Attribute("y")?.Value, y) Then
                               Return Nothing
                           End If
                           x += coordinateOffsetX
                           y += coordinateOffsetY
                           If x < 0 OrElse x >= width OrElse y < 0 OrElse y >= height Then
                               Return Nothing
                           End If
                           Return New XElement("e",
                               New XAttribute("m", PlannedMaterialId),
                               New XAttribute("x", x),
                               New XAttribute("y", y),
                               New XAttribute("col", If(cell.Attribute("col")?.Value, fallbackColor)))
                       End Function).
                Where(Function(cell) cell IsNot Nothing).
                ToList()

            roof.Elements("e").Remove()
            roof.Add(plannedRoofCells)
            roof.SetAttributeValue("idCnt", "0")
        End If

        Return blueprint
    End Function

    Private Function ParsePositiveDimension(attribute As XAttribute, name As String) As Integer
        Dim value As Integer
        If attribute Is Nothing OrElse
           Not Integer.TryParse(attribute.Value, value) OrElse value <= 0 Then
            Throw New InvalidDataException($"Ship blueprint has an invalid {name} dimension.")
        End If
        Return value
    End Function

    Private Function GetOccupiedDimension(cells As IEnumerable(Of XElement),
                                          coordinateName As String) As Integer
        Dim maximum = -1
        For Each cell In cells
            Dim coordinate As Integer
            If Integer.TryParse(cell.Attribute(coordinateName)?.Value, coordinate) Then
                maximum = Math.Max(maximum, coordinate)
            End If
        Next
        Return maximum + 1
    End Function

    Private Function ParseOptionalPositiveDimension(attribute As XAttribute) As Integer
        Dim value As Integer
        If attribute IsNot Nothing AndAlso
           Integer.TryParse(attribute.Value, value) AndAlso value > 0 Then
            Return value
        End If
        Return 0
    End Function

    Private Sub ResetRuntimeState(blueprint As XElement)
        Dim colorTiles = blueprint.Element("colorTiles")
        If colorTiles Is Nothing Then
            blueprint.Add(New XElement("colorTiles"))
        Else
            colorTiles.RemoveNodes()
        End If

        Dim manager = blueprint.Element("manager")
        Dim cleanManager = New XElement("manager",
            New XAttribute("maint", If(manager?.Attribute("maint")?.Value, "Auto")),
            New XElement("un"),
            New XElement("und"),
            New XElement("tm",
                New XAttribute("autoReqRes", "true"),
                New XElement("transporting"),
                New XElement("allToDstShip"),
                New XElement("resRules")),
            New XElement("g"),
            New XElement("ritems"))
        If manager Is Nothing Then
            blueprint.Add(cleanManager)
        Else
            manager.ReplaceWith(cleanManager)
        End If

        Dim settings = blueprint.Element("settings")
        If settings IsNot Nothing Then
            settings.Attribute("asysm")?.Remove()
            For Each flag In {"sstbls", "nss", "tss", "sss", "css", "fss"}
                settings.SetAttributeValue(flag, "0")
            Next
        End If
    End Sub

    Private Function CreatePerimeterCell(x As Integer, y As Integer) As XElement
        Return New XElement("e",
            New XAttribute("m", PlannedMaterialId),
            New XAttribute("x", x),
            New XAttribute("y", y),
            New XAttribute("sh", "61504"),
            New XAttribute("fg", "255"))
    End Function

    Private Function CreatePlannedStructureCell(x As Integer, y As Integer) As XElement
        Return New XElement("e",
            New XAttribute("m", PlannedMaterialId),
            New XAttribute("x", x),
            New XAttribute("y", y),
            New XAttribute("hd", "4"),
            New XAttribute("sh", "0"),
            New XAttribute("fg", "255"))
    End Function
End Module
