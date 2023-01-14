using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
// La clase abstracta ArithmeticExpressions hereda de Expressions y sobreescribe el ExpressionType con un valor numerico
public abstract class ArithmeticExpressions : Expressions
{
    public override ExpressionType Type
    {
        get { return ExpressionType.number; }
        set { }
    }
}
// La clase abstracta BinaryExpression hereda de ArithmeticExpressions y posee dos ArithmeticExpressions, Left y Right
public abstract class BinaryExpressions : ArithmeticExpressions
{
    public ArithmeticExpressions Left;
    public ArithmeticExpressions Right;
}
// La clase abstracta UnaryExpressions hereda de ArithmeticExpressions y posee una ArithmeticExpressions Expression
public abstract class UnaryExpressions : ArithmeticExpressions
{
    public ArithmeticExpressions Expression;
}
// La clase Sum hereda de BinaryExpressions y recibe en su constructor dos ArithmeticExpressions, asignandolas a los campos Left y Right
// de BinaryExpressions. La clase sobreescribe el metodo evaluate definido en la clase Expressions de manera tal que se evaluan
// ambas expresiones Left y Right y se devuelve el valor numerico resultante de la suma de los valores de ambas expresiones.
public class Sum : BinaryExpressions
{
    public override object Value { get; set; }
    public override ExpressionType Type { get; set; }
    public Sum(ArithmeticExpressions left, ArithmeticExpressions right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override void Evaluate()
    {
        this.Left.Evaluate();
        this.Right.Evaluate();
        this.Value = (double)this.Left.Value + (double)this.Right.Value;
    }
}
// La clase Sub hereda de BinaryExpressions y recibe en su constructor dos ArithmeticExpressions, asignandolas a los campos Left y Right
// de BinaryExpressions. La clase sobreescribe el metodo evaluate definido en la clase Expressions de manera tal que se evaluan
// ambas expresiones Left y Right y se devuelve el valor numerico resultante de la resta de los valores de ambas expresiones.
public class Sub : BinaryExpressions
{
    public override object Value { get; set; }
    public override ExpressionType Type { get; set; }
    public Sub(ArithmeticExpressions left, ArithmeticExpressions right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override void Evaluate()
    {
        this.Left.Evaluate();
        this.Right.Evaluate();
        this.Value = (double)this.Left.Value - (double)this.Right.Value;
    }
}
// La clase Mult hereda de BinaryExpressions y recibe en su constructor dos ArithmeticExpressions, asignandolas a los campos Left y Right
// de BinaryExpressions. La clase sobreescribe el metodo evaluate definido en la clase Expressions de manera tal que se evaluan
// ambas expresiones Left y Right y se devuelve el valor numerico resultante de la multiplicacion de los valores de ambas expresiones.
public class Mult : BinaryExpressions
{
    public override object Value { get; set; }
    public override ExpressionType Type { get; set; }
    public Mult(ArithmeticExpressions left, ArithmeticExpressions right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override void Evaluate()
    {
        this.Left.Evaluate();
        this.Right.Evaluate();
        this.Value = (double)this.Left.Value * (double)this.Right.Value;
    }
}
// La clase Div hereda de BinaryExpressions y recibe en su constructor dos ArithmeticExpressions, asignandolas a los campos Left y Right
// de BinaryExpressions. La clase sobreescribe el metodo evaluate definido en la clase Expressions de manera tal que se evaluan
// ambas expresiones Left y Right y se devuelve el valor numerico resultante de la division de los valores de ambas expresiones.
public class Div : BinaryExpressions
{
    public override object Value { get; set; }
    public override ExpressionType Type { get; set; }
    public Div(ArithmeticExpressions left, ArithmeticExpressions right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override void Evaluate()
    {
        this.Left.Evaluate();
        this.Right.Evaluate();
        this.Value = (double)this.Left.Value / (double)this.Right.Value;
    }
}
// La clase Number hereda de UnaryExpressions y posee varias sobrecargas para su constructor, las cuales detallaremos a continuacion.
public class Number : UnaryExpressions
{
    public override object Value { get; set; }
    private MethodInfo Method;
    private string CardName;
    private string IDValue;
    private string Zone;
    public override ExpressionType Type
    {
        get { return ExpressionType.number; }
        set { }
    }
    // Este es el constructor mas basico, en el cual se recibe un numero y se define ese numero como valor de la expresion
    public Number(int value)
    {
        this.Value = value;
    }
    // En este constructor se recibe como argumento un MethodInfo que hace referencia al metodo llama de
    // la clase FieldZones,  un string zone que nos indica la zona del campo sobre la cual nos interesa algun dato determinado y
    // un string IDValue que hace referencia al valor del dato que nos gustaria obtener; el cual puede ser el numero de cartas
    // existentes en la zona, el ataque combinado de todas las cartas de tipo unidad en la zona, o el mayor/menor ataque de
    // entre los de todas las cartas de tipo unidad en la zona.
    public Number(MethodInfo method, string IdValue, string zone)
    {
        this.Method = method;
        this.IDValue = IdValue;
        this.Zone = zone;
    }
    // En este constructor se recibe como argumento el nombre de una carta y se almacena en la variable de estado CardName. 
    // El objetivo de esto es conseguir en tiempo de ejecucion el ataque de dicha carta y asignarlo como valor de la expresion
    public Number(string CardName)
    {
        this.CardName = CardName;
    }
    // El metodo evaluate se encarga de asignar un valor numerico a la expresion en caso de que no haya sido usado el primer
    // constructor listado anteriormente
    public override void Evaluate()
    {
        if (Method != null)
        {
            // En el caso de que el MethodInfo Method no sea nulo, esto quiere decir que se ha utilizado el segundo constructor 
            // listado; por lo cual se procede a realizar un analisis en funcion de que informacion se quiere obtener acerca de
            // la zona del juego zone. En cada caso se realiza una operacion diferente sobre la Lista de cartas que se obtendra
            // al invocar al MethodInfo Method con la zona en cuestion como parametro.
            List<Cards> cards = (List<Cards>)Method.Invoke(null, new object[] { this.Zone });
            switch (IDValue)
            {
                
                case TokenValues.numberofcardsin:                    
                    this.Value = cards.Count();
                    break;
                case TokenValues.damagein:
                    int sum = 0;
                    foreach (var item in cards)
                    {
                        if (item is UnitCard)
                        {
                            ((UnitCard)item).damage.Evaluate();
                            sum += (int)((UnitCard)item).damage.Value;
                        }
                    }
                    this.Value = sum;
                    break;
                case TokenValues.highestattackin:
                    int highest = 0;
                    foreach (var item in cards)
                    {
                        if(item is UnitCard)
                        { 
                            UnitCard currentCard = (UnitCard)item;
                            currentCard.damage.Evaluate();
                            if ((int)currentCard.damage.Value > highest)
                            {
                                highest = (int)currentCard.damage.Value;
                            }
                        }
                        
                    }
                    this.Value = highest;
                    break;
                case TokenValues.lowestattackin:
                    int lowest = 0;
                    foreach (var item in cards)
                    {
                        if(item is UnitCard)
                        { 
                            UnitCard currentCard = (UnitCard)item;
                            currentCard.damage.Evaluate();
                            if ((int)currentCard.damage.Value < lowest)
                            {
                                highest = (int)currentCard.damage.Value;
                            }
                        }
                        
                    }
                    this.Value = lowest;
                    break;
                default:
                    break;
            }

        }
        // En el caso de que el nombre de la carta no sea nulo se procede a buscar una carta que exista en el juego cuyo
        // nombre coincida con el ingresado y de encontrarse se comprueba que sea de tipo unidad y se procede a asignar 
        // su ataque como valor de la expresion.
        else if (!(CardName is null))
        {
            int attack = 0;
            List<Cards> cards = FieldZones.Llama(TokenValues.allexistingcards);
            foreach (var item in cards)
            {
                if (item is UnitCard)
                {
                    UnitCard currentCard = (UnitCard)item;
                    if (currentCard.name == CardName)
                    {
                        currentCard.damage.Evaluate();
                        attack = (int)currentCard.damage.Value;
                    }
                }
                
            }
            this.Value = attack;
        }
    }
}