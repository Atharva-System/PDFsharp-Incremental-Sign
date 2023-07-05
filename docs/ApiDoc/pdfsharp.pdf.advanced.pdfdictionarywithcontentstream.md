# PdfDictionaryWithContentStream

Namespace: PdfSharp.Pdf.Advanced

Represents a base class for dictionaries with a content stream.
 Implement IContentStream for use with a content writer.

```csharp
public abstract class PdfDictionaryWithContentStream : PdfSharp.Pdf.PdfDictionary, System.ICloneable, System.Collections.Generic.IEnumerable`1[[System.Collections.Generic.KeyValuePair`2[[System.String, System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e],[PdfSharp.Pdf.PdfItem, PdfSharp, Version=0.1.2.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=6.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], System.Collections.IEnumerable, IContentStream
```

Inheritance [Object](https://docs.microsoft.com/en-us/dotnet/api/system.object) → [PdfItem](./pdfsharp.pdf.pdfitem) → [PdfObject](./pdfsharp.pdf.pdfobject) → [PdfDictionary](./pdfsharp.pdf.pdfdictionary) → [PdfDictionaryWithContentStream](./pdfsharp.pdf.advanced.pdfdictionarywithcontentstream)<br>
Implements [ICloneable](https://docs.microsoft.com/en-us/dotnet/api/system.icloneable), [IEnumerable&lt;KeyValuePair&lt;String, PdfItem&gt;&gt;](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1), [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/system.collections.ienumerable), [IContentStream](./pdfsharp.pdf.advanced.icontentstream)

## Properties

### **Elements**

Gets the dictionary containing the elements of this dictionary.

```csharp
public DictionaryElements Elements { get; }
```

#### Property Value

[DictionaryElements](./pdfsharp.pdf.pdfdictionary.dictionaryelements)<br>

### **Stream**

Gets or sets the PDF stream belonging to this dictionary. Returns null if the dictionary has
 no stream. To create the stream, call the CreateStream function.

```csharp
public PdfStream Stream { get; set; }
```

#### Property Value

[PdfStream](./pdfsharp.pdf.pdfdictionary.pdfstream)<br>

### **Owner**

Gets the PdfDocument this object belongs to.

```csharp
public PdfDocument Owner { get; }
```

#### Property Value

[PdfDocument](./pdfsharp.pdf.pdfdocument)<br>

### **IsIndirect**

Indicates whether the object is an indirect object.

```csharp
public bool IsIndirect { get; }
```

#### Property Value

[Boolean](https://docs.microsoft.com/en-us/dotnet/api/system.boolean)<br>

### **Internals**

Gets the PdfInternals object of this document, that grants access to some internal structures
 which are not part of the public interface of PdfDocument.

```csharp
public PdfObjectInternals Internals { get; }
```

#### Property Value

[PdfObjectInternals](./pdfsharp.pdf.advanced.pdfobjectinternals)<br>

### **Reference**

Gets the indirect reference of this object. If the value is null, this object is a direct object.

```csharp
public PdfReference Reference { get; internal set; }
```

#### Property Value

[PdfReference](./pdfsharp.pdf.advanced.pdfreference)<br>

### **ReferenceNotNull**

Gets the indirect reference of this object. Throws if it is null.

```csharp
public PdfReference ReferenceNotNull { get; }
```

#### Property Value

[PdfReference](./pdfsharp.pdf.advanced.pdfreference)<br>

#### Exceptions

[InvalidOperationException](https://docs.microsoft.com/en-us/dotnet/api/system.invalidoperationexception)<br>
The indirect reference must be not null here.

## Constructors

### **PdfDictionaryWithContentStream()**

Initializes a new instance of the [PdfDictionaryWithContentStream](./pdfsharp.pdf.advanced.pdfdictionarywithcontentstream) class.

```csharp
public PdfDictionaryWithContentStream()
```

### **PdfDictionaryWithContentStream(PdfDocument)**

Initializes a new instance of the [PdfDictionaryWithContentStream](./pdfsharp.pdf.advanced.pdfdictionarywithcontentstream) class.

```csharp
public PdfDictionaryWithContentStream(PdfDocument document)
```

#### Parameters

`document` [PdfDocument](./pdfsharp.pdf.pdfdocument)<br>
The document.

## Methods

### **GetFontName(XFont, PdfFont&)**

```csharp
internal string GetFontName(XFont font, PdfFont& pdfFont)
```

#### Parameters

`font` [XFont](./pdfsharp.drawing.xfont)<br>

`pdfFont` [PdfFont&](./pdfsharp.pdf.advanced.pdffont&)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetFontName(String, Byte[], PdfFont&)**

```csharp
internal string GetFontName(string idName, Byte[] fontData, PdfFont& pdfFont)
```

#### Parameters

`idName` [String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

`fontData` [Byte[]](https://docs.microsoft.com/en-us/dotnet/api/system.byte)<br>

`pdfFont` [PdfFont&](./pdfsharp.pdf.advanced.pdffont&)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetImageName(XImage)**

Gets the resource name of the specified image within this dictionary.

```csharp
internal string GetImageName(XImage image)
```

#### Parameters

`image` [XImage](./pdfsharp.drawing.ximage)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>

### **GetFormName(XForm)**

Gets the resource name of the specified form within this dictionary.

```csharp
internal string GetFormName(XForm form)
```

#### Parameters

`form` [XForm](./pdfsharp.drawing.xform)<br>

#### Returns

[String](https://docs.microsoft.com/en-us/dotnet/api/system.string)<br>