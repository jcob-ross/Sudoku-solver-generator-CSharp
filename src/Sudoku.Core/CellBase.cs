using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Core
{
  public abstract class CellBase
  {
    public virtual bool ReadOnly { get; set; } = false;
    public virtual int Value { get; set; } = 0;
    public virtual bool IsValid { get; set; } = true;
  }
}
