﻿using System.Numerics;
using System.Threading;

FountainOfObjectsGame game;
Console.WriteLine("On what size of the map do you want do play?", ConsoleColor.Yellow);
Console.ForegroundColor = ConsoleColor.Cyan;
string? choice = Console.ReadLine();
switch (choice)
{
    case "small":
        game = CreateSmallGame();
        game.Run();
        break;
    case "medium":
        game = CreateMediumGame();
        game.Run();
        break;
    case "large":
        game = CreateLargeGame();
        game.Run();
        break;
}

// -------------------------------------------------------------------------------
//                                   Methods
// -------------------------------------------------------------------------------


// Creates a small 4x4 game.
FountainOfObjectsGame CreateSmallGame()
{
    Map map = new Map(4, 4);
    Location start = new Location(0, 0);
    Maelstrom maelstrom = new Maelstrom(new Location(0, 3));
    Amarok amarok = new Amarok(new Location(3, 2));
    map.SetRoomTypeAtLocation(start, RoomType.Entrance);
    map.SetRoomTypeAtLocation(new Location(0, 2), RoomType.Fountain);
    map.SetRoomTypeAtLocation(new Location(1, 3), RoomType.Pit);
    map.SetRoomTypeAtLocation(maelstrom.Location, RoomType.Maelstrom);
    map.SetRoomTypeAtLocation(amarok.Location, RoomType.Amarok);

    Monster[] monsters = new Monster[] { maelstrom, amarok };

    return new FountainOfObjectsGame(map, new Player(start), monsters);
}

FountainOfObjectsGame CreateMediumGame()
{
    Map map = new Map(6, 6);
    Location start = new Location(0, 0);
    Maelstrom maelstrom = new Maelstrom(new Location(3, 0));
    Amarok amarok = new Amarok(new Location(3, 2));
    Amarok amarok2 = new Amarok(new Location(4, 4));
    map.SetRoomTypeAtLocation(start, RoomType.Entrance);
    map.SetRoomTypeAtLocation(new Location(4, 2), RoomType.Fountain);
    map.SetRoomTypeAtLocation(new Location(1, 3), RoomType.Pit);
    map.SetRoomTypeAtLocation(new Location(3, 4), RoomType.Pit);
    map.SetRoomTypeAtLocation(maelstrom.Location, RoomType.Maelstrom);
    map.SetRoomTypeAtLocation(amarok.Location, RoomType.Amarok);
    map.SetRoomTypeAtLocation(amarok2.Location, RoomType.Amarok);

    Monster[] monsters = new Monster[] { maelstrom, amarok, amarok2 };

    return new FountainOfObjectsGame(map, new Player(start), monsters);
}

FountainOfObjectsGame CreateLargeGame()
{
    Map map = new Map(8, 8);
    Location start = new Location(0, 0);
    Maelstrom maelstrom = new Maelstrom(new Location(3, 0));
    Maelstrom maelstrom2 = new Maelstrom(new Location(5, 2));
    Amarok amarok = new Amarok(new Location(3, 2));
    Amarok amarok2 = new Amarok(new Location(4, 4));
    Amarok amarok3 = new Amarok(new Location(6, 3));
    map.SetRoomTypeAtLocation(start, RoomType.Entrance);
    map.SetRoomTypeAtLocation(new Location(6, 2), RoomType.Fountain);
    map.SetRoomTypeAtLocation(new Location(1, 3), RoomType.Pit);
    map.SetRoomTypeAtLocation(new Location(3, 4), RoomType.Pit);
    map.SetRoomTypeAtLocation(new Location(6, 6), RoomType.Pit);
    map.SetRoomTypeAtLocation(new Location(7, 5), RoomType.Pit);
    map.SetRoomTypeAtLocation(maelstrom.Location, RoomType.Maelstrom);
    map.SetRoomTypeAtLocation(maelstrom2.Location, RoomType.Maelstrom);
    map.SetRoomTypeAtLocation(amarok.Location, RoomType.Amarok);
    map.SetRoomTypeAtLocation(amarok2.Location, RoomType.Amarok);
    map.SetRoomTypeAtLocation(amarok3.Location, RoomType.Amarok);

    Monster[] monsters = new Monster[] { maelstrom, maelstrom2 };

    return new FountainOfObjectsGame(map, new Player(start), monsters);
}

