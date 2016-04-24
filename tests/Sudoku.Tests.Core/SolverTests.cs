namespace Sudoku.Tests.Core
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Text;
  using Sudoku.Core;
  using Xunit;
  using XunitShould;

  public class SolverTests
  {
    [Fact]
    public void PickCellToGuessOn_should_correctly_pick_cell_by_minimumCellCandidates_parameter()
    {
      //164 382 597         http://planetsudoku.com/  Puzzle #1811494
      //523 971 648
      //897 654 .2. (1, 1/3) <- here is the one that should be picked
      //679 513 482             cell 26
      //281 497 3.. (6, 5)
      //435 826 719
      //948 135 276
      //312 769 854
      //756 248 93. (1)
      const int expectedResult = 26;

      string input = @" 164 382 597
                        523 971 648
                        897 654 .2.

                        679 513 482
                        281 497 3..
                        435 826 719

                        948 135 276
                        312 769 854
                        756 248 93.";

      var parser = CreateParser();

      byte[] boardValues;
      parser.TryParse9X9Board(input, out boardValues);

      var solver = CreateSolver();
      var board = solver.CreateBoard(boardValues);
      var cellIndex = solver.PickCellToGuessOn(board, minimumCellCandidates: 2);
      cellIndex.ShouldEqual(expectedResult);
    }

    [Fact]
    public void PickCellToGuessOn_should_pick_first_cell_with_no_value_set_when_all_other_cell_dont_satisfy_minCellCandidates_parameter()
    {
      //164 382 597         http://planetsudoku.com/  Puzzle #1811494
      //523 971 648
      //897 654 123
      //679 513 482
      //281 497 36. (5) <- should pick this one, index 44
      //435 826 719
      //948 135 276
      //312 769 854
      //756 248 93. (1)
      const int expectedResult = 44;
      string input = @" 164 382 597
                        523 971 648
                        897 654 123

                        679 513 482
                        281 497 36.
                        435 826 719

                        948 135 276
                        312 769 854
                        756 248 93.";

      var parser = CreateParser();

      byte[] boardValues;
      parser.TryParse9X9Board(input, out boardValues);

      var solver = CreateSolver();
      var board = solver.CreateBoard(boardValues);
      var cellIndex = solver.PickCellToGuessOn(board, minimumCellCandidates: 2);
      cellIndex.ShouldEqual(expectedResult);
    }

    // 1.......9..67...2..8....4......75.3...5..2....6.3......9....8..6...4...1..25...6.
    // 123456789456789123789123456214975638375862914968314275591637842637248591842591367
    // ..9...4...7.3...2.8...6...71..8....6....1..7.....56...3....5..1.4.....9...2...7..
    // 239187465675394128814562937123879546456213879798456312367945281541728693982631754
    [Theory]
    [InlineData(new byte[] { 1,0,0,0,0,0,0,0,9,0,0,6,7,0,0,0,2,0,0,8,0,0,0,0,4,0,0,0,0,0,0,7,5,0,3,0,0,0,5,0,0,2,0,0,0,0,6,0,3,0,0,0,0,0,0,9,0,0,0,0,8,0,0,6,0,0,0,4,0,0,0,1,0,0,2,5,0,0,0,6,0 }, 
                new byte[] { 1,2,3,4,5,6,7,8,9,4,5,6,7,8,9,1,2,3,7,8,9,1,2,3,4,5,6,2,1,4,9,7,5,6,3,8,3,7,5,8,6,2,9,1,4,9,6,8,3,1,4,2,7,5,5,9,1,6,3,7,8,4,2,6,3,7,2,4,8,5,9,1,8,4,2,5,9,1,3,6,7 })]
    [InlineData(new byte[] { 0,0,9,0,0,0,4,0,0,0,7,0,3,0,0,0,2,0,8,0,0,0,6,0,0,0,7,1,0,0,8,0,0,0,0,6,0,0,0,0,1,0,0,7,0,0,0,0,0,5,6,0,0,0,3,0,0,0,0,5,0,0,1,0,4,0,0,0,0,0,9,0,0,0,2,0,0,0,7,0,0 }, 
                new byte[] { 2,3,9,1,8,7,4,6,5,6,7,5,3,9,4,1,2,8,8,1,4,5,6,2,9,3,7,1,2,3,8,7,9,5,4,6,4,5,6,2,1,3,8,7,9,7,9,8,4,5,6,3,1,2,3,6,7,9,4,5,2,8,1,5,4,1,7,2,8,6,9,3,9,8,2,6,3,1,7,5,4 })]
    public void Solve_should_correctly_solve_given_sudoku_board(byte[] boardValues, byte[] expectedResult)
    {
      var sut = CreateSolver();
      var board = sut.CreateBoard(boardValues);

      var result = sut.Solve(board);

      for (var i = 0; i < result.Length; ++i)
      {
        result.Cells[i].Value.ShouldEqual(expectedResult[i]);
      }
    }

    [Theory]
    [InlineData(2, new byte[] { 9,0,6,0,7,0,4,0,3,0,0,0,4,0,0,2,0,0,0,7,0,0,2,3,0,1,0,5,0,0,0,0,0,1,0,0,0,4,0,2,0,8,0,6,0,0,0,3,0,0,0,0,0,5,0,3,0,7,0,0,0,5,0,0,0,7,0,0,5,0,0,0,4,0,5,0,1,0,7,0,8 })]
    [InlineData(5, new byte[] { 8,0,0,6,0,0,9,0,5,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,3,1,0,0,0,7,3,1,8,0,6,0,2,4,0,0,0,0,0,7,3,0,0,0,0,0,0,0,0,0,0,0,2,7,9,0,1,0,0,5,0,0,0,8,0,0,3,6,0,0,3,0,0,0,0,0,0 })]
    public void FindAllSolutions_should_find_correct_number_of_solutions(int expectedSolutionCount, byte[] boardValues)
    {
      //boardValues.Length.ShouldEqual(81);

      #region 2 boards and solutions

      // 9.6.7.4.3...4..2...7..23.1.5.....1...4.2.8.6...3.....5.3.7...5...7..5...4.5.1.7.8
      // 926571483351486279874923516582367194149258367763149825238794651617835942495612738
      // 926571483351486279874923516582367194149258367763194825238749651617835942495612738
      // 8..6..9.5.............2.31...7318.6.24.....73...........279.1..5...8..36..3......
      // 814637925325149687796825314957318462241956873638274591462793158579481236183562749
      // 814637925325941687796825314957318462241569873638472591462793158579184236183256749
      // 834671925125839647796425318957318462241956873368247591682793154579184236413562789
      // 834671925125839647796524318957318462241956873368247591682793154519482736473165289
      // 834671925125839647796524318957318462241965873368247591682793154519482736473156289

      #endregion

      var sut = CreateSolver();
      var board = sut.CreateBoard(boardValues);

      var result = sut.FindAllSolutions(board);

      result.Count.ShouldEqual(expectedSolutionCount);
    }

    private SudokuParser CreateParser()
    {
      return new SudokuParser();
    }

    private SudokuSolver CreateSolver()
    {
      return new SudokuSolver();
    }
  }
}