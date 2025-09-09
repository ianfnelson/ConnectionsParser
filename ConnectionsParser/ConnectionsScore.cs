namespace ConnectionsParser;

public class ConnectionsScore
{
    private readonly DateOnly _connectionsEpoch = new(2023, 6, 11);
    public DateOnly Date => _connectionsEpoch.AddDays(Puzzle);
    
    public required string FamilyMember { get; set; }
    
    public int Puzzle { get; set; }
    
    public int Score { get; set; }
}