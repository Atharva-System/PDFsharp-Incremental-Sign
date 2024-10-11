// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf.IO;
using PdfSharp.Quality;
using PdfSharp.Snippets.Font;
using System.IO;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Signatures;
using Xunit;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;
using PdfSharp.Drawing.Layout;

namespace PdfSharp.Tests.IO
{
    [Collection("PDFsharp")]
    public class WriterTests
    {
        [Fact]
        public void Write_import_file()
        {
            var testFile = IOUtility.GetAssetsPath("archives/samples-1.5/PDFs/SomeLayout.pdf")!;

            var filename = PdfFileUtility.GetTempPdfFileName("ImportTest");

            var doc = PdfReader.Open(testFile, PdfDocumentOpenMode.Import);

            Action save = () => doc.Save(filename);
            save.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Append_To_File()
        {
            var sourceFile = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/WPF 1.31/Table-Layout.pdf")!;
            var targetFile = Path.Combine(Path.GetTempPath(), "AA-Append.pdf");
            File.Copy(sourceFile, targetFile, true);

            using var fs = File.Open(targetFile, FileMode.Open, FileAccess.ReadWrite);
            var doc = PdfReader.Open(fs, PdfDocumentOpenMode.Append);
            var numPages = doc.PageCount;
            var numContentsPerPage = new List<int>();
            foreach (var page in doc.Pages)
            {
                // remember count of existing contents
                numContentsPerPage.Add(page.Contents.Elements.Count);
                // add new content
                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawString("I was added", new XFont("Arial", 16), new XSolidBrush(XColors.Red), 40, 40);
            }

            doc.Save(fs, true);

            // verify that the new content is picked up
            var idx = 0;
            doc = PdfReader.Open(targetFile, PdfDocumentOpenMode.Import);
            doc.PageCount.Should().Be(numPages);
            foreach (var page in doc.Pages)
            {
                var c = page.Contents.Elements.Count;
                c.Should().Be(numContentsPerPage[idx] + 1);
                idx++;
            }
        }

        [Fact]
        public void Update_With_Deletion()
        {
            // create input file
            Append_To_File();

            var sourceFile = Path.Combine(Path.GetTempPath(), "AA-Append.pdf");
            var targetFile = Path.Combine(Path.GetTempPath(), "AA-Append-Delete.pdf");
            File.Copy(sourceFile, targetFile, true);

            using var fs = File.Open(targetFile, FileMode.Open, FileAccess.ReadWrite);
            var doc = PdfReader.Open(fs, PdfDocumentOpenMode.Append);
            var numPages = doc.Pages.Count;

            var firstPage = doc.Pages[0];
            // page will not be deleted, because it is referenced by other objects (Outlines)
            // delete contentes as well, so we have at least SOME "f"-entries in the new xref-table
            firstPage.Contents.Elements.Clear();
            doc.Pages.Remove(firstPage);

            doc.Save(fs, true);

            doc = PdfReader.Open(targetFile, PdfDocumentOpenMode.Import);
            doc.PageCount.Should().Be(numPages - 1);
            // new xref-table was checked manually (opened in notepad)
        }

        [Fact]
        public void Sign()
        {
            var cert = new X509Certificate2(@"C:\Data\Test Digital Certificate Password is 123456 (1).pfx", "123456");

            for (var i = 1; i <= 2; i++)
            {
                var renderer = new CustomSignatureRenderer(); // Assuming CustomSignatureRenderer implements ISignatureRenderer

                var options = new PdfSignatureOptions
                {
                    Certificate = cert,
                    FieldName = "Signature-" + Guid.NewGuid().ToString("N"),
                    PageIndex = 0,
                    Rectangle = new XRect(120 * i, 40, 100, 60),
                    Location = "My PC",
                    Reason = "Approving Rev #" + i,
                    Image = XImage.FromFile(@"C:\Data\stamp.png"),
                    FieldFlags = PdfSharp.Pdf.Annotations.PdfAnnotationFlags.Print,
                    //Renderer = renderer // Assign your custom renderer here
                };




                string sourceFile;
                string targetFile;

                // Set source and target files for first and second signature
                if (i == 1)
                {
                    sourceFile = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/WPF 1.31/Table-Layout.pdf")!;
                    targetFile = Path.Combine(Path.GetTempPath(), "AA-Signed.pdf");
                }
                else
                {
                    sourceFile = Path.Combine(Path.GetTempPath(), "AA-Signed.pdf");
                    targetFile = Path.Combine(Path.GetTempPath(), "AA-Signed-2.pdf");
                }

                // Copy the source file to target
                File.Copy(sourceFile, targetFile, true);

                // Sign the document
                using (var fs = File.Open(targetFile, FileMode.Open, FileAccess.ReadWrite))
                {
                    var signer = new PdfSigner(fs, options);
                    var resultStream = signer.Sign();
                    fs.Seek(0, SeekOrigin.Begin);
                    resultStream.CopyTo(fs);
                    //// Re-open the document for appending the signature appearance
                    //using (var document = PdfReader.Open(resultStream, PdfDocumentOpenMode.Append))
                    //{
                    //    var page = document.Pages[0];  // Assuming the signature is on the first page
                    //    XGraphics gfx = XGraphics.FromPdfPage(page);

                    //    // Load and draw the stamp image
                    //    XImage stampImage = XImage.FromFile(@"C:\Data\stamp.png");
                    //    gfx.DrawImage(stampImage, 120 * i, page.Height - 40-60 , 100, 60);  // Match the Rectangle coordinates

                    //    // Save the document after signing
                    //    document.Save(fs);
                    //}
                }
            }

            // Verify the final document
            using (var finalDoc = PdfReader.Open(Path.Combine(Path.GetTempPath(), "AA-Signed-2.pdf"), PdfDocumentOpenMode.Modify))
            {
                var acroForm = finalDoc.AcroForm;
                acroForm.Should().NotBeNull();
                var signatureFields = acroForm!.GetAllFields().OfType<PdfSignatureField>().ToList();
                signatureFields.Count.Should().Be(2);  // Ensure both signatures are present
            }
        }

    }
}


public class CustomSignatureRenderer : ISignatureRenderer
{
    public void Render(XGraphics gfx, XRect rect, PdfSignatureOptions options)
    {
        var imageFolder = IOUtility.GetAssetsPath() ??
                           throw new InvalidOperationException("Call Download-Assets.ps1 before running the tests.");
        var pngFile = @"C:\Data\stamp.png";
        var image = XImage.FromFile(pngFile);

        string text = "John Doe\nSeattle, " + DateTime.Now.ToString(CultureInfo.GetCultureInfo("EN-US"));
        var font = new XFont("Verdana", 7.0, XFontStyleEx.Regular);
        var textFormatter = new XTextFormatter(gfx);
        double num = (double)image.PixelWidth / image.PixelHeight;
        double signatureHeight = rect.Height * .4;
        var point = new XPoint(rect.Width / 10, rect.Height / 10);
        // Draw image.
        gfx.DrawImage(image, point.X, point.Y, signatureHeight * num, signatureHeight);
        //// Adjust position for text. We draw it below image.
        //point = new XPoint(point.X, rect.Height / 2d);
        ////textFormatter.DrawString(text, font, new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)), new XRect(point.X, point.Y, rect.Width, rect.Height - point.Y), XStringFormats.TopLeft);
        //textFormatter.DrawString(text, font, XBrushes.Black, new XRect(point.X, point.Y, rect.Width, rect.Height - point.Y), XStringFormats.TopLeft);
    }
}
