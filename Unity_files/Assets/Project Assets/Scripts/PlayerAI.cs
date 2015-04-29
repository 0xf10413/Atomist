using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Threading;

/// <summary>
/// Un joueur piloté par l'intelligence artificielle.
/// </summary>
public class PlayerAI : Player
{
    /// <summary>
    /// Le joueur a-t-il fait une action ? On change
    /// la phrase de fin de tour en fonction.
    /// </summary>
    public bool hasPlayed { get; private set; }

    /// <summary>
    /// La difficulté. De 0 à 2, difficulté et agressivité croissantes. 
    /// Symbolisée par 8), :| et >(.
    /// </summary>
    public int difficulty { get; private set; }

    /// <summary>
    /// Le constructeur usuel. Ajoute simplement le nom.
    /// </summary>
    /// <param name="nName">Le nom de l'IA.</param>
    /// <param name="dDifficulty">La difficulté, entre 0 et 2.</param>
    public PlayerAI (string nName, int dDifficulty=0)
        : base (nName)
    { hasPlayed = false;
    difficulty = dDifficulty;
        printName += difficulty == 0 ? " 8)" :
            difficulty == 1 ? " :|" :
            difficulty == 2 ? " >(" : ""; // Mon dieu que c'est moche
        printName = "IA-" + printName;
    }

    /// <summary>
    /// Initialisation des éléments graphiques. On masque tout sauf
    /// le plateau et l'affichage des rangs.
    /// </summary>
    public override void init ()
    {
        base.init ();
        playerScreen.transform.Find ("Energy container").gameObject.SetActive (false);
        playerScreen.transform.Find ("Cards List").gameObject.SetActive (false);
        playerScreen.transform.Find ("Reactions").gameObject.SetActive (false);
        playerScreen.transform.Find ("Turn buttons").gameObject.SetActive (false);
    }

    /// <summary>
    /// Récupération des cartes. À partir de ce moment, le comportement classique change.
    /// L'IA réussit toujours à récupérer les deux cartes.
    /// </summary>
    public override void pickCards (int nbCards, string message, bool askInPeriodicTable)
    {
        for (int i = 0; i < nbCards; i++)
            deck.AddCard (Main.pickCard ());

        // Ajouter un chrono ? 
        Main.infoDialog (name + " pioche " + nbCards.ToString () +
            " carte" + (nbCards > 1 ? "s" : ""), delegate { think (); });

    }

    /// <summary>
    /// Fonction principale de l'IA. Prend les décisions.
    /// Actuellement minimaliste : joue la première réaction possible.
    /// Elle est appelée récursivement jusqu'à la décision de fin de tour.
    /// </summary>
    public void think ()
    {

        // D'abord, on jette les gaz nobles
        if (deck.listCards.Find (ca => ca.element.family == "Gaz Noble") != null) {
            int energyToGain = 0;
            int nbCardsToPick = 0;
            int thrown = 0;
            for (int i = deck.getNbCards () - 1; i >= 0; i--) {
                Card c = deck.getCard (i);
                if (c.element.family == "Gaz Noble") {
                    energyToGain += NOBLE_GAZ_ENERGY * c.nbCards;
                    nbCardsToPick += NOBLE_GAZ_CARDS * c.nbCards;
                    thrown += c.nbCards;
                    deck.RemoveCards (c.element, c.nbCards);
                }
            }

            energy += energyToGain;
            deck.updatePositions ();

            Main.infoDialog (name + " jette " + thrown + " carte" + 
                (thrown > 1 ? "s" : "") + " gaz noble", delegate
            { pickCards (nbCardsToPick, false); });
            hasPlayed = true;
            return;

        }
        // On essaie d'avancer
        Reaction r = chooseReaction ();
        if (r == null)
            if (hasPlayed) {
                Main.infoDialog (name + " vient de finir son tour", delegate { EndTurn (); });
                hasPlayed = false;
            }
            else
                Main.infoDialog (name + " passe son tour", delegate { EndTurn (); });
        else {
            r.effect (this); // Le déplacement est inclus dans cette fonction.
            hasPlayed = true;
        }
    }

    /// <summary>
    /// Détermine la prochaine réaction à jouer.
    /// </summary>
    /// <returns>Une réaction jouable si elle existe, null sinon.</returns>
    public Reaction chooseReaction ()
    {
        Reaction reaction = null;
        List<ObstacleToken> obs = new List<ObstacleToken> (); // Obstacles à examiner (au plus 2)
        foreach (ObstacleToken o in obstacles) {
            if (o.room == room)
                obs.Add (o);
        }

        List<Reaction> potential = new List<Reaction> (); // Réactions permettant de sortir
        foreach (Reaction r in Main.reactions) {
            // Réactifs présents
            bool possibleReaction = true;
            foreach (KeyValuePair<Element, int> reagents in r.reagentsList) {
                Card eltCard = deck.getCard (reagents.Key);
                if (eltCard == null) {
                    possibleReaction = false;
                    break;
                }
                if (eltCard.nbCards < reagents.Value) {
                    possibleReaction = false;
                    break;
                }
            }

            if (possibleReaction)
                // Compatibilité avec l'un des obstacles au moins
                if (null != obs.Find (o => o.obstacle.weakness == r.type))
                    // Niveau d'énergie requis
                    if (r.cost <= energy)
                        potential.Add (r);
        }
        if (potential.Count == 0)
            return null;

        // Recherche d'un minimum
        reaction = potential[0];
        foreach (Reaction r in potential)
            if (r.cost < reaction.cost)
                reaction = r;
        return reaction;
    }

    public override void moveToNextRoom ()
    {
        room++;
        updateRanks ();
        foreach (Penalty p in penalties)
            p.Remove ();
        penalties.Clear ();
        progressMoveToNextRoom ();

        if (room >= NB_ROOMS)
            Main.infoDialog (name + " lance une réaction ... et gagne !",
                delegate
                {
                    isPlaying = false;
                    Main.winners.Add (this);
                    think (); // Pour conclure les dernières actions
                });

        else
            Main.infoDialog (name + " lance une réaction... et passe à la salle"
                + " suivante !",
                delegate {  think (); });
    }



}
