using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


//CardBase es la plantilla visual de las cartas, es la que se muestra en pantalla y se agrega de forma dinamica
//a las "escenas", o sea se crean tantas como cartas haya.
public class CardBase : MarginContainer
{
    //Tiene las mismas propiedades que puede tener una carta
    //Las propiedades que tienen la palabra reservada [Export] se refiere
    //a que estas propiedades podran tomarse ya una vez que la carta este en forma visual
    //pues en ese momento la clase CardBase ya no sera posible acceder a ella como CardBase sino como
    //"Node".
    public string Name { get; set; }
    [Export]
    public string Attack { get; set; }
    public string Phrase { get; set; }
    public string Power { get; set; }
    public Cards.location Place { get; set; }
    public string Rute { get; set; }
    public string Type { get; set; }
    string place;
    public bool leaderUsed;
    public int identifier;
    [Export]
    public bool show;

    [Export]
    public bool OnHand;
    [Export]
    public bool Ready;
    public bool rightClick;
    public string band { get; set; }
    public override void _Ready()
    {
        Hide();
    }

    //Esta funcion se utiliza para generar la carta, una vez que se pasan los datos para el objeto de tipo Cards para
    //el objeto de tipo CardBase(metodo que se encuentra en GameHUD) entonces se proeceden a pasar esa informacion a los 
    //nodos que representan el nombre, el ataque, la frase, la foto, etc.
    public void GenerateCard()
    {
        //Aqui se llenan las etiquetas, o sea lo que es texto.
        GetNode<Label>("BackGround/Name").Text = Name;
        GetNode<Label>("BackGround/Attack").Text = Attack;
        GetNode<RichTextLabel>("BackGround/Phrase").Text = Phrase;
        GetNode<Label>("BackGround/Power").Text = Power;
        if(this.Place == Cards.location.Melee)place = "Melee";
        else if(this.Place == Cards.location.Middle)place = "Middle";
        else if(this.Place == Cards.location.Siege)place = "Siege";
        else if((this.Place == Cards.location.Climate))place = "Climate";
        else if(this.Place == Cards.location.Support)place = "Support";
        GetNode<Label>("BackGround/Place").Text = place;

        //se crean variables para almacenar las imagenes.
        var BackGround = new ImageTexture();
        var Photo = new ImageTexture();

        //se cargan de la carpeta del proyecto dichas imagenes.
        BackGround.Load(@"../Battle-Card/data/textures/cards/band0/front.jpg");
        Photo.Load(@"../Battle-Card/data/textures/cards/" + Rute);

        //y luego se procede a escalar los nodos en funcion de las fotos y el size de unas estructuras predeterminadas que
        //se ajustaron para eso.
        GetNode<Sprite>("BackGround").Scale = RectSize / BackGround.GetSize();
        GetNode<Sprite>("BackGround/PhotoMark/Photo").Scale = GetNode<MarginContainer>("BackGround/PhotoMark").RectSize / Photo.GetSize();

        //y por ultimo se pone la foto en esos nodos que ya estan preparados para que cuando llegue el momento
        //la foto no quede mas grande o mas chica, sino que se ajuste al espacio que se preparo para ella.
        GetNode<Sprite>("BackGround/PhotoMark/Photo").Texture = Photo;
        GetNode<Sprite>("BackGround").Texture = BackGround;
    }

