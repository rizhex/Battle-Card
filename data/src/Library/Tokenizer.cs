using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using Godot;
// La clase Tokenizer es la encargada del proceso de Tokenizacion del codigo (para eso se auxilia en la clase TokenReader).
// Esta clase posee un conjunto de metodos que le permiten identificar tokens en el codigo introducido por el usuario, asi como 
// diccionarios en los cuales asocia las palabras claves definidas en nuestro lenguaje con valores constantes obtenidos de la clase
// TokenValues.
public class Tokenizer
{
    //El diccionario operators asocia cada operador definido en el lenguaje con una constante definida en TokenValues.
    Dictionary<string, string> operators = new Dictionary<string, string>();
    //El diccionario keywords asocia cada palabra clave definida en el lenguaje con una constante definida en TokenValues.
    Dictionary<string, string> keywords = new Dictionary<string, string>();    
    Dictionary<string,string> Propeties = new Dictionary<string, string>();
    //El diccionario texts asocia cada token de tipo texto definido en el lenguaje con una constante definida en TokenValues.
    Dictionary<string, string> texts = new Dictionary<string, string>();
    // La lista Keywords agrupa el conjunto de palabras claves definidas en el lenguaje.
    public List<string> Keywords { get { return keywords.Keys.ToList(); } }
    // El metodo RegisterOperator es usado para asociar los operadores a las constantes definidas en TokenValues mediante operators
    public void RegisterOperator(string op, string tokenValue)
    {
        this.operators[op] = tokenValue;
    }
    // El metodo RegisterKeyWord es usado para asociar las palabras clave a las constantes definidas en TokenValues mediante keywords
    public void RegisterKeyword(string keyword, string tokenValue)
    {
        this.keywords[keyword] = tokenValue;
    }
    // El metodo RegisterText es usado para asociar los tokens de texto a las constantes definidas en TokenValues mediante texts
    public void RegisterText(string start, string end)
    {
        this.texts[start] = end;
    }
    public void RegisterProperty(string prop, string value)
    {
        this.Propeties[prop] = value;
    }
    // El metodo MatchSymbol recibe como argumentos un TokenReader y una lista de Token, luego itera por cada valor de operators
    // y comprueba si a partir del Token que esta siendo analizado viene el simbolo de la iteracion actual. En caso de ser asi
    // el token es incorporado a la lista como un token con el valor del simbolo en cuestion, tipo simbolo y la localizacion actual
    // que marca el TokenReader y se devuelve true.
    private bool MatchSymbol(TokenReader stream, List<Token> tokens)
    {
        foreach (var op in operators.Keys.OrderByDescending(k => k.Length))
            if (stream.Matches(op))
            {
                tokens.Add(new Token(operators[op], TokenType.symbols, stream.Location));
                return true;
            }
        return false;
    }
    // El metodo MatchText recibe como argumentos un TokenReader y una lista de Token, luego itera por cada valor de texts
    // y comprueba si a partir del Token que esta siendo analizado viene el texto de la iteracion actual. En caso de ser asi
    // el token es incorporado a la lista como un token con el valor del texto en cuestion, tipo texto y la localizacion actual
    // que marca el TokenReader y se devuelve true.
    private bool MatchText(TokenReader stream, List<Token> tokens)
    {
        foreach (var start in texts.Keys.OrderByDescending(k => k.Length))
        {
            string text;
            if (stream.Matches(start))
            {
                if (!stream.ReadUntil(texts[start], out text))
                    //errors.Add(new CompilingError(stream.Location, ErrorCode.Expected, texts[start]));
                    tokens.Add(new Token(text, TokenType.text, stream.Location));
                return true;
            }
        }
        return false;
    }
    // El metodo GetTokens recibe como argumentos el codigo introducido por el usuario y el nombre de la carta siendo analizada
    // y devuelve una lista con todos los tokens en los que se ha convertido el codigo, siempre que no se haya empleado algun
    // termino ajeno a la sintaxis definida. El metodo se apoya totalmente en los metodos auxiliares de la clase TokenReader.
    public IEnumerable<Token> GetTokens(string code, string cardname)
    {
        // Esta es la lista de tokens que vamos a llenar y devolver.
        List<Token> tokens = new List<Token>();
        // Este es el TokenReader que nos va a permitir avanzar por el codigo caracter a caracter en busqueda de los tokens
        TokenReader stream = new TokenReader(code, cardname);
        // Aqui se inicia un bucle que va a repetirse hasta que lleguemos al final del archivo.
        while (!stream.EndOfFile)
        {   
            // Este valuea representa el posible token a incorporar
            string value;
            // Aqui se desechan los espacios en blanco
            if (stream.ReadWhiteSpace())
                continue;
            // Aqui se llama al metodo MatchSymbol, exlicado anteriormente. En caso de devolverse true se salta a la siguiente 
            // iteracion del bucle            
            if (MatchSymbol(stream, tokens))
                continue;
            // Aqui se llama al metodo ReadKeyWord definido en TokenReader. En caso de retornar true, se procede a comprobar si 
            // la posible palabra clave existe en el lenguaje, en cuyo caso se incorpora el token a la lista. En caso contrario se
            // lanza una excepcion.
            if(stream.ReadKeyWord(out value)){
                
                if(Keywords.Contains(value)) 
                {
                    tokens.Add(new Token(keywords[value], TokenType.keyword, stream.Location));
                    continue;
                }
                else if (Propeties.Keys.Contains(value))
                {
                    tokens.Add(new Token(Propeties[value], TokenType.properties, stream.Location));
                    continue;
                }
                throw new Exception($"Unknown Token in card {stream.Location.CardName} at line: {stream.Location.Line} , comillas expected");
            }
            // Aqui se llama al metodo ReadID definido en TokenReader. En caso de retornar true, se procede a incorporar el token a 
            // la lista.
            if (stream.ReadID(out value))
            {   
                value = value.Substring(1, value.Length-2);      
                tokens.Add(new Token(value, TokenType.identifier, stream.Location));
                continue;
            }
            // Aqui se llama al metodo ReadNumber definido en TokenReader. En caso de retornar true, se procede a tratar de parsear 
            // el valor a un numero; si se puede se incorpora el token a la lista. En caso contrario se lanza una excepcion.
            if (stream.ReadNumber(out value))
            {
                double d;
                if (!double.TryParse(value, out d))                    
                    throw new Exception($"Wrong number format at line: {stream.Location.Line}");
                tokens.Add(new Token(value, TokenType.number, stream.Location));
                continue;
            }
             // Aqui se llama al metodo MatchText, exlicado anteriormente. En caso de devolverse true se salta a la siguiente 
            // iteracion del bucle  
            if (MatchText(stream, tokens))
                continue;
            // Si se llega a esta instruccion significa que el candidato a token no puede ser identificado, por tanto no pertenece
            // al lenguaje y se lanza una excepcion en concecuencia.
            var unkOp = stream.ReadAny();
            throw new Exception($"Unknown Token at line: {stream.Location.Line} , character:{unkOp}");
        }

        return tokens;
    }
}
// La clase TokenReader posee una serie de metodos utiles para avanzar por el codigo caracter a caracter. Posee un int posicion que
// funge de puntero para poder tener constancia de como vamos avanzando y otro int que indica la linea en la cual estamos parados
class TokenReader
{
    string Code;
    int position;
    int line;
    string cardName;
    public CodeLocation Location
    {
        get
        {
            return new CodeLocation
            {
                Line = this.line,
                CardName = this.cardName
            };
        }
    }
    // TokenReader recibe como argumentos el codigo del usuario, el nombre de la carta y se inicializa la posicion en 0 y la linea en 1
    public TokenReader(string code, string cardname)
    {
        this.Code = code;
        this.position = 0;
        this.line = 1;
        this.cardName = cardname;
    }
    // El metodo PeekNext devuelve el caracter en la posicion actual del codigo, siempre que esta sea valida
    public char PeekNext()
    {
        if (position < 0 || position >= this.Code.Length)
        {
            throw new InvalidOperationException();
        }
        return Code[position];
    }
    // La propiedad EndOfFile devuelve verdadero si la posicion es mayor o igual a la cantidad de caracteres del codigo
    public bool EndOfFile { get { return this.position >= Code.Length; } }
    // La propiedad EndOfLine devuelve verdadero si se alcanza un caracter de salto de linea '\n'
    public bool EndOfLine { get { return EndOfFile || this.Code[position] == '\n'; } }
    // El metodo ContinuesWith recibe un string prefix como argumento y comienza a avanzar mientras la posicion sea valida y cada caracter
    // del prefix sea igual a cada caracter del codigo en la siguiente posicion. Si esto no se cumple hasta que se haya cubierto
    // toda la longitud del prefix se devuelve falso; de lo contrario verdadero.
    public bool ContinuesWith(string prefix)
    {
        if (this.position + prefix.Length > this.Code.Length)
            return false;
        for (int i = 0; i < prefix.Length; i++)
            if (this.Code[this.position + i] != prefix[i])
                return false;
        return true;
    }
    // El metodo Matches recine como argumento un string prefix y chequea el metodo ContinuesWith. Si este devuelve verdadero entonces
    // Matches aumenta la posicion el la longitud del prefix y devuelve verdadero; de lo contrario mantiene la posicion y devuelve falso
    public bool Matches(string prefix)
    {
        if (ContinuesWith(prefix))
        {
            this.position += prefix.Length;
            return true;
        }

        return false;
    }
    // Este metodo chequea si el caracter en cuestion es valido para formar una palabra clave. Si es el primer caracter de la cadena 
    // ha de ser una letra, de lo contrario ha de ser una letra o digito. En cualquier otro caso se devuelve falso.
    public bool IsValidIdKeyWordCharacter(char c, bool begining)
    {
        return (begining ? char.IsLetter(c) : char.IsLetterOrDigit(c));
    }
    // El metodo ReadKeyWord se encarga de leer una cadena de caracteres siempre que estos sean validos para crear una palabra
    // clave, y devuelve el verdadero si la cadena obtenida no es vacia, ademas de devolver la cadena como valor de retorno
    public bool ReadKeyWord(out string KeyWord)
    {
        KeyWord = "";
        while (!EndOfLine && IsValidIdKeyWordCharacter(PeekNext(), KeyWord.Length == 0))
            KeyWord += ReadAny();
        return KeyWord.Length > 0;
    }
    // El metodo ReadID se encarga de leer una cadena de caracteres siempre que estos sean validos para crear un identificador,
    // y devuelve el verdadero si la cadena obtenida no es vacia, ademas de devolver la cadena como valor de retorno
    public bool ReadID(out string id)
    {
        id = "";        
        while (!EndOfLine && IsValidIdCharacter(PeekNext(), id.Length == 0))
        {
            id+=ReadAny();
        }
        if(id.Length>0) id += ReadAny();
        return id.Length>0;
    }
    // Este metodo chequea si el caracter en cuestion es valido para formar un identificador. Si es el primer caracter de la cadena 
    // ha de ser unas comillas, de lo contrario no pueden ser comillas. En cualquier otro caso se devuelve falso.
    public bool IsValidIdCharacter(char c, bool begining)
    {
        if (begining)
        {
            return c== '"';
        }
        else if(c=='"') return false;
        else return true;
    }
    // Este metodo lee y omite todo el segmento de codigo mientras sea un espacio en blanco
    public bool ReadWhiteSpace()
    {
        if (char.IsWhiteSpace(PeekNext()))
        {
            ReadAny();
            return true;
        }
        return false;
    }
    // Este metodo lee hasta un string determinado. Solo devuelve falso si se alcanza un salto de linea o el fin del codigo
    public bool ReadUntil(string end, out string text)
    {
        text = "";
        while (!Matches(end))
        {
            if (EndOfFile || EndOfLine)
                return false;
            text += ReadAny();
        }
        return true;
    }
    // Este metodo lee una cadena de texto como un numero y devuelve falso si la longitu de la cadena es 0
    public bool ReadNumber(out string number)
    {
        number = "";
        if(ContinuesWith("-"))
        number+=ReadAny();
        while (!EndOfLine && char.IsDigit(PeekNext()))
            number += ReadAny();
        if (!EndOfLine && Matches("."))
        {
            number += '.';
            while (!EndOfLine && char.IsDigit(PeekNext()))
                number += ReadAny();
        }

        if (number.Length == 0)
            return false;

        return number.Length > 0;
    }
    // Este metodo lee cualquier caracter a continuacion, siempre que la posicion siguiente sea valida
    public char ReadAny()
    {
        if (EndOfFile)
            throw new InvalidOperationException();
        if (EndOfLine) line += 1;

        return this.Code[this.position++];
    }
}