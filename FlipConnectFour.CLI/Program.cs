using FlipConnectFour;
using System.Diagnostics;

// Connect 4 board
Board b = new(7, 6, 4);

// Each board can be in many terminal states:
// Win for One
// Win for Two
// Draw

string drawDir = "Draws";
string p1Wins = "P1_Wins";
string p2Wins = "P2_Wins";

void Clear(string s)
{
    if (!Directory.Exists(s))
        Directory.CreateDirectory(s);
    else
    {
        Directory.Delete(s, true);
        Directory.CreateDirectory(s);
    }
}
//Clear(drawDir);
//Clear(p1Wins);
//Clear(p2Wins);

Board.State Next(Board.State player) => player == Board.State.One ? Board.State.Two : Board.State.One;

long wins = 0;
long losses = 0;
long draws = 0;

void WriteBoardWin(Board.State winner, Board b)
{
    //var path = winner == Board.State.One ? p1Wins : p2Wins;
    //File.WriteAllText(Path.Combine(path, $"{b.Width}_{b.Height}_{b.Actions.Count}_{DateTime.UtcNow.Ticks}.txt"), b.ToString());
    if (winner == Board.State.One)
        wins++;
    else
        losses++;
    if (wins % 5000 == 0 || losses % 5000 == 0)
        Console.WriteLine($"{wins} - {losses}: {winner} Won in {b.Actions.Count} Actions!");
}

void WriteBoardDraw(Board b)
{
    //File.WriteAllText(Path.Combine(drawDir, $"{b.Width}_{b.Height}_{b.Actions.Count}_{DateTime.UtcNow.Ticks}.txt"), b.ToString());
    draws++;
    if (draws % 5000 == 0)
        Console.WriteLine($"{draws} - Draw in {b.Actions.Count} Actions!");
}

void MakeAction(Board toClone, Board.State player)
{
    // Try each legal action.
    // For each action we take, we need to let our opponent make each legal action
    // If our board ever gets to a state where one of us has won, we stop.
    int noPlaceCount = 0;
    for (int i = 0; i < toClone.Width; i++)
    {
        if (toClone.CanPlace(i))
        {
            // Only modify a copy of the original board.
            var b2 = new Board(toClone);
            if (b2.Place(player, i))
            {
                // We win!
                WriteBoardWin(player, b2);
            } else
            {
                // If we DO NOT win, let our opponent play the game.
                MakeAction(b2, Next(player));
            }
        } else
        {
            // Otherwise, we can't place a piece here.
            noPlaceCount++;
        }
    }
    if (toClone.CanFlip)
    {
        // If we can flip, attempt to flip and check who wins
        var b2 = new Board(toClone);
        var winner = b2.Flip();
        if (winner != Board.State.Empty)
        {
            // SOMEONE wins!
            WriteBoardWin(winner, b2);
        } else
        {
            // If no one wins, let our opponent play, they can't flip though.
            MakeAction(b2, Next(player));
        }
    } else if (noPlaceCount == toClone.Width)
    {
        // If we CANNOT flip AND we CANNOT place, we write a draw!
        WriteBoardDraw(toClone);
    }
    // Otherwise, we have iterated everything, so we are good to go.
}
var st = Stopwatch.StartNew();

void FirstPass()
{
    // Try each legal action.
    // For each action we take, we need to let our opponent make each legal action
    // If our board ever gets to a state where one of us has won, we stop.
    int noPlaceCount = 0;
    for (int i = 0; i < b.Width; i++)
    {
        if (b.CanPlace(i))
        {
            // Only modify a copy of the original board.
            var b2 = new Board(b);
            if (b2.Place(Board.State.One, i))
            {
                // We win!
                WriteBoardWin(Board.State.One, b2);
            }
            else
            {
                // If we DO NOT win, let our opponent play the game.
                MakeAction(b2, Board.State.Two);
                Console.WriteLine("============================================================");
                Console.WriteLine($"Completed an initial Branch! Running total: {st.Elapsed}!");
                Console.WriteLine("============================================================");
            }
        }
        else
        {
            // Otherwise, we can't place a piece here.
            noPlaceCount++;
        }
    }
    if (b.CanFlip)
    {
        // If we can flip, attempt to flip and check who wins
        var b2 = new Board(b);
        var winner = b2.Flip();
        if (winner != Board.State.Empty)
        {
            // SOMEONE wins!
            WriteBoardWin(winner, b2);
        }
        else
        {
            // If no one wins, let our opponent play, they can't flip though.
            MakeAction(b2, Board.State.Two);
        }
    }
    else if (noPlaceCount == b.Width)
    {
        // If we CANNOT flip AND we CANNOT place, we write a draw!
        WriteBoardDraw(b);
    }
    // Otherwise, we have iterated everything, so we are good to go.
}

FirstPass();
st.Stop();
Console.WriteLine($"Took: {st.Elapsed}");
Console.WriteLine($"Wins: {wins}");
Console.WriteLine($"Losses: {losses}");
Console.WriteLine($"Draws: {draws}");

//b.Place(Board.State.One, 0);
//b.Place(Board.State.Two, 0);
//// Should be true
//b.Place(Board.State.Two, 1);
//// Should be State.Two
//b.Flip();
//// Should still be State.Two
//b.Flip();
//// Should be true, since One can win with this move too.
//b.Place(Board.State.One, 1);
//// Should be State.Empty, since they should tie.
//b.Flip();