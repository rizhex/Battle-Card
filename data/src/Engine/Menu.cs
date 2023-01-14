using Godot;
using System;

public class Menu : Node2D
{   
    //La palabra reservada [Signal] indica que StartGame se puede "Emitir" y conectar como si fuera un metodo a otro nodo 
    [Signal]
    public delegate void StartGame();
    [Signal]
    public delegate void Manage();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GetNode<AudioStreamPlayer>("Music").Play();
    }

    public void _on_StartButton_pressed(){
        Hide();
        GetNode<AudioStreamPlayer>("ButtonSound").Play();
        EmitSignal("StartGame");
    }
    public void _on_ManageButton_pressed(){
        Hide();
        GetNode<AudioStreamPlayer>("ButtonSound").Play();
        EmitSignal("Manage");
    }
    public void _on_Music_finished(){
        GetNode<AudioStreamPlayer>("Music").Play();
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}