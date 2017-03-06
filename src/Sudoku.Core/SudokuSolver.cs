namespace Sudoku.Core
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  /// <summary>
  ///   Sudoku solver
  /// </summary>
  public class SudokuSolver
  {
    public class SudokuSolverException : ArgumentException
    {
      public SudokuSolverException(string message) : base(message)
      {
      }
    }

    private const int SupportedBoardSize = 81;
    
    /// <summary>
    ///   Creates instance of <see cref="Board"/> where cell values are given by parameter.
    /// </summary>
    /// <param name="boardValues">
    ///   <see cref="T:byte[]"/> containing cell values.
    ///   <remarks>
    ///     Cell values are red in sequence from top-left to bottom-right board corner
    ///   </remarks>
    /// </param>
    /// <returns><see cref="Board"/> with cell candidates set according to sudoku rules.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="boardValues"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Unsupported board size - not a 9x9 sudoku board.</exception>
    /// <exception cref="ArgumentException">Board is unsolvable - value placement on the board violates sudoku rules.</exception>
    public Board CreateBoard(byte[] boardValues)
    {
      if (null == boardValues)
        throw new ArgumentNullException(nameof(boardValues));
      
      if (boardValues.Length != SupportedBoardSize)
        throw new ArgumentException("Unsupported board size", paramName: nameof(boardValues));

      var cells = boardValues.Select(val => val > 0 ? new Cell(val) : new Cell(Candidates.All)).ToArray();
      var board = new Board(cells, width: 9, sectorWidth: 3, length: SupportedBoardSize);

      for (var i = 0; i < SupportedBoardSize; ++i)
      {
        if (!board.Cells[i].HasValue)
          continue;

        board = board.PlaceAndApplyRules(i, board.Cells[i].Value);
        if (board == null)
          throw new ArgumentException("Board is unsolvable", paramName: nameof(boardValues));
      }
      return board;
    }

    public Board PlaceValue(Board input, int cellIndex, byte value)
    {
      // early exit if the value is not present in cell's candidate list
      if (!input.Cells[cellIndex].HasCandidate(value))
        return null;

      var board = input.PlaceAndApplyRules(cellIndex, value);
      if (board == null)
        return null;

      foreach (var singularizedCellIndex in FindSingularizedCells(input, board, cellIndex))
      {
        #if DEBUG
        Debug.Assert(board.Cells[singularizedCellIndex].CandidateCount == 1);
        #endif

        if (null == (board = PlaceValue(board, singularizedCellIndex, board.Cells[singularizedCellIndex].GetCandidateAsValue())))
          return null;
      }

      return board;
    }

    /// <summary>
    ///   Finds cells which candidate count has been reduced to one.
    /// </summary>
    /// <param name="originalBoard">Board before placing a value in the cell</param>
    /// <param name="modifiedBoard">Board after placing a value in the cell</param>
    /// <param name="cellIndex">Index of the cell that has been modified.</param>
    /// <returns>
    ///   <see cref="T:List{int}"/> containing indexes of cells which candidate count has been reduced
    ///   to exactly one by the act of placing a value.
    /// </returns>
    public List<int> FindSingularizedCells(Board originalBoard, Board modifiedBoard, int cellIndex)
    {
      Debug.Assert(originalBoard.Length == modifiedBoard.Length);
      var result = new List<int>();

      var peerIndxes = originalBoard.GetPeers(cellIndex);
      for (var i = 0; i < peerIndxes.Length; ++i)
      {
        if (modifiedBoard.Cells[peerIndxes[i]].HasSingleCandidate && !originalBoard.Cells[peerIndxes[i]].HasSingleCandidate)
          result.Add(peerIndxes[i]);
      }
      return result;
    }

    /// <summary>
    ///   Returns cell index of a cell that has atleast <paramref name="minimumCellCandidates"/> number of candidate values.
    /// </summary>
    /// <param name="input"><see cref="Board"/> to pick a cell from.</param>
    /// <param name="minimumCellCandidates">
    ///   Mimimum number of candidate values a cell has to have to be considered.
    /// </param>
    /// <returns><see cref="int"/> index of a picked cell.</returns>
    public int PickCellToGuessOn(Board input, int minimumCellCandidates = 2)
    {
      var cellWithAtLeastMinCandidates = input.Cells.Where(cell => cell.CandidateCount >= minimumCellCandidates);
      if (! cellWithAtLeastMinCandidates.Any())
        return Array.FindIndex(input.Cells, c => c.HasValue == false);
      
      var localMinimum = cellWithAtLeastMinCandidates.Min(c => c.CandidateCount);
      return Array.FindIndex(input.Cells, c => c.CandidateCount >= localMinimum);
    }

    /// <summary>
    ///   Searches for first valid solution for given sudoku board.
    /// </summary>
    /// <param name="input"><see cref="Board"/> to solve.</param>
    /// <returns>Fully filled out <see cref="Board"/> -or- <see langword="null"/> if no solution was found.</returns>
    public Board Solve(Board input)
    {
      if (input.Cells.All(c => c.HasValue)) // every cell on board has one candidate => solved board
        return input;

      var activeCellIndex = PickCellToGuessOn(input);
      var possibleVals = input.Cells[activeCellIndex].PossibleValues();
      for (int i = 0; i < possibleVals.Length; ++i)
      {
        Board board;
        if ((board = PlaceValue(input, activeCellIndex, possibleVals[i])) != null)
          if ((board = Solve(board)) != null)
            return board;
      }
      return null;
    }

    /// <summary>
    ///   Searches for all solutions of given sudoku board up to a number
    ///   of solutions (if specified).
    /// </summary>
    /// <param name="input"><see cref="Board"/> to solve.</param>
    /// <param name="maximumSolutions">Maximum number of solutions to look for.</param>
    /// <returns><see cref="T:List{Board}"/> containing solutions.</returns>
    public List<Board> FindAllSolutions(Board input, int maximumSolutions = 0)
    {
      var results = new List<Board>();
      Solve(input, board =>
                   {
                     results.Add(board);
                     return results.Count < maximumSolutions || maximumSolutions <= 0;
                   });
      return results;
    }

    public Tuple<Board, Board> CreateRandomBoard()
    {
      return CreateSeededBoard(new Board());
    }

    public Tuple<Board, Board> CreateSeededBoard(Board seed)
    {
      var startingBoard = seed;
      var random = new Random();

      // 1 - choose random cell     (with more than one candidate)
      // 2 - set cell's value       (from among that cell's candidates)
      // 3 - solve that board

      // 4a   - exactly one solution found? -> return that board, we're done
      // 4b   - no solution?                -> go to step 1 (discard original cell placement)
      // 4c   - multiple solutions found?   -> go to step 1 (keep original cell placement)

      while (true)
      {
        var emptyCells = startingBoard.Cells
                              .Select((cell, index) => new { index, cell })
                              .Where(t => !t.cell.HasValue && !t.cell.HasSingleCandidate)
                              .Select(t => t.index) // we need just indexes
                              .ToArray();

        var cellIndex = emptyCells[random.Next(emptyCells.Length)];
        var possibleValues = startingBoard.Cells[cellIndex].PossibleValues();
        var idx = random.Next(possibleValues.Length);
        var candidateValue = possibleValues[idx];

        var board = PlaceValue(startingBoard, cellIndex, candidateValue);
        if (board != null)
        {
          var solutions = FindAllSolutions(board, 2);

          if (solutions.Count == 1)
            return new Tuple<Board, Board>(board, solutions.Single());
          if (solutions.Count == 0)
            continue;

          startingBoard = board;
        }
      }
    }

    private Board Solve(Board input, Func<Board, bool> solutionFoundFunc)
    {
      if (input.Cells.All(c => c.HasValue))
      {
        if (solutionFoundFunc == null)
          return input;

        return solutionFoundFunc(input) ? null : input;
      }

      var activeCellIndex = PickCellToGuessOn(input);
      var possibleValues = input.Cells[activeCellIndex].PossibleValues();

      for (var i = 0; i < possibleValues.Length; ++i)
      {
        Board board;
        if ((board = PlaceValue(input, activeCellIndex, possibleValues[i])) != null)
          if ((board = Solve(board, solutionFoundFunc)) != null)
            return board;
      }
      return null;
    }
  }
}