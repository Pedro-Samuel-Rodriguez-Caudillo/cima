using System;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.EventHandlers;
using cima.Domain.Events.Listings;
using cima.Domain.Shared;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace cima.EventHandlers;

/// <summary>
/// Tests unitarios para ArchitectStatisticsHandler.
/// </summary>
public sealed class ArchitectStatisticsHandlerTests : cimaDomainTestBase<cimaDomainTestModule>
{
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly ILogger<ArchitectStatisticsHandler> _logger;
    private readonly ArchitectStatisticsHandler _handler;

    public ArchitectStatisticsHandlerTests()
    {
        _architectRepository = Substitute.For<IRepository<Architect, Guid>>();
        _logger = Substitute.For<ILogger<ArchitectStatisticsHandler>>();
        _handler = new ArchitectStatisticsHandler(_architectRepository, _logger);
    }

    #region ListingStatusChangedEto Tests

    [Fact]
    public async Task HandleEventAsync_Should_Increment_Stats_When_Published()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(totalPublished: 5, activeListings: 3);
        _architectRepository.FindAsync(architectId).Returns(architect);

        var eventData = new ListingStatusChangedEto(
            listingId: Guid.NewGuid(),
            architectId: architectId,
            oldStatus: ListingStatus.Draft,
            newStatus: ListingStatus.Published);

        // Act
        await _handler.HandleEventAsync(eventData);

