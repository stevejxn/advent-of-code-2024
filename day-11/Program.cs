using System.Diagnostics;
Console.WriteLine("Starting Day 11");

// run with the test data
//var dataFile = "../../../input_test.txt";

// run with the first input file
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var records = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new PartOneSolver(records);
var partOneStoneCount = partOneSolver.GetStoneCount(25);
Console.WriteLine($"Stone count: {partOneStoneCount} ({sw.ElapsedMilliseconds}ms)");

sw.Restart();
var partTwoSolver = new PartTwoSolver(records);
var partTwoStoneCount = partTwoSolver.GetStoneCount(75);
Console.WriteLine($"Stone count: {partTwoStoneCount} ({sw.ElapsedMilliseconds}ms)");

internal class PartOneSolver(long[] stones)
{
    private List<Func<long, long[]>> strategies =
    [
        ZeroToOneStrategy,
        EvenDigitsToTwoStonesStrategy,
        DefaultStrategy
    ];
    
    internal long GetStoneCount(int blinks)
    {
        var currentStones = stones.ToList();
        
        for (var i = 0; i < blinks; i++)
        {
            Console.WriteLine($"Blink {i + 1} current stone count: {currentStones.Count}");
            var workingStones = currentStones.ToList();
            currentStones.Clear();
            
            foreach (var stone in workingStones)
            {
                foreach (var strategy in strategies)
                {
                    var result = strategy(stone);
                    if (result.Length == 0) continue;
                    currentStones.AddRange(result);
                    break;
                }
            }
        }

        return currentStones.Count;
    }

    private static long[] ZeroToOneStrategy(long stoneValue)
    {
        if (stoneValue == 0)
        {
            return [1];
        }

        return [];
    }
    
    private static long[] EvenDigitsToTwoStonesStrategy(long stoneValue)
    {
        var stoneString = stoneValue.ToString();

        if (stoneString.Length % 2 != 0)
            return [];
        
        var midPoint = stoneString.Length / 2;
        return [long.Parse(stoneString[..midPoint]), long.Parse(stoneString[midPoint..])];
    }
    
    private static long[] DefaultStrategy(long stoneValue)
    {
        return [stoneValue * 2024];
    }
}

internal class PartTwoSolver(long[] stones)
{
    private List<Func<long, long[]>> strategies =
    [
        ZeroToOneStrategy,
        EvenDigitsToTwoStonesStrategy,
        DefaultStrategy
    ];

    private Dictionary<long, long> resultCache = [];
    
    internal long GetStoneCount(int blinks)
    {
        foreach (var stone in stones)
        {
            resultCache.Add(stone, 1);
        }
        
        for (var i = 0; i < blinks; i++)
        {
            var workingStones = resultCache.ToDictionary(k => k.Key, v => v.Value);
            resultCache.Clear();
            
            foreach (var stoneKvp in workingStones)
            {
                foreach (var strategy in strategies)
                {
                    var results = strategy(stoneKvp.Key);
                    
                    if (results.Length == 0) continue;

                    foreach (var result in results)
                    {
                        if (resultCache.ContainsKey(result))
                        {
                            resultCache[result] += stoneKvp.Value;
                        }
                        else
                        {
                            resultCache[result] = stoneKvp.Value;
                        }    
                    }
                    
                    break;
                }
            }
        }

        return resultCache.Values.Sum();
    }
    
    private static long[] ZeroToOneStrategy(long stoneValue)
    {
        if (stoneValue == 0)
        {
            return [1];
        }

        return [];
    }
    
    private static long[] EvenDigitsToTwoStonesStrategy(long stoneValue)
    {
        var stoneString = stoneValue.ToString();

        if (stoneString.Length % 2 != 0)
            return [];
        
        var midPoint = stoneString.Length / 2;
        return [long.Parse(stoneString[..midPoint]), long.Parse(stoneString[midPoint..])];
    }
    
    private static long[] DefaultStrategy(long stoneValue)
    {
        return [stoneValue * 2024];
    }
}

internal class DataReader(string path)
{
    public long[] LoadFile()
    {
        var f = File.OpenText(path);
        var line = f.ReadLine();
        var stones = line
            .Split(" ")
            .Select(long.Parse)
            .ToArray();

        return stones;
    }
}