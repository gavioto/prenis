Imports System.IO

Public Class Main
    Private Const help As String = "Usage: prenis.exe -source=[file.nsi] -target=[file.nsi] -recursive=[true/false]" & vbCrLf _
    & "-source : Source NSI file with preprocessor commands" & vbCrLf _
    & "-target : Target NSI file with expanded commands" & vbCrLf _
    & "-recursive : Determines if Prenis adds binary files referenced in referenced projects. Optional, default=true." & vbCrLf

    Public Shared Sub Main()
        Console.WriteLine("PreNIS (C)2006-2007 Pixolüt Industries")
        Console.WriteLine("Written by Joe Cincotta, Stephen Trembath and Chris Thomas")
        Console.WriteLine("NSIS Pre Processor " & My.Application.Info.Version.ToString())
        Console.WriteLine("--------------------------------------------------")
        Console.WriteLine("Use -help for usage")
        Console.WriteLine()
        Console.WriteLine("This program is free software: you can redistribute it and/or modify")
        Console.WriteLine("it under the terms of the GNU General Public License as published by")
        Console.WriteLine("the Free Software Foundation, either version 3 of the License, or")
        Console.WriteLine("(at your option) any later version.")
        Console.WriteLine()
        Console.WriteLine("This program is distributed in the hope that it will be useful,")
        Console.WriteLine("but WITHOUT ANY WARRANTY; without even the implied warranty of")
        Console.WriteLine("MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the")
        Console.WriteLine("GNU General Public License for more details.")
        Console.WriteLine()
        Console.WriteLine("You should have received a copy of the GNU General Public License")
        Console.WriteLine("along with this program.  If not, see <http://www.gnu.org/licenses/>.")
        Console.WriteLine()
        Console.WriteLine("You can get this project from <http://www.sf.net/projects/prenis>.")
        Console.WriteLine("Project information and examples can be found at <http://www.pixolut.com/prenis/>.")
        Console.WriteLine("--------------------------------------------------")
        Console.WriteLine()


        If GetFlag("help") Then
            Console.WriteLine(help)
            Return
        End If

        Dim inputFilename As String = GetArg("source")
        Dim outputFilename As String = GetArg("target")
        Dim useRecursiveReferencing As Boolean = True

        If inputFilename = "" Then
            Console.WriteLine("source file not specified")
            Return
        Else
            Console.WriteLine("Input Filename: " & inputFilename)
        End If
        If outputFilename = "" Then
            Console.WriteLine("target file not specified")
            Return
        Else
            Console.WriteLine("Output Filename: " & outputFilename)
        End If

        'Process command line options
        Dim o As New Options
        o.UseRecursiveReferencing = True
        Dim strRecursive As String = GetArg("recursive")
        If strRecursive <> "" Then
            o.UseRecursiveReferencing = [Boolean].Parse(strRecursive)
        End If
        Try
            Dim s As New Script(inputFilename, o)
            s.Process(outputFilename)
        Catch ex As Exception
            Console.WriteLine("Fatal error:")
            Console.WriteLine(ex.ToString())
        End Try
    End Sub

    Private Shared Function GetArg(ByVal name As String) As String
        Dim args() As String = Environment.GetCommandLineArgs()
        Dim action As String = "-" & name.ToLower() & "="
        For i As Integer = 0 To args.Length - 1
            If args(i).ToLower().StartsWith(action) Then
                Return args(i).Substring(action.Length)
            End If
        Next
        Return ""
    End Function

    Private Shared Function GetFlag(ByVal name As String) As Boolean
        Dim args() As String = Environment.GetCommandLineArgs()
        Dim action As String = "-" & name.ToLower()
        For i As Integer = 0 To args.Length - 1
            If args(i).ToLower().StartsWith(action) Then Return True
        Next
        Return False
    End Function

    'Private Shared Function GetAllFiles(ByVal searchPath As String) As String()
    '    Dim tmpFiles As New ArrayList
    '    tmpFiles.AddRange(Directory.GetFiles(searchPath))
    '    Dim dirList() As String = Directory.GetDirectories(searchPath)
    '    For i As Integer = 0 To dirList.Length - 1
    '        tmpFiles.AddRange(GetAllFiles(dirList(i)))
    '    Next
    '    Return tmpFiles.ToArray(GetType(String))
    'End Function
End Class
