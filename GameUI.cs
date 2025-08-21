using Godot;

public partial class GameUI : Label
{
    // This static variable can be accessed from anywhere in your code.
    public static int GrassCount = 0;

    public override void _Process(double delta)
    {
        // Update the label's text every frame.
        Text = GrassCount.ToString();
    }
}