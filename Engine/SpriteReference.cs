namespace Engine;

public class SpriteReference
{
    private Sprite? _sprite;
    private string _spriteName;

    /// <summary>
    /// Signals that this SpriteReference needs its Sprite updated.
    /// </summary>
    public event EventHandler SpriteUpdateEvent;

    public string SpriteName
    {
        get => _spriteName;
        set
        {
            _spriteName = value;
            SpriteUpdateEvent(this, new());
        }
    }

    public Sprite Sprite
    {
        get 
    }
}
