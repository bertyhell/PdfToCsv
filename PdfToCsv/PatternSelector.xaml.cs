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
            //List<List<string>> optimisedWordBlocks = OptimizeRows(allWordBlocks[SelectedpatternIndex]);
            StringBuilder csvContent = new StringBuilder();
            foreach (List<string> wordBlocks in allWordBlocks[SelectedpatternIndex])
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

        /*
         * Will try and merge cells to make all rows equal size by trying to reduce the levington distance between cells in the same column
         */ 
        private List<List<string>> OptimizeRows(List<List<string>> rows)
        {
            List<string> shortestList = rows.Aggregate((first, second) => first.Count < second.Count ? first : second);
            for (int refColumnIndex = 1; refColumnIndex < shortestList.Count; refColumnIndex++)
            {
                Console.WriteLine("Column: " + shortestList[refColumnIndex] + "----------------------");
                for(int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    Console.WriteLine("Row: " + rowIndex);
                    List<string> currentRow = rows[rowIndex];
                    int minDistanceIndex = refColumnIndex;
                    int minDistance = int.MaxValue;
                    for(int columnIndex = refColumnIndex; columnIndex < currentRow.Count; columnIndex++)
                    {
                        int distance = LevenshteinDistance.Compute(shortestList[refColumnIndex], currentRow[columnIndex]);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            minDistanceIndex = columnIndex;
                        }
                    }
                    // Best matching column to the right is identified
                    int numberOfCellsToMerge = minDistanceIndex - (refColumnIndex - 1);
                    if (numberOfCellsToMerge > 1) {
                        // Merge rows to the left between refColumnIndex - 1 and minDistanceIndex
                        List<string> cellsToBeMerged = currentRow.GetRange(refColumnIndex - 1, numberOfCellsToMerge);
                        string mergedCellContent = string.Join(" ", cellsToBeMerged);
                        // Insert the merged cell content back into the current row
                        currentRow[refColumnIndex - 1] = mergedCellContent;
                        // Delete the cells between the merged cell and the ref column cell
                        currentRow.RemoveRange(refColumnIndex, numberOfCellsToMerge - 1);
                    }
                }
            }
            return rows;
        }
    }

    class PatternIdentifier
    {
        public int NumberOfRows { get; set; }
        public string FirstRow { get; set; }
    }
}
