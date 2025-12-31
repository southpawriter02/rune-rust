using System;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models.Magic;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine.Services
{
    public class AetherServiceTests
    {
        private readonly Mock<IEventBus> _mockEventBus;
        private readonly Mock<ILogger<AetherService>> _mockLogger;
        private readonly AetherService _service;

        public AetherServiceTests()
        {
            _mockEventBus = new Mock<IEventBus>();
            _mockLogger = new Mock<ILogger<AetherService>>();
            _service = new AetherService(_mockEventBus.Object, _mockLogger.Object);
        }

        [Fact]
        public void InitialState_IsZeroAndSafe()
        {
            Assert.Equal(0, _service.GetCurrentFlux());
            Assert.Equal(FluxThreshold.Safe, _service.GetThreshold());
        }

        [Theory]
        [InlineData(10, 10, FluxThreshold.Safe)]
        [InlineData(30, 30, FluxThreshold.Elevated)]
        [InlineData(60, 60, FluxThreshold.Critical)]
        [InlineData(80, 80, FluxThreshold.Overload)]
        public void ModifyFlux_UpdatesValueAndReturnsCorrectThreshold(int amount, int expectedValue, FluxThreshold expectedThreshold)
        {
            var result = _service.ModifyFlux(amount, "Test");

            Assert.Equal(expectedValue, result);
            Assert.Equal(expectedValue, _service.GetCurrentFlux());
            Assert.Equal(expectedThreshold, _service.GetThreshold());
        }

        [Fact]
        public void ModifyFlux_ClampsToMinZero()
        {
            _service.ModifyFlux(-10, "Test");
            Assert.Equal(0, _service.GetCurrentFlux());
        }

        [Fact]
        public void ModifyFlux_ClampsToMaxHundred()
        {
            _service.ModifyFlux(150, "Test");
            Assert.Equal(100, _service.GetCurrentFlux());
        }

        [Fact]
        public void ModifyFlux_PublishesEvent_WhenChanged()
        {
            _service.ModifyFlux(10, "Test");

            _mockEventBus.Verify(x => x.Publish(It.Is<FluxChangedEvent>(e =>
                e.OldValue == 0 &&
                e.NewValue == 10 &&
                e.Source == "Test")), Times.Once);
        }

        [Fact]
        public void ModifyFlux_DoesNotPublishEvent_WhenUnchanged()
        {
            _service.ModifyFlux(0, "Test");
            _mockEventBus.Verify(x => x.Publish(It.IsAny<FluxChangedEvent>()), Times.Never);
        }

        [Fact]
        public void DissipateFlux_ReducesFluxByDissipationRate()
        {
            // Setup initial state
            _service.ModifyFlux(20, "Setup");
            _mockEventBus.Invocations.Clear(); // Clear setup events

            int dissipated = _service.DissipateFlux();

            // Default rate is 5
            Assert.Equal(5, dissipated);
            Assert.Equal(15, _service.GetCurrentFlux());
        }

        [Fact]
        public void DissipateFlux_DoesNotGoBelowZero()
        {
            _service.ModifyFlux(3, "Setup");

            int dissipated = _service.DissipateFlux();

            Assert.Equal(3, dissipated);
            Assert.Equal(0, _service.GetCurrentFlux());
        }

        [Fact]
        public void Reset_SetsFluxToZero()
        {
            _service.ModifyFlux(50, "Setup");
            _service.Reset();

            Assert.Equal(0, _service.GetCurrentFlux());
            _mockEventBus.Verify(x => x.Publish(It.Is<FluxChangedEvent>(e =>
                e.NewValue == 0 &&
                e.Source == "Reset")), Times.Once);
        }

        [Fact]
        public void ModifyFlux_LogsInformation_WhenThresholdChanges()
        {
            // Transition from Safe (0) to Elevated (30)
            _service.ModifyFlux(30, "Test");

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Flux threshold changed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
