using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

//La clase cards, es una clase abstracta, donde los subtipos UnitCard, EffectCard y LeaderCard heredaran de ella
//propiedades que son generales para todas las cartas del juego. Independientemente a eso, los subtipos tendran
//sus propiedades especificas que lo diferenciaran de los otros subtipos.
public abstract class Cards : AST_Root
{
    public abstract string name { get; protected set; }
    public abstract string type { get; protected set; }
    public abstract string imagePath { get; protected set; }
    public abstract List<Power> powers { get; protected set; }
    public abstract string band {get; set;}
    public abstract int identifier{get; set;}
    public enum location {Melee, Middle, Siege, Climate, Support}

}
public class UnitCard : Cards
{
    public override string name { get; protected set; }
    public override string type { get; protected set; }
    public override string imagePath { get; protected set; }
    public override string band { get; set; }
    public override int identifier { get; set; }
    public override List<Power> powers { get; protected set; }
    public string phrase { get; protected set; }
    public location position { get; set; }
    public ArithmeticExpressions damage { get; protected set; }
    public Dictionary<string,Expressions> External{get; private set;}
    public UnitCard(string Name, string Type, string ImagePath, List<Power> Powers, string Phrase, location Position , ArithmeticExpressions Damage)
    {
        this.name = Name;
        this.type = Type;
        this.imagePath = ImagePath;
        this.powers = Powers;
        this.phrase = Phrase;
        this.position = Position;
        this.damage = Damage;
    }
    public UnitCard(string Name, string Type, string ImagePath, List<Power> Powers, string Phrase, location Position , ArithmeticExpressions Damage, Dictionary<string, Expression> external)
    {
        this.name = Name;
        this.type = Type;
        this.imagePath = ImagePath;
        this.powers = Powers;
        this.phrase = Phrase;
        this.position = Position;
        this.damage = Damage;
        this.External = external;
    }
}
public class LeaderCard : Cards
{
    public override string name { get; protected set; }
    public override string type { get; protected set; }
    public override string imagePath { get; protected set; }
    public override string band { get; set; }
    public override List<Power> powers { get; protected set; }
    public string phrase { get; protected set; }
    public override int identifier { get; set; }
    public Dictionary<string, Expression> External{get; private set;}
    public LeaderCard(string Name, string Type, string ImagePath, List<Power> Powers, string Phrase)
    {
        this.name = Name;
        this.type = Type;
        this.imagePath = ImagePath;
        this.powers = Powers;
        this.phrase = Phrase;
    }
    public LeaderCard(string Name, string Type, string ImagePath, List<Power> Powers, string Phrase, Dictionary<string, Expression> external)
    {
        this.name = Name;
        this.type = Type;
        this.imagePath = ImagePath;
        this.powers = Powers;
        this.phrase = Phrase;
        this.External = external;
    }
}
public class EffectCard : Cards
{
    public override string name { get; protected set; }
    public override string type { get; protected set; }
    public override string imagePath { get; protected set; }
    public override string band { get; set; }
    public override List<Power> powers { get; protected set; }
    public location position { get; set; }
    public override int identifier { get; set; }
    public Dictionary<string, Expression> External{get; private set;}

    public EffectCard(string Name, string Type, string ImagePath, List<Power> Powers, location Position)
    {
        this.name = Name;
        this.type = Type;
        this.imagePath = ImagePath;
        this.powers = Powers;
        this.position = Position;
    }
    public EffectCard(string Name, string Type, string ImagePath, List<Power> Powers, location Position, Dictionary<string, Expression> external)
    {
        this.name = Name;
        this.type = Type;
        this.imagePath = ImagePath;
        this.powers = Powers;
        this.position = Position;
        this.External = external;
    }
}