// -------------------------------------------------------------------------------
//                                 Type definitions
// -------------------------------------------------------------------------------

// The beating heart of the Fountain of Objects game. Tracks the progression of a single
// round of gameplay.
public class FountainOfObjectsGame
{
    // The map being used by the game.
    public Map Map { get; }

    // The player playing the game.
    public Player Player { get; }

    // The list of monsters in the game.
    public Monster[] Monsters { get; }

    // Whether the player has turned on the fountain yet or not. (Defaults to `false`.)
    public bool IsFountainOn { get; set; }

    // A list of senses that the player can detect. Add to this collection in the constructor.
    private readonly ISense[] _senses;

    // Initializes a new game round with a specific map and player.
    public FountainOfObjectsGame(Map map, Player player, Monster[] monsters)
    {
        Map = map;
        Player = player;
        Monsters = monsters;

        // Each of these senses will be used during the game. Add new senses here.
        _senses = new ISense[]
        {
            new LightInEntranceSense(),
            new FountainSense(),
            new PitBreezeSense(),
            new MaelstromGrowlSense(),
            new AmarokSmellSense()
        };
    }

    // Runs the game one turn at a time.
    public void Run()
    {
        // This is the "game loop." Each turn runs through this `while` loop once.
        while (!HasWon && Player.IsAlive)
        {
            DisplayStatus();
            ICommand command = GetCommand();
            command.Execute(this);

            foreach (Monster monster in Monsters)
            {
                if (Player.Location == monster.Location && monster.IsAlive)
                    monster.Activate(this);
            }

            if (CurrentRoom == RoomType.Pit)
                Player.Kill("Fell into the pit.");
        }

        if (HasWon)
        {
            ConsoleHelper.WriteLine("The Fountain of Objects has been reactivated, and you have escaped with your life!", ConsoleColor.DarkGreen);
            ConsoleHelper.WriteLine("You win!", ConsoleColor.DarkGreen);
        }
        else
        {
            ConsoleHelper.WriteLine(Player.CauseOfDeath, ConsoleColor.Red);
            ConsoleHelper.WriteLine("You lost.", ConsoleColor.Red);
        }
    }

    // Displays the status to the player, including what room they are in and asks each sense to display itself
    // if it is currently relevant.
    private void DisplayStatus()
    {
        ConsoleHelper.WriteLine("--------------------------------------------------------------------------------", ConsoleColor.Gray);
        ConsoleHelper.WriteLine($"You are in the room at (Row={Player.Location.Row}, Column={Player.Location.Column}).", ConsoleColor.Gray);
        foreach (ISense sense in _senses)
            if (sense.CanSense(this))
                sense.DisplaySense(this);
    }

    // Gets an `ICommand` object that represents the player's desires.
    private ICommand GetCommand()
    {
        while (true) // Until we get a legitimate command, keep asking.
        {
            ConsoleHelper.Write("What do you want to do? ", ConsoleColor.White);
            Console.ForegroundColor = ConsoleColor.Cyan;
            string? input = Console.ReadLine();

            // Check for a match with each known command.
            if (input == "move north") return new MoveCommand(Direction.North);
            if (input == "move south") return new MoveCommand(Direction.South);
            if (input == "move east") return new MoveCommand(Direction.East);
            if (input == "move west") return new MoveCommand(Direction.West);
            if (input == "shoot north") return new ShootCommand(Shoot.North);
            if (input == "shoot south") return new ShootCommand(Shoot.South);
            if (input == "shoot east") return new ShootCommand(Shoot.East);
            if (input == "shoot west") return new ShootCommand(Shoot.West);
            if (input == "enable fountain") return new EnableFountainCommand();
            // More commands go here.
        }
    }

