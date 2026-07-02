Public Module SaveIntegrityRepairService
    Public Sub Apply(finding As SaveIntegrityFinding)
        If finding Is Nothing OrElse Not finding.CanRepair Then Return

        Select Case finding.RepairKind
            Case SaveIntegrityRepairKind.RemoveElements
                For Each element In finding.ElementsToRemove
                    If element?.Parent IsNot Nothing Then element.Remove()
                Next
            Case SaveIntegrityRepairKind.ClampRelationshipValues
                If finding.SourceElement Is Nothing Then Return
                For Each attributeName In {"friendship", "attraction", "compatibility"}
                    Dim value As Integer
                    If Not Integer.TryParse(finding.SourceElement.Attribute(attributeName)?.Value, value) Then value = 0
                    finding.SourceElement.SetAttributeValue(attributeName, Math.Max(-100, Math.Min(100, value)))
                Next
            Case SaveIntegrityRepairKind.RaiseCounter
                finding.SourceElement?.SetAttributeValue(finding.CounterAttributeName, finding.CounterValue)
        End Select
    End Sub
End Module
