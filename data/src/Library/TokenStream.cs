using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
// La clase TokenStream contiene una serie de metodos utiles para desplazarnos por una lista de tokens 
// y complementar el proceso de parseo. La clase recibe como argumento la lista de tokens en cuestion
public class TokenStream
{
    private List<Token> Tokens;
    private int pos;
    // La propiedad Position indica el indice correspondiente al Token que estamos analizando ahora mismo
    public int Position { get { return this.pos; } }
    // La lista de string ProcessFunction agrupa los nombres de las funciones de la clase Process correspondientes a las funcionalidades
    // definidas en el juego.
    public List<string> ProcessFunctions { get; private set; }
    // La lista de string FieldZones agrupa los nombres de las funciones de la clase FieldZones correspondiente a las distintas zonas
    // del juego
    public List<string> FieldZoness { get; private set; }
    // La lista de string FieldZonesConsult agrupa las consultas referentes a algunos valores numericos relacionados con las zonas del juego
    public List<string> FieldZonesConsult { get; private set; }
    // La lista de string BooleanOperators agrupa los nombres de los operadores booleanos definidos en el lenguaje
    public List<string> BooleanOperators { get; private set; }
    // La lista de string BooleanComparers agrupa los nombres referentes a los comparadores booleanos definidos en el lenguaje
    public List<string> BooleanComparers { get; private set; }
    public List<string> ArithmeticOperators{get; private set;}
    // Esta funcion inicializa y llena la Lista BooleanOperators
    private void SetBooleanOperators()
    {
        BooleanOperators = new List<string>();
        BooleanOperators.Add(TokenValues.and);
        BooleanOperators.Add(TokenValues.or);
        BooleanOperators.Add(TokenValues.not);
        BooleanOperators.Add(TokenValues.binarycomparer);
        BooleanOperators.Add(TokenValues.truepredicate);
        BooleanOperators.Add(TokenValues.falsepredicate);
        BooleanOperators.Add(TokenValues.existcardin);
    }
    // Esta funcion inicializa y llena la Lista BooleanComparers
    private void SetBooleanComparers()
    {
        BooleanComparers = new List<string>();
        BooleanComparers.Add(TokenValues.Equal);
        BooleanComparers.Add(TokenValues.Major);
        BooleanComparers.Add(TokenValues.Minor);
        BooleanComparers.Add(TokenValues.MajorOrEqual);
        BooleanComparers.Add(TokenValues.MinorOrEqual);
    }
    // Esta funcion inicializa y llena la Lista ProcessFunctions
    private void SetProcessFunctions()
    {
        ProcessFunctions = new List<string>();
        ProcessFunctions.Add(TokenValues.destroy);
        ProcessFunctions.Add(TokenValues.draw);
        ProcessFunctions.Add(TokenValues.modifyAttack);
        ProcessFunctions.Add(TokenValues.reborn);
        ProcessFunctions.Add(TokenValues.summon);
        ProcessFunctions.Add(TokenValues.switchband);
    }
    // Esta funcion inicializa y llena la Lista FieldZones
    private void SetFieldZones()
    {
        FieldZoness = new List<string>();
        FieldZoness.Add(TokenValues.allowncards);
        FieldZoness.Add(TokenValues.allenemycards);
        FieldZoness.Add(TokenValues.allexistingcards);
        FieldZoness.Add(TokenValues.owndeck);
        FieldZoness.Add(TokenValues.owngraveryard);
        FieldZoness.Add(TokenValues.ownhand);
        FieldZoness.Add(TokenValues.ownmelee);
        FieldZoness.Add(TokenValues.ownmiddle);
        FieldZoness.Add(TokenValues.ownsiege);
        FieldZoness.Add(TokenValues.enemydeck);
        FieldZoness.Add(TokenValues.enemygraveryard);
        FieldZoness.Add(TokenValues.enemyhand);
        FieldZoness.Add(TokenValues.enemymelee);
        FieldZoness.Add(TokenValues.enemymiddle);
        FieldZoness.Add(TokenValues.enemysiege);
    }
    // Esta funcion inicializa y llena la Lista FieldZonesConsult
    private void SetFieldZonesConsult()
    {
        FieldZonesConsult = new List<string>();
        FieldZonesConsult.Add(TokenValues.highestattackin);
        FieldZonesConsult.Add(TokenValues.lowestattackin);
        FieldZonesConsult.Add(TokenValues.numberofcardsin);
        FieldZonesConsult.Add(TokenValues.damagein);
        FieldZonesConsult.Add(TokenValues.damage);
    }
    private void SetArithmeticOperators()
    {
        ArithmeticOperators.Add(TokenValues.sum);
        ArithmeticOperators.Add(TokenValues.sub);
        ArithmeticOperators.Add(TokenValues.mult);
        ArithmeticOperators.Add(TokenValues.div);
    }
    public TokenStream(IEnumerable<Token> tokens)
    {
        this.Tokens = new List<Token>(tokens);
        this.pos = 0;
        SetProcessFunctions();
        SetFieldZones();
        SetFieldZonesConsult();
        SetBooleanOperators();
        SetBooleanComparers();
        SetArithmeticOperators();
    }
    // Todos los metodos a continuacion son intuitivos, por lo cual prescindimos de su explicacion.
    public bool ReachedEnd => Position == this.Tokens.Count - 1;
    public void MoveForward(int cant)
    {
        pos += cant;
    }
    public void MoveBackWard(int cant)
    {
        pos -= cant;
    }
    public bool MoveNext()
    {
        if (this.pos < Tokens.Count - 1)
        {
            this.pos++;
        }
        return this.pos < Tokens.Count;
    }
    public bool MoveNext(TokenType Type)
    {
        if (this.pos < Tokens.Count - 1 && LookAhead(1).Type == Type)
        {
            this.pos++;
            return true;
        }
        return false;
    }
    public bool MoveNext(string value)
    {
        if (this.pos < Tokens.Count - 1 && LookAhead(1).Value == value)
        {
            this.pos++;
            return true;
        }
        return false;
    }
    public bool CanLookAhead(int k = 0)
    {
        return Tokens.Count - this.pos > k;
    }
    public Token LookAhead(int k = 0)
    {
        return Tokens[this.pos + k];
    }
}