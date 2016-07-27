namespace Sudoku.Console
{
  using System;

  /// <summary>
  ///   Represents type of task for the solver
  /// </summary>
  [Flags]
  public enum TaskType
  {
    None = 0,
    SolveFromCli      = 1 << 0,
    SolveFromFile     = 1 << 1,
    PrettyPrint       = 1 << 2,
    Benchmark         = 1 << 3,
    Create            = 1 << 4
  }
}