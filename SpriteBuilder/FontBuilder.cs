using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace SpriteBuilder;

public static class FontBuilder
{
    public static SpriteFont BuildFont(Texture2D texture, Point maxGlyphSize, Point padding, char startChar, Point spaceSize, bool trim = true)
    {
        var glyphBounds = new List<Rectangle>();
        var cropping = new List<Rectangle>();
        var characters = new List<char>();
        var kerning = new List<Vector3>();

        int x = 0, y = 0;
        char curChar = startChar;
        while (y < texture.Height)
        {
            while (x < texture.Width)
            {
                var bounds = new Rectangle(new Point(x, y), maxGlyphSize);
                if (curChar == ' ')
                {
                    bounds.Size = spaceSize;
                }
                else if (trim)
                {
                    bounds = Trim(texture, bounds);
                }
                glyphBounds.Add(bounds);
                cropping.Add(new Rectangle());
                characters.Add(curChar++);
                kerning.Add(new Vector3(0, bounds.Width, 0));
                x += maxGlyphSize.X + padding.X;
            }
            y += maxGlyphSize.Y + padding.Y;
            x = 0;
        }

        return new SpriteFont(texture, glyphBounds, cropping, characters, maxGlyphSize.Y + 1, 1, kerning, characters.Last());
    }

    public static SpriteFont BuildFont(Texture2D texture, Point maxGlyphSize, Point padding, char startChar, bool trim = true)
    {
        return BuildFont(texture, maxGlyphSize, padding, startChar, maxGlyphSize, trim);
    }

    //returns a rectangle contained within the given rectangle that excludes any transparent pixels on the right or bottom edge.
    public static Rectangle Trim(Texture2D texture, Rectangle bounds)
    {
        Color[] data = new Color[texture.Width * texture.Height];
        texture.GetData(data);

        bounds = Rectangle.Intersect(bounds, texture.Bounds);

        //left side
        //while(bounds.Width > 0)
        //{
        //    bool cut = true;
        //    for (int y = bounds.Top; y < bounds.Bottom; y++)
        //    {
        //        if (data[texture.Width * y + bounds.X].A > 0)
        //        {
        //            cut = false;
        //            break;
        //        }
        //    }
        //    if (cut)
        //    {
        //        bounds.X += 1;
        //        bounds.Width -= 1;
        //    }
        //    else
        //    {
        //        break;
        //    }
        //}

        //right side
        while (bounds.Width > 0)
        {
            bool cut = true;
            for (int y = bounds.Top; y < bounds.Bottom; y++)
            {
                if (data[texture.Width * y + bounds.Right - 1].A > 0)
                {
                    cut = false;
                    break;
                }
            }
            if (cut)
            {
                bounds.Width -= 1;
            }
            else
            {
                break;
            }
        }

        //top side
        //while (bounds.Height > 0)
        //{
        //    bool cut = true;
        //    for (int x = bounds.Left; x < bounds.Right; x++)
        //    {
        //        if (data[texture.Width * bounds.Y + x].A > 0)
        //        {
        //            cut = false;
        //            break;
        //        }
        //    }
        //    if (cut)
        //    {
        //        bounds.Y += 1;
        //        bounds.Height -= 1;
        //    }
        //    else
        //    {
        //        break;
        //    }
        //}

        //bottom side
        while (bounds.Height > 0)
        {
            bool cut = true;
            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                if (data[texture.Width * (bounds.Bottom - 1) + x].A > 0)
                {
                    cut = false;
                    break;
                }
            }
            if (cut)
            {
                bounds.Height -= 1;
            }
            else
            {
                break;
            }
        }

        return bounds;
    }

    public static string LimitStringWidth(SpriteFont font, string text, int width)
    {
        string[] words = text.Split(' ');
        string result = words[0];
        for (int i = 1; i < words.Length; i++)
        {
            var temp = result + ' ' + words[i];
            if (font.MeasureString(temp).X <= width)
            {
                result = temp;
            }
            else
            {
                result += "\n" + words[i];
            }
        }

        return result;
    }
}
