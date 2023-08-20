using Engine;
using Engine.Components;
using Engine.Managers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

#region Data

var _directions = new string[]
{
    "down",
    "downright",
    "right",
    "upright",
    "up",
    "upleft",
    "left",
    "downleft",
};

#endregion

#region Driver Code

SpriteManager _spriteManager = new SpriteManager();

Console.WriteLine("Enter a sprite file path to load a sprite from JSON or create a new JSON file.");
Console.ForegroundColor = ConsoleColor.Blue;
string filePath = Console.ReadLine() ?? "";
Console.ForegroundColor = ConsoleColor.Gray;

_spriteManager.LoadSprites(filePath);
Console.WriteLine("Loaded " + _spriteManager.Sprites.Count + " sprite(s).");

while (true)
{
    if (GetYN("See " + _spriteManager.Sprites.Count + " sprite(s)? (y/n)"))
    {
        foreach (var item in _spriteManager.Sprites)
        {
            Console.WriteLine("- " + item.Key + "\t(" + item.Value.Animations.Count + " animation(s))");
        }
    }

    Console.WriteLine("Enter sprite name (or empty line to quit):");
    Console.ForegroundColor = ConsoleColor.Blue;
    string spriteName = Console.ReadLine() ?? "";
    Console.ForegroundColor = ConsoleColor.Gray;
    if (spriteName == "")
    {
        break;
    }

    SpriteComponent? sprite;
    if (!_spriteManager.Sprites.TryGetValue(spriteName, out sprite))
    {
        if (!GetYN("Couldn't find a sprite called '" + spriteName + "'. Create a new sprite? (y/n)"))
        {
            continue;
        }
        sprite = CreateSprite();
        _spriteManager.Sprites.Add(spriteName, sprite);
        SaveSpriteJson();
    }

    Console.WriteLine("Texture file path: " + sprite.SpriteName);
    Console.WriteLine("Offset: " + sprite.Offset);
    Console.WriteLine("Frame Ratio: " + sprite.FrameRatio);
    Console.WriteLine("Start Frame: " + sprite.FrameIndex);
    Console.WriteLine(sprite.Animations.Count + " animation(s).");

    while (true)
    {
        if (GetYN("See " + sprite.Animations.Count + " animation(s)? (y/n)"))
        {
            foreach (var item in sprite.Animations)
            {
                Console.WriteLine("- " + item.Key + "\t(" + item.Value.Frames.Count + " frame(s))");
            }
        }

        Console.WriteLine("Enter animation name (or empty line to go back): ");
        Console.ForegroundColor = ConsoleColor.Blue;
        string animationName = Console.ReadLine() ?? "";
        Console.ForegroundColor = ConsoleColor.Gray;
        if (animationName == "")
        {
            break;
        }
        if (!sprite.Animations.ContainsKey(animationName))
        {
            AnimationNotFound(sprite, animationName);
            continue;
        }

        Animation animation = sprite.Animations[animationName];
        Console.WriteLine("Size: " + animation.Size);
        Console.WriteLine("Length: " + animation.Length);
        Console.WriteLine("End Action: " + animation.EndAction);
        Console.WriteLine("Frames: ");
        foreach (var frame in animation.Frames)
        {
            Console.WriteLine(frame);
        }

        if (GetYN("Rebuild animation? (y/n)"))
        {
            sprite.Animations[animationName] = CreateAnimation();
            SaveSpriteJson();
        }
    }
}

#endregion

void AnimationNotFound(SpriteComponent sprite, string animationName)
{
    if (!GetYN("Couldn't find an animation called '" + animationName + "'. Create a new animation? (y/n)"))
    {
        return;
    }

    if (GetYN("Is this an animation set?"))
    {
        (string Name, Animation animation)[] namedAnimations = CreateAnimationSet(animationName);
        foreach (var item in namedAnimations)
        {
            sprite.Animations.Add(item.Name, item.animation);
        }
    }
    else
    {
        sprite.Animations.Add(animationName, CreateAnimation());
    }
    SaveSpriteJson();
}

#region Builder Functions

SpriteComponent CreateSprite()
{
    Console.WriteLine("Texture file path: ");
    Console.ForegroundColor = ConsoleColor.Blue;
    string textureFilePath = Console.ReadLine() ?? "";
    Console.ForegroundColor = ConsoleColor.Gray;

    float offsetX = GetFloat("Offset X: ");
    float offsetY = GetFloat("Offset Y: ");

    float frameRatio = GetFloat("Frame Ratio: ");
    float startFrame = GetFloat("Start Frame: ");

    return new SpriteComponent(textureFilePath, new(offsetX, offsetY), Color.White, frameRatio, startFrame);
}

