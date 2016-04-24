namespace Sudoku.Core
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;

  /// <summary>
  ///   Represents sudoku board
  /// </summary>
  public class Board : IEnumerable<Cell>, IEquatable<Board>
  {
    private Cell[] _cells;
    private readonly int _width;
    private readonly int _sectorWidth;
    private readonly int _length;

    /// <summary>
    ///   Initializes new instance of 9x9 <see cref="Board"/>
    ///   where all cells are empty and have all <see cref="Cell.Candidates"/> set.
    /// </summary>
    public Board()
    {
      const int width = 9;

      _width = width;
      _sectorWidth = 3;
      _length = width*width;

      _cells = Enumerable.Repeat(new Cell(Candidates.All), width * width).ToArray();
    }

    private Board(int width = 9, int sectorWidth = 3, int length = 81)
    {
      _width = width;
      _sectorWidth = sectorWidth;
      _length = length;
    }

    /// <summary>
    ///   Initializes new instance of <see cref="Board"/>
    /// </summary>
    /// <remarks>
    ///   Parameter values are not checked.
    /// </remarks>
    /// <param name="cells">Array of cells</param>
    /// <param name="width">Size of board's side</param>
    /// <param name="sectorWidth">Size of the board's sector</param>
    /// <param name="length">Number of cells</param>
    public Board(Cell[] cells, int width = 9, int sectorWidth = 3, int length = 81)
    { 
      _width = width;
      _sectorWidth = sectorWidth;
      _length = length;
      _cells = cells;
    }

    /// <summary>
    ///   Array of <see cref="Cell"/>s on the board.
    /// </summary>
    public Cell[] Cells => _cells;

    /// <summary>
    ///   Two-dimensional indexer where coordinates [0, 0] indicate first cell on the board and [8, 8] the last one.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException" accessor="get">
    ///   Specified <paramref name="x"/> -or- <paramref name="y"/> is smaller than 0 -or- greater than <see cref="Board.Width"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException" accessor="set">
    ///   Specified <paramref name="x"/> -or- <paramref name="y"/> is smaller than 0 -or- greater than <see cref="Board.Width"/>.
    /// </exception>
    public Cell this[int x, int y]
    {
      get
      {
        if (x > _width - 1 || x < 0)
          throw new ArgumentOutOfRangeException(nameof(x));

        if (y > _width - 1 || y < 0)
          throw new ArgumentOutOfRangeException(nameof(y));

        return _cells[y * _width + x];
      }
      set
      {
        if (x > _width - 1 || x < 0)
          throw new ArgumentOutOfRangeException(nameof(x));

        if (y > _width - 1 || y < 0)
          throw new ArgumentOutOfRangeException(nameof(y));

        _cells[y * _width + x] = value;
      }
    }

    /// <summary>
    ///   Width of the board side. (e.g. 9 for 9x9 board)
    /// </summary>
    public int Width => _width;

    /// <summary>
    ///   Width of the sector on the board (e.g. 3 for 9x9 board)
    /// </summary>
    public int SectorWidth => _sectorWidth;

    /// <summary>
    ///   Total number of squares on the board
    /// </summary>
    public int Length => _length;

    /// <summary>
    ///   Retrieves peer cell indexes of a given <see cref="Cell"/>.
    /// </summary>
    /// <remarks>
    ///   Peer is a square that is in the same row, column or box.
    /// </remarks>
    /// <param name="cellIndex">Index of a <see cref="Cell"/> which peers are to be retrieved</param>
    /// <returns><see cref="T:int[]"/> of cell indexes</returns>
    public int[] GetPeers(int cellIndex)
    {
      if (!PeerIndexCache.ContainsKey(cellIndex))
        PeerIndexCache.Add(cellIndex, Enumerable.Range(0, _width * _width).Where(cell => IsPeer(cellIndex, cell)).ToArray());

      return PeerIndexCache[cellIndex];
    }
    
    // Index cache - for every cell, that cell's peers are always the same
    // In case of multiple instances of boards of differents sizes this kind of caching won't work
    private static readonly Dictionary<int, int[]> PeerIndexCache = new Dictionary<int, int[]>(81);

    /// <summary>
    ///   Places a <paramref name="value"/> in a <see cref="Cell"/> and removes that value from <see cref="Cell.Candidates"/> of peer cells.
    /// </summary>
    /// <param name="cellIndex">Index of a <see cref="Cell"/> to be modified.</param>
    /// <param name="value"><see cref="byte"/> value to be placed in a <see cref="Cell"/></param>
    /// <returns><see cref="Board"/> after the placement of <paramref name="value"/></returns>
    public Board PlaceAndApplyRules(int cellIndex, byte value)
    {
      var board = (Board)this.Clone();

      board._cells[cellIndex] = new Cell(value);

      var peers = GetPeers(cellIndex);
      for (var i = 0; i < peers.Length; ++i)
      {
        var peerCell = board._cells[peers[i]];
        peerCell.RemoveValueFromCandidates(value);

        if (! peerCell.HasValue && peerCell.Candidates == Candidates.None)
          return null;

        board._cells[peers[i]] = peerCell;
      }
      return board;
    }

    public bool IsValid()
    {
      // for every cell
      //  if the cell has value
      //    for every peer of that cell
      //      if peer has that value -or- if peer has that value among it's candidates
      //        board is not valid

      //  if the cell does NOT have value
      //    for every candidate of that cell
      //      if peer cell has that value
      //        board is not valid
      //
      //  board must be valid at this point

      for (var i = 0; i < _length; ++i)
      {
        if (_cells[i].HasValue)
        {
          var peers = GetPeers(i);
          for (var j = 0; j < peers.Length; ++j)
          {
            if ((_cells[peers[i]].HasValue && _cells[peers[i]].Value == _cells[i].Value) || _cells[peers[i]].HasCandidate(_cells[i].Value))
              return false;
          }
        }
        else
        {
          var peers = GetPeers(i);
          for (var j = 0; j < peers.Length; ++j)
          {
            var possibleVals = _cells[i].PossibleValues();
            for (var k = 0; k < possibleVals.Length; ++k)
            {
              if (_cells[peers[j]].HasValue && _cells[peers[j]].Value == possibleVals[k])
                return false;
            }
          }
        }
      }
      return true;
    }

    /// <summary>
    ///   Checks if a specified square indexes are each other's peers. 
    /// </summary>
    /// <remarks>
    ///   Peer is a square that is in the same row, column or box.
    /// </remarks>
    /// <param name="indexLeft">Square's index</param>
    /// <param name="indexRight">Square's index</param>
    /// <returns><c>True</c> if squares are peers, <c>False</c> otherwise.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is smaller than 0 -or- greater than <see cref="Board.Length"/>.</exception>
    public bool IsPeer(int indexLeft, int indexRight)
    {
      if (indexLeft < 0 || indexLeft > _length - 1)
        throw new ArgumentOutOfRangeException(nameof(indexLeft));
      if (indexRight < 0 || indexRight > _length - 1)
        throw new ArgumentOutOfRangeException(nameof(indexRight));

      if (indexLeft == indexRight)
        return false;

      // row
      if (indexLeft / Width == indexRight / Width)
        return true;

      // column
      if (indexLeft % Width == indexRight % Width)
        return true;

      // box
      if ((indexLeft / Width / SectorWidth == indexRight / Width / SectorWidth) &&
        (indexLeft % Width / SectorWidth == indexRight % Width / SectorWidth))
        return true;

      return false;
    }

    /// <summary>
    ///   Returns an <see cref="IEnumerator{Cell}"/> that iterates through the cells on the <see cref="Board"/>.
    /// </summary>
    /// <returns><see cref="IEnumerator{Cell}"/></returns>
    public IEnumerator<Cell> GetEnumerator()
    {
      return (IEnumerator<Cell>)_cells.GetEnumerator();
    }

    /// <summary>
    ///   Returns an <see cref="IEnumerator{Cell}"/> that iterates through the cells on the <see cref="Board"/>.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    ///   Compares individual square contents of two instances of <see cref="Board"/>
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <see langword="null" />.</exception>
    public bool Equals(Board other)
    {
      if (null == other)
        throw new ArgumentNullException(nameof(other));

      if (_width != other.Width || _sectorWidth != other.SectorWidth || _length != other.Length)
        return false;

      for (var i = 0; i < _length; ++i)
      {
        if (_cells[i].Value != other.Cells[i].Value)
          return false;
      }
      return true;
    }

    public object Clone()
    {
      var clone = new Board(_width, _sectorWidth, _length);
      clone._cells = new Cell[_length];
      Array.Copy(_cells, clone._cells, _length);
      return clone;
    }
  }
}