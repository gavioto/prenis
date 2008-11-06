Imports System.Xml
Imports System.IO

Public Class Project2008
    Inherits Project2005

    Public Sub New(ByVal fileContents As String, ByVal projectFolder As String, ByVal configName As String, ByVal importList As ProjectCollection)
        MyBase.New(fileContents, projectFolder, configName, importList)

    End Sub

    Public Overrides Function GetProjectGuid() As String
        Try
            Return Me.m_doc.SelectSingleNode("//PropertyGroup/ProjectGuid").InnerText
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Overrides Function GetAssemblyName() As String
        Try
            Return Me.m_doc.SelectSingleNode("//PropertyGroup/AssemblyName").InnerText
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Overrides Function IsAssemblyDLL() As Boolean
        Try
            Dim mode As String = Me.m_doc.SelectSingleNode("//PropertyGroup/OutputType").InnerText.Trim()
            If mode = "Library" Then Return True
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    Public Overrides Function IsAssemblyEXE() As Boolean
        Try
            Dim mode As String = Me.m_doc.SelectSingleNode("//PropertyGroup/OutputType").InnerText.Trim()
            If (mode.ToLower().IndexOf("exe") > -1) Then Return True
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    Public Overrides Function GetAssemblyInfoVersion() As AsmInfoVersion
        Dim node As XmlElement = Me.m_doc.SelectSingleNode("//Compile[@Include='AssemblyInfo.vb']")
        If Not node Is Nothing Then
            Return New AsmInfoVersion(Path.Combine(Me.GetProjectFolder(), "AssemblyInfo.vb"))
        End If
        node = Me.m_doc.SelectSingleNode("//Compile[@Include='Properties\AssemblyInfo.cs']")
        If Not node Is Nothing Then
            Return New AsmInfoVersion(Path.Combine(Me.GetProjectFolder(), "Properties\AssemblyInfo.cs"))
        End If
        Return Nothing  ' #!#!
    End Function

    Public Overrides Function GetOutputPath() As String
        Try
            Return Me.m_doc.SelectSingleNode("//PropertyGroup[contains(@Condition, '" & m_configName & "')=1]/OutputPath").InnerText
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Overrides Function GetBin(ByVal options As Options) As String()
        Dim entries As New ArrayList
        If Me.IsAssemblyDLL() Then
            entries.Add(Me.FullCompiledPath(Me.AsDLL(Me.GetAssemblyName())))
        End If
        If Me.IsAssemblyEXE() Then
            entries.Add(Me.FullCompiledPath(Me.AsEXE(Me.GetAssemblyName())))
        End If
        Dim nodes As XmlNodeList

        If options.UseReferenceBinaries Then
            nodes = Me.m_doc.SelectNodes("//ItemGroup/Reference")
            If nodes Is Nothing Then Return Nothing
            For i As Integer = 0 To nodes.Count - 1
                If Not nodes(i).SelectSingleNode("HintPath") Is Nothing Then
                    Dim fn As String = nodes(i).SelectSingleNode("HintPath").InnerText
                    Dim binFileName As String = Me.FullCompiledPath(Path.GetFileName(fn))
                    If File.Exists(binFileName) Then
                        entries.Add(binFileName)
                    Else
                        'The dll does not exist in the bin directory.
                        'Try using the hint file path - however, this may not always be accurate,
                        'so we can only use it as a fallback
                        entries.Add(fn)
                    End If
                End If
            Next
        End If

        If options.UseProjectBinaries Then
            nodes = Me.m_doc.SelectNodes("//ItemGroup/ProjectReference")
            If nodes Is Nothing Then Return Nothing
            For i As Integer = 0 To nodes.Count - 1
                Dim myName As String = nodes(i).SelectSingleNode("Name").InnerText
                Dim projectGuid As String = nodes(i).SelectSingleNode("Project").InnerText
                Dim p As Project = Me.m_importList.GetProjectByGuid(projectGuid)
                If p Is Nothing Then Throw New Exception("Referenced project not found: " & myName)
                entries.Add(Me.FullCompiledPath(AsDLL(p.GetAssemblyName())))
                If options.UseRecursiveReferencing Then
                    'Also add the DLLs that this project references
                    Dim referenceProjectBinaries As String() = p.GetBin(options)
                    For j As Integer = 0 To referenceProjectBinaries.Length - 1
                        entries.Add(referenceProjectBinaries(j))
                    Next
                End If
            Next
        End If

        Return entries.ToArray(GetType(String))
    End Function

    Public Overrides Function GetPDB() As String()
        Dim entries As New ArrayList
        entries.Add(Me.FullCompiledPath(Me.AsPDB(GetAssemblyName())))
        Dim nodes As XmlNodeList
        nodes = Me.m_doc.SelectNodes("//ItemGroup/ProjectReference")
        If nodes Is Nothing Then Return Nothing
        For i As Integer = 0 To nodes.Count - 1
            Dim myName As String = nodes(i).SelectSingleNode("Name").InnerText
            Dim projectGuid As String = nodes(i).SelectSingleNode("Project").InnerText
            Dim p As Project = Me.m_importList.GetProjectByGuid(projectGuid)
            If p Is Nothing Then Throw New Exception("Referenced project not found: " & myName)
            entries.Add(Me.FullCompiledPath(Me.AsPDB(p.GetAssemblyName())))
        Next
        Return entries.ToArray(GetType(String))
    End Function

    Protected Overrides Function GetFileEntries() As String()
        Dim entries As New ArrayList
        Dim nodes As XmlNodeList
        nodes = Me.m_doc.SelectNodes("//Content")
        If nodes Is Nothing Then Return Nothing
        For i As Integer = 0 To nodes.Count - 1
            Dim relPath As String = nodes(i).Attributes.GetNamedItem("Include").InnerText
            entries.Add(relPath)
        Next
        Return entries.ToArray(GetType(String))
    End Function


End Class
