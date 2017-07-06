Imports System.Net
Imports System.IO
Imports System.Threading

Public Class checker
    Public Shared scraped As List(Of String) = New List(Of String)
    Dim working As New List(Of String)
    Dim thrdCnt As Integer = 100
    Dim d As New Dictionary(Of String, Thread)()
    Private curProxLock As Object = New Object
    Dim isRunning As Boolean = False
    Dim tempScraper As New scraper

    'default constructor
    Public Sub New()
        Console.WriteLine("Default checker created")
    End Sub

    'constructor with custom paramters
    Public Sub New(threadCount As Integer, proxyToCheck As List(Of String))
        Console.WriteLine("Custom checker created with " & threadCount & " threads")
        thrdCnt = threadCount
        scraped = proxyToCheck
    End Sub

    'test single proxy
    Function checkProxy(proxy As String) As Boolean
        Dim myProxy As WebProxy
        Dim Temp As String
        Try 'uses azenv.net proxy judge
            myProxy = New WebProxy(proxy)
            Dim r As HttpWebRequest = HttpWebRequest.Create("http://azenv.net")
            r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36"
            r.Timeout = 3000
            r.Proxy = myProxy
            Dim re As HttpWebResponse = r.GetResponse()
            Dim rs As Stream = re.GetResponseStream
            Using sr As New StreamReader(rs)
                Temp = sr.ReadToEnd()
            End Using
            Dim Text = Temp
            rs.Dispose()
            rs.Close()
            r.Abort()
            'success conditions
            If Text.Contains("HTTP_HOST = azenv.net") Then
                If Text.Contains("REQUEST_TIME =") Then
                    Return True
                End If
            Else
                Return False
            End If
        Catch ex As Exception

        End Try
        Return False
    End Function

    'thread action
    Private Sub threadedProxyChecker()
        Dim proxy As String
        While isRunning
            'checks if there are any more to check, and if the scraper is still processing sources
            If scraped.Count() = 0 And Not tempScraper.checkRunning Then
                Exit While
            End If
            SyncLock curProxLock
                proxy = scraped.Item(0) 'adds first proxy to temp var
                scraped.RemoveAt(0) 'removes that item from the list
            End SyncLock
            If (checkProxy(proxy)) Then
                working.Add(proxy)
                Console.WriteLine(proxy) 'writes working proxy to console
            End If
        End While
        If thrdCnt = 1 Then
            isRunning = False
        End If
        thrdCnt -= 1
    End Sub

    'main function of this class, returns working proxies
    Public Function check() As List(Of String)
        startThreads()
        Console.WriteLine("Checker started")
        While thrdCnt > 0
            If scraped.Count = 0 And thrdCnt < 5 Then
                System.Threading.Thread.Sleep(5000)
                Exit While
            End If
            System.Threading.Thread.Sleep(100)
        End While
        Return working
    End Function

    'main fuction of this class, given a list of proxies as string, returns working proxies
    Public Function check(proxies As List(Of String)) As List(Of String)
        scraped = proxies
        Console.WriteLine(scraped.Count & " proxies to check")
        startThreads()
        Console.WriteLine("Checker started")
        While thrdCnt > 0
            If scraped.Count = 0 And thrdCnt < 5 Then
                System.Threading.Thread.Sleep(5000)
                Exit While
            End If
            System.Threading.Thread.Sleep(100)
        End While
        Return working
    End Function

    'thread controller
    Private Sub startThreads()
        isRunning = True 'sets isRunning flag
        thrdCnt = 100

        For int As Integer = 1 To thrdCnt Step 1
            d(int.ToString) = New Thread(AddressOf threadedProxyChecker)
            d(int.ToString).IsBackground = True
            d(int.ToString).Start()
            Console.WriteLine("Thread " & int & " started on checker")
        Next
    End Sub

    'returns working, allows working to remain private
    Public Function returnWorking() As List(Of String)
        Return working
    End Function

    'returns count of proxies left to check
    Public Function scrapedListCount() As Integer
        Return scraped.Count
    End Function

    'returns count of working proxies
    Public Function workingCount() As Integer
        Return working.Count
    End Function

    'checks if threads are running
    Public Function checkRunning() As Boolean
        Return isRunning
    End Function
End Class
