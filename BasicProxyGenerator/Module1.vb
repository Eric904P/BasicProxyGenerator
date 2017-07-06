Imports System.IO
Imports System.Windows.Forms
Imports System.Threading

Module Module1
    Dim scraper As New scraper
    Dim checker As New checker
    Private scrpr As New Thread(AddressOf scraper.scrape)
    Private chckr As New Thread(AddressOf checker.check)
    'runs the scraping and checking as 2 separate threads for faster functionality

    'Save dialogue
    Private Sub saveFile(tempL As List(Of String))
        If (tempL.Count() > 0) Then
            Dim fs As New SaveFileDialog
            fs.RestoreDirectory = True
            fs.Filter = "txt files (*.txt)|*.txt"
            fs.FilterIndex = 1
            fs.ShowDialog()
            If Not (fs.FileName = Nothing) Then
                Using sw As New StreamWriter(fs.FileName)
                    For Each line As String In tempL
                        sw.WriteLine(line)
                    Next
                End Using
            End If
        Else
            Console.WriteLine("Please wait until program is finished.")
        End If
    End Sub

    'starts the scraper and checker
    Private Sub startThreads()
        scrpr.IsBackground = True
        chckr.IsBackground = True
        scrpr.Start()
        chckr.Start()
    End Sub

    Sub Main()
        Console.Clear()
        checker.check(scraper.scrape())
        While checker.checkRunning And checker.scraped.Count > 0 And scraper.srcListCnt > 0 'idles while program runs
            If checker.scraped.Count = 0 And scraper.srcListCnt = 0 Then 'checks for completion, even if the threads are stuck
                Thread.Sleep(5000)
                Exit While
            End If
            Thread.Sleep(1000)
        End While
        'notifies user of completion
        Console.WriteLine("Program finished!")
        Console.WriteLine(checker.workingCount & " working proxies found")
        Console.WriteLine("Press enter to save...")
        Console.ReadLine()

        saveFile(checker.returnWorking()) 'save dialogue for working proxies

        Console.Clear()
        Console.WriteLine("Press any key to exit...")
        Console.ReadLine()
    End Sub

    'main thread of this program, scrapes and checks proxies
    'Sub Main()
    '    Console.Clear()
    '    startThreads()
    '    While checker.checkRunning And checker.scraped.Count > 0 And scraper.srcListCnt > 0 'idles while program runs
    '        If checker.scraped.Count = 0 And scraper.srcListCnt = 0 Then 'checks for completion, even if the threads are stuck
    '            Thread.Sleep(5000)
    '            Exit While
    '        End If
    '        Thread.Sleep(1000)
    '    End While
    '    'notifies user of completion
    '    Console.WriteLine("Program finished!")
    '    Console.WriteLine(checker.workingCount & " working proxies found")
    '    Console.WriteLine("Press enter to save...")
    '    Console.ReadLine()

    '    saveWorking(checker.returnWorking()) 'save dialogue for working proxies

    '    Console.Clear()
    '    Console.WriteLine("Press any key to exit...")
    '    Console.ReadLine()
    'End Sub

End Module
