// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.AcroForms.Rendering;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents an interactive form (or AcroForm), a collection of fields for
    /// gathering information interactively from the user.
    /// </summary>
    public sealed class PdfAcroForm : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of AcroForm.
        /// </summary>
        internal PdfAcroForm(PdfDocument document)
            : base(document)
        {
            _document = document;
        }

        internal PdfAcroForm(PdfDictionary dictionary)
            : base(dictionary)
        { }

        /// <summary>
        /// Gets the fields collection of this form.
        /// </summary>
        public PdfAcroField.PdfAcroFieldCollection Fields
        {
            get
            {
                if (_fields == null)
                {
                    var o = Elements.GetValue(Keys.Fields, VCF.CreateIndirect);
                    _fields = (PdfAcroField.PdfAcroFieldCollection?)o ?? NRT.ThrowOnNull<PdfAcroField.PdfAcroFieldCollection>();
                }
                return _fields;
            }
        }
        PdfAcroField.PdfAcroFieldCollection? _fields;

        internal PdfResources? Resources
        {
            get
            {
                if (resources == null)
                    resources = (PdfResources?)Elements.GetValue(Keys.DR, VCF.None);
                return resources;
            }
        }
        PdfResources? resources;

        /// <summary>
        /// Gets the <see cref="PdfResources"/> of this <see cref="PdfAcroForm"/> or creates a new one if none exist
        /// </summary>
        /// <returns>The <see cref="PdfResources"/> of this AcroForm</returns>
        internal PdfResources GetOrCreateResources()
        {
            var resources = Resources;
            if (resources == null)
                Elements.Add(Keys.DR, new PdfResources(_document));
            return Resources!;
        }

        private PdfAcroFieldRenderer? fieldRenderer;
        /// <summary>
        /// Gets the <see cref="PdfAcroFieldRenderer"/> used to render <see cref="PdfAcroField"/>s
        /// </summary>
        public PdfAcroFieldRenderer FieldRenderer
        {
            get
            {
                fieldRenderer ??= new PdfAcroFieldRenderer();
                return fieldRenderer;
            }
        }

        internal override void PrepareForSave()
        {
            // Need to create "Fields" Entry after importing fields from external documents
            if (_fields != null && _fields.Elements.Count > 0 && !Elements.ContainsKey(Keys.Fields))
            {
                Elements.Add(Keys.Fields, _fields);
            }
            // do not use the Fields-Property, as that may create new unwanted fields !
            var fieldsArray = Elements.GetArray(Keys.Fields);
            if (fieldsArray != null)
            {
                for (var i = 0; i < fieldsArray.Elements.Count; i++)
                {
                    var field = fieldsArray.Elements[i] as PdfReference;
                    if (field != null && field.Value != null)
                        field.Value.PrepareForSave();
                }
            }
            base.PrepareForSave();
        }

        /// <summary>
        /// Flattens the AcroForm by rendering Field-contents directly onto the page
        /// </summary>
        public void Flatten()
        {
            for (var i = 0; i < Fields.Elements.Count; i++)
            {
                var field = Fields[i];
                field.Flatten();
            }
            _document.Catalog.AcroForm = null;
        }

        /// <summary>
        /// Adds a new <see cref="PdfTextField"/> to the <see cref="PdfAcroForm"/>
        /// </summary>
        /// <param name="configure">
        /// A method that receives the new <see cref="PdfTextField"/> for further customization<br></br>
        /// </param>
        /// <returns>The created and configured <see cref="PdfTextField"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfTextField AddTextField(Action<PdfTextField> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var field = new PdfTextField(_document);
            return AddToFieldList(field, configure);
        }

        /// <summary>
        /// Adds a new <see cref="PdfCheckBoxField"/> to the <see cref="PdfAcroForm"/>
        /// </summary>
        /// <param name="configure">
        /// A method that receives the new <see cref="PdfCheckBoxField"/> for further customization<br></br>
        /// </param>
        /// <returns>The created and configured <see cref="PdfCheckBoxField"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfCheckBoxField AddCheckBoxField(Action<PdfCheckBoxField> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var field = new PdfCheckBoxField(_document);
            return AddToFieldList(field, configure);
        }

        /// <summary>
        /// Adds a new <see cref="PdfRadioButtonField"/> to the <see cref="PdfAcroForm"/>
        /// </summary>
        /// <param name="configure">
        /// A method that receives the new <see cref="PdfRadioButtonField"/> for further customization<br></br>
        /// </param>
        /// <returns>The created and configured <see cref="PdfRadioButtonField"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfRadioButtonField AddRadioButtonField(Action<PdfRadioButtonField> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var field = new PdfRadioButtonField(_document);
            return AddToFieldList(field, configure);
        }

        /// <summary>
        /// Adds a new <see cref="PdfComboBoxField"/> to the <see cref="PdfAcroForm"/>
        /// </summary>
        /// <param name="configure">
        /// A method that receives the new <see cref="PdfComboBoxField"/> for further customization<br></br>
        /// </param>
        /// <returns>The created and configured <see cref="PdfComboBoxField"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfComboBoxField AddComboBoxField(Action<PdfComboBoxField> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var field = new PdfComboBoxField(_document);
            return AddToFieldList(field, configure);
        }

        /// <summary>
        /// Adds a new <see cref="PdfListBoxField"/> to the <see cref="PdfAcroForm"/>
        /// </summary>
        /// <param name="configure">
        /// A method that receives the new <see cref="PdfListBoxField"/> for further customization<br></br>
        /// </param>
        /// <returns>The created and configured <see cref="PdfListBoxField"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfListBoxField AddListBoxField(Action<PdfListBoxField> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var field = new PdfListBoxField(_document);
            return AddToFieldList(field, configure);
        }

        /// <summary>
        /// Adds a new <see cref="PdfPushButtonField"/> to the <see cref="PdfAcroForm"/>
        /// </summary>
        /// <param name="configure">
        /// A method that receives the new <see cref="PdfPushButtonField"/> for further customization<br></br>
        /// </param>
        /// <returns>The created and configured <see cref="PdfPushButtonField"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfPushButtonField AddPushButtonField(Action<PdfPushButtonField> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var field = new PdfPushButtonField(_document);
            return AddToFieldList(field, configure);
        }

        /// <summary>
        /// Adds a new <see cref="PdfSignatureField"/> to the <see cref="PdfAcroForm"/>
        /// </summary>
        /// <param name="configure">
        /// A method that receives the new <see cref="PdfSignatureField"/> for further customization<br></br>
        /// </param>
        /// <returns>The created and configured <see cref="PdfSignatureField"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfSignatureField AddSignatureField(Action<PdfSignatureField> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var field = new PdfSignatureField(_document);
            return AddToFieldList(field, configure);
        }

        /// <summary>
        /// Adds a new <see cref="PdfGenericField"/> to the <see cref="PdfAcroForm"/><br></br>
        /// Typically used as a container for other fields
        /// </summary>
        /// <param name="configure">
        /// A method that receives the new <see cref="PdfGenericField"/> for further customization<br></br>
        /// </param>
        /// <returns>The created and configured <see cref="PdfGenericField"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfGenericField AddGenericField(Action<PdfGenericField> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            var field = new PdfGenericField(_document);
            return AddToFieldList(field, configure);
        }

        private T AddToFieldList<T>(T field, Action<T> configure) where T: PdfAcroField
        {
            _document.IrefTable.Add(field);
            configure(field);
            if (field.Parent == null)
            {
                // ensure names of root-fields are unique
                var existingField = Fields.GetValue(field.Name);
                if (existingField != null)
                {
                    var name = existingField.Name.AddIncrementalSuffix();
                    // search for next free number
                    while (Fields.GetValue(name) != null)
                    {
                        name = name.AddIncrementalSuffix();
                    }
                    field.Name = name;
                }
                Fields.Elements.Add(field);
            }
            return field;
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public sealed class Keys : KeysBase
        {
            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Required) An array of references to the documentís root fields (those with
            /// no ancestors in the field hierarchy).
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required, typeof(PdfAcroField.PdfAcroFieldCollection))]
            public const string Fields = "/Fields";

            /// <summary>
            /// (Optional) A flag specifying whether to construct appearance streams and
            /// appearance dictionaries for all widget annotations in the document.
            /// Default value: false.
            /// </summary>
            [KeyInfo(KeyType.Boolean | KeyType.Optional)]
            public const string NeedAppearances = "/NeedAppearances";

            /// <summary>
            /// (Optional; PDF 1.3) A set of flags specifying various document-level characteristics
            /// related to signature fields.
            /// Default value: 0.
            /// </summary>
            [KeyInfo("1.3", KeyType.Integer | KeyType.Optional)]
            public const string SigFlags = "/SigFlags";

            /// <summary>
            /// (Required if any fields in the document have additional-actions dictionaries
            /// containing a C entry; PDF 1.3) An array of indirect references to field dictionaries
            /// with calculation actions, defining the calculation order in which their values will 
            /// be recalculated when the value of any field changes.
            /// </summary>
            [KeyInfo(KeyType.Array)]
            public const string CO = "/CO";

            /// <summary>
            /// (Optional) A document-wide default value for the DR attribute of variable text fields.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResources))]
            public const string DR = "/DR";

            /// <summary>
            /// (Optional) A document-wide default value for the DA attribute of variable text fields.
            /// </summary>
            [KeyInfo(KeyType.String | KeyType.Optional)]
            public const string DA = "/DA";

            /// <summary>
            /// (Optional) A document-wide default value for the Q attribute of variable text fields.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string Q = "/Q";

            /// <summary>
            /// (Optional; PDF 1.5) A stream or array containing an XFA resource,
            /// whose format shall be described by the Data Package (XDP) Specification. (see the Bibliography).<br></br>
            /// The value of this entry shall be either a stream representing the entire contents
            /// of the XML Data Package or an array of text string and stream pairs
            /// representing the individual packets comprising the XML Data Package.
            /// </summary>
            [KeyInfo(KeyType.ArrayOrDictionary | KeyType.Optional)]
            public const string XFA = "/XFA";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta
            {
                get
                {
                    if (s_meta == null)
                        s_meta = CreateMeta(typeof(Keys));
                    return s_meta;
                }
            }

            static DictionaryMeta? s_meta;

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
