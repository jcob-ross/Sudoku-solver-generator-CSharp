using System;
using Sudoku.Core;
using Xunit;
using XunitShould;

namespace Sudoku.Tests.Core
{
  public class WriterTests
  {
    [Fact]
    public void GetStringRepresentation_should_throw_ArgumentNullException_on_null_input()
    {
      var sut = GetWriter();
      Assert.Throws<ArgumentNullException>(() => sut.GetPrettyStringRepresentation(null));
    }

    [Fact]
    public void GetStringRepresentation_NullBoard_input_should_throw_ArgumentException()
    {
      var sut = GetWriter();
      Assert.Throws<ArgumentException>(() => sut.GetPrettyStringRepresentation(new NullBoard()));
    }
    
    private SudokuWriter GetWriter()
    {
      return new SudokuWriter();
    }
  }
}