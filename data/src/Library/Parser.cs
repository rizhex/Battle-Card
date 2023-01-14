using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
// La clase Parser le atribuye una significacion semantica a la lista de tokens, y en ultima instancia 
// crea la carta con las especificidades definidas por el usuario. Con este objetivo se apoya en los 
// metodos definidos en la clase TokenStream para desplazarse por la lista de tokens
public class Parser
{
    public TokenStream Stream { get; private set; }
    // El parser recibe como argumento un TokenStream para recorrer la lista de tokens
    public Parser(TokenStream stream)
    
    {
        this.Stream = stream;

    }
    // El metodo ParseCard se encarga de comenzar el proceso de parseo. Procede a identificar el tipo de la carta a parsear
    // para llamar al metodo correspondiente en cada caso, y si no coincide con las opciones definidas lanza una excepcion
    public Cards ParseCard()
    {
        Cards answer;
        if (!Stream.CanLookAhead()) throw new Exception("Error");
        switch (Stream.LookAhead().Value)
        {
            case TokenValues.unitcard:
                answer = ParseUnitCard();
                break;
            case TokenValues.leadercard:
                answer = ParseLeaderCard();
                break;
            case TokenValues.effectcard:
                answer = ParseEffectCard();
                break;
            default:
                throw new Exception($"Card identifier expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                break;
        }
        return answer;
    }
    
    // El metodo ParsePower se encarga de parsear el poder de las cartas, siguieno una secuencia de pasos en los cuales se 
    // garantiza el cumplimiento con la sintaxis definida para el lenguage y se va construyendo el poder paulatinamente
    public Power ParsePower()
    {
        string name;
        List<Condition> ConditionSet;
        List<Instruction> InstructionSet;
        if (!(Stream.LookAhead().Value == TokenValues.power))
        {
            throw new Exception($"Power expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        if (this.Stream.MoveNext(TokenType.identifier))
        {
            // Aqui se obtiene el nombre del poder
            name = Stream.LookAhead().Value;
        }
        else throw new Exception($"ID expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");

        if (!Stream.MoveNext(TokenValues.OpenCurlyBrackets))
        {
            throw new Exception($"Open Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        // Esta instruccion se encarga de parsear el conjunto de condiciones necesarias(definidas por el usuario) para la 
        // ejecucion del poder
        ConditionSet = ParseConditionSet().ToList();

        // Esta instruccion se encarga de parsear el conjunto de instrucciones que se ejecutaran una vez se active el poder
        // (en tiempo de ejecucion)
        InstructionSet = ParseInstructionSet().ToList();

        if (!Stream.MoveNext(TokenValues.ClosedCurlyBrackets))
        {
            throw new Exception($"Closed Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui se crea un nuevo poder con el conjunto de condiciones y el conjunto de instrucciones ya parseados
        return new Power(name, ConditionSet, InstructionSet);
    }
    
    public PowerSet ParsePowerSet()
    {
        List<Power> answer = new List<Power>();
        string name;
        if(Stream.LookAhead().Type != TokenType.identifier) throw new Exception("PowerSet name expected");
        name = Stream.LookAhead().Value;
        if(!Stream.MoveNext(TokenValues.OpenCurlyBrackets)) throw new Exception("{ expected");
        Stream.MoveForward(1);
        if (this.Stream.LookAhead().Value == TokenValues.power)
        {
            // Aqui se parsea el poder de la carta
            while (this.Stream.LookAhead().Value == TokenValues.power)
            {
                answer.Add(ParsePower());
                Stream.MoveForward(1);
            }
        }
        if(Stream.LookAhead().Value != TokenValues.ClosedCurlyBrackets) throw new Exception("} expected");
        return new PowerSet(name, answer);
    }
    // El metodo ParseConditionSet se encarga de parsear el conjunto de condiciones que contiene el poder, velando siempre
    // por que se respete la sintaxis y lanzando excepcion en caso de que ocurra alguna irregularidad
    public IEnumerable<Condition> ParseConditionSet()
    {
        List<Condition> answer = new List<Condition>();
        if (!Stream.MoveNext(TokenValues.conditionset))
        {
            throw new Exception($"ConditionSet expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        if (!Stream.MoveNext(TokenValues.OpenCurlyBrackets))
        {
            throw new Exception($"Open Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        while (Stream.MoveNext(TokenValues.condition))
        {
            // Esta instruccion parsea tantas condiciones como defina el usuario, las cuales quedaran todas incluidas en la lista
            // answer
            answer.Add(ParseCondition());
        }

        if (!Stream.MoveNext(TokenValues.ClosedCurlyBrackets))
        {
            throw new Exception($"Closed Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        return answer;
    }
    
    // El metodo ParseInstructionSet se encarga de parsear el conjunto de instrucciones que contiene el poder, velando siempre
    // por que se respete la sintaxis y lanzando excepcion en caso de que ocurra alguna irregularidad
    public IEnumerable<Instruction> ParseInstructionSet()
    {
        List<Instruction> answer = new List<Instruction>();
        if (!Stream.MoveNext(TokenValues.instructionset))
        {
            throw new Exception($"InstructionSet expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        if (!Stream.MoveNext(TokenValues.OpenCurlyBrackets))
        {
            throw new Exception($"Open Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        while (Stream.MoveNext(TokenValues.instruction))
        {
            // Esta instruccion parsea tantas instrucciones como defina el usuario, las cuales quedaran todas incluidas en la lista
            // answer
            answer.Add(ParseInstruction());
        }

        if (!Stream.MoveNext(TokenValues.ClosedCurlyBrackets))
        {
            throw new Exception($"Closed Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        return answer;
    }
    Expressions ParseExpression(string type)
    {
        Expressions answer = null;
        Stream.MoveForward(1); 
        if (type == TokenValues.arithmeticexpression)
        {
            Stream.MoveBackWard(1);
            answer = ParseArithmeticExpression();
        }  
        else if (type== TokenValues.booleanexpression)
        {
            answer = ParseBooleanExpression();
        }
        else if (type == TokenValues.powerset)
        {
            answer = ParsePowerSet();
        }  
        else if (type == TokenValues.textexpression)
        {
            answer = ParseTextExpression();
        }
        else
        {
            throw new Exception("Wrong expression");
        }        
        return answer;
    }
    TextExpression ParseTextExpression()
    {
        TextExpression answer = null;
        if (Stream.LookAhead().Type != TokenType.identifier) throw new Exception($"ID expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        answer = new TextExpression(Stream.LookAhead().Value);
        if(!Stream.MoveNext(TokenValues.StatementSeparator)) throw new Exception($"; expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");     
        return answer;
    }
    // El metodo ParseArithmeticExpression se encarga de parsear expresiones aritmeticas. El metodo realiza una prueba para ver
    // que expresion especifica ha de parsear y llama al metodo correspondiente en consecuencia
    ArithmeticExpressions ParseArithmeticExpression()
    {
        ArithmeticExpressions answer = null;
        Stream.MoveNext();
        switch (Stream.LookAhead().Value)
        {
            case TokenValues.sum:
                // En este caso se parsea una suma
                answer = ParseSum();
                break;
            case TokenValues.sub:
                // En este caso se parsea una resta
                answer = ParseSub();
                break;
            case TokenValues.mult:
                // En este caso se parsea una multiplicacion
                answer = ParseMult();
                break;
            case TokenValues.div:
                // En este caso se parsea una division
                answer = ParseDiv();
                break;
            case TokenValues.damage:
                // En este caso se parsea el ataque de una carta
                answer = ParseCardDamage();
                break;
            case TokenValues.damagein:
                // En este caso se parsea el ataque acumulado de una zona del campo
                answer = ParseDamageIn();
                break;
            case TokenValues.numberofcardsin:
                // En este caso se parsea el numero de cartas de una zona del campo
                answer = ParseNumberOfCardsIn();
                break;
            default:
                if (Stream.LookAhead().Type == TokenType.number)
                {
                    // En este caso se parsea un numero
                    answer = ParseNumber();
                }
                // Aqui se lanza una excepcion acorde al contexto actual
                else throw new Exception($"Number expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                break;
        }
        return answer;
    }
    
    // El metodo ParseSum se encarga de parsear expresiones suma, velando siempre por que se cumpla con la sintaxis definida
    // y lanzando excepcion oportunamente en caso de cualquier anormalidad
    ArithmeticExpressions ParseSum()
    {
        ArithmeticExpressions left;
        ArithmeticExpressions right;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion Parsea la expresion pasada como primer argumento de nuestra suma
        left = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion Parsea la expresion pasada como segundo argumento de nuestra suma
        right = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui retornamos una nueva expresion suma con las dos expresiones parseadas
        return new Sum(left, right);
    }
    
    // El metodo ParseSub se encarga de parsear expresiones resta, velando siempre por que se cumpla con la sintaxis definida
    // y lanzando excepcion oportunamente en caso de cualquier anormalidad
    ArithmeticExpressions ParseSub()
    {
        ArithmeticExpressions left;
        ArithmeticExpressions right;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion Parsea la expresion pasada como primer argumento de nuestra resta
        left = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion Parsea la expresion pasada como segundo argumento de nuestra resta
        right = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui retornamos una nueva expresion resta con las dos expresiones parseadas
        return new Sub(left, right);
    }
    
    // El metodo ParseMult se encarga de parsear expresiones multiplicacion, velando siempre por que se cumpla con la sintaxis definida
    // y lanzando excepcion oportunamente en caso de cualquier anormalidad
    ArithmeticExpressions ParseMult()
    {
        ArithmeticExpressions left;
        ArithmeticExpressions right;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion Parsea la expresion pasada como primer argumento de nuestra multiplicacion
        left = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion Parsea la expresion pasada como segundo argumento de nuestra multiplicacion
        right = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui retornamos una nueva expresion multiplicacion con las dos expresiones parseadas
        return new Mult(left, right);
    }
    
    // El metodo ParseMult se encarga de parsear expresiones division, velando siempre por que se cumpla con la sintaxis definida
    // y lanzando excepcion oportunamente en caso de cualquier anormalidad
    ArithmeticExpressions ParseDiv()
    {
        ArithmeticExpressions left;
        ArithmeticExpressions right;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion Parsea la expresion pasada como primer argumento de nuestra division
        left = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion Parsea la expresion pasada como segundo argumento de nuestra division
        right = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui retornamos una nueva expresion multiplicacion con las dos expresiones parseadas
        return new Div(left, right);
    }
    
    // El metodo ParseNumber se encarga de parsear expresiones Numero
    ArithmeticExpressions ParseNumber()
    {
        // Aqui retornamos una expresion Numero con la conversion a valor entero de el token actual de la lista
        return new Number(int.Parse(Stream.LookAhead().Value));
    }
    
    // El metodo ParseCardDamage se encarga de parsear una expresion de tipo numero referente al ataque de una carta de tipo unidad
    // Esto puede ser directamente el ataque de una carta o el mayor/menor ataque de una zona determinada. En otro caso se lanzara 
    // excepcion de manera acorde al fallo cometido
    ArithmeticExpressions ParseCardDamage()
    {
        string name = null;
        MethodInfo Method = null;
        string zone = null;
        string Value = null;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui se chequea que esten nombrando una carta especifica y se almacena su nombre
        if (Stream.MoveNext(TokenValues.card))
        {
            if (Stream.MoveNext(TokenType.identifier))
            {
                name = Stream.LookAhead().Value;
            }
        }
        // Aqui se chequea que esten haciendo referencia a la carta de mayor/menor ataque en una zona dada y se procede a comprobar
        // la correctitud del codigo escrito por el usuario
        else if (Stream.MoveNext(TokenValues.highestattackin) || Stream.MoveNext(TokenValues.lowestattackin))
        {
            switch (Stream.LookAhead().Value)
            {   
                // Aqui se identifica el caso en el que se hace referencia al mayor ataque de una zona determinada
                case TokenValues.highestattackin:
                    if (!Stream.MoveNext(TokenValues.OpenBracket))
                    {
                        throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    }
                    // En esta instruccion se identifica si la zona en cuestion es valida y en caso positivo se almacena el nombre de  la zona 
                    // como un string, se almacena un MethodInfo que hace referencia a la funcion Llama de la clase FieldZones y un string
                    //que sirve para identificar que buscamos el mayor ataque en dicha zona. Estos parametro se emplean para crear una expresion
                    // aritmetica de tipo Numbermas adelante
                    if (Stream.FieldZoness.Contains(Stream.LookAhead(1).Value))
                    {
                        Stream.MoveNext();
                        zone = Stream.LookAhead().Value;
                        Type MyType = Type.GetType("FieldZones");
                        Method = MyType.GetMethod("Llama");
                        Value = TokenValues.highestattackin;
                        Stream.MoveForward(1);
                    }
                    else throw new Exception($"Field Zone expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    if (!Stream.MoveNext(TokenValues.ClosedBracket))
                    {
                        throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    }
                    break;
                // Aqui se identifica el caso en el que se hace referencia al menor ataque de una zona determinada
                case TokenValues.lowestattackin:
                    if (!Stream.MoveNext(TokenValues.OpenBracket))
                    {
                        throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    }
                    // En esta instruccion se identifica si la zona en cuestion es valida y en caso positivo se almacena el nombre de  la zona 
                    // como un string, se almacena un MethodInfo que hace referencia a la funcion Llama de la clase FieldZones y un string
                    //que sirve para identificar que buscamos el menor ataque en dicha zona. Estos parametro se emplean para crear una expresion
                    // aritmetica de tipo Numbermas adelante
                    if (Stream.FieldZoness.Contains(Stream.LookAhead(1).Value))
                    {
                        Stream.MoveNext();
                        zone = Stream.LookAhead().Value;
                        Type MyType = Type.GetType("FieldZones");
                        Method = MyType.GetMethod("Llama");
                        Value = TokenValues.lowestattackin;
                        Stream.MoveForward(1);
                    }
                    else throw new Exception($"Field Zone expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    if (!Stream.MoveNext(TokenValues.ClosedBracket))
                    {
                        throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    }
                    break;
                default:
                    break;
            }
        }
        else throw new Exception($"Card or Card return function expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instrucion se chequea que el nombre de la carta no sea null, y de ser asi se usa como argumento para crear una
        // expresion aritmetica de tipo Number que nos devolvera el ataque de la carta cuando sea evaluada
        if (!(name is null))
        {
            return new Number(name);
        }
        // En esta instruccion se crea un objeto de tipo number pasando como parametros un Methodinfo que referencia al metodo Llama
        // de la clase FieldZones, un string Value que indica si buscamos el mayor o menor ataque y un string zone que indica en que
        // zona lo buscamos
        else
        {
            return new Number(Method, Value, zone);
        }

    }
   
    // El metodo ParseDamageIn se encarga de parsear una expresion de tipo numero referente al ataque de las cartas de tipo unidad
    // de una zona del juego. Este metodo comprueba que se respete la sintaxis definida para el lenguaje y se pasen los valores adecuados
    // En otro caso se lanzara excepcion de manera acorde al fallo cometido
    
    ArithmeticExpressions ParseDamageIn()
    {
        MethodInfo Method;
        string methodName;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui se comprueba que se este recibiendo como argumento una zona del juego valida y se procede a almacenar su nombre en
        // la variable methodName, junto con un MethodInfo que hace referencia a la funcion Llama de la clase FieldZones. Estos 
        // elementos se pasan como parametros para obtener una expresion aritetica de tipo Number mas adelante. Si la zona del juego
        // especificada no es valida (no existe) se lanza una excepcion de manera acorde
        if (Stream.FieldZoness.Contains(Stream.LookAhead(1).Value))
        {
            methodName = Stream.LookAhead(1).Value;
            Type MyType = Type.GetType("FieldZones");
            Method = MyType.GetMethod("Llama");
            Stream.MoveForward(1);
        }
        else throw new Exception($"Field Zone expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se crea un objeto de tipo number pasando como parametros un Methodinfo que referencia al metodo Llama
        // de la clase FieldZones, un string con el valor de TokenValues.damagein y un string zone que indica en que zona buscamos
        // el ataque acumulado
        return new Number(Method, TokenValues.damagein, methodName);
    }
    
    // El metodo ParseNumberOfCardsIn se encarga de parsear una expresion de tipo numero referente a la cantidad de cartas
    // de una zona del juego. Este metodo comprueba que se respete la sintaxis definida para el lenguaje y se pasen los valores adecuados
    // En otro caso se lanzara excepcion de manera acorde al fallo cometido
    ArithmeticExpressions ParseNumberOfCardsIn()
    {
        MethodInfo Method;
        string methodName;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui se comprueba que se este recibiendo como argumento una zona del juego valida y se procede a almacenar su nombre en
        // la variable methodName, junto con un MethodInfo que hace referencia a la funcion Llama de la clase FieldZones. Estos 
        // elementos se pasan como parametros para obtener una expresion aritetica de tipo Number mas adelante. Si la zona del juego
        // especificada no es valida (no existe) se lanza una excepcion de manera acorde
        if (Stream.FieldZoness.Contains(Stream.LookAhead(1).Value))
        {
            methodName = Stream.LookAhead(1).Value;
            Type MyType = Type.GetType("FieldZones");
            Method = MyType.GetMethod("Llama");
            Stream.MoveForward(1);
        }
        else throw new Exception($"Field Zone expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se crea un objeto de tipo number pasando como parametros un Methodinfo que referencia al metodo Llama
        // de la clase FieldZones, un string con el valor de TokenValues.numberofcardsin y un string zone que indica en que zona buscamos
        // el numero de cartas existentes
        return new Number(Method, TokenValues.numberofcardsin, methodName);
    }
    
    // El metodo ParseBooleanExpression se encarga de parsear expresiones booleanas. El metodo realiza una prueba para ver
    // que expresion especifica ha de parsear y llama al metodo correspondiente en consecuencia
    BooleanExpresion ParseBooleanExpression()
    {
        BooleanExpresion answer = null;
        Stream.MoveNext();
        switch (Stream.LookAhead().Value)
        {
            case TokenValues.not:
            // Aqui se parsea una expresion Not
                answer = ParseNot();
                break;
            case TokenValues.and:
            // Aqui se parsea una expresion And
                answer = ParseAnd();
                break;
            case TokenValues.or:
            // Aqui se parsea una expresion Or
                answer = ParseOr();
                break;
            case TokenValues.binarycomparer:
            // Aqui se parsea una expresion BinaryComparer
                answer = ParseBinaryComparer();
                break;
            case TokenValues.truepredicate:
            // Aqui se parsea una expresion True predicate
                answer = ParseTruePredicate();
                break;
            case TokenValues.falsepredicate:
            // Aqui se parsea una expresion False predicate
                answer = ParseFalsePredicate();
                break;
            case TokenValues.existcardin:
            // Aqui se parsea una expresion Exist
                answer = ParseExistCardIn();
                break;
            default:
            // En este caso se lanza una excepcion, pues no coincide el argumento con ninguna expresion booleana valida
                throw new Exception($"Boolean operator expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                break;
        }
        return answer;
    }
    
    // El metodo ParseNot se encarga de parsear expresiones de negacion logico, velando siempre por que se cumpla con la sintaxis definida
    // y lanzando excepcion oportunamente en caso de cualquier anormalidad
    BooleanExpresion ParseNot()
    {
        BooleanExpresion answer;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui se procede a parsear la expresion booleana que recibe la expresion Not
        answer = ParseBooleanExpression();
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se devuelve una nueva expresion de tipo Not que recibe como argumento la expresion previamente parseada
        return new Not(answer);
    }
    
    // El metodo ParseAnd se encarga de parsear expresiones de conjuncion logica, velando siempre por que se cumpla con la sintaxis definida
    // y lanzando excepcion oportunamente en caso de cualquier anormalidad
    BooleanExpresion ParseAnd()
    {
        BooleanExpresion left;
        BooleanExpresion right;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion parsea la expresion booleana pasada como primer argumento a nuestra expresion And
        left = ParseBooleanExpression();
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion parsea la expresion booleana pasada como segundo argumento a nuestra expresion And
        right = ParseBooleanExpression();
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui retornamos una nueva expresion And con las dos expresiones parseadas
        return new And(left, right);
    }
    
    // El metodo ParseOr se encarga de parsear expresiones de disyuncion logica, velando siempre por que se cumpla con la sintaxis definida
    // y lanzando excepcion oportunamente en caso de cualquier anormalidad
    BooleanExpresion ParseOr()
    {
        BooleanExpresion left;
        BooleanExpresion right;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion parsea la expresion booleana pasada como primer argumento a nuestra expresion Or
        left = ParseBooleanExpression();
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Esta instruccion parsea la expresion booleana pasada como segundo argumento a nuestra expresion Or
        right = ParseBooleanExpression();
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui retornamos una nueva expresion Or con las dos expresiones parseadas
        return new Or(left, right);
    }
    
    // El metodo ParseBinaryComparer se encarga de parsear expresiones de comparador binario, velando siempre por que se cumpla 
    // con la sintaxis definida y lanzando excepcion oportunamente en caso de cualquier anormalidad
    
    BooleanExpresion ParseBinaryComparer()
    {
        ArithmeticExpressions left;
        ArithmeticExpressions right;
        BooleanComparer criteria;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se parsea la expresion aritmetica pasada como primer argumento a nuestro comparador binario
        left = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se parsea la expresion aritmetica pasada como segundo argumento a nuestro comparador binario
        right = ParseArithmeticExpression();
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aeui se comprueba que el comparador booleano esperado sea valido y se realiza la prueba por casos para saber con cual
        // de los que ya tenemos definidos se corresponde. En caso de no ser valido se lanza una excepcion
        if (Stream.BooleanComparers.Contains(Stream.LookAhead(1).Value))
        {
            Stream.MoveNext();
            switch (Stream.LookAhead().Value)
            {
                case TokenValues.Equal:
                    criteria = BooleanComparers.Equal;
                    break;
                case TokenValues.Major:
                    criteria = BooleanComparers.Major;
                    break;
                case TokenValues.Minor:
                    criteria = BooleanComparers.Minor;
                    break;
                case TokenValues.MajorOrEqual:
                    criteria = BooleanComparers.MajorOrEqual;
                    break;
                case TokenValues.MinorOrEqual:
                    criteria = BooleanComparers.MinorOrEqual;
                    break;
                default:
                    throw new Exception($"Comparisson criteria expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
            }
        }
        else throw new Exception($"Comparer criteria expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui se retorna una nueva expresion de tipo BinaryComparer con las dos instrucciones previamente parseadas y el comparador
        // booleano introducido por el usuario
        return new BinaryComparer(left, right, criteria);
    }
    
    // El metodo ParseTruePredicate devuelve una expresion de tipo TruePredicate
    BooleanExpresion ParseTruePredicate()
    {
        return new TruePredicate();
    }
    
    // El metodo ParseFalsePredicate devuelve una expresion de tipo FalsePredicate
    BooleanExpresion ParseFalsePredicate()
    {
        return new FalsePredicate();
    }
    
    // El metodo ParseExistCardIn parsea expresiones booleanas Exist, velando siempre por que se cumpla 
    // con la sintaxis definida y lanzando excepcion oportunamente en caso de cualquier anormalidad
    BooleanExpresion ParseExistCardIn()
    {
        string cardname = null;
        string zone = null;
        MethodInfo Method = null;
        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName}  in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se vela por que se reciba como primer parametro una carta
        if (!Stream.MoveNext(TokenValues.card))
        {
            throw new Exception($"Card expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se comprueba que la carta posea un identificador valido
        if (!Stream.MoveNext(TokenType.identifier))
        {
            throw new Exception($"Identifier expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se almacena el nombre de la carta en la variable de string cardname
        cardname = Stream.LookAhead().Value;
        if (!Stream.MoveNext(TokenValues.ValueSeparator))
        {
            throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui se comprueba que se este recibiendo una zona del juego valida, y se almacena el nombre de la zona en la variable de
        // string zone, asi como un MethodInfo que hace referencia a la funcion Llama de la clase FieldZones y lo almacena en la variable
        // Method. En caso de no ser una zona del juego valida (no existir) se lanza una excepcion acorde
        if (Stream.FieldZoness.Contains(Stream.LookAhead(1).Value))
        {
            zone = Stream.LookAhead(1).Value;
            Type MyType = Type.GetType("FieldZones");
            Method = MyType.GetMethod("Llama");
            Stream.MoveForward(1);
        }
        else throw new Exception($"Field Zone expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se retorna una nueva expresion Exist con el nombre de la carta a chequear, el MethodInfo Method
        // y la zona del juego en la cual se quiere comprobar la existencia de la carta
        return new Exist(cardname, Method, zone);
    }
    
    // El metodo ParseCondition parsea una condicion perteneciente al ConditionSet de la clase Power, velando siempre por que se cumpla 
    // con la sintaxis definida y lanzando excepcion oportunamente en caso de cualquier anormalidad
    public Condition ParseCondition()
    {
        BooleanExpresion answer;
        if (!(Stream.LookAhead().Value == TokenValues.condition))
        {
            throw new Exception($"Condition expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        if (!Stream.MoveNext(TokenValues.OpenCurlyBrackets))
        {
            throw new Exception($"Open Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se comprueba que se reciba un operador booleano valido y se procede a parsear la expresion booleana 
        // en cuestion. En caso de no ser un operador booleano valido (no existe) se lanza una excepcion acorde
        if (Stream.BooleanOperators.Contains(Stream.LookAhead(1).Value))
        {            
            answer = ParseBooleanExpression();
        }
        else throw new Exception($"Boolean Operator expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        if (!Stream.MoveNext(TokenValues.ClosedCurlyBrackets))
        {
            throw new Exception($"Closed Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // Aqui se retorna una nueva Condition con la expresion booleana parseada como parametro
        return new Condition(answer);
    }
    
    // El metodo ParseInstruction parsea una instruccion perteneciente al ConditionSet de la clase Power, velando siempre por
    // que se cumpla con la sintaxis definida y lanzando excepcion oportunamente en caso de cualquier anormalidad
     
    public Instruction ParseInstruction()
    {
        string name;
        List<object> parameters = new List<object>();
        if (Stream.LookAhead().Value != TokenValues.instruction)
        {
            throw new Exception($"Instruction expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        if (!Stream.MoveNext(TokenValues.OpenCurlyBrackets))
        {
            throw new Exception($"Open Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // En esta instruccion se comprueba que la instruccion se corresponda con una funcion definida en la clase Process
        // (funcionalidades del juego) y se almacena el nombre de la funcion en la variable de string name. En caso de que no sea
        // el caso se lanza una excepcion acorde
        if (Stream.ProcessFunctions.Contains(Stream.LookAhead(1).Value))
        {
            name = Stream.LookAhead(1).Value;
            Stream.MoveForward(1);
        }
        else throw new Exception($"Process function expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");

        if (!Stream.MoveNext(TokenValues.OpenBracket))
        {
            throw new Exception($"( expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        // El metodo ParseDestroy se encarga de parsear la instruccion destruir cartas, asociada a la funcion Destroy de la clase Process
        // La funcion Destroy recibe como argumento un valor entero que indica la cantidad de cartas a destruir, o una lista de
        // string con los nombres de las cartas especificas a destruir, o un objeto de tipo MethodInfo que indica una zona especifica
        // del campo en la cual todas las cartas van a ser destruidas. Esto es garantizado por el metodo
        List<object> ParseDestroy()
        {
            List<object> result = new List<object>();
            // En esta instruccion se comprueba si el siguiente token es una zona del campo valida y de ser asi se procede a verificar
            // si es una zona de cartas de tipo unidad ("cuerpo a cuerpo", "a distancia" o "asedio") de cualquier jugador. En ese
            // caso se crea un objeto de tipo Methodinfo que hace referencia a una funcion de la clase FieldZones correspondiente
            // con la zona indicada y se agrega a la lista de returno. En caso contrario se procede a analizar las posibilidades restantes
            if (Stream.FieldZoness.Contains(Stream.LookAhead(1).Value))
            {
                string methodName = Stream.LookAhead(1).Value;
                if (methodName == TokenValues.ownmelee || methodName == TokenValues.ownmiddle || methodName == TokenValues.ownsiege || methodName == TokenValues.enemymelee || methodName == TokenValues.enemymiddle || methodName == TokenValues.enemysiege)
                {
                    Type MyType = Type.GetType("FieldZones");
                    MethodInfo Method = MyType.GetMethod(methodName);
                    result.Add(Method);
                    Stream.MoveForward(1);
                }
                else throw new Exception($"Wrong Field Zone chosen in line:{Stream.LookAhead(1).Location.Line}");
            }
            // En esta instrucion se comprueba si el siguiente token es un numero. En caso positivo se agrega a la lista de retorno.
            // en caso contrario se procede a analizar la posibilidad restante
            else if (Stream.LookAhead(1).Type is TokenType.number || Stream.ArithmeticOperators.Contains(Stream.LookAhead(1).Value))
            {
                if(Stream.LookAhead(1).Type is TokenType.number) result.Add(int.Parse(Stream.LookAhead(1).Value));
                else
                {
                    Stream.MoveForward(1);
                    result.Add(ParseArithmeticExpression());
                }
                Stream.MoveForward(1);
            }
            //En esta instruccion se chequea que el token siguiente sea una carta. De ser asi se procede a entrar en un bucle que le permite
            // al usuario declarar tantas cartas como desee y se van agregando los nombres de estas a una lista de string que se agregara
            // a su vez a la lista de retorno. En caso contrario se lanza una excepcion, pues los argumentos recibidos no coinciden con
            // los definidos en el lenguaje   
            else if (Stream.LookAhead(1).Value == TokenValues.card)
            {
                List<string> names = new List<string>();
                while (Stream.MoveNext(TokenValues.card))
                {
                    string name0;
                    if (!Stream.MoveNext(TokenType.identifier))
                    {
                        throw new Exception("ID expected in card {Stream.LookAhead().Location.CardName}");
                    }
                    name0 = Stream.LookAhead().Value;
                    if (!Stream.MoveNext(TokenValues.ValueSeparator))
                    {
                        if (Stream.LookAhead(1).Value != TokenValues.ClosedBracket) throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    }
                    names.Add(name0);
                }
                result.Add(names);
            }
            else throw new Exception("Wrong arguments");
            // Aqui se devuelve la lista de objetos que pasaremos a la funcion como parametros
            return result;
        }
        
        // El metodo ParseDraw se encarga de parsear la instruccion robar cartas, asociada a la funcion Draw de la clase Process
        // La funcion Draw recibe como argumento un valor entero que indica la cantidad de cartas a robar. Esto es garantizado
        // por el metodo
        List<Object> ParseDraw()
        {
            List<object> result = new List<object>();
            // Aqui se comprueba que el Token recibido posea un valor numerico y de ser asi se agrega a la lista de objetos que 
            // vamos a devolver. En caso contrario se lanza una excepcion indicando que se esperaba un numero
            if (Stream.LookAhead(1).Type is TokenType.number || Stream.ArithmeticOperators.Contains(Stream.LookAhead(1).Value))
            {
                if (Stream.LookAhead(1).Type is TokenType.number)
                {
                    result.Add(int.Parse(Stream.LookAhead(1).Value));
                    Stream.MoveForward(1);
                }
                else
                {
                    Stream.MoveForward(1);
                    result.Add(ParseArithmeticExpression());
                }
            }
            else throw new Exception("Number of cards expected in card {Stream.LookAhead().Location.CardName}");
            // Aqui se devuelve la lista de objetos que pasaremos a la funcion como parametros
            return result;
        }
        
        // El metodo ParseReborn se encarga de parsear la instruccion revivir cartas, asociada a la funcion Reborn de la clase Process
        // La funcion Reborn recibe como argumento una palabra clave que indica el cementerio desde el cual se va a revivir y ademas
        // recibe un numero que indica la cantidad de cartas que vamos a revivir o una lista de string con los nombres de las cartas
        // especificas que vamos a revivir. Esto es garantizado por el metodo        
        List<Object> ParseReborn()
        {
            List<object> answer = new List<object>();
            // Si el proximo token de la lista coincide con una de las dos palanbras clave asociadas a los cementerios
            // se procede a analizar la naturaleza del segundo argumento recibido. En caso contrario se lanza una excepcion acorde
            if (Stream.LookAhead(1).Value == TokenValues.owngraveryard || Stream.LookAhead(1).Value == TokenValues.enemygraveryard)
            {
                Stream.MoveForward(1);
                string methodName = Stream.LookAhead().Value;
                Type MyType = Type.GetType("FieldZones");
                // En dependencia de el cementerio indicado se guardara una referencia al metodo de la clase FieldZones correspondiente 
                // a esa zona del campo y se agrega a la lista de retorno
                MethodInfo Method = MyType.GetMethod(methodName);
                answer.Add(Method);
                if (!Stream.MoveNext(TokenValues.ValueSeparator)) throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                // Si el siguiente Token de la lista es un numero entonces se agrega e la lista de retorno. De no ser el caso se
                // procede a comprobar si es una carta, y en caso contrario lanza una excepcion
                if (Stream.MoveNext(TokenType.number))
                {
                    answer.Add(int.Parse(Stream.LookAhead().Value));
                }
                else if (Stream.ArithmeticOperators.Contains(Stream.LookAhead(1).Value))
                {
                    Stream.MoveForward(1);
                    answer.Add(ParseArithmeticExpression());
                }
                else if (Stream.LookAhead(1).Value == TokenValues.card)
                {
                    List<string> names = new List<string>();
                    // El usuario puede no solo querer revivir una carta, sino un conjunto de cartas, por lo cual vamos a entrar en un bucle
                    // que le permita seguir definiendo tantas cartas como quiera
                    while (Stream.MoveNext(TokenValues.card))
                    {
                        string name2;
                        if (!Stream.MoveNext(TokenType.identifier))
                        {
                            throw new Exception("ID expected in card {Stream.LookAhead().Location.CardName}");
                        }
                        name2 = Stream.LookAhead().Value;
                        if (!Stream.MoveNext(TokenValues.ValueSeparator))
                        {
                            if (Stream.LookAhead(1).Value != TokenValues.ClosedBracket) throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                        }
                        names.Add(name2);
                    }
                    // En esta instruccion se agrega cada nombre de las cartas a una lista de string que sera agregada posteriormente
                    // a la lista de retorno
                    answer.Add(names);
                }
                else throw new Exception("Number or cards expected in card {Stream.LookAhead().Location.CardName}");
            }

            else throw new Exception("Graveryard Selection expected in card {Stream.LookAhead().Location.CardName}");
            // Aqui se devuelve la lista de objetos que pasaremos a la funcion como parametros
            return answer;
        }
        
        // El metodo ParseSummon se encarga de parsear la instruccion invocar cartas, asociada a la funcion Summon de la clase Process
        // La funcion Summon recibe un numero indicando la cantidad de cartas que vamos a invocar o una lista con los nombres de las cartas
        // que vamos a invocar. Esto es garantizado por el metodo
        List<Object> ParseSummon()
        {
            List<object> answer = new List<object>();
            // Aqui se comprueba si el siguiente token de la lista es un numero. En caso positivo se agrega a la lista que vamos a
            // retornar y en caso negativo se procede a analizar la otra posibilidad
            if (Stream.LookAhead(1).Type is TokenType.number)
            {
                answer.Add(int.Parse(Stream.LookAhead(1).Value));
                Stream.MoveForward(1);
            }
            else if(Stream.ArithmeticOperators.Contains(Stream.LookAhead(1).Value))
            {
                Stream.MoveForward(1);
                answer.Add(ParseArithmeticExpression());
            }
            // Si el siguiente token de la lista no es un numero, debe ser una carta especifica, por lo cual se comprueba dicho aspecto
            // De no ser asi se lanza una excepcion, pues el argumento recibido no se corresponden con los definidos en el lenguaje
            else if (Stream.LookAhead(1).Value == TokenValues.card)
            {
                
                List<string> cardnames = new List<string>();
                // El usuario puede no solo querer invocar una carta, sino un conjunto de cartas, por lo cual vamos a entrar en un bucle
                // que le permita seguir definiendo tantas cartas como quiera
                while (Stream.MoveNext(TokenValues.card))
                {
                    string name3;
                    if (!Stream.MoveNext(TokenType.identifier))
                    {
                        throw new Exception("ID expected in card {Stream.LookAhead().Location.CardName}");
                    }
                    name3 = Stream.LookAhead().Value;
                    if (!Stream.MoveNext(TokenValues.ValueSeparator))
                    {
                        if (Stream.LookAhead(1).Value != TokenValues.ClosedBracket) throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    }
                    // En esta instruccion se agrega cada nombre de las cartas a una lista de string que sera agregada posteriormente
                    // a la lista de retorno
                    cardnames.Add(name3);
                }
                answer.Add(cardnames);
            }
            else throw new Exception("Number or cards expected in card {Stream.LookAhead().Location.CardName}");
            // Aqui se devuelve la lista de objetos que pasaremos a la funcion como parametros
            return answer;
        }
        
        // El metodo ParseModifyAttack se encarga de parsear la instruccion modificar el ataque de las cartas, asociada a la 
        // funcion ModifyAttack de la clase Process. La funcion ModifyAttack recibe como argumentos un numero que indica 
        // la cantidad en la cual va a ser modificado el etaque y recibe ademas: un numero que indica la cantidad de cartas a las cuales
        // modificar el ataque, o un objeto de tipo MethodInfo que indica una zona del campo especifica en la cual todas las cartas de tipo unidad van a ser modificadas, o una
        // palabra clave que permitira escoger esta zona del campo en tiempo de ejecucion o una lista de string con los nombres de
        // las cartas especificas a modificar. Esto es garantizado por el metodo
        List<object> ParseModifyAttack()
        {
            List<object> answer = new List<object>();
            // Aqui se confirma que el siguiente token de la lista sea un numero y se agrega a la lista de retorno, y en caso contrario 
            // se lanza una excepcion acorde           
            if (Stream.MoveNext(TokenType.number))
            {
                answer.Add(int.Parse(Stream.LookAhead().Value));
                if (!Stream.MoveNext(TokenValues.ValueSeparator))
                {
                    throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                }
                //En esta instruccion se chequea que el token siguiente sea un numero. De ser asi se agrega a la lista de retorno. En caso
                // contrario se procede a explorar las opciones restantes
                if (Stream.MoveNext(TokenType.number))
                {
                    answer.Add(int.Parse(Stream.LookAhead().Value));
                }
                else if (Stream.ArithmeticOperators.Contains(Stream.LookAhead(1).Value))
                {
                    Stream.MoveForward(1);
                    answer.Add(ParseArithmeticExpression());
                }
                //En esta instruccion se chequea que el token siguiente sea la palabra clave que indica la posibilidad de elegir la 
                //zona del campo en tiempo de ejecucion. De ser asi se agrega a la lista de retorno. En caso contrario se procede                
                //a explorar las opciones restantes
                else if (Stream.MoveNext(TokenValues.freeelection))
                {
                    answer.Add(Stream.LookAhead().Value);
                }
                //En esta instruccion se chequea que el token siguiente sea una carta. De ser asi se procede a entrar en un bucle que le permite
                // al usuario declarar tantas cartas como desee y se van agregando los nombres de estas a una lista de string que se agregara
                // a su vez a la lista de retorno. En caso contrario se procede a explorar la opcion restante                  
                else if (Stream.LookAhead(1).Value == TokenValues.card)
                {
                    List<string> names = new List<string>();
                    while (Stream.MoveNext(TokenValues.card))
                    {
                        string name4;
                        if (!Stream.MoveNext(TokenType.identifier))
                        {
                            throw new Exception("ID expected in card {Stream.LookAhead().Location.CardName}");
                        }
                        name4 = Stream.LookAhead().Value;
                        if (!Stream.MoveNext(TokenValues.ValueSeparator))
                        {
                            if (Stream.LookAhead(1).Value != TokenValues.ClosedBracket) throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead(1).Location.Line}");
                        }
                        names.Add(name4);
                    }
                    answer.Add(names);
                }
                //En esta instruccion se chequea que el token siguiente sea una zona valida del campo. En caso contrario se habran explorado
                // todas las posibilidades y se lanzara una excepcion
                else if (Stream.FieldZoness.Contains(Stream.LookAhead(1).Value))
                {
                    string methodName = Stream.LookAhead(1).Value;
                    // Aqui se comprueba que la zona designada sea una zona de cartas de tipo unidad ("cuerpo a cuerpo", "a distancia"
                    // o "asedio") de cualquier jugador. En caso de ser asi se procede a crear un objeto de tipo Methodinfo que hace
                    // referencia a una funcion de la clase FieldZones correspondiente a la zona designada y se agrega a la lista de
                    // retorno. En caso contrario se lanza una excepcion acorde
                    if (methodName == TokenValues.ownmelee || methodName == TokenValues.ownmiddle || methodName == TokenValues.ownsiege || methodName == TokenValues.enemymelee || methodName == TokenValues.enemymiddle || methodName == TokenValues.enemysiege)
                    {
                        Type MyType = Type.GetType("FieldZones");                        
                        MethodInfo Method = MyType.GetMethod(methodName);
                        answer.Add(Method);
                        Stream.MoveForward(1);
                    }
                    else throw new Exception($"Wrong zone at line {Stream.LookAhead(1).Location.Line}");
                }
                else throw new Exception("Number of cards, cards or field zone expected in card {Stream.LookAhead().Location.CardName}");
            }     
            else if (Stream.ArithmeticOperators.Contains(Stream.LookAhead(1).Value))
            {
                Stream.MoveForward(1);
                answer.Add(ParseArithmeticExpression());
                if (!Stream.MoveNext(TokenValues.ValueSeparator))
                {
                    throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                }
                //En esta instruccion se chequea que el token siguiente sea un numero. De ser asi se agrega a la lista de retorno. En caso
                // contrario se procede a explorar las opciones restantes
                if (Stream.MoveNext(TokenType.number))
                {
                    answer.Add(int.Parse(Stream.LookAhead().Value));
                }
                else if (Stream.ArithmeticOperators.Contains(Stream.LookAhead(1).Value))
                {
                    Stream.MoveForward(1);
                    answer.Add(ParseArithmeticExpression());
                }
                //En esta instruccion se chequea que el token siguiente sea la palabra clave que indica la posibilidad de elegir la 
                //zona del campo en tiempo de ejecucion. De ser asi se agrega a la lista de retorno. En caso contrario se procede                
                //a explorar las opciones restantes
                else if (Stream.MoveNext(TokenValues.freeelection))
                {
                    answer.Add(Stream.LookAhead().Value);
                }
                //En esta instruccion se chequea que el token siguiente sea una carta. De ser asi se procede a entrar en un bucle que le permite
                // al usuario declarar tantas cartas como desee y se van agregando los nombres de estas a una lista de string que se agregara
                // a su vez a la lista de retorno. En caso contrario se procede a explorar la opcion restante                  
                else if (Stream.LookAhead(1).Value == TokenValues.card)
                {
                    List<string> names = new List<string>();
                    while (Stream.MoveNext(TokenValues.card))
                    {
                        string name4;
                        if (!Stream.MoveNext(TokenType.identifier))
                        {
                            throw new Exception("ID expected in card {Stream.LookAhead().Location.CardName}");
                        }
                        name4 = Stream.LookAhead().Value;
                        if (!Stream.MoveNext(TokenValues.ValueSeparator))
                        {
                            if (Stream.LookAhead(1).Value != TokenValues.ClosedBracket) throw new Exception($", expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead(1).Location.Line}");
                        }
                        names.Add(name4);
                    }
                    answer.Add(names);
                }
                //En esta instruccion se chequea que el token siguiente sea una zona valida del campo. En caso contrario se habran explorado
                // todas las posibilidades y se lanzara una excepcion
                else if (Stream.FieldZoness.Contains(Stream.LookAhead(1).Value))
                {
                    string methodName = Stream.LookAhead(1).Value;
                    // Aqui se comprueba que la zona designada sea una zona de cartas de tipo unidad ("cuerpo a cuerpo", "a distancia"
                    // o "asedio") de cualquier jugador. En caso de ser asi se procede a crear un objeto de tipo Methodinfo que hace
                    // referencia a una funcion de la clase FieldZones correspondiente a la zona designada y se agrega a la lista de
                    // retorno. En caso contrario se lanza una excepcion acorde
                    if (methodName == TokenValues.ownmelee || methodName == TokenValues.ownmiddle || methodName == TokenValues.ownsiege || methodName == TokenValues.enemymelee || methodName == TokenValues.enemymiddle || methodName == TokenValues.enemysiege)
                    {
                        Type MyType = Type.GetType("FieldZones");
                        MethodInfo Method = MyType.GetMethod(methodName);
                        answer.Add(Method);
                        Stream.MoveForward(1);
                    }
                    else throw new Exception($"Wrong zone at line {Stream.LookAhead(1).Location.Line}");
                }
                else throw new Exception("Number of cards, cards or field zone expected in card {Stream.LookAhead().Location.CardName}");
            }      
            else throw new Exception($"Increase ammount {Stream.LookAhead(1).Location.Line}");
            // Aqui se devuelve la lista de objetos que pasaremos a la funcion como parametros
            return answer;
        }
        
        // El metodo ParseSwitchBand se encarga de parsear la instruccion robar cartas, asociada a la funcion SwitchBand de la clase Process
        // La funcion SwitchBand recibe como argumento el nombre de una carta que indica la carta qu cambiaremos de bando. Esto es
        // garantizado por el metodo
        List<object> ParseSwitchBand()
        {
            List<object> answer = new List<object>();
            // Aqui se comprueba que el token recibido sea una carta, y en caso contrario se lanza una excepcion acorde
            if (!Stream.MoveNext(TokenValues.card))
            {
                throw new Exception($"Card expected in card {Stream.LookAhead().Location.CardName} in line: {Stream.LookAhead().Location.Line}");
            }
            // Aqui se comprueba que la carta posea un identificador valido, y en caso contrario se lanza una excepcion acorde
            if (!Stream.MoveNext(TokenType.identifier))
            {
                throw new Exception($"Card identifier expected in card {Stream.LookAhead().Location.CardName} in line: {Stream.LookAhead().Location.Line}");
            }
            // En esta instruccion se agrega a la lista de objetos el identificador recibido
            answer.Add(Stream.LookAhead().Value);
            // Aqui se devuelve la lista de objetos que pasaremos a la funcion como parametros
            return answer;
        }
        // En esta instruccion se realiza la prueba por casos para saber a cual funcion del Process debemos referirnos y se procede
        // a parsear la funcionalidad correspondiente en cada caso. Las funciones asociadas a cada funcionalidad devuelven una 
        // lista de objetos que son los parametros que se pasaran como argumentos a las funciones de la clase Process. La correctitud
        // de estos elementos la garantiza cada uno de los metodos que parsean las funcionalidades
        switch (name)
        {
            case TokenValues.destroy:
            // Aqui se parsea la funcionalidad Destruir cartas
                parameters = ParseDestroy();
                break;
            case TokenValues.draw:
            // Aqui se parsea la funcionalidad Robar cartas
                parameters = ParseDraw();
                break;
            case TokenValues.reborn:
            // Aqui se parsea la funcionalidad Revivir cartas
                parameters = ParseReborn();
                break;
            case TokenValues.summon:
            // Aqui se parsea la funcionalidad Invocar cartas
                parameters = ParseSummon();
                break;
            case TokenValues.modifyAttack:
            // Aqui se parsea la funcionalidad Modificar el ataque de las cartas
                parameters = ParseModifyAttack();
                break;
            case TokenValues.switchband:
            // Aqui se parsea la funcionalidad Cambiar de bando una carta
                parameters = ParseSwitchBand();
                break;
            default:
                break;
        }
        if (!Stream.MoveNext(TokenValues.ClosedBracket))
        {
            throw new Exception($") expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }

        if (!Stream.MoveNext(TokenValues.ClosedCurlyBrackets))
        {
            throw new Exception($"Closed Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        return new Instruction(name, parameters);
    }
    
    // El Metodo ParseUnitCard es el encargado de parsear las cartas de tipo unidad. El metodo se encarga de chequear Token
    // por Token que se respete la sintaxis definida en el lenguaje(en caso de que esto no ocurra se lanzara una excepcion acorde) 
    // y de ir parseando cada uno de los campos de la carta.
    public UnitCard ParseUnitCard()
    {
        string Name = "";
        string Path = "";
        List<Power> powers = new List<Power>();
        string Phrase = "";
        Cards.location Position = Cards.location.Melee;
        ArithmeticExpressions Damage = null;
        List<Expressions> externalProperties = new List<Expressions>();
        
        Dictionary<string, bool> Properties = new Dictionary<string, bool>();
        Properties.Add(TokenValues.name, false);
        Properties.Add(TokenValues.path, false);
        Properties.Add(TokenValues.phrase, false);
        Properties.Add(TokenValues.attack, false);
        Properties.Add(TokenValues.position, false);
        Properties.Add(TokenValues.powerset, false);

        if (!Stream.MoveNext(TokenValues.OpenCurlyBrackets))
        {
            throw new Exception($"Open Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        while (Stream.MoveNext(TokenValues.arithmeticexpression)||Stream.MoveNext(TokenValues.booleanexpression)||Stream.MoveNext(TokenValues.textexpression)||Stream.MoveNext(TokenValues.powerset))
        {
            string expressiontype = Stream.LookAhead().Value;
            if(expressiontype!=TokenValues.powerset)
            {
                System.Console.WriteLine(expressiontype);
                if(!Stream.MoveNext(TokenType.properties)) throw new Exception($"Property Expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
            }
            string property = Stream.LookAhead().Value;
            if (Properties[property]) throw new Exception($"{property} has already been defined in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
            if (expressiontype != TokenValues.powerset)if (!Stream.MoveNext(TokenValues.assign)) throw new Exception($"= expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
            switch (expressiontype)
            {
                case TokenValues.arithmeticexpression:                
                if (property == TokenValues.attack)
                {
                    Damage = (ArithmeticExpressions)ParseExpression(expressiontype);
                    Properties[property] = true;
                    if(!Stream.MoveNext(TokenValues.StatementSeparator)) throw new Exception("; expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                }
                else
                {
                    externalProperties.Add(((ArithmeticExpressions)ParseExpression(expressiontype)));
                    if(!Stream.MoveNext(TokenValues.StatementSeparator)) throw new Exception("; expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                }
                break;

                case TokenValues.booleanexpression:
                externalProperties.Add((BooleanExpresion)ParseExpression(expressiontype));
                if(!Stream.MoveNext(TokenValues.StatementSeparator)) throw new Exception("; expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                break;

                case TokenValues.textexpression:
                switch (property)
                {
                    case TokenValues.name:
                    Name = ((TextExpression)ParseExpression(expressiontype)).Value.ToString();
                    Properties[property] = true;
                    break;
                    case TokenValues.path:
                    Path = ((TextExpression)ParseExpression(expressiontype)).Value.ToString();
                    Properties[property] = true;
                    break;
                    case TokenValues.phrase:
                    Phrase = ((TextExpression)ParseExpression(expressiontype)).Value.ToString();
                    Properties[property] = true;
                    break;
                    case TokenValues.position:
                    switch (((TextExpression)ParseExpression(expressiontype)).Value.ToString())
                    {
                        case TokenValues.melee:
                        Position = Cards.location.Melee;
                        Properties[property] = true;
                        break;
                        case TokenValues.middle:
                        Position = Cards.location.Middle;
                        Properties[property] = true;
                        break;
                        case TokenValues.siege:
                        Position = Cards.location.Siege;
                        Properties[property] = true;
                        break;
                        default:
                        throw new Exception($"Wrong Position in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
                    }
                    break;
                    default:
                    externalProperties.Add((TextExpression)ParseExpression(expressiontype));
                    break;
                }
                break;
                case TokenValues.powerset:
                powers = ((PowerSet)ParseExpression(expressiontype)).List;
                Properties[property] = true;
                break;

            }
        }
        foreach (var item in Properties.Values)
        {
            if (!item)
            {
               throw new Exception("Not all basic properties initialized");
            }
        }
        if (!Stream.MoveNext(TokenValues.ClosedCurlyBrackets))
        {
            throw new Exception($"Closed Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        if(externalProperties.Count==0)return new UnitCard(Name, TokenValues.unitcard, Path, powers, Phrase, Position, Damage);  
        else return new UnitCard(Name, TokenValues.unitcard, Path, powers, Phrase, Position, Damage, externalProperties); 
    }
    
    // El Metodo ParseLeaderCard es el encargado de parsear las cartas de tipo lider. El metodo se encarga de chequear Token
    // por Token que se respete la sintaxis definida en el lenguaje(en caso de que esto no ocurra se lanzara una excepcion acorde) 
    // y de ir parseando cada uno de los campos de la carta.
    public LeaderCard ParseLeaderCard()
    {
        string Name = "";
        string Path = "";
        List<Power> powers = new List<Power>();
        string Phrase = "";
        List<Expressions> externalProperties = new List<Expressions>();
        
        Dictionary<string, bool> Properties = new Dictionary<string, bool>();
        Properties.Add(TokenValues.name, false);
        Properties.Add(TokenValues.path, false);
        Properties.Add(TokenValues.phrase, false);
        Properties.Add(TokenValues.powerset, false);
        if (!Stream.MoveNext(TokenValues.OpenCurlyBrackets))
        {
            throw new Exception($"Open Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        while (Stream.MoveNext(TokenValues.arithmeticexpression)||Stream.MoveNext(TokenValues.booleanexpression)||Stream.MoveNext(TokenValues.textexpression)||Stream.MoveNext(TokenValues.powerset))
        {
            string expressiontype = Stream.LookAhead().Value;
            if(!Stream.MoveNext(TokenType.properties)) throw new Exception("Property Expected");
            string property = Stream.LookAhead().Value;
            if (Properties[property]) throw new Exception($"{property} has already been defined");
            if (!Stream.MoveNext(TokenValues.assign)) throw new Exception("= expected");
            switch (expressiontype)
            {
                case TokenValues.arithmeticexpression:                
                externalProperties.Add(((ArithmeticExpressions)ParseExpression(expressiontype)));
                break;

                case TokenValues.booleanexpression:
                externalProperties.Add((BooleanExpresion)ParseExpression(expressiontype));
                break;

                case TokenValues.textexpression:
                switch (property)
                {
                    case TokenValues.name:
                    Name = ((TextExpression)ParseExpression(expressiontype)).Value.ToString();
                    Properties[property] = true;
                    break;
                    case TokenValues.path:
                    Path = ((TextExpression)ParseExpression(expressiontype)).Value.ToString();
                    Properties[property] = true;
                    break;
                    case TokenValues.phrase:
                    Phrase = ((TextExpression)ParseExpression(expressiontype)).Value.ToString();
                    Properties[property] = true;
                    break;                    
                    default:
                    externalProperties.Add((TextExpression)ParseExpression(expressiontype));
                    break;
                }
                break;
                case TokenValues.powerset:
                powers = ((PowerSet)ParseExpression(expressiontype)).List;
                Properties[property] = true;
                break;

            }
        }
        foreach (var item in Properties.Values)
        {
            if (!item)
            {
               throw new Exception("Not all basic properties initialized");
            }
        }
        if (!Stream.MoveNext(TokenValues.ClosedCurlyBrackets))
        {
            throw new Exception($"Closed Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        if(externalProperties.Count==0)return new LeaderCard(Name, TokenValues.leadercard, Path, powers, Phrase);
        else return new LeaderCard(Name, TokenValues.leadercard, Path, powers, Phrase, externalProperties);
    }
    
    // El Metodo ParseEffectCard es el encargado de parsear las cartas de tipo efecto. El metodo se encarga de chequear Token
    // por Token que se respete la sintaxis definida en el lenguaje(en caso de que esto no ocurra se lanzara una excepcion acorde) 
    // y de ir parseando cada uno de los campos de la carta.
    public EffectCard ParseEffectCard()
    {
        string Name = "";
        string Path = "";
        List<Power> powers = new List<Power>();
        Cards.location Position = Cards.location.Support;
        List<Expressions> externalProperties = new List<Expressions>();
        
        Dictionary<string, bool> Properties = new Dictionary<string, bool>();
        Properties.Add(TokenValues.name, false);
        Properties.Add(TokenValues.path, false);
        Properties.Add(TokenValues.position, false);
        Properties.Add(TokenValues.powerset, false);

        if (!Stream.MoveNext(TokenValues.OpenCurlyBrackets))
        {
            throw new Exception($"Open Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        while (Stream.MoveNext(TokenValues.arithmeticexpression)||Stream.MoveNext(TokenValues.booleanexpression)||Stream.MoveNext(TokenValues.textexpression)||Stream.MoveNext(TokenValues.powerset))
        {
            string expressiontype = Stream.LookAhead().Value;
            if(!Stream.MoveNext(TokenType.properties)) throw new Exception("Property Expected");
            string property = Stream.LookAhead().Value;
            if (Properties[property]) throw new Exception($"{property} has already been defined");
            if (!Stream.MoveNext(TokenValues.assign)) throw new Exception("= expected");
            switch (expressiontype)
            {
                case TokenValues.arithmeticexpression:                
                externalProperties.Add(((ArithmeticExpressions)ParseExpression(expressiontype)));
                break;

                case TokenValues.booleanexpression:
                externalProperties.Add((BooleanExpresion)ParseExpression(expressiontype));
                break;

                case TokenValues.textexpression:
                switch (property)
                {
                    case TokenValues.name:
                    Name = ((TextExpression)ParseExpression(expressiontype)).Value.ToString();
                    Properties[property] = true;
                    break;
                    case TokenValues.path:
                    Path = ((TextExpression)ParseExpression(expressiontype)).Value.ToString();
                    Properties[property] = true;
                    break;
                    
                    case TokenValues.position:
                    switch (((TextExpression)ParseExpression(expressiontype)).Value.ToString())
                    {
                        case TokenValues.support:
                        Position = Cards.location.Support;
                        break;
                        case TokenValues.weather:
                        Position = Cards.location.Climate;
                        break;                        
                        default:
                        throw new Exception("Wrong Position");
                    }
                    break;
                    default:
                    externalProperties.Add((TextExpression)ParseExpression(expressiontype));
                    break;
                }
                break;
                case TokenValues.powerset:
                powers = ((PowerSet)ParseExpression(expressiontype)).List;
                Properties[property] = true;
                break;

            }
        }
        foreach (var item in Properties.Values)
        {
            if (!item)
            {
               throw new Exception("Not all basic properties initialized");
            }
        }
        if (!Stream.MoveNext(TokenValues.ClosedCurlyBrackets))
        {
            throw new Exception($"Closed Curly Bracket expected in card {Stream.LookAhead().Location.CardName} in line {Stream.LookAhead().Location.Line}");
        }
        if(externalProperties.Count==0)return new EffectCard(Name, TokenValues.effectcard, Path, powers, Position);
        else return new EffectCard(Name, TokenValues.effectcard, Path, powers, Position, externalProperties);
    }    
}