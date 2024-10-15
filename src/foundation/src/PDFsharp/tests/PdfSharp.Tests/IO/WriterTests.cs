// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using FluentAssertions;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Signatures;
using PdfSharp.Quality;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Xunit;

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

        PageInfo[] signer1 = new PageInfo[]
        {
            new PageInfo { PageNumber = 1, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 2, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 3, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 4, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 5, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 6, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 7, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 8, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 316.554, Y1 = 563.126, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 9, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 10, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 349.351, Y1 = 522.023, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 11, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 12, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 13, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 14, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 125.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } }
        };

        PageInfo[] signer2 = new PageInfo[]
       {
            new PageInfo { PageNumber = 1, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 2, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 3, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 4, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 5, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 6, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 7, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 8, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 679.341, Y1 = 307.846, X2 = 99.9938, Y2 = 39.9975 } } },
            new PageInfo { PageNumber = 9, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 10, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 622.886, Y1 = 372.877, X2 = 99.9938, Y2 = 39.9975 } } },
            new PageInfo { PageNumber = 11, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 12, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 13, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 14, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 240.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } }
       };

        PageInfo[] signer3 = new PageInfo[]
       {
            new PageInfo { PageNumber = 1, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 2, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 3, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 4, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 5, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 6, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 7, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 8, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 0.0, Y1 = 322.301, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 9, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 10, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 72.5693, Y1 = 391.514, X2 = 99.9937, Y2 = 39.9975 } } },
            new PageInfo { PageNumber = 11, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 12, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 13, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } },
            new PageInfo { PageNumber = 14, PageSize = 792.0, PDFCoordinates = new PdfCoordinate[] { new PdfCoordinate { X1 = 10.0, Y1 = 722.0, X2 = 100.0, Y2 = 40.0 } } }
       };

        private PageInfo[] GetSigner1Pages()
        {
            return signer1;
        }

        private PageInfo[] GetSigner2Pages()
        {
            return signer2;
        }

        private PageInfo[] GetSigner3Pages()
        {
            return signer3;
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

        [Fact]
        public void Sign1()
        {
            // Arrange
            string sourceFile = IOUtility.GetAssetsPath("archives/grammar-by-example/GBE/ReferencePDFs/WPF 1.31/BS_Ankit Chokshi  Co._31.03.2024.pdf")!;
            string outputFile = Path.Combine(Path.GetTempPath(), "AA-Signed.pdf");
            string certPath = @"C:\Data\Test Digital Certificate Password is 123456 (1).pfx"; // Certificate path
            string certPassword = "123456"; // Certificate password

            // Set up signer pages
            List<PageInfo[]> signers = new List<PageInfo[]>() { GetSigner1Pages(), GetSigner2Pages(), GetSigner3Pages() };

            // Act
            SignPdf(sourceFile, outputFile, certPath, certPassword, signers);
        }

        public void SignPdf(string sourceFile, string outputFile, string certPath, string certPassword, List<PageInfo[]> signers)
        {
            using var document = PdfReader.Open(sourceFile, PdfDocumentOpenMode.Modify);
            var certificate = new X509Certificate2(certPath, certPassword);
            int i = 1;
            foreach (var signerPages in signers)
            {
                foreach (var pageInfo in signerPages)
                {
                    var page = document.Pages[pageInfo.PageNumber - 1];
                    // Determine page dimensions
                    double pageWidth = page.Width;
                    double pageHeight = page.Height;

                    // Adjust coordinates for landscape pages
                    double x = pageInfo.PDFCoordinates[0].X1;
                    double y = pageInfo.PDFCoordinates[0].Y1;

                    // Check if the page is in landscape orientation
                    bool isLandscape = pageWidth > pageHeight;

                    // Log coordinates for debugging
                    Console.WriteLine($"Signing page {pageInfo.PageNumber}: X={x}, Y={y}");

                    // Adjust coordinates based on orientation
                    if (isLandscape)
                    {
                        //page.Rotate = 0;
                        // If the page is landscape, swap X and Y coordinates
                        double tempX = x;
                        x = y; // Assuming X corresponds to Y when landscape
                        y = tempX;
                    }
                    int rotation = page.Rotate;
                    // Create graphics object and draw the signature
                    using var gfx = XGraphics.FromPdfPage(page);

                    //XRect signRect = new XRect(x, y, 133.33, 33);//XRect(newX, newY, signWidth, signHeight);
                    //if (rotation == 90 || rotation == 270)
                    //{
                    //    // Rotate text at the center of the signature box
                    //    gfx.RotateAtTransform(270, new XPoint(signRect.X + signRect.Width / 2, signRect.Y + signRect.Height / 2));
                    //}

                    // Define a larger font size for better visibility
                    var font = new XFont("Arial", 12);
                    gfx.DrawString("Signer" + i, font, new XSolidBrush(XColors.Red), x, y);

                    // Optionally, add a rectangle around the signature to make it more visible
                    gfx.DrawRectangle(new XSolidBrush(XColors.Transparent), x - 5, y - 5, 133.33, 33); // Adjust size as needed
                }
                i++;
            }
            document.Save(outputFile);
        }
        private void DrawSignature(PdfPage page, PdfCoordinate coordinate)
        {
            // Create an XGraphics object for drawing
            using (XGraphics gfx = XGraphics.FromPdfPage(page))
            {
                // Load the signature image
                // You can change the path to your image
                string signatureImagePath = @"C:\Data\stamp.png";
                XImage image = XImage.FromFile(signatureImagePath);

                // Draw the image at the specified coordinates
                gfx.DrawImage(image, coordinate.X1, coordinate.Y1, coordinate.X2, coordinate.Y2);
            }
        }
    }
}

public class PdfCoordinate
{
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
}

public class PageInfo
{
    public int PageNumber { get; set; }
    public double PageSize { get; set; }
    public PdfCoordinate[] PDFCoordinates { get; set; }
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
