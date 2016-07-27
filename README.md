# Sudoku-solver-generator-CSharp

Simple Sudoku solver and generator written in c#

Currently supports 9x9 boards and multiple solutions.


```
# usage:
-r --raw          To solve board from command line
                  eg. -r=1.......9..67...2..8....4......75.3...5..2....6.3......9....8..6...4...1..25...6.
                  or  -r=800600905000000000000020310007318060240000073000000000002790100500080036003000000

-p --pretty       Pretty prints solutions (otherwise same format as in --raw input)

-x --max          Specifies maximum number of solutions to look for before exit

-b --benchmark    To run sample benchmark containing 95 very hard boards

-f --file         Solves sudokus from file
                  eg. -f=foo.bar or --file=c:\deadbeef
                  Each board must be on separate line and in the same format as needed for --raw
```


If opening in VS, use the `src/Sudoku.Windows/Sudoku.Windows.sln` file.