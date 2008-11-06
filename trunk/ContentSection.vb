Public Class ContentSection
    Inherits PreProcessSection

    Public Sub New(ByVal p As Project, ByVal options As Options)
        MyBase.New(p, options)
    End Sub

    Protected Overrides Function GetFiles(ByVal pathfilter As String) As String()
        Return AssignedProject.GetContent(pathfilter, Me.GetExcludedFiles())
    End Function

    Protected Overrides ReadOnly Property MySectionType() As PreProcessSection.SectionType
        Get
            Return PreProcessSection.SectionType.LeafExpand
        End Get
    End Property
End Class
