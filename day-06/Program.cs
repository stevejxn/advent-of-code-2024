using System.Diagnostics;

Console.WriteLine("Starting Day 06");

// run with the test data
//var dataFile = "../../../input_test.txt";

// run with puzzle input
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var grid = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new PartOneSolver((Grid)grid.Clone());
var resultPart1 = partOneSolver.GetDistinctPositions();
Console.WriteLine($"Part one result: {resultPart1} ({sw.ElapsedMilliseconds}ms)");

sw.Restart();
var partTwoSolver = new PartTwoSolver((Grid)grid.Clone());
var resultPart2 = partTwoSolver.GetCountOfLoopCausingBlocks();
Console.WriteLine($"Part two result: {resultPart2} ({sw.ElapsedMilliseconds}ms)");

internal class PartOneSolver(Grid grid)
{
    public long GetDistinctPositions()
    {
        var navigator = new GridNavigator(grid);
        navigator.PatrolUntilDone();
        
        return grid.CountEntries('X');
    }
}

internal class PartTwoSolver(Grid grid)
{
    public long GetCountOfLoopCausingBlocks()
    {
        var loopCausingBlocks = 0;
        
        for (var row = 0; row < grid.Height; row++)
        {
            for (var col = 0; col < grid.Width; col++)
            {
                var testGrid = (Grid)grid.Clone();
                
                // skip if not empty
                if (testGrid.GetData(new Point(row, col)) != '.')
                    continue;
                
                // try adding a block
                testGrid.SetData(new Point(row, col), '#');
                    
                // try to patrol
                var navigator = new GridNavigator(testGrid);
                var result = navigator.PatrolUntilDone();
                    
                if (result == PatrolResult.LoopDetected)
                {
                    loopCausingBlocks++;
                }
            }
        }

        return loopCausingBlocks;
    }
}

internal enum Direction
{
    Up,
    Down,
    Left,
    Right
}
    
enum PatrolResult
{
    OutOfBounds,
    LoopDetected,
}

internal class GridNavigator
{
    private readonly Grid _grid;
    private Direction _currentDirection;

    private Point CurrentPos { get; set; }
    
    public GridNavigator(Grid grid)
    {
        _grid = grid;
        CurrentPos = grid.GetPositionOf('^');
        _currentDirection = Direction.Up;
        _grid.SetData(CurrentPos, 'X');
    }

    public PatrolResult PatrolUntilDone()
    {
        int turnsSinceLastX = 0;
        
        while (true)
        {
            // move in the current direction
            var nextPos = GetNextPosition(_currentDirection);

            if (_grid.IsOutOfBounds(nextPos))
            {
                return PatrolResult.OutOfBounds;
            }

            if (_grid.GetData(nextPos) == '#')
            {
                // hit a block, turn right
                _currentDirection = TurnRight();
                turnsSinceLastX++;
                
                if (turnsSinceLastX >= 4)
                {
                    return PatrolResult.LoopDetected;
                }
                
                continue;
            }

            CurrentPos = nextPos;
            if (_grid.GetData(CurrentPos) != 'X')
            {
                _grid.SetData(CurrentPos, 'X');
                turnsSinceLastX = 0;
            }
        }
    }

    private Direction TurnRight()
    {
        return _currentDirection switch
        {
            Direction.Up => Direction.Right,
            Direction.Right => Direction.Down,
            Direction.Down => Direction.Left,
            Direction.Left => Direction.Up,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private Point GetNextPosition(Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Point(CurrentPos.Row - 1, CurrentPos.Col),
            Direction.Down => new Point(CurrentPos.Row + 1, CurrentPos.Col),
            Direction.Left => new Point(CurrentPos.Row, CurrentPos.Col - 1),
            Direction.Right => new Point(CurrentPos.Row, CurrentPos.Col + 1),
            _ => Point.Empty
        };
    }
}


internal class Grid: ICloneable
{
    private readonly List<char[]> _data = [];
    
    public void AddLine(char[] line)
    {
        _data.Add(line);
    }
    
    public void SetData(Point point, char value)
    {
        _data[point.Row][point.Col] = value;
    }
    
    public char GetData(Point point)
    {
        return _data[point.Row][point.Col];
    }
    
    public Point GetPositionOf(char value)
    {
        for (var row = 0; row < _data.Count; row++)
        {
            var pos = Array.IndexOf(_data[row], value);
            if (pos != -1)
            {
                return new Point(row, pos);
            }
        }
        
        return Point.Empty;
    }
    
    public int CountEntries(char value)
    {
        return _data.Sum(row => row.Count(c => c == value));
    }
    
    public int Width => _data[0].Length;
    public int Height => _data.Count;
    
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