    // Indicates if the player has won or not.
    public bool HasWon => CurrentRoom == RoomType.Entrance && IsFountainOn;

    // Looks up what room type the player is currently in.
    public RoomType CurrentRoom => Map.GetRoomTypeAtLocation(Player.Location);
}

// Represents the map and what each room is made out of.
public class Map
{
    // Stores which room type each room in the world is. The default is `Normal` because that is the first
    // member in the enumeration list.
    private readonly RoomType[,] _rooms;

    // The total number of rows in this specific game world.
    public int Rows { get; }

    // The total number of columns in this specific game world.
    public int Columns { get; }


    // Creates a new map with a specific size.
    public Map(int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        _rooms = new RoomType[rows, columns];
    }

    // Returns what type a room at a specific location is.
    public RoomType GetRoomTypeAtLocation(Location location) => IsOnMap(location) ? _rooms[location.Row, location.Column] : RoomType.OffTheMap;

    // Determines if a neighboring room is of the given type.
    public bool HasNeighborWithType(Location location, RoomType roomType)
    {
        if (GetRoomTypeAtLocation(new Location(location.Row - 1, location.Column - 1)) == roomType) return true;
        if (GetRoomTypeAtLocation(new Location(location.Row - 1, location.Column)) == roomType) return true;
        if (GetRoomTypeAtLocation(new Location(location.Row - 1, location.Column + 1)) == roomType) return true;
        if (GetRoomTypeAtLocation(new Location(location.Row, location.Column - 1)) == roomType) return true;
        if (GetRoomTypeAtLocation(new Location(location.Row, location.Column)) == roomType) return true;
        if (GetRoomTypeAtLocation(new Location(location.Row, location.Column + 1)) == roomType) return true;
        if (GetRoomTypeAtLocation(new Location(location.Row + 1, location.Column - 1)) == roomType) return true;
        if (GetRoomTypeAtLocation(new Location(location.Row + 1, location.Column)) == roomType) return true;
        if (GetRoomTypeAtLocation(new Location(location.Row + 1, location.Column + 1)) == roomType) return true;
        return false;
    }

    // Indicates whether a specific location is actually on the map or not.
    public bool IsOnMap(Location location) =>
        location.Row >= 0 &&
        location.Row < _rooms.GetLength(0) &&
        location.Column >= 0 &&
        location.Column < _rooms.GetLength(1);

    // Changes the type of room at a specific spot in the world to a new type.
    public void SetRoomTypeAtLocation(Location location, RoomType type) => _rooms[location.Row, location.Column] = type;
}

// Represents a location in the 2D game world, based on its row and column.
public record Location(int Row, int Column);

// Represents the player in the game.
public class Player
{
    // The player's current location.
    public Location Location { get; set; }

    // Indicates whether the player is alive or not.
    public bool IsAlive { get; private set; } = true;

    // Explains why a player died. Empty string until death.
    public string CauseOfDeath { get; private set; } = "";

    // Creates a new player that starts at the given location.
    public Player(Location start) => Location = start;

    public void Kill(string cause)
    {
        IsAlive = false;
        CauseOfDeath = cause;
    }
}
/// <summary>
/// Represents one of the several monster types in the game.
/// </summary>
public abstract class Monster
{
    // The monster's current location.
    public Location Location { get; set; }

    // Whether the monster is alive or not.
    public bool IsAlive { get; set; } = true;

    // Creates a monster at the given location.
    public Monster(Location start) => Location = start;

    // Called when the monster and the player are both in the same room. Gives
    // the monster a chance to do its thing.
    public abstract void Activate(FountainOfObjectsGame game);

    public void Kill(string cause)
    {
        IsAlive = false;
        ConsoleHelper.WriteLine(cause, ConsoleColor.Green);
    }
}

public class Maelstrom : Monster
{
    // Creates a monster at the given location.
    public Maelstrom(Location start) : base(start)
    {

    }

