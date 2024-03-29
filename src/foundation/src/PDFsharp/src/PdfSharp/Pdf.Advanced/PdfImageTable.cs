// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Contains all used images of a document.
    /// </summary>
    sealed class PdfImageTable : PdfResourceTable
    {
        /// <summary>
        /// Initializes a new instance of this class, which is a singleton for each document.
        /// </summary>
        public PdfImageTable(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Gets a PdfImage from an XImage. If no PdfImage already exists, a new one is created.
        /// </summary>
        public PdfImage GetImage(XImage image)
        {
            var selector = image._selector;
            if (selector == null!)
            {
                selector = new ImageSelector(image);
                image._selector = selector;
            }

            if (!_images.TryGetValue(selector, out var pdfImage))
            {
                pdfImage = new PdfImage(Owner, image);
                //pdfImage.Document = _document;
                Debug.Assert(pdfImage.Owner == Owner);
                _images[selector] = pdfImage;
            }
            return pdfImage;
        }

        /// <summary>
        /// Map from ImageSelector to PdfImage.
        /// </summary>
        readonly Dictionary<ImageSelector, PdfImage> _images = new();

        /// <summary>
        /// A collection of information that uniquely identifies a particular PdfImage.
        /// </summary>
        public class ImageSelector
        {
            /// <summary>
            /// Initializes a new instance of ImageSelector from an XImage.
            /// </summary>
            public ImageSelector(XImage image)
            {
                // HACK: implement a way to identify images when they are reused
                // TODO 4STLA Implementation that calculates MD5 hashes for images generated for the images can be found here: http://forum.pdfsharp.net/viewtopic.php?p=6959#p6959
                if (image._path == null!)
                    image._path = "*" + Guid.NewGuid().ToString("B");

                // HACK: just use full path to identify
                Path = image._path.ToLowerInvariant();
            }

            public string Path { get; }

            public override bool Equals(object? obj)
            {
                if (obj is not ImageSelector selector)
                    return false;
                return Path == selector.Path;
            }

            public override int GetHashCode()
            {
                return Path.GetHashCode();
            }
        }
    }
}
