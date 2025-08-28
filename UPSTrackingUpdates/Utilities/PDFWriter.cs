using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using iTextSharp.text.pdf;

using iTextSharp.text;
using iTextSharp.tool.xml;
using SimpleImpersonation;

namespace UPSTrackingUpdates.Utilities
{
    public static class PDFWriter
    {
        static string rootPath = $"C:\\SQLCLRDLLs\\SharedResources\\Images\\";

        public static void WritePDF(string html, string fullFilePath)
        {
            var finalStream = new MemoryStream();

            System.Drawing.Image upsLogoImage = System.Drawing.Image.FromFile(rootPath + "ups_logo.gif");

            Image upsLogo = Image.GetInstance(upsLogoImage, System.Drawing.Imaging.ImageFormat.Gif);
            
            Image deliverySignature = null;

            Image deliveryPhotograph = null;

            var deliverySignaturePath = $"{Path.GetDirectoryName(fullFilePath)}\\DeliverySignature.gif";

            if (File.Exists(deliverySignaturePath))
            {
                System.Drawing.Image deliverySignatureImage = System.Drawing.Image.FromFile(deliverySignaturePath);

                deliverySignature = Image.GetInstance(deliverySignatureImage, System.Drawing.Imaging.ImageFormat.Gif);

                deliverySignature.ScaleAbsolute(208.0f, 47.0f);
            }

            var deliveryPhotographPath = $"{Path.GetDirectoryName(fullFilePath)}\\DeliveryPhotograph.gif";

            if (File.Exists(deliveryPhotographPath))
            {
                System.Drawing.Image deliveryPhotographImage = System.Drawing.Image.FromFile(deliverySignaturePath);

                deliveryPhotograph = Image.GetInstance(deliveryPhotographImage, System.Drawing.Imaging.ImageFormat.Gif);

                //deliverySignature.ScalePercent()
            }

            upsLogo.ScaleAbsolute(100.0f, 100.0f);

            var streams = new List<MemoryStream>();

            Byte[] bytes;

            using (var ms = new MemoryStream())
            {
                using (var doc = new Document(PageSize.A4, 125.0f, 25.0f, 25.0f, 25.0f))
                {
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {
                        writer.CloseStream = false;

                        doc.Open();

                        var example_css = "";

                        using (var msCss = new MemoryStream(Encoding.UTF8.GetBytes(example_css)))
                        {
                            using (var msHtml = new MemoryStream(Encoding.UTF8.GetBytes(html)))
                            {
                                XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msHtml, msCss);
                            }
                        }

                        if (deliverySignature != null)
                        {
                            deliverySignature.SpacingBefore = 20.0f;

                            var deliverySignatureChunk = new Chunk("Delivery Signature");

                            var deliverySignaturePhrase = new Phrase(deliverySignatureChunk);

                            var deliverySignatureParagraph = new Paragraph(deliverySignaturePhrase);

                            var deliverySignatureTable = new PdfPTable(1);

                            deliverySignatureTable.SetWidths(new float[] { 100.0f });

                            deliverySignatureTable.WidthPercentage = 100.0f;

                            var deliverySignatureTableCell = new PdfPCell();

                            deliverySignatureTableCell.Border = 0;

                            deliverySignatureTableCell.Padding = 0;

                            deliverySignatureTableCell.AddElement(deliverySignatureParagraph);

                            deliverySignatureTable.AddCell(deliverySignatureTableCell);

                            doc.Add(deliverySignatureTable);

                            doc.Add(deliverySignature);
                        }

                        if (deliveryPhotograph != null)
                        {
                            deliveryPhotograph.SpacingBefore = 20.0f;

                            var deliveryPhotographChunk = new Chunk("Delivery Photograph");

                            var deliveryPhotographPhrase = new Phrase(deliveryPhotographChunk);

                            var deliveryPhotographParagraph = new Paragraph(deliveryPhotographPhrase);

                            var deliveryPhotographTable = new PdfPTable(1);

                            deliveryPhotographTable.SetWidths(new float[] { 100.0f });

                            deliveryPhotographTable.WidthPercentage = 100.0f;

                            var deliveryPhotographTableCell = new PdfPCell();

                            deliveryPhotographTableCell.Border = 0;

                            deliveryPhotographTableCell.Padding = 0;

                            deliveryPhotographTableCell.AddElement(deliveryPhotographParagraph);

                            deliveryPhotographTable.AddCell(deliveryPhotographTableCell);

                            doc.Add(deliveryPhotographTable);

                            doc.Add(deliveryPhotograph);
                        }

                        doc.Close();

                        ms.Position = 0;

                        var stamperStream = new MemoryStream();

                        var reader = new PdfReader(ms);

                        var stamper = new PdfStamper(reader, stamperStream);

                        var count = reader.NumberOfPages;

                        for (var i = 1; i <= count; i++)
                        {
                            var c = stamper.GetOverContent(i);

                            PdfPTable HeaderTable = new PdfPTable(1);

                            HeaderTable.SetWidthPercentage(new float[] { 100.0f }, PageSize.A4);

                            var LeftHeaderCell = new PdfPCell();

                            LeftHeaderCell.Padding = 0;
                            LeftHeaderCell.Border = 0;
                            LeftHeaderCell.AddElement(upsLogo);
                            
                            HeaderTable.AddCell(LeftHeaderCell);

                            HeaderTable.WriteSelectedRows(0, -1, 12, 792, c);
                        }

                        stamper.Close();

                        reader.Close();

                        streams.Add(stamperStream);

                        Document finalDocument = new Document();

                        PdfCopy copy = new PdfCopy(finalDocument, finalStream);

                        copy.CloseStream = false;

                        finalDocument.Open();

                        foreach (var stream in streams)
                        {
                            PdfReader thisReader = new PdfReader(stream.ToArray());

                            for (var i = 0; i < thisReader.NumberOfPages; i++)
                            {
                                copy.AddPage(copy.GetImportedPage(thisReader, i + 1));
                            }

                            copy.FreeReader(thisReader);
                            thisReader.Close();
                        }

                        copy.Close();

                        finalDocument.Close();

                        doc.Close();
                    }
                }

                //After all of the PDF "stuff" above is done and closed but **before** we
                //close the MemoryStream, grab all of the active bytes from the stream
                bytes = finalStream.ToArray();
            }

            //Now we just need to do something with those bytes.
            //Here I'm writing them to disk but if you were in ASP.Net you might Response.BinaryWrite() them.
            //You could also write the bytes to a database in a varbinary() column (but please don't) or you
            //could pass them to another function for further PDF processing.]

            int f = 0;

            var credentials = new UserCredentials("DESIGNERSGUILD\\SQLAgentFileAccess", "34949C02-4677-4711-999C-8E038E26B10F");

            Impersonation.RunAsUser(credentials, LogonType.Interactive, () =>
            {
                File.WriteAllBytes(fullFilePath, bytes);
            });
        }
    }
}