    // Called when the monster and the player are both in the same room. Gives
    // the monster a chance to do its thing.
    public override void Activate(FountainOfObjectsGame game)
    {
        if ((game.Player.Location.Row - 1 == -1))
        {
            if (game.Player.Location.Column + 2 == game.Map.Columns)
                game.Player.Location = new Location(game.Map.Rows - 1, 0);
            else if (game.Player.Location.Column + 2 == game.Map.Columns + 1)
                game.Player.Location = new Location(game.Map.Rows - 1, 1);
            else
                game.Player.Location = new Location(game.Player.Location.Row - 1, game.Player.Location.Column + 2);
        }

        if ((this.Location.Row + 1 == game.Map.Rows))
        {
            if (this.Location.Column - 2 == -1)
                this.Location = new Location(0, game.Map.Columns - 1);
            else if (this.Location.Column - 2 == -2)
                this.Location = new Location(0, game.Map.Columns - 2);
            else
                this.Location = new Location(this.Location.Row + 1, this.Location.Column - 2);
        }
    }
}

public class Amarok : Monster
{
    // Creates a monster at the given location.
    public Amarok(Location start) : base(start)
    {

    }

    // Called when the monster and the player are both in the same room. Gives
    // the monster a chance to do its thing.
    public override void Activate(FountainOfObjectsGame game)
    {
        game.Player.Kill("Killed by the amarok.");
    }
}

// An interface to represent one of many commands in the game. Each new command should
// implement this interface.
public interface ICommand
{
    void Execute(FountainOfObjectsGame game);
}

// Represents a movement command, along with a specific direction to move.
public class MoveCommand : ICommand
{
    // The direction to move.
    public Direction Direction { get; }

    // Creates a new movement command with a specific direction to move.
    public MoveCommand(Direction direction)
    {
        Direction = direction;
    }

    // Causes the player's position to be updated with a new position, shifted in the intended direction,
    // but only if the destination stays on the map. Otherwise, nothing happens.
    public void Execute(FountainOfObjectsGame game)
    {
        Location currentLocation = game.Player.Location;
        Location newLocation = Direction switch
        {
            Direction.North => new Location(currentLocation.Row - 1, currentLocation.Column),
            Direction.South => new Location(currentLocation.Row + 1, currentLocation.Column),
            Direction.West => new Location(currentLocation.Row, currentLocation.Column - 1),
            Direction.East => new Location(currentLocation.Row, currentLocation.Column + 1)
        };

        if (game.Map.IsOnMap(newLocation))
            game.Player.Location = newLocation;
        else
            ConsoleHelper.WriteLine("There is a wall there.", ConsoleColor.Red);
    }
}

public class ShootCommand : ICommand
{
    public Shoot ShootDirection { get; }
    public ShootCommand(Shoot shootDirection)
    {
        ShootDirection = shootDirection;
    }


    public void Execute(FountainOfObjectsGame game)
    {
        Location currentLocation = game.Player.Location;
        Location shootLocation = ShootDirection switch
        {
            Shoot.North => new Location(currentLocation.Row - 1, currentLocation.Column),
            Shoot.South => new Location(currentLocation.Row + 1, currentLocation.Column),
            Shoot.West => new Location(currentLocation.Row, currentLocation.Column - 1),
            Shoot.East => new Location(currentLocation.Row, currentLocation.Column + 1)
        };

        foreach(Monster monster in game.Monsters)
        {
            if(shootLocation == monster.Location)
            {
                monster.Kill($"You killed a {monster.ToString()}");
                game.Map.SetRoomTypeAtLocation(monster.Location, RoomType.Normal);
            }
        }
    }
}

// A command that represents a request to turn the fountain on.
public class EnableFountainCommand : ICommand
{
    // Turns the fountain on if the player is in the room with the fountain. Otherwise, nothing happens.
    public void Execute(FountainOfObjectsGame game)
    {
        if (game.Map.GetRoomTypeAtLocation(game.Player.Location) == RoomType.Fountain) game.IsFountainOn = true;
        else ConsoleHelper.WriteLine("The fountain is not in this room. There was no effect.", ConsoleColor.Red);
    }
}

