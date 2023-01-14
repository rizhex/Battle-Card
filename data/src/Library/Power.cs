using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using Godot;

public class PowerSet:Expressions
{
    public List<Power> List = new List<Power>();
    public string Name{get; private set;}
    public override ExpressionType Type 
    { 
        get => ExpressionType.power;
        set => this.Type = ExpressionType.power;
    }
    public override object Value 
    { 
        get => List; 
        set => Evaluate(); 
    }
    public override void Evaluate()
    {
        
    }
    public PowerSet(string name, IEnumerable<Power> list)
    {
        this.Name = name;
        this.List = (List<Power>)list;
    }
}
public class Power 
{

    public string Name { get; private set; }
    
    List<Condition> Conditions = new List<Condition>();
    List<Instruction> Instructions = new List<Instruction>();
    public Power(string name, List<Condition> conditions, List<Instruction> instructions)
    {
        this.Name = name;
        this.Conditions = conditions;
        this.Instructions = instructions;
    }
    public bool Check()
    {
        foreach (var condition in Conditions)
        {
            if (!condition.EvaluateCondition()) return false;
        }
        return true;
    }
    public void Execute()
    {
        GameHUD.powerData = new List<PowerData>();

        if (Check())
        {
            foreach (var instruction in Instructions)
            {
                Process.Execute(instruction.Name, instruction.Commands);
            }

            GameHUD.startProcessing = true;
            if (GameHUD.phase == GameHUD.Phase.PlayerTurn) GameHUD.phase = GameHUD.Phase.PlayerWaiting;
            else if (GameHUD.phase == GameHUD.Phase.EnemyTurn) GameHUD.phase = GameHUD.Phase.EnemyWaiting;

        }else{
            if (GameHUD.phase == GameHUD.Phase.PlayerTurn) GameHUD.phase = GameHUD.Phase.PlayerWaiting;
            else if (GameHUD.phase == GameHUD.Phase.EnemyTurn) GameHUD.phase = GameHUD.Phase.EnemyWaiting;
        }

        
    }
}

