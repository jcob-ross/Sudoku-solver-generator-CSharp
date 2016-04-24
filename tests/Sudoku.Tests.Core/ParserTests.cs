using System;
using System.Text;
using Sudoku.Core;
using Xunit;
using XunitShould;

namespace Sudoku.Tests.Core
{
  using System.Text.RegularExpressions;

  public class ParserTests
  {
    [Fact]
    public void TryParseBoard_should_throw_ArgNullException_on_null_or_whitespace_input()
    {
      var sut = CreateParser();

      byte[] cellValues;

      Assert.Throws<ArgumentNullException>(() => sut.TryParse9X9Board(null, out cellValues));
    }

    [Fact]
    public void TryParseBoard_should_fail_on_boards_bigger_than_9x9()
    {
      var sampleBoard = GenerateBoardString(10, 10);
      var sut = CreateParser();

      byte[] cellValues;
      var result = sut.TryParse9X9Board(sampleBoard, out cellValues);

      result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(12)]
    [InlineData(16)]
    [InlineData(36)]
    public void TryParseBoard_sould_fail_on_boards_other_than_9x9(int width)
    {
      var sampleBoard = GenerateBoardString(width, width);

      var sut = CreateParser();
      byte[] board;
      var result = sut.TryParse9X9Board(sampleBoard, out board);

      result.ShouldBeFalse();
    }

    [Fact]
    public void TryParseBoard_should_correctly_parse_valid_9x9_board_and_preserve_coordinate_system()
    {
      var sampleBoard = @"
                          5 3 .   . 7 .   . . .
                          6 . .   1 9 5   . . .
                          . 9 8   . . .   . 6 .

                          8 . .   . 6 .   . . 3
                          4 . .   8 . 3   . . 1
                          7 . .   . 2 .   . . 6

                          . 6 .   . . .   2 8 .
                          . . .   4 1 9   . . 5
                          . . .   . 8 .   . 7 9
                        ";

      var regex = new Regex("[^0-9.]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
      var cleanInput = regex.Replace(sampleBoard, String.Empty);
      const int boardLength = 81;
      var parser = CreateParser();

      byte[] sut;
      parser.TryParse9X9Board(sampleBoard, out sut);

      // compare every square
      for (var i = 0; i < boardLength; ++i)
      {
        var expectedValue = Char.IsDigit(cleanInput[i]) ? (byte)(cleanInput[i] - '0') : 0;
        sut[i].ShouldEqual((byte)expectedValue);
      }
    }


    /// <summary>
    ///   Generates string representing sudoku board.
    ///   Only uses values 1 - 9 and '.' (dot)
    /// </summary>
    /// <returns>string representation of the board</returns>
    private static string GenerateBoardString(int width = 9, int height = 9)
    {
      var rand = new Random(DateTime.Now.Millisecond + 8008135);
      var sb = new StringBuilder();
      for (var y = 0; y < height; y++)
      {
        for (var x = 0; x < width; x++)
        {
          sb.Append(rand.NextDouble() > 0.3 ? rand.Next(1, 10).ToString() : ".");
        }
      }
      return sb.ToString();
    }

    private static SudokuParser CreateParser()
    {
      return new SudokuParser();
    }
  }
}