using FluentAssertions;
using TicketManager.Domain.Entities;
using TicketManager.Domain.Enums;
using TicketManager.Domain.Exceptions;

namespace TicketManager.Tests.Unit.Domain;

public class TicketTests
{
    [Fact]
    public void Create_ValidInput_ReturnsOpenTicket()
    {
        var ticket = Ticket.Create("Écran cassé", "L'écran ne s'allume plus", Guid.NewGuid(), TicketPriority.High);

        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.Priority.Should().Be(TicketPriority.High);
        ticket.History.Should().BeEmpty();
    }

    [Fact]
    public void Create_EmptyTitle_ThrowsDomainException()
    {
        var act = () => Ticket.Create("", "description", Guid.NewGuid(), TicketPriority.Low);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void StartProgress_FromOpen_TransitionsAndLogsHistory()
    {
        var ticket = Ticket.Create("Titre", "Description", Guid.NewGuid(), TicketPriority.Medium);

        ticket.StartProgress();

        ticket.Status.Should().Be(TicketStatus.InProgress);
        ticket.History.Should().ContainSingle(h => h.Field == "Status" && h.NewValue == "InProgress");
    }

    [Fact]
    public void StartProgress_WhenAlreadyInProgress_ThrowsDomainException()
    {
        var ticket = Ticket.Create("Titre", "Description", Guid.NewGuid(), TicketPriority.Medium);
        ticket.StartProgress();

        var act = () => ticket.StartProgress();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateDetails_WhenResolved_ThrowsDomainException()
    {
        var ticket = Ticket.Create("Titre", "Description", Guid.NewGuid(), TicketPriority.Medium);
        ticket.StartProgress();
        ticket.Resolve("Redémarrage de l'appareil.");

        var act = () => ticket.UpdateDetails("Nouveau titre", "Nouvelle description");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FullLifecycle_OpenToClosed_Succeeds()
    {
        var ticket = Ticket.Create("Titre", "Description", Guid.NewGuid(), TicketPriority.Medium);

        ticket.StartProgress();
        ticket.Resolve("Redémarrage de l'appareil.");
        ticket.Close();

        ticket.Status.Should().Be(TicketStatus.Closed);
        ticket.History.Should().HaveCount(3);
    }

    [Fact]
    public void EnsureDeletable_WhenNotOpen_ThrowsDomainException()
    {
        var ticket = Ticket.Create("Titre", "Description", Guid.NewGuid(), TicketPriority.Medium);
        ticket.StartProgress();

        var act = () => ticket.EnsureDeletable();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Resolve_ValidNotes_StoresResolutionNotes()
    {
        var ticket = Ticket.Create("Titre", "Description", Guid.NewGuid(), TicketPriority.Medium);
        ticket.StartProgress();

        ticket.Resolve("Câble remplacé.");

        ticket.ResolutionNotes.Should().Be("Câble remplacé.");
    }

    [Fact]
    public void Resolve_EmptyNotes_ThrowsDomainException()
    {
        var ticket = Ticket.Create("Titre", "Description", Guid.NewGuid(), TicketPriority.Medium);
        ticket.StartProgress();

        var act = () => ticket.Resolve("");

        act.Should().Throw<DomainException>();
    }
}
