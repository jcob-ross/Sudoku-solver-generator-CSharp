namespace Sudoku.Core
{
  using System;
  using System.Text;

  /// <summary>
  ///   Writer for solved sudoku boards
  /// </summary>
  public class SudokuWriter
  {
    private readonly StringBuilder _sb = new StringBuilder(81);

    /// <summary>
    ///   Creates <see cref="String"/> representation of the specifed <see cref="Board"/> as a single continuous sequence of characters.
    ///   Zero/empty squares are represented as "." (dot), otherwise square's integer value is used.
    /// </summary>
    /// <param name="board"><see cref="Board"/> to be stringified.</param>
    /// <returns><see cref="String"/> representation of the <see cref="Board"/></returns>
    /// <exception cref="ArgumentNullException"><paramref name="board"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="board"/> is instance of <see cref="NullBoard"/></exception>
    public string GetStringRepresentation(Board board)
    {
      if (null == board)
        throw new ArgumentNullException(nameof(board));

      if (board is NullBoard)
        throw new ArgumentException("Invalid board data", nameof(board));

      foreach (var cell in board.Cells)
      {
        if (cell.HasValue)
          _sb.Append(cell.Value);
        else
          _sb.Append('.');
      }
      var result = _sb.ToString();
      _sb.Clear();
      return result;
    }

    /// <summary>
    ///   Creates readable, multiline version of solved sudoku board
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public string GetPrettyStringRepresentation(Board board)
    {
      if (null == board)
        throw new ArgumentNullException(nameof(board));

      if (board is NullBoard)
        throw new ArgumentException("Invalid board data", nameof(board));


      _sb.AppendLine("  A B C   D E F   G H I");
      _sb.AppendLine("+-------+-------+-------+");
      var rowNumber = 0;
      for (var i = 1; i <= board.Length; ++i)
      {
        if ((i - 1 ) % 9 == 0) // left side of the board
          _sb.Append("| ");

        if (board.Cells[i - 1].HasValue)
          _sb.Append(board.Cells[i - 1].Value + " ");
        else
          _sb.Append("_ ");

        if (i % 3 == 0) // horizontal box spacing
          _sb.Append("| ");

        if (i % 9 == 0) // new row of cells
          _sb.Append((char)((rowNumber++) + (int)'1') + Environment.NewLine);

        if (i % 27 == 0) // new row of boxes
          _sb.AppendLine("+-------+-------+-------+");
      }

      var result = _sb.ToString();
      _sb.Clear();
      return result;
    }
  }
}