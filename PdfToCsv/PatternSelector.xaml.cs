using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace PdfToCsv
{
    /// <summary>
    /// Interaction logic for PatternSelector.xaml
    /// </summary>
    public partial class PatternSelector : Window
    {
        private List<List<List<string>>> allWordBlocks;
        private string v;
        private string csvPath;

        public PatternSelector()
        {
            InitializeComponent();
            DataContext = this;
        }

        public PatternSelector(List<List<List<string>>> allWordBlocks, string csvPath)
        {
            InitializeComponent();
            DataContext = this;

            this.allWordBlocks = allWordBlocks;
            List<PatternIdentifier> firstPatternOccurences = new List<PatternIdentifier>();
            foreach (List<List<String>> WordBlocks in allWordBlocks)
            {
                firstPatternOccurences.Add(new PatternIdentifier() { NumberOfRows = WordBlocks.Count, FirstRow = string.Join("\n", WordBlocks[0]) });
            }
            patternListView.ItemsSource = firstPatternOccurences;
            this.csvPath = csvPath;
        }

        public int SelectedpatternIndex { get; set; }


        private void ChoosePatternButton_Click(object sender, RoutedEventArgs e)
        {
            // List<List<string>> optimisedWordBlocks = GeneticRowOptimizer.OptimizeRows(allWordBlocks[SelectedpatternIndex]);
            List<List<string>> optimisedWordBlocks = allWordBlocks[SelectedpatternIndex];

            StringBuilder csvContent = new StringBuilder();
            foreach (List<string> wordBlocks in optimisedWordBlocks)
            {
                // Avoid fake columns in the csv because of comma characters in the content
                wordBlocks.ForEach(wb => wb = wb.Replace(",", "\\,"));
                csvContent.AppendLine(string.Join(",", wordBlocks));
            }
            try
            {
                File.Delete(csvPath);
                File.WriteAllText(csvPath, csvContent.ToString(), Encoding.UTF8);
                MessageBox.Show(
                    this,
                    "Bestand succesvol omgezet.",
                    "PDF to CSV",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Process.Start(Path.GetDirectoryName(csvPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(
                    this,
                    "Kan bestand niet opslaan, is het csv bestand nog geopend?",
                    "PDF to CSV",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        ///*
        // * Will try and merge cells to make all rows equal size by trying to reduce the levington distance between cells in the same column
        // */
        //public static List<List<string>> OptimizeRows(List<List<string>> rows)
        //{








        //    int minLength = rows[0].Count;
        //    int minRowIndex = 0;
        //    for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        //    {
        //        if (rows[rowIndex].Count < minLength)
        //        {
        //            minLength = rows[rowIndex].Count;
        //            minRowIndex = rowIndex;
        //        }
        //    }
        //    List<string> referenceRow = rows[minRowIndex];
        //    //for (int refColumnIndex = 1; refColumnIndex < referenceRow.Count; refColumnIndex++)
        //    //{
        //    //Console.WriteLine("Column: " + referenceRow[refColumnIndex] + "----------------------");
        //    for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        //    {
        //        if (rowIndex == minRowIndex)
        //        {
        //            continue; // Skip optimizing the reference row
        //        }

        //        //Console.WriteLine("Row: " + rowIndex);
        //        //List<LowestDifferenceCell> currentRowInfos = new List<LowestDifferenceCell>();

        //        //// Convert each cell to a lowestDifferenceCell
        //        //foreach (string cellContent in rows[rowIndex])
        //        //{
        //        //    List<int> differences = new List<int>();
        //        //    // Calculate differences with each reference cell
        //        //    for (int refColumnIndex = 1; refColumnIndex < referenceRow.Count; refColumnIndex++)
        //        //    {
        //        //        differences.Add(LevenshteinDistance.Compute(referenceRow[refColumnIndex], cellContent));
        //        //    }
        //        //    currentRowInfos.Add(new LowestDifferenceCell() { content = cellContent, differences = differences });
        //        //}

        //        // Merge any cells so that the row length becomes equal to the reference row length
        //        rows[rowIndex] = OptimizeRow(referenceRow, rows[rowIndex]);


        //        //List<string> currentRow = rows[rowIndex];
        //        //int minDistanceIndex = refColumnIndex;
        //        //int minDistance = int.MaxValue;
        //        //for (int columnIndex = refColumnIndex; columnIndex < currentRow.Count; columnIndex++)
        //        //{
        //        //    int distance = LevenshteinDistance.Compute(shortestList[refColumnIndex], currentRow[columnIndex]);
        //        //    if (distance < minDistance)
        //        //    {
        //        //        minDistance = distance;
        //        //        minDistanceIndex = columnIndex;
        //        //    }
        //        //}
        //        //// Best matching column to the right is identified
        //        //int numberOfCellsToMerge = minDistanceIndex - (refColumnIndex - 1);
        //        //if (numberOfCellsToMerge > 1)
        //        //{
        //        //    // Merge rows to the left between refColumnIndex - 1 and minDistanceIndex
        //        //    List<string> cellsToBeMerged = currentRow.GetRange(refColumnIndex - 1, numberOfCellsToMerge);
        //        //    string mergedCellContent = string.Join(" ", cellsToBeMerged);
        //        //    // Insert the merged cell content back into the current row
        //        //    currentRow[refColumnIndex - 1] = mergedCellContent;
        //        //    // Delete the cells between the merged cell and the ref column cell
        //        //    currentRow.RemoveRange(refColumnIndex, numberOfCellsToMerge - 1);
        //        //}
        //    }
        //    //}
        //    return rows;
        //}

        /**
         * 
         */
        private static List<string> OptimizeRow(List<string> referenceRow, List<string> currentRow)
        {
            //int index = 1;
            //while (index < currentRow.Count - 1) // Lets not consider the tail cells
            //{
            //    Tuple<int, int> validRange = getValidMergingRange(index, referenceRow.Count, currentRow.Count);
            //    if (validRange.Item1 > validRange.Item2)
            //    {
            //        index++;
            //        continue;
            //    }
            //    int bestMatchCellIndex = IndexOfMinInRefRowInRange(referenceRow, currentRow[index], validRange.Item1, validRange.Item2);
            //    if (bestMatchCellIndex < index)
            //    {
            //        // merge with cell on the left
            //        currentRow[bestMatchCellIndex] = currentRow[bestMatchCellIndex] + " " + currentRow[index];
            //        currentRow.RemoveAt(index);
            //    } else
            //    {
            //        index++;
            //    }
            //}
            return currentRow;

            //// Now we found the cell that has the best match with a reference cell
            //// Split the row into 3 => the current cells, the range before it and the range after
            //// Recursivly resolve the before and after ranges, then merge the results back into a single row
            //List<string> referenceSubRowBefore = referenceRow.GetRange(0, minimalDifferenceRefIndex);
            //List<LowestDifferenceCell> subRowBefore = currentRowInfos.GetRange(0, minimalDifferenceIndex);
            //List<string> subRowOptimizedLeft = OptimizeRow(referenceSubRowBefore, subRowBefore);


            //List<string> referenceSubRowAfter = referenceRow.GetRange(minimalDifferenceRefIndex + 1, referenceRow.Count - minimalDifferenceRefIndex - 1);
            //List<LowestDifferenceCell> subRowAfter = currentRowInfos.GetRange(minimalDifferenceIndex + 1, currentRowInfos.Count - minimalDifferenceIndex - 1);
            //List<string> subRowOptimizedRight = OptimizeRow(referenceSubRowAfter, subRowAfter);

            //// Merge arrays into one
            //subRowOptimizedLeft.Add(currentRowInfos[minimalDifferenceIndex].content);
            //subRowOptimizedLeft.AddRange(subRowOptimizedRight);
            //return subRowOptimizedLeft;
        }

        //Console.WriteLine("optimizing row: " + string.Join(", ", currentRowInfos.Select(i => i.content)));
        //if (referenceRow.Count == 1)
        //{
        //    return new List<string>() { string.Join(" ", currentRowInfos) };
        //}
        //else if (referenceRow.Count == currentRowInfos.Count)
        //{
        //    return currentRowInfos.Select(ri => ri.content).ToList();
        //}
        //else
        //{
        //    if (referenceRow.Count > currentRowInfos.Count)
        //    {
        //        throw new InvalidOperationException("You should never have less cells in the row than in the reference row");
        //    }
        //    // ReferenceRow is shorter than the row
        //    // Try to split the problem on the cell with the lowest difference to a reference cell

        //    // Find minmum difference for each cell with its own valid merging range
        //    // Then we select the lowest difference of all cells
        //    int minimalDifference = 0;
        //    int minimalDifferenceRefIndex = 0;
        //    int minimalDifferenceIndex = 0;
        //    for (int index = 1; index < currentRowInfos.Count - 1; index++) // Lets not consider the tail cells
        //    {
        //        Tuple<int, int> validRange = getValidMergingRange(index, referenceRow.Count, currentRowInfos.Count);
        //        int bestMatchCellIndex = IndexOfMinInRefRowInRange(currentRowInfos[index], validRange.Item1, validRange.Item2);
        //        int difference = currentRowInfos[index].differences[bestMatchCellIndex];
        //        if (difference < minimalDifference)
        //        {
        //            minimalDifference = difference;
        //            minimalDifferenceRefIndex = bestMatchCellIndex;
        //            minimalDifferenceIndex = index;
        //        }
        //    }

        //    // Now we found the cell that has the best match with a reference cell
        //    // Split the row into 3 => the current cells, the range before it and the range after
        //    // Recursivly resolve the before and after ranges, then merge the results back into a single row
        //    List<string> referenceSubRowBefore = referenceRow.GetRange(0, minimalDifferenceRefIndex);
        //    List<LowestDifferenceCell> subRowBefore = currentRowInfos.GetRange(0, minimalDifferenceIndex);
        //    List<string> subRowOptimizedLeft = OptimizeRow(referenceSubRowBefore, subRowBefore);


        //    List<string> referenceSubRowAfter = referenceRow.GetRange(minimalDifferenceRefIndex + 1, referenceRow.Count - minimalDifferenceRefIndex - 1);
        //    List<LowestDifferenceCell> subRowAfter = currentRowInfos.GetRange(minimalDifferenceIndex + 1, currentRowInfos.Count - minimalDifferenceIndex - 1);
        //    List<string> subRowOptimizedRight = OptimizeRow(referenceSubRowAfter, subRowAfter);

        //    // Merge arrays into one
        //    subRowOptimizedLeft.Add(currentRowInfos[minimalDifferenceIndex].content);
        //    subRowOptimizedLeft.AddRange(subRowOptimizedRight);
        //    return subRowOptimizedLeft;
        //}

        public static Tuple<int, int> getValidMergingRange(int index, int referenceRowLength, int currentRowLength)
        {
            if (index > referenceRowLength)
            {
                throw new Exception("index should never be 2 units greater than the reference row length");
            }
            return new Tuple<int, int>(Math.Max(index - 1, 0), Math.Min(index, referenceRowLength - 1));

            //// Calculate cells that can be selected for best match
            //// These make sure the merged row length is not shorter and not longer than the reference row
            //// This demand requires certain rules:
            //// The range has a min index and a max index
            //// The min index needs to allow enough cells before it and enough cells behind it
            //// The max index needs to allow enough cells before it and enough cells behind it

            //// Ensure enough cells to the left 
            //int minBefore = index == 0 ? 0 : 1;

            //// Ensure enough cells to the right
            //int minAfter = Math.Max(referenceRowLength - 1 - currentRowLength - index, 0);

            //// Ensure enough cells to the left
            //int maxBefore = Math.Max(index - 1, 0);

            //// Ensure enough cells to the right
            //int maxAfter = Math.Max(referenceRowLength - currentRowLength - index, 0);

            //// The max of the calculated mins is the one we want
            //int min = Math.Max(minBefore, minAfter);

            //// The min of the calculated maxes is the one we want
            //int max = Math.Min(maxBefore, maxAfter);

            //// Return range based on most severe restrictions
            //return new Tuple<int, int>(Math.Max(min, 0), Math.Max(max, 0)); // Make sure these indexes do not go below 0
        }

        //private static int IndexOfMinInRefRowInRange(List<string> referenceRow, string currentCell, int lowestIndex, int highestIndex)
        //{
        //    double min = StringDifferance(currentCell, referenceRow[lowestIndex]);
        //    int minRefIndex = lowestIndex;

        //    for (int i = lowestIndex + 1; i <= highestIndex; ++i)
        //    {
        //        double diff = StringDifferance(currentCell, referenceRow[i]);
        //        if (diff < min)
        //        {
        //            min = diff;
        //            minRefIndex = i;
        //        }
        //    }

        //    return minRefIndex;
        //}

    }

    class LowestDifferenceCell
    {
        public string content;
        public List<int> differences;

        public override string ToString()
        {
            return content;
        }
    }

    class PatternIdentifier
    {
        public int NumberOfRows { get; set; }
        public string FirstRow { get; set; }
    }
}
