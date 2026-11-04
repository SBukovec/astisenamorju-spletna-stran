Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Text.RegularExpressions

' Export the existing template, preserving its layout and manually added head tags.
Friend NotInheritable Class SeoExporter
    Private Shared ReadOnly Utf8 As New UTF8Encoding(False)
    Private Const Flags As RegexOptions = RegexOptions.IgnoreCase Or RegexOptions.Singleline

    Private Shared Function Value(data As JsonObject, key As String) As String
        Return If(data(key)?.ToString(), "")
    End Function

    Private Shared Function Enc(value As String) As String
        Return WebUtility.HtmlEncode(value)
    End Function

    Private Shared Function Fill(html As String, id As String, content As String) As String
        Dim start = "<!--seo:" & id & "-->"
        Dim finish = "<!--/seo:" & id & "-->"
        Dim marked = Regex.Escape(start) & ".*?" & Regex.Escape(finish)
        If html.Contains(start) Then
            Return Regex.Replace(html, marked, Function(m) start & content & finish, Flags)
        End If
        ' Only known template containers are touched. Markers allow nested generated markup.
        Dim pattern = "(?<open><(?<tag>[a-z][a-z0-9]*)\b[^>]*\bid\s*=\s*[""']" & Regex.Escape(id) & "[""'][^>]*>).*?(?<close></\k<tag>\s*>)"
        If Not Regex.IsMatch(html, pattern, Flags) Then Throw New InvalidDataException("V index.html manjka element: " & id)
        Return Regex.Replace(html, pattern, Function(m) m.Groups("open").Value & start & content & finish & m.Groups("close").Value, Flags)
    End Function

    Private Shared Function Attribute(html As String, id As String, name As String, value As String) As String
        Dim pattern = "<[a-z][a-z0-9]*\b[^>]*\bid\s*=\s*[""']" & Regex.Escape(id) & "[""'][^>]*>"
        Return Regex.Replace(html, pattern, Function(m)
            Dim tag = m.Value
            Dim attr = "\s+" & name & "\s*=\s*([""']).*?\1"
            tag = Regex.Replace(tag, attr, "", Flags)
            Return tag.Substring(0, tag.Length - 1) & " " & name & "=""" & Enc(value) & """>"
        End Function, Flags)
    End Function

    Private Shared Function SafeUrl(url As String) As String
        Dim trimmed = url.Trim()
        If trimmed.StartsWith("//", StringComparison.Ordinal) Then Return "#"
        Dim absolute As Uri = Nothing
        If Uri.TryCreate(trimmed, UriKind.Absolute, absolute) AndAlso
            absolute.Scheme <> "https" AndAlso absolute.Scheme <> "http" AndAlso absolute.Scheme <> "mailto" Then Return "#"
        Return trimmed
    End Function

    Private Shared Function Anchor(label As String, url As String) As String
        Return "<a href=""" & Enc(SafeUrl(url)) & """>" & Enc(label) & "</a>"
    End Function

    Friend Shared Sub Save(folder As String, data As JsonObject, javascript As String)
        Dim indexPath = Path.Combine(folder, "index.html")
        Dim html = File.ReadAllText(indexPath)
        Dim mapping = {
            "hero-eyebrow:eyebrow", "hero-title:naslov", "hero-highlight:poudarek", "hero-lead:uvodniOpis",
            "scroll-note:scrollTekst", "story-index:storyIndex", "story-title:storyTitle", "story-one:zgodba1", "story-two:zgodba2",
            "music-index:musicIndex", "music-title:musicTitle", "music-description:musicDescription", "music-note:musicNote",
            "live-index:liveIndex", "live-title:liveTitle", "live-description:liveDescription", "live-caption:liveCaption",
            "gallery-index:galleryIndex", "gallery-title:galleryTitle", "gallery-description:galleryDescription",
            "video-index:videoIndex", "video-title:videoTitle", "video-description:videoDescription",
            "contact-index:contactIndex", "contact-title:contactTitle", "contact-highlight:contactHighlight", "contact-description:contactDescription",
            "footer-left:footerLeft", "footer-right:footerRight"}
        For Each pair In mapping
            Dim parts = pair.Split(":"c)
            html = Fill(html, parts(0), Enc(Value(data, parts(1))))
        Next
        Dim title = Value(data, "naslovStrani")
        If String.IsNullOrWhiteSpace(title) Then title = Value(data, "imeBenda")
        html = Regex.Replace(html, "<title\b[^>]*>.*?</title>", Function(m) "<title>" & Enc(title) & "</title>", Flags)
        html = Attribute(html, "meta-description", "content", Value(data, "metaOpis"))

        For Each spec In {"hero-button-one:heroGumb1:heroGumb1Link", "hero-button-two:heroGumb2:heroGumb2Link", "live-link:liveLinkText:liveLinkUrl"}
            Dim parts = spec.Split(":"c)
            html = Fill(html, parts(0), Enc(Value(data, parts(1))))
            html = Attribute(html, parts(0), "href", SafeUrl(Value(data, parts(2))))
        Next
        html = Fill(html, "contact-email", Enc(Value(data, "kontakt")))
        html = Attribute(html, "contact-email", "href", "mailto:" & Value(data, "kontakt"))
        For Each spec In {"story-image:storySlika:storySlikaAlt", "live-image:liveSlika:liveSlikaAlt"}
            Dim parts = spec.Split(":"c)
            html = Attribute(html, parts(0), "src", SafeUrl(Value(data, parts(1))))
            html = Attribute(html, parts(0), "alt", Value(data, parts(2)))
            html = Attribute(html, parts(0), "loading", "lazy")
        Next
        html = Attribute(html, "hero-video", "poster", SafeUrl(Value(data, "videoPoster")))
        html = Attribute(html, "hero-video-source", "src", SafeUrl(Value(data, "video")))

        Dim nav As New StringBuilder()
        For i = 1 To 9
            If Value(data, "menu" & i) <> "" Then nav.Append(Anchor(Value(data, "menu" & i), Value(data, "menu" & i & "Link")))
        Next
        html = Fill(html, "main-nav", nav.ToString())
        Dim lineup As New StringBuilder()
        For i = 1 To 4
            If Value(data, "zasedba" & i) <> "" Then lineup.Append("<span>" & Enc(Value(data, "zasedba" & i)) & "</span>")
        Next
        html = Fill(html, "lineup", lineup.ToString())
        Dim songs As New StringBuilder()
        Dim songArray = TryCast(data("pesmi"), JsonArray)
        If songArray IsNot Nothing Then
            Dim number = 0
            For Each node In songArray
                Dim song = TryCast(node, JsonObject)
                If song Is Nothing Then Continue For
                number += 1
                songs.Append("<article class=""song""><span class=""song-no"">" & number.ToString("00") & "</span><div><h3>" & Enc(Value(song, "naslov")) & "</h3><p>" & Enc(Value(song, "opis")) & "</p></div>")
                If Value(song, "link") <> "" Then songs.Append(Anchor("Poslušaj skladbo", Value(song, "link")))
                songs.Append("</article>")
            Next
        End If
        html = Fill(html, "song-list", songs.ToString())
        Dim gallery As New StringBuilder()
        Dim galleryArray = TryCast(data("galerija"), JsonArray)
        If galleryArray IsNot Nothing Then
            For Each node In galleryArray
                Dim photo = TryCast(node, JsonObject)
                If photo Is Nothing OrElse Value(photo, "slika") = "" Then Continue For
                gallery.Append("<figure><img loading=""lazy"" src=""" & Enc(SafeUrl(Value(photo, "slika"))) & """ alt=""" & Enc(Value(photo, "opis")) & """>")
                If Value(photo, "opis") <> "" Then gallery.Append("<figcaption>" & Enc(Value(photo, "opis")) & "</figcaption>")
                gallery.Append("</figure>")
            Next
        End If
        html = Fill(html, "gallery-grid", gallery.ToString())
        Dim videos As New StringBuilder()
        Dim videoArray = TryCast(data("videoteka"), JsonArray)
        If videoArray IsNot Nothing Then
            For Each node In videoArray
                Dim clip = TryCast(node, JsonObject)
                If clip Is Nothing OrElse Value(clip, "video") = "" OrElse Value(clip, "aktiven").Trim().ToLowerInvariant() = "ne" Then Continue For
                Dim caption = Value(clip, "naslov")
                If caption = "" Then caption = "Oglej si video"
                videos.Append("<figure class=""video-card"">" & Anchor(caption, Value(clip, "video")) & "</figure>")
            Next
        End If
        html = Fill(html, "video-grid", videos.ToString())
        Dim social As New StringBuilder()
        For Each service In {"facebook", "instagram", "youtube"}
            If Value(data, service & "Url") <> "" Then social.Append(Anchor(Value(data, service & "Text"), Value(data, service & "Url")))
        Next
        html = Fill(html, "socials", social.ToString())

        ' Keep pre-rendered content visible if JavaScript is disabled or fails.
        Const styleId As String = "seo-visible-content"
        If Not html.Contains("id=""" & styleId & """") Then
            html = html.Replace("</head>", "<style id=""" & styleId & """>.reveal{opacity:1;transform:none}</style>" & vbLf & "</head>")
        End If

        ' Preserve an existing canonical; otherwise use the repository's custom domain.
        Dim canonicalPattern = "<link\b(?=[^>]*\brel\s*=\s*[""']canonical[""'])[^>]*>"
        Dim canonicalTag = Regex.Match(html, canonicalPattern, Flags)
        Dim canonical As String = ""
        If canonicalTag.Success Then
            Dim href = Regex.Match(canonicalTag.Value, "\bhref\s*=\s*[""']([^""']+)[""']", Flags)
            canonical = WebUtility.HtmlDecode(href.Groups(1).Value)
        ElseIf File.Exists(Path.Combine(folder, "CNAME")) Then
            Dim domain = File.ReadAllText(Path.Combine(folder, "CNAME")).Trim()
            If Uri.CheckHostName(domain) = UriHostNameType.Dns Then
                canonical = "https://" & domain & "/"
                html = html.Replace("</head>", "<link rel=""canonical"" href=""" & Enc(canonical) & """>" & vbLf & "</head>")
            End If
        End If

        ' Existing JS rebuilds these collections: clear the static items first, exactly once.
        Dim scriptPath = Path.Combine(folder, "script.js")
        Dim script = File.ReadAllText(scriptPath)
        script = script.Replace("// seo-export", "/* seo-export */")
        For Each id In {"main-nav", "lineup", "song-list", "socials", "video-grid"}
            Dim pattern = "const\s+(?<variable>\w+)\s*=\s*document\.getElementById\(['"" ]*" & Regex.Escape(id) & "['"" ]*\);"
            Dim declaration = Regex.Match(script, pattern)
            If Not declaration.Success Then Throw New InvalidDataException("V script.js manjka zbirka: " & id)
            Dim clear = declaration.Groups("variable").Value & ".replaceChildren(); /* seo-export */"
            If Not script.Contains(clear) Then script = script.Replace(declaration.Value, declaration.Value & vbLf & clear)
        Next
        Dim version = DateTime.UtcNow.ToString("yyyyMMddHHmmssfffffff")
        html = Regex.Replace(html, "(?<file>(?:styles\.css|vsebina\.js|script\.js))\?v=[^""'\s>]+", Function(m) m.Groups("file").Value & "?v=" & version)
        Dim files As New Dictionary(Of String, String) From {{indexPath, html}, {scriptPath, script}, {Path.Combine(folder, "vsebina.js"), javascript}}
        Dim canonicalUri As Uri = Nothing
        If Uri.TryCreate(canonical, UriKind.Absolute, canonicalUri) AndAlso (canonicalUri.Scheme = "https" OrElse canonicalUri.Scheme = "http") Then
            Dim sitemapPath = Path.Combine(folder, "sitemap.xml")
            Const sitemapMarker As String = "<!-- Generated by Astisenamorju SEO exporter -->"
            If Not File.Exists(sitemapPath) OrElse File.ReadAllText(sitemapPath).Contains(sitemapMarker) Then
                files(sitemapPath) = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbLf & sitemapMarker & vbLf & "<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""><url><loc>" & Enc(canonical) & "</loc></url></urlset>" & vbLf
            End If
            Dim robotsPath = Path.Combine(folder, "robots.txt")
            If Not File.Exists(robotsPath) Then files(robotsPath) = "User-agent: *" & vbLf & "Allow: /" & vbLf & "Sitemap: " & New Uri(canonicalUri, "/sitemap.xml").AbsoluteUri & vbLf
        End If
        ' Prepare everything before writing; restore originals if any write fails.
        Dim originals As New Dictionary(Of String, Byte())()
        For Each name In files.Keys
            originals(name) = If(File.Exists(name), File.ReadAllBytes(name), Nothing)
        Next
        Try
            For Each item In files
                File.WriteAllText(item.Key, item.Value, Utf8)
            Next
        Catch
            For Each item In originals
                If item.Value Is Nothing Then
                    If File.Exists(item.Key) Then File.Delete(item.Key)
                Else
                    File.WriteAllBytes(item.Key, item.Value)
                End If
            Next
            Throw
        End Try
    End Sub
End Class
