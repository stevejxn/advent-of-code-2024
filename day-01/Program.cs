Console.WriteLine("Starting");

// run with the test data
//var dataFile = "../../../input_test.txt";

// run with the first input file
var dataFile = "../../../input_main_01.txt";

var reader = new DataReader(dataFile);
var (l1, l2) = reader.LoadFileAndOrderItems();

var partOneSolver = new PartOneSolver(l1, l2);
Console.WriteLine($"The total distance is {partOneSolver.CalculateDistance()}");

var partTwoSolver = new PartTwoSolver(l1, l2);
Console.WriteLine($"The similarity score is {partTwoSolver.CalculateSimilarityScore()}");

internal class PartOneSolver(List<long> list1, List<long> list2)
{
    public long CalculateDistance()
    {
        return list1
            .Zip(list2)
            .Sum(pair => Math.Abs(pair.First - pair.Second));
    }
}

internal class PartTwoSolver(List<long> list1, List<long> List2)
{
    public long CalculateSimilarityScore()
    {
        return list1
            .Select(n => List2.Count(x => x == n) * n)
            .Sum();
    }
}

internal class DataReader(string path)
{
    public (List<long>, List<long>) LoadFileAndOrderItems()
    {
        List<long> list1 = [];
        List<long> list2 = [];

        var f = File.OpenText(path);
        while (f.ReadLine() is { } line)
        {
            var items = line
                .Split("   ")
                .Select(x => x.Trim());

            if (items.Count() != 2)
            {
                throw new Exception("Expected number pairs!");
            }

            list1.Add(long.Parse(items.First()));
            list2.Add(long.Parse(items.Last()));
        }

        return (
            list1.Order().ToList(),
            list2.Order().ToList()
        );
    }
}