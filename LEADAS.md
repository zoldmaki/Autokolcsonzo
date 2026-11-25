# Autókölcsönző nyilvántartó alkalmazás

## Futtatás

1. Csomagolja ki a ZIP-et, majd nyissa meg az `Autokolcsonzo.slnx` fájlt Visual Studio 2026 Community Editionben. A projekthez a .NET 10 SDK, a .NET asztali fejlesztési munkaterhelés és az SQL Server Express LocalDB szükséges.
2. Állítsa vissza a NuGet-csomagokat, majd indítsa el az alkalmazást az F5 billentyűvel. Az `appsetting.json` fájlban megadott LocalDB-adatbázis hiányában a program létrehozza az adatbázist, és betölti a mintaadatokat.
3. A bal oldali **Összes adat** fülön egy táblázatban jelenik meg mind az öt adattábla tartalma. A további oszlopok vízszintesen görgethetők.

## A mellékelt adatbázis csatolása

Az `Adatbazis` mappában lévő `Autokolcsonzo.mdf` és `Autokolcsonzo_log.ldf` együtt alkotja a mintaadatbázist. Tartsa a két fájlt ugyanabban a mappában. A csatoláshoz az SQL Server Management Studióban kapcsolódjon a `(localdb)\MSSQLLocalDB` példányhoz, és a **Databases → Attach** menüpontban válassza ki az MDF-fájlt. Adjon neki egy szabad nevet, például `AutoKolcsonzo_Leadadas`.

Ha az alkalmazást a csatolt adatbázissal szeretné futtatni, az `appsetting.json` fájlban a `ConnectionStrings.DefaultConnection` érték `Database` részébe írja be a csatoláskor megadott nevet, majd indítsa újra a programot.
