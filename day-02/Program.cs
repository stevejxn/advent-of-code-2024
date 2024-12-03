Console.WriteLine("Starting Day 02");

// run with the test data
//var dataFile = "../../../input_test.txt";

// run with the first input file
var dataFile = "../../../input_main_01.txt";

var reader = new DataReader(dataFile);
var records = reader.LoadFile();

var partOneSolver = new PartOneSolver(records);
var safeCount = partOneSolver.GetSafeCount();
Console.WriteLine($"Safe count: {safeCount}");

var partTwoSolver = new PartTwoSolver(records);
var safeWithDampenerCount = partTwoSolver.GetSafeCount();
Console.WriteLine($"Safe count with dampener: {safeWithDampenerCount}");

internal class PartOneSolver(long[][] records)
{
    public int GetSafeCount() => records.Count(r => r.IsSafe());
}

internal class PartTwoSolver(long[][] records)
{
    public int GetSafeCount()
    {
        var safe = records
            .Where(r => r.IsSafe());
        
        var safeWithDampener = records
            .Except(safe)
            .Where(IsSafeWithDampener);
        
        return safe.Count() + safeWithDampener.Count();
    }

    private static bool IsSafeWithDampener(long[] levels)
    {
        return Enumerable.Range(0, levels.Length)
            .Select(i => levels
                .Where((_, index) => index != i)
                .ToArray())
            .Any(checkLevels => checkLevels.IsSafe());
    }
}

internal static class Extensions
{
    internal static bool IsSafe(this long[] levels)
    {
        var (minDiff, maxDiff) = levels.GetMinMaxDiffs();

        // must be ascending or descending
        // and adjacent diff is at least 1 and at most 3
        return levels.IsIncOrDec() 
               && minDiff >= 1 
               && maxDiff <= 3;
    }

    private static bool IsIncOrDec(this long[] levels)
    {
        // if the levels are the same when sorted in ascending or descending order 
        // then they are either increasing or decreasing
        return levels.Order().SequenceEqual(levels) ||
               levels.OrderDescending().SequenceEqual(levels);
    }

    private static (long min, long max) GetMinMaxDiffs(this long[] levels)
    {
        var offsets = levels.Skip(1);
        var pairs = levels.Zip(offsets);

        var diffs = pairs
            .Select(pair => Math.Abs(pair.First - pair.Second))
            .ToArray();
        
        return (diffs.Min(), diffs.Max());
    }
}

internal class DataReader(string path)
{
    public long[][] LoadFile()
    {
        List<long[]> records = [];
        
        var f = File.OpenText(path);
        while (f.ReadLine() is { } line)
        {
            var levels = line
                .Split(" ")
                .Select(long.Parse)
                .ToArray();

            records.Add(levels);
        }

        return records.ToArray();
    }
}