namespace NovaDrive.Tests.UnitTests;

public class SupportTicketTests
{
    [Fact]
    public void Create_SetsInitialValues()
    {
        var passengerId = Guid.NewGuid();
        var before = DateTimeOffset.UtcNow;

        var ticket = SupportTicket.Create(passengerId, "Issue with ride", "The driver was late");

        Assert.Equal(passengerId, ticket.PassengerId);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal("Issue with ride", ticket.Subject);
        Assert.Equal("The driver was late", ticket.Description);
        Assert.Equal(TicketPriority.Unknown, ticket.Priority);
        Assert.Null(ticket.ResolvedAt);
        Assert.True(ticket.CreatedAt >= before);
    }

    [Fact]
    public void Create_WithEmptySubject_Throws()
    {
        var ex = Assert.Throws<SupportDomainException>(() => SupportTicket.Create(Guid.NewGuid(), "", "Valid description"));
        Assert.Equal(SupportDomainException.InvalidSubject, ex.Message);
    }

    [Fact]
    public void Create_WithEmptyDescription_Throws()
    {
        var ex = Assert.Throws<SupportDomainException>(() => SupportTicket.Create(Guid.NewGuid(), "Valid subject", ""));
        Assert.Equal(SupportDomainException.InvalidDescription, ex.Message);
    }

    [Fact]
    public void StartWork_FromOpen_SetsStatusToInProgress()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");
        ticket.StartWork();
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
    }

    [Fact]
    public void StartWork_WhenNotOpen_Throws()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");
        ticket.StartWork(); // now InProgress

        var ex = Assert.Throws<SupportDomainException>(() => ticket.StartWork());
        Assert.Equal(SupportDomainException.NotOpen, ex.Message);
    }

    [Fact]
    public void Resolve_FromInProgress_SetsStatusToResolvedAndResolvedAt()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");
        ticket.StartWork();
        ticket.Resolve();

        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.True(ticket.ResolvedAt.HasValue);
        Assert.InRange(ticket.ResolvedAt.Value, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Resolve_WhenNotInProgress_Throws()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");
        // Still Open, not InProgress

        var ex = Assert.Throws<SupportDomainException>(() => ticket.Resolve());
        Assert.Equal(SupportDomainException.NotInProgress, ex.Message);
    }

    [Fact]
    public void Resolve_FromOpen_Throws()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");

        var ex = Assert.Throws<SupportDomainException>(() => ticket.Resolve());
        Assert.Equal(SupportDomainException.NotInProgress, ex.Message);
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_Throws()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");
        ticket.StartWork();
        ticket.Resolve();

        var ex = Assert.Throws<SupportDomainException>(() => ticket.Resolve());
        Assert.Equal(SupportDomainException.NotInProgress, ex.Message);
    }

    [Fact]
    public void UpdatePriority_ChangesPriority()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");
        ticket.UpdatePriority(TicketPriority.High);
        Assert.Equal(TicketPriority.High, ticket.Priority);
    }

    [Fact]
    public void UpdatePriority_WithUnknown_Throws()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");

        var ex = Assert.Throws<SupportDomainException>(() => ticket.UpdatePriority(TicketPriority.Unknown));
        Assert.Equal(SupportDomainException.InvalidPriority, ex.Message);
    }

    [Fact]
    public void UpdatePriority_ToSameValue_IsIndempotent()
    {
        var ticket = SupportTicket.Create(Guid.NewGuid(), "Issue", "Description");
        ticket.UpdatePriority(TicketPriority.Medium); 
        ticket.UpdatePriority(TicketPriority.Medium); // should not throw or change state
        Assert.Equal(TicketPriority.Medium, ticket.Priority); // should remain unchanged
    }
}


