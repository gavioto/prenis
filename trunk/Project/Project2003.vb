Imports System.Xml
Imports System.IO

Public Class Project2003
    Inherits Project

    Public Sub New(ByVal fileContents As String, ByVal projectFolder As String, ByVal configName As String, ByVal importList As ProjectCollection)
        m_projectFolder = projectFolder
        m_configName = configName
        Me.m_doc = New XmlDocument
        Me.m_doc.LoadXml(fileContents)
        Me.m_importList = importList
    End Sub

    Public Overrides Function GetProjectGuid() As String
        If IsCS() Then
            Dim node As XmlNode = Me.m_doc.SelectSingleNode("/VisualStudioProject/CSHARP")
            Return node.Attributes.GetNamedItem("ProjectGuid").InnerText
        ElseIf IsVB() Then
            Dim node As XmlNode = Me.m_doc.SelectSingleNode("/VisualStudioProject/VisualBasic")
            Return node.Attributes.GetNamedItem("ProjectGuid").InnerText
        End If
        Return Nothing
    End Function

    Public Overrides Function GetAssemblyName() As String
        Try
            Return Me.m_doc.SelectSingleNode("//Build/Settings").Attributes.GetNamedItem("AssemblyName").InnerText
        Catch ex As Exception
            Return ""
        End Try
    End Function

    Public Overrides Function IsAssemblyDLL() As Boolean
        Try
            Dim mode As String = Me.m_doc.SelectSingleNode("//Build/Settings").Attributes.GetNamedItem("OutputType").InnerText.Trim()
            If mode = "Library" Then Return True
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    Public Overrides Function IsAssemblyEXE() As Boolean
        Try
            Dim mode As String = Me.m_doc.SelectSingleNode("//Build/Settings").Attributes.GetNamedItem("OutputType").InnerText.Trim()
            If (mode.ToLower().IndexOf("exe") > -1) Then Return True
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    Public Overrides Function GetAssemblyInfoVersion() As AsmInfoVersion
        If Me.IsCS Then
            Return New AsmInfoVersion(Path.Combine(Me.GetProjectFolder(), "AssemblyInfo.cs"))
        End If
        If Me.IsVB Then
            Return New AsmInfoVersion(Path.Combine(Me.GetProjectFolder(), "AssemblyInfo.vb"))
        End If
        Return Nothing  ' #!#!
    End Function

    Public Overrides Function GetOutputPath() As String
        Try
            Return Me.m_doc.SelectSingleNode("//Build/Settings/Config[@Name='" & m_configName & "']").Attributes.GetNamedItem("OutputPath").InnerText
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


        nodes = Me.m_doc.SelectNodes("//Reference")
        If nodes Is Nothing Then Return Nothing
        For i As Integer = 0 To nodes.Count - 1
            Dim myName As String = nodes(i).Attributes.GetNamedItem("Name").InnerText
            If options.UseProjectBinaries Then
                If Not nodes(i).Attributes.GetNamedItem("Project") Is Nothing Then
                    Dim projectGuid As String = nodes(i).Attributes.GetNamedItem("Project").InnerText
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
                End If
            ElseIf Not nodes(i).Attributes.GetNamedItem("HintPath") Is Nothing Then
                If options.UseReferenceBinaries Then
                    Dim fn As String = nodes(i).Attributes.GetNamedItem("HintPath").InnerText
                    If fn.IndexOf("\Microsoft.NET\Framework") < 0 Then
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
                End If
            End If
        Next
        Return entries.ToArray(GetType(String))
    End Function

    Public Overrides Function GetPDB() As String()
        Dim entries As New ArrayList
        entries.Add(Me.FullCompiledPath(Me.AsPDB(GetAssemblyName())))
        Dim nodes As XmlNodeList
        nodes = Me.m_doc.SelectNodes("//Reference")
        If nodes Is Nothing Then Return Nothing
        For i As Integer = 0 To nodes.Count - 1
            Dim myName As String = nodes(i).Attributes.GetNamedItem("Name").InnerText
            If Not nodes(i).Attributes.GetNamedItem("Project") Is Nothing Then
                Dim projectGuid As String = nodes(i).Attributes.GetNamedItem("Project").InnerText
                Dim p As Project = Me.m_importList.GetProjectByGuid(projectGuid)
                If p Is Nothing Then Throw New Exception("Referenced project not found: " & myName)
                entries.Add(Me.FullCompiledPath(Me.AsPDB(p.GetAssemblyName())))
            End If
        Next
        Return entries.ToArray(GetType(String))
    End Function

    Protected Overrides Function GetFileEntries() As String()
        Dim entries As New ArrayList
        Dim nodes As XmlNodeList
        nodes = Me.m_doc.SelectNodes("//File")
        If nodes Is Nothing Then Return Nothing
        For i As Integer = 0 To nodes.Count - 1
            Dim relPath As String = nodes(i).Attributes.GetNamedItem("RelPath").InnerText
            Dim buildAction As String = nodes(i).Attributes.GetNamedItem("BuildAction").InnerText
            If buildAction = "Content" Then
                entries.Add(relPath)
            End If
        Next
        Return entries.ToArray(GetType(String))
    End Function

    Private Function IsCS() As Boolean
        Dim node As XmlNode = Me.m_doc.SelectSingleNode("/VisualStudioProject/CSHARP")
        If Not node Is Nothing Then Return True
        Return False
    End Function

    Private Function IsVB() As Boolean
        Dim node As XmlNode = Me.m_doc.SelectSingleNode("/VisualStudioProject/VisualBasic")
        If Not node Is Nothing Then Return True
        Return False
    End Function

End Class
