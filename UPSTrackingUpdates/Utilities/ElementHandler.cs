using iTextSharp.text;
using System.Collections.Generic;

using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline;

namespace UPSTrackingUpdates.Utilities
{
    internal class ElementHandler : IElementHandler
    {
        public void Add(IWritable w)
        {
            if (w is WritableElement) 
            {
                var elements = ((WritableElement)w).Elements();

                foreach (var e in elements)
                {
                    ((WritableElement)w).Add(e);
                }
            }
        }
    }
}