using System.Diagnostics;

Console.WriteLine("Starting Day 08");

// run with the test data
//var dataFile = "../../../input_test.txt";
//var dataFile = "../../../input_test_2.txt";

// run with the test data
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var grid = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new PartOneSolver(grid);
var part1Result = partOneSolver.GetUniqueLocations();
Console.WriteLine($"Part one result: {part1Result} ({sw.ElapsedMilliseconds}ms)");

sw.Restart();
var partTwoSolver = new PartTwoSolver(grid);
var part2Result = partTwoSolver.GetUniqueLocations();
Console.WriteLine($"Part two result: {part2Result} ({sw.ElapsedMilliseconds}ms)");

internal class PartOneSolver(Grid grid)
{
    public long GetUniqueLocations()
    {
        var result = 0;
        
        var uniqueAntenna = grid.FindUniqueChars('.');
        var antinodeGrid = grid.Clone() as Grid;

        foreach (var antenna in uniqueAntenna)
        {
            var positions = grid.FindAllPositionsOf(antenna);
            var combinations = GetPossibleCombinations(positions);
            foreach (var combination in combinations)
            {
                // get the x,y difference between the two points
                var xDiff = (combination.Item1.Col - combination.Item2.Col);
                var yDiff = (combination.Item1.Row - combination.Item2.Row);
                
                // get the position of the antinodes, taking into account the direction
                 var antinode1 = new Point(combination.Item1.Row + yDiff, combination.Item1.Col + xDiff);
                 var antinode2 = new Point(combination.Item2.Row - yDiff, combination.Item2.Col - xDiff);
                //
                // check if the antinodes are out of bounds
                if (!grid.IsOutOfBounds(antinode1))
                {
                    antinodeGrid.SetData(antinode1, '#');
                }
                
                if (!grid.IsOutOfBounds(antinode2))
                {
                    antinodeGrid.SetData(antinode2, '#');
                }
            }
        }

        Console.WriteLine(antinodeGrid);
        
        return antinodeGrid.CountEntries('#');
    }
    
    private List<(Point, Point)> GetPossibleCombinations(List<Point> points)
    {
        var result = new List<(Point, Point)>();

        for (var i = 0; i < points.Count; i++)
        {
            for (var j = i + 1; j < points.Count; j++)
            {
                result.Add((points[i], points[j]));
            }
        }

        return result;
    }
}

internal class PartTwoSolver(Grid grid)
{
    public long GetUniqueLocations()
    {
        var result = 0;
        
        var uniqueAntenna = grid.FindUniqueChars('.');
        var antinodeGrid = grid.Clone() as Grid;

        foreach (var antenna in uniqueAntenna)
        {
            var positions = grid.FindAllPositionsOf(antenna);
            var combinations = GetPossibleCombinations(positions);
            foreach (var combination in combinations)
            {
                // each antenna is also an antinode
                antinodeGrid.SetData(combination.Item1, '#');
                antinodeGrid.SetData(combination.Item2, '#');
                
                // get the x,y difference between the two points
                var xDiff = (combination.Item1.Col - combination.Item2.Col);
                var yDiff = (combination.Item1.Row - combination.Item2.Row);
                
                // write the antinodes to the grid until we go out of bounds
                var xd = xDiff;
                var yd = yDiff;
                var nextAntinode1 = new Point(combination.Item1.Row + yd, combination.Item1.Col + xd);
                while (!grid.IsOutOfBounds(nextAntinode1))
                {
                    antinodeGrid.SetData(nextAntinode1, '#');
                    xd += xDiff;
                    yd += yDiff;
                    nextAntinode1 = new Point(combination.Item1.Row + yd, combination.Item1.Col + xd);
                }

                xd = xDiff;
                yd = yDiff;
                var nextAntinode2 = new Point(combination.Item2.Row - yDiff, combination.Item2.Col - xDiff);
                while (!grid.IsOutOfBounds(nextAntinode2))
                {
                    antinodeGrid.SetData(nextAntinode2, '#');
                    xd += xDiff;
                    yd += yDiff;
                    nextAntinode2 = new Point(combination.Item2.Row - yd, combination.Item2.Col - xd);
                }
            }
        }

        Console.WriteLine(antinodeGrid);
        
        return antinodeGrid.CountEntries('#');
    }
    
    private List<(Point, Point)> GetPossibleCombinations(List<Point> points)
    {
        var result = new List<(Point, Point)>();

        for (var i = 0; i < points.Count; i++)
        {
            for (var j = i + 1; j < points.Count; j++)
            {
                result.Add((points[i], points[j]));
            }
        }

        return result;
    }
}

internal class Grid: ICloneable
{
    private readonly List<char[]> _data = [];
    
    public void AddLine(char[] line)
    {
        _data.Add(line);
    }
    
    public char[] FindUniqueChars(params char[] ignore)
    {
        return _data
            .SelectMany(row => row)
            .Where(x => !ignore.Contains(x))
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();        
    }
    
    public List<Point> FindAllPositionsOf(char value)
    {
        var positions = new List<Point>();
        
        for (var row = 0; row < _data.Count; row++)
        {
            for (var col = 0; col < _data[row].Length; col++)
            {
                if (_data[row][col] == value)
                {
                    positions.Add(new Point(row, col));
                }
            }
        }
        
        return positions;
    }
    
    public void SetData(Point point, char value)
    {
        _data[point.Row][point.Col] = value;
    }
    
    // public char GetData(Point point)
    // {
    //     return _data[point.Row][point.Col];
    // }
    //
    // public Point GetPositionOf(char value)
    // {
    //     for (var row = 0; row < _data.Count; row++)
    //     {
    //         var pos = Array.IndexOf(_data[row], value);
    //         if (pos != -1)
    //         {
    //             return new Point(row, pos);
    //         }
    //     }
    //     
    //     return Point.Empty;
    // }
    //
    public int CountEntries(char value)
    {
        return _data.Sum(row => row.Count(c => c == value));
    }
    
    // public int Width => _data[0].Length;
    // public int Height => _data.Count;
    //
    public bool IsOutOfBounds(Point point)
    {
        return point.Row < 0 || point.Row >= _data.Count ||  point.Col < 0 || point.Col >= _data[point.Row].Length;
    }
    
    public override string ToString()
    {
        return string.Join("\n", _data.Select(row => new string(row)));
    }

    public object Clone()
    {
        var newGrid = new Grid();
        foreach (var row in _data)
        {
            var r = new char[row.Length];
            Array.Copy(row, r, row.Length);
            newGrid.AddLine(r);
        }

        return newGrid;
    }
}


internal class Point(int row, int col)
{
    public int Col { get; } = col;
    public int Row { get; } = row;
    
    public static Point Empty = new Point(-1, -1);

    public override string ToString()
    {
        return $"({Row}, {Col})";
    }
}

internal class DataReader(string path)
{
    public Grid LoadFile()
    {
        var grid = new Grid();
        
        var f = File.OpenText(path);
        while (f.ReadLine() is { } line)
        {
            grid.AddLine(line.ToCharArray());
        }

        return grid;
    }
}
