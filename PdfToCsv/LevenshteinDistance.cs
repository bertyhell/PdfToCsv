using System;
using System.Diagnostics;

namespace PdfToCsv
{
    /// <summary>
    /// Contains approximate string matching
    /// </summary>
    public static class LevenshteinDistance
    {

        public static int Compute(string first, string second, int switchGroupCost = 10, int switchCapCost = 5)
        {
            int firstLength = first.Length;
            int secondLength = second.Length;
            int[,] distances = new int[firstLength + 1, secondLength + 1];

            if (firstLength == 0)
            {
                return secondLength * switchGroupCost;
            }
            if (secondLength == 0)
            {
                return firstLength * switchGroupCost;
            }

            // Fill first row and column
            for (int i = 0; i <= firstLength; i++)
            {
                distances[i, 0] = i * switchGroupCost;
            }

            for (int j = 0; j <= secondLength; j++)
            {
                distances[0, j] = j * switchGroupCost;
            }

            for (int rowIndex = 1; rowIndex <= secondLength; rowIndex++)
            {
                for (int columnIndex = 1; columnIndex <= firstLength; columnIndex++)
                {
                    int cost = GetCost(first.Substring(0, columnIndex), second.Substring(0, rowIndex), switchGroupCost, switchCapCost);
                    // Get min of left top and diagonal left top cell
                    int minimalPreviousCost = Math.Min(
                        GetFromArray(distances, rowIndex - 1, columnIndex - 1), 
                        Math.Min(
                            GetFromArray(distances, rowIndex, columnIndex - 1), 
                            GetFromArray(distances, rowIndex - 1, columnIndex)));
                    // Set first cell = 0
                    minimalPreviousCost = minimalPreviousCost == int.MaxValue ? 0 : minimalPreviousCost;
                    distances[columnIndex, rowIndex] = cost + minimalPreviousCost;
                }
            }

            //for (var rowIndex = 0; rowIndex < distances.GetLength(1); rowIndex++)
            //{

            //    for (var columnIndex = 0; columnIndex < distances.GetLength(0); columnIndex++)
            //    {
            //        Trace.Write(distances[columnIndex, rowIndex] + "\t");
            //    }
            //    Trace.Write("\n");
            //}
            
            return distances[firstLength, secondLength];
        }

        private static int GetFromArray(int[,] array, int row, int col, int defaultValue = int.MaxValue)
        {
            if (row < 0 || col < 0 || row >= array.GetLength(1) || col >= array.GetLength(0))
            {
                return defaultValue;
            }
            else
            {
                return array[col, row];
            }
        }

        private enum CharacterType
        {
            lower = 0,
            upper = 1,
            digit = 2,
            symbol = 3
        }

        /**
         * Cost by characterType pair
         * Order: upper, lower, digit, symbol
         */
        private static int[,] costTable = new int[,]
        {
            {  0,   1,    2,   2 },
            {  1,   0,    2 ,  2 },
            {  2,   2,    0,   2 },
            {  2,   2,    2,   0 }
        };

        private static int GetCost(string first, string second, int switchGroupCost, int switchCapCost)
        {
            if (first.Length != second.Length)
            {
                return switchGroupCost;
            }

            char firstLetter = first[first.Length - 1];
            char secondLetter = second[second.Length - 1];
            if (firstLetter == secondLetter)
            {
                return 0;
            }

            int[] costArray = new int[] { 1, switchCapCost, switchGroupCost };

            CharacterType firstCharacterType  = GetCharacterType(firstLetter);
            CharacterType secondCharacterType = GetCharacterType(secondLetter);
            return costArray[costTable[(int)firstCharacterType, (int)secondCharacterType]];
        }

        private static CharacterType GetCharacterType(char letter)
        {
            if (char.IsDigit(letter))
            {
                return CharacterType.digit;
            }
            else if (char.IsLetter(letter))
            {
                if (char.IsUpper(letter))
                {
                    return CharacterType.upper;
                }
                else
                {
                    return CharacterType.lower;
                }
            }
            else
            {
                return CharacterType.symbol;
            }
        }
    }
}

