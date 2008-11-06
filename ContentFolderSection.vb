Public Class ContentFolderSection
    Inherits PreProcessSection


    Public Sub New(ByVal p As Project, ByVal options As Options)
        MyBase.New(p, options)
    End Sub

    Protected Overloads Overrides Function GetFiles(ByVal pathfilter As String) As String()
        Return Me.AssignedProject.GetContentFolders()
    End Function

    Protected Overrides ReadOnly Property MySectionType() As PreProcessSection.SectionType
        Get
            Return PreProcessSection.SectionType.NestedExpand
        End Get
    End Property
End Class
