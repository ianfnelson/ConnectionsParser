namespace ConnectionsParser;

public class ConnectionsRange
{
    public string Name { get; set; }
    
    public List<ConnectionsRangeEntry> Entries { get; set; }
}

public class ConnectionsRangeEntry
{
    public string Rank { get; set; }
    
    public string FamilyMember { get; set; }
    
    public int Played { get; set; }
    
    public double Average { get; set; }
    

    public override string ToString()
    {
        return Rank.PadRight(3) +
               FamilyMember.PadRight(16) +
               Played.ToString().PadLeft(4) + "   " +
               Average.ToString("F3").PadLeft(6);
    }
}