Imports System.IO
Imports System.Security.Cryptography

Module MD5

    Private buffer_size As Integer = 102400

    Public Function fileMD5(ByVal path As String) As String

        Dim provider As MD5CryptoServiceProvider
        Dim byteValue() As Byte
        Dim bytHash() As Byte
        Dim md5 As String = String.Empty
        Dim i As Integer

        provider = New MD5CryptoServiceProvider()

        Dim fileinfo As System.IO.FileInfo = My.Computer.FileSystem.GetFileInfo(path)

        If fileinfo.Length <= buffer_size Then
            byteValue = System.IO.File.ReadAllBytes(path)
        Else
            byteValue = readpartfile(path, fileinfo.Length)
        End If

        bytHash = provider.ComputeHash(byteValue)

        provider.Clear()

        For i = 0 To bytHash.Length - 1
            md5 &= bytHash(i).ToString("x").PadLeft(2, "0")
        Next

        Return md5

    End Function

    Private Function readpartfile(ByVal path As String, ByVal size As Long) As Byte()

        Dim buffer() As Byte = New Byte(buffer_size - 1) {}

        Try

            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)
                fs.Seek(size - buffer.Length - 1, 0)
                fs.Read(buffer, 0, buffer.Length)
                fs.Close()
            End Using

        Catch ex As Exception
        End Try

        Return buffer

    End Function

End Module
