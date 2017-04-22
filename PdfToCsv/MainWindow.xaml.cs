using System;
using System.Collections.Generic;
using System.Diagnostics;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Win32;
using Path = System.IO.Path;
using System.Linq;

namespace PdfToCsv
{
    public partial class MainWindow
    {
        private const string PDF_FILTER = "Pdf bestanden|*.pdf";

        public MainWindow()
        {
            InitializeComponent();

            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = PDF_FILTER, Title = "Selecteer het PDF bestand"};
            bool? userClickedOk = openFileDialog.ShowDialog();
            if (userClickedOk == true)
            {
                string[] blocks = GetTextBlocsFromPdf(openFileDialog.FileName);
                var AllWordBlocks = AnalysePatterns(blocks);
                new PatternSelector(AllWordBlocks, openFileDialog.FileName.Replace(".pdf", ".csv")).Show();
            }
            Close();
        }

        private List<List<List<string>>> AnalysePatterns(string[] blocks)
        {
            // Find occurance indeces for each word
            Dictionary<string, List<int>> histogram = new Dictionary<string, List<int>>();
            for(int blockIndex = 0; blockIndex < blocks.Length; blockIndex++)
            {
                string blockText = blocks[blockIndex];
                string[] blockParts = blockText.Split(' ');
                foreach(string blockPart in blockParts)
                {
                    if (!histogram.ContainsKey(blockPart))
                    {
                        histogram.Add(blockPart, new List<int>());
                    }
                    histogram[blockPart].Add(blockIndex);
                }
            }

            /*
             * Count length of lists => list length that occures the most will be the one we're looking for
             * Dictionary
             *    Key => length of word occurence list
             *    Value
             *        - times this list length was found
             *        - All words that have this length in occurences
             */
            var metaHistogram = new Dictionary<int, WordPattern>();
            foreach (KeyValuePair<string, List<int>> wordOccurence in histogram)
            {
                if (!metaHistogram.ContainsKey(wordOccurence.Value.Count))
                {
                    metaHistogram.Add(wordOccurence.Value.Count, new WordPattern(0, new List<string>()));
                }
                metaHistogram[wordOccurence.Value.Count].NumOfOccurences++;
                metaHistogram[wordOccurence.Value.Count].Words.Add(wordOccurence.Key);
            }

            // Remove lists that only occur once
            var keys = metaHistogram.Keys.ToArray();
            for (var i = keys.Length - 1; i >= 0; i--)
            {
                if (keys[i] == 1)
                {
                    metaHistogram.Remove(keys[i]);
                }
            }

            // Remove lists that span more than 100 wordblocks
            keys = metaHistogram.Keys.ToArray();
            for (var i = keys.Length - 1; i >= 0; i--)
            {
                List<string> words = metaHistogram[keys[i]].Words;
                int firstWordIndex = histogram[words[0]][0];
                int lastWordIndex  = histogram[words[words.Count - 1]][0];
                if (lastWordIndex - firstWordIndex > 100)
                {
                    metaHistogram.Remove(keys[i]);
                }
            }

            List<KeyValuePair<int, WordPattern>> metaHistogramSorted = (from metaHistoryEntry in metaHistogram orderby metaHistoryEntry.Key * metaHistoryEntry.Value.NumOfOccurences descending select metaHistoryEntry).ToList();

            List<List<List<string>>> AllWordBlocks = new List<List<List<string>>>();
            foreach(KeyValuePair<int, WordPattern> metaHistoryEntry in metaHistogramSorted)
            {
                // Get the blocks for all repetitive patterns that were found in the pdf
                WordPattern wordPattern = metaHistoryEntry.Value;
                List<List<int>> wordInces = new List<List<int>>();
                foreach (string word in wordPattern.Words)
                {
                    wordInces.Add(histogram[word]);
                }

                // Run over first word in best pattern series
                List<List<string>> wordBlocks = new List<List<string>>();
                for (int patternIndex = 0; patternIndex < wordInces[0].Count; patternIndex++)
                {
                    List<string> wordBlock = new List<string>();
                    // Run over each text block between the first best word idex and the last best word index
                    int firstBlockIndex = wordInces[0][patternIndex];
                    int lastBlockIndex = wordInces[wordInces.Count - 1][patternIndex];

                    for (int wordBlockIndex = firstBlockIndex; wordBlockIndex < lastBlockIndex; wordBlockIndex++)
                    {
                        wordBlock.Add(blocks[wordBlockIndex]);
                    }
                    if (wordBlock.Count > 0)
                    {
                        wordBlocks.Add(wordBlock);
                    }
                }
                if (wordBlocks.Count > 0)
                {
                    AllWordBlocks.Add(wordBlocks);
                }
            }
            return AllWordBlocks;
        }

        public string[] GetTextBlocsFromPdf(string fileName)
        {
            var blocks = new List<string>();
            var reader = new PdfReader(fileName);
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                var strategy = new TextAsParagraphsExtractionStrategy();
                string text = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                blocks.AddRange(strategy.strings);
            }
            reader.Close();
            return blocks.Select(b => b.Trim()).ToArray();
        }

        class WordPattern
        {
            public int NumOfOccurences;
            public List<string> Words;

            public WordPattern(int numOfOccurences, List<string> words)
            {
                NumOfOccurences = numOfOccurences;
                Words = words;
            }
        }
    }
}
