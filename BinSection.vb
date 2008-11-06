Public Class BinSection
    Inherits PreProcessSection

    Public Sub New(ByVal p As Project, ByVal options As Options)
        MyBase.New(p, options)
    End Sub


    Protected Overloads Overrides Function GetFiles(ByVal path As String) As String()
        Return Me.AssignedProject.GetBin(Me.Options)
    End Function

    Protected Overrides ReadOnly Property MySectionType() As PreProcessSection.SectionType
        Get
            Return PreProcessSection.SectionType.LeafExpand
        End Get
    End Property
End Class
