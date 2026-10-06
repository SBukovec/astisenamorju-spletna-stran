Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.RegularExpressions
Imports System.Windows.Forms

Public Class MainForm
    Inherits Form

    Private ReadOnly fields As New Dictionary(Of String, Control)()
    Private ReadOnly websiteFolder As String
    Private ReadOnly contentFile As String
    Private ReadOnly statusLabel As New Label()
    Private ReadOnly galleryTable As New DataGridView()
    Private ReadOnly videoTable As New DataGridView()
    Private ReadOnly musicTable As New DataGridView()
    Private ReadOnly githubSettingsFile As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ASTISENAMORJU-CMS","github-folder.txt")
    Private lastSaveSucceeded As Boolean
    Private ReadOnly menuPanel As New TableLayoutPanel()
    Private ReadOnly menuItems As New List(Of Tuple(Of TextBox, TextBox))()

    Public Sub New()
        websiteFolder = FindWebsiteFolder()
        contentFile = Path.Combine(websiteFolder, "vsebina.js")
        BuildInterface()
        LoadContent()
    End Sub

    Private Shared Function BuildStamp() As String
        Try
            Dim exe = Path.Combine(AppContext.BaseDirectory, "AstisenamorjuUrejevalnik.exe")
            If File.Exists(exe) Then Return File.GetLastWriteTime(exe).ToString("yyyy-MM-dd HH:mm")
        Catch
        End Try
        Return "?"
    End Function

    Private Shared Function FindWebsiteFolder() As String
        Dim folder As New DirectoryInfo(AppContext.BaseDirectory)
        While folder IsNot Nothing
            If File.Exists(Path.Combine(folder.FullName, "index.html")) Then Return folder.FullName
            folder = folder.Parent
        End While
        Throw New DirectoryNotFoundException("Ne najdem mape spletne strani z datoteko index.html.")
    End Function

    Private Sub BuildInterface()
        Text = $"ASTIŠENAMORJU — CMS  ·  Verzija {BuildStamp()}"
        StartPosition = FormStartPosition.CenterScreen
        MinimumSize = New Size(900, 720)
        Size = New Size(1060, 860)
        BackColor = Color.FromArgb(240, 234, 223)
        Font = New Font("Segoe UI", 9.5F)
        Try
            Dim iconPath = Path.Combine(AppContext.BaseDirectory, "AstisenamorjuUrejevalnik.ico")
            If File.Exists(iconPath) Then Icon = New Icon(iconPath)
        Catch
        End Try

        Dim root As New TableLayoutPanel With {.Dock = DockStyle.Fill, .RowCount = 3, .ColumnCount = 1, .Padding = New Padding(18)}
        root.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        root.RowStyles.Add(New RowStyle(SizeType.AutoSize))

        Dim tabs As New TabControl With {.Dock = DockStyle.Fill}
        BuildGeneralTab(tabs)
        BuildHeroTab(tabs)
        BuildStoryTab(tabs)
        BuildMusicTab(tabs)
        BuildLiveTab(tabs)
        BuildGalleryTab(tabs)
        BuildVideoTab(tabs)
        BuildContactTab(tabs)
        BuildAppearanceTab(tabs)
        root.Controls.Add(tabs, 0, 0)

        Dim buttons As New FlowLayoutPanel With {.AutoSize = True, .FlowDirection = FlowDirection.LeftToRight, .Margin = New Padding(0, 14, 0, 8)}
        buttons.Controls.Add(MakeButton("Shrani spremembe", AddressOf SaveClicked, True))
        buttons.Controls.Add(MakeButton("Odpri predogled", AddressOf PreviewClicked, False))
        buttons.Controls.Add(MakeButton("Ponovno naloži", AddressOf ReloadClicked, False))
        buttons.Controls.Add(MakeButton("Odpri mapo Materiali", AddressOf MaterialsClicked, False))
        buttons.Controls.Add(MakeButton("Pripravi za GitHub", AddressOf PublishToGitHubClicked, False))
        buttons.Controls.Add(MakeButton("Nastavi GitHub mapo", AddressOf ChooseGitHubFolderClicked, False))
        root.Controls.Add(buttons, 0, 1)

        statusLabel.AutoSize = True
        statusLabel.ForeColor = Color.FromArgb(94, 87, 78)
        root.Controls.Add(statusLabel, 0, 2)
        Controls.Add(root)
    End Sub

    Private Function NewTab(tabs As TabControl, title As String) As TableLayoutPanel
        Dim page As New TabPage(title) With {.BackColor = Color.FromArgb(248, 245, 239), .Padding = New Padding(14)}
        Dim panel As New TableLayoutPanel With {.Dock = DockStyle.Fill, .AutoScroll = True, .ColumnCount = 2, .RowCount = 0}
        panel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 220))
        panel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        page.Controls.Add(panel) : tabs.TabPages.Add(page)
        Return panel
    End Function

    Private Sub BuildGeneralTab(t As TabControl)
        Dim p=NewTab(t,"Splošno in navigacija")
        AddField(p,"imeBenda","Ime benda")
        AddChoice(p,"headerMode","Prikaži napis imena benda",{"napis — besedilo","grafika — slika"})
        AddAsset(p,"imeGrafika","Grafika napisa (ime benda)","Slike|*.png;*.jpg;*.jpeg;*.webp;*.svg")
        AddChoice(p,"imeGrafikaVelikost","Velikost grafike napisa",{"50 — pol manjša","75 — malo manjša","100 — privzeta","125 — malo večja","150 — precej večja","200 — dvojna"})
        AddField(p,"imeGrafikaOdmikX","Položaj — premik desno (+px) / levo (−px)")
        AddField(p,"imeGrafikaOdmikY","Položaj — premik dol (+px) / gor (−px)")
        AddAsset(p,"logotip","Logotip","Slike|*.png;*.jpg;*.jpeg;*.webp;*.svg")
        AddField(p,"naslovStrani","Naslov zavihka brskalnika") : AddField(p,"metaOpis","Opis za iskalnike",True)
        
        ' Dinamični meniji
        Dim menuRow=p.RowCount : p.RowCount+=1
        Dim menuLabel As New Label With {.Text="Navigacija (meniji)",.AutoSize=True,.Anchor=AnchorStyles.Left,.Margin=New Padding(0,9,12,7)}
        p.Controls.Add(menuLabel,0,menuRow)
        
        menuPanel.Dock=DockStyle.Top : menuPanel.AutoSize=True : menuPanel.AutoSizeMode=AutoSizeMode.GrowAndShrink : menuPanel.ColumnCount=2 : menuPanel.RowCount=0
        menuPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent,100))
        menuPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent,100))
        p.Controls.Add(menuPanel,1,menuRow)
        
        Dim buttonRow=p.RowCount : p.RowCount+=1
        Dim menuButtons As New FlowLayoutPanel With {.AutoSize=True,.Dock=DockStyle.Top,.Margin=New Padding(0,8,0,8)}
        menuButtons.Controls.Add(MakeButton("Dodaj meni",AddressOf AddMenuItem,True))
        menuButtons.Controls.Add(MakeButton("Odstrani zadnji meni",AddressOf RemoveLastMenuItem,False))
        p.Controls.Add(menuButtons,1,buttonRow)
    End Sub
    
    Private Sub AddMenuItem(s As Object,e As EventArgs)
        Dim index=menuItems.Count+1
        Dim nameBox As New TextBox With {.Dock=DockStyle.Top,.Margin=New Padding(0,5,6,7),.Tag=$"menu{index}"}
        Dim linkBox As New TextBox With {.Dock=DockStyle.Top,.Margin=New Padding(0,5,6,7),.Tag=$"menu{index}Link"}
        
        Dim row=menuPanel.RowCount : menuPanel.RowCount+=1
        Dim nameLabel As New Label With {.Text=$"Meni {index} — naziv",.AutoSize=True,.Anchor=AnchorStyles.Left,.Margin=New Padding(0,9,12,7)}
        Dim linkLabel As New Label With {.Text=$"Meni {index} — povezava",.AutoSize=True,.Anchor=AnchorStyles.Left,.Margin=New Padding(0,9,12,7)}
        menuPanel.Controls.Add(nameLabel,0,row) : menuPanel.Controls.Add(nameBox,1,row)
        row=menuPanel.RowCount : menuPanel.RowCount+=1
        menuPanel.Controls.Add(linkLabel,0,row) : menuPanel.Controls.Add(linkBox,1,row)
        
        menuItems.Add(Tuple.Create(nameBox,linkBox))
        SetStatus($"Dodan meni {index}. Klikni Shrani spremembe.",False)
    End Sub
    
    Private Sub RemoveLastMenuItem(s As Object,e As EventArgs)
        If menuItems.Count=0 Then Return
        Dim last=menuItems.Last()
        menuPanel.Controls.Remove(last.Item1) : menuPanel.Controls.Remove(last.Item2)
        menuItems.Remove(last)
        SetStatus("Zadnji meni je odstranjen. Klikni Shrani spremembe.",False)
    End Sub

    Private Sub BuildHeroTab(t As TabControl)
        Dim p=NewTab(t,"Naslovni del")
        AddField(p,"eyebrow","Nadnaslov") : AddField(p,"naslov","Glavni naslov") : AddField(p,"poudarek","Barvni poudarek")
        AddField(p,"uvodniOpis","Uvodni opis",True) : AddAsset(p,"video","Naslovni video","Video|*.mp4;*.webm")
        AddAsset(p,"videoPoster","Slika pred videom","Slike|*.png;*.jpg;*.jpeg;*.webp")
        AddField(p,"videoPozicija","Položaj videa (npr. center top)")
        AddField(p,"heroGumb1","Prvi gumb") : AddField(p,"heroGumb1Link","Povezava prvega gumba")
        AddField(p,"heroGumb2","Drugi gumb") : AddField(p,"heroGumb2Link","Povezava drugega gumba") : AddField(p,"scrollTekst","Besedilo za pomik")
    End Sub

    Private Sub BuildStoryTab(t As TabControl)
        Dim p=NewTab(t,"Zgodba in zasedba")
        AddField(p,"storyIndex","Oznaka sekcije") : AddField(p,"storyTitle","Naslov sekcije",True)
        AddField(p,"zgodba1","Prvi odstavek",True) : AddField(p,"zgodba2","Drugi odstavek",True)
        AddAsset(p,"storySlika","Predstavitvena fotografija","Slike|*.png;*.jpg;*.jpeg;*.webp") : AddField(p,"storySlikaAlt","Opis fotografije") : AddField(p,"storySlikaPozicija","Položaj fotografije (npr. 50% 20%)")
        AddChoice(p,"storySlikaNacin","Vidni del fotografije",{"contain — pokaži celo fotografijo","cover — zapolni okvir z izrezom"}) : AddField(p,"storySlikaVelikost","Povečava v % (100 = brez dodatne povečave)")
        For i=1 To 4 : AddField(p,$"zasedba{i}",$"Zasedba {i}") : Next
    End Sub

    Private Sub BuildMusicTab(t As TabControl)
        Dim p=NewTab(t,"Glasba")
        AddField(p,"musicIndex","Oznaka sekcije") : AddField(p,"musicTitle","Naslov") : AddField(p,"musicDescription","Opis",True)
        Dim row=p.RowCount : p.RowCount+=1
        musicTable.Dock=DockStyle.Fill : musicTable.Height=300 : musicTable.AllowUserToAddRows=False : musicTable.AllowUserToDeleteRows=False
        musicTable.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill : musicTable.SelectionMode=DataGridViewSelectionMode.FullRowSelect : musicTable.MultiSelect=False
        musicTable.Columns.Add("naslov","Naslov skladbe") : musicTable.Columns.Add("opis","Opis") : musicTable.Columns.Add("link","Povezava")
        musicTable.Columns(0).FillWeight=40 : musicTable.Columns(1).FillWeight=30 : musicTable.Columns(2).FillWeight=30
        p.Controls.Add(musicTable,0,row) : p.SetColumnSpan(musicTable,2)
        row=p.RowCount : p.RowCount+=1
        Dim buttons As New FlowLayoutPanel With {.AutoSize=True,.Dock=DockStyle.Top,.Margin=New Padding(0,8,0,8)}
        buttons.Controls.Add(MakeButton("Dodaj skladbo",AddressOf AddSongRow,True))
        buttons.Controls.Add(MakeButton("Odstrani",AddressOf RemoveSongRow,False))
        buttons.Controls.Add(MakeButton("Premakni gor",AddressOf MoveSongUp,False))
        buttons.Controls.Add(MakeButton("Premakni dol",AddressOf MoveSongDown,False))
        p.Controls.Add(buttons,0,row) : p.SetColumnSpan(buttons,2)
        row=p.RowCount : p.RowCount+=1
        Dim help As New Label With {.AutoSize=True,.Text="Z dodajanjem lahko vneseš poljubno število skladb. Prazno povezavo pusti, če skladbe še ni mogoče poslušati.",.ForeColor=Color.FromArgb(94,87,78)}
        p.Controls.Add(help,0,row) : p.SetColumnSpan(help,2)
        AddField(p,"musicNote","Opomba")
    End Sub

    Private Sub AddSongRow(s As Object,e As EventArgs)
        Dim index=musicTable.Rows.Add("","","") : musicTable.ClearSelection() : musicTable.Rows(index).Selected=True
        SetStatus("Vpiši naslov, opis in povezavo skladbe ter shrani spremembe.",False)
    End Sub

    Private Sub RemoveSongRow(s As Object,e As EventArgs)
        If musicTable.SelectedRows.Count=1 Then musicTable.Rows.Remove(musicTable.SelectedRows(0))
    End Sub

    Private Sub MoveSongUp(s As Object,e As EventArgs)
        MoveSongRow(-1)
    End Sub

    Private Sub MoveSongDown(s As Object,e As EventArgs)
        MoveSongRow(1)
    End Sub

    Private Sub MoveSongRow(direction As Integer)
        If musicTable.SelectedRows.Count<>1 Then Return
        Dim row=musicTable.SelectedRows(0) : Dim target=row.Index+direction
        If target<0 OrElse target>=musicTable.Rows.Count Then Return
        Dim values={row.Cells(0).Value,row.Cells(1).Value,row.Cells(2).Value}
        For i=0 To 2 : row.Cells(i).Value=musicTable.Rows(target).Cells(i).Value : musicTable.Rows(target).Cells(i).Value=values(i) : Next
        musicTable.ClearSelection() : musicTable.Rows(target).Selected=True
    End Sub

    Private Sub BuildLiveTab(t As TabControl)
        Dim p=NewTab(t,"V živo")
        AddField(p,"liveIndex","Oznaka sekcije") : AddField(p,"liveTitle","Naslov",True) : AddField(p,"liveDescription","Opis",True)
        AddField(p,"liveLinkText","Besedilo povezave") : AddField(p,"liveLinkUrl","Povezava")
        AddAsset(p,"liveSlika","Koncertna fotografija","Slike|*.png;*.jpg;*.jpeg;*.webp") : AddField(p,"liveSlikaAlt","Opis fotografije") : AddField(p,"liveSlikaPozicija","Položaj fotografije (npr. center top)")
        AddChoice(p,"liveSlikaNacin","Vidni del fotografije",{"contain — pokaži celo fotografijo","cover — zapolni okvir z izrezom"}) : AddField(p,"liveSlikaVelikost","Povečava v % (100 = brez dodatne povečave)") : AddField(p,"liveCaption","Podpis fotografije")
    End Sub

    Private Sub BuildGalleryTab(t As TabControl)
        Dim p=NewTab(t,"Galerija")
        AddField(p,"galleryIndex","Oznaka sekcije") : AddField(p,"galleryTitle","Naslov") : AddField(p,"galleryDescription","Opis",True)
        Dim row=p.RowCount : p.RowCount+=1
        galleryTable.Dock=DockStyle.Fill : galleryTable.Height=340 : galleryTable.AllowUserToAddRows=False : galleryTable.AllowUserToDeleteRows=False
        galleryTable.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill : galleryTable.SelectionMode=DataGridViewSelectionMode.FullRowSelect : galleryTable.MultiSelect=False
        galleryTable.Columns.Add("slika","Datoteka") : galleryTable.Columns.Add("opis","Opis fotografije") : galleryTable.Columns.Add("pozicija","Položaj izreza")
        galleryTable.Columns.Add("velikost","Povečava %")
        Dim modeColumn As New DataGridViewComboBoxColumn With {.Name="nacin",.HeaderText="Vidni del",.DisplayStyle=DataGridViewComboBoxDisplayStyle.DropDownButton,.FlatStyle=FlatStyle.Flat}
        modeColumn.Items.AddRange("contain — cela fotografija","cover — zapolnjen okvir") : galleryTable.Columns.Add(modeColumn)
        galleryTable.Columns(0).FillWeight=30 : galleryTable.Columns(1).FillWeight=32 : galleryTable.Columns(2).FillWeight=16 : galleryTable.Columns(3).FillWeight=11 : galleryTable.Columns(4).FillWeight=11
        p.Controls.Add(galleryTable,0,row) : p.SetColumnSpan(galleryTable,2)
        row=p.RowCount : p.RowCount+=1
        Dim buttons As New FlowLayoutPanel With {.AutoSize=True,.Dock=DockStyle.Top,.Margin=New Padding(0,8,0,8)}
        buttons.Controls.Add(MakeButton("Dodaj fotografijo …",AddressOf AddGalleryImage,True))
        buttons.Controls.Add(MakeButton("Zamenjaj fotografijo …",AddressOf ReplaceGalleryImage,False))
        buttons.Controls.Add(MakeButton("Odstrani",AddressOf RemoveGalleryImage,False))
        buttons.Controls.Add(MakeButton("Premakni gor",AddressOf MoveGalleryUp,False))
        buttons.Controls.Add(MakeButton("Premakni dol",AddressOf MoveGalleryDown,False))
        p.Controls.Add(buttons,0,row) : p.SetColumnSpan(buttons,2)
        row=p.RowCount : p.RowCount+=1
        Dim help As New Label With {.AutoSize=True,.Text="Spletna stran fotografije dinamično razporedi v največ 4 stolpce in 2 vrstici; presežek prestavi na novo stran. Za cel posnetek izberi contain in povečavo 100.",.ForeColor=Color.FromArgb(94,87,78)}
        p.Controls.Add(help,0,row) : p.SetColumnSpan(help,2)
    End Sub

    Private Sub BuildVideoTab(t As TabControl)
        Dim p=NewTab(t,"Video")
        AddField(p,"videoIndex","Oznaka sekcije") : AddField(p,"videoTitle","Naslov") : AddField(p,"videoDescription","Opis",True)
        Dim row=p.RowCount : p.RowCount+=1
        videoTable.Dock=DockStyle.Fill : videoTable.Height=340 : videoTable.AllowUserToAddRows=False : videoTable.AllowUserToDeleteRows=False
        videoTable.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill : videoTable.SelectionMode=DataGridViewSelectionMode.FullRowSelect : videoTable.MultiSelect=False
        videoTable.Columns.Add("video","Video datoteka") : videoTable.Columns.Add("naslov","Naslov") : videoTable.Columns.Add("aktiven","Aktiven (da/ne)")
        videoTable.Columns(0).FillWeight=55 : videoTable.Columns(1).FillWeight=30 : videoTable.Columns(2).FillWeight=15
        p.Controls.Add(videoTable,0,row) : p.SetColumnSpan(videoTable,2)
        row=p.RowCount : p.RowCount+=1
        Dim buttons As New FlowLayoutPanel With {.AutoSize=True,.Dock=DockStyle.Top,.Margin=New Padding(0,8,0,8)}
        buttons.Controls.Add(MakeButton("Dodaj video …",AddressOf AddVideo,True))
        buttons.Controls.Add(MakeButton("Dodaj povezavo …",AddressOf AddVideoLink,False))
        buttons.Controls.Add(MakeButton("Zamenjaj video …",AddressOf ReplaceVideo,False))
        buttons.Controls.Add(MakeButton("Odstrani",AddressOf RemoveVideo,False))
        buttons.Controls.Add(MakeButton("Premakni gor",AddressOf MoveVideoUp,False))
        buttons.Controls.Add(MakeButton("Premakni dol",AddressOf MoveVideoDown,False))
        p.Controls.Add(buttons,0,row) : p.SetColumnSpan(buttons,2)
        row=p.RowCount : p.RowCount+=1
        Dim help As New Label With {.AutoSize=True,.Text="Video dodajaš iz računalnika (mp4/webm); urejevalnik ga kopira v mapo assets. V stolpcu »Aktiven« vpiši da ali ne. Na spletni strani se videoposnetki prikažejo v mreži.",.ForeColor=Color.FromArgb(94,87,78)}
        p.Controls.Add(help,0,row) : p.SetColumnSpan(help,2)
    End Sub

    Private Function CopyVideoFile() As String
        Using d As New OpenFileDialog With {.Title="Izberi video za video-galerijo",.Filter="Video|*.mp4;*.webm|Vse datoteke|*.*"}
            If d.ShowDialog(Me)<>DialogResult.OK Then Return ""
            Dim assets=Path.Combine(websiteFolder,"assets") : Directory.CreateDirectory(assets)
            Dim name="video-" & DateTime.Now.ToString("yyyyMMdd-HHmmssfff") & Path.GetExtension(d.FileName).ToLowerInvariant()
            File.Copy(d.FileName,Path.Combine(assets,name),True)
            Return "assets/" & name
        End Using
    End Function

    Private Sub AddVideo(s As Object,e As EventArgs)
        Try
            Dim path=CopyVideoFile() : If path="" Then Return
            Dim index=videoTable.Rows.Add(path,"","da") : videoTable.ClearSelection() : videoTable.Rows(index).Selected=True
            SetStatus("Video je dodan. Vpiši naslov in shrani spremembe.",False)
        Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri dodajanju videa",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
    End Sub

    Private Sub AddVideoLink(s As Object,e As EventArgs)
        Try
            Dim url=PromptForUrl()
            If url="" Then Return
            Dim index=videoTable.Rows.Add(url,"","da") : videoTable.ClearSelection() : videoTable.Rows(index).Selected=True
            SetStatus("Video povezava je dodana. Vpiši naslov in shrani spremembe.",False)
        Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri dodajanju videopovezave",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
    End Sub

    Private Function PromptForUrl() As String
        Using d As New Form With {.Text="Vnesi povezavo do videa",.StartPosition=FormStartPosition.CenterParent,.FormBorderStyle=FormBorderStyle.FixedDialog,.MaximizeBox=False,.MinimizeBox=False,.Width=480,.Height=170}
            Dim label As New Label With {.Text="Prilepi povezavo (YouTube, OneDrive, Google Drive, mp4/webm …):",.AutoSize=True,.Location=New Point(20,18)}
            Dim input As New TextBox With {.Location=New Point(20,48),.Width=420,.Tag="url"}
            Dim ok As New Button With {.Text="V redu",.DialogResult=DialogResult.OK,.Location=New Point(280,92),.AutoSize=True}
            Dim cancel As New Button With {.Text="Prekliči",.DialogResult=DialogResult.Cancel,.Location=New Point(360,92),.AutoSize=True}
            d.Controls.Add(label) : d.Controls.Add(input) : d.Controls.Add(ok) : d.Controls.Add(cancel)
            d.AcceptButton=ok : d.CancelButton=cancel
            If d.ShowDialog(Me)=DialogResult.OK Then Return input.Text.Trim()
        End Using
        Return ""
    End Function

    Private Sub ReplaceVideo(s As Object,e As EventArgs)
        If videoTable.SelectedRows.Count<>1 Then Return
        Try
            Dim path=CopyVideoFile() : If path<>"" Then videoTable.SelectedRows(0).Cells(0).Value=path
        Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri zamenjavi videa",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
    End Sub

    Private Sub RemoveVideo(s As Object,e As EventArgs)
        If videoTable.SelectedRows.Count=1 Then videoTable.Rows.Remove(videoTable.SelectedRows(0))
    End Sub

    Private Sub MoveVideoUp(s As Object,e As EventArgs)
        MoveVideoRow(-1)
    End Sub

    Private Sub MoveVideoDown(s As Object,e As EventArgs)
        MoveVideoRow(1)
    End Sub

    Private Sub MoveVideoRow(direction As Integer)
        If videoTable.SelectedRows.Count<>1 Then Return
        Dim row=videoTable.SelectedRows(0) : Dim target=row.Index+direction
        If target<0 OrElse target>=videoTable.Rows.Count Then Return
        Dim values={row.Cells(0).Value,row.Cells(1).Value,row.Cells(2).Value}
        For i=0 To 2 : row.Cells(i).Value=videoTable.Rows(target).Cells(i).Value : videoTable.Rows(target).Cells(i).Value=values(i) : Next
        videoTable.ClearSelection() : videoTable.Rows(target).Selected=True
    End Sub

    Private Sub BuildContactTab(t As TabControl)
        Dim p=NewTab(t,"Kontakt in noga")
        AddField(p,"contactIndex","Oznaka sekcije") : AddField(p,"contactTitle","Naslov") : AddField(p,"contactHighlight","Barvni poudarek")
        AddField(p,"contactDescription","Opis",True) : AddField(p,"kontakt","E-poštni naslov")
        For Each k In {"facebook","instagram","youtube"} : AddField(p,k & "Text",Char.ToUpper(k(0)) & k.Substring(1) & " — naziv") : AddField(p,k & "Url",Char.ToUpper(k(0)) & k.Substring(1) & " — povezava") : Next
        AddField(p,"footerLeft","Noga levo") : AddField(p,"footerRight","Noga desno")
    End Sub

    Private Sub BuildAppearanceTab(t As TabControl)
        Dim p=NewTab(t,"Barve")
        AddColor(p,"barvaOzadja","Temno ozadje") : AddColor(p,"barvaPapirja","Svetlo ozadje") : AddColor(p,"barvaPoudarka","Poudarjena barva")
        AddColor(p,"glavaBarvaOzadja","Trak ozadje (prosojno ali barva)") : AddColor(p,"glavaBarvaBesedila","Trak barva menijev in napisa")
    End Sub

    Private Sub AddField(p As TableLayoutPanel,key As String,labelText As String,Optional multiline As Boolean=False)
        Dim row=p.RowCount : p.RowCount+=1
        Dim l As New Label With {.Text=labelText,.AutoSize=True,.Anchor=AnchorStyles.Left,.Margin=New Padding(0,9,12,7)}
        Dim b As New TextBox With {.Dock=DockStyle.Top,.Margin=New Padding(0,5,6,7),.Tag=key}
        If multiline Then b.Multiline=True : b.Height=64 : b.ScrollBars=ScrollBars.Vertical
        fields(key)=b : p.Controls.Add(l,0,row) : p.Controls.Add(b,1,row)
    End Sub

    Private Sub AddChoice(p As TableLayoutPanel,key As String,labelText As String,choices As String())
        Dim row=p.RowCount : p.RowCount+=1
        Dim l As New Label With {.Text=labelText,.AutoSize=True,.Anchor=AnchorStyles.Left,.Margin=New Padding(0,9,12,7)}
        Dim b As New ComboBox With {.Dock=DockStyle.Top,.Margin=New Padding(0,5,6,7),.DropDownStyle=ComboBoxStyle.DropDownList,.Tag=key}
        b.Items.AddRange(choices) : fields(key)=b : p.Controls.Add(l,0,row) : p.Controls.Add(b,1,row)
    End Sub

    Private Sub AddAsset(p As TableLayoutPanel,key As String,labelText As String,filter As String)
        Dim row=p.RowCount : p.RowCount+=1
        p.Controls.Add(New Label With {.Text=labelText,.AutoSize=True,.Anchor=AnchorStyles.Left,.Margin=New Padding(0,9,12,7)},0,row)
        Dim holder As New TableLayoutPanel With {.Dock=DockStyle.Top,.ColumnCount=2,.AutoSize=True,.Margin=New Padding(0,5,6,7)}
        holder.ColumnStyles.Add(New ColumnStyle(SizeType.Percent,100)) : holder.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        Dim b As New TextBox With {.Dock=DockStyle.Fill,.ReadOnly=True,.Tag=key} : fields(key)=b : holder.Controls.Add(b,0,0)
        Dim choose=MakeButton("Izberi …",Sub(s,e) ChooseAsset(key,filter),False) : holder.Controls.Add(choose,1,0) : p.Controls.Add(holder,1,row)
    End Sub

    Private Sub AddColor(p As TableLayoutPanel,key As String,labelText As String)
        Dim row=p.RowCount : p.RowCount+=1
        While p.RowStyles.Count<row+1 : p.RowStyles.Add(New RowStyle(SizeType.AutoSize)) : End While
        Dim cell As New Panel With {.Height=62,.Dock=DockStyle.Top,.Margin=New Padding(0,4,6,7)}
        cell.Controls.Add(New Label With {.Text=labelText,.AutoSize=True,.AutoEllipsis=True,.MaximumSize=New Size(340,0),.Location=New Point(0,2),.Anchor=AnchorStyles.Left})
        Dim b As New TextBox With {.Location=New Point(0,26),.Height=26,.Width=150,.Tag=key} : fields(key)=b : cell.Controls.Add(b)
        Dim btn=MakeButton("Izberi barvo …",Sub(s,e) ChooseColor(key),False) : btn.Location=New Point(158,22) : cell.Controls.Add(btn)
        p.Controls.Add(cell,0,row) : p.SetColumnSpan(cell,2)
    End Sub

    Private Shared Function MakeButton(caption As String,handler As EventHandler,primary As Boolean) As Button
        Dim b As New Button With {.Text=caption,.AutoSize=True,.Padding=New Padding(10,6,10,6),.Margin=New Padding(0,0,8,0),.FlatStyle=FlatStyle.Flat}
        If primary Then b.BackColor=Color.FromArgb(213,111,62) : b.ForeColor=Color.FromArgb(24,18,14) : b.FlatAppearance.BorderSize=0
        AddHandler b.Click,handler : Return b
    End Function

    Private Function ReadObject() As JsonObject
        Dim raw=File.ReadAllText(contentFile) : Dim a=raw.IndexOf("{"c) : Dim z=raw.LastIndexOf("}"c)
        If a<0 OrElse z<=a Then Throw New InvalidDataException("Datoteka vsebina.js ni veljavna.")
        Return JsonNode.Parse(raw.Substring(a,z-a+1)).AsObject()
    End Function

    Private Sub LoadContent()
        Try
            Dim data=ReadObject()
            For Each pair In fields
                Dim value=If(data(pair.Key)?.GetValue(Of String)(),"")
                If TypeOf pair.Value Is ComboBox Then
                    Dim combo=DirectCast(pair.Value,ComboBox)
                    combo.SelectedIndex=-1
                    For i=0 To combo.Items.Count-1
                        If combo.Items(i).ToString().StartsWith(value,StringComparison.OrdinalIgnoreCase) Then combo.SelectedIndex=i : Exit For
                    Next
                Else
                    pair.Value.Text=value
                End If
            Next
            
            ' Naloži dinamične menije
            menuPanel.Controls.Clear() : menuItems.Clear() : menuPanel.RowCount=0
            Dim menuIndex=1
            While True
                Dim menuKey=$"menu{menuIndex}"
                Dim linkKey=$"menu{menuIndex}Link"
                If Not data.ContainsKey(menuKey) Then Exit While
                
                Dim menuValue=If(data(menuKey)?.GetValue(Of String)(),"")
                Dim linkValue=If(data(linkKey)?.GetValue(Of String)(),"")
                
                Dim nameBox As New TextBox With {.Dock=DockStyle.Top,.Margin=New Padding(0,5,6,7),.Tag=menuKey,.Text=menuValue}
                Dim linkBox As New TextBox With {.Dock=DockStyle.Top,.Margin=New Padding(0,5,6,7),.Tag=linkKey,.Text=linkValue}
                
                Dim row=menuPanel.RowCount : menuPanel.RowCount+=1
                Dim nameLabel As New Label With {.Text=$"Meni {menuIndex} — naziv",.AutoSize=True,.Anchor=AnchorStyles.Left,.Margin=New Padding(0,9,12,7)}
                Dim linkLabel As New Label With {.Text=$"Meni {menuIndex} — povezava",.AutoSize=True,.Anchor=AnchorStyles.Left,.Margin=New Padding(0,9,12,7)}
                menuPanel.Controls.Add(nameLabel,0,row) : menuPanel.Controls.Add(nameBox,1,row)
                row=menuPanel.RowCount : menuPanel.RowCount+=1
                menuPanel.Controls.Add(linkLabel,0,row) : menuPanel.Controls.Add(linkBox,1,row)
                
                menuItems.Add(Tuple.Create(nameBox,linkBox))
                menuIndex+=1
            End While
            
            galleryTable.Rows.Clear()
            Dim gallery=data("galerija")?.AsArray()
            If gallery IsNot Nothing Then
                For Each item In gallery
                    Dim o=item.AsObject()
                    Dim mode=If(o("nacin")?.GetValue(Of String)(),"cover")
                    galleryTable.Rows.Add(If(o("slika")?.GetValue(Of String)(),""),If(o("opis")?.GetValue(Of String)(),""),If(o("pozicija")?.GetValue(Of String)(),"center center"),If(o("velikost")?.GetValue(Of String)(),"100"),If(mode="contain","contain — cela fotografija","cover — zapolnjen okvir"))
                Next
            End If
            SetStatus("Vsebina je naložena.",False)
            Call LoadVideoTable(data)
            Call LoadMusicTable(data)
        Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri nalaganju",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
    End Sub

    Private Sub LoadMusicTable(data As JsonObject)
        musicTable.Rows.Clear()
        Dim pesmi=data("pesmi")?.AsArray()
        If pesmi IsNot Nothing AndAlso pesmi.Count>0 Then
            For Each item In pesmi
                Dim o=item.AsObject()
                musicTable.Rows.Add(If(o("naslov")?.GetValue(Of String)(),""),If(o("opis")?.GetValue(Of String)(),""),If(o("link")?.GetValue(Of String)(),""))
            Next
        Else
            ' Stara oblika (skladba1, skladba1Opis, skladba1Link …) — za nazaj združljivo
            Dim i=1
            While data.ContainsKey($"skladba{i}")
                musicTable.Rows.Add(If(data($"skladba{i}")?.GetValue(Of String)(),""),If(data($"skladba{i}Opis")?.GetValue(Of String)(),""),If(data($"skladba{i}Link")?.GetValue(Of String)(),""))
                i+=1
            End While
        End If
    End Sub

    Private Sub LoadVideoTable(data As JsonObject)
        videoTable.Rows.Clear()
        Dim videos=data("videoteka")?.AsArray()
        If videos IsNot Nothing Then
            For Each item In videos
                Dim o=item.AsObject()
                videoTable.Rows.Add(If(o("video")?.GetValue(Of String)(),""),If(o("naslov")?.GetValue(Of String)(),""),If(o("aktiven")?.GetValue(Of String)(),"da"))
            Next
        End If
    End Sub

    Private Sub SaveClicked(sender As Object,e As EventArgs)
        lastSaveSucceeded=False
        Try
            Dim data=ReadObject()
            For Each pair In fields
                Dim value=pair.Value.Text.Trim()
                If TypeOf pair.Value Is ComboBox Then
                    Dim selected=pair.Value.Text.Trim()
                    value=selected.Split(New String() {" — "},StringSplitOptions.None)(0).Trim()
                End If
                data(pair.Key)=value
            Next
            
            ' Shrani dinamične menije
            Dim i=1
            For Each item In menuItems
                data($"menu{i}")=item.Item1.Text.Trim()
                data($"menu{i}Link")=item.Item2.Text.Trim()
                i+=1
            Next
            
            Dim gallery As New JsonArray()
            For Each row As DataGridViewRow In galleryTable.Rows
                Dim slika=Convert.ToString(row.Cells(0).Value).Trim()
                If slika<>"" Then gallery.Add(New JsonObject From {{"slika",slika},{"opis",Convert.ToString(row.Cells(1).Value).Trim()},{"pozicija",DefaultCell(row,2,"center center")},{"velikost",DefaultCell(row,3,"100")},{"nacin",If(DefaultCell(row,4,"cover").StartsWith("contain"),"contain","cover")}})
            Next
            data("galerija")=gallery
            Dim videoteka As New JsonArray()
            For Each row As DataGridViewRow In videoTable.Rows
                Dim vid=Convert.ToString(row.Cells(0).Value).Trim()
                If vid<>"" Then videoteka.Add(New JsonObject From {{"video",vid},{"naslov",Convert.ToString(row.Cells(1).Value).Trim()},{"aktiven",DefaultCell(row,2,"da")}})
            Next
            data("videoteka")=videoteka
            Dim pesmi As New JsonArray()
            For Each row As DataGridViewRow In musicTable.Rows
                Dim naslov=Convert.ToString(row.Cells(0).Value).Trim()
                If naslov<>"" Then pesmi.Add(New JsonObject From {{"naslov",naslov},{"opis",Convert.ToString(row.Cells(1).Value).Trim()},{"link",Convert.ToString(row.Cells(2).Value).Trim()}})
            Next
            data("pesmi")=pesmi
            Dim options As New JsonSerializerOptions With {.WriteIndented=True,.Encoder=JavaScriptEncoder.UnsafeRelaxedJsonEscaping}
            File.WriteAllText(contentFile,"window.VSEBINA = " & data.ToJsonString(options) & ";" & Environment.NewLine,New System.Text.UTF8Encoding(False))
            lastSaveSucceeded=True
            BumpCacheVersion()
            SetStatus("Vse spremembe so shranjene. Predogled se posodobi samodejno.",True)
        Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri shranjevanju",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
    End Sub

    Private Sub BumpCacheVersion()
        Try
            Dim indexFile=Path.Combine(websiteFolder,"index.html")
            If Not File.Exists(indexFile) Then Return
            Dim html=File.ReadAllText(indexFile)
            Dim m=Regex.Match(html,"v=(\d{8})([a-z])")
            If Not m.Success Then Return
            Dim datePart=m.Groups(1).Value : Dim letter=m.Groups(2).Value
            Dim nextLetter=If(letter="z","a",Chr(Asc(letter)+1))
            Dim newDate=If(nextLetter="a",DateTime.Now.ToString("yyyyMMdd"),datePart)
            html=html.Replace(m.Value,"v=" & newDate & nextLetter)
            File.WriteAllText(indexFile,html,New System.Text.UTF8Encoding(False))
        Catch
        End Try
    End Sub

    Private Function CopyGalleryFile() As String
        Using d As New OpenFileDialog With {.Title="Izberi fotografijo za galerijo",.Filter="Slike|*.png;*.jpg;*.jpeg;*.webp|Vse datoteke|*.*"}
            If d.ShowDialog(Me)<>DialogResult.OK Then Return ""
            Dim assets=Path.Combine(websiteFolder,"assets") : Directory.CreateDirectory(assets)
            Dim name="gallery-" & DateTime.Now.ToString("yyyyMMdd-HHmmssfff") & Path.GetExtension(d.FileName).ToLowerInvariant()
            File.Copy(d.FileName,Path.Combine(assets,name),True)
            Return "assets/" & name
        End Using
    End Function

    Private Sub AddGalleryImage(s As Object,e As EventArgs)
        Try
            Dim path=CopyGalleryFile() : If path="" Then Return
            Dim index=galleryTable.Rows.Add(path,"","center center","100","cover — zapolnjen okvir") : galleryTable.ClearSelection() : galleryTable.Rows(index).Selected=True
            SetStatus("Fotografija je dodana. Vpiši opis in shrani spremembe.",False)
        Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri dodajanju",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
    End Sub

    Private Sub ReplaceGalleryImage(s As Object,e As EventArgs)
        If galleryTable.SelectedRows.Count<>1 Then Return
        Try
            Dim path=CopyGalleryFile() : If path<>"" Then galleryTable.SelectedRows(0).Cells(0).Value=path
        Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri zamenjavi",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
    End Sub

    Private Sub RemoveGalleryImage(s As Object,e As EventArgs)
        If galleryTable.SelectedRows.Count=1 Then galleryTable.Rows.Remove(galleryTable.SelectedRows(0))
    End Sub

    Private Sub MoveGalleryUp(s As Object,e As EventArgs)
        MoveGalleryRow(-1)
    End Sub

    Private Sub MoveGalleryDown(s As Object,e As EventArgs)
        MoveGalleryRow(1)
    End Sub

    Private Sub MoveGalleryRow(direction As Integer)
        If galleryTable.SelectedRows.Count<>1 Then Return
        Dim row=galleryTable.SelectedRows(0) : Dim target=row.Index+direction
        If target<0 OrElse target>=galleryTable.Rows.Count Then Return
        Dim values={row.Cells(0).Value,row.Cells(1).Value,row.Cells(2).Value,row.Cells(3).Value,row.Cells(4).Value}
        For i=0 To 4 : row.Cells(i).Value=galleryTable.Rows(target).Cells(i).Value : galleryTable.Rows(target).Cells(i).Value=values(i) : Next
        galleryTable.ClearSelection() : galleryTable.Rows(target).Selected=True
    End Sub

    Private Shared Function DefaultCell(row As DataGridViewRow,index As Integer,defaultValue As String) As String
        Dim value=Convert.ToString(row.Cells(index).Value).Trim()
        Return If(value="",defaultValue,value)
    End Function

    Private Sub ChooseAsset(key As String,filter As String)
        Using d As New OpenFileDialog With {.Title="Izberi datoteko",.Filter=filter & "|Vse datoteke|*.*"}
            If d.ShowDialog(Me)<>DialogResult.OK Then Return
            Try
                Dim assets=Path.Combine(websiteFolder,"assets") : Directory.CreateDirectory(assets)
                Dim baseName=Regex.Replace(key.ToLowerInvariant(),"[^a-z0-9]+","-").Trim("-"c)
                Dim dest=Path.Combine(assets,baseName & Path.GetExtension(d.FileName).ToLowerInvariant())
                If Not String.Equals(Path.GetFullPath(d.FileName),Path.GetFullPath(dest),StringComparison.OrdinalIgnoreCase) Then File.Copy(d.FileName,dest,True)
                fields(key).Text="assets/" & Path.GetFileName(dest) : SetStatus("Datoteka je pripravljena. Klikni »Shrani spremembe«.",False)
            Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri kopiranju",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
        End Using
    End Sub

    Private Sub ChooseColor(key As String)
        Try
            Using d As New ColorDialog With {.FullOpen=True}
                Dim current As Color=Color.Empty
                Try
                    Dim fixed=fields(key).Text.Trim()
                    If fixed.StartsWith("#") AndAlso Regex.IsMatch(fixed,"^#[0-9a-fA-F]{6}$") Then current=ColorTranslator.FromHtml(fixed)
                Catch
                End Try
                If Not current.IsEmpty Then d.Color=current
                If d.ShowDialog(Me)=DialogResult.OK Then fields(key).Text=ColorTranslator.ToHtml(d.Color) : SetStatus("Barva je izbrana. Klikni »Shrani spremembe«, da se uveljavi.",False)
            End Using
        Catch ex As Exception : MessageBox.Show(ex.Message,"Napaka pri izbiri barve",MessageBoxButtons.OK,MessageBoxIcon.Error) : End Try
    End Sub

    Private Sub PreviewClicked(s As Object,e As EventArgs)
        Process.Start(New ProcessStartInfo(Path.Combine(websiteFolder,"index.html")) With {.UseShellExecute=True})
    End Sub
    Private Sub ReloadClicked(s As Object,e As EventArgs)
        LoadContent()
    End Sub
    Private Sub MaterialsClicked(s As Object,e As EventArgs)
        Process.Start(New ProcessStartInfo(Path.Combine(websiteFolder,"Materiali")) With {.UseShellExecute=True})
    End Sub

    Private Function SavedGitHubFolder() As String
        Try
            If File.Exists(githubSettingsFile) Then
                Dim saved=File.ReadAllText(githubSettingsFile).Trim()
                If Directory.Exists(saved) Then Return saved
            End If
            Dim suggested=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"GitHub","astisenamorju-spletna-stran")
            If Directory.Exists(suggested) Then Return suggested
        Catch
        End Try
        Return ""
    End Function

    Private Function ChooseGitHubFolder() As String
        Using d As New FolderBrowserDialog With {.Description="Izberi lokalno mapo repozitorija astisenamorju-spletna-stran",.UseDescriptionForTitle=True,.ShowNewFolderButton=False}
            Dim current=SavedGitHubFolder() : If current<>"" Then d.SelectedPath=current
            If d.ShowDialog(Me)<>DialogResult.OK Then Return ""
            If Not Directory.Exists(Path.Combine(d.SelectedPath,".git")) Then
                MessageBox.Show("Izbrana mapa ni lokalni GitHub repozitorij. V GitHub Desktopu klikni »Show in Explorer« in izberi mapo, ki se odpre.","Napačna mapa",MessageBoxButtons.OK,MessageBoxIcon.Warning)
                Return ""
            End If
            Directory.CreateDirectory(Path.GetDirectoryName(githubSettingsFile))
            File.WriteAllText(githubSettingsFile,d.SelectedPath)
            Return d.SelectedPath
        End Using
    End Function

    Private Sub ChooseGitHubFolderClicked(s As Object,e As EventArgs)
        Dim folder=ChooseGitHubFolder()
        If folder<>"" Then SetStatus("GitHub mapa je nastavljena: " & folder,True)
    End Sub

    Private Sub PublishToGitHubClicked(s As Object,e As EventArgs)
        SaveClicked(s,e) : If Not lastSaveSucceeded Then Return
        Dim target=SavedGitHubFolder() : If target="" Then target=ChooseGitHubFolder()
        If target="" Then Return
        Try
            For Each fileName As String In New String() {"index.html","styles.css","script.js","vsebina.js"}
                RobustCopy(Path.Combine(websiteFolder,fileName),Path.Combine(target,fileName))
            Next
            CopyFolder(Path.Combine(websiteFolder,"assets"),Path.Combine(target,"assets"))
            CopyFolder(Path.Combine(websiteFolder,"Urejevalnik"),Path.Combine(target,"Urejevalnik"))
            SetStatus("Pripravljeno za GitHub. V GitHub Desktopu naredi Commit in Push origin.",True)
            MessageBox.Show("Spletne datoteke IN urejevalnik (program + izvorna koda) so prekopirani v lokalni GitHub repozitorij." & Environment.NewLine & Environment.NewLine & "Zdaj v GitHub Desktopu naredi Commit to main in nato Push origin.","Pripravljeno za objavo",MessageBoxButtons.OK,MessageBoxIcon.Information)
        Catch ex As Exception
            If ex.Message.Contains("uporablja") OrElse ex.Message.Contains("used by another process") Then
                MessageBox.Show("Datoteko trenutno drži drug program (najpogosteje odprti predogled v brskalniku)." & Environment.NewLine & "Zapri zavihek s predogledom in poskusi znova.","Datoteka je zasedena",MessageBoxButtons.OK,MessageBoxIcon.Warning)
            Else
                MessageBox.Show(ex.Message,"Napaka pri pripravi za GitHub",MessageBoxButtons.OK,MessageBoxIcon.Error)
            End If
        End Try
    End Sub

    Private Shared Sub RobustCopy(sourcePath As String,targetPath As String)
        For attempt As Integer=1 To 10
            Try
                Dim targetDir=Path.GetDirectoryName(targetPath)
                If Not String.IsNullOrEmpty(targetDir) Then Directory.CreateDirectory(targetDir)
                Using src As New IO.FileStream(sourcePath,IO.FileMode.Open,IO.FileAccess.Read,IO.FileShare.ReadWrite Or IO.FileShare.Delete)
                Using dst As New IO.FileStream(targetPath,IO.FileMode.Create,IO.FileAccess.Write,IO.FileShare.None)
                    src.CopyTo(dst)
                End Using
                End Using
                Return
            Catch ex As Exception
                If attempt>=10 Then Throw
                Threading.Thread.Sleep(200)
            End Try
        Next
    End Sub

    Private Shared Sub CopyFolder(source As String,target As String)
        Directory.CreateDirectory(target)
        For Each filePath In Directory.GetFiles(source) : RobustCopy(filePath,Path.Combine(target,Path.GetFileName(filePath))) : Next
        For Each folderPath In Directory.GetDirectories(source) : CopyFolder(folderPath,Path.Combine(target,Path.GetFileName(folderPath))) : Next
    End Sub

    Private Sub SetStatus(message As String,success As Boolean)
        statusLabel.Text=message
        statusLabel.ForeColor=If(success,Color.FromArgb(47,112,72),Color.FromArgb(94,87,78))
    End Sub
End Class
