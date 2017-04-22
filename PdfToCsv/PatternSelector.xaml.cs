using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
    }

    class PatternIdentifier
    {
        public int NumberOfRows { get; set; }
        public string FirstRow { get; set; }
    }
}
