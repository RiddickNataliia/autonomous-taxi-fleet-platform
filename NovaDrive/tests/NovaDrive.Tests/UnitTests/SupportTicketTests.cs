namespace NovaDrive.Tests.UnitTests;

public class SupportTicketTests
{

    [Fact]
    public void Create_SetsInitialValues()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue with ride", "The driver was late", TicketPriority.Medium);

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal("Issue with ride", ticket.Subject);
        Assert.Equal("The driver was late", ticket.Description);
        Assert.Equal(TicketPriority.Medium, ticket.Priority);
    }

    [Fact]
    public void Create_WithEmptySubject_Throws()
    {
        var ex = Assert.Throws<SupportDomainException>(() => SupportTicket.Create(Guid.NewGuid(), "", "Valid description", TicketPriority.Low));
        Assert.Equal(SupportDomainException.InvalidSubject, ex.Message);
    }

    [Fact]
    public void Create_WithEmptyDescription_Throws()
    {
        var ex = Assert.Throws<SupportDomainException>(() => SupportTicket.Create(Guid.NewGuid(), "Valid subject", "", TicketPriority.Low));
        Assert.Equal(SupportDomainException.InvalidDescription, ex.Message);
    }

    [Fact]
    public void StartWork_FromOpen_SetsStatusToInProgress()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description", TicketPriority.Low);
        ticket.StartWork();
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }

    [Fact]
    public void StartWork_WhenNotOpen_Throws()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description", TicketPriority.Low);
        ticket.StartWork(); // now InProgress

        var ex = Assert.Throws<SupportDomainException>(() => ticket.StartWork());
        Assert.Equal(SupportDomainException.NotOpen, ex.Message);
    }

    [Fact]
    public void Resolve_FromInProgress_SetsStatusToResolvedAndResolvedAt()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description", TicketPriority.Low);
        ticket.StartWork();
        ticket.Resolve();

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.True(ticket.ResolvedAt.HasValue);
        Assert.InRange(ticket.ResolvedAt.Value, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Resolve_WhenNotInProgress_Throws()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description", TicketPriority.Low);
        // Still Open, not InProgress

        var ex = Assert.Throws<SupportDomainException>(() => ticket.Resolve());
        Assert.Equal(SupportDomainException.NotInProgress, ex.Message);
    }

    [Fact]
    public void UpdatePriority_ChangesPriority()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description", TicketPriority.Low);
        ticket.UpdatePriority(TicketPriority.High);
        Assert.Equal(TicketPriority.High, ticket.Priority);
    }

    [Fact]
    public void UpdatePriority_WithUnknown_Throws()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description", TicketPriority.Low);

        var ex = Assert.Throws<SupportDomainException>(() => ticket.UpdatePriority(TicketPriority.Unknown));
        Assert.Equal(SupportDomainException.InvalidPriority, ex.Message);
    }

    [Fact]
    public void UpdatePriority_ToSameValue_IsIndempotent()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description", TicketPriority.Medium);
        ticket.UpdatePriority(TicketPriority.Medium); // same priority

        Assert.Equal(TicketPriority.Medium, ticket.Priority); // should remain unchanged
    }
}


