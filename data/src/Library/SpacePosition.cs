using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

    //SpacePosition es una abstraccion para representar los lugares logicos y ademas tiene la funcion de devolver las posiciones que debe tener cada
    //carta en la parte visual. 
public class SpacePosition : Node
{   
    //Places, es un diccionario que tendra por llave un entero que representa una posicion logica, ademas de la visual, como valor tiene otro
    //una lista de cartas.
    public Dictionary<location, List<Cards>> Places = new Dictionary<location, List<Cards>>();

    //Positions es un diccionario, donde su llave al igual que en Places, representara una posicion logica, ademas de la visual, sus valores
    //son un tanto especiales, pues es una dupla, con el inicio y el final de el lugar geometrico donde deben ser colocadas las cartas en la
    //parte visual.
    //Los Vector2 no son mas que un tipo de objeto de Godot(motor) que contienen una coordenada X y una coordenada Y.
    public Dictionary<location, KeyValuePair<Vector2, Vector2>> Positions = new Dictionary<location, KeyValuePair<Vector2, Vector2>>();

    //Estas variables representan las posiciones en el juego, mas que nada son para usarse como guia para el propio programador, no tener que
    //estar recordando cual era cual.
    public enum location {playerDeck, enemyDeck, playerHand, enemyHand, playerMelee, enemyMelee, playerMiddle,
    enemyMiddle, playerSiege, enemySiege, playerGrave, enemyGrave, playerLeader, enemyLeader, climate, cardSelection,
    supportPlayerMelee, supportEnemyMelee, supportPlayerMiddle, supportEnemyMiddle, supportPlayerSiege, supportEnemySiege, nothing}


    //Constructor por defecto.    
    public SpacePosition(){}

