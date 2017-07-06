Imports System.Text.RegularExpressions
Imports System.Net
Imports System.IO
Imports System.Threading
Imports System.Reflection

Public Class scraper
    Public Shared scraped As List(Of String) = New List(Of String)
    Dim sources As List(Of String) = New List(Of String)
    Dim working As List(Of String) = New List(Of String)
    Dim listLock As Object = New Object
    Dim isRunning As Boolean = False
    Dim thrdCnt As Integer = 50
    Dim d As New Dictionary(Of String, Thread)()

    'for loading from embedded resources
    Dim _assembly As [Assembly]
    Dim _textStreamReader As StreamReader

    'default constructor
    Public Sub New()
        Console.WriteLine("Default scraper created")
    End Sub

    'constructor with specific params
    Public Sub New(threadCount As Integer, sourceList As List(Of String))
        thrdCnt = threadCount
        sources = sourceList
        Console.WriteLine("Custom scraper created")
    End Sub

    'scrapes a single link
    Private Function scrapeLink(link As String) As List(Of String)
        Dim proxies As List(Of String) = New List(Of String)
        Dim Temp As String
        Try 'gets the entire web page as a string
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
            'uses extractProx method to find all proxies on the webpage
            proxies = extractProx(src)

            If proxies.Count > 0 Then
                Console.WriteLine(proxies.Count & " proxies found at " & link)
                working.Add(link)
            End If
        Catch ex As Exception

        End Try
        'returns scraped result
        Return proxies
    End Function

    'finds all proxies in a given string, returns them as a List(Of String)
    Private Function extractProx(http As String) As List(Of String)
        Dim output As List(Of String) = New List(Of String)

        For Each proxy As Match In Regex.Matches(http, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+")
            output.Add(proxy.ToString())
        Next
        Return output
    End Function

    'thread controller
    Private Sub startThreads()
        thrdCnt = 50
        isRunning = True 'sets isRunning flag

        For int As Integer = 1 To thrdCnt Step 1
            d(int.ToString) = New Thread(AddressOf threadedProxyScraper)
            d(int.ToString).IsBackground = True
            d(int.ToString).Start()
            Console.WriteLine("Thread " & int & " started on scraper")
        Next
    End Sub

    'main sub for each thread
    Private Sub threadedProxyScraper()
        Dim source As String
        While isRunning
            SyncLock listLock
                If sources.Count() = 0 Then
                    Exit While 'checks if there are any more to scrape
                End If
                source = sources.Item(0) 'loads first entry in list to temp var
                sources.RemoveAt(0) 'removes first entry in list
            End SyncLock
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

    'loads all source links from internal resources
    Private Sub loadSrc()
        Dim psrc As String = BasicProxyGenerator.My.Resources.psrc
        sources = psrc.Split("$").ToList()
        If Not sources.Count > 0 Then
            loadSrcWeb()
        End If
        sources.RemoveAt(0)
    End Sub

    'fallback method, will remove to keep my sources safe
    Private Sub loadSrcWeb()
        Dim client As WebClient = New WebClient()
        Dim reader As StreamReader = New StreamReader(client.OpenRead("https://pastebin.com/raw/KhiFJE4m"))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        sources = tmpSrc.ToList()
        sources.RemoveAt(0)
    End Sub

    'Main function of this class, returns the scraped proxies
    Public Function scrape() As List(Of String)
        loadSrc()
        Console.WriteLine(sources.Count & " sources loaded")
        startThreads()
        Console.WriteLine("Scraper started")
        While thrdCnt > 0
            If sources.Count = 0 And thrdCnt < 5 Then
                System.Threading.Thread.Sleep(5000)
                Exit While
            End If
            System.Threading.Thread.Sleep(100)
        End While
        Return scraped
    End Function

    'Main function of this class, given a list of sources as string, returns scraped proxies
    Public Function scrape(src As List(Of String)) As List(Of String)
        sources = src
        Console.WriteLine(sources.Count & " sources loaded")
        startThreads()
        Console.WriteLine("Scraper started")
        While thrdCnt > 0
            If sources.Count = 0 And thrdCnt < 5 Then
                System.Threading.Thread.Sleep(5000)
                Exit While
            End If
            System.Threading.Thread.Sleep(100)
        End While
        Return scraped
    End Function

    'returns source list count
    Public Function srcListCnt() As Integer
        Return sources.Count
    End Function

    'returns scraped proxy count
    Public Function scrapedCount() As Integer
        Return scraped.Count()
    End Function

    'checks if threads are running
    Public Function checkRunning() As Boolean
        Return isRunning
    End Function

    'returns list of working sources - useful only if outside sources loaded
    'Public Function returnWorking() As List(Of String)
    '    Return working
    'End Function
End Class