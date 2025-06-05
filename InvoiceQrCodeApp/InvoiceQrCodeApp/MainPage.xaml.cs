using Microsoft.Maui.Storage;

namespace InvoiceQrCodeApp;

public partial class MainPage : ContentPage
{
    private string? _selectedFilePath;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        try
        {
            var file = await FilePicker.Default.PickAsync();
            if (file != null)
            {
                _selectedFilePath = file.FullPath;
                StatusLabel.Text = $"Vybraný soubor: {file.FileName}";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Chyba", $"Soubor nebylo možné načíst: {ex.Message}", "OK");
        }
    }

    private async void OnGenerateQrCodeClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedFilePath))
        {
            await DisplayAlert("Chyba", "Nejprve vyberte soubor.", "OK");
            return;
        }

        try
        {
            var processor = new InvoiceProcessor();
            string generatedPdfPath = await Task.Run(() => processor.GenerateQrCodeAndPrint(_selectedFilePath));

            StatusLabel.Text = "Faktura byla vygenerována a odeslána na tisk.";
            await DisplayAlert("Hotovo", "Tisk byl úspěšně spuštěn.", "OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při generování QR kódu: {ex}");
            await DisplayAlert("Chyba", $"Tisk se nezdařil: {ex.Message}", "OK");
        }
    }

    private async void OnSendEmailClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedFilePath))
        {
            await DisplayAlert("Chyba", "Nejprve vyberte soubor.", "OK");
            return;
        }

        try
        {
            var processor = new InvoiceProcessor();
            string generatedPdfPath = await Task.Run(() => processor.GenerateQrCode(_selectedFilePath));

            SharePdf(generatedPdfPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při sdílení PDF: {ex}");
            await DisplayAlert("Chyba", $"Sdílení se nezdařilo: {ex.Message}", "OK");
        }
    }

    private void SharePdf(string pdfFilePath)
    {
        try
        {
            Share.RequestAsync(new ShareFileRequest
            {
                Title = "Sdílení faktury",
                File = new ShareFile(pdfFilePath)
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při sdílení: {ex.Message}");
            throw;
        }
    }
}
