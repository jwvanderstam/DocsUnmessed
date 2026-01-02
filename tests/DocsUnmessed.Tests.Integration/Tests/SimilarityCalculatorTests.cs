namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Services.Duplicates;
using NUnit.Framework;

/// <summary>
/// Tests for SimilarityCalculator
/// </summary>
[TestFixture]
public sealed class SimilarityCalculatorTests
{
    private SimilarityCalculator? _calculator;

    [SetUp]
    public void Setup()
    {
        _calculator = new SimilarityCalculator();
    }

    [Test]
    public void CalculateLevenshteinDistance_IdenticalStrings_ReturnsZero()
    {
        // Act
        var distance = _calculator!.CalculateLevenshteinDistance("hello", "hello");

        // Assert
        Assert.That(distance, Is.EqualTo(0));
    }

    [Test]
    public void CalculateLevenshteinDistance_OneCharDifference_ReturnsOne()
    {
        // Act
        var distance = _calculator!.CalculateLevenshteinDistance("hello", "hallo");

        // Assert
        Assert.That(distance, Is.EqualTo(1));
    }

    [Test]
    public void CalculateLevenshteinDistance_CompletelyDifferent_ReturnsLength()
    {
        // Act
        var distance = _calculator!.CalculateLevenshteinDistance("abc", "xyz");

        // Assert
        Assert.That(distance, Is.EqualTo(3));
    }

    [Test]
    public void CalculateStringSimilarity_IdenticalStrings_ReturnsOne()
    {
        // Act
        var similarity = _calculator!.CalculateStringSimilarity("document", "document");

        // Assert
        Assert.That(similarity, Is.EqualTo(1.0));
    }

    [Test]
    public void CalculateStringSimilarity_SimilarStrings_ReturnsHighValue()
    {
        // Act
        var similarity = _calculator!.CalculateStringSimilarity("document", "documents");

        // Assert
        Assert.That(similarity, Is.GreaterThan(0.8));
    }

    [Test]
    public void CalculateStringSimilarity_DifferentStrings_ReturnsLowValue()
    {
        // Act
        var similarity = _calculator!.CalculateStringSimilarity("document", "picture");

        // Assert
        Assert.That(similarity, Is.LessThan(0.5));
    }

    [Test]
    public void CalculateFileNameSimilarity_SameNameDifferentExtension_ReturnsHigh()
    {
        // Act
        var similarity = _calculator!.CalculateFileNameSimilarity("Document.pdf", "Document.docx");

        // Assert
        Assert.That(similarity, Is.GreaterThan(0.9));
    }

    [Test]
    public void CalculateFileNameSimilarity_SimilarNamesWithNumbers_ReturnsHigh()
    {
        // Act
        var similarity = _calculator!.CalculateFileNameSimilarity("Report_2024.pdf", "Report_2025.pdf");

        // Assert
        Assert.That(similarity, Is.GreaterThan(0.8));
    }

    [Test]
    public void CalculateFileNameSimilarity_DifferentNames_ReturnsLow()
    {
        // Act
        var similarity = _calculator!.CalculateFileNameSimilarity("Invoice.pdf", "Picture.jpg");

        // Assert
        Assert.That(similarity, Is.LessThan(0.5));
    }

    [Test]
    public void CalculateFileNameSimilarity_UnderscoresVsSpaces_ConsideredSimilar()
    {
        // Act
        var similarity = _calculator!.CalculateFileNameSimilarity("My_Document.pdf", "My Document.pdf");

        // Assert
        Assert.That(similarity, Is.GreaterThan(0.9));
    }

    [Test]
    public void CalculateSizeSimilarity_IdenticalSizes_ReturnsOne()
    {
        // Act
        var similarity = _calculator!.CalculateSizeSimilarity(1024, 1024);

        // Assert
        Assert.That(similarity, Is.EqualTo(1.0));
    }

    [Test]
    public void CalculateSizeSimilarity_SlightDifference_ReturnsHigh()
    {
        // Act
        var similarity = _calculator!.CalculateSizeSimilarity(1024, 1030);

        // Assert
        Assert.That(similarity, Is.GreaterThan(0.99));
    }

    [Test]
    public void CalculateSizeSimilarity_LargeDifference_ReturnsLow()
    {
        // Act
        var similarity = _calculator!.CalculateSizeSimilarity(1024, 2048);

        // Assert
        Assert.That(similarity, Is.LessThan(0.6));
    }

    [Test]
    public void CalculateDateSimilarity_SameDate_ReturnsOne()
    {
        // Arrange
        var date = DateTime.UtcNow;

        // Act
        var similarity = _calculator!.CalculateDateSimilarity(date, date, 24);

        // Assert
        Assert.That(similarity, Is.EqualTo(1.0));
    }

    [Test]
    public void CalculateDateSimilarity_WithinThreshold_ReturnsHigh()
    {
        // Arrange
        var date1 = new DateTime(2025, 1, 1, 12, 0, 0);
        var date2 = new DateTime(2025, 1, 1, 18, 0, 0); // 6 hours later

        // Act
        var similarity = _calculator!.CalculateDateSimilarity(date1, date2, 24);

        // Assert
        Assert.That(similarity, Is.GreaterThan(0.7));
    }

    [Test]
    public void CalculateDateSimilarity_BeyondThreshold_ReturnsZero()
    {
        // Arrange
        var date1 = new DateTime(2025, 1, 1, 12, 0, 0);
        var date2 = new DateTime(2025, 1, 3, 12, 0, 0); // 48 hours later

        // Act
        var similarity = _calculator!.CalculateDateSimilarity(date1, date2, 24);

        // Assert
        Assert.That(similarity, Is.EqualTo(0.0));
    }

    [Test]
    public void CalculateCombinedSimilarity_EqualWeights_ReturnsAverage()
    {
        // Act
        var combined = _calculator!.CalculateCombinedSimilarity(
            nameSimilarity: 0.9,
            sizeSimilarity: 0.8,
            dateSimilarity: 0.7,
            weights: (0.33, 0.33, 0.34));

        // Assert
        Assert.That(combined, Is.EqualTo(0.8).Within(0.01));
    }

    [Test]
    public void CalculateCombinedSimilarity_NameWeightHigh_PrioritizesName()
    {
        // Act
        var combined = _calculator!.CalculateCombinedSimilarity(
            nameSimilarity: 1.0,
            sizeSimilarity: 0.5,
            dateSimilarity: 0.5,
            weights: (0.8, 0.1, 0.1));

        // Assert
        Assert.That(combined, Is.GreaterThan(0.85));
    }

    [Test]
    public void CalculateCombinedSimilarity_InvalidWeights_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _calculator!.CalculateCombinedSimilarity(0.9, 0.8, 0.7, (0.5, 0.3, 0.1)));
    }
}
