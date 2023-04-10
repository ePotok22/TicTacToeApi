namespace TicTacToeApi;

public class BoardResult
{
    public bool IsEndGame { get; set; }
    public bool IsDraw { get; set; }
    public string? Status { get; set; }  
    public string? NameWinner { get; set; }
}