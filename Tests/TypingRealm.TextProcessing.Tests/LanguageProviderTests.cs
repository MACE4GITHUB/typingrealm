using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TypingRealm.Testing;
using Xunit;

namespace TypingRealm.TextProcessing.Tests;

public class LanguageProviderTests : TextProcessingTestsBase
{
    private readonly LanguageProvider _sut = new LanguageProvider();

    [Theory, AutoDomainData]
    public async Task ShouldThrow_WhenLanguageIsInvalid(Language language)
    {
        SetPrimitiveValue(language, nameof(Language.Value), "wrong");

        await AssertThrowsAsync<NotSupportedException>(
            async () => await _sut.FindLanguageInformationAsync(language));
    }

    [Fact]
    public async Task ShouldSupportEmptyText_InAllLanguages()
    {
        foreach (var languageValue in TextConstants.SupportedLanguageValues)
        {
            var language = new Language(languageValue);

            var info = await _sut.FindLanguageInformationAsync(language);
            Assert.True(info.IsAllLettersAllowed(string.Empty));
        }
    }

    [Fact]
    public async Task ShouldSupportNumbers_InAllLanguages()
    {
        foreach (var languageValue in TextConstants.SupportedLanguageValues)
        {
            var language = new Language(languageValue);

            var info = await _sut.FindLanguageInformationAsync(language);
            Assert.True(info.IsAllLettersAllowed(TextConstants.NumberCharacters));
        }
    }

    [Fact]
    public async Task ShouldSupportAllPunctuationCharacters_InAllLanguages()
    {
        foreach (var languageValue in TextConstants.SupportedLanguageValues)
        {
            var language = new Language(languageValue);

            var info = await _sut.FindLanguageInformationAsync(language);
            Assert.True(info.IsAllLettersAllowed(TextConstants.PunctuationCharacters));
        }
    }

    [Fact]
    public async Task ShouldSupportSpace_InAllLanguages()
    {
        foreach (var languageValue in TextConstants.SupportedLanguageValues)
        {
            var language = new Language(languageValue);

            var info = await _sut.FindLanguageInformationAsync(language);
            Assert.True(info.IsAllLettersAllowed(TextConstants.SpaceCharacter.ToString()));
        }
    }

    [Fact]
    public async Task ShouldNotSupportNonPrintableCharacters_InAllLanguages()
    {
        foreach (var languageValue in TextConstants.SupportedLanguageValues)
        {
            var language = new Language(languageValue);

            var info = await _sut.FindLanguageInformationAsync(language);
            Assert.False(info.IsAllLettersAllowed("é"));
        }
    }

    [Fact]
    public async Task EnglishLanguage_ShouldSupportEnglishCharacters()
    {
        var info = await _sut.FindLanguageInformationAsync(TextConstants.EnglishLanguage);
        Assert.True(info.IsAllLettersAllowed("abcde"));
    }

    [Fact]
    public async Task EnglishLanguage_ShouldNotSupportRussianCharacters()
    {
        var info = await _sut.FindLanguageInformationAsync(TextConstants.EnglishLanguage);
        Assert.False(info.IsAllLettersAllowed("абвгд"));
    }

    [Fact]
    public async Task RussianLanguage_ShouldSupportRussianCharacters()
    {
        var info = await _sut.FindLanguageInformationAsync(TextConstants.RussianLanguage);
        Assert.True(info.IsAllLettersAllowed("абвгд"));
    }

    [Fact]
    public async Task RussianLanguage_ShouldNotSupportEnglishCharacters()
    {
        var info = await _sut.FindLanguageInformationAsync(TextConstants.RussianLanguage);
        Assert.False(info.IsAllLettersAllowed("abcde"));
    }

    [Fact]
    public void AddTextProcessing_ShouldRegisterAsSingleton()
    {
        var serviceProvider = new ServiceCollection()
            .AddTextProcessing()
            .BuildServiceProvider();

        serviceProvider.AssertRegisteredSingleton<ILanguageProvider, LanguageProvider>();
    }
}
