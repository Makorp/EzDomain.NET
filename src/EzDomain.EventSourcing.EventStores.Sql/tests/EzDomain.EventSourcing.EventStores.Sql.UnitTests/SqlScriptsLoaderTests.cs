using System.Reflection;

namespace EzDomain.EventSourcing.EventStores.Sql.UnitTests;

[TestFixture]
internal sealed class SqlScriptsLoaderTests
{
    private readonly ISqlScriptsLoader _systemUnderTest = new SqlScriptsLoader(Assembly.GetExecutingAssembly());

    [Test]
    public void GetScriptGetsScriptContent_WhenScriptsWereCorrectlyLoadedFromAssembly()
    {
        // Act
        var scriptContent = _systemUnderTest.GetScript("TestEmbeddedScript");

        // Assert
        scriptContent
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    [Test]
    public void GetScriptThrowsInvalidOperationException_WhenScriptWasNotFound()
    {
        // Act
        var act = () => _systemUnderTest.GetScript("TestEmbeddedScriptNotFound");

        // Assert
        act
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Script \"TestEmbeddedScriptNotFound\" not found.");
    }

    [Test]
    public void GetScriptThrowsInvalidOperationException_WhenScriptContentWasEmpty()
    {
        // Act
        var act = () => _systemUnderTest.GetScript("TestEmbeddedScriptEmpty");

        // Assert
        act
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Script \"TestEmbeddedScriptEmpty\" not found.");
    }

    [Test]
    public void GetScriptThrowsInvalidOperationException_WhenScriptContentWasWhiteSpace()
    {
        // Act
        var act = () => _systemUnderTest.GetScript("TestEmbeddedScriptWhiteSpace");

        // Assert
        act
            .Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("Script \"TestEmbeddedScriptWhiteSpace\" not found.");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void GetScriptThrowsArgumentNullException_WhenScriptNameWasNullOrWhiteSpace(string scriptName)
    {
        // Act
        var act = () => _systemUnderTest.GetScript(null!);

        // Assert
        act
            .Should()
            .ThrowExactly<ArgumentNullException>();
    }

    [Test]
    public void CreationOfSqlScriptsLoaderThrowsArgumentException_WhenScriptKeyIsDuplicated()
    {
        // Act
        var act = () => new SqlScriptsLoader(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly());

        // Assert
        act
            .Should()
            .ThrowExactly<ArgumentException>()
            .WithMessage("An item with the same key has already been added. Key: TestEmbeddedScript");
    }
}