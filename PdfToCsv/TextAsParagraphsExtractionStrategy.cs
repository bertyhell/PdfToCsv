using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf.parser;

namespace PdfToCsv
{
    public class TextAsParagraphsExtractionStrategy : ITextExtractionStrategy
    {
        //Text buffer
        private StringBuilder result = new StringBuilder();

        //Store last used properties
        private LineSegment lastBaseLine;

        //Buffer of lines of text and their Y coordinates. NOTE, these should be exposed as properties instead of fields but are left as is for simplicity's sake
        public List<string> strings = new List<String>();
        public List<float> baselines = new List<float>();

        //This is called whenever a run of text is encountered
        public void RenderText(TextRenderInfo renderInfo)
        {
            //This code assumes that if the baseline changes then we're on a newline
            LineSegment curBaseline = renderInfo.GetBaseline();
            Vector curBaseLineStart = curBaseline.GetStartPoint();
            
            if (lastBaseLine != null)
            {
                // Check if text is on a new line or if text is new block of text on the same line
                // Horizontally compare max coordinate of previous block with min of current block
                // For vertical compare max of both to get the base of the textblock
                var horizontalDifference = Math.Abs(Math.Max(lastBaseLine.GetStartPoint()[0], lastBaseLine.GetEndPoint()[0]) - Math.Min(curBaseline.GetStartPoint()[0], curBaseline.GetEndPoint()[0]));
                var verticalDifference = Math.Abs(Math.Max(lastBaseLine.GetStartPoint()[1], lastBaseLine.GetEndPoint()[1]) - Math.Max(curBaseline.GetStartPoint()[1], curBaseline.GetEndPoint()[1]));
                if (verticalDifference > 1 || horizontalDifference > 20)
                //if ((this.lastBaseLine != null) && (curBaseline[Vector.I2] != lastBaseLine[Vector.I2]))
                {
                    //See if we have text and not just whitespace
                    if ((!String.IsNullOrWhiteSpace(result.ToString())))
                    {
                        //Mark the previous line as done by adding it to our buffers
                        this.baselines.Add(this.lastBaseLine.GetStartPoint()[Vector.I2]);
                        this.strings.Add(this.result.ToString());
                    }
                    //Reset our "line" buffer
                    this.result.Clear();
                }
            }

            //Append the current text to our line buffer
            this.result.Append(renderInfo.GetText());

            //Reset the last used line
            this.lastBaseLine = curBaseline;
        }

        public string GetResultantText()
        {
            //One last time, see if there's anything left in the buffer
            if ((!String.IsNullOrWhiteSpace(this.result.ToString())))
            {
                this.baselines.Add(this.lastBaseLine.GetStartPoint()[Vector.I2]);
                this.strings.Add(this.result.ToString());
            }
            //We're not going to use this method to return a string, instead after callers should inspect this class's strings and baselines fields.
            return null;
        }

        //Not needed, part of interface contract
        public void BeginTextBlock() { }
        public void EndTextBlock() { }
        public void RenderImage(ImageRenderInfo renderInfo) { }
    }
}
