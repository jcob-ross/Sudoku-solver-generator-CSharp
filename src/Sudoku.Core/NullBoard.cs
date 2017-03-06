namespace Sudoku.Core
{
  /// <summary>
  ///   Represents invalid sudoku board
  /// </summary>
  public class NullBoard : Board
  {
    // NOTE: yagni broken, this is used only in SudokuWriter class.
    public NullBoard() : base(null, 0, 0, 0){}
  }
}