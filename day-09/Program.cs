using System.Diagnostics;

Console.WriteLine("Starting Day 09");

// run with the test data
//var dataFile = "../../../input_test.txt";

// run with the main data
var dataFile = "../../../input_main.txt";

var reader = new DataReader(dataFile);
var diskMap = reader.LoadFile();

var sw = new Stopwatch();
sw.Start();
var partOneSolver = new PartOneSolver();
var part1Result = partOneSolver.Solve(diskMap);
Console.WriteLine($"Part one result: {part1Result} ({sw.ElapsedMilliseconds}ms)");

sw.Restart();
var partTwoSolver = new PartTwoSolver();
var part2Result = partTwoSolver.Solve(diskMap);
Console.WriteLine($"Part two result: {part2Result} ({sw.ElapsedMilliseconds}ms)");

internal class PartOneSolver
{
    public long Solve(DiskMap diskMap)
    {
        var elements = diskMap.ToFileLayout();
        var blocks = ExpandToBlocks(elements);
        var defragedData = Defrag(blocks);
        var checksum  = CalculateChecksum(defragedData);

        return checksum;
    }
    
    private List<FileBlock> ExpandToBlocks(List<(int, int)> elements)
    {
        var blocks = new List<FileBlock>();

        for (var el =0; el < elements.Count; el++)
        {   
            var (blockSize, freeSpace) = elements[el];
           
            for (var i =0 ; i < blockSize; i++)
            {
                blocks.Add(new FileBlock(el));
            }
            
            for (var i = 0; i < freeSpace; i++)
            {
                blocks.Add(new FileBlock());
            }
        }

        return blocks;
    }
    private List<FileBlock> Defrag(List<FileBlock> fileBlocks)
    {
        var defragged = new List<FileBlock>();

        while (fileBlocks.Count > 0)
        {
            var fb = fileBlocks[0];
            if (!fb.IsEmpty)
            {
                defragged.Add(fb);
            }
            else
            {
                var indexOfLast = IndexOfLastData(fileBlocks);
                if (indexOfLast == -1)
                {
                    break;
                }
                defragged.Add(fileBlocks[indexOfLast]);
                fileBlocks.RemoveAt(indexOfLast);
            }

            fileBlocks.RemoveAt(0);
        }
        
        return defragged;
    }
    
    private int IndexOfLastData(List<FileBlock> data)
    {
        for (var i=data.Count-1; i >= 0; i--)
        {
            if (!data[i].IsEmpty)
            {
                return i;
            }
        }

        return -1;
    }

    private long CalculateChecksum(List<FileBlock> fileBlocks)
    {
        var sum = 0L;
        for (var i = 0; i < fileBlocks.Count; i++)
        {
            sum += i * fileBlocks[i].FileId;
        }

        return sum;
    }

    private class FileBlock
    {
        public FileBlock(long fileId)
        {
            this.FileId = fileId;
            this.IsEmpty = false;
        }

        public FileBlock()
        {
            this.IsEmpty = true;
        }
    
        public long FileId { get;  } 
    
        public bool IsEmpty { get; }
    }
}

internal class PartTwoSolver()
{
     public long Solve(DiskMap diskMap)
    {
        var elements = diskMap.ToFileLayout();
        var blocks = ExpandToBlocks(elements);
        var defraggedData = Defrag(blocks);

        var visual = String.Join("", defraggedData.Select(x => x.IsEmpty ? "." : x.FileId.ToString()));
//        Console.WriteLine(visual);
        
        var checksum  = CalculateChecksum(defraggedData);

        return checksum;
    }
    
    private List<FileBlock> ExpandToBlocks(List<(int, int)> elements)
    {
        var blocks = new List<FileBlock>();

        for (var el =0; el < elements.Count; el++)
        {   
            var (blockSize, freeSpace) = elements[el];
           
            for (var i =0 ; i < blockSize; i++)
            {
                blocks.Add(new FileBlock(el, blockSize));
            }
            
            for (var i = 0; i < freeSpace; i++)
            {
                blocks.Add(new FileBlock());
            }
        }

        return blocks;
    }
    private List<FileBlock> Defrag(List<FileBlock> fileBlocks)
    {
        // this is awful, but it works
        // lesson for next time - should have used a linked list
        
        var byFileId = fileBlocks
            .Where(fb => !fb.IsEmpty)
            .GroupBy(fb => fb.FileId)
            .Reverse();
        
        foreach (var fb in byFileId)
        {
            var blockSize = fb.First().BlockSize;
            var searchTo = fileBlocks.FindIndex(x => x.Guid == fb.First().Guid);
            
            // find a location where this will fit
            var emptyBlockSize = 0;
            for (var i = 0; i < searchTo; i++)
            {
                if (!fileBlocks[i].IsEmpty)
                {
                    emptyBlockSize = 0;
                    continue;
                }
                
                emptyBlockSize++;

                if (emptyBlockSize < blockSize) 
                    continue;
                
                // can move here
                var insertAt = i - emptyBlockSize + 1;
                var copyFrom = 0;
                for (var j = insertAt; j < insertAt + blockSize; j++)
                {
                    var blockToMove = fb.ElementAt(copyFrom);
                    var blockLocation = fileBlocks.FindIndex(fb => fb.Guid == blockToMove.Guid);
                       
                    if (blockLocation == -1)
                    {
                        throw new Exception("Block not found");
                    }
                        
                    fileBlocks[j] = blockToMove;
                    fileBlocks.RemoveAt(blockLocation);
                    fileBlocks.Insert(blockLocation, new FileBlock());
                    copyFrom++;
                }
                    
                break;
            }
        }

        return fileBlocks;
    }

    private long CalculateChecksum(List<FileBlock> fileBlocks)
    {
        var sum = 0L;
        for (var i = 0; i < fileBlocks.Count; i++)
        {
            if (fileBlocks[i].IsEmpty)
            {
                continue;
            }
            
            sum += i * fileBlocks[i].FileId;
        }

        return sum;
    }

    private class FileBlock
    {
        public FileBlock(long fileId, int blockSize)
        {
            this.FileId = fileId;
            this.BlockSize = blockSize;
            this.IsEmpty = false;
            this.Guid = Guid.NewGuid();
        }

        public FileBlock()
        {
            this.IsEmpty = true;
        }
    
        public long FileId { get;  }
        public int BlockSize { get; }

        public bool IsEmpty { get; }
        public Guid Guid { get; }
    }
}

internal class DiskMap(string input)
{
    public List<(int, int)> ToFileLayout()
    {
        return GetPairs(input.ToCharArray())
            .Select((x, y) => ((int)char.GetNumericValue(x.Item1), (int)char.GetNumericValue(x.Item2)))
            .ToList();
    }

    private static List<(char, char)> GetPairs(char[] chars)
    {
        var working = new List<char>(chars);
        if (chars.Length % 2 != 0)
        {
            working.Add('0');
        }

        var pairs = new List<(char, char)>();

        for (var i = 0; i < working.Count - 1; i += 2)
        {
            pairs.Add((working[i], working[i + 1]));
        }

        return pairs;
    }
}

internal class DataReader(string path)
{
    public DiskMap LoadFile()
    {
        var data = File.ReadAllText(path);
        var diskMap = new DiskMap(data);

        return diskMap;
    }
}