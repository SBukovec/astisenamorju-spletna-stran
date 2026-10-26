# Urejanje spletne strani ASTIŠENAMORJU

## Na tem računalniku

- Vsebina in edini aktivni repozitorij: `C:\SILVO\SpletnaStranBenda-Vsebina`.
- Izvorni projekt VB.NET: `C:\Users\BUKOVECS.ILIRIJALK\Documents\Codex\SpletnaStranBenda\Urejevalnik`.
- Program: podmapa `Program` v projektu. Zagon je tudi z `UREDI-SPLETNO-STRAN.cmd` v mapi `Codex\SpletnaStranBenda`.

## Običajno urejanje

1. V GitHub Desktopu izberi repozitorij s potjo `C:\SILVO\SpletnaStranBenda-Vsebina`; pred urejanjem naredi Fetch/Pull, če so na GitHubu nove spremembe.
2. Odpri urejevalnik. Naslov okna pokaže mapo, ki jo urejaš.
3. Za glasbo izberi ali dodaj skladbo in klikni **Naloži MP3**. Datoteka se shrani v `assets/glasba`.
4. Klikni **Pripravi za GitHub**. Gumb shrani vsebino in v repozitorij pripravi tudi celoten samostojni urejevalnik z izvorno kodo VB.NET.
5. V GitHub Desktopu naredi **Commit to main**, nato **Push origin**. Po objavi osveži stran s Ctrl+F5.

Urejevalnik si pot zapomni za Windows uporabnika. Program lahko prestaviš drugam; pot do vsebine ostane enaka. Drugo mapo izbereš z gumbom **Mapa spletne strani**.

## Drugi računalnik

1. Kloniraj celoten repozitorij `https://github.com/SBukovec/astisenamorju-spletna-stran`.
2. Zaženi `Urejevalnik/AstisenamorjuUrejevalnik.exe` ali priloženi `UREDI-SPLETNO-STRAN.cmd`.
3. Izberi klonirano mapo, ki vsebuje `index.html` in `vsebina.js`.
4. Za razvoj odpri `Urejevalnik/AstisenamorjuUrejevalnik.vbproj` v Visual Studiu z .NET 9 in Windows Forms.

Program je samostojen za Windows x64; namestitev .NET za zagon ni potrebna. Ohraniti moraš celotno mapo programa z datotekami DLL, ne samo EXE. Izvorne datoteke `.vb`, `.vbproj`, ikona, konfiguracija NuGet in navodila so priloženi.

Mapa `Urejevalnik` v repozitoriju je distribucijska kopija za GitHub in drugi računalnik. Razvojna različica na tem računalniku je v Codexu. Vsebina strani je vedno samo v glavni mapi pod SILVO.