// Represents one of the four directions of movement.
public enum Direction { North, South, West, East }

// Represents one of the four directions of shooting.
public enum Shoot { North, South, West, East }


// Represents something that the player can sense as they wander the caverns.
public interface ISense
{
    // Returns whether the player should be able to sense the thing in question.
    bool CanSense(FountainOfObjectsGame game);

    // Displays the sensed information. (Assumes `CanSense` was called first and returned `true`.)
    void DisplaySense(FountainOfObjectsGame game);
}

// Represents the player's ability to sense the light in the entrance room.
public class LightInEntranceSense : ISense
{
    // Returns `true` if the player is in the entrance room.
    public bool CanSense(FountainOfObjectsGame game) => game.Map.GetRoomTypeAtLocation(game.Player.Location) == RoomType.Entrance;

    // Displays the appropriate message if the player can see the light from outside the caverns.
    public void DisplaySense(FountainOfObjectsGame game) => ConsoleHelper.WriteLine("You see light in this room coming from outside the cavern. This is the entrance.", ConsoleColor.Yellow);
}

// Represents the player's ability to sense the dripping of a deactivated fountain or rushing waters of an activated fountain.
public class FountainSense : ISense
{
    // Returns `true` if the player is in the fountain room.
    public bool CanSense(FountainOfObjectsGame game) => game.Map.GetRoomTypeAtLocation(game.Player.Location) == RoomType.Fountain;

    // Displays the appropriate message depending on whether the fountain is enabled or disabled.
    public void DisplaySense(FountainOfObjectsGame game)
    {
        if (game.IsFountainOn)
            ConsoleHelper.WriteLine("You hear the rushing waters from the Fountain of Objects. It has been reactivated!", ConsoleColor.DarkCyan);
        else
            ConsoleHelper.WriteLine("You hear water dripping in this room. The Fountain of Objects is here!", ConsoleColor.DarkCyan);
    }
}

public class PitBreezeSense : ISense
{
    // Returns `true` if the player is next to (including diagonally) a pit.
    public bool CanSense(FountainOfObjectsGame game) => game.Map.HasNeighborWithType(game.Player.Location, RoomType.Pit);

    // Displays the appropriate message to warn the player.
    public void DisplaySense(FountainOfObjectsGame game) => ConsoleHelper.WriteLine("You feel a draft. There is a pit in a nearby room.", ConsoleColor.DarkGray);

}

public class MaelstromGrowlSense : ISense
{
    // Returns `true` if the player is next to (including diagonally) a pit.
    public bool CanSense(FountainOfObjectsGame game) => game.Map.HasNeighborWithType(game.Player.Location, RoomType.Maelstrom);

    // Displays the appropriate message to warn the player.
    public void DisplaySense(FountainOfObjectsGame game) => ConsoleHelper.WriteLine("You hear the growling and groaning of a maelstrom nearby.",
        ConsoleColor.DarkGray);
}

public class AmarokSmellSense : ISense
{
    public bool CanSense(FountainOfObjectsGame game) => game.Map.HasNeighborWithType(game.Player.Location, RoomType.Amarok);

    public void DisplaySense(FountainOfObjectsGame game) => ConsoleHelper.WriteLine("You can smell the rotten stench of an amarok in a nearby room",
        ConsoleColor.DarkGray);
}

// A collection of helper methods for writing text to the console using specific colors.
public static class ConsoleHelper
{
    // Changes to the specified color and then displays the text on its own line.
    public static void WriteLine(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
    }

    // Changes to the specified color and then displays the text without moving to the next line.
    public static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
    }
}

// Represents one of the different types of rooms in the game.
public enum RoomType { Normal, Entrance, Fountain, OffTheMap, Pit, Maelstrom, Amarok }

