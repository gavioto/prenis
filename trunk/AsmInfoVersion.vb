Imports System.IO

Public Class AsmInfoVersion

    Private _filename As String
    Public Sub New(ByVal filename As String)
        Me._filename = filename
    End Sub

    Public Function GetVersion(ByVal separator As String) As String
        Dim sr As StreamReader = File.OpenText(Me._filename)
        Dim info As String = sr.ReadToEnd()
        sr.Close()
        Dim token As String = "AssemblyVersion"
        Dim idx As Integer = info.IndexOf(token)
        idx = idx + token.Length
        If idx > -1 Then
            Dim endIdx As Integer = info.IndexOf(")", idx)
            If endIdx > -1 Then
                Return info.Substring(idx, endIdx - idx).Replace("""", "").Replace("(", "").Replace(".*", "").Replace(".", separator).Trim()
            End If
        End If
        Return ""
    End Function

End Class
