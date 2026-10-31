#:property PublishAot=false

// BEFORE: flags to unwind nested loops. Same result as the C# 15 version, more noise.
//
// WHAT IT DOES
//   Scan a 2D grid for the value 9 (row by row, left to right).
//     - A negative cell is a "blocked" marker: abandon the rest of THAT ROW, go to the next row.
//     - When the target is found: stop scanning completely.
//   Plain break/continue only affect the innermost loop, so we need two extra bool flags
//   that both loop conditions must check.
//
// EXPECTED OUTPUT: found=(2, 2), visited=8

int[][] grid =
[
    [1, 2, 3],
    [4, -1, 6],   // blocked cell: skip the rest of this row
    [7, 8, 9],
];

int target = 9;
(int Row, int Col)? found = null;   // stays null if the target is never reached
int visited = 0;                    // counts inspected cells, proves the early exits work

bool done = false;                  // flag 1: tells the OUTER loop to stop (target found)
for (int r = 0; r < grid.Length && !done; r++)
{
    bool rowBlocked = false;        // flag 2: tells the INNER loop to stop (blocked cell), reset per row
    for (int c = 0; c < grid[r].Length && !rowBlocked; c++)
    {
        visited++;

        // Can't "continue" the outer loop from here: set a flag, then continue the inner
        // loop, whose condition sees the flag and ends it.
        if (grid[r][c] < 0) { rowBlocked = true; continue; }

        // Same trick: set the flag, break the inner loop; the outer condition then sees `done`.
        if (grid[r][c] == target) { found = (r, c); done = true; break; }
    }
}

Console.WriteLine($"found={found}, visited={visited}");
