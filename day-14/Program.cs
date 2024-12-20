using System.Diagnostics;
using System.Text;

Console.WriteLine("Starting Day 14");

// run with the test data
// var width = 11;
// var height = 7;
// var dataFile = "../../../input_test.txt";

// run with the main data
var width = 101;
var height = 103;
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var robots = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new PartOneSolver(robots);
var part1Result = partOneSolver.GetSafetyFactor(100, width, height);
Console.WriteLine($"Part one result: {part1Result} ({sw.ElapsedMilliseconds}ms)");

sw.Restart();
var partTwoSolver = new PartTwoSolver(robots);
var part2Result = partTwoSolver.GetSecondsToTree(width, height);
Console.WriteLine($"Part two result: {part2Result} ({sw.ElapsedMilliseconds}ms)");

internal class PartOneSolver(List<Robot> robots)
{
    internal long GetSafetyFactor(int seconds, int gridWidth, int gridHeight)
    {
        var grid = InitGrid(gridWidth, gridHeight);
        
        foreach (var robot in robots)
        {
            Console.WriteLine($"Position: {robot.StartingLocation} Velocity: {robot.Velocity}");
            
            var fullPosition = GetFullPositionAtTime(robot, seconds);
            var position = GetPositionOnGrid(grid, fullPosition);
            grid.Increment(position);
        }
        
        SetGridIntersectionsToZero(grid);
        
        var robotCounts = GetRobotCountPerQuadrant(grid);
        var safetyFactor = robotCounts.Aggregate((a, b) => a * b);
        
        Console.WriteLine(grid.ToString());
        
        return safetyFactor;
    }

    private List<int> GetRobotCountPerQuadrant(Grid grid)
    {
        var robotCounts = new List<int>();
        
        var midWidth = grid.Width / 2;
        var midHeight = grid.Height / 2;
        
        // quadrant 1 coords
        var q1RowStart = 0;
        var q1RowEnd = midHeight;
        var q1ColStart = 0;
        var q1ColEnd = midWidth;
        robotCounts.Add(SumCellsInRange(grid, q1RowStart, q1RowEnd, q1ColStart, q1ColEnd));
        
        // quadrant 2 coords
        var q2RowStart = 0;
        var q2RowEnd = midHeight;
        var q2ColStart = midWidth;
        var q2ColEnd = grid.Width;
        robotCounts.Add(SumCellsInRange(grid, q2RowStart, q2RowEnd, q2ColStart, q2ColEnd));
        
        // quadrant 3 coords
        var q3RowStart = midHeight;
        var q3RowEnd = grid.Height;
        var q3ColStart = 0;
        var q3ColEnd = midWidth;
        robotCounts.Add(SumCellsInRange(grid, q3RowStart, q3RowEnd, q3ColStart, q3ColEnd));
        
        // quadrant 4 coords
        var q4RowStart = midHeight;
        var q4RowEnd = grid.Height;
        var q4ColStart = midWidth;
        var q4ColEnd = grid.Width;
        robotCounts.Add(SumCellsInRange(grid, q4RowStart, q4RowEnd, q4ColStart, q4ColEnd));
        
        return robotCounts;
    }

    private int SumCellsInRange(Grid grid, int rowStart, int rowEnd, int colStart, int colEnd)
    {
        var sum = 0;
        
        for (var row = rowStart; row < rowEnd; row++)
        {
            for (var col = colStart; col < colEnd; col++)
            {
                sum += grid.GetData(new Point(row, col)).Value;     
            }
        }

        return sum;
    }

    private void SetGridIntersectionsToZero(Grid grid)
    {
        var midWidth = grid.Width / 2;
        var midHeight = grid.Height / 2;

        for (var row = 0; row < grid.Height; row++)
        {
            grid.GetData(new Point(row, midWidth)).Value = 0;

            if (row != midHeight) continue;
            for (int col = 0; col < grid.Width; col++)
            {
                grid.GetData(new Point(row, col)).Value = 0;
            }
        }
    }
    
    private Point GetFullPositionAtTime(Robot robot, int seconds)
    {
        var row = robot.StartingLocation.Row + (robot.Velocity.Row * seconds);
        var col = robot.StartingLocation.Col + (robot.Velocity.Col * seconds);
        
        return new Point(row, col);
    }

    private Point GetPositionOnGrid(Grid grid, Point fullPosition)
    {
        var col = fullPosition.Col % grid.Width;   
        var row = fullPosition.Row % grid.Height;

        if (col < 0) col += grid.Width;
        if (row < 0) row += grid.Height;
        
        return new Point(row, col);
    }

    private Grid InitGrid(int width, int height)
    {
        var grid = new Grid();
        
        for (var i = 0; i < height; i++)
        {
            var line = new Cell[width];
            for (var j = 0; j < width; j++)
            {
                line[j] = new Cell
                {
                    Value = 0,
                    Location = new Point(i, j)
                };
            }
            grid.AddLine(line);
        }

        return grid;
    }
}