    //Constructor que utilizaremos, recibe 38 argumentos, los cuales son dos listas de Cards:playerDeck y enemyDeck, y el resto
    //serian las duplas de Vector2 que representan los espacios geometricos en el juego. 
    public SpacePosition(List<Cards> PlayerDeck, List<Cards> EnemyDeck, Vector2 PlayerHand1, Vector2 PlayerHand2,
     Vector2 PlayerMelee1, Vector2 PlayerMelee2, Vector2 PlayerMiddle1, Vector2 PlayerMiddle2,
     Vector2 PlayerSiege1, Vector2 PlayerSiege2, Vector2 EnemyMelee1, Vector2 EnemyMelee2,
     Vector2 EnemyMiddle1, Vector2 EnemyMiddle2, Vector2 EnemySiege1, Vector2 EnemySiege2, Vector2 PlayerGrave1,
     Vector2 PlayerGrave2, Vector2 EnemyGrave1, Vector2 EnemyGrave2, Vector2 PlayerLeader1, Vector2 PlayerLeader2,
     Vector2 EnemyLeader1, Vector2 EnemyLeader2, Vector2 Climate1, Vector2 Climate2, Vector2 CardSelection1, Vector2 CardSelection2,
     Vector2 SupportPlayerSiege0, Vector2 SupportPlayerSiege1, Vector2 SupportPlayerMiddle0, Vector2 SupportPlayerMiddle1, 
     Vector2 SupportPlayerMelee0, Vector2 SupportPlayerMelee1, Vector2 SupportEnemyMelee0, Vector2 SupportEnemyMelee1,
     Vector2 SupportEnemyMiddle0, Vector2 SupportEnemyMiddle1, Vector2 SupportEnemySiege0, Vector2 SupportEnemySiege1, 
     Vector2 EnemyHand1, Vector2 EnemyHand2){
        
        //Se inicializan las posiciones de Places.
        Places.Add(location.playerDeck, PlayerDeck);
        Places.Add(location.enemyDeck, EnemyDeck);
        Places.Add(location.playerHand, new List<Cards>());
        Places.Add(location.enemyHand, new List<Cards>());
        Places.Add(location.playerMelee, new List<Cards>());
        Places.Add(location.enemyMelee, new List<Cards>());
        Places.Add(location.playerMiddle, new List<Cards>());
        Places.Add(location.enemyMiddle, new List<Cards>());
        Places.Add(location.playerSiege, new List<Cards>());
        Places.Add(location.enemySiege, new List<Cards>());
        Places.Add(location.playerGrave, new List<Cards>());
        Places.Add(location.enemyGrave, new List<Cards>());
        Places.Add(location.playerLeader, new List<Cards>());
        Places.Add(location.enemyLeader, new List<Cards>());
        Places.Add(location.climate, new List<Cards>());
        Places.Add(location.cardSelection, new List<Cards>());
        Places.Add(location.supportPlayerSiege, new List<Cards>());
        Places.Add(location.supportEnemySiege, new List<Cards>());
        Places.Add(location.supportPlayerMiddle, new List<Cards>());
        Places.Add(location.supportEnemyMiddle, new List<Cards>());
        Places.Add(location.supportPlayerMelee, new List<Cards>());
        Places.Add(location.supportEnemyMelee, new List<Cards>());


        //Se agregan las Duplas a Positions.
        AddDupla(PlayerHand1, PlayerHand2, location.playerHand);
        AddDupla(PlayerLeader1, PlayerLeader2, location.playerLeader);
        AddDupla(PlayerMelee1, PlayerMelee2, location.playerMelee);
        AddDupla(PlayerMiddle1, PlayerMiddle2, location.playerMiddle);
        AddDupla(PlayerSiege1, PlayerSiege2, location.playerSiege);
        AddDupla(PlayerGrave1, PlayerGrave2, location.playerGrave);
        AddDupla(SupportPlayerSiege0, SupportPlayerSiege1, location.supportPlayerSiege);
        AddDupla(SupportPlayerMiddle0, SupportPlayerMiddle1, location.supportPlayerMiddle);
        AddDupla(SupportPlayerMelee0, SupportPlayerMelee1, location.supportPlayerMelee);

        AddDupla(EnemyHand1, EnemyHand2, location.enemyHand);
        AddDupla(EnemyMelee1, EnemyMelee2, location.enemyMelee);
        AddDupla(EnemyLeader1, EnemyLeader2, location.enemyLeader);
        AddDupla(EnemyMiddle1, EnemyMiddle2, location.enemyMiddle);
        AddDupla(EnemySiege1, EnemySiege2, location.enemySiege);
        AddDupla(EnemyGrave1, EnemyGrave2, location.enemyGrave);
        AddDupla(SupportEnemySiege0, SupportEnemySiege1, location.supportEnemySiege);
        AddDupla(SupportEnemyMiddle0, SupportEnemyMiddle1, location.supportEnemyMiddle);
        AddDupla(SupportEnemyMelee0, SupportEnemyMelee1, location.supportEnemyMelee);

        AddDupla(Climate1, Climate2, location.climate);
        AddDupla(CardSelection1, CardSelection2, location.cardSelection);
        
        //Se llama al metodo GetReady.
        GetReady(PlayerDeck, EnemyDeck);
        
        

    }

    //Se encarga de posicionar los leaderCards de cada bando en su posicion especial.
    private void GetReady(List<Cards> playerDeck, List<Cards> enemyDeck){

        foreach(var item in playerDeck){
            if(item is LeaderCard){
                this.Places[SpacePosition.location.playerLeader].Add(item);
                this.Places[SpacePosition.location.playerDeck].Remove(item);
                break;
            }
        }

        foreach(var item in enemyDeck){
            if(item is LeaderCard){
                this.Places[SpacePosition.location.enemyLeader].Add(item);
                this.Places[SpacePosition.location.enemyDeck].Remove(item);
                break;
            }
        }
    }

    //Este metodo agrega las duplas de Vector2 a Positions
    private void AddDupla(Vector2 start, Vector2 end, location place){
        KeyValuePair<Vector2, Vector2> current = new KeyValuePair<Vector2, Vector2>(start, end);

        this.Positions.Add(place, current);
    }
}
    


//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }ss