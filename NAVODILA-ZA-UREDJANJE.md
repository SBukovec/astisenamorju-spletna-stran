# NAVODILA ZA UREJANJE SPLETNE STRANI ASTIŠENAMORJU

Za lastnika (ti) in za kolega oblikovalca.

---

## KORAK 1 — LASTNIK: Pošlji kolegu dostop

### 1.1 Dodeli dostop na GitHubu
1. Odpri repozitorij na GitHubu: `https://github.com/SBukovec/astisenamorju-spletna-stran`
2. Klikni zavihek **Settings** (zgoraj desno)
3. V levem meniju klikni **Collaborators**
4. Klikni zeleni gumb **Add people**
5. Vpiši **GitHub uporabniško ime** kolega ali pa njegov **e-poštni naslov** in pošlji povabilo.
6. Koleg bo prejel e-pošto — naj klikne **Accept invitation** (sprejmi povabilo).

> Če koleg nima GitHub računa, naj se najprej brezplačno registrira na https://github.com (ne potrebuje plačljivega paketa).

### 1.2 Pošlji kolegu to datoteko in sporočilo
Pošlji mu to datoteko (NAVODILA-ZA-UREDJANJE.md) in naslednje podatke:
- **URL repozitorija:** `https://github.com/SBukovec/astisenamorju-spletna-stran`
- Povej mu, da mora slediti **Koraku 2** (spodaj)

### 1.3 Po tem, ko koleg potisne spremembe
- Na https://github.com preveri, da so spremembe vidne.
- GitHub Pages objavi samodejno v ~10 minutah. Stran se osveži na istem naslovu.

---

## KORAK 2 — KOLEGA: Namestitev in prvo zagon

Vse, kar potrebujete, so **4 preprosti koraki** (spodaj).

### Korak A: Prenesi GitHub Desktop (1x)
1. Odpri spletno stran: **https://desktop.github.com**
2. Klikni velik gumb **Download for Windows**
3. Odpre se datoteka — dvoklikni jo in klikni **Install** / **Namesti**
4. Ko se namestitev konča, klikni **Sign in to GitHub.com**
5. Vpiši svoj **GitHub uporabniško ime** in **geslo** (tisto, kar si uporabil/a za registracijo)
6. Pusti vse nastavitve takšne, kot so (če te vpraša za dovoljenja — dovoli)

> Ko je GitHub Desktop nameščen, ga ne bo treba znova namestiti.

### Korak B: Prenesi spletno stran na svoj računalnik (1x)
1. Odpri **GitHub Desktop** (če se ni odprl sam)
2. V zgornjem levem kotu klikni **File** → **Clone Repository…**
3. Klikni zavihek **URL** (desno zgoraj)
4. V polje **Repository URL** prilepi ta naslov:
   `https://github.com/SBukovec/astisenamorju-spletna-stran`
5. Klikni **Clone**
6. Izberi mapo — pusti privzeto (običajno `Documents\GitHub\astisenamorju-spletna-stran`)
7. Klikni **Clone** in počakaj

> Ko je prenesena, se mapa ne bo treba znova prenašati — samo osveži se.

### Korak C: Odpri urejevalnik (vsakič znova)
1. V **GitHub Desktop** klikni zgoraj desno gumb **Show in Explorer** (ali **Show in File Explorer**)
2. Odpre se mapa. Dvoklikni mapo **Urejevalnik**
3. Dvoklikni datoteko **AstisenamorjuUrejevalnik.exe**
   - **Če Windows prikaže modro ali rumeno obvestilo** »Windows je blokiral neznanega izdajatelja« — to je normalno. Klikni **Več informacij (More info)** in nato **Vseeno zaženi (Run anyway)**.
4. Odpre se program **ASTIŠENAMORJU — CMS**

> **Nasvet za prihodnje:** v Windowsu lahko to datoteko pripneš na opravilno vrstico ali začetni meni za hiter dostop.

### Korak D: Uredi in objavi (vsakič, ko želiš nekaj spremeniti)
1. V programu **spremeni besedilo** ali izberi novo fotografijo (zavihki zgoraj)
2. Klikni zeleni gumb **Shrani spremembe**
3. Odpri **GitHub Desktop** (naj bo že odprt v ozadju)
4. Na dnu bo pisalo **Changes** — videl/a boš, kaj se je spremenilo
5. V polje **Summary** (zgoraj) vpiši kratko opis npr. `Posodobil/a besedilo`
6. Klikni **Commit to main** (zeleni gumb spodaj levo)
7. Klikni modri gumb **Push origin** (zgoraj desno)
8. Počakaj ~10 minut — nova vsebina bo vidna na spletu!

> Če se ti zatakne, vpračaj lastnika (tvojega znanca) za pomoč.

---

## POMOČ — Najpogostejša vprašanja

**Kaj je geslo za GitHub?**
> Geslo, ki si ga ustvaril/a ob registraciji na GitHubu. Če ga ne veš več, klikni **Forgot password** na strani https://github.com/login

**Program noče odpreti mape — kaj naj naredim?**
> V GitHub Desktop klikni **Show in Explorer** znova in preveri, da si v mapi `astisenamorju-spletna-stran`.

**Po pritisnjenem "Push" piše "Push rejected" — kaj?**
> Mogoče ni povezave z internetom. Preveri WiFi. Če te prosi za GitHub uporabniško ime in geslo — vpiši enaka kot prej.

**Ne vem, kaj pomeni nek v programu?**
> Kar pusti takšno, kot je, in zaprosi lastnika. Bolje, da ne spremeniš česa po naključju.

---

## NAVODILA ZA LASTNIKA (kratek povzetek za pošiljanje kolegu)

1. **Registriraj GitHub račun za kolega** (če ga nima) ali izvedi njegovo uporabniško ime.
2. **Dodeli dostop:** Settings → Collaborators → Add people → vpiši njegovo uporabniško ime / e-pošto.
3. **Pošlji mu to datoteko** (NAVODILA-ZA-UREDJANJE.md) skupaj z naslovom repozitorija: `https://github.com/SBukovec/astisenamorju-spletna-stran`
4. Ko koleg zaključi z urejanjem in stisne **Push**, se nova vsebina objavi samodejno.
