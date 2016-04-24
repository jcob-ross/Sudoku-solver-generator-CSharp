using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sudoku.Core
{
  /// <summary>
  ///   Parser for 9x9 sudoku boards
  /// </summary>
  public class SudokuParser
  {
    // everything except numeric and dot
    private readonly Regex _inputFilterRegex = new Regex("[^0-9.]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    ///   Initializes new instance of <see cref="SudokuParser"/>.
    /// </summary>
    public SudokuParser(){}

    /// <summary>
    ///   Tries to parse 9x9 sudoku board.
    /// </summary>
    /// <param name="input">
    ///   <see cref="String"/> representation of sudoku board.
    ///   <remarks>
    ///     All characters except 0 - 9 and '.' (dot) will be removed.
    ///     Cell values are red in sequence from top-left to bottom-right board corner
    ///   </remarks>
    /// </param>
    /// <param name="cellValues">
    ///   <see cref="T:byte[]"/> to store parsed sudoku board into.
    /// </param>
    /// <returns><see cref="bool"/> representing success of the parsing operation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null" /> -or- white space.</exception>
    /// <exception cref="RegexMatchTimeoutException">A time-out occurred.</exception>
    public bool TryParse9X9Board(string input, out byte[] cellValues)
    {
      if (string.IsNullOrWhiteSpace(input))
        throw new ArgumentNullException(nameof(input));

      const int boardLength = 81;

      cellValues = new byte[boardLength];

      var cleanInput = _inputFilterRegex.Replace(input, String.Empty);
      if (cleanInput.Length != boardLength)
        return false;

      for (var i = 0; i < boardLength; ++i)
      {
        byte value;
        if (Byte.TryParse(cleanInput[i].ToString(), out value))
          cellValues[i] = value;
        else
          cellValues[i] = 0;
      }
      return true;
    }
  }
}