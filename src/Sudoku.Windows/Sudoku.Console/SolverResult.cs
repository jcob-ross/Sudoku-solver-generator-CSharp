namespace Sudoku.Console
{
  using System.Collections.Generic;
  using Core;

  /// <summary>
  ///   Represents sudoku board and it's solutions
  /// </summary>
  public class SolverResult
  {
    /// <summary>
    ///   Total time needed to find all solutions
    /// </summary>
    public double TotalDurationMs;
    /// <summary>
    ///   Original board
    /// </summary>
    public Board OriginalBoard;
    /// <summary>
    ///   List of solutions
    /// </summary>
    public List<Board> SolutionList;
    /// <summary>
    ///   List of times needed to solve for individual boards
    /// </summary>
    public List<double> SolvingTimes = new List<double>();
    /// <summary>
    ///   Collection for misc. messages
    /// </summary>
    public List<string> Messages = new List<string>();
  }
}