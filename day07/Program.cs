using System.Diagnostics;

Console.WriteLine("Starting Day 07");

// run with the test data
//var dataFile = "../../../input_test.txt";

// run with the test data
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var equations = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new Solver(equations, ['+', '*']);
var resultPart1 = partOneSolver.CalculateTotalCalibrationResult();
Console.WriteLine($"Part one result: {resultPart1} ({sw.ElapsedMilliseconds}ms)");
sw.Restart();

var partTwoSolver = new Solver(equations, ['+', '*', '|']);
var resultPart2 = partTwoSolver.CalculateTotalCalibrationResult();
Console.WriteLine($"Part two result: {resultPart2} ({sw.ElapsedMilliseconds}ms)");

internal class Solver(Equation[] equations, char[] supportedOperations)
{
    private Lock _lock = new Lock();

    public long CalculateTotalCalibrationResult()
    {
        long result = 0;

        Parallel.ForEach(equations, equation =>
        {
            var possibleOperations = GetPossibleOperations(equation.Values.Length, supportedOperations);

            foreach (var op in possibleOperations)
            {
                var resultOption = CalcResult(op, equation.Values);

                if (resultOption != equation.Result)
                    continue;

                lock (_lock)
                {
                    result += resultOption;
                }

                break;
            }
        });

        return result;
    }

    private long CalcResult(char[] operations, long[] values)
    {
        var ops = new Queue<char>(operations);
        var vals = new Queue<long>(values);

        var result = vals.Dequeue();

        // loop through the vals stack
        while (vals.Count > 0)
        {
            var op = ops.Dequeue();
            var val = vals.Dequeue();

            var calcFn = GetOperation(op);
            result = calcFn(result, val);
        }

        return result;
    }
    
    private Func<long, long, long> GetOperation(char op)
    {
        return op switch
        {
            '+' => (a, b) => a + b,
            '*' => (a, b) => a * b,
            '|' => (a, b) => long.Parse($"{a}{b}"), 
            _ => throw new InvalidOperationException($"Invalid operation {op}")
        };
    }
    
    private List<char[]> GetPossibleOperations(int numValues, char[] operators)
    {
        var numOperations = numValues - 1;
        
        var result = new List<char[]>();
        BuildCombinations("", numOperations, operators, result);
        
        return result;
    }

    private void BuildCombinations(string prefix, int length, char[] operators, List<char[]> result)
    {
        if (length == 0)
        {
            result.Add(prefix.ToArray());
            return;
        }
        
        foreach (var op in operators)
        {
            BuildCombinations(prefix + op, length - 1, operators, result);    
        }
    }
}

internal class Equation
{
    public long Result { get; set; }
    public long[] Values { get; set;  }
}

internal class DataReader(string path)
{
    public Equation[] LoadFile()
    {
        var equations = new List<Equation>();
        
        var f = File.OpenText(path);
        while (f.ReadLine() is { } line)
        {
            var data = line.Split(":");
            var result = long.Parse(data[0]);
            var values = data[1]
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(long.Parse)
                .ToArray();

            equations.Add(new Equation { Result = result, Values = values });
        }

        return equations.ToArray();
    }
}
