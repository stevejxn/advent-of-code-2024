using System.Collections;

Console.WriteLine("Starting Day 05");

// run with the test data
//var dataFile = "../../../input_test_01.txt";

// run with the first input file
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var (pageOrderingRules, pageUpdates) = reader.LoadFile();
var partOneSolver = new PartOneSolver(pageOrderingRules, pageUpdates);
var resultPart1 = partOneSolver.GetSumMiddlePages();
Console.WriteLine($"Part one result: {resultPart1}");

var partTwoSolver = new PartTwoSolver(pageOrderingRules, pageUpdates);
var resultPart2 = partTwoSolver.GetSumMiddlePages();
Console.WriteLine($"Part two result: {resultPart2}");

Console.WriteLine("Done!");

internal class PartOneSolver(PageOrderingRules pageOrderingRules, PageUpdates pageUpdates)
{
    public long GetSumMiddlePages()
    {
        return pageUpdates
            .Where(pageUpdate => pageUpdate.IsCorrectlyOrdered(pageOrderingRules))
            .Sum(pageUpdate => pageUpdate.MiddlePage);
    }
}

internal class PartTwoSolver(PageOrderingRules pageOrderingRules, PageUpdates pageUpdates)
{
    public long GetSumMiddlePages()
    {   
        return pageUpdates
            .Where(x => !x.IsCorrectlyOrdered(pageOrderingRules))
            .Sum(x => x.GetCorrectOrder(pageOrderingRules).MiddlePage);
    }
}

internal class PageOrderingRules
{
    private readonly List<PageOrder> _orderingRules = [];
    
    public List<int> GetPagesAfter(int page)
    {
        return _orderingRules
            .Where(rule => rule.Before == page)
            .Select(rule => rule.After)
            .ToList();
    }
    
    public List<int> GetPagesBefore(int page)
    {
        return _orderingRules
            .Where(rule => rule.After == page)
            .Select(rule => rule.Before)
            .ToList();
    }

    public void AddRule(string[] beforeAfterPair)
    {
        if (beforeAfterPair.Length != 2)
        {
            throw new Exception("Expected a pair of two integers!");
        }

        _orderingRules.Add(new PageOrder(
            int.Parse(beforeAfterPair[0]),
            int.Parse(beforeAfterPair[1])));
    }
}

internal class PageOrder(int before, int after)
{
    public int Before { get; } = before;
    public int After { get; } = after;
}

internal class PageUpdates() : IEnumerable<PageUpdate>
{
    private readonly List<PageUpdate> _updates = [];

    public void AddUpdate(int[] pageUpdates)
    {
        _updates.Add(new PageUpdate(pageUpdates));
    }

    public IEnumerator<PageUpdate> GetEnumerator()
    {
        return _updates.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

internal class PageUpdate(int[] pageUpdates)
{
    public bool IsCorrectlyOrdered(PageOrderingRules pageOrderingRules)
    {
        for (var i = 0; i < pageUpdates.Length; i++)
        {
            var pagesAfter = pageOrderingRules.GetPagesAfter(pageUpdates[i]);
            var pagesBefore = pageOrderingRules.GetPagesBefore(pageUpdates[i]);
            
            var containsAllAfter = pageUpdates.Skip(i + 1).All(x => pagesAfter.Contains(x));
            var containsAllBefore = pageUpdates.Take(i).All(x => pagesBefore.Contains(x));
            
            if (!( containsAllAfter && containsAllBefore)) return false;
        }

        return true;
    }

    public PageUpdate GetCorrectOrder(PageOrderingRules pageOrderingRules)
    {
        var pages = pageUpdates.Select(i => i).ToList();
        pages.Sort(new PageOrderingComparer(pageOrderingRules));
        return new PageUpdate(pages.ToArray());
    }

    public int MiddlePage { get; } = pageUpdates[pageUpdates.Length / 2];

    private class PageOrderingComparer(PageOrderingRules pageOrderingRules) : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            var pagesAfter = pageOrderingRules.GetPagesAfter(x);
            var pagesBefore = pageOrderingRules.GetPagesBefore(x);

            if (pagesAfter.Contains(y))
                return -1;

            if (pagesBefore.Contains(y))
                return 1;

            return 0;
        }
    }
}

internal class DataReader(string path)
{
    public (PageOrderingRules, PageUpdates) LoadFile()
    {
        var pageOrderingRules = new PageOrderingRules();
        
        var f = File.OpenText(path);
        while (f.ReadLine() is { } line)
        {
            if (line == "")
            {
                break;
            }
            pageOrderingRules.AddRule(line.Split('|'));
        }
        
        var pageUpdates = new PageUpdates();
        while (f.ReadLine() is { } line)
        {
            pageUpdates.AddUpdate(line.Split(',').Select(int.Parse).ToArray());
        }

        return (pageOrderingRules, pageUpdates);
    }
}