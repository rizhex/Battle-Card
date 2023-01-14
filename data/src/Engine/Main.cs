using Godot;
using System;

public class Main : Node
{
	//Este es el nodo principal, todos los otros se ejecutan en este espacio.
	//Tiene como unica funcion de momento ayudar a mover las cartas.
	//En gamehud, en el "While(true)" se verifica constantemente la variable move.
	//esta se volvera true si una carta llama a CardSignal. 

	public static bool move = false;

	//CardSignal lo que hace es pasar los datos de la carta que la llamo a variables globales que
	//hay en gamehud, permitiendo que cuando gamehud detecte el true, mueva la carta desde donde se indica con
	//from, hacia donde se indica con to.
	public static void CardSignal(SpacePosition.location from, SpacePosition.location to, int identifier){
		GameHUD.currentFrom = from;
		GameHUD.currentTo = to;
		GameHUD.n = identifier;
		move = true;
		
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	}
}

