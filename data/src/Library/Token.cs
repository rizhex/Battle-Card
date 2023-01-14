using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
// La clase Token representa un objeto el cual posee tres propiedades, un valor, un TokenType y un CodeLocation.
// El TokenType representa el tipo que posee el token y el CodeLocation representa la posicion del token en el codigo, lo que facilita
// la deteccion de errores.
public class Token
{
    public string Value { get; private set; }
    public TokenType Type { get; private set; }
    public CodeLocation Location { get; private set; }
    public Token(string value, TokenType type, CodeLocation location)
    {
        this.Value = value;
        this.Type = type;
        this.Location = location;
    }
}
// El enum TokenType define los tipos de Token que pueden existir. En escencia los tokens pueden ser: numeros, texto, palabras clave,
// identificadores o simbolos.
public enum TokenType
{
    number,
    text,
    keyword,
    identifier,
    symbols,
    properties,
}
// La clase TokenValues define una gran variedad de constantes que representan los posibles valores asociados a cada Token definido
// en nuestro lenguaje. Esto hace mucho mas comodo y eficiente el proceso de identificacion de los tokens, siempre y cuando se respete
// el uso exclusivo de estas constantes como valores de los tokens que creemos.
public class TokenValues
{
    //Arithmetic operators
    public const string sum = "Addition"; // +
    public const string sub = "Substraction"; // -
    public const string mult = "Multiply"; // *
    public const string div = "Division";   // /        
                                            //Comparing operators
    public const string assign = "Assign"; // =        
    public const string Equal = "Equal"; // ==
    public const string Major = "Major"; // >
    public const string Minor = "Minor"; // <
    public const string MajorOrEqual = "MajorOrEqual"; // >=
    public const string MinorOrEqual = "MinorOrEqual"; // <=
                                                       //Boolean operators
    public const string and = "And";
    public const string or = "Or";
    public const string not = "Not";
    public const string binarycomparer = "BinaryComparer";
    public const string truepredicate = "TruePredicate";
    public const string falsepredicate = "FalsePredicate";
    // Symbols   
    public const string ValueSeparator = "ValueSeparator";  // ,
    public const string StatementSeparator = "StatementSeparator";  // ;
    public const string OpenBracket = "OpenBracket"; // (
    public const string ClosedBracket = "ClosedBracket"; // )
    public const string OpenCurlyBrackets = "OpenCurlyBrackets"; // {
    public const string ClosedCurlyBrackets = "ClosedCurlyBrackets"; // }
                                                                     //Keywords
    public const string arithmeticexpression = "ArithmeticExpression";
    public const string booleanexpression = "BooleanExpression";
    public const string textexpression = "TextExpression";
    public const string conditionset = "ConditionSet";  //IEnumerable de Condition
    public const string instructionset = "InstructionSet";  //IEnumerable de Instruction
    public const string condition = "Condition";  //Objeto de tipo Condition, es un Boolean Expression
    public const string instruction = "Instruction";  //Objeto de tipo Instruction
    public const string card = "Card"; //Objeto de tipo Card
    public const string unitcard = "UnitCard"; //Constructor para las cartas de tipo unidad
    public const string leadercard = "LeaderCard"; //Constructor para las cartas de tipo lider
    public const string effectcard = "EffectCard"; //Constructor para las cartas de tipo efecto
    public const string name = "Name"; //Propiedad de las cartas
    public const string attack = "Attack";   //Propiedad de las cartas, es una Arithmetic Expression
    public const string power = "Power"; //Propiedad de las cartas, es un Power
    public const string powerset = "PowerSet";
    public const string position = "Position";  //Propiedad de las cartas
    public const string phrase = "Phrase";  //Propiedad de las cartas
    public const string path = "Path";  //Propiedad de las cartas
    public const string type = "Type";  //Propiedad de las cartas
    public const string melee = "Melee";
    public const string middle = "Middle";
    public const string siege = "Siege";
    public const string weather = "Weather";
    public const string support = "Support";
    public const string destroy = "Destroy";    //Funcion del Process
    public const string summon = "Summon";  //Funcion del Process
    public const string reborn = "Reborn";  //Funcion del Process
    public const string draw = "Draw";  //Funcion del Process  
    public const string modifyAttack = "ModifyAttack"; //Funcion del Process 
    public const string allenemycards = "AllEnemyCards";  //IEnumerable de todas las cartas enemigas en juego
    public const string allowncards = "AllOwnCards";    //IEnumerable de todas las cartas propias en juego
    public const string highestattackin = "HighestAttackIn"; //Funcion que recibe un IEnumerable de cartas y devuelve la carta de mayor ataque
    public const string lowestattackin = "LowestAttackIn"; //Funcion que recibe un IEnumerable de cartas y devuelve la carta de menor ataque       
    public const string numberofcardsin = "NumberOfCardsIn"; //Funcion que recibe un IEnumerable de cartas y devuelve la cantidad de cartas
    public const string damage = "Damage"; //Funcion q recibe una carta y devuelve su atk
    public const string damagein = "DamageIn";//Funcion q recibe un IEnumerable de cartas y devuelve el atk combinado
    public const string existcardin = "ExistCardIn"; //Funcion q recibe un IEnumerable de cartas y devuelve si existe la carta
    public const string ownhand = "OwnHand"; //IEnumerable de cartas de la propia mano
    public const string ownmelee = "OwnMelee"; //IEnumerable de cartas de la propia zona melee
    public const string ownmiddle = "OwnMiddle"; //IEnumerable de cartas de la propia zona a distancia
    public const string ownsiege = "OwnSiege"; //IEnumerable de cartas de la propia zona de asedio
    public const string owngraveryard = "OwnGraveryard"; //IEnumerable de cartas del propio cementerio
    public const string owndeck = "OwnDeck"; //IEnumerable de cartas del propio deck
    public const string enemyhand = "EnemyHand"; //IEnumerable de cartas de la mano enemiga
    public const string enemymelee = "EnemyMelee"; //IEnumerable de cartas de la zona melee enemiga
    public const string enemymiddle = "EnemyMiddle"; //IEnumerable de cartas de la zona a distancia enemiga
    public const string enemysiege = "EnemySiege";  //IEnumerable de cartas de la zona de asedio enemiga
    public const string enemygraveryard = "EnemyGraveryard";  //IEnumerable de cartas del cementerio enemigo
    public const string enemydeck = "EnemyDeck"; //IEnumerable de cartas del deck enemigo  
    public const string allexistingcards = "AllExistingCards";
    public const string switchband = "SwitchBand";
    public const string freeelection = "FreeElection";
}