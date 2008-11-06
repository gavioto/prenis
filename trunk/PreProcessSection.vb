Imports System.IO

Public MustInherit Class PreProcessSection
    Private _guid As Guid
    Private _buffer As String
    Private _project As Project
    Private _children As Hashtable
    Private _excludes As ArrayList
    Private _includes As ArrayList

    Protected Enum SectionType
        SimpleExpand
        NestedExpand
        LeafExpand
    End Enum

    Public Sub New(ByVal p As Project, ByVal o As Options)
        _guid = Guid.NewGuid()
        _project = p
        _children = New Hashtable
        _excludes = New ArrayList
        _includes = New ArrayList
        _Options = o
    End Sub

    Public Property Buffer() As String
        Get
            Return _buffer
        End Get
        Set(ByVal Value As String)
            _buffer = Value
        End Set
    End Property

    Private _Options As Options
    Public Property Options() As Options
        Get
            Return _Options
        End Get
        Set(ByVal Value As Options)
            _Options = Value
        End Set
    End Property

    Public ReadOnly Property AssignedProject() As Project
        Get
            Return Me._project
        End Get
    End Property

    Public ReadOnly Property ID() As String
        Get
            Return "##" & Me._guid.ToString() & "##"
        End Get
    End Property

    Public Sub AddChild(ByVal p As PreProcessSection)
        Me._children.Add(p.ID, p)
        Me._buffer = Me._buffer & p.ID
    End Sub

    Public Sub AddExclude(ByVal filename As String)
        _excludes.Add(filename)
    End Sub

    Public Sub AddInclude(ByVal filename As String)
        _includes.Add(filename)
    End Sub

    Protected Function GetExcludedFiles() As String()
        Return Me._excludes.ToArray(GetType(String))
    End Function

    Private Function ExpandVariables(ByVal localfilename As String) As String
        Dim isPath As Boolean = Not Path.HasExtension(localfilename)
        Try
            Dim absFilename As String = IIf(isPath, "", localfilename)
            Dim absPath As String = IIf(isPath, localfilename, Path.GetDirectoryName(localfilename))
            If Not absPath.EndsWith("\") Then absPath = absPath & "\"
            Dim relFilename As String = IIf(isPath, "", Me._project.ConvertToRelativePath(localfilename))
            If relFilename.StartsWith("\") Then relFilename = relFilename.Substring(1)
            Dim outpath As String
            If Me._project.ConvertToRelativePath(localfilename) = "" Then
                outpath = ""
            Else
                outpath = IIf(isPath, Me._project.ConvertToRelativePath(localfilename), Path.GetDirectoryName(Me._project.ConvertToRelativePath(localfilename)))
                If outpath.StartsWith("\") Then outpath = outpath.Substring(1)
                If Not outpath.EndsWith("\") Then outpath = outpath & "\"
            End If
            Dim filename As String = IIf(isPath, "", Path.GetFileName(relFilename))
            Return Buffer.Replace("%%absfilename%%", absFilename).Replace("%%abspath%%", absPath).Replace("%%outpath%%", outpath).Replace("%%filename%%", filename).Replace("%%relfilename%%", relFilename)
        Catch ex As Exception
            Console.WriteLine(ex.ToString())
            Console.WriteLine(localfilename)
            Console.WriteLine(Me._project.ConvertToRelativePath(localfilename))
        End Try
        Return Nothing  ' #!#!
    End Function

    Public Function Render() As String
        Return Me.Render("")
    End Function

    Public Function Render(ByVal pathname As String) As String
        Console.Write(".")
        Console.Out.Flush()
        Dim files() As String = Me.GetFiles(pathname)
        Dim output As String = ""
        If Not files Is Nothing Then
            If files.Length > 0 Then
                For i As Integer = 0 To files.Length - 1
                    If Me._excludes.Count > 0 Then
                        If Not IsExcluded(files(i)) Then output = output & Me.ExpandVariables(files(i))
                    ElseIf Me._includes.Count > 0 Then
                        If IsIncluded(files(i)) Then output = output & Me.ExpandVariables(files(i))
                    ElseIf Me._includes.Count = 0 And Me._excludes.Count = 0 Then
                        output = output & Me.ExpandVariables(files(i))
                    End If
                    If MySectionType = SectionType.NestedExpand Then output = ExpandChildren(output, files(i))
                Next
            End If
        Else
            output = Buffer
        End If
        If MySectionType = SectionType.SimpleExpand Then output = ExpandChildren(output, pathname)
        Return output
    End Function

    Protected Function ExpandChildren(ByVal input As String, ByVal pathname As String) As String
        If Me._children.Count < 1 Then Return input
        Dim de As IDictionaryEnumerator = Me._children.GetEnumerator()
        de.Reset()
        While de.MoveNext()
            Dim output As String = CType(de.Value, PreProcessSection).Render(pathname)
            input = input.Replace(de.Key, output)
        End While
        Return input
    End Function

    Private Function IsExcluded(ByVal filename As String) As Boolean
        For i As Integer = 0 To Me._excludes.Count - 1
            Dim cmp As String = CStr(Me._excludes(i)).Replace("*", "")
            If CStr(Me._excludes(i)).StartsWith("*") And CStr(Me._excludes(i)).EndsWith("*") Then If filename.IndexOf(cmp) > -1 Then Return True
            If CStr(Me._excludes(i)).StartsWith("*") Then If filename.EndsWith(cmp) Then Return True
            If CStr(Me._excludes(i)).EndsWith("*") Then If filename.StartsWith(cmp) Then Return True
        Next
        Return False
    End Function

    Private Function IsIncluded(ByVal filename As String) As Boolean
        For i As Integer = 0 To Me._includes.Count - 1
            Dim cmp As String = CStr(Me._includes(i)).Replace("*", "")
            If CStr(Me._includes(i)).StartsWith("*") And CStr(Me._includes(i)).EndsWith("*") Then If filename.IndexOf(cmp) > -1 Then Return True
            If CStr(Me._includes(i)).StartsWith("*") Then If filename.EndsWith(cmp) Then Return True
            If CStr(Me._includes(i)).EndsWith("*") Then If filename.StartsWith(cmp) Then Return True
        Next
        Return False
    End Function

    Protected MustOverride ReadOnly Property MySectionType() As SectionType
    Protected MustOverride Function GetFiles(ByVal pathfilter As String) As String()


End Class
