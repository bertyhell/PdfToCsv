using System.Collections.Generic;

namespace PdfToCsv
{
    public class GeneticSpecimen
    {
        public List<GeneticRow> Rows { get; private set; }
        public double Score { get; set; }

        public GeneticSpecimen(List<GeneticRow> rows): this(rows, -1) { }

        public GeneticSpecimen(List<GeneticRow> rows, double score)
        {
            Rows = rows;
            Score = score;
        }
    }
}
