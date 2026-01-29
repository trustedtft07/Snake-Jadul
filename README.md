# Snake Android (C#)

Game ular klasik untuk Android yang dibuat dengan C# (.NET for Android / `net8.0-android`).

## Fitur
- Canvas grid 18x24 dengan kecepatan bertahap seiring skor naik
- Kontrol sentuh: swipe langsung di layar atau tombol arah di bawah
- Penyimpanan skor tertinggi lokal (`SharedPreferences`)

## Build & Run
Memerlukan .NET 8 SDK dengan workload Android (`dotnet workload install android`):
```bash
dotnet build -t:Run
```
