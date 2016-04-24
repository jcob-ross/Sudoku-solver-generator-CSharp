namespace Sudoku.Tests.Core
{
  using System;
  using Sudoku.Core;
  using Xunit;
  using Xunit.Sdk;
  using XunitShould;

  public class CellTests
  {
    [Fact]
    public void Ctor_Default_should_create_cell_with_HasValue_property_equal_to_false()
    {
      var sut = new Cell();

      sut.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Ctor_Default_should_create_cell_with_Value_property_equal_to_zero()
    {
      var sut = new Cell();

      sut.Value.ShouldEqual((byte)0);
    }

    [Fact]
    public void Ctor_Default_should_create_cell_with_Candidates_property_equal_to_Candidates_None()
    {
      var sut = new Cell();

      sut.Candidates.ShouldEqual(Candidates.None);
    }

    [Theory]
    [InlineData(Candidates.All)]
    [InlineData(Candidates.Two)]
    [InlineData(Candidates.Four | Candidates.Seven)]
    [InlineData(Candidates.All & ~Candidates.Nine)]
    [InlineData(Candidates.One | Candidates.Seven | Candidates.Eight)]
    public void Ctor_with_Candidates_param_should_correctly_setup_candidates(Candidates candidates)
    {
      var sut = new Cell(candidates);

      sut.Candidates.ShouldEqual(candidates);
    }

    [Fact]
    public void Ctor_with_Candidates_param_should_produce_cell_with_HasValue_property_false()
    {
      var sut = new Cell(Candidates.Eight);

      sut.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Ctor_with_Candidates_param_should_produce_cell_with_Value_property_equal_to_zero()
    {
      var sut = new Cell(Candidates.Seven);

      sut.Value.ShouldEqual((byte)0);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(5)]
    public void Value_property_setter_should_remove_all_candidates(byte cellValue)
    {
      var sut = new Cell(Candidates.All);

      sut.Value = cellValue;

      sut.Candidates.ShouldEqual(Candidates.None);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void Value_property_setter_should_correctly_set_value_of_the_cell(byte cellValue)
    {
      var sut = new Cell(Candidates.Eight);

      sut.Value = cellValue;
      
      sut.Value.ShouldEqual(cellValue);
    }

    [Theory]
    [InlineData(9, Candidates.One | Candidates.Five | Candidates.Nine, Candidates.One | Candidates.Five)]
    [InlineData(1, Candidates.All, Candidates.Two | Candidates.Three | Candidates.Four | Candidates.Five | Candidates.Six | Candidates.Seven | Candidates.Eight | Candidates.Nine)]
    [InlineData(2, Candidates.One | Candidates.Four | Candidates.Five | Candidates.Nine, Candidates.One | Candidates.Four | Candidates.Five | Candidates.Nine)]
    public void RemoveValueFromCandidates_should_correctly_remove_value_from_candidates(byte value, Candidates candidates, Candidates expectedCandidates)
    {
      var sut = new Cell(candidates);

      sut.RemoveValueFromCandidates(value);

      sut.Candidates.ShouldEqual(expectedCandidates);
    }

    [Theory]
    [InlineData(255)]
    [InlineData(10)]
    public void RemoveValueFromCandidates_should_throw_ArgumentOutOfRangeException_when_value_parameter_out_of_range(byte value)
    {
      var sut = new Cell();

      Assert.Throws<ArgumentOutOfRangeException>(() => sut.RemoveValueFromCandidates(value));
    }

    [Theory]
    [InlineData(Candidates.None,  0)]
    [InlineData(Candidates.One,   1)]
    [InlineData(Candidates.Two,   2)]
    [InlineData(Candidates.Three, 3)]
    [InlineData(Candidates.Four,  4)]
    [InlineData(Candidates.Five,  5)]
    [InlineData(Candidates.Six,   6)]
    [InlineData(Candidates.Seven, 7)]
    [InlineData(Candidates.Eight, 8)]
    [InlineData(Candidates.Nine,  9)]
    public void GetLastCandidateAsValue_should_return_last_remaining_candidate_as_value(Candidates candidates, byte expectedValue)
    {
      var sut = new Cell(candidates);

      sut.GetCandidateAsValue().ShouldEqual(expectedValue);
    }

    [Theory]
    [InlineData(9, Candidates.All)]
    [InlineData(4, Candidates.One | Candidates.Four)]
    [InlineData(5, Candidates.Four | Candidates.Five)]
    public void GetLastCandidateAsValue_should_return_the_biggest_candidate_if_more_than_one_candidate_remains(byte expectedValue, Candidates candidates)
    {
      var sut = new Cell(candidates);

      sut.GetCandidateAsValue().ShouldEqual(expectedValue);
    }

    [Theory]
    [InlineData(9, Candidates.All)]
    [InlineData(0, Candidates.None)]
    [InlineData(2, Candidates.One | Candidates.Five)]
    [InlineData(3, Candidates.One | Candidates.Five | Candidates.Seven)]
    public void CandidateCount_should_return_correct_number_of_cell_candidates(int expectedCount, Candidates candidates)
    {
      var sut = new Cell(candidates);

      sut.CandidateCount.ShouldEqual(expectedCount);
    }

    [Theory]
    [InlineData(1, true, Candidates.One)]
    [InlineData(7, true, Candidates.All)]
    [InlineData(9, true, Candidates.One | Candidates.Nine)]
    [InlineData(2, false, Candidates.One | Candidates.Nine)]
    public void HasCandidate_should_successfuly_find_candidate_by_its_byte_value(byte value, bool expectedResult, Candidates candidates)
    {
      var sut = new Cell(candidates);

      sut.HasCandidate(value).ShouldEqual(expectedResult);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void HasCandidate_should_throw_ArgumentOutOfRangeException_when_input_is_out_of_range(byte value)
    {
      var sut = new Cell(Candidates.All);

      Assert.Throws<ArgumentOutOfRangeException>(() => sut.HasCandidate(value));
    }

    [Theory]
    [InlineData(Candidates.All, new byte[] {1,2,3,4,5,6,7,8,9})]
    [InlineData(Candidates.None, new byte[] {})]
    [InlineData(Candidates.Two | Candidates.Six | Candidates.Seven, new byte[] {2,6,7})]
    public void PossibleValues_should_return_correct_candidates(Candidates candidates, byte[] expectedValues)
    {
      var sut = new Cell(candidates);

      sut.PossibleValues().ShouldEnumerateEqual(expectedValues);
    }

    [Theory]
    [InlineData(1, Candidates.None, Candidates.None)]
    [InlineData(1, Candidates.One, Candidates.None)]
    [InlineData(9, Candidates.Nine, Candidates.None)]
    [InlineData(8, Candidates.Seven | Candidates.Eight, Candidates.Seven)]
    public void RemoveValueFromCandidates_should_remove_candidate_specified_by_its_value(byte value, Candidates initialCandidates, Candidates expectedCandidates)
    {
      var sut = new Cell(initialCandidates);

      sut.RemoveValueFromCandidates(value);

      sut.Candidates.ShouldEqual(expectedCandidates);
    }

    [Theory]
    [InlineData(0, Candidates.None)]
    [InlineData(0, Candidates.All)]
    [InlineData(10, Candidates.One)]
    [InlineData(10, Candidates.Two | Candidates.Eight)]
    public void RemoveValueFromCandidates_should_throw_ArgumentOutOfRangeException_when_value_is_out_of_range(byte value, Candidates initialCandidates)
    {
      var sut = new Cell(initialCandidates);

      Assert.Throws<ArgumentOutOfRangeException>(() => sut.RemoveValueFromCandidates(value));
    }

    [Theory]
    [InlineData(false, Candidates.None)]
    [InlineData(false, Candidates.All)]
    [InlineData(false, Candidates.One | Candidates.Eight | Candidates.Five)]
    [InlineData(true, Candidates.One)]
    [InlineData(true, Candidates.Two)]
    [InlineData(true, Candidates.Three)]
    [InlineData(true, Candidates.Four)]
    [InlineData(true, Candidates.Five)]
    [InlineData(true, Candidates.Six)]
    [InlineData(true, Candidates.Seven)]
    [InlineData(true, Candidates.Eight)]
    [InlineData(true, Candidates.Nine)]
    public void HasSingleCandidate_should_correctly_identify_presence_of_single_flag_only(bool expectedResult, Candidates candidates)
    {
      var sut = new Cell(candidates);

      sut.HasSingleCandidate.ShouldEqual(expectedResult);
    }

    [Fact]
    public void HasSingleCandidate_should_return_false_if_cell_has_value_set()
    {
      var sut = new Cell(6);

      sut.HasSingleCandidate.ShouldBeFalse();
    }
  }
}