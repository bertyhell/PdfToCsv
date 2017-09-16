using System;
using System.Collections.Generic;

namespace PdfToCsv
{
    public class GeneticCell
    {
        public string Content { get; private set; }
        public Dictionary<char, int> CharacterFrequency { get; private set; }

        public GeneticCell(string content) : this(content, GetCharacterFrequency(content)) { }

        public GeneticCell(string content, Dictionary<char, int> characterFrequency)
        {
            Content = content;
            CharacterFrequency = characterFrequency;
        }

        public static GeneticCell operator +(GeneticCell cell1, GeneticCell cell2)
        {
            return new GeneticCell(cell1.Content + " " + cell2.Content, CombineCharacterFrequencies(cell1.CharacterFrequency, cell2.CharacterFrequency));
        }

        private static Dictionary<char, int> GetCharacterFrequency(string text)
        {
            var characterCount = new Dictionary<char, int>();
            foreach (var c in text)
            {
                if (characterCount.ContainsKey(c))
                    characterCount[c]++;
                else
                    characterCount[c] = 1;
            }
            return characterCount;
        }

        public static Dictionary<char, int> CombineCharacterFrequencies(Dictionary<char, int> freq1, Dictionary<char, int> freq2)
        {
            var combinedFreq = new Dictionary<char, int>(freq1);
            foreach (KeyValuePair<char, int> pair in freq2)
            {
                if (!combinedFreq.ContainsKey(pair.Key))
                {
                    combinedFreq[pair.Key] = 0;
                }
                combinedFreq[pair.Key] += pair.Value; 
            }
            return combinedFreq;
        }
    }
}
