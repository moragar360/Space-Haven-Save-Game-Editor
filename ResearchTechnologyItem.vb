Imports System.Xml.Linq

Public Class ResearchTechnologyItem
    Public Property TechId As Integer
    Public Property TechnologyName As String
    Public Property StateElement As XElement
    Public Property IsQueued As Boolean

    Public ReadOnly Property StageCount As Integer
        Get
            Return StateElement?.Element("stageStates")?.Elements("l").Count()
        End Get
    End Property

    Public ReadOnly Property CompletedStageCount As Integer
        Get
            Return StateElement?.Element("stageStates")?.Elements("l").
                Count(Function(stage) String.Equals(stage.Attribute("done")?.Value, "true",
                                                    StringComparison.OrdinalIgnoreCase))
        End Get
    End Property

    Public ReadOnly Property StageSummary As String
        Get
            Return $"{CompletedStageCount}/{StageCount}"
        End Get
    End Property

    Public ReadOnly Property Status As String
        Get
            If StageCount > 0 AndAlso CompletedStageCount = StageCount Then Return "Completed"
            If IsQueued Then Return "Queued"
            If HasProgress Then Return "In Progress"
            Return "Not Started"
        End Get
    End Property

    Public ReadOnly Property ProgressSummary As String
        Get
            Dim stages = StateElement?.Element("stageStates")?.Elements("l").ToList()
            If stages Is Nothing OrElse stages.Count = 0 Then Return "No stages"

            Return String.Join(" | ", stages.Select(
                Function(stage)
                    Dim stageNumber = stage.Attribute("stage")?.Value
                    If String.IsNullOrWhiteSpace(stageNumber) Then stageNumber = "?"
                    Dim blocks = stage.Element("blocksDone")
                    Dim progress = If(blocks Is Nothing,
                                      "tasks",
                                      $"{blocks.Attribute("level1")?.Value}/{blocks.Attribute("level2")?.Value}/{blocks.Attribute("level3")?.Value}")
                    Dim done = String.Equals(stage.Attribute("done")?.Value, "true",
                                             StringComparison.OrdinalIgnoreCase)
                    Return $"Stage {stageNumber}: {progress}{If(done, " done", "")}"
                End Function))
        End Get
    End Property

    Private ReadOnly Property HasProgress As Boolean
        Get
            For Each stage In StateElement?.Element("stageStates")?.Elements("l")
                If String.Equals(stage.Attribute("done")?.Value, "true", StringComparison.OrdinalIgnoreCase) Then Return True
                Dim blocks = stage.Element("blocksDone")
                If blocks Is Nothing Then Continue For
                For Each attributeName In {"level1", "level2", "level3"}
                    Dim value As Integer
                    If Integer.TryParse(blocks.Attribute(attributeName)?.Value, value) AndAlso value > 0 Then Return True
                Next
            Next
            Return False
        End Get
    End Property
End Class

Public Class ResearchQueueItem
    Public Property TechId As Integer
    Public Property TechnologyName As String
    Public Property QueueElement As XElement

    Public ReadOnly Property DisplayName As String
        Get
            Return $"{TechnologyName} ({TechId})"
        End Get
    End Property
End Class
