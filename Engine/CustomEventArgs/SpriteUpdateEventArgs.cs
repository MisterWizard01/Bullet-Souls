namespace Engine.CustomEventArgs;

public class SpriteUpdateEventArgs : EventArgs
{
    public string SpriteName { get; set; }
 
    public SpriteUpdateEventArgs(string spriteName)
    {
        SpriteName = spriteName;
    }
}
