using System;
using Microsoft.Extensions.Logging;
using Moq;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Events;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Engine.Services;
using Xunit;

namespace RuneAndRust.Tests.Engine.Services
{
    public class AetherServiceTests
    {
        private readonly Mock<ILogger<AetherService>> _loggerMock;
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly AetherService _service;

        public AetherServiceTests()
        {
            _loggerMock = new Mock<ILogger<AetherService>>();
            _eventBusMock = new Mock<IEventBus>();
            _service = new AetherService(_loggerMock.Object, _eventBusMock.Object);
        }

        [Fact]
        public void InitialState_ShouldBeZeroAndSafe()
        {
            // Assert
            Assert.Equal(0, _service.GetCurrentFlux());
            Assert.Equal(FluxThreshold.Safe, _service.GetThreshold());
        }

        [Fact]
        public void ModifyFlux_ShouldIncreaseValue_AndPublishEvent()
        {
            // Act
            _service.ModifyFlux(10, "Test Source");

            // Assert
            Assert.Equal(10, _service.GetCurrentFlux());
            _eventBusMock.Verify(x => x.Publish(It.Is<FluxChangedEvent>(e =>
                e.OldValue == 0 &&
                e.NewValue == 10 &&
                e.Source == "Test Source")), Times.Once);
        }

        [Fact]
        public void ModifyFlux_ShouldClampToZero()
        {
            // Arrange
            _service.ModifyFlux(10, "Setup");

            // Act
            _service.ModifyFlux(-20, "Test Source");

            // Assert
            Assert.Equal(0, _service.GetCurrentFlux());
        }

        [Fact]
        public void ModifyFlux_ShouldClampToMax()
        {
            // Act
            _service.ModifyFlux(150, "Test Source");

            // Assert
            Assert.Equal(100, _service.GetCurrentFlux());
            Assert.Equal(FluxThreshold.Overload, _service.GetThreshold());
        }

        [Theory]
        [InlineData(0, FluxThreshold.Safe)]
        [InlineData(24, FluxThreshold.Safe)]
        [InlineData(25, FluxThreshold.Elevated)]
        [InlineData(49, FluxThreshold.Elevated)]
        [InlineData(50, FluxThreshold.Critical)]
        [InlineData(74, FluxThreshold.Critical)]
        [InlineData(75, FluxThreshold.Overload)]
        [InlineData(100, FluxThreshold.Overload)]
        public void GetThreshold_ShouldReturnCorrectEnum(int value, FluxThreshold expected)
        {
            // Arrange
            _service.Reset();
            _service.ModifyFlux(value, "Setup");

            // Assert
            Assert.Equal(expected, _service.GetThreshold());
        }

        [Fact]
        public void DissipateFlux_ShouldReduceByRate()
        {
            // Arrange
            _service.ModifyFlux(20, "Setup"); // Rate is default 5

            // Act
            _service.DissipateFlux("Test");

            // Assert
            Assert.Equal(15, _service.GetCurrentFlux());
            _eventBusMock.Verify(x => x.Publish(It.IsAny<FluxChangedEvent>()), Times.AtLeastOnce);
        }

        [Fact]
        public void Reset_ShouldSetToZero()
        {
            // Arrange
            _service.ModifyFlux(50, "Setup");

            // Act
            _service.Reset("Test");

            // Assert
            Assert.Equal(0, _service.GetCurrentFlux());
            _eventBusMock.Verify(x => x.Publish(It.Is<FluxChangedEvent>(e => e.NewValue == 0)), Times.AtLeastOnce);
        }
    }
}
