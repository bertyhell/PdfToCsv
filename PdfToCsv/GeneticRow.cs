using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfToCsv
{
    public class GeneticRow
    {
        public List<GeneticCell> Cells { get; private set; }
        public bool[] ColumnSeperators { get; private set; }

        private int _shortestRowLength;
        private Random _rand;

        public GeneticRow(List<string> cells, int shortestRowLength, bool[] columnSeperators, Random rand)
        {
            _rand = rand;
            _shortestRowLength = shortestRowLength;
            Cells = new List<GeneticCell>();
            foreach (string cellContent in cells)
            {
                Cells.Add(GeneticRowOptimizer.GetCachedGeneticCell(cellContent));
            }
            if (columnSeperators == null)
            {
                // Randomize column seperators
                ColumnSeperators = GenerateRandomBoolArray(cells.Count - 1, shortestRowLength - 1, rand);
            } else
            {
                if (columnSeperators.Length != cells.Count - 1)
                {
                    throw new Exception("column seperator length is incompatible with the specified shortestRowLength. column seperator length should be equal to the shortest row length - 1");
                }
                ColumnSeperators = columnSeperators;
            }
        }

        public void Mutate(double severity)
        {
            // Mutate the current row seperators a bit
            if (ColumnSeperators.All(c => c))
            {
                // all column seperators are required, cannot mutate this row
                return;
            }

            // Find the indices of all true values and all false values
            List<int> indexesWithTrueValue = new List<int>();
            List<int> indexesWithFalseValue = new List<int>();
            for (int i = 0; i < ColumnSeperators.Length; i++)
            {
                if (ColumnSeperators[i])
                {
                    indexesWithTrueValue.Add(i);
                } else
                {
                    indexesWithFalseValue.Add(i);
                }
            }
            indexesWithTrueValue.OrderBy(index => _rand.Next()).ToList();
            indexesWithFalseValue.OrderBy(index => _rand.Next()).ToList();

            // Pick a few true false pairs and swap their locations in the ColumnSeperator array
            int maxSwapsPossible = Math.Min(indexesWithTrueValue.Count, indexesWithFalseValue.Count);
            int numberOfSeperatorsToSwap = (int)Math.Ceiling(maxSwapsPossible * severity);
            for (int i = 0; i < numberOfSeperatorsToSwap; i++)
            {
                ColumnSeperators[indexesWithTrueValue[i]] = false;
                ColumnSeperators[indexesWithFalseValue[i]] = true;
            }
        }

        public GeneticCell GetMergedCellByIndex(int desiredMergedIndex)
        {
            // Get the cells that need to be merged
            int unmergedIndex = 0;
            int mergedIndex = 0;
            while (mergedIndex < desiredMergedIndex) // Move to the desired position in the unmerged array
            {
                if (ColumnSeperators[unmergedIndex])
                {
                    unmergedIndex++;
                    mergedIndex++;
                } else
                {
                    unmergedIndex++;
                }
            }

            // Merge the cells as long as we do not encounter a column seperator
            GeneticCell mergedCell = Cells[unmergedIndex];
            while (unmergedIndex < ColumnSeperators.Length && !ColumnSeperators[unmergedIndex]) // from this position merge cells to the right while columnSeperator is false
            {
                unmergedIndex++;
                mergedCell += Cells[unmergedIndex];
            }
            return mergedCell;
        }

        private bool[] GenerateRandomBoolArray(int length)
        {
            // TODO this might be less limiting but has a lower chance of successfully coming with an answer
            throw new NotImplementedException();
        }

        public List<string> GetSimpleRow()
        {
            List<string> simpleRow = new List<string>();
            for (int i = 0; i < _shortestRowLength; i++)
            {
                simpleRow.Add(GetMergedCellByIndex(i).Content);
            }
            return simpleRow;
        }

        /// <summary>
        /// Generates an array of specified length and populates it with the sepcified number of true values on random locations
        /// The other values will get value: false
        /// </summary>
        /// <param name="length"></param>
        /// <param name="numberOfTrues"></param>
        private bool[] GenerateRandomBoolArray(int length, int numberOfTrues, Random rand)
        {
            var bools = new bool[length];
            var randomIndices = Enumerable.Range(0, length).OrderBy(x => rand.Next()).Take(numberOfTrues).ToList();
            foreach (int randomIndex in randomIndices)
            {
                bools[randomIndex] = true;
            }
            return bools;
        }
    }
}
