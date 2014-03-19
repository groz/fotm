using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FotM.Utilities.Tests
{
    [TestFixture]
    public class MathUtilsTests
    {
        [Test]
        public void MinMaxAvgTest()
        {
            double[] data = new[] {1.0, 2, 6};

            double min, max, avg;
            data.MinMaxAvg(out min, out max, out avg);

            Assert.AreEqual(1.0, min);
            Assert.AreEqual(6.0, max);
            Assert.AreEqual(3.0, avg);
        }

        [Test]
        public void RatingChangeShouldBe16ForEqualTeams()
        {
            int pRating = 2300;
            int oppRating = 2300;

            int newRating = RatingUtils.EstimatedRatingChange(pRating, oppRating, true);
            Assert.AreEqual(16, newRating);
        }

        [Test]
        public void RatingChangeShouldBeMoreThan16ForUnderdogWin()
        {
            int pRating = 2300;
            int oppRating = 2400;

            int newRating = RatingUtils.EstimatedRatingChange(pRating, oppRating, true);
            Assert.Greater(newRating, 16);
        }

        [Test]
        public void RatingChangeShouldBeLessThan16ForUnderdogLoss()
        {
            int pRating = 2300;
            int oppRating = 2400;

            int newRating = RatingUtils.EstimatedRatingChange(pRating, oppRating, false);
            Assert.Less(Math.Abs(newRating), 16);
        }

        [Test]
        public void RatingChangeShouldBeLessThan16ForFavoriteWin()
        {
            int pRating = 2400;
            int oppRating = 2300;

            int newRating = RatingUtils.EstimatedRatingChange(pRating, oppRating, true);
            Assert.Less(newRating, 16);
        }

        [Test]
        public void RatingChangeShouldBeGreaterThan16ForFavoriteLoss()
        {
            int pRating = 2400;
            int oppRating = 2300;

            int newRating = RatingUtils.EstimatedRatingChange(pRating, oppRating, false);
            Assert.Greater(Math.Abs(newRating), 16);
        }

        [Test]
        public void NumberOfCombinations()
        {
            Assert.AreEqual(0, MathUtils.NumberOfCombinations(3, 4));

            Assert.AreEqual(3, MathUtils.NumberOfCombinations(3, 1));
            Assert.AreEqual(3, MathUtils.NumberOfCombinations(3, 2));
            Assert.AreEqual(1, MathUtils.NumberOfCombinations(3, 3));

            Console.WriteLine(MathUtils.NumberOfCombinations(4, 2));
        }

        [Test]
        public void FactorialTest()
        {
            Assert.AreEqual(120, MathUtils.Factorial(5));
            Assert.AreEqual(1, MathUtils.Factorial(0));

            Assert.Throws<OverflowException>(() => MathUtils.Factorial(15));
        }

        [Test]
        public void AllCombinationsOfSingleElementSetShouldBeItself()
        {
            var source = new[] {5};

            var combinations = source.Combinations(1);

            Assert.AreEqual(1, combinations.Count());

            Assert.AreEqual(1, combinations.First().Count());

            Assert.AreEqual(5, combinations.First().First());
        }

        [Test]
        public void Combinations3Choose2()
        {
            var source = new[] { 1, 2, 3 };

            var combinations = source.Combinations(2);

            Assert.AreEqual(3, combinations.Count());

            var expected = new[]
            {
                new[] {1, 2},
                new[] {1, 3},
                new[] {2, 3}
            };

            CollectionAssert.AreEquivalent(expected, combinations);
        }

        [Test]
        public void GradientDescentTest()
        {
            const double learningRate = 1e-2;

            var seed = new []{ 1.0, 1.0 };

            Func<double[], double> f = c => 5 + c[0] + c[0]*c[0];
            
            var solution = Functional.FindMinimum(f, seed, learningRate);

            Assert.AreEqual(-0.5, solution[0], 0.01);
        }

    }
}
