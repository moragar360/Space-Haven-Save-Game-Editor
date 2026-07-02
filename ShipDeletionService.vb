Imports System.IO
Imports System.Xml.Linq

Public Module ShipDeletionService
    Public Function DeleteShip(savePath As String, shipSid As Integer) As String
        If String.IsNullOrWhiteSpace(savePath) OrElse Not File.Exists(savePath) Then
            Throw New FileNotFoundException("The selected save file could not be found.", savePath)
        End If

        Dim document = XDocument.Load(savePath)
        Dim root = document.Root
        If root Is Nothing OrElse root.Name <> "game" Then
            Throw New InvalidDataException("The selected file is not a valid Space Haven save.")
        End If

        Dim sid = shipSid.ToString()
        Dim ship = root.Element("ships")?.
            Elements("ship").
            FirstOrDefault(Function(candidate) candidate.Attribute("sid")?.Value = sid)
        If ship Is Nothing Then Throw New InvalidDataException($"Ship SID {shipSid} was not found.")

        Dim createdShipRef = root.Descendants("createdShips").
            Elements("l").
            FirstOrDefault(Function(instance)
                               Return instance.Attribute("slid")?.Value = sid OrElse
                                      instance.Attribute("createdShipId")?.Value = sid
                           End Function)
        Dim isStation = ship.Attribute("sta")?.Value = "1" OrElse
                        ship.Element("station") IsNot Nothing OrElse
                        String.Equals(createdShipRef?.Attribute("station")?.Value, "true", StringComparison.OrdinalIgnoreCase)
        Dim isPlayerOwned = String.Equals(ship.Element("settings")?.Attribute("owner")?.Value, "Player", StringComparison.OrdinalIgnoreCase) OrElse
                            String.Equals(createdShipRef?.Parent?.Parent?.Attribute("isPlayer")?.Value, "true", StringComparison.OrdinalIgnoreCase)
        If isStation AndAlso isPlayerOwned Then
            Throw New InvalidOperationException("The player station is protected and cannot be deleted.")
        End If

        Dim shipCount = root.Element("ships")?.Elements("ship").Count()
        If shipCount.GetValueOrDefault() <= 1 Then
            Throw New InvalidOperationException("The last ship in a save cannot be deleted.")
        End If

        If isPlayerOwned Then
            Dim playerStructureCount = root.Element("ships")?.
                Elements("ship").
                Count(Function(candidate)
                          Dim candidateSid = candidate.Attribute("sid")?.Value
                          Dim candidateRef = root.Descendants("createdShips").
                              Elements("l").
                              FirstOrDefault(Function(instance)
                                                 Return instance.Attribute("slid")?.Value = candidateSid OrElse
                                                        instance.Attribute("createdShipId")?.Value = candidateSid
                                             End Function)
                          Return String.Equals(candidate.Element("settings")?.Attribute("owner")?.Value, "Player", StringComparison.OrdinalIgnoreCase) OrElse
                                 String.Equals(candidateRef?.Parent?.Parent?.Attribute("isPlayer")?.Value, "true", StringComparison.OrdinalIgnoreCase)
                      End Function)
            If playerStructureCount.GetValueOrDefault() <= 1 Then
                Throw New InvalidOperationException("The last player-owned ship or station cannot be deleted.")
            End If
        End If

        Dim shipName = ship.Attribute("sname")?.Value
        If String.IsNullOrWhiteSpace(shipName) Then shipName = $"SID {shipSid}"

        ship.Remove()

        root.Element("blueprints")?.
            Elements("ship").
            Where(Function(candidate) candidate.Attribute("sid")?.Value = sid).
            Remove()

        root.Descendants("createdShips").
            Elements("l").
            Where(Function(instance)
                      Return instance.Attribute("slid")?.Value = sid OrElse
                             instance.Attribute("createdShipId")?.Value = sid
                  End Function).
            Remove()

        Dim backupPath = savePath & ".bak"
        File.Copy(savePath, backupPath, True)
        document.Save(savePath)
        Return shipName
    End Function
End Module
