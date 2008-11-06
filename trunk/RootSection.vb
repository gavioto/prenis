Public Class RootSection
    Inherits PreProcessSection

    Public Sub New(ByVal options As Options)
        MyBase.New(Nothing, options)
    End Sub

    Protected Overloads Overrides Function GetFiles(ByVal path As String) As String()
        Return Nothing
    End Function

    Protected Overrides ReadOnly Property MySectionType() As PreProcessSection.SectionType
        Get
            Return PreProcessSection.SectionType.SimpleExpand
        End Get
    End Property
End Class
