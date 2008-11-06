Public Class ProjectCollection
    Private _projects As Hashtable

    Public Sub New()
        Me._projects = New Hashtable
    End Sub

    Public Sub AddProject(ByVal p As Project)
        If Not Me._projects.ContainsKey(p.GetProjectGuid()) Then
            Me._projects.Add(p.GetProjectGuid(), p)
        End If
    End Sub

    Public Function GetProjectByGuid(ByVal guidReference As String) As Project
        Return Me._projects(guidReference)
    End Function

    Public Function GetProjectByAssemblyName(ByVal name As String) As Project
        Dim de As IDictionaryEnumerator = Me._projects.GetEnumerator()
        de.Reset()
        While de.MoveNext()
            If CType(de.Value, Project).GetAssemblyName().Trim().ToLower() = name.Trim().ToLower() Then Return CType(de.Value, Project)
        End While
        Return Nothing
    End Function
End Class
