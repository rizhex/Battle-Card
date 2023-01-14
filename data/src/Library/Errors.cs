// La estrutura CodeLocation va a ir vinculada estrechamente a los tokens, pues cada token posee un CodeLocation que indica el
// nomre de la carta y la linea en la cual se encuentra el token. El principal objetivo de esto es facilitar la deteccion de errores
// de compilacion al usuario.
public struct CodeLocation
{
    public int Line;
    public string CardName;
}
public struct Error
{
    public string Info{get; private set;}
    public Error(string info)
    {
        this.Info = info;
    }
}