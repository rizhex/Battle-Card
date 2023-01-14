using Godot;
using System;

public class Manage : Node2D
{   
    private string adress = "../Battle-Card/data/textures/cards/band0";
    private bool read;
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {   
        Hide();
        read = true;
    }
    public void _on_MenuButton_pressed(){
        Hide();
    }
    public void _on_Menu_Manage(){
        Show();
    }
    public void _on_DeleteButton_pressed(){
        string name = GetNode<TextEdit>("Name").Text;
        System.IO.File.Delete(adress + "/" + name + ".txt");
    }
    public void _on_SaveButton_pressed(){
        string name = GetNode<TextEdit>("Name").Text;
        string text = GetNode<TextEdit>("CardEdit").Text;
        System.IO.File.WriteAllText(adress + "/" + name + ".txt", text);
        read = true;
    }
    public void _on_OpenButton_pressed(){
        string name = GetNode<TextEdit>("Name").Text;
        GetNode<TextEdit>("CardEdit").Text = string.Empty;
        GetNode<TextEdit>("CardEdit").Text = System.IO.File.ReadAllText(adress + "/" + name + ".txt");
    }
    public void _on_BandButton_pressed(){
        read = true;
        if(GetNode<Button>("BandButton").Text == "Daenerys") {
            adress = "../Battle-Card/data/textures/cards/band1";
            GetNode<Button>("BandButton").Text = "JonSnow";
        }else{
            adress = "../Battle-Card/data/textures/cards/band0";
            GetNode<Button>("BandButton").Text = "Daenerys";
        } 
    }


//  // Called every frame. 'delta' is the elapsed time since the previous frame.
 public override void _Process(float delta)
 {
    if(read){
        GetNode<TextEdit>("Cards").Text = string.Empty;
        string [] adresses = System.IO.Directory.GetFiles(@adress, "*.txt");
        foreach(var item in adresses){
            GetNode<TextEdit>("Cards").Text += item.Substring(adress.Length + 1, item.Length - adress.Length - 5) + "\n";
        }
        read = false;
    }
 }
}
