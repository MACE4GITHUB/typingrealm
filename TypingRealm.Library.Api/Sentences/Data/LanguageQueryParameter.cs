using System.ComponentModel;
using TypingRealm.TextProcessing;

namespace TypingRealm.Library.Api.Sentences.Data;

public sealed class LanguageQueryParameter
{
    /// <include file='../ApiDocs.xml' path='Api/Global/Language/*'/>
    [DefaultValue(TextConstants.DefaultLanguageValue)]
    public string Language { get; init; } = TextConstants.DefaultLanguageValue;
}
