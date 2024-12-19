using System.Diagnostics;
Console.WriteLine("Starting Day 13");

// run with the test data
//var dataFile = "../../../input_test.txt";

// run with the main data
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var machinePrizes = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new PartOneSolver(machinePrizes);
var part1Result = partOneSolver.GetTokens();
Console.WriteLine($"Part one result: {part1Result} ({sw.ElapsedMilliseconds}ms)");

sw.Restart();
var partTwoSolver = new PartTwoSolver(machinePrizes);
var part2Result = partTwoSolver.GetTokens();
Console.WriteLine($"Part two result: {part2Result} ({sw.ElapsedMilliseconds}ms)");

internal class PartOneSolver(List<MachinePrize> machinePrizes)
{
    public long GetTokens()
    {
        return machinePrizes.Sum(SolveForMachine);
    }

    private long SolveForMachine(MachinePrize mp)
    {
        var dividend = mp.ButtonB.Y * mp.Prize.X - mp.ButtonB.X * mp.Prize.Y;
        var divisor = mp.ButtonB.Y * mp.ButtonA.X - mp.ButtonB.X * mp.ButtonA.Y;
        
        if (divisor == 0 || dividend % divisor != 0)
        {
            return 0;
        }

        var a = dividend / divisor;
        var b = (mp.Prize.X - mp.ButtonA.X * a) / mp.ButtonB.X;
        
        var costA = a * mp.ButtonACost;
        var costB = b * mp.ButtonBCost;

        return costA + costB;
    }
}

internal class PartTwoSolver(List<MachinePrize> machinePrizes)
{
    public long GetTokens()
    {
        // update the prizes!
        machinePrizes.ForEach(mp =>
        { 
            mp.Prize.X += 10_000_000_000_000;
            mp.Prize.Y += 10_000_000_000_000;
        });

        return machinePrizes.Sum(SolveForMachine);
    }

    private static long SolveForMachine(MachinePrize mp)
    {
        var dividend = mp.ButtonB.Y * mp.Prize.X - mp.ButtonB.X * mp.Prize.Y;
        var divisor = mp.ButtonB.Y * mp.ButtonA.X - mp.ButtonB.X * mp.ButtonA.Y;
        
        if (divisor == 0 || dividend % divisor != 0)
        {
            return 0;
        } 

        var a = (dividend) / (divisor);
        var b = ((mp.Prize.X - mp.ButtonA.X * a) / mp.ButtonB.X);
        
       // verify the solution
       var sol1 = mp.ButtonA.X * a + mp.ButtonB.X * b == mp.Prize.X;
       var sol2 = mp.ButtonA.Y * a + mp.ButtonB.Y * b == mp.Prize.Y;
       if (!sol1 || !sol2)
       {
           return 0;
       }
        
        var costA = a * mp.ButtonACost;
        var costB = b * mp.ButtonBCost;

        return costA + costB;
    }
}

internal class MachinePrize
{
    public Coord ButtonA { get; set; }
    public long ButtonACost { get; set; }
    public Coord ButtonB { get; set; }
    public long ButtonBCost { get; set; }
    public Coord Prize { get; set; }
}

internal class Coord
{
    public long X { get; set; }
    public long Y { get; set; }
}

internal class DataReader(string path)
{
    public List<MachinePrize> LoadFile()
    {
        var machinePrizes = new List<MachinePrize>();
        
        var f = File.OpenText(path);
        
        // read three lines at a time
        while (!f.EndOfStream)
        {
            var buttonA = f.ReadLine();
            var buttonB = f.ReadLine();
            var prize = f.ReadLine();
            f.ReadLine();

            var buttonACoords = buttonA.Split(":")[1].Split(',');
            var buttonBCoords = buttonB.Split(":")[1].Split(',');
            var prizeCoords = prize.Split(":")[1].Split(',');

            var buttonAX = long.Parse(buttonACoords[0].Trim().Replace("X", ""));
            var buttonAY = long.Parse(buttonACoords[1].Trim().Replace("Y", ""));
            var buttonBX = long.Parse(buttonBCoords[0].Trim().Replace("X", ""));
            var buttonBY = long.Parse(buttonBCoords[1].Trim().Replace("Y", ""));
            var prizeX = long.Parse(prizeCoords[0].Trim().Replace("X=", ""));
            var prizeY = long.Parse(prizeCoords[1].Trim().Replace("Y=", ""));
            
            var machinePrize = new MachinePrize
            {
                ButtonA = new Coord { X = buttonAX, Y = buttonAY },
                ButtonB = new Coord { X = buttonBX, Y = buttonBY },
                Prize = new Coord { X = prizeX, Y = prizeY },
                ButtonACost = 3,
                ButtonBCost = 1
            };

            machinePrizes.Add(machinePrize);
        }

        return machinePrizes;
    }
}