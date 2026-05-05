using System;
using System.Diagnostics;
using System.Text.Json;
using Godot;

namespace Yotf;

public record struct Photo(
    string PhotoTaker,
    string[] Subjects,
    byte[] Data,
    Image.Format Format,
    int Width,
    int Height,
    bool Mipmaps
)
{
    public static Photo New(
        string photoTaker,
        string[] Subjects,
        Godot.Collections.Dictionary godotDict
    )
    {
        var Data = (byte[])godotDict["data"];
        var Format = (Image.Format)(int)godotDict["format"];
        var Width = (int)godotDict["width"];
        var Height = (int)godotDict["height"];
        var Mipmaps = (bool)godotDict["mipmaps"];
        return new(photoTaker, Subjects, Data, Format, Width, Height, Mipmaps);
    }

    public readonly Godot.Collections.Dictionary ToGodotDict() =>
        new()
        {
            { "data", Data },
            { "format", (int)Format }, // TODO (j) Format isn't being encoded properly, might need to manually map the enums, or just hard code the one we use?
            { "height", Height },
            { "width", Width },
            { "mipmaps", Mipmaps },
        };

    public readonly string ToJson() => JsonSerializer.Serialize(this);

    public static Photo FromJson(string jsonData)
    {
        var data = JsonSerializer.Deserialize<Photo>(jsonData);
        return data;
    }
};

// each parameter is a float from 0-1
public record struct PhotoGrade(
    float CenterScore,
    float SizeScore,
    float FacingScore,
    float LightScore
);
