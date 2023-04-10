namespace TicTacToeApi;

public class BoardGame
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public DateTime? DateTime { get; set; }
    public int? TimeDuration { get; set; }
    public bool IsComplete { get; set; }
}