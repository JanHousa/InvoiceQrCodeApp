using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using Microsoft.Maui.Controls.PlatformConfiguration;

#if ANDROID
using Plugin.CurrentActivity;
using Android.Content;
using Android.Print;
using Android.Net;
using AndroidX.Core.Content;
#endif

using GemBox.Spreadsheet;

namespace InvoiceQrCodeApp
{
    public class InvoiceProcessor
    {
        public string GenerateQrCode(string excelFilePath)
        {
            return ProcessInvoice(excelFilePath, print: false);
        }

        public string GenerateQrCodeAndPrint(string excelFilePath)
        {
            return ProcessInvoice(excelFilePath, print: true);
        }

        private string ProcessInvoice(string excelFilePath, bool print)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    string amountRaw = worksheet.Cells["G33"].Text;
                    string amount = ConvertAmountFormat(amountRaw);

                    string varSymbol = worksheet.Cells["G1"].Text;

                    string dueDateRaw = worksheet.Cells["G9"].Text;
                    string dueDate = ConvertDateFormat(dueDateRaw);

                    string message = worksheet.Cells["E2"].Text;

                    string qrUrl = GeneratePaymentUrl(amount, varSymbol, dueDate, message);
                    byte[] qrImage = DownloadQrCode(qrUrl).Result;

                    InsertQrCodeIntoExcel(worksheet, qrImage);
                    HideCellsBasedOnCondition(worksheet);

                    // Uložení Excel souboru
                    string tempExcelFilePath = Path.Combine(FileSystem.CacheDirectory, "TempInvoice.xlsx");
                    package.SaveAs(new FileInfo(tempExcelFilePath));

                    // Převod Excel na PDF
                    string tempPdfFilePath = Path.Combine(FileSystem.CacheDirectory, "Faktura.pdf");
                    ConvertExcelToPdf(tempExcelFilePath, tempPdfFilePath);

                    if (print)
                    {
#if ANDROID
                        // Tisk na Androidu
                        string publicPdfPath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "Faktura_Ladislav_Housa.pdf");
                        ConvertExcelToPdf(tempExcelFilePath, publicPdfPath);
                        ShowPdfForPrinting(publicPdfPath, Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity);
#endif
                    }

                    return tempPdfFilePath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při zpracování faktury: {ex.Message}");
                throw;
            }
        }

#if ANDROID
        private void ShowPdfForPrinting(string pdfFilePath, Context context)
        {
            try
            {
                var pdfUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                    context, $"{context.PackageName}.provider", new Java.IO.File(pdfFilePath));

                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(pdfUri, "application/pdf");
                intent.SetFlags(ActivityFlags.GrantReadUriPermission);

                context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při zobrazení PDF: {ex.Message}");
            }
        }
#endif

        private string ConvertAmountFormat(string amountRaw)
        {
            string sanitized = amountRaw.Replace(" ", "").Replace("Kč", "").Replace(",", "").Trim();
            if (decimal.TryParse(sanitized, out decimal parsedAmount))
                return parsedAmount.ToString("F0");
            throw new FormatException($"Neplatný formát částky: {amountRaw}");
        }

        private string ConvertDateFormat(string dateRaw)
        {
            if (DateTime.TryParseExact(dateRaw, new[] { "dd.MM.yyyy", "dd-MM-yyyy" }, null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                return parsedDate.ToString("yyyy-MM-dd");
            throw new FormatException($"Neplatný formát datumu: {dateRaw}");
        }

        private string GeneratePaymentUrl(string amount, string varSymbol, string dueDate, string message)
        {
           
            string accountNumber = "XXXX";
            string bankCode = "XXXX";
            string currency = "CZK";

            if (message.Contains("&"))
            {
                message = message.Replace("&", "a");
            }

            return $"https://api.paylibo.com/paylibo/generator/czech/image?compress=false&size=440&accountNumber={accountNumber}&bankCode={bankCode}&amount={amount}&currency={currency}&vs={varSymbol}&message={message}";
        }

        private async Task<byte[]> DownloadQrCode(string url)
        {
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsByteArrayAsync();
                throw new Exception($"Chyba při stahování QR kódu: {response.StatusCode}");
            }
        }

        private void InsertQrCodeIntoExcel(OfficeOpenXml.ExcelWorksheet worksheet, byte[] qrCodeImage)
        {
            var picture = worksheet.Drawings.AddPicture("QRCode", new MemoryStream(qrCodeImage));
            picture.SetPosition(43, 0, 6, 0);
            picture.SetSize(120, 120);
        }

        private void HideCellsBasedOnCondition(OfficeOpenXml.ExcelWorksheet worksheet)
        {
            foreach (var cell in worksheet.Cells)
            {
                if (string.IsNullOrEmpty(cell.Text) || cell.Text == "Neplatné")
                {
                    cell.Value = null;
                }
            }
        }

        private void ConvertExcelToPdf(string excelFilePath, string pdfFilePath)
        {
            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            var workbook = GemBox.Spreadsheet.ExcelFile.Load(excelFilePath);

            foreach (var worksheet in workbook.Worksheets)
            {
                worksheet.PrintOptions.FitWorksheetWidthToPages = 1;
                worksheet.PrintOptions.FitWorksheetHeightToPages = 1;
                worksheet.PrintOptions.PaperType = PaperType.A4;
                worksheet.PrintOptions.Portrait = true;
                worksheet.PrintOptions.PrintGridlines = false;
                worksheet.PrintOptions.PrintHeadings = false;
                worksheet.PrintOptions.LeftMargin = 0.5;
                worksheet.PrintOptions.RightMargin = 0.5;
                worksheet.PrintOptions.TopMargin = 0.5;
                worksheet.PrintOptions.BottomMargin = 0.5;
            }

            workbook.Save(pdfFilePath, SaveOptions.PdfDefault);
        }
    }
}
