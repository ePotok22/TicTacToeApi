using TicTacToeApi;
using TicTacToeApi.Database;
using Microsoft.EntityFrameworkCore;

public class MinimalApi
{
    private readonly static int[][] winningCombinations;

    static MinimalApi()
    {
        winningCombinations = new int[][] {
            new int[] {0, 1, 2},
            new int[] {3, 4, 5},
            new int[] {6, 7, 8},
            new int[] {0, 3, 6},
            new int[] {1, 4, 7},
            new int[] {2, 5, 8},
            new int[] {0, 4, 8},
            new int[] {2, 4, 6},
            };
    }

    private static bool StringIsNull(string val) =>
        string.IsNullOrWhiteSpace(val);

    private static string DateTimeToString(DateTime? dt, string format)
        => dt == null ? "n/a" : ((DateTime)dt).ToString(format);

    private static double CalculatorPercent(double a, double b) =>
        b == 0 ? 0 : Math.Round((a / b) * 100.0, 2);
    public static void Setup(WebApplication app)
    {
        app.Environment.ApplicationName = "Minimal";
        app.MapGet("/GetBoardGame", async (BoardGameDb db) =>
            await db.BoardGames.Where(_ => _.IsComplete).Select(_ => new
            {
                _.Id,
                _.IsComplete,
                _.Status,
                _.TimeDuration,
                DateTime = DateTimeToString(_.DateTime, "yyyy-MM-dd HH:mm:ss"),
            }).ToListAsync());

        app.MapGet("/GetBoardStatistics", async (BoardGameDb db) =>
        {
            List<BoardGame> getBoardGames = await db.BoardGames.Where(_ => _.IsComplete).ToListAsync();
            BoardStatistics temp = new BoardStatistics();
            temp.TimeoutPercent = CalculatorPercent(getBoardGames.Where(_ => _.Status.Equals("TIME-OUT")).Count(), getBoardGames.Count);
            temp.OWinPercent = CalculatorPercent(getBoardGames.Where(_ => _.Status.Equals("O-WIN")).Count(), getBoardGames.Count);
            temp.XWinPercent = CalculatorPercent(getBoardGames.Where(_ => _.Status.Equals("X-WIN")).Count(), getBoardGames.Count);
            temp.QuitPercent = CalculatorPercent(getBoardGames.Where(_ => _.Status.Equals("QUIT")).Count(), getBoardGames.Count);
            temp.DrawPercent = CalculatorPercent(getBoardGames.Where(_ => _.Status.Equals("DRAW")).Count(), getBoardGames.Count);
            return temp;
        });

        app.MapPost("NewGame", async (BoardGameDb db) =>
        {
            BoardGame temp = new BoardGame();
            List<BoardGame> getAll = await db.BoardGames.ToListAsync();
            temp.Id = getAll.Count + 1;
            temp.IsComplete = false;
            db.BoardGames.Add(temp);
            await db.SaveChangesAsync();
            return temp.Id;
        });

        app.MapPost("CheckWinner", async (BoardData data, BoardGameDb db) =>
        {
            BoardResult temp = new BoardResult();
            temp.Status = "-";
            temp.NameWinner = "-";
            temp.IsEndGame = false;
            temp.IsDraw = false;
            foreach (int[] item in winningCombinations)
            {
                int a = item[0];
                int b = item[1];
                int c = item[2];
                if (!StringIsNull(data.Score[a]) && data.Score[a] == data.Score[b] && data.Score[a] == data.Score[c])
                {
                    temp.Status = "WIN";
                    temp.NameWinner = data.Score[a];
                    temp.IsEndGame = true;
                    // Update
                    BoardGame findBoardGame = await db.BoardGames.FindAsync(keyValues: data.Id);
                    findBoardGame.Status = $"{data.Score[a]}-WIN".ToUpper();
                    findBoardGame.DateTime = DateTime.Now;
                    findBoardGame.TimeDuration = data.TimeDuration;
                    findBoardGame.IsComplete = true;
                    await db.SaveChangesAsync();
                    break;
                }
            }
            int countCell = data.Score.Where(_ => StringIsNull(_)).Count();
            if (countCell < 2 && !temp.IsEndGame)
            {
                temp.Status = "DRAW";
                temp.NameWinner = "-";
                temp.IsEndGame = true;
                temp.IsDraw = true;
                // Update
                BoardGame findBoardGame = await db.BoardGames.FindAsync(keyValues: data.Id);
                findBoardGame.Status = "DRAW";
                findBoardGame.DateTime = DateTime.Now;
                findBoardGame.TimeDuration = data.TimeDuration;
                findBoardGame.IsComplete = true;
                await db.SaveChangesAsync();
            }
            return temp;
        });

        app.MapPut(pattern: "{id}/QuitGame", async (int id, string timeDuration, BoardGameDb db) =>
        {
            if (await db.BoardGames.FindAsync(keyValues: id) is BoardGame findBoardGame)
            {
                findBoardGame.Status = "QUIT";
                findBoardGame.DateTime = DateTime.Now;
                findBoardGame.TimeDuration = int.Parse(timeDuration);
                findBoardGame.IsComplete = true;
                await db.SaveChangesAsync();
                return Results.Ok();
            }
            return Results.NotFound();
        });

        app.MapPut(pattern: "{id}/Timeout", async (int id, string timeDuration, BoardGameDb db) =>
        {
            if (await db.BoardGames.FindAsync(keyValues: id) is BoardGame findBoardGame)
            {
                findBoardGame.Status = "TIME-OUT";
                findBoardGame.DateTime = DateTime.Now;
                findBoardGame.TimeDuration = int.Parse(timeDuration);
                findBoardGame.IsComplete = true;
                await db.SaveChangesAsync();
                return Results.Ok();
            }
            return Results.NotFound();
        });
    }
}