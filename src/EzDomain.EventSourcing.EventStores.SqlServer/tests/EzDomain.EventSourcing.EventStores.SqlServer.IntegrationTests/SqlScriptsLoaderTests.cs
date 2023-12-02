using System.Reflection;
using EzDomain.EventSourcing.EventStores.Sql;
using FluentAssertions;

namespace EzDomain.EventSourcing.EventStores.SqlServer.IntegrationTests;

[TestFixture]
internal sealed class SqlScriptsLoaderTests
{
    [Test]
    public void LoadScripts_ShouldReturnAllScripts()
    {
        // Arrange
        var scriptsLoader = new SqlScriptsLoader(Assembly.GetExecutingAssembly());

        // Act
        var embeddedScript = scriptsLoader.GetScript("EmbeddedScript");
        var copiedScript = scriptsLoader.GetScript("CopiedScript");

        // Assert
        embeddedScript
            .Should()
            .NotBeNullOrWhiteSpace();

        copiedScript
            .Should()
            .NotBeNullOrWhiteSpace();
    }
}