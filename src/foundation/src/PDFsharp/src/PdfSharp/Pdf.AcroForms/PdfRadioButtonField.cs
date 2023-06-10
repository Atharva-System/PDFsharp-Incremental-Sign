// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf.Annotations;
using System.Collections.ObjectModel;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the radio button field.
    /// </summary>
    public sealed class PdfRadioButtonField : PdfButtonField
    {
        /// <summary>
        /// Initializes a new instance of PdfRadioButtonField.
        /// </summary>
        internal PdfRadioButtonField(PdfDocument document)
            : base(document)
        {
            _document = document;
            SetFlags |= PdfAcroFieldFlags.Radio;
        }

        internal PdfRadioButtonField(PdfDictionary dict)
            : base(dict)
        {
            if (!Elements.ContainsKey(Keys.Opt))
            {
                var array = new PdfArray(_document);
                foreach (var val in Options)
                    array.Elements.Add(new PdfString(val));
                Elements.Add(Keys.Opt, array);
            }
        }

        /// <summary>
        /// Gets or sets the value of this field. This should be an item from the <see cref="Options"/> list.<br></br>
        /// Setting this to null or an empty string unchecks all radio-buttons.
        /// </summary>
        public new string Value
        {
            get { return (base.Value?.ToString() ?? string.Empty).TrimStart('/'); }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Elements.SetName(PdfAcroField.Keys.V, value);
                    var index = IndexInFieldValues(value);
                    SelectedIndex = index;
                }
                else
                    SelectedIndex = -1;
            }
        }

        private List<string>? options;

        /// <summary>
        /// Gets the option-names of this RadioButton<br></br>
        /// Use one of these values when setting <see cref="Value"/><br></br>
        /// You cannot manipulate this collection directly.
        /// To change the elements you have to manipulate the <see cref="PdfAcroField.Annotations"/> of this field.
        /// </summary>
        public ReadOnlyCollection<string> Options
        {
            get
            {
                if (options != null)
                    return options.AsReadOnly();

                var values = new List<string>();
                for (var i = 0; i < Annotations.Elements.Count; i++)
                {
                    var widget = Annotations.Elements[i];
                    if (widget == null)
                        continue;
                    // convert names to ordinary strings by removing the slash
                    values.Add((GetNonOffValue(widget) ?? i.ToString()).TrimStart('/'));
                }
                options = values;
                return options.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets or sets the (optional) export-values for each entry in this radio button group.<br></br>
        /// If the field does not specify these, <b><see cref="Options"/></b> is returned.
        /// </summary>
        public ICollection<string> ExportValues
        {
            get
            {
                var opt = Elements.GetArray(Keys.Opt);
                if (opt != null)
                {
                    var list = new List<string>();
                    for (var i = 0; i < opt.Elements.Count; i++)
                        list.Add(opt.Elements.GetString(i));
                    return list;
                }
                return Options;
            }
            set
            {
                if (value.Count != Options.Count)
                    throw new ArgumentException("Length of Opt-Array must match length of Options");
                var optArray = new PdfArray();
                foreach (var val in value)
                    optArray.Elements.Add(new PdfString(val));
                Elements[Keys.Opt] = optArray;
            }
        }

        /// <summary>
        /// Gets or sets the (zero-based) index of the selected radio button in a radio button group.<br></br>
        /// Use -1 to deselect all items.<br></br>
        /// This is an alternative to the <see cref="Value"/>
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return IndexInFieldValues(Value);
            }
            set
            {
                var values = Options;
                var count = values.Count;
                if (value < -1 || value >= count)
                    throw new ArgumentOutOfRangeException(nameof(value));
                var name = value == -1 ? "/Off" : values.ElementAt(value);
                Elements.SetName(PdfAcroField.Keys.V, name);
                // first, set all annotations to /Off
                for (var i = 0; i < Annotations.Elements.Count; i++)
                {
                    var widget = Annotations.Elements[i];
                    widget?.Elements.SetName(PdfAnnotation.Keys.AS, "/Off");
                }
                if ((Flags & PdfAcroFieldFlags.RadiosInUnison) != 0)
                {
                    // Then set all Widgets with the same Appearance to the checked state
                    for (var i = 0; i < Annotations.Elements.Count; i++)
                    {
                        var widget = Annotations.Elements[i];
                        if (name == values.ElementAt(i) && widget != null)
                            widget.Elements.SetName(PdfAnnotation.Keys.AS, name);
                    }
                }
                else
                {
                    if (value >= 0 && value < Annotations.Elements.Count)
                    {
                        var widget = Annotations.Elements[value];
                        widget?.Elements.SetName(PdfAnnotation.Keys.AS, name);
                    }
                }
            }
        }

        private int IndexInFieldValues(string value)
        {
            return Options.IndexOf(value);
        }

        protected override void RenderAppearance()
        {
            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                var rect = widget.Rectangle;
                if (widget.Page != null && !rect.IsEmpty)
                {
                    // existing/imported field ?
                    if (widget.Elements.ContainsKey(PdfAnnotation.Keys.AP))
                    {
                        widget.Elements.SetName(PdfAnnotation.Keys.AS, i == SelectedIndex ? Options.ElementAt(i) : "/Off");
                    }
                    else
                        CreateAppearance(widget, GetNonOffValue(widget) ?? "/Yes");
                }
            }
        }

        /// <summary>
        /// Creates the appearance-stream for the specified Widget.
        /// </summary>
        /// <param name="widget"></param>
        /// <param name="nameOfOnState"></param>
        private void CreateAppearance(PdfWidgetAnnotation widget, string nameOfOnState)
        {
            // remove possible leading slashes (will be re-added later)
            nameOfOnState = nameOfOnState.TrimStart('/');

            var rect = widget.Rectangle;
            if (widget.Page != null && !rect.IsEmpty)
            {
                var xRect = new XRect(0, 0, Math.Max(1, rect.Width), Math.Max(1, rect.Height));
                // checked state
                var formChecked = new XForm(_document, xRect);
                using (var gfx = XGraphics.FromForm(formChecked))
                {
                    gfx.IntersectClip(xRect);
                    Owner.AcroForm?.FieldRenderer.RadioButtonFieldRenderer.RenderCheckedState(this, widget, gfx, xRect);
                }
                formChecked.DrawingFinished();

                // unchecked state
                var formUnchecked = new XForm(_document, rect.ToXRect());
                using (var gfx = XGraphics.FromForm(formUnchecked))
                {
                    gfx.IntersectClip(xRect);
                    Owner.AcroForm?.FieldRenderer.RadioButtonFieldRenderer.RenderUncheckedState(this, widget, gfx, xRect);
                }
                formUnchecked.DrawingFinished();

                var ap = new PdfDictionary(_document);
                var nDict = new PdfDictionary(_document);
                ap.Elements.SetValue("/N", nDict);
                nDict.Elements[new PdfName("/" + nameOfOnState)] = formChecked.PdfForm.Reference;
                nDict.Elements["/Off"] = formUnchecked.PdfForm.Reference;
                widget.Elements[PdfAnnotation.Keys.AP] = ap;
            }
        }

        /// <summary>
        /// A special overload for RadioButtons that allows specifying the name for the "On"-State of an individual radio-button
        /// </summary>
        /// <param name="nameOfOnState">Name of the "On"-State of this RadioButton</param>
        /// <param name="configure">A method that is used to configure the Annotation</param>
        /// <returns>The created and configured Annotation</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public PdfWidgetAnnotation AddAnnotation(string nameOfOnState, Action<PdfWidgetAnnotation> configure)
        {
            if (string.IsNullOrWhiteSpace(nameOfOnState))
                throw new ArgumentNullException(nameof(nameOfOnState), "Name of state must not be null or empty");

            var annot = AddAnnotation(configure);
            CreateAppearance(annot, nameOfOnState);
            options = null; // reset. next access to Options will re-create them
            return annot;
        }

        internal override void PrepareForSave()
        {
            base.PrepareForSave();
            RenderAppearance();
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfButtonField.Keys
        {
            /// <summary>
            /// (Optional; inheritable; PDF 1.4) An array of text strings to be used in
            /// place of the V entries for the values of the widget annotations representing
            /// the individual radio buttons. Each element in the array represents
            /// the export value of the corresponding widget annotation in the
            /// Kids array of the radio button field.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Opt = "/Opt";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
