using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Print;
using Android.Runtime;
using Android.Widget;
using AndroidX.Core.Content;
using AndroidX.Print;
using Java.IO;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using static Android.Print.PrintDocumentAdapter;

namespace CostCalculatorForApartment
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        const int RequestStorageId = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestStoragePermission();
        }

        void RequestStoragePermission()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return;
            }

            if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                // Permission is already available.
                return;
            }

            RequestPermissions(new string[] { Manifest.Permission.WriteExternalStorage, Manifest.Permission.ReadExternalStorage }, RequestStorageId);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == RequestStorageId)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    // Permission granted, proceed with file operations.
                }
                else
                {
                    // Permission denied, show a message to the user.
                    Toast.MakeText(this, "Storage permissions are required to save files.", ToastLength.Long).Show();
                }
            }
        }

        // Method to export to PDF
        public async Task ExportToPdf(string filename, byte[] pdfContent)
        {
            try
            {
                string path = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, filename);

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await fs.WriteAsync(pdfContent, 0, pdfContent.Length);
                }

                Toast.MakeText(Android.App.Application.Context, "PDF saved to Downloads folder", ToastLength.Long).Show();

                // Share the file (optional)
                var fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(Android.App.Application.Context, Android.App.Application.Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(fileUri, "application/pdf");
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);

                Android.App.Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(Android.App.Application.Context, $"Failed to save PDF: {ex.Message}", ToastLength.Long).Show();
            }
        }

        // Method to export to Excel
        public async Task ExportToExcel(string filename, byte[] excelContent)
        {
            try
            {
                string path = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, filename);

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    await fs.WriteAsync(excelContent, 0, excelContent.Length);
                }

                Toast.MakeText(Android.App.Application.Context, "Excel file saved to Downloads folder", ToastLength.Long).Show();

                // Share the file (optional)
                var fileUri = AndroidX.Core.Content.FileProvider.GetUriForFile(Android.App.Application.Context, Android.App.Application.Context.PackageName + ".fileprovider", new Java.IO.File(path));
                var intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(fileUri, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);

                Android.App.Application.Context.StartActivity(intent);
            }
            catch (Exception ex)
            {
                Toast.MakeText(Android.App.Application.Context, $"Failed to save Excel file: {ex.Message}", ToastLength.Long).Show();
            }
        }

        // Method to print content
        public void PrintContent(string content)
        {
            try
            {
                var printManager = (PrintManager)GetSystemService(Context.PrintService);
                var printAdapter = new HtmlPrintDocumentAdapter(content);

                printManager.Print("Print Document", printAdapter, new PrintAttributes.Builder().Build());
            }
            catch (Exception ex)
            {
                Toast.MakeText(Android.App.Application.Context, $"Failed to print content: {ex.Message}", ToastLength.Long).Show();
            }
        }
    }

    // Custom print document adapter for printing HTML content
    public class HtmlPrintDocumentAdapter : PrintDocumentAdapter
    {
        private string _htmlContent;

        public HtmlPrintDocumentAdapter(string htmlContent)
        {
            _htmlContent = htmlContent;
        }

        public override void OnLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes, CancellationSignal cancellationSignal, LayoutResultCallback callback, Bundle extras)
        {
            if (cancellationSignal.IsCanceled)
            {
                callback.OnLayoutCancelled();
                return;
            }

            var printDocumentInfo = new PrintDocumentInfo.Builder("PrintDocument")
                .SetContentType(PrintContentType.Document)
                .SetPageCount(PrintDocumentInfo.PageCountUnknown)
                .Build();

            callback.OnLayoutFinished(printDocumentInfo, true);
        }

        public override void OnWrite(PageRange[] pages, ParcelFileDescriptor destination, CancellationSignal cancellationSignal, WriteResultCallback callback)
        {
            using (var input = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_htmlContent)))
            {
                using (var output = new FileOutputStream(destination.FileDescriptor))
                {
                    var buffer = new byte[4096];
                    int size;

                    while ((size = input.Read(buffer)) >= 0 && !cancellationSignal.IsCanceled)
                    {
                        output.Write(buffer, 0, size);
                    }

                    if (cancellationSignal.IsCanceled)
                    {
                        callback.OnWriteCancelled();
                        return;
                    }

                    callback.OnWriteFinished(new PageRange[] { PageRange.AllPages });
                }
            }
        }
    }
}