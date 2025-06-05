#if ANDROID
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Print;
using Java.IO;

namespace InvoiceQrCodeApp;

public class PdfDocumentAdapter : PrintDocumentAdapter
{
    private readonly Context _context;
    private readonly Android.Net.Uri _pdfUri;

    public PdfDocumentAdapter(Context context, Android.Net.Uri pdfUri)
    {
        _context = context;
        _pdfUri = pdfUri;
    }

    public override void OnWrite(PageRange[] pages, ParcelFileDescriptor destination, CancellationSignal cancellationSignal, WriteResultCallback callback)
    {
        try
        {
            using (var input = _context.ContentResolver.OpenInputStream(_pdfUri))
            using (var output = new FileOutputStream(destination.FileDescriptor))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;
                while ((bytesRead = input.Read(buffer)) != -1)
                {
                    output.Write(buffer, 0, bytesRead);
                }
            }
            callback.OnWriteFinished(new[] { PageRange.AllPages });
        }
        catch (Exception e)
        {
            System.Console.WriteLine($"Chyba při zápisu PDF: {e.Message}");
        }
    }

    public override void OnLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes, CancellationSignal cancellationSignal, LayoutResultCallback callback, Bundle extras)
    {
        var builder = new PrintDocumentInfo.Builder("invoice.pdf");
        builder.SetContentType(PrintContentType.Document);
        builder.SetPageCount(PrintDocumentInfo.PageCountUnknown);
        callback.OnLayoutFinished(builder.Build(), true);
    }
}
#endif
