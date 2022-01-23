﻿using System.Collections.Generic;
using System.Linq;
using TypingRealm.TextProcessing;

namespace TypingRealm.Texts;

public sealed record TextGenerationConfiguration
{
    private const int StandardMinimumLength = 300;

    private TextGenerationConfiguration(
        Language language,
        int minimumLength,
        bool cutLastSentence,
        TextStructure textStructure,
        bool isLowerCase,
        bool stripPunctuation,
        bool stripNumbers,
        IEnumerable<string> shouldContain)
    {
        Language = language;
        MinimumLength = minimumLength;
        CutLastSentence = cutLastSentence;
        TextStructure = textStructure;
        IsLowerCase = isLowerCase;
        StripPunctuation = stripPunctuation;
        StripNumbers = stripNumbers;
        ShouldContain = shouldContain;
    }

    public Language Language { get; }
    public int MinimumLength { get; }
    public bool CutLastSentence { get; }
    public TextStructure TextStructure { get; }
    public bool IsLowerCase { get; }
    public bool StripPunctuation { get; }
    public bool StripNumbers { get; }
    public IEnumerable<string> ShouldContain { get; }

    public bool IsSelfImprovement => ShouldContain.Any();
    public string StatisticsTypeIdentifier => $"{nameof(TextStructure)}-{TextStructure}-{nameof(StatisticsType)}-{StatisticsType}-{nameof(IsSelfImprovement)}-{IsSelfImprovement}";

    public StatisticsType StatisticsType
    {
        get
        {
            if (MinimumLength != StandardMinimumLength
                || IsLowerCase
                || StripPunctuation
                || StripNumbers)
            {
                return StatisticsType.Custom;
            }

            // Standard text is requested.

            return StatisticsType.Standard;
        }
    }

    public static TextGenerationConfiguration Standard(
        Language language,
        TextStructure textStructure = TextStructure.Text,
        bool cutLastSentence = false)
    {
        return new TextGenerationConfiguration(
            language,
            StandardMinimumLength,
            cutLastSentence,
            textStructure,
            false, false, false,
            Enumerable.Empty<string>());
    }

    public static TextGenerationConfiguration SelfImprovement(
        Language language,
        TextStructure textStructure = TextStructure.Text,
        bool cutLastSentence = false,
        IEnumerable<string>? shouldContain = null)
    {
        if (shouldContain == null)
            shouldContain = Enumerable.Empty<string>();

        return new TextGenerationConfiguration(
            language,
            StandardMinimumLength,
            cutLastSentence,
            textStructure,
            false, false, false,
            shouldContain);
    }

    public static TextGenerationConfiguration Custom(
        Language language,
        TextStructure textStructure = TextStructure.Text,
        int minimumLength = StandardMinimumLength,
        bool cutLastSentence = false,
        bool isLowerCase = false, bool stripPunctuation = false, bool stripNumbers = false,
        IEnumerable<string>? shouldContain = null)
    {
        if (shouldContain == null)
            shouldContain = Enumerable.Empty<string>();

        return new TextGenerationConfiguration(
            language,
            minimumLength,
            cutLastSentence,
            textStructure,
            isLowerCase, stripPunctuation, stripNumbers,
            shouldContain);
    }
}
