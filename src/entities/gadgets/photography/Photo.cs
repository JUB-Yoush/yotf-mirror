using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Godot;

namespace Yotf;

public record PhotoData(
    string PhotoTaker,
    string[] Subjects,
    byte[] Bytes,
    Image.Format Format,
    int Width,
    int Height,
    bool Mipmaps
)
{
    public static PhotoData New(
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

    public Godot.Collections.Dictionary ToGodotDict() =>
        new()
        {
            { "data", Bytes },
            { "format", (int)Format }, // TODO (j) Format isn't being encoded properly, might need to manually map the enums, or just hard code the one we use?
            { "height", Height },
            { "width", Width },
            { "mipmaps", Mipmaps },
        };

    public string ToJson() => JsonSerializer.Serialize(this);

    public static PhotoData FromJson(string jsonData)
    {
        var data = JsonSerializer.Deserialize<PhotoData>(jsonData);
        return data!;
    }

    public Texture2D ToTexture()
    {
        var photoImg = Image.CreateFromData(Width, Height, Mipmaps, Image.Format.Rgb8, Bytes);
        var imgTex = new ImageTexture();
        imgTex.SetImage(photoImg);
        return imgTex;
    }
};

/// <summary>
/// Each float is on a scale of 0-1
/// </summary>
public record struct PhotoGrade(
    float CenterScore,
    float SizeScore,
    float FacingScore,
    float LightScore,
    int Totalfish,
    bool InAction,
    bool ContainsInk,
    bool IsDead
);

public record Photo(
    PhotoData Data,
    Dictionary<string, PhotoGrade> SubjectGrades,
    IPhotographable.PhotoModifier[] Modifiers
);
