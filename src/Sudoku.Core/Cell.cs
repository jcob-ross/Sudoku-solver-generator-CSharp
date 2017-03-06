namespace Sudoku.Core
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;

  /// <summary>
  ///   Represents cell holding a value and candidate values
  /// </summary>
  [DebuggerDisplay("Val: {Value} HasSingleCandidate: {HasSingleCandidate} Candidates: {Candidates}")]
  public struct Cell
  {
    // also determined by (Enum.GetValues(typeof (Candidates)).Length - 2);
    public static readonly ushort NumPossibleCellValues = 9;

    // bits 1 - 16 for candidate flags		
    // bits 17 - 24 used for value
    // bit 29 acts as HasValue boolean

    // HasValue    Value     Candidate Flags
    //    |      |+++++++| |+++++++++++++++++|
    // 0000 0000 0000 0000 0000 0000 0000 0000
    private int _data;

    /// <summary>
    ///   Creates new cell struct with specified candidate values
    /// </summary>
    /// <param name="candidates">Possible values for cell to hold</param>
    public Cell(Candidates candidates)
    {
      _data = (int)candidates;
    }

    /// <summary>
    ///   Creates instance of <see cref="Cell"/> with it's value pre-set
    /// </summary>
    /// <param name="value"><see cref="byte"/> value of the cell.</param>
    public Cell(byte value)
    {
      Debug.Assert(value > 0);
      _data = (1 << 28) + (value << 16);
    }

    /// <summary>
    ///   Cell's candidate values that are not violating sudoku rules
    /// </summary>
    public Candidates Candidates => (Candidates)_data;

    /// <summary>
    ///   Returns <see cref="bool"/> indicating whether cell holds some value or is empty
    /// </summary>
    public bool HasValue => _data >> 28 == 1;

    /// <summary>
    ///   Represents cell's actual value
    /// </summary>
    public byte Value
    {
      get { return (byte) (_data >> 16); }
      set
      {
        _data = 1 << 28;
        _data += value << 16;
      }
    }

    /// <summary>
    ///   Removes cell's candidate by it's <see cref="byte"/> value
    /// </summary>
    /// <param name="value">Value representation of a cell candidate</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of range of possible candidate values.</exception>
    public void RemoveValueFromCandidates(byte value)
    {
      if (value > NumPossibleCellValues || value < 1)
        throw new ArgumentOutOfRangeException(nameof(value));
      
      // HasValue    Value     Candidate Flags
      //    |      |+++++++| |+++++++++++++++++|
      // 0000 0000 0000 0000 0000 0000 0000 0000
      _data &= ~(ushort)(1 << (value - 1));
    }

    /// <summary>
    ///   Returns all remaining cell's candidates as array of byte values.
    /// </summary>
    /// <returns>
    ///   <see cref="T:byte[]"/> containing <see cref="Cell"/>'s <see cref="Core.Candidates"/> 
    ///   represented by their <see cref="byte"/> values.
    /// </returns>
    public byte[] PossibleValues()
    {
      var result = new List<byte>();
      var candidates = (ushort) _data;
      var counter = 1;
      while (candidates > 0)
      {
        if ((candidates & 1) == 1)
          result.Add((byte)(counter));

        candidates >>= 1;
        ++counter;
      }

      return result.ToArray();
    }

    /// <summary>
    ///   Checks whether <see cref="Cell.Candidates"/> property contains candidate represented by it's <see cref="byte"/> value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is out of range of possible candidate values</exception>
    public bool HasCandidate(byte value)
    {
      if (value > NumPossibleCellValues || value < 1)
        throw new ArgumentOutOfRangeException(nameof(value));

      var bitToCheck = (Candidates)(1 << (value - 1));
      return ((Candidates)_data & bitToCheck) == bitToCheck;
    }

    /// <summary>
    ///   Indicates whether cell has only one candidate left.
    /// <remarks>
    ///   If the cell has value set, returns <c>false</c>
    /// </remarks>
    /// </summary>
    public bool HasSingleCandidate
    {
      get
      {
        if (HasValue)
          return false;

        // Single flag is set when value is not 0 and at the same time power of two 
        return (_data != 0) && ((_data & (_data - 1)) == 0);
      }
    }

    /// <summary>
    ///   Returns <see cref="int"/> representing number of candidates for this cell.
    /// </summary>
    public int CandidateCount
    {
      get
      {
        var candidates = (Candidates) _data;
        var count = 0;
        while (candidates != Candidates.None)
        {
          candidates &= (candidates - 1);
          ++count;
        }
        return count;
      }
    }

    /// <summary>
    ///   Returns <see cref="Cell.Candidates"/> property represented by <see cref="byte"/> value.
    ///   If there are more than exactly one candidate remaining, returns the greatest one
    ///   and 0 if there are none left.
    /// </summary>
    /// <remarks>
    ///   <see cref="Cell.CandidateCount"/> should be used in conjunction with <see cref="Cell.GetCandidateAsValue"/>.
    /// </remarks>
    /// <returns><see cref="byte"/> value of the <see cref="Cell.Candidates"/> property</returns>
    public byte GetCandidateAsValue()
    {
      var candidates = (ushort) _data;
      byte value = 0;
      while (candidates > 0)
      {
        candidates >>= 1;
        ++value;
      }
      return value;
    } 
  }
}