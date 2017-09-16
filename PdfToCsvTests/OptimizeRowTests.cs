using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfToCsv.Tests
{
    [TestClass()]
    public class OptimizeRowTests
    {

        private static readonly List<string> ROW_1 = new List<string>() { "Dossiernr.:", "101.203", "Erkenningsnr.:", "PE2568", "Residentie", "Vincent", "van", "Gogh", "Beheersinstantie", "Boomsesteenweg", "15", "2630", "Aartselaar" };
        private static readonly List<string> ROW_2 = new List<string>() { "Dossiernr.:", "102.201", "Erkenningsnr.:", "CE1963", "Joostens-Lemme", "Beheersinstantie", "Dambruggestraat", "306", "2060", "Antwerpen" };
        private static readonly Random rand = new Random();
        private static readonly int shortestRowLength = Math.Min(ROW_1.Count, ROW_2.Count);

        [TestMethod]
        public void TestFitnessFunction()
        {

            // Create genetic rows with perfect/desired column seperators
            List<GeneticRow> geneticRows = new List<GeneticRow>();
            geneticRows.Add(new GeneticRow(ROW_1, shortestRowLength, new bool[] { true, true, true, true, false, false, false, true, true, true, true, true }, rand));
            geneticRows.Add(new GeneticRow(ROW_2, shortestRowLength, new bool[] { true, true, true, true, true, true, true, true, true }, rand));

            double scorePerfect = GeneticRowOptimizer.FitnessFunction(geneticRows, shortestRowLength);

            // Create genetic rows with inperfect column seperators
            geneticRows = new List<GeneticRow>();
            geneticRows.Add(new GeneticRow(ROW_1, shortestRowLength, new bool[] { true, true, true, false, true, false, false, true, true, true, true, true }, rand));
            geneticRows.Add(new GeneticRow(ROW_2, shortestRowLength, new bool[] { true, true, true, true, true, true, true, true, true }, rand));

            double scoreMedium = GeneticRowOptimizer.FitnessFunction(geneticRows, shortestRowLength);

            // Create genetic rows with inperfect column seperators
            geneticRows = new List<GeneticRow>();
            geneticRows.Add(new GeneticRow(ROW_1, shortestRowLength, new bool[] { false, false, false, true, true, true, true, true, true, true, true, true }, rand));
            geneticRows.Add(new GeneticRow(ROW_2, shortestRowLength, new bool[] { true, true, true, true, true, true, true, true, true }, rand));

            double scoreWorst = GeneticRowOptimizer.FitnessFunction(geneticRows, shortestRowLength);

            Assert.IsTrue(scorePerfect > scoreMedium);
            Assert.IsTrue(scorePerfect > scoreWorst);
            Assert.IsTrue(scoreMedium > scoreWorst);
        }

        [TestMethod]
        public void TestGeneticRowConstuctor()
        {
            GeneticRow row1 = new GeneticRow(ROW_1, shortestRowLength, null, rand);
            GeneticRow row2 = new GeneticRow(ROW_2, shortestRowLength, new bool[] { true, true, true, true, true, true, true, true, true }, rand);

            Assert.IsTrue(row1.ColumnSeperators.Count(cs => cs) == shortestRowLength - 1);
            Assert.IsTrue(row2.ColumnSeperators.Count(cs => cs) == shortestRowLength - 1);

            Assert.IsTrue(row1.ColumnSeperators.Length == ROW_1.Count - 1);
            Assert.IsTrue(row2.ColumnSeperators.Length == ROW_2.Count - 1);
        }



        //[TestMethod()]
        //public void OptimizeRows()
        //{
        //    string row1 = "Dossiernr.: 101.203 Erkenningsnr.: PE2568 Residentie Vincent van Gogh Beheersinstantie Boomsesteenweg 15 2630 Aartselaar VZW Zonnewende tel. : 03 870 55 70 Boomsesteenweg 15 fax: 03 870 55 98 2630 Aartselaar e-mail: directie @wzczonnewende.be url: www.zonnewende.be Erkende capaciteit: 9";
        //    string row2 = "Dossiernr.: 102.201 Erkenningsnr.: CE1963 Joostens-Lemme Beheersinstantie Dambruggestraat 306 2060 Antwerpen VER  Zorgbedrijf Antwerpen tel. : 03 201 21 90 Ballaarstraat 35 fax: 03 201 21 99 2018 Antwerpen e-mail: dc.essenhof @zorgbedrijf.antwerpen.be url: www.zorgbedrijf.antwerpen.be Erkende capaciteit: 29";
        //    List<List<string>> optimizedRows = PatternSelector.OptimizeRows(new List<List<string>> { new List<string>(row1.Split(' ')), new List<string>(row2.Split(' ')) });
        //    Assert.AreEqual("Dossiernr.:", optimizedRows[0][0]);
        //    Assert.AreEqual("Dossiernr.:", optimizedRows[1][0]);
        //    Assert.AreEqual("101.203", optimizedRows[0][1]);
        //    Assert.AreEqual("102.201", optimizedRows[1][1]);
        //}

        //[TestMethod]
        //public void getValidMergingRange1()
        //{
        //    // xxx xxx
        //    // xxx xxx ooo xxx
        //    Tuple<int, int> interval = PatternSelector.getValidMergingRange(2, 2, 4);
        //    Assert.AreEqual(1, interval.Item1);
        //    Assert.AreEqual(1, interval.Item2);
        //}

        //[TestMethod]
        //public void getValidMergingRange2()
        //{
        //    // xxx xxx xxx
        //    // xxx xxx ooo xxx
        //    Tuple<int, int> interval = PatternSelector.getValidMergingRange(2, 3, 4);
        //    Assert.AreEqual(1, interval.Item1);
        //    Assert.AreEqual(2, interval.Item2);
        //}
    }
}