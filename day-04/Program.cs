Console.WriteLine("Starting Day 04");

// run with the test data
//var dataFile = "../../../input_test_01.txt";
//var dataFile = "../../../input_test_02.txt";

// run with the first input file
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile, ['X', 'M', 'A', 'S']);
var data = reader.LoadFile();
var partOneSolver = new PartOneSolver(data, Enum.GetValues<Direction>());
var resultPart1 = partOneSolver.GetXmasCount();
Console.WriteLine($"Part one result: {resultPart1}");

var reader2 = new DataReader(dataFile, ['M', 'A', 'S']);
var data2 = reader2.LoadFile();
var partTwoSolver = new PartTwoSolver(data2);
var resultPart2 = partTwoSolver.GetMasCount();
Console.WriteLine($"Part two result: {resultPart2}");

internal class PartOneSolver(char[][] data, Direction[] directions)
{
    public long GetXmasCount()
    {
        var xmasCount = 0;
        
        for (var row = 0; row < data.Length; row++)
        {
            for (var col = 0; col < data[row].Length; col++)
            {
                xmasCount += FindWordAroundCell(data, ['X', 'M', 'A', 'S'], row, col);
            }
        }

        return xmasCount;
    }
    
    private int FindWordAroundCell(char[][] data, char[] checkFor, int row, int col)
    {
        if (data[row][col] != checkFor[0])
            return 0;

        return directions
            .Select(direction => IsMatch(data, GetCheckLocations(direction, row, col, checkFor)))
            .Sum();
    }

    private static int IsMatch(char[][] data, List<Check> checks)
    {
        return checks.All(c => c.Row < data.Length
                               && c.Row >= 0
                               && c.Col < data[c.Row].Length
                               && c.Col >= 0
                               && data[c.Row][c.Col] == c.Value)
            ? 1
            : 0;
    }

    private static readonly Dictionary<Direction, (Func<int, int, int> rowFn, Func<int, int, int> colFn)> locationFns =
        new()
        {
            { Direction.LeftToRight, ((row, idx) => row, (col, idx) => col + idx) },
            { Direction.DiagonalLowerRight, ((row, idx) => row + idx, (col, idx) => col + idx) },
            { Direction.TopToBottom, ((row, idx) => row + idx, (col, idx) => col) },
            { Direction.DiagonalLowerLeft, ((row, idx) => row + idx, (col, idx) => col - idx) },
            { Direction.RightToLeft, ((row, idx) => row, (col, idx) => col - idx) },
            { Direction.DiagonalUpperLeft, ((row, idx) => row - idx, (col, idx) => col - idx) },
            { Direction.BottomToTop, ((row, idx) => row - idx, (col, idx) => col) },
            { Direction.DiagonalUpperRight, ((row, idx) => row - idx, (col, idx) => col + idx) },
        };

    private static List<Check> GetCheckLocations(Direction direction, int row, int col, char[] find)
    {
        var fns = locationFns[direction];
        return find
            .Select((c, idx) => new Check { Row = fns.rowFn(row, idx), Col = fns.colFn(col, idx), Value = c })
            .ToList();
    }
}

internal class PartTwoSolver(char[][] data)
{
    private readonly List<char[][]> _patterns =
    [
        [
            ['M', '.', 'S'],
            ['.', 'A', '.'],
            ['M', '.', 'S'],
        ],
        [
            ['S', '.', 'M'],
            ['.', 'A', '.'],
            ['S', '.', 'M'],
        ],
        [
            ['M', '.', 'M'],
            ['.', 'A', '.'],
            ['S', '.', 'S'],
        ],
        [
            ['S', '.', 'S'],
            ['.', 'A', '.'],
            ['M', '.', 'M'],
        ]
    ];    
    
    public long GetMasCount()
    {
        var masCount = 0;
        
        for (var row = 0; row < data.Length; row++)
        {
            for (var col = 0; col < data[row].Length; col++)
            {
                masCount += FindXMasAroundCell(data, row, col);
            }
        }

        return masCount;
    }

    private int FindXMasAroundCell(char[][] data, int row, int col)
    {
        // check if any of the patterns exist starting at this cell
        var any = _patterns
            .Any(p => IsMatch(p, data, row, col));
        
        return any ? 1 : 0;
    }

    private static bool IsMatch(char[][] pattern, char[][] data, int row, int col)
    {
        if (!IsInBounds(pattern[0].Length, pattern.Length, data, row, col))
            return false;

        for (var prow = 0; prow < pattern.Length; prow++)
        {
            for (var pcol = 0; pcol < pattern[prow].Length; pcol++)
            {
                if (pattern[prow][pcol] == '.')
                    continue;
                
                if (data[row+prow][col+pcol] != pattern[prow][pcol])
                    return false;
            }
        }
        
        return true;
    }
    
    private static bool IsInBounds(int patternWidth, int patternHeight, char[][]data, int row, int col)
    {
        return row + patternHeight <= data.Length &&
               col + patternWidth <= data[row].Length;
    }
}

internal enum Direction
{
    LeftToRight,
    DiagonalLowerRight,
    TopToBottom,
    DiagonalLowerLeft,
    RightToLeft,
    DiagonalUpperLeft,
    BottomToTop,
    DiagonalUpperRight
}

internal struct Check
{
    public int Row { get; set; }
    public int Col { get; set; }
    public char Value { get; set; }
}

internal class DataReader(string path, char[] allowedChars)
{
    public char[][] LoadFile()
    {
        var lines = new List<char[]>();
        
        var f = File.OpenText(path);
        while (f.ReadLine() is { } line)
        {
            lines.Add(line
                .ToCharArray()
                .Select(c => allowedChars.Contains(c) ? c  : '.')
                .ToArray());
        }
        
        return lines.ToArray();
    }
}