internal class PartTwoSolver(List<Robot> robots)
{
    internal long GetSecondsToTree(int gridWidth, int gridHeight)
    {
        var seconds = 0;
        var points = new List<Point>();
        
        while (true)
        {
            var grid = InitGrid(gridWidth, gridHeight);
            points.Clear();

            foreach (var robot in robots)
            {
                var fullPosition = GetFullPositionAtTime(robot, seconds);
                var position = GetPositionOnGrid(gridWidth, gridHeight, fullPosition);
                grid.Increment(position);
            }
            
            // it's a bit hacky but look for contiguous blocks of set data
            var rowsWithContiguousValues = grid.CountOfRowsWithContiguousValues(15);
            
            if (rowsWithContiguousValues > 0)
            {
                return seconds;
            }
            
            seconds++;
        }
    }
    
    private Point GetFullPositionAtTime(Robot robot, int seconds)
    {
        var row = robot.StartingLocation.Row + (robot.Velocity.Row * seconds);
        var col = robot.StartingLocation.Col + (robot.Velocity.Col * seconds);
        
        return new Point(row, col);
    }

    private Point GetPositionOnGrid(int gridWidth, int gridHeight, Point fullPosition)
    {
        var col = fullPosition.Col % gridWidth;   
        var row = fullPosition.Row % gridHeight;

        if (col < 0) col += gridWidth;
        if (row < 0) row += gridHeight;
        
        return new Point(row, col);
    }
    
    private Grid InitGrid(int width, int height)
    {
        var grid = new Grid();
        
        for (var i = 0; i < height; i++)
        {
            var line = new Cell[width];
            for (var j = 0; j < width; j++)
            {
                line[j] = new Cell
                {
                    Value = 0,
                    Location = new Point(i, j)
                };
            }
            grid.AddLine(line);
        }

        return grid;
    }
}


internal class Grid: ICloneable
{
    private readonly List<Cell[]> _data = [];
    
    internal int Width => _data[0].Length;
    internal int Height => _data.Count;
    
    public void AddLine(Cell[] line)
    {
        var row = _data.Count;
        for (var col = 0; col < line.Length; col++)
        {
            line[col].Location = new Point(row, col);
        }
        _data.Add(line);
    }
    
    public int CountOfRowsWithContiguousValues(int contiguousCount)
    {
        return _data.Count(row => CountContiguousValues(row.Select(x => x.Value).ToArray()) > contiguousCount);
    }

    private int CountContiguousValues(int[] array)
    {
        var maxCount = 0;
        var currentCount = 0;

        foreach (var num in array)
        {
            if (num != 0)
            {
                currentCount++;
                if (currentCount > maxCount)
                {
                    maxCount = currentCount;
                }
            }
            else
            {
                currentCount = 0;
            }
        }

        return maxCount;
    }

    public Cell GetData(Point point)
    {
        return _data[point.Row][point.Col];
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var row in _data)
        {
            foreach (var col in row)
            {
                sb.Append(col);
            }
            
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public object Clone()
    {
        var newGrid = new Grid();
        foreach (var row in _data)
        {
            var r = new Cell[row.Length];

            for (var i=0; i<row.Length; i++)
            {
                r[i] = new Cell
                {
                    Value = row[i].Value,
                    Location = row[i].Location,
                    //Region = row[i].Region
                };
            }
            
            newGrid.AddLine(r);
        }

        return newGrid;
    }

    public void Increment(Point position)
    {
        _data[position.Row][position.Col].Value++;
    }
}

internal class Cell
{
    public int Value { get; set; }
    
    public Point Location { get; set; }
    
    // public int Region { get; set; }

    public override string ToString()
    {
        return Value.ToString();
    }
}

public class Point(int row, int col)
{
    public int Col { get; } = col;
    public int Row { get; } = row;
    
    public static Point Empty = new Point(-1, -1);

    public override string ToString()
    {
        return $"({Row}, {Col})";
    }
}

internal class Robot
{
    public Point StartingLocation { get; set; }
    public Point Velocity { get; set; }
}

internal class DataReader(string path)
{
    public List<Robot> LoadFile()
    {
        var robots = new List<Robot>();
        
        var f = File.OpenText(path);
        while (f.ReadLine() is { } line)
        {
            if (line == "---")
                break;
            
            var parts = line.Split(" ");
            var startPos = parts[0].Replace("p=", "").Split(",");
            var velocity = parts[1].Replace("v=", "").Split(",");
            
            var robot = new Robot
            {
                StartingLocation = new Point(int.Parse(startPos[1]), int.Parse(startPos[0])),
                Velocity = new Point(int.Parse(velocity[1]), int.Parse(velocity[0]))
            };
            
            robots.Add(robot);

        }

        return robots;
    }
}