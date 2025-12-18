using NUnit.Framework;
using FluentAssertions;
using MoneyManager.Models;

namespace MoneyManager.Tests.Models
{
    [TestFixture]
    public class BudgetTests
    {
        [Test]
        public void Remaining_WhenTotalGreaterThanUsed_ReturnsPositiveValue()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 1000,
                UsedAmount = 300
            };

            // Act
            var remaining = budget.Remaining;

            // Assert
            remaining.Should().Be(700);
        }

        [Test]
        public void Remaining_WhenTotalEqualsUsed_ReturnsZero()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 500,
                UsedAmount = 500
            };

            // Act & Assert
            budget.Remaining.Should().Be(0);
        }

        [Test]
        public void Remaining_WhenUsedExceedsTotal_ReturnsNegativeValue()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 300,
                UsedAmount = 500
            };

            // Act & Assert
            budget.Remaining.Should().Be(-200);
        }

        [Test]
        public void Percentage_WhenTotalIsZero_ReturnsZero()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 0,
                UsedAmount = 100
            };

            // Act & Assert
            budget.Percentage.Should().Be(0);
        }

        [Test]
        public void Percentage_WhenUsedIsZero_ReturnsZero()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 1000,
                UsedAmount = 0
            };

            // Act & Assert
            budget.Percentage.Should().Be(0);
        }

        [Test]
        public void Percentage_WhenUsedIsHalfOfTotal_Returns50()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 1000,
                UsedAmount = 500
            };

            // Act & Assert
            budget.Percentage.Should().Be(50);
        }

        [Test]
        public void Percentage_WhenUsedEqualsTotal_Returns100()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 1000,
                UsedAmount = 1000
            };

            // Act & Assert
            budget.Percentage.Should().Be(100);
        }

        [Test]
        public void Percentage_WhenUsedExceedsTotal_ReturnsOver100()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 500,
                UsedAmount = 750
            };

            // Act & Assert
            budget.Percentage.Should().Be(150);
        }

        [Test]
        public void Percentage_WithPreciseValues_CalculatesCorrectly()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 333.33m,
                UsedAmount = 111.11m
            };

            // Act & Assert
            var expected = (111.11m / 333.33m) * 100;
            budget.Percentage.Should().BeApproximately(expected, 0.01m);
        }

        [Test]
        public void Percentage_CalculatedValue_ShouldBeConsistent()
        {
            // Arrange
            var budget = new Budget
            {
                Category = "Test",
                TotalAmount = 1234.56m,
                UsedAmount = 789.01m
            };

            // Act
            var calculatedPercentage = (budget.UsedAmount / budget.TotalAmount) * 100;
            var propertyPercentage = budget.Percentage;

            // Assert - 两种计算方式应该一致
            propertyPercentage.Should().BeApproximately(calculatedPercentage, 0.0001m);
        }
    }
}