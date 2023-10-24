﻿using PdfSharp.Drawing;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Fonts.StandardFonts;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Fonts
{
    /// <summary>
    /// Resolves the 14 standard-fonts and/or fonts in existing documents.<br></br>
    /// </summary>
    public class DocumentFontResolver : IFontResolver
    {
        private static readonly Dictionary<string, byte[]> localFonts = new();

        private readonly PdfDocument? document;
        private readonly PdfAcroField? acroField;

        /// <summary>
        /// Registers a new font
        /// </summary>
        /// <param name="fontName">The name of the font</param>
        /// <param name="fontData">The font-data</param>
        /// <param name="isBold">Specifies, whether the font is bold</param>
        /// <param name="isItalic">Specifies, whether the font is italic</param>
        public static void Register(string fontName, byte[] fontData, bool isBold = false, bool isItalic = false)
        {
            var localName = MakeLocalName(fontName, isBold, isItalic);
            localFonts[localName] = fontData;
            // get the name stored in the font itself
            var fontSource = XFontSource.GetOrCreateFrom(fontData);
            var typeFace = OpenTypeFontface.GetOrCreateFrom(fontSource);
            if (!string.IsNullOrEmpty(typeFace.FullFaceName))
            {
                localName = MakeLocalName(typeFace.FullFaceName, isBold, isItalic);
                localFonts[localName] = fontData;
            }    
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DocumentFontResolver"/>
        /// </summary>
        /// <param name="document"></param>
        public DocumentFontResolver(PdfDocument? document = null)
        {
            this.document = document;
        }

        /// <summary>
        /// Internal constructor used by <see cref="PdfAcroField"/> to resolve fonts in an existing document.
        /// </summary>
        /// <param name="field"></param>
        internal DocumentFontResolver(PdfAcroField field)
        {
            document = field.Owner;
            acroField = field;
        }

        /// <summary>
        /// Gets the data for the specified font.
        /// </summary>
        /// <param name="faceName">Name of the font</param>
        /// <returns>Font data or null, if no font with the specified name could be found</returns>
        public byte[]? GetFont(string faceName)
        {
            var result = Resolve(faceName, false, false);
            return result.Item1;
        }

        /// <summary>
        /// Get a <see cref="FontResolverInfo"/> for the specified font
        /// </summary>
        /// <param name="familyName">Name of the font</param>
        /// <param name="isBold"></param>
        /// <param name="isItalic"></param>
        /// <returns>A <see cref="FontResolverInfo"/> or null, if no font with the specified name could be found</returns>
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            var result = Resolve(familyName, isBold, isItalic);
            return result.Item2;
        }

        private static string MakeLocalName(string fontName, bool isBold, bool isItalic)
        {
            var localName = fontName;
            if (isBold || isItalic)
            {
                localName += "+";
                if (isBold)
                    localName += "b";
                if (isItalic)
                    localName += "i";
            }
            return localName;
        }

        private Tuple<byte[]?, FontResolverInfo?> Resolve(string fontName, bool isBold, bool isItalic)
        {
            var localName = MakeLocalName(fontName, isBold, isItalic);
            if (localFonts.TryGetValue(localName, out var localData))
                return new Tuple<byte[]?, FontResolverInfo?>(localData, new FontResolverInfo(fontName, isBold, isItalic));

            var data = StandardFontData.GetFontData(fontName);
            if (data != null)
            {
                return new Tuple<byte[]?, FontResolverInfo?>(data, new FontResolverInfo(fontName, isBold, isItalic));
            }
            if (document == null)
                return new Tuple<byte[]?, FontResolverInfo?>(null, null);

            // in a document, fonts are referenced by their name
            if (!fontName.StartsWith('/'))
                fontName = "/" + fontName;

            var possibleResources = new List<PdfDictionary?>
            {
                document.AcroForm?.Elements.GetDictionary(PdfAcroForm.Keys.DR),
                acroField?.Elements.GetDictionary(PdfAcroForm.Keys.DR)
            };
            foreach (var page in document.Pages)
            {
                possibleResources.Add(page.Resources);
            }
            foreach (var resources in possibleResources)
            {
                if (resources != null && resources.Elements.ContainsKey("/Font"))
                {
                    var fontList = resources.Elements.GetDictionary("/Font");
                    var fontRef = fontList?.Elements.GetReference(fontName);
                    if (fontRef != null)
                    {
                        var fontDict = fontRef.Value as PdfDictionary;
                        var descriptor = fontDict?.Elements.GetDictionary(PdfFont.Keys.FontDescriptor);
                        if (descriptor != null)
                        {
                            var fileRef = descriptor.Elements.GetDictionary(PdfFontDescriptor.Keys.FontFile2);
                            if (fileRef != null)
                            {
                                var fontData = fileRef?.Stream.UnfilteredValue;
                                return new Tuple<byte[]?, FontResolverInfo?>(fontData, new FontResolverInfo(fontName.TrimStart('/'), isBold, isItalic));
                            }
                        }
                    }
                    else if (fontList != null)
                    {
                        // may be a Type0 Font, dig a bit deeper
                        foreach (var key in fontList.Elements.Keys)
                        {
                            if (fontList.Elements.GetObject(key) is PdfDictionary value)
                            {
                                var baseFont = value.Elements.GetName(PdfType0Font.Keys.BaseFont);
                                if (baseFont == fontName)
                                {
                                    var descendantFonts = value.Elements.GetArray(PdfType0Font.Keys.DescendantFonts);
                                    if (descendantFonts != null)
                                    {
                                        foreach (var descendantFont in descendantFonts.Elements)
                                        {
                                            var fontDict = descendantFont is PdfReference fref
                                                ? fref.Value as PdfDictionary
                                                : descendantFont as PdfDictionary;
                                            var descriptor = fontDict?.Elements.GetDictionary(PdfFont.Keys.FontDescriptor);
                                            if (descriptor != null)
                                            {
                                                var fileRef = descriptor.Elements.GetDictionary(PdfFontDescriptor.Keys.FontFile2);
                                                if (fileRef != null)
                                                {
                                                    var fontData = fileRef?.Stream.UnfilteredValue;
                                                    return new Tuple<byte[]?, FontResolverInfo?>(fontData, new FontResolverInfo(fontName.TrimStart('/'), isBold, isItalic));
                                                }
                                            }
                                        }
                                    }
                                    else if (value.Elements.ContainsKey(PdfFont.Keys.FontDescriptor))
                                    {
                                        var descriptor = value?.Elements.GetDictionary(PdfFont.Keys.FontDescriptor);
                                        if (descriptor != null)
                                        {
                                            var fileRef = descriptor.Elements.GetDictionary(PdfFontDescriptor.Keys.FontFile2);
                                            if (fileRef != null)
                                            {
                                                var fontData = fileRef?.Stream.UnfilteredValue;
                                                return new Tuple<byte[]?, FontResolverInfo?>(fontData, new FontResolverInfo(fontName.TrimStart('/'), isBold, isItalic));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new Tuple<byte[]?, FontResolverInfo?>(null, null);
        }
    }
}