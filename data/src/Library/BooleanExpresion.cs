using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using Godot;
// La clase abstracta BooleanExpression hereda de Expressions y sobreescribe el ExpressionType con un valor booleano
public abstract class BooleanExpresion : Expressions
{
    public override ExpressionType Type { get => ExpressionType.boolean; set { Type = ExpressionType.boolean; } }

}
// La clase abstracta BinaryBooleann hereda de BooleanExpression y posee dos Expression, Left y Right
public abstract class BinaryBoolean : BooleanExpresion
{
    public Expressions Left;
    public Expressions Right;
}
// La clase abstracta UnaryBoolean hereda de BooleanExpression y posee una BooleanExpression Expression
public abstract class UnaryBoolean : BooleanExpresion
{
    public BooleanExpresion Expression;
}
// La clase Not hereda de UnaryBoolean y recibe en su constructor un BooleanExpression. El metodo Evaluate es sobresscrito con la
// siguiente definicion: Se evalua la expresion booleana recibida y se retorna como valor de la expresion propia la negacion
// del valor de la expresion anterior
public class Not : UnaryBoolean
{
    public override object Value { get; set; }
    public override ExpressionType Type { get; set; }
    public Not(BooleanExpresion expression)
    {
        this.Expression = expression;
    }
    public override void Evaluate()
    {
        this.Expression.Evaluate();
        this.Value = !(bool)this.Expression.Value;
    }
}
// La clase And hereda de BinaryBoolean y recibe en su constructor dos BooleanExpression, las cuales asigna a Left y Right. El metodo
// Evaluate es sobreescrito con la definicion siguiente: Se evaluan ambas expresiones recibidas y se retorna como valor el resultado
// de la conjuncion logica entre los valores de ambas expresiones.
public class And : BinaryBoolean
{
    public override object Value { get; set; }
    public override ExpressionType Type { get; set; }
    public And(BooleanExpresion left, BooleanExpresion right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override void Evaluate()
    {
        this.Left.Evaluate();
        this.Right.Evaluate();
        this.Value = (bool)this.Left.Value && (bool)this.Right.Value;
    }
}
// La clase Or hereda de BinaryBoolean y recibe en su constructor dos BooleanExpression, las cuales asigna a Left y Right. El metodo
// Evaluate es sobreescrito con la definicion siguiente: Se evaluan ambas expresiones recibidas y se retorna como valor el resultado
// de la disyuncion logica entre los valores de ambas expresiones.

public class Or : BinaryBoolean
{
    public override object Value { get; set; }
    public override ExpressionType Type { get; set; }
    public Or(BooleanExpresion left, BooleanExpresion right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override void Evaluate()
    {
        this.Left.Evaluate();
        this.Right.Evaluate();
        this.Value = (bool)this.Left.Value || (bool)this.Right.Value;
    }
}
// Un BooleanComparer es un predicado que recibe dos enteros, a y b.
public delegate bool BooleanComparer(int a, int b);
// La clase BooleanComparers agrupa una serie de objetos BooleanComparer basicos
public static class BooleanComparers
{
    public static BooleanComparer Equal = (int a, int b) => a == b;
    public static BooleanComparer Major = (int a, int b) => a > b;
    public static BooleanComparer Minor = (int a, int b) => a < b;
    public static BooleanComparer MajorOrEqual = (int a, int b) => a >= b;
    public static BooleanComparer MinorOrEqual = (int a, int b) => a <= b;

}
// La clase BinaryComparer hereda de BinaryBoolean y recibe en su constructor dos ArithmeticExpressions, las cuales asigna a Left y Right,
// y un BooleanComparer. El metodo Evaluate es sobreescrito y se le asigna la siguiente definicion: Se evaluan ambas expresiones 
// aritmeticas recibidas y se asigna al valor de la expresion el resultado del predicado evaluado en los valores de ambas expresiones
public class BinaryComparer : BinaryBoolean
{
    public override object Value { get; set; }
    public override ExpressionType Type { get; set; }
    private BooleanComparer Criteria;
    public BinaryComparer(ArithmeticExpressions left, ArithmeticExpressions right, BooleanComparer criteria)
    {
        this.Left = left;
        this.Right = right;
        this.Criteria = criteria;
    }
    public override void Evaluate()
    {
        this.Left.Evaluate();
        this.Right.Evaluate();
        this.Value = Criteria((int)this.Left.Value, (int)this.Right.Value);
    }
}
// La clase TruePredicate asigna automaticamente un valor true a la expresion y el metodo evaluate es sobreescrito aunque no hace nada.
public class TruePredicate : UnaryBoolean
{
    public override void Evaluate()
    {

    }
    public override object Value { get; set; }
    public override ExpressionType Type
    {
        get
        {
            return ExpressionType.boolean;
        }
        set { }
    }
    public TruePredicate()
    {
        this.Value = true;
    }
}
// La clase TruePredicate asigna automaticamente un valor false a la expresion y el metodo evaluate es sobreescrito aunque no hace nada.
public class FalsePredicate : UnaryBoolean
{
    public override void Evaluate()
    {

    }
    public override object Value { get; set; }
    public override ExpressionType Type
    {
        get
        {
            return ExpressionType.boolean;
        }
        set { }
    }
    public FalsePredicate()
    {
        this.Value = false;
    }
}
// La clase Exist hereda de UnaryBoolean y recibe en su constructor el nombre de una carta, un MethodInfo que hace referencia a
// la funcion Llama de la clase FieldZones y un string que indica la zona en cuestion. El metodo Evaluate es sobreescrito de manera
// que asigna verdadero al valor de la expresion si existe alguna carta en la lista que nos devuelve el MethodInfo Method al invocarlo
// con la zona recibida como argumento, cuyo nombre coincida con el cardname recibido; de lo contrario asigna falso al valor de la 
// expresion.
public class Exist : UnaryBoolean
{
    private string CardName;
    public override object Value { get; set; }
    private object[] Zone = new object[1];
    private List<Cards> list = new List<Cards>();
    MethodInfo Method;

