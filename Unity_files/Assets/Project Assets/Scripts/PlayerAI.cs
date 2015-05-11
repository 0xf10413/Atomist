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
    public PlayerAI (string nName, int dDifficulty=2)
        : base (nName)
    { hasPlayed = false;
    difficulty = dDifficulty;
    printName += difficulty == 0 ? " ★" :
            difficulty == 1 ? " ★★" :
            difficulty == 2 ? " ★★★" : "";
        printName = "IA-" + printName;
    }

    /// <summary>
    /// Initialisation des éléments graphiques. On masque tout sauf
    /// le plateau et l'affichage des rangs.
    /// </summary>
    public override void init () {
        base.init ();
        playerScreen.transform.Find ("Energy container").gameObject.SetActive (false);
        //playerScreen.transform.Find ("Cards List").gameObject.SetActive (false);
        playerScreen.transform.Find ("Reactions").gameObject.SetActive (false);
        playerScreen.transform.Find ("Card Buttons").gameObject.SetActive (false);
    }

    /// <summary>
    /// Récupération des cartes. À partir de ce moment, le comportement classique change.
    /// L'IA réussit toujours à récupérer les deux cartes.
    /// </summary>
    public override void pickCards (int nbCards, string message, bool askInPeriodicTable) {
        bool[] getTheCard = new bool[nbCards];
        Element[] cardsPicked = new Element[nbCards];
        double probToFind = 0;
        switch (difficulty) {
            case 0 :
                probToFind = 0.5;
                break;
            case 1 :
                probToFind = 0.8;
                break;
            case 2 :
                probToFind = 1;
                break;
        }
        for (int i=0;i<nbCards;i++) {
            if (!askInPeriodicTable || cardsDiscovered.Contains(cardsPicked[i]))
                getTheCard[i] = true;
            else
                getTheCard[i] = (Main.randomGenerator.NextDouble() < probToFind);
        }
        int nbCardsActuallyPicked = 0;
        for (int i = 0; i < nbCards; i++) {
            if (getTheCard[i]) {
                addCardToPlayer (Main.pickCard ());
                nbCardsActuallyPicked++;
            }
        }

        // Ajouter un chrono ?
        if (askInPeriodicTable) {
            Main.infoDialog (name + " pioche " + nbCards.ToString () + " carte" + (nbCards > 1 ? "s" : ""), delegate {
                Main.infoDialog (name + " récupère " + nbCardsActuallyPicked + " carte" + (nbCardsActuallyPicked > 1 ? "s" : ""), delegate {
                    think ();
                });
            });
        }
        else {
            Main.infoDialog (name + " récupère " + nbCards.ToString () + " carte" + (nbCards > 1 ? "s" : ""), delegate {
                think ();
            });
        }
    }

    /// <summary>
    /// Fonction principale de l'IA. Prend les décisions.
    /// Actuellement minimaliste : joue la première réaction possible.
    /// Elle est appelée récursivement jusqu'à la décision de fin de tour.
    /// </summary>
    public void think () {
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
        if (r != null) {
            if (r is DelayedReaction) {
                consumeForReaction(r);
                Player cible = null;
                foreach (Player p in Main.players) {
                    if (p != this) {
                        if (p.isPlaying) {
                            if (cible == null)
                                cible = p;
                            else if (cible.room > p.room) // Todo : shuffle
                                cible = p;
                        }
                    }
                }
                if (cible != null) {
                    Main.infoDialog(name + " effectue la réaction \""+ r.reagents+"->"+r.products +"\" sur "+ cible.name, delegate {
                        think();
                    });
                    ((DelayedReaction) r).inflict(cible);
                    hasPlayed = true;
                }
                else
                    r = null;
            }
            else {
                r.effect (this); // Le déplacement est inclus dans cette fonction.
                hasPlayed = true;
            }
        }
        if (r == null) {
            if (hasPlayed) {
                Main.infoDialog (name + " vient de finir son tour", delegate { EndTurn (); });
                hasPlayed = false;
            }
            else
                Main.infoDialog (name + " passe son tour", delegate { EndTurn (); });
        }
    }

    /// <summary>
    /// Détermine la prochaine réaction à jouer.
    /// </summary>
    /// <returns>Une réaction jouable si elle existe, null sinon.</returns>
    public Reaction chooseReaction () {
        Reaction reaction = null;
        List<ObstacleToken> obs = new List<ObstacleToken> (); // Obstacles à examiner (au plus 2)
        foreach (ObstacleToken o in obstacles) {
            if (o.room == room)
                obs.Add (o);
        }

        List<Reaction> potential = new List<Reaction> (); // Réactions permettant de sortir
        List<Reaction> penalties = new List<Reaction> (); // Réactions d'attaque
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
            if (possibleReaction) {
                if (r is DelayedReaction) // Si c'est une réaction à pénalité
                    penalties.Add(r);
                // Compatibilité avec l'un des obstacles au moins
                else if (null != obs.Find (o => o.obstacle.weakness == r.type))
                    // Niveau d'énergie requis
                    if (r.cost <= energy)
                        potential.Add (r);
            }
        }
        if (potential.Count == 0) {
            if (penalties.Count == 0)
                return null;
            // S'il y a des réactions à pénalité possibles, mais aucune réaction à obstacle
            if (difficulty == 0)
                return null; // Le joueur facile n'attaque pas
            else {
                double[] choiceValues = new double[penalties.Count];
                for (int i=0;i<choiceValues.Length;i++) {
                    Reaction r = penalties[i];
                    if (r.type.name == "Poison")
                        choiceValues[i] = ((PoisonReaction) r).nbOfTurns;
                    else if (r.type.name == "Radioactif")
                        choiceValues[i] = 1/UraniumReaction.PROBA_DESINTEGRATION;
                }
                int idChoix = 0;
                for (int i=1;i<choiceValues.Length;i++) {
                    if (choiceValues[i] < choiceValues[idChoix])
                        i = idChoix;
                }
                return penalties[idChoix];
            }
        }

        // Recherche d'un minimum
        reaction = potential[0];
        foreach (Reaction r in potential)
            if (r.cost < reaction.cost)
                reaction = r;

        foreach (Reaction r in Main.reactions) {

        }
        return reaction;
    }

    public override void moveToNextRoom () {
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
