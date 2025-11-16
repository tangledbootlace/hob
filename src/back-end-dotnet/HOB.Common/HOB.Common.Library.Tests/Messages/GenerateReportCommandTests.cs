using FluentAssertions;
using HOB.Common.Library.Messages;

namespace HOB.Common.Library.Tests.Messages;

public class GenerateReportCommandTests
{
    [Fact]
    public void GenerateReportCommand_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var command = new GenerateReportCommand();

        // Assert
        command.CorrelationId.Should().Be(Guid.Empty);
        command.RequestedAt.Should().Be(default(DateTime));
        command.StartDate.Should().BeNull();
        command.EndDate.Should().BeNull();
        command.RequestedBy.Should().Be("System");
    }

    [Fact]
    public void GenerateReportCommand_ShouldInitializeWithProvidedValues()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var requestedAt = DateTime.UtcNow;
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var requestedBy = "TestUser";

        // Act
        var command = new GenerateReportCommand
        {
            CorrelationId = correlationId,
            RequestedAt = requestedAt,
            StartDate = startDate,
            EndDate = endDate,
            RequestedBy = requestedBy
        };

        // Assert
        command.CorrelationId.Should().Be(correlationId);
        command.RequestedAt.Should().Be(requestedAt);
        command.StartDate.Should().Be(startDate);
        command.EndDate.Should().Be(endDate);
        command.RequestedBy.Should().Be(requestedBy);
    }

    [Fact]
    public void GenerateReportCommand_ShouldSupportRecordEquality()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var requestedAt = DateTime.UtcNow;

        var command1 = new GenerateReportCommand
        {
            CorrelationId = correlationId,
            RequestedAt = requestedAt,
            RequestedBy = "TestUser"
        };

        var command2 = new GenerateReportCommand
        {
            CorrelationId = correlationId,
            RequestedAt = requestedAt,
            RequestedBy = "TestUser"
        };

        // Act & Assert
        command1.Should().Be(command2);
    }

    [Fact]
    public void GenerateReportCommand_ShouldSupportWithSyntax()
    {
        // Arrange
        var originalCommand = new GenerateReportCommand
        {
            CorrelationId = Guid.NewGuid(),
            RequestedAt = DateTime.UtcNow,
            RequestedBy = "OriginalUser"
        };

        // Act
        var modifiedCommand = originalCommand with { RequestedBy = "ModifiedUser" };

        // Assert
        modifiedCommand.CorrelationId.Should().Be(originalCommand.CorrelationId);
        modifiedCommand.RequestedAt.Should().Be(originalCommand.RequestedAt);
        modifiedCommand.RequestedBy.Should().Be("ModifiedUser");
    }
}