    public Exist(string cardname, MethodInfo method, string zone)
    {
        this.CardName = cardname;
        this.Method = method;
        this.Zone[0] = zone;
    }
    public override void Evaluate()
    {
        GD.Print(this.CardName);
        GD.Print(this.Method.Name);
        GD.Print(this.Zone[0]);
        GD.Print("Evaluando existencia");
        
        if(GameHUD.phase == GameHUD.Phase.EnemyTurn){
            switch(this.Zone[0]){
                
                case "OwnMelee":
                this.Zone[0] = "EnemyMelee";
                break;
                case "OwnMiddle":
                this.Zone[0] = "EnemyMiddle";
                break;
                case "OwnSiege":
                this.Zone[0] = "EnemySiege";
                break;
                case "OwnHand":
                this.Zone[0] = "EnemyHand";
                break;
                case "OwnGraveryard":
                this.Zone[0] = "EnemyGraveryard";
                break;
                case "OwnDeck":
                this.Zone[0] = "EnemyDeck";
                break;
                case "EnemyHand":
                this.Zone[0] = "OwnHand";
                break;
                case "EnemyMelee":
                this.Zone[0] = "OwnMelee";
                break;
                case "EnemyMiddle":
                this.Zone[0] = "OwnMiddle";
                break;
                case "EnemySiege":
                this.Zone[0] = "OwnSiege";
                break;
                case "EnemyGraveryard":
                this.Zone[0] = "OwnGraveryard";
                break;
                case "EnemyDeck":
                this.Zone[0] = "OwnDeck";
                break;
                case "AllOwnCards":
                this.Zone[0] = "AllEnemyCards";
                break;
                case "AllEnemyCards":
                this.Zone[0] = "AllOwnCards";
                break;
            }
        }
        
        this.list = (List<Cards>)this.Method.Invoke(null, this.Zone);
        bool contains()
        {
            foreach (var item in this.list)
            {
                if (item.name == CardName) return true;
            }
            return false;
        }
        this.Value = contains();
    }

}