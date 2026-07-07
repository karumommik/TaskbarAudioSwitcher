# Taskbar Audio Switcher (Heliväljundi ja helitugevuse kiirvalik taskbaril)

See on ülimalt kergekaaluline, stabiilne ja mugav Windows 11 utiliit, mis paigutab end automaatselt taskbarile (kella ja süsteemiikoonide kõrvale) und võimaldab kontrollida kogu arvuti heli kiirelt ja mugavalt.

## Peamised funktsioonid
1. **Heliseadmete kiirvahetus:** Kuni 3 valitud aktiivse seadme vahel (näiteks kõlarid, kõrvaklapid, teler) vastavatel ikoonidel klõpsates.
2. **Helitugevuse reguleerimine:** Liuguri lohistamisega või hiirerattaga otse utiliidi kohal kerides.
3. **Mute/unmute:** Heli vaigistamine ühe klõpsuga.
4. **Rakenduste helimikser:** Helitugevuse protsendi kõrval olevale nupule vajutades avaneb utiliidi kohale dünaamiline mikser, kus saab reguleerida iga heliga rakenduse (nt Chrome, Firefox, Spotify) helitugevust eraldi või neid vaigistada.
5. **Seadistusvaade:** Süsteemiikoonilt avatav seadete aken võimaldab:
   - Valida ja filtreerida, milliseid heliseadmeid utiliidi ribal kuvatakse.
   - Määrata, kas utiliit kuvatakse tegumiriba kella suhtes vasakule või paremale.
   - Lülitada sisse "Always on Top" (Alati pealmine) režiimi, et utiliit püsiks nähtav ka täisekraanil olevate mängude/rakenduste peal.
   - Aktiveerida režiimi, mis liigutab utiliidi automaatselt teise monitori vasakusse serva, kui käivitatakse mõni täisekraanil (fullscreen või borderless windowed) olev mäng või rakendus, ning toob selle tagasi algsesse kohta pärast mängust väljumist.


---

## Viimased põhiparandused ja parendused (Uuendused)

Projektile on viimastes arendustsüklites tehtud mitmeid kriitilisi madala taseme Windows API ja mälu parendusi, mis tagavad utiliidi stabiilsuse 24/7 tööks:

* **Täielik helimikseri toimimine ja COM v-table parandus:**
  Parandasime helisessioonide liidese (`IAudioSessionControl`) IID GUID trükivea ja lisasime puuduoleva `SetDisplayName` meetodi. See pani koodi täielikku sünkroonsusesse Windowsi madala taseme COM virtuaaltabeliga (v-table), lahendades vale Process ID tagastamise ja tagades, et rakenduste nimid, ikoonid ja individuaalne helitugevus laaditakse ning salvestatakse alati korrektselt.
* **Tõrkeotsing ja Fallback ID tugi:**
  Kõik WASAPI liidesed kasutavad nüüd `[PreserveSig]` atribuuti. Programm ei katu enam ootamatute erinditega, kui mõni ajutine helisessioon tagastab otsingu ajal vea `0x80070490` (Elementi ei leitud), vaid genereerib turvalise asenduskoodi (fallback ID) ning laeb programmi mikserisse edukalt edasi.
* **Nutikas mikseri automaatne sulgumine:**
  Mikseri sulgemiseks kasutatakse nüüd kombinatsiooni `GetForegroundWindow` API-st ja hiire asukoha kontrollist. Mikser püsib stabiilselt lahti, kui kasutaja hiir on selle kohal, ning sulgub ja tõmbub sekundikümnendikuga kokku kohe, kui kasutaja viib hiire eemale ja klikib kuskile mujale või kui Windows utiliidi tagaplaanile (tegumiriba taha) peidab.
* **Mälu lekkimise peatamine ja GC optimeerimine:**
  Lahendasime COM-objekti mälulekke, vabastades korrektselt kõik aktiveeritud heliväljundi COM-referentsid (`SafeRelease`). Lisaks teostab programm nüüd taustal iga 60 sekundi järel automaatse mälupuhastuse (Garbage Collection) ja vabastab liigsed ressursid koheselt ka mikseri sulgemisel. Utiliidi mälukasutus püsib alati stabiilsena vahemikus **13.5 - 14.5 MB**.

---

## Omadused ja disain
- **Automaatne paigutus:** Tuvastab dünaamiliselt süsteemse kella/kandiku asukoha ja joondub sellest lähtuvalt tegumiribale.
- **Automaatne teema tugi:** Tuvastab Windowsi heleda/tumedama kujunduse (Light/Dark mode) ja kohandab utiliidi ja mikseri värvitoonid ja piirjooned selle järgi (eemaldatud häirivad lillad toonid).
- **Mitteaktiivne aken (WS_EX_NOACTIVATE):** Utiliidil klõpsamine ei võta fookust ära teistelt aktiivsetelt akendelt, sobides ideaalselt mängijatele.
- **Süsteemiikoon (System Tray):** Töötab vaikselt taustal. Süsteemiikooni paremklõpsuga saab utiliidi mugavalt sulgeda, avada seaded või lubada sellel automaatselt koos Windowsiga käivituda.

---

## Paigaldus ja käivitamine

### 1. Kompileerimine ja käivitamine
Kaustas on kaasas mugav `compile.bat` fail. Tee sellel topeltklõps.
- See kompileerib C# koodi otse kohaliku Windowsi sisseehitatud C# compileriga (`csc.exe`).
- Faili `TaskbarAudioSwitcher.exe` luuakse samasse kausta ja see käivitatakse automaatselt.

### 2. Automaatne käivitus koos Windowsiga
- Paremklõpsa süsteemikandikul (System tray) oleval sinisel kõlariikoonil.
- Vali **"Käivita koos Windowsiga (Startup)"**. See lisab programmi automaatselt registrisse (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).

### 3. Väljumine
- Tee paremklõps süsteemikandikul oleval sinisel ikoonil ja vali **"Välju"**.
