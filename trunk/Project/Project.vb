Imports System.Xml
Imports System.IO

Public MustInherit Class Project

    Protected m_doc As XmlDocument
    Protected m_projectFolder As String
    Protected m_configName As String
    Protected m_importList As ProjectCollection
    Protected m_xmlnsmgr As XmlNamespaceManager

    Public MustOverride Function GetProjectGuid() As String

    Public MustOverride Function GetAssemblyName() As String

    Public MustOverride Function IsAssemblyDLL() As Boolean

    Public MustOverride Function IsAssemblyEXE() As Boolean

    Public MustOverride Function GetOutputPath() As String

    Public MustOverride Function GetAssemblyInfoVersion() As AsmInfoVersion

    Protected MustOverride Function GetFileEntries() As String()

    Public MustOverride Function GetBin(ByVal options As Options) As String()

    Public MustOverride Function GetPDB() As String()

    Public Function ConvertToRelativePath(ByVal src As String) As String
        Return src.Replace(Me.m_projectFolder.Substring(0, Me.m_projectFolder.Length - 1), "")
    End Function

    Public Function GetProjectFolder() As String
        Return Me.m_projectFolder
    End Function

    Protected Function FullCompiledPath(ByVal filename As String) As String
        Return Path.Combine(Path.Combine(Me.m_projectFolder, Me.GetOutputPath()), filename)
    End Function

    Protected Function FullContentPath(ByVal filename As String) As String
        Return Path.Combine(Me.m_projectFolder, filename)
    End Function

    Public Function GetContentFolders() As String()
        Dim files() As String = Me.GetFileEntries()
        Dim folders As New ArrayList
        For i As Integer = 0 To files.Length - 1
            Dim tmpPath As String = Path.GetDirectoryName(Me.FullContentPath(files(i)))
            If Not folders.Contains(tmpPath) Then folders.Add(tmpPath)
        Next
        Return folders.ToArray(GetType(String))
    End Function

    Public Function GetContent(ByVal folderName As String, ByVal excludedFiles() As String) As String()
        Dim files() As String = Me.GetFileEntries()
        Dim entries As New ArrayList
        For i As Integer = 0 To files.Length - 1
            Dim tmpPath As String = Path.GetDirectoryName(Me.FullContentPath(files(i)))
            If tmpPath = folderName Then
                If Not Excluded(excludedFiles, files(i)) Then
                    entries.Add(Me.FullContentPath(files(i)))
                End If
            End If
        Next
        Return entries.ToArray(GetType(String))
    End Function

    Protected Function Excluded(ByVal excludedFiles() As String, ByVal filename As String) As Boolean
        For i As Integer = 0 To excludedFiles.Length - 1
            If filename.StartsWith("*") And filename.EndsWith("*") Then If excludedFiles(i).IndexOf(filename) > -1 Then Return True
            If filename.StartsWith("*") Then If excludedFiles(i).Replace(Me.m_projectFolder, "").StartsWith(filename) Then Return True
            If filename.EndsWith("*") Then If excludedFiles(i).EndsWith(filename) Then Return True
        Next
        Return False
    End Function


    Protected Function AsDLL(ByVal name As String) As String
        Return name & ".dll"
    End Function

    Protected Function AsPDB(ByVal name As String) As String
        Return name & ".pdb"
    End Function

    Protected Function AsEXE(ByVal name As String) As String
        Return name & ".exe"
    End Function






End Class
