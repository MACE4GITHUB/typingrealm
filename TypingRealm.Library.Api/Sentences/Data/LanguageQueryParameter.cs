using System.ComponentModel;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Api.Sentences.Data;

public sealed record LanguageQueryParameter(string Language = TextConstants.DefaultLanguageValue)
{
    /// <include file='../ApiDocs.xml' path='Api/Global/Language/*'/>
    [DefaultValue(TextConstants.DefaultLanguageValue)]
    public string Language { get; init; } = Language;
}
