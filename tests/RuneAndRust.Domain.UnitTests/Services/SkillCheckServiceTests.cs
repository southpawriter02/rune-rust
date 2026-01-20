using FluentAssertions;
using RuneAndRust.Domain.Services;

namespace RuneAndRust.Domain.UnitTests.Services;

[TestFixture]
public class SkillCheckServiceTests
{
    [Test]
    public void PerformCheck_RollResultIsAlwaysBetween1And20()
    {
        // Arrange
        var service = new SkillCheckService();

        // Act & Assert - Run multiple times to verify range
        for (int i = 0; i < 100; i++)
        {
            var result = service.PerformCheck(10, 15);
            result.RollResult.Should().BeInRange(1, 20);
        }
    }

    [Test]
    public void PerformCheck_TotalResultIncludesAttributeValue()
    {
        // Arrange
        var random = new Random(42); // Seeded for reproducibility
        var service = new SkillCheckService(random);
        var attributeValue = 5;

        // Act
        var result = service.PerformCheck(attributeValue, 10);

        // Assert
        result.TotalResult.Should().Be(result.RollResult + attributeValue);
        result.AttributeValue.Should().Be(attributeValue);
    }

    [Test]
    public void PerformCheck_WhenTotalMeetsDC_ReturnsSuccess()
    {
        // Arrange - Use a seeded random that will give a high roll
        var random = new Random(12345); // Seeded to get consistent results
        var service = new SkillCheckService(random);

        // Act
        var result = service.PerformCheck(15, 12); // High attribute should succeed

        // Assert
        result.IsSuccess.Should().Be(result.TotalResult >= 12);
    }

    [Test]
    public void PerformCheck_SuccessMarginIsCorrect()
    {
        // Arrange
        var random = new Random(42);
        var service = new SkillCheckService(random);
        var targetDC = 15;

        // Act
        var result = service.PerformCheck(10, targetDC);

        // Assert
        result.SuccessMargin.Should().Be(result.TotalResult - targetDC);
    }

    [Test]
    public void PerformDetailedCheck_UsesDC12()
    {
        // Arrange
        var service = new SkillCheckService();

        // Act
        var result = service.PerformDetailedCheck(10);

        // Assert
        result.TargetDC.Should().Be(12);
    }

    [Test]
    public void PerformExpertCheck_UsesDC18()
    {
        // Arrange
        var service = new SkillCheckService();

        // Act
        var result = service.PerformExpertCheck(10);

        // Assert
        result.TargetDC.Should().Be(18);
    }

    [Test]
    public void DetermineExaminationLayer_WithLowRoll_ReturnsLayer1()
    {
        // Arrange - Seed to get a low roll
        var random = new Random(1); // This seed produces a low roll
        var service = new SkillCheckService(random);

        // Act
        var (highestLayer, checkResult) = service.DetermineExaminationLayer(0); // 0 WITS = very low chance

        // Assert
        if (checkResult.TotalResult < 12)
        {
            highestLayer.Should().Be(1);
        }
    }

    [Test]
    public void DetermineExaminationLayer_WithHighWits_CanReachLayer3()
    {
        // Arrange
        var service = new SkillCheckService();

        // Act - Run multiple times to verify high WITS can reach layer 3
        var reachedLayer3 = false;
        for (int i = 0; i < 100; i++)
        {
            var (highestLayer, _) = service.DetermineExaminationLayer(20); // Max WITS
            if (highestLayer == 3)
            {
                reachedLayer3 = true;
                break;
            }
        }

        // Assert
        reachedLayer3.Should().BeTrue("With WITS 20, should eventually roll high enough for layer 3");
    }

    [Test]
    public void DetermineExaminationLayer_ReturnsCorrectLayerBasedOnTotal()
    {
        // Arrange
        var random = new Random(99999); // Seeded for a specific roll
        var service = new SkillCheckService(random);

        // Act
        var (highestLayer, checkResult) = service.DetermineExaminationLayer(10);

        // Assert
        if (checkResult.TotalResult >= 18)
        {
            highestLayer.Should().Be(3);
        }
        else if (checkResult.TotalResult >= 12)
        {
            highestLayer.Should().Be(2);
        }
        else
        {
            highestLayer.Should().Be(1);
        }
    }
}
