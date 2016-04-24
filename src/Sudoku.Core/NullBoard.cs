namespace Sudoku.Core
{
  /// <summary>
  ///   Represents invalid sudoku board
  /// </summary>
  public class NullBoard : Board
  {
    public NullBoard() : base(null, 0, 0, 0){}
  }
}