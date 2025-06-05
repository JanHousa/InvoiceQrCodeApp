# InvoiceQrCodeApp

**InvoiceQrCodeApp** je multiplatformní aplikace vytvořená pomocí .NET MAUI, která umožňuje:

- 📤 Výběr Excelové faktury
- 📎 Generování QR kódu pro platbu
- 🧾 Vložení QR kódu do faktury a převod do PDF
- 🖨️ Tisk (pouze na Androidu)
- 📬 Sdílení faktury jako PDF

---

## 📁 Struktura projektu

- `MainPage.xaml.cs` – Uživatelské rozhraní a interakce
- `InvoiceProcessor.cs` – Logika zpracování faktury
- `PdfDocumentAdapter.cs` – Tisk PDF na Androidu

---

## 🔧 Použité technologie

- [.NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- [EPPlus](https://github.com/EPPlusSoftware/EPPlus) – Čtení a úprava Excel souborů
- [GemBox.Spreadsheet](https://www.gemboxsoftware.com/spreadsheet) – Export do PDF
- [Paylibo API](https://paylibo.com/) – QR platby
- Android `PrintManager` a `FileProvider`
- `Microsoft.Maui.Storage` – FilePicker, Share


