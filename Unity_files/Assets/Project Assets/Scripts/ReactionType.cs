using UnityEngine;
using System.Collections;

/// <summary>
/// Classe de types de réactions, avec un nom et une image.
/// </summary>
/// <todo>L'implémenter complètement et l'utiliser dans le main.</todo>
public class ReactionType {
    public string name { private set; get; }
    private GameObject icon;

    public ReactionType (string nName)
    {
        name = nName;
    }
}
