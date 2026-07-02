Imports System.Xml.Linq

Public Class SaveIntegrityScanner
    Public Function Scan(document As XDocument) As List(Of SaveIntegrityFinding)
        Dim findings As New List(Of SaveIntegrityFinding)
        Dim root = document?.Root
        If root Is Nothing OrElse root.Name <> "game" Then Return findings

        Dim ships = root.Element("ships")?.Elements("ship").ToList()
        If ships Is Nothing Then ships = New List(Of XElement)
        Dim validShipIds = ScanPrimaryIds(ships, "sid", "Ship", findings)

        Dim crew = ships.Elements("characters").Elements("c").ToList()
        Dim validCrewIds = ScanPrimaryIds(crew, "entId", "Crew", findings)
        Dim validRelationshipTargetIds As New HashSet(Of Long)(validCrewIds)
        For Each characterLikeNode In root.Descendants("c").
            Where(Function(node) node.Element("pers") IsNot Nothing)
            Dim entityId As Long
            If Long.TryParse(characterLikeNode.Attribute("entId")?.Value, entityId) AndAlso entityId > 0 Then
                validRelationshipTargetIds.Add(entityId)
            End If
        Next

        ScanRelationships(crew, validRelationshipTargetIds, findings)
        ScanSchedules(root, crew, findings)
        ScanShipReferences(root, validShipIds, findings)
        ScanCounters(root, findings)
        Return findings
    End Function

    Private Function ScanPrimaryIds(elements As List(Of XElement),
                                    attributeName As String,
                                    category As String,
                                    findings As List(Of SaveIntegrityFinding)) As HashSet(Of Long)
        Dim validIds As New HashSet(Of Long)
        Dim grouped As New Dictionary(Of Long, List(Of XElement))

        For Each element In elements
            Dim rawValue = element.Attribute(attributeName)?.Value
            Dim parsedValue As Long
            Dim displayName = If(element.Attribute("name")?.Value,
                                 If(element.Attribute("sname")?.Value, element.Name.LocalName))
            If String.IsNullOrWhiteSpace(rawValue) OrElse
               Not Long.TryParse(rawValue, parsedValue) OrElse parsedValue <= 0 Then
                findings.Add(New SaveIntegrityFinding With {
                    .Severity = SaveIntegritySeverity.Critical,
                    .Category = $"{category} IDs",
                    .AffectedObject = displayName,
                    .Description = $"{category} has a missing, zero, or non-numeric {attributeName}.",
                    .RepairKind = SaveIntegrityRepairKind.None
                })
                Continue For
            End If

            validIds.Add(parsedValue)
            If Not grouped.ContainsKey(parsedValue) Then grouped(parsedValue) = New List(Of XElement)
            grouped(parsedValue).Add(element)
        Next

        For Each duplicate In grouped.Where(Function(pair) pair.Value.Count > 1)
            findings.Add(New SaveIntegrityFinding With {
                .Severity = SaveIntegritySeverity.Critical,
                .Category = $"{category} IDs",
                .AffectedObject = $"{attributeName} {duplicate.Key}",
                .Description = $"{duplicate.Value.Count} {category.ToLowerInvariant()} records share the same primary ID.",
                .RepairKind = SaveIntegrityRepairKind.None
            })
        Next
        Return validIds
    End Function

    Private Sub ScanRelationships(crew As List(Of XElement),
                                  validCrewIds As HashSet(Of Long),
                                  findings As List(Of SaveIntegrityFinding))
        For Each source In crew
            Dim sourceId As Long
            If Not Long.TryParse(source.Attribute("entId")?.Value, sourceId) OrElse sourceId <= 0 Then Continue For
            Dim sourceName = If(source.Attribute("name")?.Value, $"Crew {sourceId}")
            Dim relationships = source.Element("pers")?.Element("sociality")?.
                Element("relationships")?.Elements("l").ToList()
            If relationships Is Nothing Then Continue For

            Dim validRelationshipRows As New List(Of Tuple(Of Long, XElement))
            For Each relationship In relationships
                Dim targetId As Long
                If Not Long.TryParse(relationship.Attribute("targetId")?.Value, targetId) OrElse targetId <= 0 Then
                    findings.Add(RemoveFinding(
                        SaveIntegritySeverity.Warning, "Relationships", sourceName,
                        "Relationship has a missing, zero, or non-numeric targetId.",
                        "Remove invalid relationship", relationship))
                    Continue For
                End If

                validRelationshipRows.Add(Tuple.Create(targetId, relationship))
                If targetId = sourceId Then
                    findings.Add(RemoveFinding(
                        SaveIntegritySeverity.Warning, "Relationships", sourceName,
                        "Crew member has a relationship targeting themselves.",
                        "Remove self-relationship", relationship))
                ElseIf Not validCrewIds.Contains(targetId) Then
                    findings.Add(RemoveFinding(
                        SaveIntegritySeverity.Warning, "Relationships", $"{sourceName} -> Crew {targetId}",
                        "Relationship target does not exist in the loaded save.",
                        "Remove stale relationship", relationship))
                End If

                Dim invalidValues = {"friendship", "attraction", "compatibility"}.
                    Where(Function(attributeName)
                              Dim value As Integer
                              Return Not Integer.TryParse(relationship.Attribute(attributeName)?.Value, value) OrElse
                                     value < -100 OrElse value > 100
                          End Function).ToList()
                If invalidValues.Count > 0 Then
                    findings.Add(New SaveIntegrityFinding With {
                        .Severity = SaveIntegritySeverity.Warning,
                        .Category = "Relationships",
                        .AffectedObject = $"{sourceName} -> Crew {targetId}",
                        .Description = $"Relationship values outside -100 to 100: {String.Join(", ", invalidValues)}.",
                        .RepairDescription = "Clamp values to -100..100",
                        .RepairKind = SaveIntegrityRepairKind.ClampRelationshipValues,
                        .SourceElement = relationship
                    })
                End If
            Next

            For Each duplicate In validRelationshipRows.GroupBy(Function(item) item.Item1).
                Where(Function(group) group.Count() > 1)
                Dim rows = duplicate.Select(Function(item) item.Item2).ToList()
                findings.Add(New SaveIntegrityFinding With {
                    .Severity = SaveIntegritySeverity.Warning,
                    .Category = "Relationships",
                    .AffectedObject = $"{sourceName} -> Crew {duplicate.Key}",
                    .Description = $"{rows.Count} relationship rows target the same crew member.",
                    .RepairDescription = "Keep last relationship row",
                    .RepairKind = SaveIntegrityRepairKind.RemoveElements,
                    .ElementsToRemove = rows.Take(rows.Count - 1).ToList()
                })
            Next
        Next
    End Sub

    Private Function RemoveFinding(severity As SaveIntegritySeverity,
                                   category As String,
                                   affectedObject As String,
                                   description As String,
                                   repairDescription As String,
                                   element As XElement) As SaveIntegrityFinding
        Return New SaveIntegrityFinding With {
            .Severity = severity,
            .Category = category,
            .AffectedObject = affectedObject,
            .Description = description,
            .RepairDescription = repairDescription,
            .RepairKind = SaveIntegrityRepairKind.RemoveElements,
            .ElementsToRemove = New List(Of XElement) From {element}
        }
    End Function

    Private Sub ScanSchedules(root As XElement,
                              crew As List(Of XElement),
                              findings As List(Of SaveIntegrityFinding))
        Dim scheduleGroups = root.Element("globalSchedules")?.Elements("g").ToList()
        If scheduleGroups Is Nothing Then scheduleGroups = New List(Of XElement)
        Dim validScheduleIds As New HashSet(Of Long)
        Dim grouped As New Dictionary(Of Long, Integer)

        For Each schedule In scheduleGroups
            Dim scheduleId As Long
            If Not Long.TryParse(schedule.Attribute("schedId")?.Value, scheduleId) OrElse scheduleId <= 0 Then
                findings.Add(New SaveIntegrityFinding With {
                    .Severity = SaveIntegritySeverity.Critical,
                    .Category = "Global Schedules",
                    .AffectedObject = "Global schedule",
                    .Description = "Global schedule has a missing, zero, or non-numeric schedId.",
                    .RepairKind = SaveIntegrityRepairKind.None
                })
                Continue For
            End If
            validScheduleIds.Add(scheduleId)
            grouped(scheduleId) = If(grouped.ContainsKey(scheduleId), grouped(scheduleId) + 1, 1)
        Next

        For Each duplicate In grouped.Where(Function(pair) pair.Value > 1)
            findings.Add(New SaveIntegrityFinding With {
                .Severity = SaveIntegritySeverity.Critical,
                .Category = "Global Schedules",
                .AffectedObject = $"Schedule {duplicate.Key}",
                .Description = $"{duplicate.Value} global schedules share the same schedId.",
                .RepairKind = SaveIntegrityRepairKind.None
            })
        Next

        For Each character In crew
            Dim pers = character.Element("pers")
            If Not String.Equals(pers?.Attribute("useGlobal")?.Value, "true", StringComparison.OrdinalIgnoreCase) Then Continue For
            Dim scheduleId As Long
            If Not Long.TryParse(pers.Attribute("globalSch")?.Value, scheduleId) OrElse
               Not validScheduleIds.Contains(scheduleId) Then
                Dim crewName = If(character.Attribute("name")?.Value, "Unnamed crew")
                findings.Add(New SaveIntegrityFinding With {
                    .Severity = SaveIntegritySeverity.Warning,
                    .Category = "Global Schedules",
                    .AffectedObject = crewName,
                    .Description = $"Crew member references missing global schedule '{pers.Attribute("globalSch")?.Value}'.",
                    .RepairKind = SaveIntegrityRepairKind.None
                })
            End If
        Next
    End Sub

    Private Sub ScanShipReferences(root As XElement,
                                   validShipIds As HashSet(Of Long),
                                   findings As List(Of SaveIntegrityFinding))
        Dim blueprints = root.Element("blueprints")
        If blueprints IsNot Nothing Then
            For Each blueprint In blueprints.Elements("ship")
                Dim sid As Long
                If Long.TryParse(blueprint.Attribute("sid")?.Value, sid) AndAlso sid > 0 AndAlso
                   Not validShipIds.Contains(sid) Then
                    findings.Add(RemoveFinding(
                        SaveIntegritySeverity.Warning, "Ship References",
                        If(blueprint.Attribute("sname")?.Value, $"Blueprint ship {sid}"),
                        $"Blueprint references missing ship SID {sid}.",
                        "Remove stale blueprint reference", blueprint))
                End If
            Next
        End If

        For Each createdShip In root.Descendants("createdShips").Elements("l").
            Where(Function(node)
                      Return String.Equals(node.Attribute("created")?.Value, "true", StringComparison.OrdinalIgnoreCase)
                  End Function)
            Dim candidateIds As New List(Of Long)
            For Each attributeName In {"createdShipId", "slid"}
                Dim candidateId As Long
                If Long.TryParse(createdShip.Attribute(attributeName)?.Value, candidateId) AndAlso candidateId > 0 Then
                    candidateIds.Add(candidateId)
                End If
            Next
            If candidateIds.Count > 0 AndAlso Not candidateIds.Any(Function(id) validShipIds.Contains(id)) Then
                Dim isPlayer = String.Equals(createdShip.Parent?.Parent?.Attribute("isPlayer")?.Value,
                                             "true", StringComparison.OrdinalIgnoreCase)
                If isPlayer Then
                    findings.Add(RemoveFinding(
                        SaveIntegritySeverity.Warning, "Ship References",
                        If(createdShip.Attribute("shn")?.Value, $"Player created ship {String.Join("/", candidateIds)}"),
                        "Player createdShips row does not resolve to a loaded ship by createdShipId or slid.",
                        "Remove stale createdShips row", createdShip))
                Else
                    findings.Add(New SaveIntegrityFinding With {
                        .Severity = SaveIntegritySeverity.Information,
                        .Category = "Ship References",
                        .AffectedObject = If(createdShip.Attribute("shn")?.Value,
                                             $"Created ship {String.Join("/", candidateIds)}"),
                        .Description = "Spawned non-player ship is not in the currently loaded sector and may be off-sector.",
                        .RepairKind = SaveIntegrityRepairKind.None
                    })
                End If
            End If
        Next
    End Sub

    Private Sub ScanCounters(root As XElement, findings As List(Of SaveIntegrityFinding))
        Dim highestEntityId = root.DescendantsAndSelf().
            Attributes("entId").
            Select(Function(attribute)
                       Dim value As Long
                       Return If(Long.TryParse(attribute.Value, value), value, 0)
                   End Function).
            DefaultIfEmpty(0).Max()
        Dim counterOwner = If(root.Element("masterData")?.Attribute("idCounter") IsNot Nothing,
                              root.Element("masterData"), root)
        ScanCounter(counterOwner, "idCounter", highestEntityId, "Entity Counter", findings)

        Dim highestObjectId = root.DescendantsAndSelf().
            Attributes("objId").
            Select(Function(attribute)
                       Dim value As Long
                       Return If(Long.TryParse(attribute.Value, value), value, 0)
                   End Function).
            DefaultIfEmpty(0).Max()
        ScanCounter(root.Element("starmap"), "objectIdCounter", highestObjectId, "Object Counter", findings)
    End Sub

    Private Sub ScanCounter(owner As XElement,
                            attributeName As String,
                            highestExistingId As Long,
                            category As String,
                            findings As List(Of SaveIntegrityFinding))
        If owner Is Nothing OrElse highestExistingId <= 0 Then Return
        Dim currentValue As Long
        Dim rawValue = owner.Attribute(attributeName)?.Value
        If Not Long.TryParse(rawValue, currentValue) OrElse currentValue <= highestExistingId Then
            findings.Add(New SaveIntegrityFinding With {
                .Severity = SaveIntegritySeverity.Warning,
                .Category = category,
                .AffectedObject = attributeName,
                .Description = $"{attributeName} is '{rawValue}' but must be above existing ID {highestExistingId}.",
                .RepairDescription = $"Raise to {highestExistingId + 1}",
                .RepairKind = SaveIntegrityRepairKind.RaiseCounter,
                .SourceElement = owner,
                .CounterAttributeName = attributeName,
                .CounterValue = highestExistingId + 1
            })
        End If
    End Sub
End Class
