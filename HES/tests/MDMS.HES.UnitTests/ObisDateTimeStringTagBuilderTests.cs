using MDMS.HES.Application.Services;
using MDMS.HES.Core.Common;
using MDMS.HES.Core.Models;
using Xunit;

namespace MDMS.HES.UnitTests;

public class ObisDateTimeStringTagBuilderTests
{
    [Fact]
    public void FeedsSixParts_CurrentDateTime_SynthesizesStandardObisTimeAndDate()
    {
        // Arrange
        var builder = new ObisDateTimeStringTagBuilder();
        string meterId = "M001";
        var emittedTags = new List<TagValue>();
        builder.OnObisTagSynthesized += (m, tag) => emittedTags.Add(tag);

        var now = new DateTime(2026, 11, 19, 14, 30, 45);

        // Act - feed 5 parts first
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Year", (short)2026, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Month", (short)11, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Day", (short)19, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Hour", (short)14, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Minute", (short)30, TagQuality.Good, now), out _);

        // Assert - not emitted yet
        Assert.Empty(emittedTags);

        // Act - feed 6th part (Second)
        bool result = builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Second", (short)45, TagQuality.Good, now), out var synList);

        // Assert
        Assert.True(result);
        Assert.Equal(2, synList.Count);
        Assert.Equal(2, emittedTags.Count);

        var timeTag = emittedTags.FirstOrDefault(x => x.Id == ObisConstants.ObisCurrentTime);
        var dateTag = emittedTags.FirstOrDefault(x => x.Id == ObisConstants.ObisCurrentDate);

        Assert.NotNull(timeTag);
        Assert.NotNull(dateTag);
        Assert.Equal("14:30:45", timeTag.Value);
        Assert.Equal("2026-11-19", dateTag.Value);
    }

    [Theory]
    [InlineData("MDC", ObisConstants.ObisMdcTime, ObisConstants.ObisMdcDate)]
    [InlineData("MDP", ObisConstants.ObisMdpTime, ObisConstants.ObisMdpDate)]
    [InlineData("Reset", ObisConstants.ObisResetTime, ObisConstants.ObisResetDate)]
    public void FeedsMdcMdpReset_SynthesizesCorrectObisCodes(string group, string expectedTimeCode, string expectedDateCode)
    {
        var builder = new ObisDateTimeStringTagBuilder();
        string meterId = "M002";
        var emittedTags = new List<TagValue>();
        builder.OnObisTagSynthesized += (m, tag) => emittedTags.Add(tag);

        var now = new DateTime(2026, 11, 1, 0, 0, 0);

        builder.TryProcessTag(meterId, new TagValue($"_AdvancedTags.Template.DT.{group}.Year", (short)2026, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue($"_AdvancedTags.Template.DT.{group}.Month", (short)11, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue($"_AdvancedTags.Template.DT.{group}.Day", (short)1, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue($"_AdvancedTags.Template.DT.{group}.Hour", (short)0, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue($"_AdvancedTags.Template.DT.{group}.Minute", (short)0, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue($"_AdvancedTags.Template.DT.{group}.Second", (short)0, TagQuality.Good, now), out _);

        Assert.Contains(emittedTags, x => x.Id == expectedTimeCode && (string)x.Value! == "00:00:00");
        Assert.Contains(emittedTags, x => x.Id == expectedDateCode && (string)x.Value! == "2026-11-01");
    }

    [Fact]
    public void InvalidCalendarDate_DoesNotEmit()
    {
        var builder = new ObisDateTimeStringTagBuilder();
        string meterId = "M003";
        var emittedTags = new List<TagValue>();
        builder.OnObisTagSynthesized += (m, tag) => emittedTags.Add(tag);

        var now = DateTime.UtcNow;

        // Month 13 is invalid
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Year", (short)2026, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Month", (short)13, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Day", (short)19, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Hour", (short)14, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Minute", (short)30, TagQuality.Good, now), out _);
        builder.TryProcessTag(meterId, new TagValue("_AdvancedTags.Template.DT.Current.Second", (short)45, TagQuality.Good, now), out _);

        Assert.Empty(emittedTags);
    }
}
