using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * <summary>
 * La classe principale
 * Contient l'ensemble des variables globales ainsi que les fonctions à appeler à chaque frame.</summary>
 **/
public class Main : MonoBehaviour {

	public static Main context;

    public static List<Element> elements { private set; get; } // Liste des éléments, fixée au démarrage
    public static List<Reaction> reactions { private set; get; } // Liste des réaction, fixée au démarrage
	
	public static List<Player> players = new List<Player>(); // La liste des joueurs
    public static List<Obstacle> obstacles = new List<Obstacle>(); // liste des obtacles
    public static List<ReactionType> reactionTypes = new List<ReactionType> (); // liste des types de réaction
    public static List<KeyValuePair<Element,int>> pick = new List<KeyValuePair<Element,int>>(); // La pioche : liste de paires (élément, nombre de fois où cet élément apparait dans la pioche)
	public static int turnID = 0; // L'ID du tour : 0 si c'est au tour du joueur 1, 1 si c'est au tour du joueur 2 etc

    public static System.Random randomGenerator = new System.Random(); // Générateur de nombre aléatoires
	
	/**
	 * Fonction appelée au démarrage de l'application
	 * Disons que c'est l'équivalent du main() en C++
	 **/
	void Start () {
		context = this;

        // Génération de la liste des éléments et de la pioche
		elements = new List<Element> ();
		SimpleJSON.JSONArray elementsInfos = loadJSONFile("elements").AsArray;
		foreach (SimpleJSON.JSONNode elementInfos in elementsInfos) {
            Element nElement = new Element(elementInfos["name"],elementInfos["symbol"], elementInfos["atomicNumber"].AsInt, elementInfos["family"], elementInfos["file"], elementInfos["energy"].AsInt);
			elements.Add(nElement);
            pick.Add(new KeyValuePair<Element,int>(nElement,elementInfos["nb_in_pick"].AsInt));
		}

        // Génération de la liste des réactions, ainsi que des types de réaction
        reactions = new List<Reaction> ();
        SimpleJSON.JSONArray reactionsInfos = loadJSONFile("reactions").AsArray;
        foreach (SimpleJSON.JSONNode r in reactionsInfos) {
            List<KeyValuePair<Element, int>> rList = new List<KeyValuePair<Element, int>> ();
            foreach (SimpleJSON.JSONArray elt in r["reagents"].AsArray)
            {
                rList.Add (new KeyValuePair<Element, int> (elements.Find(n => (n.symbole == elt[0].AsString)), elt[1].AsInt));
            }
            ReactionType rt = reactionTypes.Find (n => n.name == r["type"].AsString);
            if (null == rt) {
                rt = new ReactionType (r["type"]);
                reactionTypes.Add (rt);
            }
            reactions.Add (new Reaction (r["reaction"], r["products"], rList, rt, r["cost"].AsInt, r["gain"].AsInt));
        }

		players.Add (new Player ());
        players[0].BeginTurn();
	}

    /// <summary>
    /// Retourne une carte élément choisie au hasard dans la pioche
    /// </summary>
    public static Element pickCard() {
        int nbCards = 0;
        foreach (KeyValuePair<Element,int> elementInfos in pick)
            nbCards += elementInfos.Value;
        int cardPicked = randomGenerator.Next() % nbCards;
        int cardID = 0;
        foreach (KeyValuePair<Element,int> elementInfos in pick) {
            cardID += elementInfos.Value;
            if (cardID > cardPicked)
                return elementInfos.Key;
        }
        return null; // Cette ligne ne sert à rien à part éviter une erreur de compilation
    }

    public static void removeAllChilds(GameObject parent) {
        int childCount = parent.transform.childCount;
        for (int i=childCount-1; i>=0; i--)
            GameObject.Destroy(parent.transform.GetChild(i).gameObject);
    }

	/**
	 * Fonction appelée à chaque frame, environ 60 fois par seconde
	 **/
	void Update () {
	}

	/// <summary>
	/// Retourne un object JSON contenu dans un fichier
	/// </summary>
	/// <param name="fileName">Le nom du fichier (sans l'extension) contenant le fichier JSON. Ce fichier dot se trouver dans le dossier Resources/Parameters</param>
	public static SimpleJSON.JSONNode loadJSONFile(string fileName) {
		return SimpleJSON.JSON.Parse (System.IO.File.ReadAllText("Assets/Project Assets/Resources/Parameters/"+ fileName +".json"));
	}

	/**
	 * Affiche un texte dans la console
	 * Plus rapide à écrire que Debug.Log
	 **/
	public static void Write(object message) {
		Debug.Log (message);
	}

	/// <summary>
	/// Retourne le joueur dont c'est le tour.
	/// </summary>
    public static Player currentPlayer() {
        return players[turnID];
    }
}
