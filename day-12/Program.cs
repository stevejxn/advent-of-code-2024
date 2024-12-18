using System.Diagnostics;
using System.Security.Principal;

Console.WriteLine("Starting Day 12");

// run with the test data
// var dataFile = "../../../input_test_01.txt";
// var dataFile = "../../../input_test_02.txt";
//var dataFile = "../../../input_test_03.txt";
// var dataFile = "../../../input_test_04.txt";
//var dataFile = "../../../input_test_05.txt";

// run with the main data
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var grid = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new PartOneSolver((Grid)grid.Clone());
var part1Result = partOneSolver.GetFencingCost();
Console.WriteLine($"Part one result: {part1Result} ({sw.ElapsedMilliseconds}ms)");

sw.Restart();
var partTwoSolver = new PartTwoSolver((Grid)grid.Clone());
var part2Result = partTwoSolver.GetFencingCost();
Console.WriteLine($"Part two result: {part2Result} ({sw.ElapsedMilliseconds}ms)");

internal class PartOneSolver(Grid grid)
{
    internal long GetFencingCost()
    {
        var costs = new List<FenceInfo>();
        
        var plantsTypes = grid.FindUniqueChars();
        Console.WriteLine($"Plant types: {string.Join(", ", plantsTypes)}");

        grid.AllocateRegions();
        
        foreach (var plantType in plantsTypes)
        {
            var positions = grid.FindAllCellsOfValue(plantType);
            var perimeterGroups = positions.GroupBy(c => c.Region);

            foreach (var perimeterGroup in perimeterGroups)
            {
                var area = perimeterGroup.Count();

                var perimeter = perimeterGroup
                    .Select(position => grid.FindSurroundingNonMatching(position.Location))
                    .Select(adjacentNonMatching => adjacentNonMatching.Count)
                    .Sum();
                
                costs.Add(new FenceInfo(plantType, perimeterGroup.Key, area, perimeter));
            }
        }
        
        return costs.Sum(fi => fi.area * fi.perimeter);
    }
}

internal class PartTwoSolver(Grid grid)
{
    internal long GetFencingCost()
    {
        var costs = new List<FenceInfo>();
        
        var plantsTypes = grid.FindUniqueChars();
        Console.WriteLine($"Plant types: {string.Join(", ", plantsTypes)}");

        var regions = grid.AllocateRegions();

        foreach (var (region, cells) in regions)
        {
            var sides = GetNumberOfCorners(cells);
            costs.Add(new FenceInfo(cells.First().Value, region, cells.Count, sides));
        }
     
        return costs.Sum(fi => fi.area * fi.perimeter);
    }

    private int GetNumberOfCorners(List<Cell> cells)
    {
        var corners = 0;
        
        // count the number of corners in the region and this
        // will give the number of sides
        foreach (var cell in cells)
        {
            var col = cell.Location.Col;
            var row = cell.Location.Row;
            
            var up = new Point(row - 1, col);
            var down = new Point(row + 1, col);
            var left = new Point(row, col - 1);
            var right = new Point(row, col + 1);
            var upperLeft = new Point(row - 1, col - 1);
            var upperRight = new Point(row - 1, col + 1);
            var lowerLeft = new Point(row + 1, col - 1);
            var lowerRight = new Point(row + 1, col + 1);
            
            // external corners
            if ((grid.IsOutOfBounds(up) || grid.GetData(up).Value != cell.Value)
                && (grid.IsOutOfBounds(left) || grid.GetData(left).Value != cell.Value))
            {
                corners++;
            }
            
            if ((grid.IsOutOfBounds(up) || grid.GetData(up).Value != cell.Value)
                && (grid.IsOutOfBounds(right) || grid.GetData(right).Value != cell.Value))
            {
                corners++;
            }
            
            if ((grid.IsOutOfBounds(down) || grid.GetData(down).Value != cell.Value)
                && (grid.IsOutOfBounds(left) || grid.GetData(left).Value != cell.Value))
            {
                corners++;
            }

            if ((grid.IsOutOfBounds(down) || grid.GetData(down).Value != cell.Value)
                && (grid.IsOutOfBounds(right) || grid.GetData(right).Value != cell.Value))
            {
                corners++;
            }

            // internal corners
            if (!grid.IsOutOfBounds(up) && grid.GetData(up).Value == cell.Value 
                && !grid.IsOutOfBounds(left) && grid.GetData(left).Value == cell.Value 
                && !grid.IsOutOfBounds(upperLeft) && grid.GetData(upperLeft).Value != cell.Value)
            {
                corners++;
            }
            
            if (!grid.IsOutOfBounds(up) && grid.GetData(up).Value == cell.Value 
                && !grid.IsOutOfBounds(right) && grid.GetData(right).Value == cell.Value 
                && !grid.IsOutOfBounds(upperRight) && grid.GetData(upperRight).Value != cell.Value)
            {
                corners++;
            }
            
            if (!grid.IsOutOfBounds(down) && grid.GetData(down).Value == cell.Value 
                && !grid.IsOutOfBounds(left) && grid.GetData(left).Value == cell.Value 
                && !grid.IsOutOfBounds(lowerLeft) && grid.GetData(lowerLeft).Value != cell.Value)
            {
                corners++;
            }

            if (!grid.IsOutOfBounds(down) && grid.GetData(down).Value == cell.Value 
                && !grid.IsOutOfBounds(right) && grid.GetData(right).Value == cell.Value 
                && !grid.IsOutOfBounds(lowerRight) && grid.GetData(lowerRight).Value != cell.Value)
            {
                corners++;
            }
        }

        return corners;
    }
}

