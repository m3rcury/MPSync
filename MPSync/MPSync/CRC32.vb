Imports System.IO

Module CRC32

    Public CRC32Table(256) As Integer
    Private buffer_size As Integer = 102400

    Public Sub createCRC32table()

        Dim DWPolynomial As Integer = &HEDB88320
        Dim DWCRC, i, j As Integer

        'Create CRC32 Table
        For i = 0 To 255

            DWCRC = i

            For j = 8 To 1 Step -1
                If (DWCRC And 1) Then
                    DWCRC = ((DWCRC And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                    DWCRC = DWCRC Xor DWPolynomial
                Else
                    DWCRC = ((DWCRC And &HFFFFFFFE) \ 2&) And &H7FFFFFFF
                End If
            Next j

            CRC32Table(i) = DWCRC

        Next i

    End Sub

    Public Function fileCRC32(ByVal path As String) As String
        Try
            Dim fileinfo As System.IO.FileInfo = My.Computer.FileSystem.GetFileInfo(path)
            Dim byteValue() As Byte
            Dim CRC32 As Integer = &HFFFFFFFF
            Dim i, n As Integer

            If fileinfo.Length <= buffer_size Then
                byteValue = System.IO.File.ReadAllBytes(path)
            Else
                byteValue = readpartfile(path, fileinfo.Length)
            End If

            For i = 0 To byteValue.Length - 1
                n = (CRC32 And &HFF) Xor byteValue(i)
                CRC32 = ((CRC32 And &HFFFFFF00) \ &H100) And &HFFFFFF
                CRC32 = CRC32 Xor CRC32Table(n)
            Next

            Return Hex(Not (CRC32))

        Catch ex As Exception
            Return String.Empty

        End Try

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
