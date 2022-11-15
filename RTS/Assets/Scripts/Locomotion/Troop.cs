using System.Collections.Generic;

public class Troop
{
    public List<Character> Characters = new List<Character>();
    public Character Leader => Characters[0];

    public Troop(Character character)
    {
        Characters.Add(character);
    }
    public Troop(List<Character> characters)
    {
        Characters.AddRange(characters);
    }
}