        // Assert
        architect.TotalListingsPublished.ShouldBe(6);
        architect.ActiveListings.ShouldBe(4);
        await _architectRepository.Received(1).UpdateAsync(architect);
    }

    [Fact]
    public async Task HandleEventAsync_Should_Decrement_ActiveListings_When_Archived_From_Published()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(totalPublished: 5, activeListings: 3);
        _architectRepository.FindAsync(architectId).Returns(architect);

        var eventData = new ListingStatusChangedEto(
            listingId: Guid.NewGuid(),
            architectId: architectId,
            oldStatus: ListingStatus.Published,
            newStatus: ListingStatus.Archived);

        // Act
        await _handler.HandleEventAsync(eventData);

        // Assert
        architect.TotalListingsPublished.ShouldBe(5); // No cambia
        architect.ActiveListings.ShouldBe(2);
        await _architectRepository.Received(1).UpdateAsync(architect);
    }

    [Fact]
    public async Task HandleEventAsync_Should_Decrement_ActiveListings_When_Archived_From_Portfolio()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(totalPublished: 5, activeListings: 3);
        _architectRepository.FindAsync(architectId).Returns(architect);

        var eventData = new ListingStatusChangedEto(
            listingId: Guid.NewGuid(),
            architectId: architectId,
            oldStatus: ListingStatus.Portfolio,
            newStatus: ListingStatus.Archived);

        // Act
        await _handler.HandleEventAsync(eventData);

        // Assert
        architect.ActiveListings.ShouldBe(2);
        await _architectRepository.Received(1).UpdateAsync(architect);
    }

    [Fact]
    public async Task HandleEventAsync_Should_Increment_ActiveListings_When_Unarchived()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(totalPublished: 5, activeListings: 2);
        _architectRepository.FindAsync(architectId).Returns(architect);

        var eventData = new ListingStatusChangedEto(
            listingId: Guid.NewGuid(),
            architectId: architectId,
            oldStatus: ListingStatus.Archived,
            newStatus: ListingStatus.Published);

        // Act
        await _handler.HandleEventAsync(eventData);

        // Assert
        architect.ActiveListings.ShouldBe(3);
        await _architectRepository.Received(1).UpdateAsync(architect);
    }

    [Fact]
    public async Task HandleEventAsync_Should_Decrement_ActiveListings_When_Unpublished()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(totalPublished: 5, activeListings: 3);
        _architectRepository.FindAsync(architectId).Returns(architect);

        var eventData = new ListingStatusChangedEto(
            listingId: Guid.NewGuid(),
            architectId: architectId,
            oldStatus: ListingStatus.Published,
            newStatus: ListingStatus.Draft);

        // Act
        await _handler.HandleEventAsync(eventData);

        // Assert
        architect.ActiveListings.ShouldBe(2);
        await _architectRepository.Received(1).UpdateAsync(architect);
    }

    [Fact]
    public async Task HandleEventAsync_Should_Not_Go_Below_Zero_ActiveListings()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(totalPublished: 1, activeListings: 0);
        _architectRepository.FindAsync(architectId).Returns(architect);

        var eventData = new ListingStatusChangedEto(
            listingId: Guid.NewGuid(),
            architectId: architectId,
            oldStatus: ListingStatus.Published,
            newStatus: ListingStatus.Archived);

        // Act
        await _handler.HandleEventAsync(eventData);

        // Assert
        architect.ActiveListings.ShouldBe(0); // No debería ser negativo
    }

    [Fact]
    public async Task HandleEventAsync_Should_Not_Update_When_Moving_To_Portfolio()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(totalPublished: 5, activeListings: 3);
        _architectRepository.FindAsync(architectId).Returns(architect);

        var eventData = new ListingStatusChangedEto(
            listingId: Guid.NewGuid(),
            architectId: architectId,
            oldStatus: ListingStatus.Published,
            newStatus: ListingStatus.Portfolio);

        // Act
        await _handler.HandleEventAsync(eventData);

        // Assert - Portfolio no cambia contadores (sigue siendo activo)
        architect.TotalListingsPublished.ShouldBe(5);
        architect.ActiveListings.ShouldBe(3);
        await _architectRepository.DidNotReceive().UpdateAsync(architect);
    }

    [Fact]
    public async Task HandleEventAsync_Should_Handle_Missing_Architect()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        _architectRepository.FindAsync(architectId).Returns((Architect?)null);

        var eventData = new ListingStatusChangedEto(
            listingId: Guid.NewGuid(),
            architectId: architectId,
            oldStatus: ListingStatus.Draft,
            newStatus: ListingStatus.Published);

        // Act & Assert - No debería lanzar excepción
        await Should.NotThrowAsync(async () =>
            await _handler.HandleEventAsync(eventData));

        await _architectRepository.DidNotReceive().UpdateAsync(Arg.Any<Architect>());
    }

    #endregion

    #region ListingCreatedEto Tests

    [Fact]
    public async Task HandleEventAsync_ListingCreated_Should_Complete_Without_Error()
    {
        // Arrange
        var eventData = new ListingCreatedEto(
            listingId: Guid.NewGuid(),
            architectId: Guid.NewGuid(),
            title: "Nueva propiedad",
            createdAt: DateTime.UtcNow);

        // Act & Assert - Solo verificamos que no lance excepción
        await Should.NotThrowAsync(async () =>
            await _handler.HandleEventAsync(eventData));
    }

    #endregion

    #region ListingStatusChangedEto Property Tests

    [Theory]
    [InlineData(ListingStatus.Draft, ListingStatus.Published, true)]
    [InlineData(ListingStatus.Draft, ListingStatus.Portfolio, true)]
    [InlineData(ListingStatus.Published, ListingStatus.Portfolio, false)]
    [InlineData(ListingStatus.Archived, ListingStatus.Published, false)]
    public void BecamePubliclyVisible_Should_Return_Correct_Value(
        ListingStatus oldStatus,
        ListingStatus newStatus,
        bool expected)
    {
        // Arrange
        var eventData = new ListingStatusChangedEto(
            Guid.NewGuid(), Guid.NewGuid(), oldStatus, newStatus);

        // Assert
        eventData.BecamePubliclyVisible.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ListingStatus.Published, ListingStatus.Draft, true)]
    [InlineData(ListingStatus.Published, ListingStatus.Archived, true)]
    [InlineData(ListingStatus.Portfolio, ListingStatus.Archived, true)]
    [InlineData(ListingStatus.Draft, ListingStatus.Published, false)]
    public void BecameHidden_Should_Return_Correct_Value(
        ListingStatus oldStatus,
        ListingStatus newStatus,
        bool expected)
    {
        // Arrange
        var eventData = new ListingStatusChangedEto(
            Guid.NewGuid(), Guid.NewGuid(), oldStatus, newStatus);

        // Assert
        eventData.BecameHidden.ShouldBe(expected);
    }

    [Theory]
    [InlineData(ListingStatus.Draft, ListingStatus.Published, true)]
    [InlineData(ListingStatus.Archived, ListingStatus.Published, false)]
    [InlineData(ListingStatus.Published, ListingStatus.Portfolio, false)]
    public void WasPublished_Should_Return_Correct_Value(
        ListingStatus oldStatus,
        ListingStatus newStatus,
        bool expected)
    {
        // Arrange
        var eventData = new ListingStatusChangedEto(
            Guid.NewGuid(), Guid.NewGuid(), oldStatus, newStatus);

        // Assert
        eventData.WasPublished.ShouldBe(expected);
    }

    #endregion

    #region Helper Methods

    private static Architect CreateTestArchitect(
        int totalPublished = 0,
        int activeListings = 0)
    {
        return new Architect
        {
            UserId = Guid.NewGuid(),
            IsActive = true,
            TotalListingsPublished = totalPublished,
            ActiveListings = activeListings,
            RegistrationDate = DateTime.UtcNow.AddMonths(-6)
        };
    }

    #endregion
}
