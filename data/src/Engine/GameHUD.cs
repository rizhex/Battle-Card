using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Linq;

public class GameHUD : Node2D
{
	//El enum Phase es para identificar la fase del juego en que nos encontramos actualmente
	//para ello se casteara como int, y se comparara con la variable global int phase. 
	public enum Phase { PlayerTurn, EnemyTurn, PlayerWaiting, EnemyWaiting, Results }

#pragma warning disable 649

	//Este es un objeto de tipo PackedScene, que es la superclase de todos los nodos de godot
	//sera utilizado para crear objetos y agregarlos de forma dinamica como hijos a la escena GameHUD.
	//Como se puede apreciar lo precede la palabra reservada Export, la cual indica que se puede acceder
	// a CardScene desde el Editor.
	[Export]
	public PackedScene CardScene;
	//playerDeck es donde se guardara la lista de cartas del jugador en dependencia de lo que escoja el usuario.
	public static List<Cards> playerDeck = new List<Cards>();
	//enemyDeck es donde se guardara la lista de cartas del enemigo en dependencia de lo que escoja el usuario.
	public static List<Cards> enemyDeck = new List<Cards>();

	public static List<Cards> totalCards = new List<Cards>();
	//Positions es un objeto de tipo SpacePosition, es donde se almacenara las cartas en dependencia de hacia donde 
	//se vayan moviendo en el transcurso del juego. Revisar clase SpacePosition para mas detalles.
	public static SpacePosition Positions;
	//currentFrom, currentTo, y currentName, son variables de apoyo para poder mover las cartas a las que el usuario les haga click
	//pues cuando dicha accion ocurre, lo que sucede es que la carta llama a Main.CardSignal y le pasa estos valores, los cuales CardSignal
	//pasa a estas variables, que seran utilizadas por el metodo MoveCard.
	public static SpacePosition.location currentFrom;
	public static SpacePosition.location currentTo;
	public static int n;
	//una vez la carta haya sido movida a el lugar donde estan los botones que identifican a las filas del jugador, se almacenara que fila representaba en moved
	//para aplicar el poder en toda esa fila.
	public SpacePosition.location moved;
	//playerPass y enemyPass, son dos variables booleanas que se utilizaran como apoyo para saber que ocurre en el juego, pues si ambos
	//jugadores pasan, se acaba la ronda.
	public static bool playerPass = false;
	public static bool enemyPass = false;
	//PlayerLife y EnemyLife hacen referencia a las vidas de ambos jugadores, se utilizaran como apoyo para saber que ocurre en el juego, 
	//pues si alguna llegase a cero, terminaria este.
	public static int PlayerLife = 2;
	public static int EnemyLife = 2;
	//playerWins y enemyWins, se utilizan para llevar una cuenta de cuantas rondas ha ganado cada jugador, se utilizara para mostrar un 
	//resultado final.
	public int playerWins = 0;
	public int enemyWins = 0;
	//phase se utiliza para comprar con los enum de Phase luego de castearlos como int. De esta forma se puede progresar en el juego y controlar
	//que puede suceder en cada momento. Empieza con valor -1, se le da valor 1 o 0 de forma aleatoria para decidir quien comienza a jugar.
	public static Phase phase = Phase.Results;
	//powerData es una Lista de PowerData: cuando se invoca una carta con MoveCard, este metodo se encarga de decidir en dependencia de hacia
	//donde se movio dicha carta, si se debe activar su habilidad especial en caso de tenerla, si esta habilidad consiste en llamar a varias funciones
	//se hace necesario que se ejecuten en orden, por eso se almacenan sus datos en un objeto de tipo PowerData(Revisar clase PowerData para mas informacion)
	//y se ejecutaran en el orden que se agregen a powerData.
	public static List<PowerData> powerData;
	//startProcessing es una variable booleana que ayuda a saber si se deben empezar a procesar los poderes que se encuentran almacenados en powerData.
	public static bool startProcessing;
	//remove es un booleano que ayuda a saber si ya se termino con el objeto del indice 0 de power data, removerlo de la lista en ese caso y por tanto se 
	//procederia a ejecutar el nuevo indice 0, en caso de que powerData llegue a cero, terminara esa fase.
	public static bool remove;
	//ready es un booleano que ayuda a saber si al presionar los botones que identifican a las filas del jugador, si se debe mover la carta hacia alli y ejecutar 
	//su poder.
	public static bool ready;
	//una vez playerPass y enemyPass sean true, se dispara un objeto de tipo Timer que una vez termine llama al metodo ChangeEnemyTimer, el cual se encarga de 
	//mostrar los resultados de la ronda/juego, pero este Timer tarda 3 segundos en ejecutar ChangeEnemyTimer, por tanto no existe manera de detener a la funcion
	//process de dispararlo, por tanto se utiliza el booleano shooted para poder detenerlo.
	public bool shooted;
	public static string[] data;
	public static bool show;

	//La funcion Ready se ejecuta el programa. En este caso lo que hace es ocultar la escena de GameHUD hasta que sea llamda mediante _on_Main_StartGame.
	public override void _Ready()
	{
		Hide();
	}

	//La funcion Process se ejecuta constantemente, en principio 60 veces por segundo. Donde delta es el tiempo que ha pasado desde la ultima ejecucion.
	public override void _Process(float delta)
	{
		if (show)
		{
			show = false;
			GetNode<Label>("ShowCard/Card/BackGround/Name").Text = data[0];
			GetNode<Label>("ShowCard/Card/BackGround/Attack").Text = data[1];
			GetNode<RichTextLabel>("ShowCard/Card/BackGround/Phrase").Text = data[2];
			GetNode<Label>("ShowCard/Card/BackGround/Power").Text = data[3];
			GetNode<Label>("ShowCard/Card/BackGround/Place").Text = data[4];
			var Photo = new ImageTexture();
			Photo.Load(@"../Battle-Card/data/textures/cards/" + data[5]);
			GetNode<Sprite>("ShowCard/Card/BackGround/PhotoMark/Photo").Scale = GetNode<MarginContainer>("ShowCard/Card/BackGround/PhotoMark").RectSize / Photo.GetSize();
			GetNode<Sprite>("ShowCard/Card/BackGround/PhotoMark/Photo").Texture = Photo;
		}
		//Tiene dentro de si un grupo de condicionales que definen que hacer en cada fase del juego.

		if (phase == Phase.PlayerTurn)
		{
			//Si es el turno del jugador se actualizaran los visuales, ademas del numero de cartas en el deck y en la mano de ambos jugadores.
			for (int i = 4; i < 10; i++) UpdateAttacks(i);
			//La instruccion GetNode permite acceder a nodos y modificar sus propiedades, en este caso estamos
			//modificando el texto con informacion que conviene mostrar.
			GetNode<Label>("BackGround/PlayerBackDeck/Count").Text = Positions.Places[SpacePosition.location.playerDeck].Count.ToString();
			GetNode<Label>("BackGround/EnemyBackDeck/Count").Text = Positions.Places[SpacePosition.location.enemyDeck].Count.ToString();
			GetNode<Label>("BackGround/PlayerCards").Text = Positions.Places[SpacePosition.location.playerHand].Count.ToString();
			GetNode<Label>("BackGround/EnemyCards").Text = Positions.Places[SpacePosition.location.enemyHand].Count.ToString();

			//Se verifica si el jugador paso o no.
			if (!playerPass)
			{
				//Si no ha pasado se escucha la variable move de la clase Main, la cual se hara
				//true si se debe mover una carta, a continuacion apoyandose en currentFrom, currentTo
				//y currentName, se llama a MoveCard pasandole dichos parametros y acto seguido se devuelve Main.move
				//a su valor original; false.
				if (Main.move)
				{
					MoveCard(currentFrom, currentTo, n);
					Main.move = false;
				}
			}
			else
			{
				//Si el jugador ya paso, se devuelve el turno al enemigo, en caso de que ambos hayan pasado se 
				//dispara el timer que hace que se ejecute la funcion ChangeEnemyTimer lo cual muestra el resultado
				//de la ronda/juego y pone el juego en la fase de results: o sea la fase de espera.
				phase = Phase.EnemyTurn;
				if (playerPass && enemyPass)
				{
					shooted = true;
					if (shooted)
					{
						playerPass = false;
						enemyPass = false;
						GetNode<Timer>("ChangeEnemyTimer").Start();
						shooted = false;
						phase = Phase.Results;
					}

				}
			}
		}
		else if (phase == Phase.EnemyTurn)
		{
			//Si es el turno del enemigo se actualizaran los visuales, ademas del numero de cartas en el deck y en la mano de ambos jugadores.
			for (int i = 4; i < 10; i++) UpdateAttacks(i);
			//La instruccion GetNode permite acceder a nodos y modificar sus propiedades, en este caso estamos
			//modificando el texto con informacion que conviene mostrar.
			GetNode<Label>("BackGround/PlayerBackDeck/Count").Text = Positions.Places[SpacePosition.location.playerDeck].Count.ToString();
			GetNode<Label>("BackGround/EnemyBackDeck/Count").Text = Positions.Places[SpacePosition.location.enemyDeck].Count.ToString();
			GetNode<Label>("BackGround/PlayerCards").Text = Positions.Places[SpacePosition.location.playerHand].Count.ToString();
			GetNode<Label>("BackGround/EnemyCards").Text = Positions.Places[SpacePosition.location.enemyHand].Count.ToString();

			//Se verifica si el enemigo ha pasado o no.
			if (!enemyPass)
			{
				//Si no ha pasado se llama al metodo EnemyIA, el cual se encarga de jugar una carta o ejecutar alguna accion 
				//en dependencia de las condiciones del juego.
				EnemyIA();
			}
			else
			{
				//Si el enemigo ya paso, se devuelve el turno al jugador, en caso de que ambos hayan pasado se 
				//dispara el timer que hace que se ejecute la funcion ChangeEnemyTimer lo cual muestra el resultado
				//de la ronda/juego y pone el juego en la fase de results: o sea la fase de espera.
				phase = Phase.PlayerTurn;
				if (playerPass && enemyPass)
				{
					shooted = true;
					if (shooted)
					{
						playerPass = false;
						enemyPass = false;
						GetNode<Timer>("ChangeEnemyTimer").Start();
						shooted = false;
						phase = Phase.Results;
					}

				}
			}

		}
		else if (phase == Phase.PlayerWaiting)
		{

			//En la fase de espera del jugador, se espera a que terminen de ejcutar la cola de poderes que hay
			//en powerData. 
			PlayerGestion();

		}
		else if (phase == Phase.EnemyWaiting)
		{
			//La fase EnemyWaiting es homologa a PlayerWaiting, solo que aqui nunca se espera
			//a que el enemigo haga algo, pues las decisiones son tomadas de forma instantanea por
			//los metodos que se llaman a la hora de ejecutar los poderes.
			EnemyGestionIA();
		}
		else if (phase == Phase.Results)
		{
			//Simplemente es una fase de espera mientras se muestran los resultados. Por organizar mas que nada.
		}
	}

