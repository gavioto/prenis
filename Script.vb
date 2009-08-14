Imports System.IO
Imports System.Xml

Public Class Script
    Private Const DELIM As String = ";"
    Private Const CMD As String = "##"
    Private m_script As String
    Private m_projects As ProjectCollection
    Private m_options As Options

    Public Sub New(ByVal filename As String, ByVal options As Options)
        m_projects = New ProjectCollection
        m_options = options
        Dim sr As StreamReader = Nothing
        Try
            sr = File.OpenText(filename)
            Me.m_script = sr.ReadToEnd()
        Finally
            If Not sr Is Nothing Then sr.Close()
        End Try
    End Sub



    Public Sub Process(ByVal outputFilename As String)
        Console.WriteLine()
        Console.WriteLine("PASS 1: Processing Includes...")
        ProcessImports()
        Console.WriteLine()
        Console.WriteLine("PASS 2: Processing Macros...")
        Dim frag As PreProcessSection = Me.ProcessFrags()
        Console.WriteLine()
        Console.WriteLine("PASS 3: Rendering...")
        Dim output As String = frag.Render()
        Console.WriteLine()
        Console.WriteLine("Saving " & Path.GetFileName(outputFilename))
        Dim sw As StreamWriter = Nothing
        Try
            sw = New StreamWriter(outputFilename)
            sw.Write(output)
        Finally
            If Not sw Is Nothing Then sw.Close()
        End Try
    End Sub

    Private Sub ProcessImports()
        Dim buf As String = ""
        Dim active As Boolean = False 'command interpreting mode
        For idx As Integer = 0 To m_script.Length - 1
            buf = buf & m_script.Chars(idx)
            If active Then
                If buf.EndsWith(CMD) Then
                    If buf.Length > 2 Then
                        Dim p As Project = ParseImport(buf.Substring(0, buf.Length - 2))
                        If Not p Is Nothing Then
                            Me.m_projects.AddProject(p)
                            Console.WriteLine("Importing: " & p.GetAssemblyName() & ": " & p.GetProjectFolder())
                        End If
                    End If
                    buf = ""
                    active = False
                End If
            Else
                If buf.EndsWith(CMD) Then
                    buf = ""
                    active = True
                End If
            End If
        Next
    End Sub

    Private Function ParseImport(ByVal cmd As String) As Project
        Dim frags() As String = cmd.split(DELIM)
        If frags.Length < 1 Then Return Nothing
        If frags(0).ToLower().Trim() = "import" Then
            Try
                Dim filename As String = GetArg("project", frags)
                Dim projectFolder As String = GetArg("folder", frags)
                Dim configName As String = GetArg("config", frags)
                Dim studioVersion As String = GetArg("vs", frags)
                Console.WriteLine("Importing filename: " & filename & "; Folder: " & projectFolder & "; VS Version: " & studioVersion)
                If projectFolder = "" Then
                    projectFolder = Path.GetDirectoryName(filename)
                End If
                If Not projectFolder.EndsWith("\") Then projectFolder = projectFolder & "\"
                Dim sr As StreamReader
                'Try
                sr = File.OpenText(filename)
                Dim fileContents As String = sr.ReadToEnd()
                sr.Close()

                Dim doc As New XmlDocument
                doc.LoadXml(fileContents)
                Dim p As Project
                If studioVersion.IndexOf("2008") >= 0 Then
                    p = New Project2008(fileContents, projectFolder, configName, Me.m_projects)
                ElseIf studioVersion.IndexOf("2005") >= 0 Then
                    p = New Project2005(fileContents, projectFolder, configName, Me.m_projects)
                ElseIf studioVersion.IndexOf("2003") >= 0 Then
                    p = New Project2003(fileContents, projectFolder, configName, Me.m_projects)
                Else
                    If fileContents.IndexOf("<ProductVersion>") >= 0 Then
                        'VS2005 or VS2008
                        p = New Project2005(fileContents, projectFolder, configName, Me.m_projects)
                    Else
                        'VS2003
                        p = New Project2003(fileContents, projectFolder, configName, Me.m_projects)
                    End If
                End If
                Console.WriteLine("Imported " & p.GetType().ToString() & ": " & p.GetAssemblyName())
                Return p
            Catch ex As Exception
                SyntaxError("Import error: " & ex.Message)
            End Try
        End If
        Return Nothing
    End Function

    Public Function ProcessFrags() As RootSection
        Dim fragStack As New Stack
        Dim rootFrag As PreProcessSection = New RootSection(Me.m_options)
        Dim activeFrag As PreProcessSection = rootFrag

        Dim buf As String = ""

        Dim active As Boolean = False 'command interpreting mode
        For idx As Integer = 0 To m_script.Length - 1
            buf = buf & m_script.Chars(idx)
            If active Then
                If buf.EndsWith(CMD) Then
                    If buf.Length > 2 Then
                        Dim tmpCmd As String = buf.Substring(0, buf.Length - 2)
                        Dim tmpCmdOutput As String = ParseCommand(tmpCmd, activeFrag)
                        'If tmpCmdOutput <> "" Then Console.WriteLine(tmpCmdOutput)
                        If tmpCmdOutput.Trim() = "macro.end" Then
                            Console.WriteLine("Closing block: " & activeFrag.ID)
                            activeFrag = CType(fragStack.Pop(), PreProcessSection)
                        ElseIf tmpCmdOutput.Trim().StartsWith("macro.") Then
                            Dim tmpFrag As PreProcessSection = ParseNewSection(tmpCmdOutput.Trim(), activeFrag)
                            Console.WriteLine("Adding macro block: " & tmpFrag.ID & " " & tmpCmd)
                            activeFrag.AddChild(tmpFrag)
                            fragStack.Push(activeFrag)
                            activeFrag = tmpFrag
                        Else
                            activeFrag.Buffer = activeFrag.Buffer & tmpCmdOutput
                        End If
                    End If
                    buf = ""
                    active = False
                End If
            Else
                If buf.EndsWith(CMD) Then
                    If buf.Length > 2 Then
                        activeFrag.Buffer = activeFrag.Buffer & buf.Substring(0, buf.Length - 2)
                    End If
                    buf = ""
                    active = True
                End If
            End If
        Next
        If buf <> "" Then activeFrag.Buffer = activeFrag.Buffer & buf
        Return rootFrag
    End Function

    Private Function ParseNewSection(ByVal cmd As String, ByVal activeFrag As PreProcessSection) As PreProcessSection
        Dim sec As PreProcessSection = Nothing
        Dim params() As String = cmd.Split(DELIM)
        Dim p As Project = Nothing
        If Not activeFrag.AssignedProject Is Nothing Then p = activeFrag.AssignedProject
        Dim asmName As String = GetArg("assembly", params)
        If asmName <> "" Then p = Me.m_projects.GetProjectByAssemblyName(asmName)
        If p Is Nothing Then SyntaxError("Assembly not found: " & asmName)

        Dim recurse As String = GetArg("recursive", params).ToLower()
        If recurse = "true" Then Me.m_options.UseRecursiveReferencing = True
        If recurse = "false" Then Me.m_options.UseRecursiveReferencing = False

        Dim useReference As String = GetArg("includereferences", params).ToLower()
        If useReference = "true" Then Me.m_options.UseReferenceBinaries = True
        If useReference = "false" Then Me.m_options.UseReferenceBinaries = False

        Dim useProject As String = GetArg("includeprojects", params).ToLower()
        If useProject = "true" Then Me.m_options.UseProjectBinaries = True
        If useProject = "false" Then Me.m_options.UseProjectBinaries = False

        Select Case params(0).ToLower().Trim()
            Case "macro.dll"
                sec = New BinSection(p, Me.m_options)
            Case "macro.bin"
                sec = New BinSection(p, Me.m_options)
            Case "macro.debug"
                sec = New PDBSection(p, Me.m_options)
            Case "macro.pdb"
                sec = New PDBSection(p, Me.m_options)
            Case "macro.contentfolders"
                sec = New ContentFolderSection(p, Me.m_options)
            Case "macro.content"
                sec = New ContentSection(p, Me.m_options)
        End Select
        Return sec
    End Function

    Private Function ParseCommand(ByVal cmd As String, ByVal activeFrag As PreProcessSection) As String
        Dim params() As String = cmd.Split(DELIM)
        Select Case params(0).ToLower().Trim()
            Case "version"
                Console.WriteLine("processing: " & cmd)
                Dim p As Project = Nothing
                If Not activeFrag.AssignedProject Is Nothing Then p = activeFrag.AssignedProject
                Dim asmName As String = GetArg("assembly", params)
                If asmName <> "" Then p = Me.m_projects.GetProjectByAssemblyName(asmName)
                If p Is Nothing Then SyntaxError("Assembly not found: " & asmName)

                Dim info As AsmInfoVersion = p.GetAssemblyInfoVersion

                If info Is Nothing Then SyntaxError("version error. No version information available: " & asmName)

                Return info.GetVersion(GetArg("delimiter", params))
            Case "exclude"
                Console.WriteLine("processing: " & cmd)
                Dim fname As String = GetArg("filename", params)
                If fname = "" Then SyntaxError("exclude filename argument not specified: " & cmd)
                activeFrag.AddExclude(fname)
                Return ""
            Case "include"
                Console.WriteLine("processing: " & cmd)
                Dim fname As String = GetArg("filename", params)
                If fname = "" Then SyntaxError("include filename argument not specified: " & cmd)
                activeFrag.AddInclude(fname)
                Return ""
            Case "import"
                Return ""
        End Select
        Return cmd
    End Function

    'Note: added command trimming and white space handler to command arguments version 1.1.
    Private Shared Function GetArg(ByVal name As String, ByVal args() As String) As String
        Dim action As String = name.ToLower()
        Dim currentCommand As String = ""
        For i As Integer = 0 To args.Length - 1
            If args(i).ToLower().Trim().StartsWith(action) Then
                Dim eq As Integer = args(i).IndexOf("=")
                If eq > -1 Then
                    Return args(i).Substring(eq + 1).Trim()
                End If
            End If
        Next
        Return ""
    End Function

    Private Sub SyntaxError(ByVal cmd As String)
        Console.WriteLine(cmd)
        End
    End Sub
End Class






