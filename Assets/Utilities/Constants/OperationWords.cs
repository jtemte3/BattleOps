using System.Collections.Generic;
using UnityEngine;

public static class OperationWords
{
    public static List<string> firstWords = new List<string>() {"Soggy","Dark", "Crimson", "Epic", "Just", "Resolute", "Enduring", "Desert", "Red", "Green", "River", "Blue", "Sandy", "Indego", "Black", "Crying", "Roaring", "Restore", "Urgent", "Smoking", "Courageous", "Silver", "Swift", "Civil", "Mobile"};
    public static List<string> secondWords = new List<string>() {"Waffles", "Bend", "Serpant", "Cobra", "Summer", "Fury", "Freedom", "Cause", "Hand", "Pass", "Mountain", "Tiger", "Wolf", "Lion", "Hope", "Sabre", "Sword", "Hawk", "Light", "Gun", "Package", "Convoy", "Stream", "Strike", "Gift"};

    public static string GetRandomFirstWord()
    {
        return firstWords[Random.Range(0,firstWords.Count)];
    }

    public static string GetRandomSecondWord()
    {
        return secondWords[Random.Range(0, secondWords.Count)];
    }
}
