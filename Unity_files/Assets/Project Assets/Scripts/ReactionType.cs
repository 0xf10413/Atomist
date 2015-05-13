using UnityEngine;
using System.Collections;

/// <summary>
/// Classe de types de réactions, avec un nom et une image.
/// </summary>
/// <todo>L'implémenter complètement et l'utiliser dans le main.</todo>
public class ReactionType {
    public string name { private set; get; }
    public Sprite icon { private set; get; }
    public Sprite iconH { private set; get; } // La ressource sélectionnée

    public ReactionType (string nName)
    {
        name = nName;
        icon = Resources.Load<Sprite>("Images/Icons/" + nName);
        iconH = Resources.Load<Sprite>("Images/Icons/" + nName +"Hover");
    }
}
