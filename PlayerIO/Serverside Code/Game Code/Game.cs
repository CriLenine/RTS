using PlayerIO.GameLibrary;

public class Player : BasePlayer
{

}

[RoomType("Game")]
public class GameRoom : Game<Player>
{

}