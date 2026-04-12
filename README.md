# Kerting
A Kerting egy modern, átfogó webalkalmazás és digitális piactér, amely hidat képez a professzionális kertészeti szolgáltatásokat nyújtó szakemberek és a kertgondozási munkálatokat igénylő megrendelők között. A platform helyalapú keresést, projektmenedzsmentet, valós idejű csevegést, fórumot és referenciagalériát biztosít a felhasználók számára, így egy teljes körű ökoszisztémát teremtve a zöldterület-kezeléshez.

## Készítők és Szerepkörök
- **Zöld Dániel**
   -	Rólunk oldal
   -	Galéria
   -  Profil galéria 
   -  Fórum
   -  Főoldal
   -  Admin feladatok
   -  Ezen funkciókhoz a backend és adatbázis
   -  Frontend dokumentációja


- **Rozbora Tamás**
   -  Bejelentkezés/Regisztráció
   -  Keresés
   -  Naptár
   -  Projektek
   -  Hibajegyek
   -  Csevegés
   -  Profilok
   -  Ezen funkciókhoz a backend és adatbázis
   -  Backend dokumentációja


## Gyors Indítási Útmutató (Quick Start)

### Előfeltételek
- **.NET 8 SDK** (A backend futtatásához)
- **Node.js** és **npm** (A frontend futtatásához)
- **Microsoft SQL Server** (Az adatbázis futtatásához)

### 1. A Backend (Szerver) indítása
1. Nyisson meg egy parancssort (Terminal / PowerShell).
2. Navigáljon a backend projekt mappájába:
   ```bash
   cd Kerting/backend/Kerting_Api
   ```
3. Állítsa helyre a szükséges .NET csomagokat (opcionális, de ajánlott):
   ```bash
   dotnet restore
   ```
4. Indítsa el a szervert:
   ```bash
   dotnet run
   ```
*A szerver alapértelmezetten a `http://localhost:5000` vagy `https://localhost:5001` porton fog elindulni.*

### 2. A Frontend (Kliens) indítása
Mielőtt elindítja a frontendet, győződjön meg róla, hogy a backend már fut!
1. Nyisson meg egy **új** parancssor ablakot.
2. Navigáljon a frontend mappába:
   ```bash
   cd Kerting/frontend
   ```
3. Telepítse fel a szükséges Node.js csomagokat (ez az első indításnál eltarthat egy kis ideig):
   ```bash
   npm install
   ```
4. Indítsa el a Vite fejlesztői szervert:
   ```bash
   npm run dev
   ```
5. Nyissa meg a böngészőjét a terminálban megjelenő címen (általában `http://localhost:5173`).

### Gyakori Hibaelhárítás
- **Adatbázis csatlakozási hiba:** Ellenőrizze a `Kerting/backend/Kerting_Api/appsettings.json` fájlban a `DefaultConnection` kapcsolati karakterláncot, hogy a szerver neve megegyezik-e az Ön helyi SQL szerverével (pl. `(localdb)\mssqllocaldb` vagy `localhost`).
- **Hálózati (CORS) hiba a felületen:** Győződjön meg róla, hogy a `Kerting/frontend/.env` fájlban megadott API útvonal pontosan arra a portra mutat, amin a C# backend fut.
