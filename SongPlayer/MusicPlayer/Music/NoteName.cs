using System;
using System.Collections.Generic;

public static class NoteName
{
    public static readonly Dictionary<string, float> NoteDictionary = new Dictionary<string, float>()
    {
        {"C4", -1},
        {@"C#4", -.916667f},
        {"D4", -.833333f},
        {@"D#4", -.75f},
        {"E4", -.666666667f},
        {"F4", -.583333333f},
        {@"F#4", -.5f},
        {"G4", -.416666667f},
        {@"G#4", -.333333333f},
        {"A4", -.25f},
        {@"A#4", -.166666667f},
        {"B4", -.083333333f},
        {"C5", 0},
        {@"C#5", .083333333f},
        {"D5", .166666667f},
        {@"D#5", .25f},
        {"E5", .333333333f},
        {"F5", .416666667f},
        {@"F#5", .5f},
        {"G5", .583333333f},
        {@"G#5", .666666667f},
        {"A5", .75f},
        {@"A#5", .833333f},
        {"B5", .916667f},
        {"C6", 1}
    };

    public static float GetNoteByName(string name)
    {
        if (NoteDictionary.ContainsKey(name))
        {
            return NoteDictionary[name];
        }
        
        throw new ArgumentException("Name given is not a note. Value: [" + name + "]", "name");
    }
}
