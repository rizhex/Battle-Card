
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;
//La clase PowerData esta pensada para que a la hora de ejecutar los poderes de una carta determinada
//si esta tiene varios poderes, no se pueden lanzar uno detras de otro, porque algunos poderes no acaban
//hasta que el usuario termina de interactuar con ellos, por tanto se penso en que a la hora de que la clase 
//Power pase la informacion de los poderes, ir almacenandola en alguna estructura(lista, diccionario, etc) y 
//y luego garantizar que se ejecuten en order y sin llevarse a ninguno por delante. Cada subtipo que hereda
//de PowerData tiene propiedades especificas que serian las indicaciones que hay que llevar a cabo para que el
//poder funcione como se penso.
//Como propiedades generales, la clase power data, tiene un nombre, y algunos booleanos que serviran luego para
//saber en el "While(true)" si ya termino de ejecutarse el poder, o si ni siquiera ha empezado a ejecutarse.
public abstract class PowerData
{
    public abstract string name { get; protected set; }
    public abstract bool processing { get; set; }
    public abstract bool started { get; set; }
    public abstract int cardsCounter { get; set; }
}
public class RebornPower : PowerData
{
    public override string name { get; protected set; }
    public override bool processing { get; set; }
    public override bool started { get; set; }
    public override int cardsCounter { get; set; }
    public SpacePosition.location place;
    public bool select;
    public List<string> names;
    public List<int> identifier;


    public RebornPower(SpacePosition.location place, bool select, List<string> names, int cardsCounter)
    {
        this.started = false;
        this.name = "reborn";
        this.processing = true;
        this.place = place;
        this.select = select;
        this.names = names;
        this.cardsCounter = cardsCounter;
    }
}
public class SummonPower : PowerData
{
    public override string name { get; protected set; }
    public override bool processing { get; set; }

    public override bool started { get; set; }
    public override int cardsCounter { get; set; }
    public List<string> names;
    public bool summon;
    public bool select;


    public SummonPower(List<string> names, bool summon, bool select, int cardsCounter)
    {
        this.started = false;
        this.name = "summon";
        this.processing = true;
        this.select = select;
        this.cardsCounter = cardsCounter;
        this.names = names;
    }
}
public class SwitchBandPower : PowerData
{

    public override string name { get; protected set; }
    public override bool processing { get; set; }
    public override bool started { get; set; }
    public override int cardsCounter { get; set; }
    public string cardName;

    public SwitchBandPower(string cardName)
    {
        this.started = false;
        this.name = "switchBand";
        this.processing = true;
        this.cardName = cardName;
    }
}
public class DestroyPower : PowerData
{

    public override string name { get; protected set; }
    public override bool processing { get; set; }
    public override bool started { get; set; }
    public override int cardsCounter { get; set; }
    public int identifier;
    public SpacePosition.location from;
    public List<string> names;


    public DestroyPower(int identifier, SpacePosition.location from, List<string> names, int cardsCounter)
    {
        this.started = false;
        this.name = "destroy";
        this.processing = true;
        this.identifier = identifier;
        this.from = from;
        this.names = names;
        this.cardsCounter = cardsCounter;
    }
}
public class ModifyAttackPower : PowerData
{
    public override string name { get; protected set; }
    public override bool processing { get; set; }
    public override bool started { get; set; }
    public override int cardsCounter { get; set; }
    public SpacePosition.location where;
    public int ammount;
    public int identifier;
    public List<string> names;

    public ModifyAttackPower(SpacePosition.location where, int ammount, int identifier, int cardsCounter, List<string> names)
    {
        this.name = "modifyAttack";
        this.processing = true;
        this.started = false;
        this.ammount = ammount;
        this.cardsCounter = cardsCounter;
        this.names = names;
        this.identifier = identifier;
        this.where = where;
    }


}
public class DrawPower : PowerData
{
    public override string name { get; protected set; }
    public override bool processing { get; set; }
    public override bool started { get; set; }
    public override int cardsCounter { get; set; }

    public DrawPower(int cardsCounter)
    {
        this.name = "draw";
        this.processing = true;
        this.started = false;
        this.cardsCounter = cardsCounter;
    }
}
