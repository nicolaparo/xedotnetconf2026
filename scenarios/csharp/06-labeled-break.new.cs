#:property PublishAot=false

// AFTER (C# 15): label the outer loop, then break / continue it directly from the inner loop.
// On the .NET 10 SDK (C# 14) this does not compile.
//
// WHAT IT DOES
//   Scan a 2D grid for the value 9 (row by row, left to right).
//     - A negative cell is a "blocked" marker: abandon the rest of THAT ROW, go to the next row.
//     - When the target is found: stop scanning completely.
//   Both jumps leave the INNER loop and act on the OUTER one, which plain break/continue cannot do
//   (they only affect the innermost loop).
//
// EXPECTED OUTPUT: found=(2, 2), visited=8
//   row 0: visits 1,2,3            -> 3 cells
//   row 1: visits 4, then -1       -> 2 cells, `continue rows` skips the 6
//   row 2: visits 7,8,9 -> found   -> 3 cells, `break rows` ends everything
//   total 3 + 2 + 3 = 8

int[][] grid =
[
    [1, 2, 3],
    [4, -1, 6],   // blocked cell: skip the rest of this row
    [7, 8, 9],
];

int target = 9;
(int Row, int Col)? found = null;   // stays null if the target is never reached
int visited = 0;                    // counts inspected cells, proves the early exits work

rows: for (int r = 0; r < grid.Length; r++)     // `rows:` is the label, it names this loop
{
    for (int c = 0; c < grid[r].Length; c++)
    {
        visited++;

        // Next iteration of the OUTER loop (jump to r++), not of the inner one.
        if (grid[r][c] < 0) continue rows;

        // Exit the OUTER loop entirely. A plain `break` would only leave the inner loop
        // and the scan would carry on with the next row.
        if (grid[r][c] == target) { found = (r, c); break rows; }
    }
}

Console.WriteLine($"found={found}, visited={visited}");
