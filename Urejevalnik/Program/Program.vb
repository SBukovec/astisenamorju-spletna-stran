Imports System
Imports System.Windows.Forms

Friend Module Program
    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Try
            Application.Run(New MainForm())
        Catch ex As OperationCanceledException
            ' Preklic izbire mape.
        Catch ex As Exception
            MessageBox.Show(ex.Message,"Napaka pri zagonu",MessageBoxButtons.OK,MessageBoxIcon.Error)
        End Try
    End Sub
End Module
