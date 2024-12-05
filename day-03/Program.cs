using System.Text.RegularExpressions;

Console.WriteLine("Starting Day 03");

// run with the test data
//var dataFile = "../../../input_test_01.txt";
//var dataFile = "../../../input_test_02.txt";

// run with the first input file
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var data = reader.LoadFile();

var partOneSolver = new PartOneSolver(data);
var resultPart1 = partOneSolver.CalculateResult();
Console.WriteLine($"Part one result: {resultPart1}");

var partTwoSolver = new PartTwoSolver(data);
var resultPart2 = partTwoSolver.CalculateResult();
Console.WriteLine($"Part two result: {resultPart2}");

internal partial class PartOneSolver(string data)
{
    public long CalculateResult()
    {
        return FindMulRegex()
            .Matches(data)
            .Select(m => m.Value)
            .Select(Helper.ExtractNumberPair)
            .Sum(numPair => numPair[0] * numPair[1]);
    }

    [GeneratedRegex(@"mul\(\d{1,3},\d{1,3}\)")]
    private static partial Regex FindMulRegex();
}

internal partial class PartTwoSolver(string data)
{
    public long CalculateResult()
    {
        long result = 0;
        var doing = true;
        
        FindMulAndConditionals()
            .Matches(data)
            .Select(m => m.Value)
            .ToList()
            .ForEach(m =>
            {
                if (m is "do" or "don't")
                {
                    doing = m == "do";
                    return;
                }

                if (!doing) return;
                
                var r= Helper.ExtractNumberPair(m);
                result += r[0] * r[1];
            });
        
        return result;
    }

    [GeneratedRegex(@"mul\(\d{1,3},\d{1,3}\)|don't|do")]
    private static partial Regex FindMulAndConditionals();
}

internal static class Helper
{
    internal static long[] ExtractNumberPair(string mulInput)
    {
        return mulInput.Replace("mul(", "")
            .Replace(")", "")
            .Split(",")
            .Select(long.Parse)
            .ToArray();
    }
}

internal class DataReader(string path)
{
    public string LoadFile()
    {
        return File.ReadAllText(path);
    }
}