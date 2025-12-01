using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Features.Pdf
{
    public static class PdfMergeHelper
    {
        /// <summary>
        /// Merges the specified input PDF file paths into a single PdfDocument (in memory).
        /// Caller is responsible to call Dispose on the returned PdfDocument when done.
        /// </summary>
        /// <param name="inputPaths">Paths to input PDF files (order preserved).</param>
        /// <returns>Merged PdfDocument.</returns>
        public static PdfDocument MergeToDocument(IEnumerable<string> inputPaths)
        {
            if (inputPaths == null)
                throw new ArgumentNullException(nameof(inputPaths));

            var result = new PdfDocument();

            foreach (var path in inputPaths)
            {
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                // Open source as Import so pages can be reused.
                using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);

                for (int i = 0; i < input.PageCount; i++)
                {
                    // Adding an imported page imports it into the result document.
                    result.AddPage(input.Pages[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Merges the specified input PDF file paths and saves the result to <paramref name="outputPath"/>.
        /// Overwrites output file if it exists.
        /// </summary>
        /// <param name="inputPaths">Paths to input PDF files (order preserved).</param>
        /// <param name="outputPath">Output file path.</param>
        public static void MergeToFile(IEnumerable<string> inputPaths, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path must be provided.", nameof(outputPath));

            using var merged = MergeToDocument(inputPaths);
            merged.Save(outputPath);
        }

        /// <summary>
        /// Merges PDF documents supplied as byte arrays into a single PdfDocument (in memory).
        /// Caller is responsible to call Dispose on the returned PdfDocument when done.
        /// </summary>
        /// <param name="inputBytes">PDF files as byte arrays (order preserved).</param>
        /// <returns>Merged PdfDocument.</returns>
        public static PdfDocument MergeToDocument(IEnumerable<byte[]> inputBytes)
        {
            if (inputBytes == null)
                throw new ArgumentNullException(nameof(inputBytes));

            var result = new PdfDocument();

            foreach (var bytes in inputBytes)
            {
                if (bytes == null || bytes.Length == 0)
                    continue;

                using var ms = new MemoryStream(bytes);
                using var input = PdfReader.Open(ms, PdfDocumentOpenMode.Import);

                for (int i = 0; i < input.PageCount; i++)
                    result.AddPage(input.Pages[i]);
            }

            return result;
        }

        /// <summary>
        /// Merges PDF documents supplied as byte arrays and saves the result to <paramref name="outputPath"/>.
        /// </summary>
        /// <param name="inputBytes">PDF files as byte arrays (order preserved).</param>
        /// <param name="outputPath">Output file path.</param>
        public static void MergeToFile(IEnumerable<byte[]> inputBytes, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path must be provided.", nameof(outputPath));

            using var merged = MergeToDocument(inputBytes);
            merged.Save(outputPath);
        }

        /// <summary>
        /// Merges PDF documents supplied as streams into a single PdfDocument (in memory).
        /// Streams are read and must remain readable/seekable for the duration of the operation.
        /// The method will not close the provided streams.
        /// Caller is responsible to call Dispose on the returned PdfDocument when done.
        /// </summary>
        /// <param name="inputStreams">PDF files as streams (order preserved).</param>
        /// <returns>Merged PdfDocument.</returns>
        public static PdfDocument MergeToDocument(IEnumerable<Stream> inputStreams)
        {
            if (inputStreams == null)
                throw new ArgumentNullException(nameof(inputStreams));

            var result = new PdfDocument();

            foreach (var stream in inputStreams)
            {
                if (stream == null)
                    continue;

                // PdfReader.Open may require the stream to be seekable; if not, copy to MemoryStream.
                Stream useStream = stream;
                bool createdMemoryStream = false;
                if (!stream.CanSeek)
                {
                    useStream = new MemoryStream();
                    stream.CopyTo(useStream);
                    useStream.Position = 0;
                    createdMemoryStream = true;
                }
                else
                {
                    // Ensure we start at the beginning
                    if (useStream.Position != 0)
                        useStream.Position = 0;
                }

                using var input = PdfReader.Open(useStream, PdfDocumentOpenMode.Import);

                for (int i = 0; i < input.PageCount; i++)
                    result.AddPage(input.Pages[i]);

                if (createdMemoryStream)
                    useStream.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Merges PDF documents supplied as streams and saves the result to <paramref name="outputPath"/>.
        /// </summary>
        /// <param name="inputStreams">PDF files as streams (order preserved).</param>
        /// <param name="outputPath">Output file path.</param>
        public static void MergeToFile(IEnumerable<Stream> inputStreams, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path must be provided.", nameof(outputPath));

            using var merged = MergeToDocument(inputStreams);
            merged.Save(outputPath);
        }

        /// <summary>
        /// Merge exactly two PDF byte arrays and return the merged PDF as a byte array.
        /// </summary>
        /// <param name="pdf1Bytes">First PDF bytes (order preserved).</param>
        /// <param name="pdf2Bytes">Second PDF bytes (order preserved).</param>
        /// <returns>Merged PDF as byte array.</returns>
        public static byte[] MergeTwoPdfBytes(byte[] pdf1Bytes, byte[] pdf2Bytes)
        {
            if (pdf1Bytes == null)
                throw new ArgumentNullException(nameof(pdf1Bytes));
            if (pdf2Bytes == null)
                throw new ArgumentNullException(nameof(pdf2Bytes));

            var inputs = new[] { pdf1Bytes, pdf2Bytes };

            using var merged = MergeToDocument(inputs);
            using var ms = new MemoryStream();
            merged.Save(ms);
            return ms.ToArray();
        }
    }
}