public class Condition
{
    public BooleanExpresion Exp { get; }
    public Condition(BooleanExpresion exp)
    {
        this.Exp = exp;
    }
    public bool EvaluateCondition()
    {
        this.Exp.Evaluate();
        return (bool)this.Exp.Value;
    }
}
public class Instruction
{
    public string Name { get; }
    public IEnumerable<object> Commands { get; }
    public Instruction(string name, IEnumerable<object> commands)
    {
        this.Name = name;
        this.Commands = commands;
    }
}
public static class Process
{
    private static void Reborn(IEnumerable<object> indications)
    {
        SpacePosition.location place = SpacePosition.location.playerGrave;
        List<string> names = new List<string>();
        bool reborn;
        bool select = false;
        int cardsCounter = 0;
        List<object> Indications = indications.ToList();
        if (Indications[0] is MethodInfo)
        {
            MethodInfo method = (MethodInfo)Indications[0];
            switch (method.Name)
            {
                case "OwnGraveryard":
                    if (GameHUD.phase != GameHUD.Phase.PlayerTurn) place = SpacePosition.location.enemyGrave;
                    else place = SpacePosition.location.playerGrave;
                    break;
                case "EnemyGraveryard":
                    if (GameHUD.phase != GameHUD.Phase.PlayerTurn) place = SpacePosition.location.playerGrave;
                    else place = SpacePosition.location.enemyGrave;
                    break;
            }


        }

        if (Indications[1] is IEnumerable<string>)
        {
            names = (List<string>)Indications[1];
            reborn = true;
            select = false;
        }
        else if(Indications[1] is ArithmeticExpressions)
        {
            select = true;
            ArithmeticExpressions current = (ArithmeticExpressions)Indications[1];
            current.Evaluate();
            cardsCounter = (int)current.Value;
        }
        else
        {
            select = true;
            cardsCounter = (int)Indications[1];
        }

        GameHUD.powerData.Add(new RebornPower(place, select, names, cardsCounter));
    }
    private static void Summon(IEnumerable<object> indications)
    {
        List<string> names = new List<string>();
        bool summon;
        bool select;
        int cardsCounter = 0;
        List<object> Indications = indications.ToList();

        if (Indications[0] is IEnumerable<string>)
        {
            GD.Print("Ejuna lista");
            names = (List<string>)Indications[0];
            summon = true;
            select = false;
            foreach(var item in names){
                GD.Print(item);
            }
        }
        else if(Indications[0] is ArithmeticExpressions)
        {
            ArithmeticExpressions current = (ArithmeticExpressions)Indications[0];
            current.Evaluate();
            select = true;
            summon = true;
            cardsCounter = (int)current.Value;
        }
        else
        {
            GD.Print("Ejun numero");
            select = true;
            summon = true;
            cardsCounter = (int)Indications[0];
        }
        GameHUD.powerData.Add(new SummonPower(names, summon, select, cardsCounter));
    }
    private static void SwitchBand(IEnumerable<object> indications)
    {

        string cardName = "";
        List<object> Indications = indications.ToList();

        if (Indications[0] is string) cardName = (string)Indications[0];

        GameHUD.powerData.Add(new SwitchBandPower(cardName));

    }
    private static void Destroy(IEnumerable<object> indications)
    {
        List<string> names = new List<string>();
        int identifier = 0;
        SpacePosition.location from = 0;
        int cardsCounter = 0;
        List<object> Indications = indications.ToList();
        if (Indications[0] is MethodInfo)
        {
            MethodInfo method = (MethodInfo)Indications[0];
            switch (method.Name)
            {
                case "OwnMelee":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) from = SpacePosition.location.playerMelee;
                    else from = SpacePosition.location.enemyMelee;
                    break;
                case "OwnMiddle":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) from = SpacePosition.location.playerMiddle;
                    else from = SpacePosition.location.enemyMiddle;
                    break;
                case "OwnSiege":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) from = SpacePosition.location.playerSiege;
                    else from = SpacePosition.location.enemySiege;
                    break;
                case "EnemyMelee":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) from = SpacePosition.location.enemyMelee;
                    else from = SpacePosition.location.playerMelee;
                    break;
                case "EnemyMiddle":
                    if(GameHUD.phase == GameHUD.Phase.PlayerTurn) from = SpacePosition.location.enemyMiddle;
                    else from = SpacePosition.location.playerMiddle;
                    break;
                case "EnemySiege":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) from = SpacePosition.location.enemySiege;
                    else from = SpacePosition.location.playerSiege;
                    break;
            }
            identifier = 0;
        }
        else if (Indications[0] is IEnumerable<string>)
        {
            names = (List<string>)Indications[0];
            identifier = 1;
        }
        else if (Indications[0] is int)
        {
            cardsCounter = (int)Indications[0];
            identifier = 2;
        }

        GameHUD.powerData.Add(new DestroyPower(identifier, from, names, cardsCounter));
    }
    private static void ModifyAttack(IEnumerable<object> indications)
    {
        List<object> Indications = (List<object>)indications;
        SpacePosition.location where = 0;
        int ammount = 0;
        int identifier = 0;
        if (Indications[0] is ArithmeticExpressions)
        {
            ArithmeticExpressions current = (ArithmeticExpressions)Indications[0];
            current.Evaluate();
            ammount = (int)current.Value;
        }
        else ammount = (int)Indications[0];
        int cardsCounter = 0;
        List<string> names = new List<string>();

        if (Indications[1] is MethodInfo)
        {
            identifier = 0;
            MethodInfo method = (MethodInfo)Indications[1];
            switch (method.Name)
            {
                case "OwnMelee":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) where = SpacePosition.location.playerMelee;
                    else where = SpacePosition.location.enemyMelee;
                    break;
                case "OwnMiddle":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) where = SpacePosition.location.playerMiddle;
                    else where = SpacePosition.location.enemyMiddle;
                    break;
                case "OwnSiege":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) where = SpacePosition.location.playerSiege;
                    else where = SpacePosition.location.enemySiege;
                    break;
                case "EnemyMelee":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) where = SpacePosition.location.enemyMelee;
                    else where = SpacePosition.location.playerMelee;
                    break;
                case "EnemyMiddle":
                    if(GameHUD.phase == GameHUD.Phase.PlayerTurn) where = SpacePosition.location.enemyMiddle;
                    else where = SpacePosition.location.playerMiddle;
                    break;
                case "EnemySiege":
                    if (GameHUD.phase == GameHUD.Phase.PlayerTurn) where = SpacePosition.location.enemySiege;
                    else where = SpacePosition.location.playerSiege;
                    break;
            }

        }
        else if (Indications[1] is IEnumerable<string>)
        {
            identifier = 1;
            names = (List<string>)Indications[1];
        }
        else if (Indications[1] is int)
        {
            identifier = 2;
            cardsCounter = (int)Indications[1];
        }
        else if (Indications[1] == TokenValues.freeelection)
        {
            GD.Print("Esto es un cuerno");
            identifier = 3;
        }
        else
        {
            identifier = 2;
            ArithmeticExpressions current = (ArithmeticExpressions)Indications[0];
            current.Evaluate();
            cardsCounter = (int)current.Value;
        }

        GameHUD.powerData.Add(new ModifyAttackPower(where, ammount, identifier, cardsCounter, names));
    }
    private static void Draw(IEnumerable<object> indications)
    {
        int cardsCounter = 0;
        List<object> Indications = (List<object>)indications;
        if (Indications[0] is int) cardsCounter = (int)Indications[0];
        else if (Indications[0] is ArithmeticExpressions)
        {
            ArithmeticExpressions current = (ArithmeticExpressions)Indications[0];
            current.Evaluate();
            cardsCounter = (int)current.Value;
        }

        GameHUD.powerData.Add(new DrawPower(cardsCounter));
    }
    public static void Execute(string id, IEnumerable<object> commands)
    {
        switch (id)
        {
            case "Reborn":
                Process.Reborn(commands);
                break;
            case "Summon":
                Process.Summon(commands);
                break;
            case "Destroy":
                Process.Destroy(commands);
                break;
            case "ModifyAttack":
                Process.ModifyAttack(commands);
                break;
            case "Draw":
                Process.Draw(commands);
                break;
            case "SwitchBand":
                Process.SwitchBand(commands);
                break;
            default:
                break;
        }

    }
}
public static class FieldZones
{
    public static List<Cards> Llama(string name)
    {
        string classname = "FieldZones";
        Type called = Type.GetType(classname);
        MethodInfo Method = called.GetMethod(name);
        if (Method is null) throw new Exception("No existe el metodo");
        List<Cards> result = ((IEnumerable<Cards>)Method.Invoke(null, null)).ToList();
        if (result is null) throw new Exception("Parametros incorrectos");
        else return result;
    }
    public static IEnumerable<Cards> OwnHand()
    {
        return GameHUD.Positions.Places[SpacePosition.location.playerHand];
    }
    public static IEnumerable<Cards> OwnMelee()
    {
        return GameHUD.Positions.Places[SpacePosition.location.playerMelee];
    }
    public static IEnumerable<Cards> OwnMiddle()
    {
        return GameHUD.Positions.Places[SpacePosition.location.playerMiddle];
    }
    public static IEnumerable<Cards> OwnSiege()
    {
        return GameHUD.Positions.Places[SpacePosition.location.playerSiege];
    }
    public static IEnumerable<Cards> OwnGraveryard()
    {
        return GameHUD.Positions.Places[SpacePosition.location.playerGrave];
    }
    public static IEnumerable<Cards> OwnDeck()
    {
        return GameHUD.Positions.Places[SpacePosition.location.playerDeck];
    }
    public static IEnumerable<Cards> EnemyHand()
    {
        return GameHUD.Positions.Places[SpacePosition.location.enemyHand];
    }
    public static IEnumerable<Cards> EnemyMelee()
    {
        return GameHUD.Positions.Places[SpacePosition.location.enemyMelee];
    }
    public static IEnumerable<Cards> EnemyMiddle()
    {
        return GameHUD.Positions.Places[SpacePosition.location.enemyMiddle];
    }
    public static IEnumerable<Cards> EnemySiege()
    {
        return GameHUD.Positions.Places[SpacePosition.location.enemySiege];
    }
    public static IEnumerable<Cards> EnemyGraveryard()
    {
        return GameHUD.Positions.Places[SpacePosition.location.enemyGrave];
    }
    public static IEnumerable<Cards> EnemyDeck()
    {
        return GameHUD.Positions.Places[SpacePosition.location.enemyDeck];
    }
    public static IEnumerable<Cards> AllOwnCards()
    {
        return (GameHUD.Positions.Places[SpacePosition.location.playerMelee].Concat(GameHUD.Positions.Places[SpacePosition.location.playerMiddle])).Concat(GameHUD.Positions.Places[SpacePosition.location.playerSiege]);
    }
    public static IEnumerable<Cards> AllEnemyCards()
    {
        return (GameHUD.Positions.Places[SpacePosition.location.enemyMelee].Concat(GameHUD.Positions.Places[SpacePosition.location.enemyMiddle])).Concat(GameHUD.Positions.Places[SpacePosition.location.enemySiege]);
    }
    public static IEnumerable<Cards> AllExistingCards()
    {
        List<Cards> List0 = AllOwnCards().ToList();
        List<Cards> List1 = AllEnemyCards().ToList();        
        return (List0.Concat(List1));
    }
}