Animation CreateAnimation()
{
    int width = GetInt("Width: ");
    int height = GetInt("Height: ");
    int horizontalPadding = GetInt("Horizontal Padding: ");
    int startX = GetInt("First Frame X: ");
    int startY = GetInt("First Frame Y: ");
    int animationLength = GetInt("Animation Length: ");
    AnimationEndAction endAction = GetEnum<AnimationEndAction>("End Action: ");

    var animation = new Animation() {
        Size = new(width, height),
        EndAction = endAction,
    };
    for (int i = 0; i < animationLength; i ++)
    {
        animation.Frames.Add(new(startX + i * (width + horizontalPadding), startY));
    }
    return animation;
}

(string, Animation)[] CreateAnimationSet(string name)
{
    int width = GetInt("Width: ");
    int height = GetInt("Height: ");
    int horizontalPadding = GetInt("Horizontal Padding: ");
    int verticalPadding = GetInt("Vertical Padding: ");
    int startX = GetInt("First Frame X: ");
    int startY = GetInt("First Frame Y: ");
    int length = GetInt("Animation Length: ");
    AnimationEndAction endAction = GetEnum<AnimationEndAction>("End Action: ");
    Point nextAnimationShift = GetYN("Are animation strips arranged vertically? (y/n)") ?
        new(0, height + verticalPadding) : new((width + horizontalPadding) * length, 0);

    int directions = GetInt("How many directions?");
    Console.WriteLine("Direction suffixes will be: ");
    int step = _directions.Length / directions;
    for (int i = 0; i < directions; i++)
    {
        Console.WriteLine("- " + _directions[i * step]);
    }
    //Console.WriteLine("Change direction suffixes?");

    var animations = new (string, Animation)[directions];
    for (int i = 0; i < directions; i++)
    {
        var animation = new Animation()
        {
            Size = new(width, height),
            EndAction = endAction,
        };
        for (int j = 0; j < length; j++)
        {
            animation.Frames.Add(new(
                startX + j * (width + horizontalPadding) + nextAnimationShift.X * i,
                startY + nextAnimationShift.Y * i)
            );
        }

        animations[i] = (name + " " + _directions[i * step], animation);
    }

    Console.WriteLine("Built " + animations.Length + " animations.");
    return animations;
}

void SaveSpriteJson()
{
    Console.WriteLine("Saving...");

    var jsonFormatting = Formatting.Indented;
    string json = JsonConvert.SerializeObject(_spriteManager.Sprites, jsonFormatting);
    using StreamWriter writer = new(filePath);
    writer.Write(json);
    
    Console.WriteLine("Saved!");
}

#endregion

#region User Prompts

int GetInt(string prompt)
{
    Console.WriteLine(prompt);
    int result;
    Console.ForegroundColor = ConsoleColor.Blue;
    while (!int.TryParse(Console.ReadLine(), out result))
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Please enter an integer.");
        Console.ForegroundColor = ConsoleColor.Blue;
    }
    Console.ForegroundColor = ConsoleColor.Gray;
    return result;
}

float GetFloat(string prompt)
{
    Console.WriteLine(prompt);
    float result;
    Console.ForegroundColor = ConsoleColor.Blue;
    while (!float.TryParse(Console.ReadLine(), out result))
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Please enter an number.");
        Console.ForegroundColor = ConsoleColor.Blue;
    }
    Console.ForegroundColor = ConsoleColor.Gray;
    return result;
}

bool GetYN(string prompt)
{
    Console.WriteLine(prompt);
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        char c = Console.ReadKey().KeyChar;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine();
        if (c == 'y' || c == 'Y')
        {
            return true;
        }
        else if (c == 'n' || c == 'N')
        {
            return false;
        }
        else
        {
            Console.WriteLine("Please type 'y' or 'n'.");
        }
    }
}

TEnum GetEnum<TEnum>(string prompt)
{
    Console.WriteLine(prompt);
    object? result;
    Console.ForegroundColor = ConsoleColor.Blue;
    while (!Enum.TryParse(typeof(TEnum), Console.ReadLine(), out result) || result == null)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Please enter one of the following options: ");
        foreach (var item in Enum.GetValues(typeof(TEnum)))
        {
            Console.WriteLine("- " + item.ToString());
        }
        Console.ForegroundColor = ConsoleColor.Blue;
    }
    Console.ForegroundColor = ConsoleColor.Gray;
    return (TEnum)result;
}

#endregion