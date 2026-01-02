namespace DocsUnmessed.Tests.Integration.Tests;

using DocsUnmessed.Services.Templates;
using NUnit.Framework;

/// <summary>
/// Tests for TemplateEngine
/// </summary>
[TestFixture]
public sealed class TemplateEngineTests
{
    private TemplateEngine? _engine;

    [SetUp]
    public void Setup()
    {
        _engine = new TemplateEngine();
    }

    [Test]
    public void Process_SimpleLiteral_ReturnsLiteral()
    {
        // Arrange
        var template = "Documents/Archive";
        var context = new TemplateContext();

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("Documents/Archive"));
    }

    [Test]
    public void Process_SingleVariable_SubstitutesValue()
    {
        // Arrange
        var template = "{Name}";
        var context = new TemplateContext { FileName = "TestFile" };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("TestFile"));
    }

    [Test]
    public void Process_MultipleVariables_SubstitutesAll()
    {
        // Arrange
        var template = "{Year}/{Month}/{Name}.{Extension}";
        var context = new TemplateContext
        {
            FileName = "Document",
            Extension = "pdf",
            Date = new DateTime(2025, 1, 15)
        };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("2025/01/Document.pdf"));
    }

    [Test]
    public void Process_DateFormatting_AppliesFormat()
    {
        // Arrange
        var template = "{Date:yyyy-MM-dd}_backup";
        var context = new TemplateContext
        {
            Date = new DateTime(2025, 1, 15, 14, 30, 0)
        };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("2025-01-15_backup"));
    }

    [Test]
    public void Process_CounterFormatting_PadsWithZeros()
    {
        // Arrange
        var template = "{Counter:000}_{Name}";
        var context = new TemplateContext
        {
            FileName = "File",
            Counter = 42
        };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("042_File"));
    }

    [Test]
    public void Process_UpperFunction_ConvertsToUpperCase()
    {
        // Arrange
        var template = "{Name|upper}";
        var context = new TemplateContext { FileName = "myfile" };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("MYFILE"));
    }

    [Test]
    public void Process_LowerFunction_ConvertsToLowerCase()
    {
        // Arrange
        var template = "{Name|lower}";
        var context = new TemplateContext { FileName = "MyFile" };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("myfile"));
    }

    [Test]
    public void Process_TitleFunction_ConvertsToTitleCase()
    {
        // Arrange
        var template = "{Name|title}";
        var context = new TemplateContext { FileName = "my document file" };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("My Document File"));
    }

    [Test]
    public void Process_ReplaceFunction_ReplacesText()
    {
        // Arrange
        var template = "{Name|replace(_,-)}"
        ;
        var context = new TemplateContext { FileName = "My_File_Name" };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("My-File-Name"));
    }

    [Test]
    public void Process_SanitizeFunction_RemovesInvalidChars()
    {
        // Arrange
        var template = "{Name|sanitize}";
        var context = new TemplateContext { FileName = "My:File<Name>" };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Does.Not.Contain(":"));
        Assert.That(result, Does.Not.Contain("<"));
        Assert.That(result, Does.Not.Contain(">"));
    }

    [Test]
    public void Process_ComplexTemplate_ProcessesCorrectly()
    {
        // Arrange
        var template = "{Provider}/{Year}/{Month}/{Category}/{Name|sanitize}.{Extension}";
        var context = new TemplateContext
        {
            Provider = "OneDrive",
            FileName = "My Document:File",
            Extension = "docx",
            Category = "Work",
            Date = new DateTime(2025, 3, 15)
        };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("OneDrive/2025/03/Work/My Document_File.docx"));
    }

    [Test]
    public void Process_CustomVariables_ResolvesCorrectly()
    {
        // Arrange
        var template = "{Project}/{CustomVar}";
        var context = new TemplateContext
        {
            CustomVariables = new Dictionary<string, string>
            {
                ["Project"] = "DocsUnmessed",
                ["CustomVar"] = "TestValue"
            }
        };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("DocsUnmessed/TestValue"));
    }

    [Test]
    public void Validate_ValidTemplate_ReturnsValid()
    {
        // Arrange
        var template = "{Year}/{Month}/{Name}";

        // Act
        var result = _engine!.Validate(template);

        // Assert
        Assert.That(result.IsValid, Is.True);
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void Validate_UnclosedBrace_ReturnsInvalid()
    {
        // Arrange
        var template = "{Year}/{Month/{Name}";

        // Act
        var result = _engine!.Validate(template);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Is.Not.Empty);
    }

    [Test]
    public void Validate_UnknownFunction_ReturnsInvalid()
    {
        // Arrange
        var template = "{Name|unknownfunction}";

        // Act
        var result = _engine!.Validate(template);

        // Assert
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Contains.Item("Unknown function: unknownfunction"));
    }

    [Test]
    public void GetVariables_ExtractsAllVariables()
    {
        // Arrange
        var template = "{Year}/{Month}/{Name}.{Extension}";

        // Act
        var variables = _engine!.GetVariables(template);

        // Assert
        Assert.That(variables.Count, Is.EqualTo(4));
        Assert.That(variables, Contains.Item("Year"));
        Assert.That(variables, Contains.Item("Month"));
        Assert.That(variables, Contains.Item("Name"));
        Assert.That(variables, Contains.Item("Extension"));
    }

    [Test]
    public void Process_EmptyTemplate_ReturnsEmpty()
    {
        // Arrange
        var context = new TemplateContext();

        // Act
        var result = _engine!.Process(string.Empty, context);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Process_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _engine!.Process("{Name}", null!));
    }

    [Test]
    public void Process_TruncateFunction_TruncatesText()
    {
        // Arrange
        var template = "{Name|truncate(10)}";
        var context = new TemplateContext { FileName = "VeryLongFileName" };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result.Length, Is.LessThanOrEqualTo(10));
    }

    [Test]
    public void Process_PadFunction_PadsNumber()
    {
        // Arrange
        var template = "{Counter|pad(5)}";
        var context = new TemplateContext { Counter = 7 };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Is.EqualTo("00007"));
    }

    [Test]
    public void Process_AlphanumericFunction_RemovesSpecialChars()
    {
        // Arrange
        var template = "{Name|alphanumeric}";
        var context = new TemplateContext { FileName = "My@File#Name!" };

        // Act
        var result = _engine!.Process(template, context);

        // Assert
        Assert.That(result, Does.Match("^[a-zA-Z0-9_-]+$"));
    }
}