	//Este metodo se ejecuta siempre que un determinado nodo de tipo Timer deja pasar 3 segundos.
	public void _on_ChangeEnemyTimer_timeout()
	{
		//La funcion de esto es comparar los resultados.
		//En primer lugar en a almacenamos el damage total del jugador.
		//En b almacenamos almacenamos el damage total del enemigo.   
		int a = int.Parse(GetNode<Label>("BackGround/PlayerTotal").Text);
		int b = int.Parse(GetNode<Label>("BackGround/EnemyTotal").Text);
		string result = "";

		//Luego procedemos a comprarlos y a modificar las vidas y victorias(enemyLife, playerLife, enemyWins y playerWins) del jugador y el enemigo
		//en consecuencia con los resultados. Ademas se almacena para mostrar luego el resultado en el string result.
		if (a > b)
		{
			EnemyLife--;
			playerWins++;
			result = "You Won";
			GetNode<AudioStreamPlayer>("VictorySound").Play();
		}
		else if (b > a)
		{
			PlayerLife--;
			enemyWins++;
			result = "You Lose";
			GetNode<AudioStreamPlayer>("DefeatSound").Play();
		}
		else
		{
			PlayerLife--;
			EnemyLife--;
			playerWins++;
			enemyWins++;
			result = "Draw";
			GetNode<AudioStreamPlayer>("DefeatSound").Play();
		}

		//Luego se llama al metodo DisplayInfo pasandole esta informacion y con un booleano en false, ya que el juego aun
		//no ha terminado y lo que se mostrara es el resultado de la ronda.
		DisplayInfo(a, b, result, false);
		//Luego se llama al metodo UpdateLife para actualizar las vidas del jugador y el enemigo.
		UpdateLife();
	}