    //Este es el boton de accion de las cartas visuales. O sea lo que sucede cuando se le hace click.
    public void _on_ActionButton_pressed()
    {
        if (rightClick)
        {
            if (this.Type == "LeaderCard")
            {
                GameHUD.data = new string[6];
                GameHUD.data[0] = this.Name;
                GameHUD.data[1] = string.Empty;
                GameHUD.data[2] = this.Phrase;
                GameHUD.data[3] = this.Power;
                GameHUD.data[4] = string.Empty;
                GameHUD.data[5] = this.Rute;
                GameHUD.show = true;

            }
            else if (this.Type == "EffectCard")
            {
                GameHUD.data = new string[6];
                GameHUD.data[0] = this.Name;
                GameHUD.data[1] = string.Empty;
                GameHUD.data[2] = string.Empty;
                GameHUD.data[3] = this.Power;
                GameHUD.data[4] = this.place;
                GameHUD.data[5] = this.Rute;
                GameHUD.show = true;

            }
            else if (this.Type == "UnitCard")
            {
                GameHUD.data = new string[6];
                GameHUD.data[0] = this.Name;
                GameHUD.data[1] = this.Attack;
                GameHUD.data[2] = this.Phrase;
                GameHUD.data[3] = this.Power;
                GameHUD.data[4] = this.place;
                GameHUD.data[5] = this.Rute;
                GameHUD.show = true;

            
            }
        }
        else
        {

            //si se le hace click y la carta es tipo Leader, y la fase del juego es la de el jugador, y ademas la carta
            //no se ha utilizado, pues por reglas del juego, la carta lider solo se puede usar una vez por partida, entonces
            //se procede a buscar la carta logica que es quien tiene el Power dentro de ella y a ejecutarlo.
            if (this.Type == "LeaderCard" && GameHUD.phase == GameHUD.Phase.PlayerTurn && !leaderUsed)
            {

                leaderUsed = true;
                foreach (var item in GameHUD.playerDeck)
                {
                    if (item.name == this.Name)
                    {
                        List<Power> current = item.powers;
                        foreach (var currentPower in current)
                        {
                            if (currentPower.Name != "None")

                            {
                                currentPower.Execute();
                            }
                        }
                        break;
                    }
                }
            }

            //Esto es para que cuando quiera se le de click a una carta produzca un sonido.
            GetNode<AudioStreamPlayer>("ButtonSound").Play();

            //Si la carta esta en la mano, se procede a jugarla al campo.
            if (this.OnHand && GameHUD.phase == GameHUD.Phase.PlayerTurn)
            {
                //Las effect Card no van al campo, sino que en dependencia si son de tipo climatico
                //o de tipo support van a lugares diferentes, aqui se hace esa diferenciacion.
                if (this.Type == "EffectCard")
                {

                    if (this.Place == Cards.location.Support)
                    {
                        GameHUD.ready = true;
                        GameHUD.n = this.identifier;
                    }
                    else
                    {
                        Main.CardSignal(SpacePosition.location.playerHand, SpacePosition.location.climate, this.identifier);
                    }

                }
                else
                {
                    //Luego si la carta es tipo unidad, se hace una verificacion de cual posicion ocupa
                    //y se ejecuta el metodo que se encarga de mover las cartas, que esta en Main.CardSignal, que 
                    //a su vez avisa a GameHUD que tiene q mover la carta, no se pudo hacer de forma directa, pues al 
                    //tener un numero dinamico de cartas, no habia forma de crear un numero dinamico de conexiones

                    if (this.Place == Cards.location.Melee)
                    {
                        Main.CardSignal(SpacePosition.location.playerHand, SpacePosition.location.playerMelee, this.identifier);
                    }
                    else if (this.Place == Cards.location.Middle)
                    {
                        Main.CardSignal(SpacePosition.location.playerHand, SpacePosition.location.playerMiddle, this.identifier);
                    }
                    else if (this.Place == Cards.location.Siege)
                    {
                        Main.CardSignal(SpacePosition.location.playerHand, SpacePosition.location.playerSiege, this.identifier);
                    }
                }
            }

            //Si la carta no esta en la mano, sino que esta en estado de ready, significa que hay algun poder en marcha
            //lo que indica es que es posible que la carta este en espera de ser seleccionada, y en dependencia de cual
            //poder sea, se hara una cosa o la otra.
            else if (this.Ready)
            {

                //Lo que se hace es identificar que poder esta siendo ejecutado, que siempre sera la posicion 0, de la lista de poderes
                //pues una vez este termina de ejecutarse se remueve de la lista.
                if (GameHUD.powerData[0] is RebornPower)
                {
                    //si es reborn power, se revisa que este listo para procesar y que ademas ya haya inciado su ejecucion.
                    RebornPower data = (RebornPower)GameHUD.powerData[0];
                    if (data.processing && data.started)
                    {
                        //El efecto reborn lo que hace es revivir una carta, que si es tipo unidad la manda al campo
                        //si es tipo EffectCard se manda a la mano. 
                        //Se recuerda que los poderes se ejecutan en una fase especial del jugador o del enemigo, y en dependencia
                        //de esto se sabe hacia donde se debe mover la carta.
                        SpacePosition.location to = 0;
                        if (this.Type == "EffectCard")
                        {
                            if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) to = SpacePosition.location.playerHand;
                            else if (GameHUD.phase == GameHUD.Phase.EnemyWaiting) to = SpacePosition.location.enemyHand;
                        }
                        else
                        {

                            if (this.Place == Cards.location.Melee)
                            {
                                if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) to = SpacePosition.location.playerMelee;
                                else to = SpacePosition.location.enemyMelee;
                            }
                            else if (this.Place == Cards.location.Middle)
                            {
                                if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) to = SpacePosition.location.playerMiddle;
                                else to = SpacePosition.location.enemyMiddle;
                            }
                            else if (this.Place == Cards.location.Siege)
                            {
                                if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) to = SpacePosition.location.playerSiege;
                                else to = SpacePosition.location.enemySiege;
                            }
                        }
                        //Luego se mueve la carta y se procede a reducir un contador, pues puede ser que el poder tenga la indicacion
                        //de revivir mas de una carta, en cuyo caso, hasta que dicho contador no llegue a cero, el poder seguira ejecutandose
                        //a no ser q no hayan suficiente cantidad de cartas en el cementerio, lo que habra sido filtrado de antemano durante 
                        //la ejecucion del poder y el contador habra sido reducido a la cantidad maxima de cartas en el cementerio.
                        
                        Main.CardSignal(SpacePosition.location.cardSelection, to, this.identifier);
                        GameHUD.powerData[0].cardsCounter--;
                    }
                }
                else if (GameHUD.powerData[0] is SummonPower)
                {
                    //Si el poder es summon, o sea invocar, el proceso sera parecido a rebornPower, lo que ahora se invoca
                    //desde la baraja, lo demas permanece igual.
                    SummonPower data = (SummonPower)GameHUD.powerData[0];
                    if (data.started && data.processing)
                    {
                        SpacePosition.location from = 0;
                        SpacePosition.location to = 0;

                        if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) from = SpacePosition.location.playerDeck;
                        else from = SpacePosition.location.enemyDeck;

                        if (this.Type == "EffectCard")
                        {
                            if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) to = SpacePosition.location.playerHand;
                            if (GameHUD.phase == GameHUD.Phase.EnemyWaiting) to = SpacePosition.location.enemyHand;
                        }
                        else
                        {

                            if (this.Place == Cards.location.Melee)
                            {
                                if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) to = SpacePosition.location.playerMelee;
                                else to = SpacePosition.location.enemyMelee;
                            }
                            else if (this.Place == Cards.location.Middle)
                            {
                                if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) to = SpacePosition.location.playerMiddle;
                                else to = SpacePosition.location.enemyMiddle;
                            }
                            else if (this.Place == Cards.location.Siege)
                            {
                                if (GameHUD.phase == GameHUD.Phase.PlayerWaiting) to = SpacePosition.location.playerSiege;
                                else to = SpacePosition.location.enemySiege;
                            }
                        }

                        Main.CardSignal(from, to, this.identifier);
                        GameHUD.powerData[0].cardsCounter--;
                    }
                }
                else if (GameHUD.powerData[0] is ModifyAttackPower)
                {
                    //Luego viene el metodo modificarAtaque, que el usuario ira seleccionando cartas del campo, al igual que los otros dos
                    //tendra un contador que se ira reduciendo, lo que se hace ees modificar el ataque de la carta en funcion de los datos que hay en 
                    //powerData[0].
                    ModifyAttackPower data = (ModifyAttackPower)GameHUD.powerData[0];

                    if (data.started && data.processing)
                    {
                        GetNode<Label>("BackGround/Attack").Text = (int.Parse(GetNode<Label>("BackGround/Attack").Text) + data.ammount).ToString();
                        if (int.Parse(GetNode<Label>("BackGround/Attack").Text) < 0) GetNode<Label>("BackGround/Attack").Text = "0";
                        GameHUD.powerData[0].cardsCounter--;
                    }
                }
                else if (GameHUD.powerData[0] is DestroyPower)
                {
                    //Esta es para destruir una carta del campo, el funcionamiento es parecido a las anteriores, solo que ahora
                    //el movimiento que se realizara es de mover la carta hacia el cementerio.
                    DestroyPower data = (DestroyPower)GameHUD.powerData[0];

                    if (data.started && data.processing)
                    {
                        SpacePosition.location to = SpacePosition.location.nothing;
                        List<Cards> playerCards = FieldZones.AllOwnCards().ToList();
                        foreach (var item in playerCards)
                        {
                            if (item.name == this.Name)
                            {
                                to = SpacePosition.location.playerGrave;
                                break;
                            }
                            else
                            {
                                to = SpacePosition.location.enemyGrave;
                                break;
                            }
                        }

                        SpacePosition.location from = SpacePosition.location.nothing;

                        if (this.Place == Cards.location.Melee)
                        {
                            if (to == SpacePosition.location.playerGrave) from = SpacePosition.location.playerMelee;
                            else from = SpacePosition.location.enemyMelee;
                        }
                        else if (this.Place == Cards.location.Middle)
                        {
                            if (to == SpacePosition.location.playerGrave) from = SpacePosition.location.playerMiddle;
                            else from = SpacePosition.location.enemyMiddle;
                        }
                        else if (this.Place == Cards.location.Siege)
                        {
                            if (to == SpacePosition.location.playerGrave) from = SpacePosition.location.playerSiege;
                            else from = SpacePosition.location.enemySiege;
                        }

                        Main.CardSignal(from, to, this.identifier);
                        GameHUD.powerData[0].cardsCounter--;
                    }
                }
            }
        }
    }


    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("show"))
        {
            rightClick = true;
        }
        else if (Input.IsActionPressed("hide"))
        {
            rightClick = false;
        }
    }
}
