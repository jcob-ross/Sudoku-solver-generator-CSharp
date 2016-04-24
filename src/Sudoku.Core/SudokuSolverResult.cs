using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sudoku.Core
{
  public class SudokuSolverResult
  {
    public class SolutionAddedEventArgs : EventArgs
    {
      public SolutionAddedEventArgs(Board originalBoard, Board foundSolution)
      {
        OriginalBoard = originalBoard;
        FoundSolution = foundSolution;
      }

      public Board OriginalBoard { get; private set; }
      public Board FoundSolution { get; private set; }
    }

    private Task<SudokuSolverResult> _workTask;
    private readonly CancellationTokenSource _tokenSource;
    private readonly Board _originalBoard;
    private readonly List<Board> _solutions;

    public SudokuSolverResult(Board originalBoard, CancellationTokenSource tokenSource)
    {
      if (null == originalBoard)
        throw new ArgumentNullException(nameof(originalBoard));

      _tokenSource = tokenSource;
      _originalBoard = originalBoard;
    }

    public Board OriginalBoard => _originalBoard;
    public List<Board> SolutionsFound;

    public void CancelPendingSearch()
    {
      // todo - SudokuSolverResult - CancelPendingSearch - exceptions
      _tokenSource.Cancel();
    }

    public Task<SudokuSolverResult> WorkTask
    {
      get
      {
        return _workTask;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException(nameof(value));

        if (_workTask != null)
          throw new InvalidOperationException("Work task already registered. Cannot reassign.");

        _workTask = value;
      }
    }

    public event Action<SolutionAddedEventArgs> SolutionAdded;
    protected virtual void OnSolutionAdded(Board newSolutionBoard)
    {
      SolutionAdded?.Invoke(new SolutionAddedEventArgs(OriginalBoard, newSolutionBoard));
    }
  }
}