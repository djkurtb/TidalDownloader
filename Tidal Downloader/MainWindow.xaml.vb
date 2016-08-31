Imports System.IO
Imports System.Windows.Threading
Imports MahApps.Metro.Controls

Partial Public Class MainWindow
    Dim time As Integer
    Dim conTime As Integer
    Dim state As String = "idle"
    Dim timercount As Integer = 0
    Dim Timer1 As New DispatcherTimer
    Dim Timer2 As New DispatcherTimer

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Loaded
        AddHandler Timer1.Tick, AddressOf Timer1_Tick
        Timer1.Interval = New TimeSpan(0, 0, 1)
        AddHandler Timer2.Tick, AddressOf Timer2_Tick
        Timer2.Interval = New TimeSpan(0, 0, 1)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If state = "idle" Then
            Timer2.Start()
            state = "countdown"
        ElseIf state = "record" Then
            Timer1.Stop()
            Dim proc = Process.GetProcessesByName("fmedia")
            For i As Integer = 0 To proc.Count - 1
                proc(i).Kill()
            Next i
            state = "done"
            Title = "Recording Complete!"
            time = 0
            conTime = 0
            Button1.Content = "Clear"
        ElseIf state = "done" Then
            Button1.Content = "Record"
            ProgressBar1.Value = 0
            TextBox1.IsEnabled = True
            DateTimePicker1.IsEnabled = True
            Title = "Tidal Recorder"
            state = "idle"
        ElseIf state = "countdown" Then
            Timer2.Stop()
            Button1.Content = "Record"
            timercount = 0
            state = "idle"
        End If
    End Sub

    Public Function convertTime(fTime As String)
        Dim tArray As Integer() = {Integer.Parse(fTime.Split(":")(1)), Integer.Parse(fTime.Split(":")(2))}
        Return (tArray(0) * 60 + tArray(1))
    End Function
    Private Sub Timer1_Tick(sender As Object, e As EventArgs)
        time += 1
        ProgressBar1.Value += 1
        Dim path As String = "C:\TidalRips\" & TextBox1.Text & ".flac"
        Title = "Recording... " & TimeSpan.FromSeconds(time).ToString.Substring(3) & "/" & DateTimePicker1.Text.Substring(3) & ", " & Math.Round((New FileInfo(path).Length / 1024000), 2) & " MB"
        If time = conTime Then
            Beep()
            Title = "Recording Complete!"
            time = 0
            conTime = 0
            Button1.Content = "Clear"
            state = "done"
            Timer1.Stop()
        End If
    End Sub

    Public Sub runMedia()
        Title = "Recording... 00:00/" & DateTimePicker1.Text.Substring(3) & ", 0.00 MB"
        Dim p As New ProcessStartInfo(System.AppDomain.CurrentDomain.BaseDirectory & "lib\fmedia.exe", "--record --out=""C:\TidalRips\" & TextBox1.Text & ".flac"" --dev-capture=" & My.Settings.capDevice & " --until=" & DateTimePicker1.Text)
        p.WindowStyle = ProcessWindowStyle.Hidden
        p.CreateNoWindow = True
        Process.Start(p)
        Dim keyboard As New HenoohDeviceEmulator.KeyboardController
        keyboard.Type(HenoohDeviceEmulator.Native.VirtualKeyCode.SPACE)
        Button1.Content = ""
        conTime = convertTime(DateTimePicker1.Text)
        ProgressBar1.Maximum = conTime
        TextBox1.IsEnabled = False
        DateTimePicker1.IsEnabled = False
        Button1.Content = "Stop"
        timercount = 0
    End Sub
    Private Sub Timer2_Tick(sender As Object, e As EventArgs)

        If timercount <= 5 Then
            Button1.Content = 5 - timercount
            timercount += 1
        Else
            state = "record"
            runMedia()
            Timer1.Start()
            Timer2.Stop()
        End If
    End Sub

    Private Sub Label1_DoubleClick(sender As Object, e As EventArgs) Handles label.MouseDoubleClick
        Process.Start("C:\TidalRips")
    End Sub

    Private Sub MetroWindow_Closed(sender As Object, e As EventArgs)
        Dim proc = Process.GetProcessesByName("fmedia")
        For i As Integer = 0 To proc.Count - 1
            proc(i).Kill()
        Next i
    End Sub

    Private Sub Button1_MouseRightButtonUp(sender As Object, e As MouseButtonEventArgs) Handles Button1.MouseRightButtonUp
        Dim oProcess As New Process()
        Dim oStartInfo As New ProcessStartInfo(System.AppDomain.CurrentDomain.BaseDirectory & "lib\fmedia.exe", "--list-dev")
        oStartInfo.UseShellExecute = False
        oStartInfo.RedirectStandardOutput = True
        oProcess.StartInfo = oStartInfo
        oProcess.Start()

        Dim sOutput As String
        Using oStreamReader As System.IO.StreamReader = oProcess.StandardOutput
            sOutput = oStreamReader.ReadToEnd()
        End Using

        My.Settings.capDevice = InputBox(sOutput.Substring(sOutput.IndexOf("Capture:")).Remove(0, 9), "Choose input device...", My.Settings.capDevice)
    End Sub

End Class