	//Este metodo representa el accionar de un boton, sirve para identificar hacia donde el jugador quiere mover la EffectCard subtipo Support
	//y luego almacenar en moved, en donde se debe aplicar el poder de la EffectCard.
	public void _on_MeleeSupport_pressed()
	{
		if (ready)
		{
			moved = SpacePosition.location.playerMelee;
			ready = false;
			MoveCard(SpacePosition.location.playerHand, SpacePosition.location.supportPlayerMelee, n);
		}
	}
	//Este metodo representa el accionar de un boton, sirve para identificar hacia donde el jugador quiere mover la EffectCard subtipo Support
	//y luego almacenar en moved, en donde se debe aplicar el poder de la EffectCard.
	public void _on_MiddleSupport_pressed()
	{
		if (ready)
		{
			moved = SpacePosition.location.playerMiddle;
			ready = false;
			MoveCard(SpacePosition.location.playerHand, SpacePosition.location.supportPlayerMiddle, n);
		}
	}
	//Este metodo representa el accionar de un boton, sirve para identificar hacia donde el jugador quiere mover la EffectCard subtipo Support
	//y luego almacenar en moved, en donde se debe aplicar el poder de la EffectCard.
	public void _on_SiegeSupport_pressed()
	{
		if (ready)
		{
			moved = SpacePosition.location.playerSiege;
			ready = false;
			MoveCard(SpacePosition.location.playerHand, SpacePosition.location.supportPlayerSiege, n);
		}
	}
	//DisplayInfo sirve para mostrar informacion acerca de el resultado de la ronda/juego.
	public void DisplayInfo(int a, int b, string result, bool goMenu)
	{
		//Lo Primero que hace es poner de titulo a el cartel informativo RoundEnd.
		GetNode<Label>("DisplayInfo/Title").Text = "Round End";
		//Luego en el subtitulo del cartel se muestra el resultado.
		GetNode<Label>("DisplayInfo/SubTitle").Text = result;
		//Luego se muestra la puntuacion.
		GetNode<Label>("DisplayInfo/Results").Text = a + " - " + b;
		//Por ultimo si el booleano es true, se mostrara Menu en el boton de salida y es falso
		//se mostrara giveup, ya que el solo sera true si el juego ha terminado y la opcion disponible es ir
		//al menu.
		if (goMenu) GetNode<Button>("DisplayInfo/GiveUpButton").Text = "Menu";
		else GetNode<Button>("DisplayInfo/GiveUpButton").Text = "Give Up";
		//Finalmente se muestra el cartel luego de establecer sus propiedades.
		GetNode<Sprite>("DisplayInfo").Show();
	}
	//Representa el accionar del boton de salida del cartel informativo DisplayInfo.
	void _on_GiveUpButton_pressed()
	{
		//Una vez presionado, se devuelve la fase a su valor inicial.
		phase = Phase.Results;
		//Se produce el sonido de presionar el boton.
		GetNode<AudioStreamPlayer>("ButtonSound").Play();
		//Y se oculta el cartel.
		GetNode<Sprite>("DisplayInfo").Hide();
		//Finalmente se llama al metodo EndGame.
		EndGame();
	}
	//Representa el accionar del boton menu, siempre disponible a la izquierda de la mano de cartas del jugador. 
	//Tiene la misma funcionalidad de GiveUpButton
	public void _on_ExitButton_pressed()
	{
		phase = Phase.Results;
		GetNode<AudioStreamPlayer>("ButtonSound").Play();
		EndGame();
	}
	//Representa el accionar del boton Pass, siempre disponible a la izquierda de la mano de cartas del jugador.
	public void _on_PassButton_pressed()
	{
		GetNode<AudioStreamPlayer>("ButtonSound").Play();
		//Simplemente vuelve true playerPass

		playerPass = true;
	}
	//Representa el accionar del boton Next que se encuentra en el cartel informativo de DisplayInfo
	public void _on_NextButton_pressed()
	{
		GetNode<AudioStreamPlayer>("ButtonSound").Play();
		GetNode<Sprite>("DisplayInfo").Hide();
		//Si las vidas de alguno de los jugadores llega a cero. Se llama a DisplayInfo pasandole true el booleano y con los datos pertinentes.
		if (PlayerLife == 0 || EnemyLife == 0)
		{
			string result;
			if (playerWins > enemyWins)
			{
				GetNode<AudioStreamPlayer>("VictorySound").Play();
				result = "You Win";
			}
			else if (playerWins < enemyWins)
			{
				GetNode<AudioStreamPlayer>("DefeatSound").Play();
				result = "You Lose";
			}
			else
			{
				GetNode<AudioStreamPlayer>("DefeatSound").Play();
				result = "Draw";
			}
			//Se le pasa como parametros para mostrar playerWins, enemyWins y result.
			DisplayInfo(playerWins, enemyWins, result, true);
		}
		else
		{
			//En caso contrario se pasa a el siguiente turno y se envian las cartas del campo
			//a su respectivos cementerios.
			phase = (int)Phase.PlayerTurn;
			GoGraves();
		}


	}
	//Representa el accionar del boton NewGame del menu. Simplemente muestra GameHUD.
	public void _on_Menu_StartGame()
	{
		Show();
	}
	//Representa uno de los botones de seleccion de deck.
	public void _on_Band0Button_pressed()
	{
		GetNode<AudioStreamPlayer>("SelectSound").Play();
		//Se llama NewGame con argumento true, lo que indica que se escogio a Daenerys.
		NewGame(true);
	}
	//Representa uno de los botones de seleccion de deck.
	public void _on_Band1Button_pressed()
	{
		GetNode<AudioStreamPlayer>("SelectSound").Play();
		//Se llama NewGame con argumento true, lo que indica que se escogio a Daenerys.
		NewGame(false);
	}
	//El metodo NewGame tiene la funcion de inicializar el juego una vez se elige el deck.
	public void NewGame(bool band)
	{
		//Se oculta el cartel de seleccion de deck.
		GetNode<Node2D>("DeckSelection").Hide();
		//Se genera la informacion de los decks pasando band, indicando que deck escogio el jugador.
		GenerateDeckInfo(band);

		//Se inicializa el objeto Positions de tipo SpacePositions. Se le pasan todas las posiciones geometricas de los lugares en la pantalla
		//que son necesarios para el funcionamiento del juego.
		Positions = new SpacePosition(playerDeck, enemyDeck, GetNode<Position2D>("Hand0").Position, GetNode<Position2D>("Hand1").Position,
		GetNode<Position2D>("PlayerMelee0").Position, GetNode<Position2D>("PlayerMelee1").Position, GetNode<Position2D>("PlayerMiddle0").Position,
		GetNode<Position2D>("PlayerMiddle1").Position, GetNode<Position2D>("PlayerSiege0").Position, GetNode<Position2D>("PlayerSiege1").Position,
		GetNode<Position2D>("EnemyMelee0").Position, GetNode<Position2D>("EnemyMelee1").Position, GetNode<Position2D>("EnemyMiddle0").Position,
		GetNode<Position2D>("EnemyMiddle1").Position, GetNode<Position2D>("EnemySiege0").Position, GetNode<Position2D>("EnemySiege1").Position,
		GetNode<Position2D>("PlayerGrave0").Position, GetNode<Position2D>("PlayerGrave1").Position, GetNode<Position2D>("EnemyGrave0").Position,
		GetNode<Position2D>("EnemyGrave1").Position, GetNode<Position2D>("PlayerLeader0").Position, GetNode<Position2D>("PlayerLeader1").Position,
		GetNode<Position2D>("EnemyLeader0").Position, GetNode<Position2D>("EnemyLeader1").Position, GetNode<Position2D>("Climate0").Position,
		GetNode<Position2D>("Climate1").Position, GetNode<Position2D>("CardSelection0").Position, GetNode<Position2D>("CardSelection1").Position,
		GetNode<Position2D>("SupportPlayerSiege0").Position, GetNode<Position2D>("SupportPlayerSiege1").Position, GetNode<Position2D>("SupportPlayerMiddle0").Position,
		GetNode<Position2D>("SupportPlayerMiddle1").Position, GetNode<Position2D>("SupportPlayerMelee0").Position, GetNode<Position2D>("SupportPlayerMelee1").Position,
		GetNode<Position2D>("SupportEnemyMelee0").Position, GetNode<Position2D>("SupportEnemyMelee1").Position, GetNode<Position2D>("SupportEnemyMiddle0").Position,
		GetNode<Position2D>("SupportEnemyMiddle0").Position, GetNode<Position2D>("SupportEnemySiege0").Position, GetNode<Position2D>("SupportEnemySiege1").Position,
		GetNode<Position2D>("EnemyHand0").Position, GetNode<Position2D>("EnemyHand1").Position);

		//Se llama al metodo RandomizeHands.
		RandomizeHands();

		//Se actualizan los visaules de la mano del jugador, y los visuales de las cartas lider.
		Visuals(Positions.Positions[SpacePosition.location.playerLeader].Key, Positions.Positions[SpacePosition.location.playerLeader].Value, SpacePosition.location.playerLeader);
		Visuals(Positions.Positions[SpacePosition.location.enemyLeader].Key, Positions.Positions[SpacePosition.location.enemyLeader].Value, SpacePosition.location.enemyLeader);
		Visuals(Positions.Positions[SpacePosition.location.playerHand].Key, Positions.Positions[SpacePosition.location.playerHand].Value, SpacePosition.location.playerHand);
		Visuals(Positions.Positions[SpacePosition.location.enemyHand].Key, Positions.Positions[SpacePosition.location.enemyHand].Value, SpacePosition.location.enemyHand);
		

		//Se actualizan las vidas.
		UpdateLife();
		Random random = new Random();

		//Se decide quien comienza a jugar de forma aleatoria.
		phase = (Phase)random.Next(0, 1);
	}
	//El metodo EndGame tiene la funcion de terminar el juego.
	public void EndGame()
	{
		//Se restauran las vidas y las rondas ganadas a sus valores predeterminados.
		PlayerLife = 2;
		EnemyLife = 2;
		playerWins = 0;
		enemyWins = 0;
		//Se esconde el cartel de resultados
		//Se muestra DeckSelection para seleccionar un deck la proxima vez que se le de a jugar
		//Y por ultimo se oculta todo GameHUD
		GetNode<Sprite>("DisplayInfo").Hide();
		GetNode<Node2D>("DeckSelection").Show();
		Hide();

		//Luego se procede a eliminar todos los visuales de las cartas para evitar errores una vez se vuelva a jugar en caso de que se realicen
		//modificaciones a las cartas mediante la herramienta de modificacion.
		foreach (var card in totalCards)
		{
			GetNode<Node2D>("Cards").RemoveChild(GetNode<CardBase>("Cards/" + card.identifier));
		}
		
		GetNode<Node2D>("../Menu").Show();

	}
	//El metodo GoGraves tiene la funcion de enviar todas las cartas del campo al sus respectivos cementerios.
	public void GoGraves()
	{
		//Se crea una lista de las cartas de tipo clima.
		var climate = Positions.Places[SpacePosition.location.climate];

		//Luego se procede a ir por todas las posiciones desde la 4 a la 9, que vendrian siendo todas las de tipo unidad
		//enviandolas al cementerio.
		for (int i = 4; i < 10; i++)
		{
			SpacePosition.location to = SpacePosition.location.nothing;
			if (i % 2 == 0) to = SpacePosition.location.playerGrave;
			else to = SpacePosition.location.enemyGrave;
			foreach (var item in Positions.Places[(SpacePosition.location)i])
			{
				MoveCard((SpacePosition.location)i, to, item.identifier);
			}
		}

		//Se procede a enviar al cementerio las EffectCard de subtipo Weather.
		foreach (var item in climate)
		{
			string name = item.name;
			int to = 0;

			//se verifica si se encuentran en el deck del jugador para enviarla a su cementerio, en caso de que no se encuentren alli
			//se envia al cementerio enemigo.
			foreach (var card in playerDeck)
			{
				if (card.name == name)
				{
					MoveCard(SpacePosition.location.climate, SpacePosition.location.playerGrave, card.identifier);
					break;
				}
			}
			foreach(var card in enemyDeck){
				if (card.name == name)
				{
					MoveCard(SpacePosition.location.climate, SpacePosition.location.enemyGrave, card.identifier);
					break;
				}
			}
			
		}
		//Finalmente se procede a ir por todas las posiciones de las EffectCard subtipo Support.
		for (int i = 16; i < 22; i++)
		{
			SpacePosition.location to = SpacePosition.location.nothing;
			//Como las posiciones del jugador son pares, si i es par, se envia a el cementerio del jugador.
			//En caso contrario se envian al cementerio al enemigo.
			if (i % 2 == 0) to = SpacePosition.location.playerGrave;
			else to = SpacePosition.location.enemyGrave;
			foreach (var item in Positions.Places[(SpacePosition.location)i])
			{
				MoveCard((SpacePosition.location)i, to, item.identifier);
			}
		}
	}
	//El metodo GenerateDeckInfo es para generar todo lo respectivo a las cartas, tanto la parte logica como la parte visual.
	public void GenerateDeckInfo(bool band)
	{
		//Se declaran las variables donde se almacenaran las direcciones del jugador y el enemigo.
		string playerAdress = "";
		string enemyAdress = "";
		string playerBand = "";
		string enemyBand = "";

		//Se decide en funcion de que bando escogio donde se buscara la informacion de las cartas.
		if (band)
		{
			playerAdress = "../Battle-Card/data/textures/cards/band0";
			enemyAdress = "../Battle-Card/data/textures/cards/band1";
			playerBand = "band0";
			enemyBand = "band1";
		}
		else
		{
			playerBand = "band1";
			enemyBand = "band0";
			playerAdress = "../Battle-Card/data/textures/cards/band1";
			enemyAdress = "../Battle-Card/data/textures/cards/band0";
		}

		//luego se llama al metodo GetLogicalCards para generar las cartas logicas y almacenarlas en playerDeck y enemyDeck.
		playerDeck = GetLogicalCards(playerAdress, playerBand);
		enemyDeck = GetLogicalCards(enemyAdress, enemyBand);
		totalCards = playerDeck.Concat(enemyDeck).ToList();
		IdentifyCards();

		//Luego utilizando playerDeck y enemyDeck, se generan las cartas visuales.
		GenerateDeck(totalCards);
		IdentifyCards();

	}
	public List<Cards> GetLogicalCards(string adress, string band)
	{
		Tokenizer a = Compiler.Lexical;
		string[] texts = System.IO.Directory.GetFiles(@adress, "*.txt");
		List<Cards> answer = new List<Cards>();
		foreach (var item in texts)
		{
			string text = System.IO.File.ReadAllText(item);
			List<Token> MyList = (List<Token>)a.GetTokens(text, item);
			TokenStream b = new TokenStream(MyList);
			Parser c = new Parser(b);
			Cards card = c.ParseCard();
			card.band = band;
			answer.Add(card);

		}

		return answer;
	}
	//El metodo GenerateDeck genera el visual del deck.
	public void GenerateDeck(List<Cards> deck)
	{
		//Itera por la lista de cartas llamando al metodo GenerateCards para crear el visual.
		for (int i = 0; i < deck.Count; i++)
		{
			GenerateCard(deck[i], i);
		}

	}
	//El metodo GenerateCard genera el visual de una carta.
	public void GenerateCard(Cards item, int n)
	{   
		totalCards[n].identifier = n;
		//Primero se verifica de que tipo es para castearla y poder acceder a todas sus propiedades.
		if (item is UnitCard)
		{
			//Si es UnitCard, se castea.
			var newItem = (UnitCard)item;
			//Luego se Castea una instancia de CardScene a CardBase que son la plantilla de la carta visual.
			var card = (CardBase)CardScene.Instance();
			//Luego se trasladan las propiedades de la logica al visual.
			card.Name = newItem.name;
			card.Attack = newItem.damage.ToString();
			card.Place = newItem.position;
			card.Phrase = newItem.phrase;
			card.Rute = newItem.imagePath;
			card.Type = newItem.type;
			card.Power = newItem.powers[0].Name;
			card.band = newItem.band;
			card.identifier = n;

			//Luego se llama al metodo GenerateCard de CardBase.
			card.GenerateCard();

			//Y se procede a agregar CardBase como hijo a la escena GameHUD; o sea, ya se puede visualizar.
			GetNode<Node2D>("Cards").AddChild(card, true);

			//Luego sabiendo que todo hijo que se agrega se le nombra CardBase, aprovechamos para cambiar el nombre del nodo por el nombre
			//de la carta y poder identificarlo y moverlo a donde sea necesario.
			GetNode<MarginContainer>("Cards/CardBase").Name = n.ToString();

			//Este procedimiento es homologo para LeaderCard y EffectCard. Por lo que no se comentara.
		}
		else if (item is LeaderCard)
		{
			var newItem = (LeaderCard)item;
			var card = (CardBase)CardScene.Instance();

			card.Name = newItem.name;
			card.Phrase = newItem.phrase;
			card.Rute = newItem.imagePath;
			card.Type = newItem.type;
			card.Power = newItem.powers[0].Name;
			card.identifier = n;
			card.GenerateCard();

			GetNode<Node2D>("Cards").AddChild(card, true);

			GetNode<MarginContainer>("Cards/CardBase").Name = n.ToString();

		}
		else if (item is EffectCard)
		{
			var newItem = (EffectCard)item;
			var card = (CardBase)CardScene.Instance();
			card.Name = newItem.name;
			card.Place = newItem.position;
			card.Rute = newItem.imagePath;
			card.Type = newItem.type;
			card.Power = newItem.powers[0].Name;
			card.identifier = n;
			card.GenerateCard();

			GetNode<Node2D>("Cards").AddChild(card, true);

			GetNode<MarginContainer>("Cards/CardBase").Name = n.ToString();

		}
	}
	//El metodo Visuals actualiza los visuales de una posicion determinada.
	public void Visuals(Vector2 start, Vector2 end, SpacePosition.location place)
	{
		//Primero se declara una variable llamada count, para poder determinar cuantas cartas hay de momento en la posicion que estemos evaluando.
		var count = 0;
		//Se calcula la distancia que debe haber entre cada carta de forma equitativa en dependencia de la distancia entre el comienzo y el final del espacio.
		var distance = (end.x - start.x) / Positions.Places[place].Count;

		foreach (var item in Positions.Places[place])
		{
			//Luego se crea una variable position que sera de tipo Vector2, cuyo valor x, sera el inicio mas la distancia equitativa multiplicada
			//por la cantidad de cartas, de esta forma no se van a superponer, y su valor y, no es mas que el valor y tanto de start, como de end,
			//pues siempre estan a la misma altura.
			var position = new Vector2(start.x + (distance * count), start.y);
			//luego se amenta count para hallar bien la distancia del proximo item.
			count++;
			//luego se muestra la carta.
			GetNode<CardBase>("Cards/" + item.identifier).Show();
			//y por ultimo se modifica la posicion de esta.
			GetNode<CardBase>("Cards/" + item.identifier).RectPosition = position;
		}
	}
	//El metodo UpdateAttacks actualiza el ataque de la zona del campo que se le pase como arugmento.
	public void UpdateAttacks(int place)
	{
		//Se verifica que la zona sea valida.
		if (place == 4)
		{
			//Si la zona es valida, se acumulara el ataque de todas las cartas en dicha zona y se mostrara
			//en la etiqueta destinada para ello.
			int amount = 0;
			foreach (var x in Positions.Places[(SpacePosition.location)place])
			{
				amount += int.Parse(GetNode<Label>("Cards/" + x.identifier + "/BackGround/Attack").Text);

			}
			//Aqui se muestra el valor en el visual.
			GetNode<Label>("BackGround/PlayerMelee").Text = amount.ToString();

			//El codigo es practicamente identico y con la misma funcion para el resto de las posiciones, por lo que no se comentara.
		}
		else if (place == 5)
		{
			int amount = 0;
			foreach (var x in Positions.Places[(SpacePosition.location)place])
			{
				amount += int.Parse(GetNode<Label>("Cards/" + x.identifier + "/BackGround/Attack").Text);

			}
			GetNode<Label>("BackGround/EnemyMelee").Text = amount.ToString();
		}
		else if (place == 6)
		{
			int amount = 0;
			foreach (var x in Positions.Places[(SpacePosition.location)place])
			{
				amount += int.Parse(GetNode<Label>("Cards/" + x.identifier + "/BackGround/Attack").Text);
			}
			GetNode<Label>("BackGround/PlayerMiddle").Text = amount.ToString();
		}
		else if (place == 7)
		{
			int amount = 0;
			foreach (var x in Positions.Places[(SpacePosition.location)place])
			{
				amount += int.Parse(GetNode<Label>("Cards/" + x.identifier + "/BackGround/Attack").Text);

			}
			GetNode<Label>("BackGround/EnemyMiddle").Text = amount.ToString();
		}
		else if (place == 8)
		{
			int amount = 0;
			foreach (var x in Positions.Places[(SpacePosition.location)place])
			{
				amount += int.Parse(GetNode<Label>("Cards/" + x.identifier + "/BackGround/Attack").Text);

			}
			GetNode<Label>("BackGround/PlayerSiege").Text = amount.ToString();
		}
		else if (place == 9)
		{
			int amount = 0;
			foreach (var x in Positions.Places[(SpacePosition.location)place])
			{
				amount += int.Parse(GetNode<Label>("Cards/" + x.identifier + "/BackGround/Attack").Text);

			}
			GetNode<Label>("BackGround/EnemySiege").Text = amount.ToString();
		}

		//Por ultimo se actualizan las etiquetas que muestran el ataque global, que no es mas que una combinacion de la suma del ataque presente en las fila.
		GetNode<Label>("BackGround/PlayerTotal").Text = (int.Parse(GetNode<Label>("BackGround/PlayerMelee").Text) + int.Parse(GetNode<Label>("BackGround/PlayerMiddle").Text) + int.Parse(GetNode<Label>("BackGround/PlayerSiege").Text)).ToString();
		GetNode<Label>("BackGround/EnemyTotal").Text = (int.Parse(GetNode<Label>("BackGround/EnemyMelee").Text) + int.Parse(GetNode<Label>("BackGround/EnemyMiddle").Text) + int.Parse(GetNode<Label>("BackGround/EnemySiege").Text)).ToString();
	}
	//El metodo MoveCard se encarga de mover cartas de una posicion a otra, recibe de donde, hacia donde y el nombre de la carta.
	public void MoveCard(SpacePosition.location from, SpacePosition.location to, int n)
	{

		//Lo primero que hace es verificar que la carta aparezca desde donde se va a mover para evitar errores.
		if (Positions.Places[from].Contains(totalCards[n]))
		{
			//Luego se procede a mover de una posicion a otra en la logica.
			var From = Positions.Places[from];
			Positions.Places.Remove(from);

			From.Remove(totalCards[n]);

			var To = Positions.Places[to];
			Positions.Places.Remove(to);

			To.Add(totalCards[n]);

			Positions.Places.Add(from, From);
			Positions.Places.Add(to, To);

			//Luego si la carta se mueve a algun deck se procede a ocultar la carta.
			if (to == SpacePosition.location.playerDeck || to == SpacePosition.location.enemyDeck)
			{
				GetNode<CardBase>("Cards/" + n).Hide();
			}

			//Si se mueve hacia la mano del jugador, se hace true la variable interna OnHand de dicha carta.
			//En caso de no moverse hacia alli se hace false.
			if (to == SpacePosition.location.playerHand)
			{
				GetNode<CardBase>("Cards/" + n).OnHand = true;
			}
			else
			{
				GetNode<CardBase>("Cards/" + n).OnHand = false;
			}

			//Si se mueve hacia la posicion de seleccion se vuelve true la variable Ready de la carta
			//En caso de no moverse hacia alli se hace false.
			if (to == SpacePosition.location.cardSelection)
			{
				GetNode<CardBase>("Cards/" + n).Ready = true;
			}
			else
			{
				GetNode<CardBase>("Cards/" + n).Ready = false;
			}

			//Si se mueve a alguna zona valida del campo y desde algun lugar valido como pueden ser las manos del jugador o el enemigo,
			//se procede a activar su poder en caso de tener.
			if ((((int)to > 3 && (int)to < 10) || (int)to >= 16 || (int)to == 14) && (int)from == 2 || (int)from == 3)
			{

				//se busca el objeto de tipo Cards y se verifica que el poder no sea None, en cuyo caso se llama al metodo Execute.
				if (totalCards[n].powers.Count > 0){
					List<Power> currentPowers = totalCards[n].powers;
					foreach (var item in currentPowers)
					{
						if(item.Name != "None") item.Execute();
					}                     
				}
				else
				{
					//en caso de que sea None se cambia de fase.
					if (phase == Phase.PlayerTurn) phase = Phase.EnemyTurn;
					else if (phase == Phase.EnemyTurn) phase = Phase.PlayerTurn;
				}


			}

		}
		//Luego si la carta se movio desde y hacia algun lugar valido se actualiza el visual, desde donde y hacia donde se movio.
		if ((int)from >= 2) Visuals(Positions.Positions[from].Key, Positions.Positions[from].Value, from);
		if ((int)to >= 2) Visuals(Positions.Positions[to].Key, Positions.Positions[to].Value, to);
	}
	//El metodo RandomizeHands tiene la funcion de proveer 10 cartas aleatorias a ambas manos.
	public void RandomizeHands()
	{
		//Se crea un objeto de tipo Random.
		Random random = new Random();
		//Una lista para saber que cartas se han movido ya.
		List<int> nums = new List<int>();

		//Luego mientras la cantidad de cartas que se tengan en la mano sea inferior a 10 se seguiran generando numeros aleatorios hasta que se consiga 
		//la cantidad deseada de cartas
		while (Positions.Places[SpacePosition.location.playerHand].Count < 10)
		{
			int n = random.Next(0, playerDeck.Count);

			for (int i = 0; i < playerDeck.Count; i++)
			{
				if (n == i && !nums.Contains(n))
				{   //Si el indice de la carta es LeaderCard se agrega como si ya fuera agregada y se continua a la siguiente iteracion,
					//pues la leaderCard no puede ir a la mano.
					if (playerDeck[n] is LeaderCard)
					{
						nums.Add(n);
						continue;
					}
					MoveCard(SpacePosition.location.playerDeck, SpacePosition.location.playerHand, playerDeck[n].identifier);
					nums.Add(n);
					break;
				}
			}
		}
		//Se repite el mismo proceso con la mano rival.
		nums = new List<int>();
		while (Positions.Places[SpacePosition.location.enemyHand].Count < 10)
		{
			int n = random.Next(0, enemyDeck.Count);

			for (int i = 0; i < enemyDeck.Count; i++)
			{
				if (n == i && !nums.Contains(n))
				{
					if (enemyDeck[n] is LeaderCard)
					{
						nums.Add(n);
						continue;
					}
					MoveCard(SpacePosition.location.enemyDeck, SpacePosition.location.enemyHand, enemyDeck[n].identifier);
					nums.Add(n);
					break;
				}
			}
		}
	}
	//El metodo EnemyIA, se encarga de lanzar una carta.
	public void EnemyIA()
	{

		Random random = new Random();

		var hand = Positions.Places[SpacePosition.location.enemyHand];
		//obtiene una copia de las carta que tiene en la mano.
		if (hand.Count > 0)
		{

			int n = random.Next(0, hand.Count - 1);
			SpacePosition.location to = SpacePosition.location.nothing;
			//se movera la carta a su posicion en dependencia de si es unidad o effect.
			if (hand[n] is UnitCard)
			{
				UnitCard card = (UnitCard)hand[n];
				switch (card.position)
				{
					case Cards.location.Melee:
						to = SpacePosition.location.enemyMelee;
						break;
					case Cards.location.Middle:
						to = SpacePosition.location.enemyMiddle;
						break;
					case Cards.location.Siege:
						to = SpacePosition.location.enemySiege;
						break;
				}
				MoveCard(SpacePosition.location.enemyHand, to, card.identifier);
			}
			else if (hand[n] is EffectCard)
			{
				EffectCard card = (EffectCard)hand[n];

				switch (card.position)
				{
					case Cards.location.Climate:
						MoveCard(SpacePosition.location.enemyHand, SpacePosition.location.climate, card.identifier);
						break;
					case Cards.location.Support:
						int max = 0;
						int i = 0;
						for (int j = 5; j < 10; j = j + 2)
						{
							if (Positions.Places[(SpacePosition.location)j].Count > max)
							{
								i = j;
								max = Positions.Places[(SpacePosition.location)j].Count;
							}
						}
				
						if (i == 7)
						{
							MoveCard(SpacePosition.location.enemyHand, SpacePosition.location.supportEnemyMiddle, card.identifier);

						}
						else if (i == 9)
						{
							MoveCard(SpacePosition.location.enemyHand, SpacePosition.location.supportEnemySiege, card.identifier);

						}
						else
						{
							MoveCard(SpacePosition.location.enemyHand, SpacePosition.location.supportEnemyMelee, card.identifier);
							if (i != 5) i = 5;
						}

						moved = (SpacePosition.location)i;

						break;
				}

			}
			//si el jugador paso, el enemigo pasara, en funcion de no gastar todas sus cartas en esta ronda.
			if (playerPass)
			{
				enemyPass = true;
			}
		}
		else
		{
			//si ya no tiene cartas, ejecutara su carta lider y acto seguido pasara.
			foreach (var item in enemyDeck)
			{
				if (item is LeaderCard)
				{
					LeaderCard current = (LeaderCard)item;
					foreach (var power in current.powers)
					{
						power.Execute();
					}
				}
			}
			enemyPass = true;
		}
	}
	//Este metodo representa el visual de las vidas que tiene cada jugador.
	public void UpdateLife()
	{
		//simplemente activa la animacion que representa tantas vidas como valga playerLife y enemyLife.
		GetNode<AnimatedSprite>("BackGround/PlayerLife").Animation = PlayerLife.ToString();
		GetNode<AnimatedSprite>("BackGround/EnemyLife").Animation = EnemyLife.ToString();
	}
	//El metodo Reborn ejecuta la habilidad Reborn de las cartas utiliznado los datos que se encuentran en powerData[0].
	public void Reborn()
	{
		//Primero se castea Reborn.
		RebornPower rebornData = (RebornPower)powerData[0];
		//Si la propiedad select es true, significa que se tiene q elegir la carta  renacer.
		if (rebornData.select)
		{
			//Verificamos si estamos en el turno del jugador o del enemigo.
			if (phase == Phase.PlayerWaiting)
			{
				//Si estamos en el turno del jugador se inicializa la lista de names.
				rebornData.identifier = new List<int>();

				//luego se procede almacenar en dicha lista los nombres de la carta del lugar de donde se deben revivir las cartas(rebornData.places).
				foreach (var item in Positions.Places[rebornData.place])
				{
					rebornData.identifier.Add(item.identifier);
				}
				//Si la cantidad de cartas a revivir es menor que la cantidad de cartas disponible, la cantidad de cartas a revivir cambiara
				//al maximo de cartas disponibles.
				if (!(rebornData.identifier.Count >= rebornData.cardsCounter)) rebornData.cardsCounter = rebornData.identifier.Count;

				//Luego se procede a mover dichas cartas a la posicion de seleccion.
				foreach (var item in rebornData.identifier)
				{
					MoveCard(rebornData.place, SpacePosition.location.cardSelection, item);
				}
			}
			else
			{
				//En caso de que sea el turno del jugador, se ejecuta un codigo homologo al del jugador, con la diferencia de que la seleccion se 
				//realiza aqui de forma inmediata.
				Random random = new Random();
				List<Cards> cards = new List<Cards>();
				List<int> nums = new List<int>();
				int n = 0;

				foreach (var item in Positions.Places[rebornData.place])
				{
					cards.Add(item);
				}

				if (!(cards.Count >= rebornData.cardsCounter)) rebornData.cardsCounter = cards.Count;

				while (rebornData.cardsCounter > 0)
				{

					n = random.Next(0, cards.Count - 1);

					if (!nums.Contains(n))
					{

						SpacePosition.location to = 0;

						if (cards[n] is UnitCard)
						{
							UnitCard card = (UnitCard)cards[n];
							if (card.position == Cards.location.Melee) to = SpacePosition.location.enemyMelee;
							else if (card.position == Cards.location.Middle) to = SpacePosition.location.enemyMiddle;
							else if (card.position == Cards.location.Siege) to = SpacePosition.location.enemySiege;
						}
						else if (cards[n] is EffectCard)
						{
							to = SpacePosition.location.enemyHand;
						}
						nums.Add(n);
						MoveCard(rebornData.place, to, cards[n].identifier);
						rebornData.cardsCounter--;

					}
				}
				//finalmente se termina el procesamiento del poder.
				rebornData.processing = false;

			}
		}
		else
		{   
			List<Cards> cards = new List<Cards>();
			//Si rebornData.select es false significa que la interpretacion del poder es que se paso una lista de nombres a revivir.
			foreach (var item in rebornData.names)
			{
				//Por ello se procede a revivir dichas cartas a su respectivo lugar en dependencia de si es el turno del jugador o no.
				foreach(var card in Positions.Places[rebornData.place])
				{   
					if(item == card.name){
						cards.Add(card);
					}
				}
				foreach (var card in cards)
					{
						SpacePosition.location to = SpacePosition.location.nothing;
						if (card is UnitCard)
						{
							UnitCard newCard = (UnitCard)card;
							if (newCard.position == Cards.location.Melee)
							{
								if (phase == Phase.PlayerWaiting) to = SpacePosition.location.playerMelee;
								else to = SpacePosition.location.enemyMelee;
							}
							else if (newCard.position == Cards.location.Middle)
							{
								if (phase == Phase.PlayerWaiting) to = SpacePosition.location.playerMiddle;
								else to = SpacePosition.location.enemyMiddle;
							}
							else if (newCard.position == Cards.location.Siege)
							{
								if (phase == Phase.PlayerWaiting) to = SpacePosition.location.playerSiege;
								else to = SpacePosition.location.enemySiege;
							}
						}
						else if (card is EffectCard)
						{
							if (phase == Phase.PlayerWaiting) to = SpacePosition.location.playerHand;
							else to = SpacePosition.location.enemyHand;
						}

						MoveCard(rebornData.place, to, card.identifier);
					}
				}
				//finalmente se termina el procesamiento del poder.
				rebornData.processing = false;
			
		}
	}
	//El metodo Summon ejecuta la habilidad Summon de las cartas utilizando los datos que se encuentran en powerData[0].
	public void Summon()
	{
		//Primero se castea powerData[0] para poder acceder a las propiedades requeridas.
		SummonPower summonData = (SummonPower)powerData[0];

		//se verifica si summonData.select es true.
		if (summonData.select)
		{
			//si es true significa que se debe dar a escoger las cartas del deck a invocar.
			if (phase == Phase.PlayerWaiting)
			{
				//si es el turno del jugador se inicializa names.
				summonData.names = new List<string>();

				//luego se procede a agregar los nombres del deck del jugador a names
				foreach (var item in Positions.Places[SpacePosition.location.playerDeck])
				{
					summonData.names.Add(item.name);
				}

				//luego si la cantidad de cartas disponibles en el deck no es menor que la cantidad a invocar no se hara nada, en caso
				//contrario se pondra como maxima cantidad de cartas a invocar la cantidad de cartas en el deck.
				if (!(summonData.names.Count >= summonData.cardsCounter)) summonData.cardsCounter = summonData.names.Count;

				//luego se procede a moverlas al lugar de seleccion.
				foreach (var item in summonData.names)
				{   
					foreach(var card in Positions.Places[SpacePosition.location.playerDeck]){
						if(item == card.name){
							MoveCard(SpacePosition.location.playerDeck, SpacePosition.location.cardSelection, card.identifier);
						}
					}
				   
				}
			}
			else
			{
				//si es el turno del enemigo, se realiza un homologo al jugador, pero sin esperar a que sean selecciondas, sino q son invocadas inmediatamente.
				Random random = new Random();
				List<Cards> cards = new List<Cards>();
				List<int> nums = new List<int>();
				int n = 0;

				foreach (var item in Positions.Places[SpacePosition.location.enemyDeck])
				{
					cards.Add(item);
				}

				if (!(summonData.cardsCounter <= cards.Count)) summonData.cardsCounter = cards.Count;

				while (summonData.cardsCounter > 0)
				{

					n = random.Next(0, summonData.names.Count - 1);

					if (!nums.Contains(n))
					{

						SpacePosition.location to = SpacePosition.location.nothing;

						if (cards[n] is UnitCard)
						{
							UnitCard card = (UnitCard)cards[n];
							if (card.position == Cards.location.Melee) to = SpacePosition.location.enemyMelee;
							else if (card.position == Cards.location.Middle) to = SpacePosition.location.enemyMiddle;
							else if (card.position == Cards.location.Siege) to = SpacePosition.location.playerSiege;
						}
						else if (cards[n] is EffectCard)
						{
							to = SpacePosition.location.enemyHand;
						}
						nums.Add(n);
						MoveCard(SpacePosition.location.enemyDeck, to, cards[n].identifier);
						summonData.cardsCounter--;

					}
				}
				//una vez terminado se termina el procesamiento de summon.
				summonData.processing = false;
			}
		}
		else
		{
			//Si select es false, la interpretacion correcta es que se dio una lista de nombres para invocar.
			foreach (var item in summonData.names)
			{
				SpacePosition.location places = SpacePosition.location.nothing;
				List<Cards> cards = new List<Cards>();
				//si es el turno del jugador el lugar de procedencia de las cartas sera place.
				if (phase == Phase.PlayerWaiting) places = SpacePosition.location.playerDeck;
				else if (phase == Phase.EnemyWaiting) places = SpacePosition.location.enemyDeck;

				//luego se verifica que la carte se encuentre, de ser asi se agrega a cards.
				foreach(var card in Positions.Places[places])
				{   
					if(card.name == item){
						cards.Add(card);
					}
					
				}
				//luego se va moviendo la carta a su respectivo lugar dependiendo si es el turno del jugador o no.
				foreach (var card in cards)
				{
					SpacePosition.location to = 0;
					if (card is UnitCard)
					{
						UnitCard newCard = (UnitCard)card;
						if (newCard.position == Cards.location.Melee)
						{
							if (phase == Phase.PlayerWaiting) to = SpacePosition.location.playerMelee;
							else to = SpacePosition.location.enemyMelee;
						}
						else if (newCard.position == Cards.location.Middle)
						{
							if (phase == Phase.PlayerWaiting) to = SpacePosition.location.playerMiddle;
							else to = SpacePosition.location.enemyMiddle;
						}
						else if (newCard.position == Cards.location.Siege)
						{
							if (phase == Phase.PlayerWaiting) to = SpacePosition.location.playerSiege;
							else to = SpacePosition.location.enemySiege;
						}
					}
					else if (card is EffectCard)
					{
						if (phase == Phase.PlayerWaiting) to = SpacePosition.location.playerHand;
						else to = SpacePosition.location.enemyHand;
					}

					MoveCard(places, to, card.identifier);
				}
			}
			//finalmente se termina el procesamiento de summon.
			summonData.processing = false;
		}
	}
	//El metodo SwitchBand ejecuta la habilidad SwitchBand de las cartas utilizando los datos que se encuentran en powerData[0].
	public void SwitchBand()
	{
		//primero se castea powerData[0].
		SwitchBandPower switchBandData = (SwitchBandPower)powerData[0];
		SpacePosition.location to = SpacePosition.location.nothing;
		SpacePosition.location from = SpacePosition.location.nothing;
		int n = 0;
		//se procede a ir por todas las posiciones validas para cambiar de zona y se verficia que la carta aprezca, y este sera el lugar desde donde se movera.
		for (int i = 4; i < 10; i++)
		{
			foreach (var card in Positions.Places[(SpacePosition.location)i])
			{
				if(card.name == switchBandData.cardName){
					from = (SpacePosition.location)i;
					n = card.identifier;
					break;
				}
			}
		}
		//luego se determina en funcion de si es el turno del jugador o no, hacia donde se debe mover la carta.
		if (phase == Phase.PlayerWaiting) to = (SpacePosition.location)((int)from + 1);
		else if (phase == Phase.EnemyWaiting) to = (SpacePosition.location)((int)from - 1);

		//finalmente se mueve la carta, y se procede a terminar el procesamiento del poder.
		MoveCard(from, to, n);
		switchBandData.processing = false;
	}
	//El metodo Destroy ejecuta la habilidad Destroy de las cartas utilizando los datos que se encuentran en powerData[0].
	public void Destroy()
	{
		//En primer lugar se castea powerData[0].
		DestroyPower destroyData = (DestroyPower)powerData[0];

		//se verifica el valor de idntifier, pues cada valor tiene una interpretacion diferente en funcion de que debe hacerse
		//con los datos que hay en destroyData.
		if (destroyData.identifier == 0)
		{
			//si identifier es cero, eso significa que se mando a destruir una fila entera.
			List<Cards> cards = new List<Cards>();
			//se procede a recorrer toda la fila agregando las cartas a cards.
			foreach (var item in Positions.Places[destroyData.from])
			{
				cards.Add(item);
			}

			SpacePosition.location to;
			//luego si el lugar de donde se deben destruir es par, to sera el cementerio del jguador, en caso contrario
			//sera el cementerio del enemigo.
			if ((int)destroyData.from % 2 == 0) to = SpacePosition.location.playerGrave;
			else to = SpacePosition.location.enemyGrave;

			//luego se procede a mover cada carta a su respectivo cementerio.
			foreach (var item in cards)
			{
				MoveCard(destroyData.from, to, item.identifier);
			}
			//finalmente se termina el procesamiento de Destroy.
			destroyData.processing = false;
		}
		else if (destroyData.identifier == 1)
		{
			//si identifier es uno, eso significa que se dio una lista de nombres para destruir.
			SpacePosition.location to;

			//se itera por cada elemento de la lista.
			foreach (var item in destroyData.names)
			{
				//y se verifica si este se encuentra en alguna posicion valida, acto seguido se procede a mover a su respectivo
				//cementerio, el cual es determinado si el lugar donde se encuentra es par o no
				for (int i = 4; i < 10; i++)
				{
					foreach(var card in Positions.Places[(SpacePosition.location)i])
					{   
						if (i % 2 == 0) to = SpacePosition.location.playerGrave;
						else to = SpacePosition.location.enemyGrave;

						MoveCard((SpacePosition.location)i, to, card.identifier);
						break;
					}
				}
			}
			//finalmente se termina el procesamiento de Destroy
			destroyData.processing = false;
		}
		else if (destroyData.identifier == 2)
		{
			//si identifier es dos, eso significa que se mando a destruir una cantidad determinada de cartas en el campo.
			if (phase == Phase.PlayerWaiting)
			{
				//si es el turno del jugador, se hace una lista con todas las cartas del campo.
				List<Cards> cards = FieldZones.AllEnemyCards().ToList();
				//acto seguido se procede a poner en true su propiedad ready.
				foreach (var item in cards)
				{
					GetNode<CardBase>("Cards/" + item.identifier).Ready = true;
				}
				//se verifica que si la cantida de cartas a destruir no es mayor que la cnatidad de cartas disponibles, se procede a cambiar 
				//la cantidad de cartas a destruir por la canitdad maxima de cartas disponibles.
				if (!(cards.Count >= destroyData.cardsCounter)) destroyData.cardsCounter = cards.Count;
			}
			else if (phase == Phase.EnemyWaiting)
			{
				//si es el turno del enemigo se intentara destruir cartas del enemigo, de nos er posible se destruiran cartas propias.
				List<Cards> playerCards = FieldZones.AllOwnCards().ToList();
				List<Cards> cards = FieldZones.AllExistingCards().ToList();
				int n = 0;
				Random random = new Random();
				List<int> nums = new List<int>();
				if (!(playerCards.Count >= destroyData.cardsCounter))
				{
					destroyData.cardsCounter = playerCards.Count;
				}
				//se procede a destruir cartas aleatorias.
				while (destroyData.cardsCounter > 0)
				{

					n = random.Next(0, playerCards.Count - 1);

					if (!nums.Contains(n))
					{

						SpacePosition.location from = SpacePosition.location.nothing;

						if (playerCards[n] is UnitCard)
						{
							UnitCard card = (UnitCard)cards[n];
							if (card.position == Cards.location.Melee) from = SpacePosition.location.playerMelee;
							else if (card.position == Cards.location.Middle) from = SpacePosition.location.playerMiddle;
							else if (card.position == Cards.location.Siege) from = SpacePosition.location.playerSiege;
							nums.Add(n);

							MoveCard(from, SpacePosition.location.playerGrave, cards[n].identifier);
							destroyData.cardsCounter--;
						}



					}
				}
				//una vez terminado el proceso, se detiene el procesamiento de destroy.
				destroyData.processing = false;


			}
		}
	}
	//El metodo ModifyAttack ejecuta la habilidad ModifyAttack de las cartas utilizando los datos que se encuentran en powerData[0].
	public void ModifyAttack()
	{
		//primero se castea powerData[0].
		ModifyAttackPower modifyAttackData = (ModifyAttackPower)powerData[0];
		if (modifyAttackData.identifier == 0)
		{
			//si el identifier es cero, eso significa que se debe modificar el ataque de todas las cartas en cierta fila en cierto valor.
			foreach (var item in Positions.Places[modifyAttackData.where])
			{
				GetNode<Label>("Cards/" + item.identifier + "/BackGround/Attack").Text = (int.Parse(GetNode<Label>("Cards/" + item.identifier + "/BackGround/Attack").Text) + modifyAttackData.ammount).ToString();
			}
			//una vez terminado se procede a termianr el procesamiento del poder.
			powerData[0].processing = false;
		}
		else if (modifyAttackData.identifier == 1)
		{
			//si el identifier es uno, eso significa que se debe modificar el ataque de todas las cartas que se pasaron en una lista si estna en el campo.
			List<Cards> cards = FieldZones.AllExistingCards().ToList();
			//se procede a ir por cada item de la lista
			foreach (var item in modifyAttackData.names)
			{
				//luego se verifica que este entre las cartas del campo, en caso afirmativo se procede a modificar su ataque en el valor indicado.
				foreach (var card in cards)
				{
					if (card.name == item)
					{
						GetNode<Label>("Cards/" + card.identifier + "/BackGround/Attack").Text = (int.Parse(GetNode<Label>("Cards/" + card.identifier + "/BackGround/Attack").Text) + modifyAttackData.ammount).ToString();
					}
				}
			}
			powerData[0].processing = false;
		}
		else if (modifyAttackData.identifier == 2)
		{
			//si el identifier es dos, eso significa que se dio una cantidad determinada de cartas a modificar su ataque.
			if (phase == Phase.PlayerWaiting)
			{
				//si es el turno del jugador, se crea una lista de todas las cartas disponibles
				List<Cards> cards = (List<Cards>)FieldZones.AllExistingCards();
				//y se pone en true la variable Ready de cada carta para que pueda ser modificida.
				foreach (var item in cards)
				{
					GetNode<CardBase>("Cards/" + item.identifier).Ready = true;
				}
			}
			else if (phase == Phase.EnemyWaiting)
			{
				//si es el turno del enemigo, el comportamiento es homologo al del jugador, solo que no se debe esperar sino que se decide inmdeitamente a que cartas 
				//modificarle el ataque.
				List<Cards> cards = FieldZones.AllEnemyCards().ToList();
				if ((cards.Count >= modifyAttackData.cardsCounter)) modifyAttackData.cardsCounter = cards.Count;

				SpacePosition.location from = SpacePosition.location.nothing;
				List<int> nums = new List<int>();
				while (modifyAttackData.cardsCounter > 0)
				{
					Random random = new Random();
					int n = random.Next(0, cards.Count);
					if (!nums.Contains(n))
					{
						GetNode<Label>("Cards/" + cards[n].identifier + "/BackGround/Attack").Text = (int.Parse(GetNode<Label>("Cards/" + cards[n].identifier + "/BackGround/Attack").Text) + modifyAttackData.ammount).ToString();
						nums.Add(n);
						modifyAttackData.cardsCounter--;
					}
				}
			}

		}
		else if (modifyAttackData.identifier == 3)
		{
			//si el identifier es tres, eso significa que se utilizo una effectCard, que modificara en cierto valor una fila
			//la cual se indica mediante la variable moved.
			if (phase == Phase.PlayerWaiting)
			{
				//si es el turno del jugador se modifican sus filas.
				foreach (var card in Positions.Places[moved])
				{
					GetNode<Label>("Cards/" + card.identifier + "/BackGround/Attack").Text = (int.Parse(GetNode<Label>("Cards/" + card.identifier + "/BackGround/Attack").Text) + modifyAttackData.ammount).ToString();
				}
				//y se detiene el procesamiento.
				powerData[0].processing = false;
			}
			else
			{
				//si es el turno del enemigo se modifican sus filas
				foreach (var card in Positions.Places[moved])
				{
					GetNode<Label>("Cards/" + card.identifier + "/BackGround/Attack").Text = (int.Parse(GetNode<Label>("Cards/" + card.identifier + "/BackGround/Attack").Text) + modifyAttackData.ammount).ToString();
				}
				//y se detiene el procesamiento.
				powerData[0].processing = false;
			}
		}
	}
	//El metodo Draw ejecuta la habilidad Draw de las cartas utilizando los datos que se encuentran en powerData[0].
	public void Draw()
	{
		//Primero se castea powerData[0].
		DrawPower drawData = (DrawPower)powerData[0];
		if (phase == Phase.PlayerWaiting)
		{
			//si es el turno del jugador se seleccionan cartas aleatorias para mover a su mano.
			for (int i = 0; i < drawData.cardsCounter; i++)
			{
				Random random = new Random();
				List<Cards> cards = new List<Cards>();
				foreach (var item in Positions.Places[SpacePosition.location.playerDeck])
				{
					cards.Add(item);
				}
				int n = random.Next(0, cards.Count);
				MoveCard(SpacePosition.location.playerDeck, SpacePosition.location.playerHand, cards[n].identifier);
			}
		}
		else if (phase == Phase.EnemyWaiting)
		{
			//el comportamiento en caso de que sea turno enemigo es exactamente identico, solo que se mueven las cartas desde el deck del enemigo hacia su mano.
			for (int i = 0; i < drawData.cardsCounter; i++)
			{
				Random random = new Random();
				List<Cards> cards = new List<Cards>();
				foreach (var item in Positions.Places[SpacePosition.location.enemyDeck])
				{
					cards.Add(item);
				}
				int n = random.Next(0, cards.Count);
				MoveCard(SpacePosition.location.enemyDeck, SpacePosition.location.enemyHand, cards[n].identifier);
			}
		}
		//finalmente se termina el procesamiento de draw.
		drawData.processing = false;
	}
	//El metodo RestoreReady se encarga de devolver a su valor original la variable ready de todas las cartas existentes en el campo.
	public void RestoreReady()
	{
		List<Cards> cards = FieldZones.AllExistingCards().ToList();

		foreach (var item in cards)
		{   
			foreach(var card in totalCards){
				GetNode<CardBase>("Cards/" + card.identifier).Ready = false;
			}
			
		}
	}
	//Elimina el primer elemento de la cola de poderes.
	public void RemoveFirst()
	{
		powerData.RemoveAt(0);
	}
	public void PlayerGestion(){
		if (Main.move && GetNode<CardBase>("Cards/" + n).Ready)
			{
				MoveCard(currentFrom, currentTo, n);
				Main.move = false;
			}

			//Si remove esta en true, se llama al metodo RemoveFirst ya que se ha terminado de ejcutar el primer metodo de la cola.
			if (remove)
			{
				RemoveFirst();
				remove = false;
			}
			//Si powerData.Count = 0, esto significa que no hay mas poderes para ejecutar y por lo tanto
			//termina el procesamiento de poderes.
			if (powerData.Count == 0)
			{
				startProcessing = false;

			}

			//Se verifica si aun se estan procesando poderes.
			if (startProcessing)
			{
				//En caso afirmativo se procede a iterar por powerData.
				foreach (var item in powerData)
				{
					//Luego se verifica el tipo de poder que es item. 
					if (item is RebornPower)
					{

						//Si el poder es de tipo RebornPower, se verifica si ya comenzo a ejecutarse.

						if (item.started)
						{
							//Si ya comenzo a ejecutarse se verifica si esta en procesamiento.
							if (item.processing)
							{
								//si el poder aun esta en procesamiento despues de haber comenzado a ejecutarse
								//significa que la version que se escogio del poder necesita de que el usuario escoja cartas
								//para ello se procede a verificar si la variable contador que hay en item ya llego a cero, pues
								//cada vez que el usuario interactue con una carta este disminuye.
								if (item.cardsCounter == 0)
								{
									//En caso afirmativo se procede a mover las cartas que se habian colocado para seleccionar a su cementerio.
									//y se termina el procesamiento.
									item.processing = false;
									List<Cards> cards = Positions.Places[SpacePosition.location.cardSelection];
									foreach (var card in cards)
									{
										MoveCard(SpacePosition.location.cardSelection, SpacePosition.location.playerGrave, card.identifier);
									}
								}
								//Sea cual sea el caso hay un break, pues nunca se pasa a verificar el item siguiente, siempre se trabaja con el 
								//se encuentra en la posicion 0.
								break;
							}
							else
							{
								//Si el poder ya no esta en procesamiento se procede a remover de la cola.
								remove = true;
								break;
							}
						}
						else
						{
							//Si el poder aun no se ha empezado a procesar, se vuelve true la variable item.started
							//y se llama al metodo Reborn, el cual se encarga de con los datos que hay en powerData[0] de ejecutar
							//la version adecuada del poder.
							item.started = true;
							Reborn();
							break;
						}
					}
					else if (item is SummonPower)
					{
						//Si el poder es de tipo RebornPower, se verifica si ya comenzo a ejecutarse.
						if (item.started)
						{
							//Si ya comenzo a ejecutarse se verifica si esta en procesamiento.
							if (item.processing)
							{
								//si el poder aun esta en procesamiento despues de haber comenzado a ejecutarse
								//significa que la version que se escogio del poder necesita de que el usuario escoja cartas
								//para ello se procede a verificar si la variable contador que hay en item ya llego a cero, pues
								//cada vez que el usuario interactue con una carta este disminuye.
								if (item.cardsCounter == 0)
								{
									//En caso afirmativo se procede a mover las cartas que se habian colocado para seleccionar a su deck.
									//y se termina el procesamiento.
									item.processing = false;
									List<Cards> cards = Positions.Places[SpacePosition.location.cardSelection];
									foreach (var card in cards)
									{
										MoveCard(SpacePosition.location.cardSelection, SpacePosition.location.playerDeck, card.identifier);
									}
								}
								//Sea cual sea el caso hay un break, pues nunca se pasa a verificar el item siguiente, siempre se trabaja con el 
								//se encuentra en la posicion 0.
								break;
							}
							else
							{
								//Si el poder ya no esta en procesamiento se procede a remover de la cola.
								remove = true;
								break;
							}
						}
						else
						{
							//Si el poder aun no se ha empezado a procesar, se vuelve true la variable item.started
							//y se llama al metodo Summon, el cual se encarga de con los datos que hay en powerData[0] de ejecutar
							//la version adecuada del poder.
							item.started = true;
							Summon();
							break;
						}
					}
					else if (item is SwitchBandPower)
					{
						//Si el poder es de tipo SwitchBand, se verifica si ya ha empezado a procesarse
						if (item.started)
						{
							//La verdad es que nunca llegara aqui item.processing en true, pues esto es instantaneo y no necesita de intervencion del usuario.
							if (item.processing)
							{
								break;
							}
							else
							{
								//Por lo que procede a removerse.
								remove = true;
								break;
							}
						}
						else
						{
							//Si no ha comenzado a procesarse se hace true item.started y se llama al metodo SwitchBand.
							item.started = true;
							SwitchBand();
							break;
						}
					}
					else if (item is DestroyPower)
					{
						//Si el poder es de tipo DestroyPower se verifica si ya ha comenzado a procesarse.
						if (item.started)
						{
							//si el poder aun esta en procesamiento despues de haber comenzado a ejecutarse
							//significa que la version que se escogio del poder necesita de que el usuario escoja cartas
							//para ello se procede a verificar si la variable contador que hay en item ya llego a cero, pues
							//cada vez que el usuario interactue con una carta este disminuye.
							if (item.processing)
							{
								if (item.cardsCounter == 0)
								{
									//Una vez el usuario termine de escoger termina el procesamiento y se restauran las variables ready que hay
									//en las cartas que estaban en espera de ser seleccionadas mediante el metodo RestoreReady
									item.processing = false;
									RestoreReady();
								}
								break;
							}
							else
							{
								//Si el poder ya no esta en procesamiento se procede a remover de la cola.
								remove = true;
								break;
							}
						}
						else
						{
							//Si el poder aun no se ha empezado a procesar, se vuelve true la variable item.started
							//y se llama al metodo Destroy, el cual se encarga de con los datos que hay en powerData[0] de ejecutar
							//la version adecuada del poder.
							item.started = true;
							Destroy();
							break;
						}
					}
					else if (item is ModifyAttackPower)
					{
						//Si el poder es de tipo ModifyAttack se verifica si ya ha comenzado a procesarse.
						if (item.started)
						{
							//si el poder aun esta en procesamiento despues de haber comenzado a ejecutarse
							//significa que la version que se escogio del poder necesita de que el usuario escoja cartas
							//para ello se procede a verificar si la variable contador que hay en item ya llego a cero, pues
							//cada vez que el usuario interactue con una carta este disminuye.
							if (item.processing)
							{
								if (item.cardsCounter == 0)
								{
									//Una vez el usuario termine de escoger termina el procesamiento y se restauran las variables ready que hay
									//en las cartas que estaban en espera de ser seleccionadas mediante el metodo RestoreReady
									item.processing = false;
									RestoreReady();
								}
								break;
							}
							else
							{
								//Si el poder ya no esta en procesamiento se procede a remover de la cola.
								remove = true;
								break;
							}
						}
						else
						{
							//Si el poder aun no se ha empezado a procesar, se vuelve true la variable item.started
							//y se llama al metodo ModifyAttack, el cual se encarga de con los datos que hay en powerData[0] de ejecutar
							//la version adecuada del poder.
							item.started = true;
							ModifyAttack();
							break;
						}
					}
					else if (item is DrawPower)
					{
						//Si el poder es de tipo ModifyAttack se verifica si ya ha comenzado a procesarse.
						if (item.started)
						{
							//La verdad es que nunca llegara aqui item.processing en true, pues esto es instantaneo y no necesita de intervencion del usuario
							if (item.processing)
							{
								break;
							}
							else
							{
								//Si el poder ya no esta en procesamiento se procede a remover de la cola.
								remove = true;
								break;
							}
						}
						else
						{
							//Si el poder aun no se ha empezado a procesar, se vuelve true la variable item.started
							//y se llama al metodo Draw, el cual se encarga de con los datos que hay en powerData[0] de ejecutar
							//la version adecuada del poder.
							item.started = true;
							Draw();
							break;
						}
					}

				}

			}
			else
			{
				phase = Phase.EnemyTurn;
			}
	}
	public void EnemyGestionIA(){
		if (remove)
			{
				RemoveFirst();
				remove = false;
			}
			if (powerData.Count == 0)
			{
				startProcessing = false;
			}
			if (startProcessing)
			{

				foreach (var item in powerData)
				{
					if (item is RebornPower)
					{
						if (item.started)
						{
							if (item.processing)
							{
								break;
							}
							else
							{
								remove = true;
								break;
							}
						}
						else
						{
							item.started = true;
							Reborn();
							break;
						}
					}
					else if (item is SummonPower)
					{
						if (item.started)
						{
							if (item.processing)
							{
								break;
							}
							else
							{
								remove = true;
								break;
							}
						}
						else
						{
							item.started = true;
							Summon();
							break;
						}
					}
					else if (item is SwitchBandPower)
					{
						if (item.started)
						{
							if (item.processing)
							{
								break;
							}
							else
							{
								remove = true;
								break;
							}
						}
						else
						{
							item.started = true;
							SwitchBand();
							break;
						}
					}
					else if (item is DestroyPower)
					{
						if (item.started)
						{
							if (item.processing)
							{
								break;
							}
							else
							{
								remove = true;
								break;
							}
						}
						else
						{
							item.started = true;
							Destroy();
							break;
						}
					}
					else if (item is ModifyAttackPower)
					{
						if (item.started)
						{
							if (item.processing)
							{
								if (item.cardsCounter == 0)
								{
									item.processing = false;
								}
								break;
							}
							else
							{
								remove = true;
								break;
							}
						}
						else
						{
							item.started = true;
							ModifyAttack();
							break;
						}
					}
					else if (item is DrawPower)
					{
						if (item.started)
						{
							if (item.processing)
							{
								break;
							}
							else
							{
								remove = true;
								break;
							}
						}
						else
						{
							item.started = true;
							Draw();
							break;
						}
					}

				}


			}
			else
			{
				phase = (int)Phase.PlayerTurn;
			}


		
	}
	public void IdentifyCards(){
		for(int i = 0; i < playerDeck.Count; i++){
			foreach(var card in totalCards){
				if(playerDeck[i].name == card.name){
					playerDeck[i].identifier = card.identifier;
					break;
				}
			}
		}
		for(int i = 0; i < enemyDeck.Count; i++){
			foreach(var card in totalCards){
				if(enemyDeck[i].name == card.name){
					enemyDeck[i].identifier = card.identifier;
				}
				break;
			}
		}
	}

}


