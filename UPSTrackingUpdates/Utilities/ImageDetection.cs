using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace UPSTrackingUpdates
{
    // with thanks to Jared Beach https://stackoverflow.com/questions/5209506/how-can-i-know-what-image-format-i-get-from-a-stream#answer-66055866

    // based on https://devblogs.microsoft.com/scripting/psimaging-part-1-test-image/
    public class ImageDetector
    {
        // https://en.wikipedia.org/wiki/List_of_file_signatures
        /* Bytes in c# have a range of 0 to 255 so each byte can be represented as
         * a two digit hex string. */
        private static readonly Dictionary<ImageFormat, string[][]> SignatureTable = new Dictionary<ImageFormat, string[][]>
        {
            {
                ImageFormat.Jpeg,
                new[]
                {
                    new[] {"FF", "D8", "FF", "DB"},
                    new[] {"FF", "D8", "FF", "EE"},
                    new[] {"FF", "D8", "FF", "E0", "00", "10", "4A", "46", "49", "46", "00", "01"}
                }
            },
            {
                ImageFormat.Gif,
                new[]
                {
                    new [] { "47", "49", "46", "38", "37", "61" },
                    new [] { "47", "49", "46", "38", "39", "61" }
                }
            },
            {
                ImageFormat.Png,
                new[]
                {
                    new[] {"89", "50", "4E", "47", "0D", "0A", "1A", "0A"}
                }
            },
            {
                ImageFormat.Bmp,
                new []
                {
                    new[] { "42", "4D" }
                }
            }
        };

        /// <summary>
        /// Takes a byte array and determines the image file type by
        /// comparing the first few bytes of the file to a list of known
        /// image file signatures.
        /// </summary>
        /// <param name="imageData">Byte array of the image data</param>
        /// <returns>ImageFormat corresponding to the image file format</returns>
        /// <exception cref="ArgumentException">Thrown if the image type can't be determined</exception>
        public static ImageFormat GetImageType(byte[] imageData)
        {
            foreach (KeyValuePair<ImageFormat, string[][]> signatureEntry in SignatureTable)
            {
                foreach (string[] signature in signatureEntry.Value)
                {
                    bool isMatch = true;
                    for (int i = 0; i < signature.Length; i++)
                    {
                        string signatureByte = signature[i];

                        // ToString("X") gets the hex representation and pads it to always be length 2
                        string imageByte = imageData[i]
                            .ToString("X2");

                        if (signatureByte == imageByte)
                            continue;
                        isMatch = false;
                        break;
                    }

                    if (isMatch)
                    {
                        return signatureEntry.Key;
                    }
                }
            }

            throw new ArgumentException("The byte array did not match any known image file signatures.");
        }
    }
}