using System.Text.RegularExpressions;

namespace ConnectionsParser;

public class ConnectionsParser
{
    // [dd/MM/yyyy, HH:mm:ss] Name: First line of text
    private static readonly Regex HeaderRx = new(
        @"^\[(?<date>\d{2}/\d{2}/\d{4}),\s*(?<time>\d{2}:\d{2}:\d{2})\]\s*(?<name>[^:]+):\s*(?<text>.*)$",
        RegexOptions.Compiled);

    private static readonly Regex PuzzleRx = new(
        @"(?i)\bPuzzle\s*#\s*(?<num>\d+)\b",
        RegexOptions.Compiled);

    // Lines of exactly 4 NYT squares (allow any mix)
    private static readonly Regex AnyColorLineRx = new(@"[🟨🟩🟦🟪]{4}", RegexOptions.Compiled);

    // Lines of 4 identical squares (a solved group line)
    private static readonly Regex HomogColorLineRx = new(@"🟨🟨🟨🟨|🟩🟩🟩🟩|🟦🟦🟦🟦|🟪🟪🟪🟪", RegexOptions.Compiled);
    
    public static IEnumerable<ConnectionsRange> Parse(string filepath)
    {
        var scores = GetScores(filepath).ToList();

        var years = scores.GroupBy(x => x.Date.Year.ToString())
            .OrderByDescending(x => x.Key)
            .Take(2);
        
        foreach (var year in years)
        {
            yield return BuildRange(year.Key, year);
        }

        var months = scores
            .GroupBy(x => $"{x.Date.Year}-{x.Date.Month:00}")
            .OrderByDescending(x => x.Key)
            .Take(2);
        
        foreach (var month in months)
        {
            var monthName = month.First().Date.ToString("MMMM");
            yield return BuildRange(monthName, month);
        }
    }
    
    private static IEnumerable<ConnectionsScore> GetScores(string filepath)
    {
        return ParseScores(filepath)
            .DistinctBy(x => new { x.Date, x.FamilyMember });
    }
    
    private static IEnumerable<ConnectionsScore> ParseScores(string filePath)
    {
        string? currName = null;
        var currLines = new List<string>();

        foreach (var line in File.ReadLines(filePath))
        {
            var m = HeaderRx.Match(line);
            if (m.Success)
            {
                foreach (var r in Flush()) yield return r;

                currName = m.Groups["name"].Value.Trim();

                currLines.Clear();
                currLines.Add(m.Groups["text"].Value);
            }
            else if (currName != null)
            {
                currLines.Add(line);
            }
        }

        foreach (var r in Flush()) yield return r;
        yield break;

        IEnumerable<ConnectionsScore> Flush()
        {
            if (currName == null || currLines.Count == 0) yield break;

            var block = string.Join("\n", currLines);

            var pMatch = PuzzleRx.Match(block);
            if (!pMatch.Success) yield break;
            var puzzle = int.Parse(pMatch.Groups["num"].Value);

            // Count emoji rows
            var emojiRows = currLines.Count(l => AnyColorLineRx.IsMatch(l));
            var successes= currLines.Count(l => HomogColorLineRx.IsMatch(l));
            var mistakes= emojiRows - successes;
            
            var score = 10 * successes - mistakes;

            yield return new ConnectionsScore
            {
                FamilyMember = currName!,
                Puzzle = puzzle,
                Score = score
            };
        }
    }
    
    private static ConnectionsRange BuildRange(string name, IGrouping<string, ConnectionsScore> scoreGroup)
    {
        var range = new ConnectionsRange { Name = "Connections — " + name };

        var entries = scoreGroup
            .GroupBy(x => x.FamilyMember)
            .OrderByDescending(x => x.Average(y => y.Score))
            .ThenBy(x => x.Key)
            .Select(familyMember => new ConnectionsRangeEntry
            {
                Average = familyMember.Average(x => x.Score),
                FamilyMember = familyMember.Key,
                Played = familyMember.Count()
            }).ToList();

        AssignRanks(entries);

        range.Entries = entries;

        return range;
    }
    
    private static void AssignRanks(List<ConnectionsRangeEntry> entries)
    {
        var currentRank = 1;

        for (var i = 0; i < entries.Count; i++)
        {
            if (i > 0 && 
                Math.Abs(entries[i].Average - entries[i - 1].Average) < 0.0001D)
            {
                entries[i - 1].Rank = entries[i - 1].Rank.Replace(".", "=");
                entries[i].Rank = $"{entries[i - 1].Rank}";
            }
            else
            {
                entries[i].Rank = $"{currentRank}.";
            }

            currentRank++;
        }
    }
}