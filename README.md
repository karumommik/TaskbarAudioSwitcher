# Taskbar Audio Switcher

Choose language / Vali keel:
- [English 🇬🇧](#english-)
- [Eesti keel 🇪🇪](#eesti-keel-)

---

## English 🇬🇧

See on ülimalt kergekaaluline, stabiilne ja mugav Windows 11 utiliit, mis paigutab end automaatselt taskbarile (kella ja süsteemiikoonide kõrvale) ja võimaldab kontrollida kogu arvuti heli kiirelt ja mugavalt.

This is an extremely lightweight, stable, and convenient Windows 11 utility that automatically places itself on the taskbar (next to the system clock and system tray icons), allowing you to control all computer audio quickly and comfortably.

### Key Features
1. **Quick Audio Device Switching:** Switch between up to 3 selected active devices (e.g. speakers, headphones, TV) by clicking on their corresponding icons.
2. **Volume Regulation:** Adjust volume by dragging the slider or scrolling the mouse wheel directly over the utility.
3. **Mute/Unmute:** Mute audio with a single click.
4. **App Volume Mixer:** Clicking the button next to the volume percentage opens a dynamic mixer above the utility, allowing you to regulate the volume of each audio-producing application (e.g., Chrome, Firefox, Spotify) individually or mute them.
5. **Configuration View:** The settings window, opened from the system tray icon, allows you to:
   - Select and filter which audio devices are displayed on the utility bar.
   - Choose whether the utility is displayed to the left or right of the taskbar clock.
   - Enable "Always on Top" mode to keep the utility visible even over fullscreen games/applications.
   - Activate a mode that automatically moves the utility to the left edge of the second monitor when a fullscreen or borderless windowed game or application is launched, and returns it to its original position upon exiting the game.

### Recent Improvements and Fixes (Updates)
Several critical low-level Windows API and memory improvements have been made to the project in recent development cycles to ensure 24/7 stability:
* **Full Audio Mixer Functionality and COM V-table Fix:** Fixed a typo in the `IAudioSessionControl` IID GUID and added the missing `SetDisplayName` method. This synchronized the code perfectly with the low-level Windows COM virtual table (v-table), resolving incorrect Process ID retrieval and ensuring application names, icons, and individual volumes are always loaded and saved correctly.
* **Troubleshooting and Fallback ID Support:** All WASAPI interfaces now use the `[PreserveSig]` attribute. The program no longer crashes with unexpected exceptions if a temporary audio session returns error `0x80070490` (Element not found) during search; instead, it generates a safe fallback ID and successfully continues loading the mixer.
* **Smart Mixer Auto-Close:** The mixer now closes using a combination of the `GetForegroundWindow` API and mouse position tracking. The mixer stays open steadily as long as the user's mouse is over it, and closes/collapses in a fraction of a second as soon as the user moves the mouse away and clicks elsewhere, or when Windows hides the utility behind the taskbar.
* **Memory Leak Fix and GC Optimization:** Resolved a COM object memory leak by correctly releasing all activated audio output COM references (`SafeRelease`). Additionally, the program now performs automatic Garbage Collection in the background every 60 seconds and releases excess resources immediately upon closing the mixer. Memory usage remains stable between **13.5 - 14.5 MB**.

### Features and Design
- **Automatic Alignment:** Dynamically detects the system clock/tray position and aligns itself on the taskbar accordingly.
- **Automatic Theme Support:** Detects Windows Light/Dark mode and adjusts the utility and mixer colors and borders accordingly (removing distracting purple tones).
- **Non-activating Window (WS_EX_NOACTIVATE):** Clicking on the utility does not steal focus from other active windows, making it perfect for gamers.
- **System Tray Icon:** Runs quietly in the background. Right-clicking the system tray icon allows you to exit the utility, open settings, or enable run at Windows startup.

### Installation and Running
#### 1. Compile and Run
Double-click the convenient `compile.bat` file included in the folder.
- This compiles the C# code directly using the local built-in Windows C# compiler (`csc.exe`).
- The `TaskbarAudioSwitcher.exe` file will be created in the same folder and launched automatically.
#### 2. Auto-start with Windows
- Right-click the blue speaker icon in the system tray.
- Select **"Käivita koos Windowsiga (Startup)"** / **"Run at Windows Startup"**. This adds the program to the registry (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
#### 3. Exit
- Right-click the blue icon in the system tray and select **"Välju"** / **"Exit"**.

---

## Eesti keel 🇪🇪

See on ülimalt kergekaaluline, stabiilne ja mugav Windows 11 utiliit, mis paigutab end automaatselt taskbarile (kella ja süsteemiikoonide kõrvale) ja võimaldab kontrollida kogu arvuti heli kiirelt ja mugavalt.

### Peamised funktsioonid
1. **Heliseadmete kiirvahetus:** Kuni 3 valitud aktiivse seadme vahel (näiteks kõlarid, kõrvaklapid, teler) vastavatel ikoonidel klõpsates.
2. **Helitugevuse reguleerimine:** Liuguri lohistamisega või hiirerattaga otse utiliidi kohal kerides.
3. **Mute/unmute:** Heli vaigistamine ühe klõpsuga.
4. **Rakenduste helimikser:** Helitugevuse protsendi kõrval olevale nupule vajutades avaneb utiliidi kohale dünaamiline mikser, kus saab reguleerida iga heliga rakenduse (nt Chrome, Firefox, Spotify) helitugevust eraldi või neid vaigistada.
5. **Seadistusvaade:** Süsteemiikoonilt avatav seadete aken võimaldab:
   - Valida ja filtreerida, milliseid heliseadmeid utiliidi ribal kuvatakse.
   - Määrata, kas utiliit kuvatakse tegumiriba kella suhtes vasakule või paremale.
   - Lülitada sisse "Always on Top" (Alati pealmine) režiimi, et utiliit püsiks nähtav ka täisekraanil olevate mängude/rakenduste peal.
   - Aktiveerida režiimi, mis liigutab utiliidi automaatselt teise monitori vasakusse serva, kui käivitatakse mõni täisekraanil (fullscreen või borderless windowed) olev mäng või rakendus, ning toob selle tagasi algsesse kohta pärast mängust väljumist.

### Viimased põhiparandused ja parendused (Uuendused)
Projektile on viimastes arendustsüklites tehtud mitmeid kriitilisi madala taseme Windows API ja mälu parendusi, mis tagavad utiliidi stabiilsuse 24/7 tööks:
* **Täielik helimikseri toimimine ja COM v-table parandus:** Parandasime helisessioonide liidese (`IAudioSessionControl`) IID GUID trükivea ja lisasime puuduoleva `SetDisplayName` meetodi. See pani koodi täielikku sünkroonsusesse Windowsi madala taseme COM virtuaaltabeliga (v-table), lahendades vale Process ID tagastamise ja tagades, et rakenduste nimed, ikoonid ja individuaalne helitugevus laaditakse ning salvestatakse alati korrektselt.
* **Tõrkeotsing ja Fallback ID tugi:** Kõik WASAPI liidesed kasutavad nüüd `[PreserveSig]` atribuuti. Programm ei katu enam ootamatute erinditega, kui mõni ajutine helisessioon tagastab otsingu ajal vea `0x80070490` (Elementi ei leitud), vaid genereerib turvalise asenduskoodi (fallback ID) ning laeb programmi mikserisse edukalt edasi.
* **Nutikas mikseri automaatne sulgumine:** Mikseri sulgemiseks kasutatakse nüüd kombinatsiooni `GetForegroundWindow` API-st ja hiire asukoha kontrollist. Mikser püsib stabiilselt lahti, kui kasutaja hiir on selle kohal, ning sulgub ja tõmbub sekundikümnendikuga kokku kohe, kui kasutaja viib hiire eemale ja klikib kuskile mujale või kui Windows utiliidi tagaplaanile (tegumiriba taha) peidab.
* **Mälu lekkimise peatamine ja GC optimeerimine:** Lahendasime COM-objekti mälulekke, vabastades korrektselt kõik aktiveeritud heliväljundi COM-referentsid (`SafeRelease`). Lisaks teostab programm nüüd taustal iga 60 sekundi järel automaatse mälupuhastuse (Garbage Collection) ja vabastab liigsed ressursid koheselt ka mikseri sulgemisel. Utiliidi mälukasutus püsib alati stabiilsena vahemikus **13.5 - 14.5 MB**.

### Omadused ja disain
- **Automaatne paigutus:** Tuvastab dünaamiliselt süsteemse kella/kandiku asukoha ja joondub sellest lähtuvalt tegumiribale.
- **Automaatne teema tugi:** Tuvastab Windowsi heleda/tumedama kujunduse (Light/Dark mode) ja kohandab utiliidi ja mikseri värvitoonid ja piirjooned selle järgi (eemaldatud häirivad lillad toonid).
- **Mitteaktiivne aken (WS_EX_NOACTIVATE):** Utiliidil klõpsamine ei võta fookust ära teistelt aktiivsetelt akendelt, sobides ideaalselt mängijatele.
- **Süsteemiikoon (System Tray):** Töötab vaikselt taustal. Süsteemiikooni paremklõpsuga saab utiliidi mugavalt sulgeda, avada seaded või lubada sellel automaatselt koos Windowsiga käivituda.

### Paigaldus ja käivitamine
#### 1. Kompileerimine ja käivitamine
Kaustas on kaasas mugav `compile.bat` fail. Tee sellel topeltklõps.
- See kompileerib C# koodi otse kohaliku Windowsi sisseehitatud C# compileriga (`csc.exe`).
- Faili `TaskbarAudioSwitcher.exe` luuakse samasse kausta ja see käivitatakse automaatselt.
#### 2. Automaatne käivitus koos Windowsiga
- Paremklõpsa süsteemikandikul (System tray) oleval sinisel kõlariikoonil.
- Vali **"Käivita koos Windowsiga (Startup)"**. See lisab programmi automaatselt registrisse (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
#### 3. Väljumine
- Tee paremklõps süsteemikandikul oleval sinisel ikoonil ja vali **"Välju"**.
