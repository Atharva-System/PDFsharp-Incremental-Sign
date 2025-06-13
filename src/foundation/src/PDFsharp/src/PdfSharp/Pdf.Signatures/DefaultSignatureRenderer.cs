using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfSharp.Pdf.Signatures
{
    internal class DefaultSignatureRenderer : ISignatureRenderer
    {
        public void Render(XGraphics gfx, XRect rect, PdfSignatureOptions options)
        {
            // if an image was provided, render only that
            if (options.Image != null)
            {
                gfx.DrawImage(options.Image, 0, 0, rect.Width, rect.Height-5);
                //return;
            }

            // if an image was provided, render only that
            if (options.TickImage != null)
            {
                gfx.DrawImage(options.TickImage, 0, 0, rect.Width, rect.Height);
            }

            var sb = new StringBuilder();
            //if (options.Signer != null)
            //{
            //    sb.AppendFormat("Signed by: {0}\n", options.Signer);
            //}    
            //if (options.Location != null)
            //{
            //    sb.AppendFormat("Location: {0}\n", options.Location);
            //}
            //if (options.Reason != null)
            //{
            //    if (options.IncludeReasonText)
            //    {
            //        sb.AppendFormat("Reason: {0}\n", options.Reason);
            //    }
            //    else
            //    {
            //        sb.AppendFormat($"{options.Reason}\n", string.Empty);
            //    }
            //}

            if (options.SignDate != null)
            {
                sb.AppendFormat("Date: {0}\n", options.SignDate);


                XFont font = new XFont("Verdana", options.FontSize, XFontStyleEx.Regular);

                string text = sb.ToString();
                XSize size = gfx.MeasureString(text, font);

                // Calculate X so text is centered horizontally within rect
                double centeredX = (rect.Width - size.Width) / 2;

                XTextFormatter txtFormat = new XTextFormatter(gfx);

                txtFormat.DrawString(sb.ToString(),
                    font,
                    new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)),
                    new XRect(centeredX, rect.Height - 5, rect.Width, rect.Height),
                    XStringFormats.TopLeft);
            }
            //sb.AppendFormat(CultureInfo.CurrentCulture, "Date: {0}", DateTime.Now);

        }
    }
}
