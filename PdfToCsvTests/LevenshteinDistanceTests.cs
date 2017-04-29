using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PdfToCsv.Tests
{
    [TestClass()]
    public class LevenshteinDistanceTests
    {
        [TestMethod()]
        public void EqualStrings()
        {
            Assert.AreEqual(0, LevenshteinDistance.Compute("abc", "abc"));
        }
        [TestMethod()]
        public void FirstEmptyStringStrings()
        {
            Assert.AreEqual(30, LevenshteinDistance.Compute("", "abc"));
        }
        [TestMethod()]
        public void SecondEmptyStrings()
        {
            Assert.AreEqual(30, LevenshteinDistance.Compute("abc", ""));
        }
        [TestMethod()]
        public void UpVsLowerStrings()
        {
            Assert.AreEqual(15, LevenshteinDistance.Compute("abc", "ABC"));
        }
        [TestMethod()]
        public void DifferentLetterAndCasingStrings()
        {
            Assert.AreEqual(15, LevenshteinDistance.Compute("abc", "DEF"));
        }
        [TestMethod()]
        public void NumberAndLetterStrings()
        {
            Assert.AreEqual(20, LevenshteinDistance.Compute("abc", "DE4"));
        }
        [TestMethod()]
        public void SymbolsAndLettersStrings()
        {
            Assert.AreEqual(20, LevenshteinDistance.Compute("abc", "DE/"));
        }
        [TestMethod()]
        public void DifferentLengthStrings()
        {
            Assert.AreEqual(10, LevenshteinDistance.Compute("abc", "abcd"));
        }
        [TestMethod()]
        public void DifferentLengthDifferentSymbolsStrings()
        {
            Assert.AreEqual(50, LevenshteinDistance.Compute("abcdef", "DE/"));
        }
    }
}