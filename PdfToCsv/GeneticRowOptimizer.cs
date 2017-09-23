using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfToCsv
{
    public static class GeneticRowOptimizer
    {
        private static readonly int NUMBER_OF_SPECIMEN_TO_BREED = 200;             // Should be multiple of "allowed to breed" number
        private static readonly int NUMBER_OF_SPECIMEN_ALLOWED_TO_BREED = 20;      // Should be multipe of 2
        private static readonly int NUMBER_OF_GENERATIONS_AFTER_NO_PROGRESS = 300;
        private static readonly double ROW_MUTATION_CHANCE = 0.3;                   // 70% of the rows will mutate
        private static readonly double ROW_MUTATION_SEVERITY = 0.7;                 // 70% of the row column seperators will change position
        private static readonly double NUM_OF_CHAR_MULTIPLIER = 1;                  // relative penalty for using various characters
        private static readonly double NUM_OF_CHAR_REUSED_MULTIPLIER = 1;           // relative penalty for not reusing the same characters
        private static readonly double CHAR_GROUP_USE_MULTIPLIER = 5;               // relative penalty for using multiple character groups
        private static readonly double RARE_CHARACTERS_USED_MULTIPLIER = 1;         // relative penalty for using a character only a few times
        private static readonly double RARE_CHARACTERS_TRESHOLD = 1.0;              // number of standard deviations the char frequency has to be below average before getting this bonus
        private static readonly double SAME_LENGTH_BONUS = 2;                       // Bonus for columns that have the same length for all rows
        private static readonly Random rand = new Random();

        private static readonly Dictionary<string, GeneticCell> _cachedGeneticCells = new Dictionary<string, GeneticCell>();

        /// <summary>
        /// Attempts to unify the length of all rows, so that each row has an identical number of rows
        /// and each column has similar data
        /// </summary>
        /// <param name="rows">List of rows where each rows consists of a list of strings</param>
        /// <returns>A list of rows where each row consits of an equal number of strings</returns>
        public static List<List<string>> OptimizeRows(List<List<string>> rows)
        {
            int shortestRowLength = rows.Min(r => r.Count);

            // Generate the initial breeding generation
            List<GeneticSpecimen> solutions = new List<GeneticSpecimen>();
            for (int i = 0; i < NUMBER_OF_SPECIMEN_TO_BREED; i++)
            {
                List<GeneticRow> geneticRows = GenerateGeneticRows(rows, shortestRowLength);
                // Calculate the score of this solutions
                double score = FitnessFunction(geneticRows, shortestRowLength);
                solutions.Add(new GeneticSpecimen(geneticRows, score));
            }

            double lastBestScore = solutions.Max(s => s.Score);
            Console.WriteLine("Generation " + 1 + ": best solution score: " + lastBestScore);
            int generationsToGo = NUMBER_OF_GENERATIONS_AFTER_NO_PROGRESS;
            int generations = 1;

            while (generationsToGo > 0)
            {

                // Breed the next generation
                solutions = BreedNextGeneration(solutions);

                // Calculate scores for solutions who don't have a score yet
                foreach (GeneticSpecimen solution in solutions)
                {
                    if (solution.Score == -1)
                    {
                        solution.Score = FitnessFunction(solution.Rows, shortestRowLength);
                    }
                }

                generations++;
                double score = solutions.Max(s => s.Score);
                Console.WriteLine("Generation " + generations + ": best solution score: " + score);

                if (Math.Abs(score - lastBestScore) < 0.00000001)
                {
                    generationsToGo--;
                }
                else
                {
                    generationsToGo = NUMBER_OF_GENERATIONS_AFTER_NO_PROGRESS;
                }
                lastBestScore = score;
            }

            // Output the best solution
            List<GeneticRow> bestSolutionGeneticRows = solutions.Find(s => s.Score == lastBestScore).Rows;
            List<List<string>> bestSolutionRows = new List<List<string>>();
            foreach (GeneticRow row in bestSolutionGeneticRows)
            {
                bestSolutionRows.Add(row.GetSimpleRow());
            }
            return bestSolutionRows;
        }

        /// <summary>
        /// Creates more solutions by breeding good solutions
        /// This is done by combining 2 solutions together by taking the first part of solution 1 and the second part of solution 2 to form a new solution
        /// </summary>
        /// <param name="solutions"></param>
        /// <returns></returns>
        private static List<GeneticSpecimen> BreedNextGeneration(List<GeneticSpecimen> solutions)
        {
            List<GeneticSpecimen> offspringSolutions = new List<GeneticSpecimen>();
            int numberOfRows = solutions[0].Rows.Count;

            // Select the best solutions for breeding
            List<GeneticSpecimen> breeders = solutions.OrderByDescending(s => s.Score).Take(NUMBER_OF_SPECIMEN_ALLOWED_TO_BREED).ToList();

            // Add the best solutions to the next generation
            offspringSolutions.AddRange(breeders);

            while (offspringSolutions.Count < NUMBER_OF_SPECIMEN_TO_BREED)
            {
                // Randomize the order of the breed solutions, so they breed with different partners every time
                //breeders.OrderBy(x => rand.Next()).ToList();

                // Pick 2 breeders to breed together, but make the ones with higher scores more likely to be picked
                int correctedIndex1;
                int correctedIndex2;
                double randomIndex;
                randomIndex = rand.NextDouble() * (NUMBER_OF_SPECIMEN_ALLOWED_TO_BREED);
                correctedIndex1 = (int)Math.Floor(randomIndex * randomIndex / (NUMBER_OF_SPECIMEN_ALLOWED_TO_BREED));
                do
                {
                    randomIndex = rand.NextDouble() * NUMBER_OF_SPECIMEN_ALLOWED_TO_BREED;
                    correctedIndex2 = (int)Math.Floor(randomIndex * randomIndex / (NUMBER_OF_SPECIMEN_ALLOWED_TO_BREED));
                } while (correctedIndex1 == correctedIndex2);

                // Breed new solutions by combining the best solutions from the previous generation
                double crossOverRatio = rand.NextDouble();
                int crossOverRowIndex = (int)Math.Floor(numberOfRows * crossOverRatio);
                // A1 + B2
                // B1 + A2
                var solutionA = breeders[correctedIndex1].Rows;
                var solutionB = breeders[correctedIndex2].Rows;

                var solutionA1 = solutionA.GetRange(0, crossOverRowIndex);
                var solutionA2 = solutionA.GetRange(crossOverRowIndex, numberOfRows - crossOverRowIndex - 1);
                var solutionB1 = solutionB.GetRange(0, crossOverRowIndex);
                var solutionB2 = solutionB.GetRange(crossOverRowIndex, numberOfRows - crossOverRowIndex - 1);

                var mutant1 = new GeneticSpecimen(solutionA1.Concat(solutionB2).ToList());
                var mutant2 = new GeneticSpecimen(solutionB1.Concat(solutionA2).ToList());
                offspringSolutions.Add(mutant1);
                offspringSolutions.Add(mutant2);
            }
            foreach (GeneticSpecimen solution in offspringSolutions)
            {
                foreach (GeneticRow row in solution.Rows)
                {
                    if (rand.NextDouble() < ROW_MUTATION_CHANCE)
                    {
                        row.Mutate(ROW_MUTATION_SEVERITY);
                    }
                }

            }
            return offspringSolutions;
        }

        public static List<GeneticRow> GenerateGeneticRows(List<List<string>> rows, int shortestRowLength)
        {
            List<GeneticRow> geneticRows = new List<GeneticRow>();
            foreach (List<string> row in rows)
            {
                geneticRows.Add(new GeneticRow(row, shortestRowLength, null, rand));
            }
            return geneticRows;
        }

        /// <summary>
        /// This function calculates a score for a set of rows based on column content similarity
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="shortestRowLength"></param>
        /// <returns>Score of similarity => 0 means all cells are identical, higher means more differance</returns>
        public static double FitnessFunction(List<GeneticRow> rows, int shortestRowLength)
        {
            double totalScore = 0;
            for (int column = 0; column < shortestRowLength; column++)
            {
                Dictionary<char, int> columnCharFrequencies = new Dictionary<char, int>();
                List<string> columnCellValues = new List<string>();
                foreach (GeneticRow row in rows)
                {
                    GeneticCell cell = row.GetMergedCellByIndex(column);
                    columnCharFrequencies = GeneticCell.CombineCharacterFrequencies(cell.CharacterFrequency, columnCharFrequencies);
                    columnCellValues.Add(cell.Content);
                }

                // Give a bonus for characters that are almost not used anymore so the evolutionary algorithm gets an incentive the push these out of existiance
                double rareCharFrequencyBonus = 0;
                double averageCharFreq = columnCharFrequencies.Values.Average();
                double stdDiv = CalculateStdDev(columnCharFrequencies.Values, averageCharFreq);
                foreach (KeyValuePair<char, int> charFreq in columnCharFrequencies) // TODO relate this to the character frequency in the general pdf => Q used less in english than in french...
                {
                    double treshold = averageCharFreq - stdDiv * RARE_CHARACTERS_TRESHOLD;
                    if (charFreq.Value < treshold)
                    {
                        // Give the column a bonus if its rarly used characters are getting more and more rare => hopefully they will stop being used all together
                        rareCharFrequencyBonus += RARE_CHARACTERS_USED_MULTIPLIER * (1 - charFreq.Value / treshold); // explanatory graph: https://github.com/bertyhell/PdfToCsv/blob/genetic-column-optimizer/rare-char-usage.jpg
                    }
                }

                // Give a bonus if all cells in the column have the same length
                double samelLenthBonus = columnCellValues.All(c => c.Length == columnCellValues[0].Length) ? SAME_LENGTH_BONUS : 1;

                //// TODO see if promoting columns with identical strings is good or bad (fear of getting stuck in local optima
                //// TODO same thing with identical length in the whole column
                //columnCellValues.All(v => v == columnCellValues[0]) {
                //}

                // Give a score to this column based on the columnCharFrequency
                // - The less characters used, the better
                // - The more times the same character is used, the better
                // - The less character groups that are used, the better

                // TODO keys length normalize by dividing be average column content length?
                double score =
                    // - The less characters are used the better
                    1.0 / columnCharFrequencies.Keys.Count * NUM_OF_CHAR_MULTIPLIER *
                    // - The more times the same character is used, the better
                    columnCharFrequencies.Values.Sum() / columnCharFrequencies.Keys.Count / rows.Count * NUM_OF_CHAR_REUSED_MULTIPLIER *
                    // - The less character groups used, is better
                    GetCharGroupUseScore(columnCharFrequencies) * CHAR_GROUP_USE_MULTIPLIER *
                    // - Reward columns that have characters that are almost not used anymore => trying to add an incentive towards: The less character groups used, is better 
                    rareCharFrequencyBonus *
                    // Columns where all the cells have the same length get a bonus
                    SAME_LENGTH_BONUS;

                totalScore += score;
            }
            return totalScore;
        }

        private static int GetCharGroupUseScore(Dictionary<char, int> charFrequency)
        {
            List<char> goupsWithMultipleMembers = new List<char> { 'u', 'l', 'd' }; // Upper, Lower, Digits
            Dictionary<char, bool> freqByGroup = FillFreqByGroup(charFrequency);
            int score = 0;
            foreach (char character in freqByGroup.Keys)
            {
                if (goupsWithMultipleMembers.Contains(character))
                {
                    score += 10; // Penalty for using an extra alphanumeric group: 10
                }
                else
                {
                    score += 1; // Penalty for using an extra symbol: 1
                }
            }
            return score / charFrequency.Keys.Count;
        }

        public static GeneticCell GetCachedGeneticCell(string content)
        {
            if (_cachedGeneticCells.ContainsKey(content))
            {
                return _cachedGeneticCells[content];
            }
            else
            {
                return new GeneticCell(content);
            }
        }

        ///// <summary>
        ///// Calculates the difference between 2 strings based on characters used and total length
        ///// point system (high to low):
        ///// * both strings are equal
        ///// * lengths are equal
        ///// * characters used are from the same group (letters, capitals, digits, symbols)(each symbol is considered it's own group)
        ///// </summary>
        ///// <param name="text1">First string</param>
        ///// <param name="text2">Second string</param>
        ///// <returns>Score for similarity (higher is more similar)</returns>
        //private static double StringDifferance(string text1, string text2)
        //{
        //    if (text1 == text2)
        //    {
        //        return 0;
        //    }
        //    bool equalLength = text1.Length == text2.Length;
        //    Dictionary<char, int> text1Freq = GetCharacterFrequency(text1);
        //    Dictionary<char, int> text2Freq = GetCharacterFrequency(text2);
        //    int characterFrequencyDifferences = GetCharacterFrequencyDifferance(text1Freq, text2Freq);
        //    int characterFrequencyDiffByGroup = GetCharacterFrequencyDifferancePerGroup(text1Freq, text2Freq);
        //    return (characterFrequencyDifferences + characterFrequencyDiffByGroup) / 100.0 / (text1.Length + text2.Length);
        //}


        ///// <summary>
        ///// Calcualtes the number of letters differance between the 2 strings
        ///// Strings that use a lot of the same characters
        ///// </summary>
        ///// <param name="text1Freq"></param>
        ///// <param name="text2Freq"></param>
        ///// <returns></returns>
        //private static int GetCharacterFrequencyDifferance(Dictionary<char, int> text1Freq, Dictionary<char, int> text2Freq)
        //{
        //    List<char> uniqueCharacters = text1Freq.Keys.Concat(text2Freq.Keys).Distinct().ToList();
        //    int score = 0;
        //    foreach (char character in uniqueCharacters)
        //    {
        //        score += Math.Abs((text1Freq.ContainsKey(character) ? text1Freq[character] : 0) - (text2Freq.ContainsKey(character) ? text2Freq[character] : 0));
        //    }
        //    return score;
        //}

        ///// <summary>
        ///// Gets the differances in characters per character group
        ///// If one string uses letters and the other doesn't it is a good indication that they shouldn't match
        ///// </summary>
        ///// <param name="text1Freq"></param>
        ///// <param name="text2Freq"></param>
        ///// <returns></returns>
        //private static int GetCharacterFrequencyDifferancePerGroup(Dictionary<char, int> text1Freq, Dictionary<char, int> text2Freq)
        //{
        //    List<char> uniqueCharacters = new List<char> { 'u', 'l', 'd' }; // Upper, Lower, Digits, symbols will use the symbol character itself as the key
        //    Dictionary<char, bool> text1FreqByGroup = FillFreqByGroup(text1Freq, ref uniqueCharacters);
        //    Dictionary<char, bool> text2FreqByGroup = FillFreqByGroup(text2Freq, ref uniqueCharacters);

        //    int score = 0;
        //    foreach (char character in uniqueCharacters)
        //    {
        //        bool usedInText1 = text1FreqByGroup.ContainsKey(character) ? text1FreqByGroup[character] : false;
        //        bool usedInText2 = text2FreqByGroup.ContainsKey(character) ? text2FreqByGroup[character] : false;
        //        bool differentUsage = usedInText1 ^ usedInText2; // xor both bools
        //        score += differentUsage ? GetCharacterGroupPenaltyScore(character) : 0;
        //    }
        //    return score;
        //}

        private static int GetCharacterGroupPenaltyScore(char groupId)
        {
            switch (groupId)
            {
                case 'u':
                case 'l':
                case 'd':
                    return 100;
                default:
                    return 10;
            }
        }

        //private static Dictionary<char, bool> FillFreqByGroup(Dictionary<char, int> textFreq, ref List<char> uniqueCharacters)
        //{
        //    Dictionary<char, bool> textFreqByGroup = new Dictionary<char, bool>();
        //    foreach (KeyValuePair<char, int> pair in textFreq)
        //    {
        //        if (pair.Key >= 'A' && pair.Key <= 'Z')
        //        {
        //            textFreqByGroup['u'] = true;
        //        }
        //        else if (pair.Key >= 'a' && pair.Key <= 'z')
        //        {
        //            textFreqByGroup['l'] = true;
        //        }
        //        else if (pair.Key >= '0' && pair.Key <= '9')
        //        {
        //            textFreqByGroup['d'] = true;
        //        }
        //        else
        //        {
        //            uniqueCharacters.Add(pair.Key);
        //            textFreqByGroup[pair.Key] = true;
        //        }
        //    }
        //    return textFreqByGroup;
        //}
        private static Dictionary<char, bool> FillFreqByGroup(Dictionary<char, int> textFreq)
        {
            Dictionary<char, bool> textFreqByGroup = new Dictionary<char, bool>();
            foreach (KeyValuePair<char, int> pair in textFreq)
            {
                if (pair.Key >= 'A' && pair.Key <= 'Z')
                {
                    textFreqByGroup['u'] = true;
                }
                else if (pair.Key >= 'a' && pair.Key <= 'z')
                {
                    textFreqByGroup['l'] = true;
                }
                else if (pair.Key >= '0' && pair.Key <= '9')
                {
                    textFreqByGroup['d'] = true;
                }
                else
                {
                    textFreqByGroup[pair.Key] = true;
                }
            }
            return textFreqByGroup;
        }

        /// <summary>
        /// Calculates the statistical standard deviation for a list of numbers
        /// </summary>
        /// <param name="values">The list of numbers</param>
        /// <param name="average">Optional average of the list of numbers if already available (efficientcy++)</param>
        /// <returns>The standard deviation for the list of numbers</returns>
        private static double CalculateStdDev(IEnumerable<int> values, double? average)
        {
            double stdDev = 0;
            if (values.Count() > 0)
            {
                // Compute the Average   
                double avg;
                if (average.HasValue)
                {
                    avg = average.Value;
                }
                else
                {
                    avg = values.Average();
                }
                // Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                // Put it all together      
                stdDev = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return stdDev;
        }
    }
}
