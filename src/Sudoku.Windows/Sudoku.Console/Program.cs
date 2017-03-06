namespace Sudoku.Console
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Diagnostics;
  using System.Globalization;
  using System.IO;
  using System.Text;
  using Core;
  using static System.Console;
  
  class Program
  {
    static void Main(string[] args)
    {
      TaskType task;
      int maxSolutions;
      List<string> boards;
      try
      {
        task = ParseArgs(args, out maxSolutions, out boards);
      }
      catch
      {
        PrintUsage();
        return;
      }

      try
      {
        switch (task)
        {
          case TaskType.None:
            PrintUsage();
            break;
          case TaskType.Benchmark:
            Benchmark();
            break;
          case TaskType.SolveFromCli:
            Solve(boards, maxSolutions, prettyPrint: false);
            break;
          case TaskType.SolveFromCli | TaskType.PrettyPrint:
            Solve(boards, maxSolutions, prettyPrint: true);
            break;
          case TaskType.SolveFromFile:
            Solve(boards, maxSolutions, prettyPrint: false);
            break;
          case TaskType.SolveFromFile | TaskType.PrettyPrint:
            Solve(boards, maxSolutions, prettyPrint: true);
            break;
          case TaskType.Create: // start and go on a vacation
            throw new NotImplementedException("Board generation not supported yet.");
            CreateRandom();
            break;

          default:
            PrintUsage();
            break;
        }

      }
      catch (Exception e)
      {
        Write(e);
      }
    }

    private static void PrintUsage()
    {
      var sb = new StringBuilder();
      sb.AppendLine();
      sb.AppendLine("\t-r --raw \t-> To solve board from command line.");
      sb.AppendLine("\t\t\teg. -r=1.......9..67...2..8....4......75.3...5..2....6.3......9....8..6...4...1..25...6.");
      sb.AppendLine("\t\t\tor -r=800600905000000000000020310007318060240000073000000000002790100500080036003000000");
      sb.AppendLine();
      sb.AppendLine("\t-x --max \t-> Specifies maximum number of solutions to look for before exit");
      sb.AppendLine("\t\t\teg. -x=2");
      sb.AppendLine();
      sb.AppendLine("\t-p --pretty \t-> Prints pretty solutions");
      sb.AppendLine();
      sb.AppendLine("\t-f --file \t-> Solves sudokus from file.");
      sb.AppendLine("\t\t\teg. -f=foo.bar or -file=c:\\deadbeef");
      sb.AppendLine("\t\t\tEach board must be on separate line and in the same format as needed for --raw");
      sb.AppendLine();
      sb.AppendLine("\t-b --benchmark \t-> To run sample benchmark containing 95 boards");

      Write(sb);
    }

    private static TaskType ParseArgs(string[] args, out int maxSolutions, out List<string> board)
    {
      maxSolutions = 0;
      board = new List<string>();
      TaskType result = TaskType.None;

      if (args.Length == 0)
        return TaskType.None;

      if (args.Contains("-b") || args.Contains("--benchmark"))
        return TaskType.Benchmark;

      foreach (var arg in args)
      {
        if (arg.StartsWith("-r") || arg.StartsWith("--raw"))
        {
          var startIdx = arg.IndexOf('=');
          if (arg.Length < startIdx + 1)
            return TaskType.None;

          var res = arg.Substring(startIdx + 1);
          if (res.Length != 81)
            return TaskType.None;

          board.Add(res);
          result |= TaskType.SolveFromCli;
        }

        if (arg.StartsWith("-x") || arg.StartsWith("--max"))
        {
          var startIdx = arg.IndexOf('=');
          if (arg.Length < startIdx + 1)
            return TaskType.None;

          var numSolutionsString = arg.Substring(startIdx + 1);
          var parseSuccess = Int32.TryParse(numSolutionsString, NumberStyles.Integer, new NumberFormatInfo(),
                                       out maxSolutions);

          if (!parseSuccess)
            return TaskType.None;
        }

        if (arg.StartsWith("-f") || arg.StartsWith("--file"))
        {
          var startIdx = arg.IndexOf('=');
          if (arg.Length < startIdx + 1)
            return TaskType.None;

          var filePath = arg.Substring(startIdx + 1);

          if (! TryParseBoardsFromFile(filePath, out board))
          {
            WriteLine($"Invalid file({filePath})");
            return TaskType.None;
          }
          result &= ~TaskType.SolveFromCli;
          result |= TaskType.SolveFromFile;
        }


        if (arg.StartsWith("-p") || arg.StartsWith("--pretty"))
        {
          result |= TaskType.PrettyPrint;
        }
      }

      return result;
    }

    private static bool TryParseBoardsFromFile(string filePath, out List<string> boardsList)
    {
      boardsList = new List<string>();
      try
      {
        const Int32 bufferSize = 4096;
        using (var fileStream = File.OpenRead(filePath))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, bufferSize))
        {
          String line;
          while ((line = streamReader.ReadLine()) != null)
            boardsList.Add(line.Trim());
        }
      }
      catch
      {
        return false;
      }
      return true;
    }

    private static void CreateRandom()
    {
      // todo - board generation WIP
      var boardsToCreate = 100;
      var maxCluesToAccept = 80;
      var solver = new SudokuSolver();
      var writer = new SudokuWriter();
      var sw = new Stopwatch();
      var messages = new List<string>();

      var solverResults = new List<SolverResult>();
      Write("How many boards to create?: ");
      WriteLine("\b");

      while (true)
      {
        var line = ReadLine();
        int num = 0;
        if (Int32.TryParse(line, out num))
        {
          boardsToCreate = num;
          break;
        }
      }

      Write("Maximum clues to accept?[17 - 80]: ");
      while (true)
      {
        var line = ReadLine();
        int num = 0;
        if (Int32.TryParse(line, out num))
        {
          if (num > 16 && num < 81)
          {
            maxCluesToAccept = num;
            break;
          }
        }
      }

      Clear();
      
      WriteLine($"Boards to create: {boardsToCreate}");
      WriteLine($"Maximum clues to accept: {maxCluesToAccept}");
      WriteLine("Starting...\n\n");

      while (solverResults.Count < boardsToCreate)
      {
        Beep(3000,50); // it takes so long that I put this here
        sw.Start();
        var workingBoard = solver.CreateRandomBoard();
        sw.Stop();
        if (workingBoard.Item1.Cells.Where(c => c.HasValue).Count() > maxCluesToAccept)
        {
          sw.Reset();
          continue;
        }
        WriteLine(writer.GetStringRepresentation(workingBoard.Item1));
        Write('\a');
        solverResults.Add(new SolverResult()
                          {
                            TotalDurationMs = sw.Elapsed.TotalMilliseconds,
                            OriginalBoard = workingBoard.Item1,
                            SolutionList = new List<Board>() { workingBoard.Item2 }
                          });
        sw.Reset();
      }

      messages.Add($"Total time it took to create {boardsToCreate} boards is {solverResults.Aggregate(0.0, (t, i) => t + i.TotalDurationMs)}");
      messages.Add($"Avg. {solverResults.Aggregate(0.0, (t, i) => t + i.TotalDurationMs) / solverResults.Count}ms per board.\n");
      
      foreach (var result in solverResults)
      {
        messages.Add($"Created in {result.TotalDurationMs}ms, total cues: {result.OriginalBoard.Cells.Where(c => c.HasValue).Count()}");
        messages.Add($"{writer.GetStringRepresentation(result.OriginalBoard)}");
        messages.Add($"{writer.GetStringRepresentation(result.SolutionList.Single())}");
      }

      messages.ForEach(WriteLine);
    }

    private static void Benchmark()
    {
      Solve(GetSomePuzzles(), 0, false);
    }

    private static void Solve(List<string> boards, int maxSolutions, bool prettyPrint)
    {
      maxSolutions = Math.Abs(maxSolutions);

      var parser = new SudokuParser();
      var solver = new SudokuSolver();

      var boardsToSolve = new List<SolverResult>();
      foreach (var b in boards)
      {
        byte[] rawBoard;
        if (!parser.TryParse9X9Board(b, out rawBoard))
        {
          WriteLine($"Failed to parse {b}");
          continue;
        }

        var board = solver.CreateBoard(rawBoard);
        boardsToSolve.Add(new SolverResult() {OriginalBoard = board});
      }

      var sw = new Stopwatch();

      WriteLine("Solving started...");

      GC.Collect();
      GC.WaitForPendingFinalizers();
      GC.Collect();

      for (var i = 0; i < boardsToSolve.Count; ++i)
      {
        sw.Start();
        boardsToSolve[i].SolutionList = solver.FindAllSolutions(boardsToSolve[i].OriginalBoard, maxSolutions);
        sw.Stop();
        boardsToSolve[i].TotalDurationMs = sw.Elapsed.TotalMilliseconds;
        sw.Reset();
      }

      var messages = new List<string>();

      var totalSolutionCount = boardsToSolve.Aggregate(0, (total, item) => total + item.SolutionList.Count);
      var totalSolvingTime = boardsToSolve.Aggregate(0.0, (total, item) => total + item.TotalDurationMs);

      messages.Add($"Finding {totalSolutionCount} solution for {boardsToSolve.Count} boards took {totalSolvingTime}ms");
      messages.Add($"Avg. solving time for single board is {totalSolvingTime / totalSolutionCount}ms");

      var writer = new SudokuWriter();
      boardsToSolve.ForEach(s =>
      {
        messages.Add($"Solutions found in {s.TotalDurationMs}ms");
        if (prettyPrint)
        {
          messages.Add(writer.GetPrettyStringRepresentation(s.OriginalBoard));
          s.SolutionList.ForEach(b => messages.Add(writer.GetPrettyStringRepresentation(b)));
        }
        else
        {
          messages.Add(writer.GetStringRepresentation(s.OriginalBoard));
          s.SolutionList.ForEach(b => messages.Add(writer.GetStringRepresentation(b)));
        }
      });

      messages.ForEach(WriteLine);
      messages.Clear();
    }

    private static List<string> GetSomePuzzles()
    {
      #region 95 hard ones (hard95)
      var hard95 = @"4.....8.5.3..........7......2.....6.....8.4......1.......6.3.7.5..2.....1.4......
                    52...6.........7.13...........4..8..6......5...........418.........3..2...87.....
                    6.....8.3.4.7.................5.4.7.3..2.....1.6.......2.....5.....8.6......1....
                    48.3............71.2.......7.5....6....2..8.............1.76...3.....4......5....
                    ....14....3....2...7..........9...3.6.1.............8.2.....1.4....5.6.....7.8...
                    ......52..8.4......3...9...5.1...6..2..7........3.....6...1..........7.4.......3.
                    6.2.5.........3.4..........43...8....1....2........7..5..27...........81...6.....
                    .524.........7.1..............8.2...3.....6...9.5.....1.6.3...........897........
                    6.2.5.........4.3..........43...8....1....2........7..5..27...........81...6.....
                    .923.........8.1...........1.7.4...........658.........6.5.2...4.....7.....9.....
                    6..3.2....5.....1..........7.26............543.........8.15........4.2........7..
                    .6.5.1.9.1...9..539....7....4.8...7.......5.8.817.5.3.....5.2............76..8...
                    ..5...987.4..5...1..7......2...48....9.1.....6..2.....3..6..2.......9.7.......5..
                    3.6.7...........518.........1.4.5...7.....6.....2......2.....4.....8.3.....5.....
                    1.....3.8.7.4..............2.3.1...........958.........5.6...7.....8.2...4.......
                    6..3.2....4.....1..........7.26............543.........8.15........4.2........7..
                    ....3..9....2....1.5.9..............1.2.8.4.6.8.5...2..75......4.1..6..3.....4.6.
                    45.....3....8.1....9...........5..9.2..7.....8.........1..4..........7.2...6..8..
                    .237....68...6.59.9.....7......4.97.3.7.96..2.........5..47.........2....8.......
                    ..84...3....3.....9....157479...8........7..514.....2...9.6...2.5....4......9..56
                    .98.1....2......6.............3.2.5..84.........6.........4.8.93..5...........1..
                    ..247..58..............1.4.....2...9528.9.4....9...1.........3.3....75..685..2...
                    4.....8.5.3..........7......2.....6.....5.4......1.......6.3.7.5..2.....1.9......
                    .2.3......63.....58.......15....9.3....7........1....8.879..26......6.7...6..7..4
                    1.....7.9.4...72..8.........7..1..6.3.......5.6..4..2.........8..53...7.7.2....46
                    4.....3.....8.2......7........1...8734.......6........5...6........1.4...82......
                    .......71.2.8........4.3...7...6..5....2..3..9........6...7.....8....4......5....
                    6..3.2....4.....8..........7.26............543.........8.15........8.2........7..
                    .47.8...1............6..7..6....357......5....1..6....28..4.....9.1...4.....2.69.
                    ......8.17..2........5.6......7...5..1....3...8.......5......2..4..8....6...3....
                    38.6.......9.......2..3.51......5....3..1..6....4......17.5..8.......9.......7.32
                    ...5...........5.697.....2...48.2...25.1...3..8..3.........4.7..13.5..9..2...31..
                    .2.......3.5.62..9.68...3...5..........64.8.2..47..9....3.....1.....6...17.43....
                    .8..4....3......1........2...5...4.69..1..8..2...........3.9....6....5.....2.....
                    ..8.9.1...6.5...2......6....3.1.7.5.........9..4...3...5....2...7...3.8.2..7....4
                    4.....5.8.3..........7......2.....6.....5.8......1.......6.3.7.5..2.....1.8......
                    1.....3.8.6.4..............2.3.1...........958.........5.6...7.....8.2...4.......
                    1....6.8..64..........4...7....9.6...7.4..5..5...7.1...5....32.3....8...4........
                    249.6...3.3....2..8.......5.....6......2......1..4.82..9.5..7....4.....1.7...3...
                    ...8....9.873...4.6..7.......85..97...........43..75.......3....3...145.4....2..1
                    ...5.1....9....8...6.......4.1..........7..9........3.8.....1.5...2..4.....36....
                    ......8.16..2........7.5......6...2..1....3...8.......2......7..3..8....5...4....
                    .476...5.8.3.....2.....9......8.5..6...1.....6.24......78...51...6....4..9...4..7
                    .....7.95.....1...86..2.....2..73..85......6...3..49..3.5...41724................
                    .4.5.....8...9..3..76.2.....146..........9..7.....36....1..4.5..6......3..71..2..
                    .834.........7..5...........4.1.8..........27...3.....2.6.5....5.....8........1..
                    ..9.....3.....9...7.....5.6..65..4.....3......28......3..75.6..6...........12.3.8
                    .26.39......6....19.....7.......4..9.5....2....85.....3..2..9..4....762.........4
                    2.3.8....8..7...........1...6.5.7...4......3....1............82.5....6...1.......
                    6..3.2....1.....5..........7.26............843.........8.15........8.2........7..
                    1.....9...64..1.7..7..4.......3.....3.89..5....7....2.....6.7.9.....4.1....129.3.
                    .........9......84.623...5....6...453...1...6...9...7....1.....4.5..2....3.8....9
                    .2....5938..5..46.94..6...8..2.3.....6..8.73.7..2.........4.38..7....6..........5
                    9.4..5...25.6..1..31......8.7...9...4..26......147....7.......2...3..8.6.4.....9.
                    ...52.....9...3..4......7...1.....4..8..453..6...1...87.2........8....32.4..8..1.
                    53..2.9...24.3..5...9..........1.827...7.........981.............64....91.2.5.43.
                    1....786...7..8.1.8..2....9........24...1......9..5...6.8..........5.9.......93.4
                    ....5...11......7..6.....8......4.....9.1.3.....596.2..8..62..7..7......3.5.7.2..
                    .47.2....8....1....3....9.2.....5...6..81..5.....4.....7....3.4...9...1.4..27.8..
                    ......94.....9...53....5.7..8.4..1..463...........7.8.8..7.....7......28.5.26....
                    .2......6....41.....78....1......7....37.....6..412....1..74..5..8.5..7......39..
                    1.....3.8.6.4..............2.3.1...........758.........7.5...6.....8.2...4.......
                    2....1.9..1..3.7..9..8...2.......85..6.4.........7...3.2.3...6....5.....1.9...2.5
                    ..7..8.....6.2.3...3......9.1..5..6.....1.....7.9....2........4.83..4...26....51.
                    ...36....85.......9.4..8........68.........17..9..45...1.5...6.4....9..2.....3...
                    34.6.......7.......2..8.57......5....7..1..2....4......36.2..1.......9.......7.82
                    ......4.18..2........6.7......8...6..4....3...1.......6......2..5..1....7...3....
                    .4..5..67...1...4....2.....1..8..3........2...6...........4..5.3.....8..2........
                    .......4...2..4..1.7..5..9...3..7....4..6....6..1..8...2....1..85.9...6.....8...3
                    8..7....4.5....6............3.97...8....43..5....2.9....6......2...6...7.71..83.2
                    .8...4.5....7..3............1..85...6.....2......4....3.26............417........
                    ....7..8...6...5...2...3.61.1...7..2..8..534.2..9.......2......58...6.3.4...1....
                    ......8.16..2........7.5......6...2..1....3...8.......2......7..4..8....5...3....
                    .2..........6....3.74.8.........3..2.8..4..1.6..5.........1.78.5....9..........4.
                    .52..68.......7.2.......6....48..9..2..41......1.....8..61..38.....9...63..6..1.9
                    ....1.78.5....9..........4..2..........6....3.74.8.........3..2.8..4..1.6..5.....
                    1.......3.6.3..7...7...5..121.7...9...7........8.1..2....8.64....9.2..6....4.....
                    4...7.1....19.46.5.....1......7....2..2.3....847..6....14...8.6.2....3..6...9....
                    ......8.17..2........5.6......7...5..1....3...8.......5......2..3..8....6...4....
                    963......1....8......2.5....4.8......1....7......3..257......3...9.2.4.7......9..
                    15.3......7..4.2....4.72.....8.........9..1.8.1..8.79......38...........6....7423
                    ..........5724...98....947...9..3...5..9..12...3.1.9...6....25....56.....7......6
                    ....75....1..2.....4...3...5.....3.2...8...1.......6.....1..48.2........7........
                    6.....7.3.4.8.................5.4.8.7..2.....1.3.......2.....5.....7.9......1....
                    ....6...4..6.3....1..4..5.77.....8.5...8.....6.8....9...2.9....4....32....97..1..
                    .32.....58..3.....9.428...1...4...39...6...5.....1.....2...67.8.....4....95....6.
                    ...5.3.......6.7..5.8....1636..2.......4.1.......3...567....2.8..4.7.......2..5..
                    .5.3.7.4.1.........3.......5.8.3.61....8..5.9.6..1........4...6...6927....2...9..
                    ..5..8..18......9.......78....4.....64....9......53..2.6.........138..5....9.714.
                    ..........72.6.1....51...82.8...13..4.........37.9..1.....238..5.4..9.........79.
                    ...658.....4......12............96.7...3..5....2.8...3..19..8..3.6.....4....473..
                    .2.3.......6..8.9.83.5........2...8.7.9..5........6..4.......1...1...4.22..7..8.9
                    .5..9....1.....6.....3.8.....8.4...9514.......3....2..........4.8...6..77..15..6.
                    .....2.......7...17..3...9.8..7......2.89.6...13..6....9..5.824.....891..........
                    3...8.......7....51..............36...2..4....7...........6.13..452...........8..";
      #endregion

      using (var reader = new StringReader(hard95))
      {
        var result = new List<string>();
        string line;
        while ((line = reader.ReadLine()) != null)
          result.Add(line);

        return result;
      }
    }
  }
}