internal record FenceInfo(char plant, int region, int area, int perimeter)
{
    public override string ToString()
    {
        return $"{plant} - {region} - {area} - {perimeter}";
    }
}

internal class Grid: ICloneable
{
    private readonly List<Cell[]> _data = [];
    
    public void AddLine(Cell[] line)
    {
        var row = _data.Count;
        for (var col = 0; col < line.Length; col++)
        {
            line[col].Location = new Point(row, col);
        }
        _data.Add(line);
    }
    
    public char[] FindUniqueChars(params char[] ignore)
    {
        return _data
            .SelectMany(row => row)
            .Where(x => !ignore.Contains(x.Value))
            .GroupBy(x => x.Value)
            .Where(x => x.Any())
            .Select(x => x.Key)
            .ToArray();        
    }

    public Dictionary<int, List<Cell>> AllocateRegions()
    {
        // find first cell without a region
        // then walk all connected cells of the same value changing their region to the current region
        // when done, increment the region and repeat
        Dictionary<int, List<Cell>> regions = [];
        
        Cell? cell;
        var region = 0;
        while ((cell = FindFirstEmptyRegion()) is not null)
        {
            region++;
            cell.Region = region;
            var cells = FindConnectedCellsAndAllocateRegion(cell);
            cells.Insert(0, cell);
            regions[region] = cells;    
        }

        return regions;
    }

    private List<Cell> FindConnectedCellsAndAllocateRegion(Cell cell)
    {
        var cells = new List<Cell>();

        var toCheck = new Queue<Cell>();
        toCheck.Enqueue(cell);
        
        while (toCheck.Count > 0)
        {
            var current = toCheck.Dequeue();
            var surrounding = FindSurroundingMatching(current);
            foreach (var s in surrounding.Where(s => s.Region == -1))
            {
                s.Region = current.Region;
                cells.Add(s);
                toCheck.Enqueue(s);
            }
        }

        return cells;
    }

    private Cell? FindFirstEmptyRegion()
    {
        for (var row = 0; row < _data.Count; row++)
        {
            for (var col = 0; col < _data[row].Length; col++)
            {
                if (_data[row][col].Region == -1)
                {
                    return _data[row][col];
                }
            }
        }
        
        return null;
    }
    
    public List<Cell> FindAllCellsOfValue(char value)
    {
        var cells = new List<Cell>();
        
        for (var row = 0; row < _data.Count; row++)
        {
            for (var col = 0; col < _data[row].Length; col++)
            {
                if (_data[row][col].Value == value)
                {
                    cells.Add(_data[row][col]);
                }
            }
        }
        
        return cells;
    }

    private List<Cell> FindSurroundingMatching(Cell cell)
    {
        var results = new List<Cell>();
        
        var up = new Point(cell.Location.Row - 1, cell.Location.Col);
        if (!IsOutOfBounds(up) && GetData(up).Value == cell.Value)
        {
            results.Add(GetData(up));
        }
        
        var down = new Point(cell.Location.Row + 1, cell.Location.Col);
        if (!IsOutOfBounds(down) && GetData(down).Value == cell.Value)
        {
            results.Add(GetData(down));
        }
        
        var left = new Point(cell.Location.Row, cell.Location.Col - 1);
        if (!IsOutOfBounds(left) && GetData(left).Value == cell.Value)
        {
            results.Add(GetData(left));
        }
        
        var right = new Point(cell.Location.Row, cell.Location.Col + 1);
        if (!IsOutOfBounds(right) && GetData(right).Value == cell.Value)
        {
            results.Add(GetData(right));
        }

        return results;
    }

    public List<Point> FindSurroundingNonMatching(Point point)
    {
        var results = new List<Point>();
        var cell = GetData(point);
        
        var up = new Point(point.Row - 1, point.Col);
        if (IsOutOfBounds(up) || GetData(up).Value != cell.Value)
        {
            results.Add(up);
        }
        
        var down = new Point(point.Row + 1, point.Col);
        if (IsOutOfBounds(down) || GetData(down).Value != cell.Value)
        {
            results.Add(down);
        }
        
        var left = new Point(point.Row, point.Col - 1);
        if (IsOutOfBounds(left) || GetData(left).Value != cell.Value)
        {
            results.Add(left);
        }
        
        var right = new Point(point.Row, point.Col + 1);
        if (IsOutOfBounds(right) || GetData(right).Value != cell.Value)
        {
            results.Add(right);
        }

        return results;
    }

    public Cell GetData(Point point)
    {
        return _data[point.Row][point.Col];
    }

    public bool IsOutOfBounds(Point point)
    {
        return point.Row < 0 || point.Row >= _data.Count ||  point.Col < 0 || point.Col >= _data[point.Row].Length;
    }
    
    public override string ToString()
    {
        return string.Join("\n", _data.Select(row => row.Select(r => r)));
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
                    Region = row[i].Region
                };
            }
            
            newGrid.AddLine(r);
        }

        return newGrid;
    }
}

internal class Cell
{
    public char Value { get; set; }
    
    public Point Location { get; set; }
    
    public int Region { get; set; }
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

internal class DataReader(string path)
{
    public Grid LoadFile()
    {
        var grid = new Grid();
        
        var f = File.OpenText(path);
        while (f.ReadLine() is { } line)
        {
            grid.AddLine(line.ToCharArray().Select(c => new Cell()
                { Value = c, Region = -1 }
            ).ToArray());
        }

        return grid;
    }
}