Imports System.Text.RegularExpressions
Imports System.Net
Imports System.IO
Imports System.Threading

Public Class scraper

    Public Shared scraped As List(Of String) = New List(Of String)
    Dim sources As List(Of String) = New List(Of String)
    Dim listLock As Object = New Object
    Dim isRunning As Boolean = False
    Dim thrdCnt As Integer = 50
    Dim d As New Dictionary(Of String, Thread)()

    Public Sub New()

    End Sub

    Public Sub New(thrd As Integer, psrc As List(Of String))
        thrdCnt = thrd
        sources = psrc
    End Sub

    Private Function scrapeLink(link As String) As List(Of String)
        Dim proxies As List(Of String) = New List(Of String)
        Dim Temp As String
        Try
            Dim r As HttpWebRequest = HttpWebRequest.Create(link)
            r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
            r.Timeout = 15000
            Dim re As HttpWebResponse = r.GetResponse()
            Dim rs As Stream = re.GetResponseStream
            Using sr As New StreamReader(rs)
                Temp = sr.ReadToEnd()
            End Using
            Dim src = Temp
            rs.Dispose()
            rs.Close()
            r.Abort()

            proxies = extractProx(src)

        Catch ex As Exception

        End Try

        Return proxies
    End Function

    Private Function extractProx(http As String) As List(Of String)
        Dim output As List(Of String) = New List(Of String)

        For Each proxy As Match In Regex.Matches(http, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+")
            output.Add(proxy.ToString())
        Next
        Return output
    End Function

    Private Sub startThreads()
        For int As Integer = 1 To thrdCnt Step 1
            d(int.ToString) = New Thread(AddressOf threadedProxyScraper)
            d(int.ToString).IsBackground = True
            d(int.ToString).Start()
        Next
    End Sub

    Private Sub threadedProxyScraper()
        Dim source As String
        While isRunning
            If sources.Count() = 0 Then
                Exit While
            End If
            SyncLock listLock
                source = sources.Item(0)
                sources.RemoveAt(0)
            End SyncLock
            'do work
            Dim tmp = (scraped.Count() - 1)

            For Each str As String In scrapeLink(source)
                scraped.Add(str)
            Next
        End While
        'check for job completion
        If thrdCnt = 1 Then
            isRunning = False
        End If
        thrdCnt -= 1
    End Sub

    Private Sub loadSrc()
        Dim client As WebClient = New WebClient()
        Dim reader As StreamReader = New StreamReader(client.OpenRead("https://pastebin.com/raw/KhiFJE4m"))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        sources = tmpSrc.ToList()
        sources.RemoveAt(0)
    End Sub

    Public Function scrape() As List(Of String)
        loadSrc()
        startThreads()
        While thrdCnt > 0
            System.Threading.Thread.Sleep(100)
        End While
        Return scraped
    End Function
End Class