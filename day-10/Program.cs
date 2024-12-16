using System.Diagnostics;
Console.WriteLine("Starting Day 10");

// run with the test data
//var dataFile = "../../../input_test.txt";

// run with the main data
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var grid = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new PartOneSolver(grid);
var part1Result = partOneSolver.Solve();
Console.WriteLine($"Part one result: {part1Result} ({sw.ElapsedMilliseconds}ms)");

sw.Restart();
var partTwoSolver = new PartTwoSolver(grid);
var part2Result = partTwoSolver.Solve();
Console.WriteLine($"Part two result: {part2Result} ({sw.ElapsedMilliseconds}ms)");

internal class PartOneSolver(Grid grid)
{
    internal long Solve()
    {
        var result = 0;
        
        var trailheads = grid.FindAllPositionsOf('0');

        foreach (var trailHead in trailheads)
        {
            var start = new TrailStep(trailHead, 0);
            var tree = new TreeNode<TrailStep>(start, null);
            PopulateTree(tree, 1, [], 9);
        
            // traverse the tree to find the number of end points with the value of 9
            var count = FindCountWithValue(tree, 9);
            result += count;    
        }
        
        return result;
    }

    private void PopulateTree(TreeNode<TrailStep> node, int nextStep, List<string> routes, int maxStep)
    {
        var stepChar = nextStep.ToString()[0];
        var surrounding = grid.FindSurrounding(node.Data.Location, stepChar);

        foreach (var point in surrounding)
        {
            var path = $"{point.Row}{point.Col}";
            
            if (routes.Contains(path))
            {
                continue;
            }

            var step = new TrailStep(point, (int)char.GetNumericValue(stepChar));
            var child = new TreeNode<TrailStep>(step, node);
            node.Children.Add(child);
            
            routes.Add(path);

            var next = nextStep + 1;
            if (next <= maxStep)
            {
                PopulateTree(child, next, routes, maxStep);
            }
        }
    }

    private int FindCountWithValue(TreeNode<TrailStep> root, int value)
    {
        var count = 0;
        Queue<TreeNode<TrailStep>> queue = new();
        queue.Enqueue(root);
        while (queue.Count > 0) 
        {
            var current = queue.Dequeue();
            if (current.Data.Value == value)
            {
                count++;
            }
            
            foreach (var child in current.Children) 
            {
                queue.Enqueue(child);
            }
        }

        return count;
    }
}

internal class PartTwoSolver(Grid grid)
{
    internal long Solve()
    {
        var result = 0;
        
        var trailheads = grid.FindAllPositionsOf('0');

        foreach (var trailHead in trailheads)
        {
            var start = new TrailStep(trailHead, 0);
            var tree = new TreeNode<TrailStep>(start, null);
            PopulateTree(tree, 1, 9);
        
            // traverse the tree to find the number of end points with the value of 9
            var count = FindCountWithValue(tree, 9);
            result += count;    
        }
        
        return result;
    }

    private void PopulateTree(TreeNode<TrailStep> node, int nextStep, int maxStep)
    {
        var stepChar = nextStep.ToString()[0];
        var surrounding = grid.FindSurrounding(node.Data.Location, stepChar);

        foreach (var point in surrounding)
        {
            var step = new TrailStep(point, (int)char.GetNumericValue(stepChar));
            var child = new TreeNode<TrailStep>(step, node);
            node.Children.Add(child);

            var next = nextStep + 1;
            if (next <= maxStep)
            {
                PopulateTree(child, next, maxStep);
            }
        }
    }

    private int FindCountWithValue(TreeNode<TrailStep> root, int value)
    {
        var count = 0;
        Queue<TreeNode<TrailStep>> queue = new();
        queue.Enqueue(root);
        while (queue.Count > 0) 
        {
            var current = queue.Dequeue();
            if (current.Data.Value == value)
            {
                count++;
            }
            
            foreach (var child in current.Children) 
            {
                queue.Enqueue(child);
            }
        }

        return count;
    }
}


internal class TrailStep
{
    public TrailStep(Point location, int value)
    {
        this.Location = location;
        this.Value = value;
    }
    
    public Point Location { get; set; }
    public int Value { get; set; }
}

class TreeNode<T>
{
    public T Data { get; set; }
    public TreeNode<T> Parent { get; set; }
    public List<TreeNode<T>> Children { get; } = new List<TreeNode<T>>();

    public TreeNode(T data, TreeNode<T>? parent)
    {
        Data = data;
        Parent = parent;
    }
}

internal class Grid: ICloneable
{
    private readonly List<char[]> _data = [];
    
    public void AddLine(char[] line)
    {
        _data.Add(line);
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

    // todo - add flags enum specifying up, down, left, right, upleft, upright, downleft, downright
    public List<Point> FindSurrounding(Point point, char value)
    {
        // given location, check up, down, left, right for value
        // ensuring we don't go out of bounds
        var results = new List<Point>();
        
        var up = new Point(point.Row - 1, point.Col);
        if (!IsOutOfBounds(up) && GetData(up) == value)
        {
            results.Add(up);
        }
        
        var down = new Point(point.Row + 1, point.Col);
        if (!IsOutOfBounds(down) && GetData(down) == value)
        {
            results.Add(down);
        }
        
        var left = new Point(point.Row, point.Col - 1);
        if (!IsOutOfBounds(left) && GetData(left) == value)
        {
            results.Add(left);
        }
        
        var right = new Point(point.Row, point.Col + 1);
        if (!IsOutOfBounds(right) && GetData(right) == value)
        {
            results.Add(right);
        }
        
        return results;
    }
    
    public void SetData(Point point, char value)
    {
        _data[point.Row][point.Col] = value;
    }
    
    public char GetData(Point point)
    {
        return _data[point.Row][point.Col];
    }

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