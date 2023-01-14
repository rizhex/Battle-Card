// La clase abstracta Expressions agrupa las caracteristicas basicas de toda expresion. Toda expresion 
// posee un valor de tipo objeto, un metodo Evaluate() que asigna un valor a la propiedad anterior y
// un tipo de expresion definida mediante el enum ExpressionType
public abstract class Expressions : AST_Root
{
    public abstract void Evaluate();

    public abstract object Value { get; set; }
    public abstract ExpressionType Type { get; set; }
}
public class TextExpression: Expressions
{
    public override object Value {get; set;}
    public override void Evaluate()
    {
        
    }
    public override ExpressionType Type{get => ExpressionType.text; set=> this.Type = ExpressionType.text;}
    public TextExpression(string content)
    {
        this.Value = content;
    }

}
// El enum ExpressionType agrupa los diferentes tipos de expresiones, los cuales pueden ser: numero,
// texto o booleano.
public enum ExpressionType
{
    number,
    text,
    boolean,
    errorType,
    power,
}