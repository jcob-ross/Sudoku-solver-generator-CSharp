namespace Sudoku.Tests.Core
{
  using System;
  using System.Linq;
  using Sudoku.Core;
  using Xunit;
  using XunitShould;

  public class BoardTests
  {
    // cells are stored as one continuous array -> cell indexes are in range <0 .. 80>
    // therefore cell indexes 0 and 1 are the same row, same box, but not same column. See below.

    //  0  1  2    3  4  5    6  7  8
    //  9 10 11   12 13 14   15 16 17
    // 18 19 20   21 22 23   24 25 26
    //
    // 27 28 29   30 31 32   33 34 35
    // 36 37 38   39 40 41   42 43 44
    // 45 46 47   48 49 50   51 52 53
    //
    // 54 55 56   57 58 59   60 61 62
    // 63 54 65   66 67 68   69 70 71
    // 72 73 74   75 76 77   78 79 80

    [Fact]
    public void Ctor_Default_should_create_9x9_board()
    {
      const int expectedWidth = 9;
      const int expectedLength = expectedWidth * expectedWidth;
      const int expectedSectorWidth = 3;

      var sut = CreateBoard();
      
      sut.Width.ShouldEqual(expectedWidth);
      sut.SectorWidth.ShouldEqual(expectedSectorWidth);
      sut.Length.ShouldEqual(expectedLength);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(-2, -3)]
    [InlineData(0, -1)]
    [InlineData(0, 81)]
    [InlineData(81, 0)]
    [InlineData(81, 81)]
    public void IsPeer_should_throw_ArgOutOfRange_on_invalid_input(int indexLeft, int indexRight)
    {
      var sut = CreateBoard();

      Assert.Throws<ArgumentOutOfRangeException>(() => sut.IsPeer(indexLeft, indexRight));
    }

    [Theory]
    [InlineData(0, 1, true)]
    [InlineData(3, 29, false)]
    [InlineData(30, 50, true)]
    [InlineData(30, 51, false)]
    public void IsPeer_should_correctly_identify_peer_cells(int indexLeft, int indexRight, bool expectedResult)
    {
      // peer cell is a cell in the same row, column or box

      //  0  1  2    3  4  5    6  7  8
      //  9 10 11   12 13 14   15 16 17
      // 18 19 20   21 22 23   24 25 26
      //
      // 27 28 29   30 31 32   33 34 35
      // 36 37 38   39 40 41   42 43 44
      // 45 46 47   48 49 50   51 52 53
      //
      // 54 55 56   57 58 59   60 61 62
      // 63 54 65   66 67 68   69 70 71
      // 72 73 74   75 76 77   78 79 80
      var sut = CreateBoard();

      sut.IsPeer(indexLeft, indexRight).ShouldEqual(expectedResult);
    }

    [Theory]
    [InlineData(0, new[] {1,2,3,4,5,6,7,8, /* row */ 9,10,11,18,19,20, /* box */ 27,36,45,54,63,72 /* column */})]
    [InlineData(80, new[] {8,17,26,35,44,53, /* column */ 60,61,62,69,70,71, /* box */ 72,73,74,75,76,77,78,79 /* row */})]
    [InlineData(39, new[] {3,12,21, 30,31,32, 36,37,38,40,41,42,43,44, 48,49,50, 57,66,75})]
    public void GetPeers_should_retrieve_correct_peer_indexes_for_specified_cell_index(int cellIndex, int[] expectedPeers)
    {
      const int peerCount = 20; // always 20 for 9x9 board
      var sut = CreateBoard();
      
      var result = sut.GetPeers(cellIndex);

      result.Count().ShouldEqual(peerCount);
      result.ShouldEnumerateEqual(expectedPeers);
    }

    [Fact]
    public void Clone_should_properly_copy_all_cells()
    {
      var board = CreateBoard();
      for (var i = 0; i < board.Length; ++i)
      {
        board.Cells[i] = new Cell(6);
      }

      var sut = (Board)board.Clone();

      for (var i = 0; i < board.Length; ++i)
      {
        board.Cells[i] = new Cell(1);
      }

      for (var i = 0; i < board.Length; ++i)
      {
        board.Cells[i].Value.ShouldEqual((byte)1);
      }
      for (var i = 0; i < board.Length; ++i)
      {
        sut.Cells[i].Value.ShouldEqual((byte)6);
      }

    }

    private Board CreateBoard()
    {
      return new Board();
    }
  }
}