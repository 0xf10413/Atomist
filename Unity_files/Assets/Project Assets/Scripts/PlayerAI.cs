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
    /// Symbolisée par des étoiles.
    /// </summary>
    public int difficulty { get; private set; }

    /// <summary>
    /// Le constructeur usuel. Ajoute simplement le nom.
    /// </summary>
    /// <param name="nName">Le nom de l'IA.</param>
    /// <param name="dDifficulty">La difficulté, entre 0 et 2.</param>
    public PlayerAI (string nName, Color color, int dDifficulty=2)
        : base (nName,color)
    { hasPlayed = false;
    difficulty = dDifficulty;
    printName += difficulty == 0 ? " ★" :
            difficulty == 1 ? " ★★" :
            difficulty == 2 ? " ★★★" : "";
        printName = "IA-" + printName;
        cpu = true;
    }

    /// <summary>
    /// Initialisation des éléments graphiques. On masque tout sauf
    /// le plateau et l'affichage des rangs.
    /// </summary>
    public override void init () {
        base.init ();
        playerScreen.transform.Find ("Energy container").gameObject.SetActive (false);
        playerScreen.transform.Find ("Cards List").gameObject.SetActive (false);
        playerScreen.transform.Find ("Reactions").gameObject.SetActive (false);
        playerScreen.transform.Find ("Card Buttons").gameObject.SetActive (false);
    }

    /// <summary>
    /// Récupération des cartes. À partir de ce moment, le comportement classique change.
    /// </summary>
    public override void pickCards (int nbCards, string message, bool askInPeriodicTable) {
        bool[] getTheCard = new bool[nbCards];
        Element[] cardsPicked = new Element[nbCards];
        for (int i = 0; i < nbCards; i++) {
            if (Main.didacticiel)
                cardsPicked[i] = Main.getElementBySymbol("H"); // On ne donne à l'IA que des hydrogènes, comme ça elle peut rien faire
            else
                cardsPicked[i] = Main.pickCard ();
        }
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
            if (!askInPeriodicTable || cardsDiscovered.Contains(cardsPicked[i]) || cardsBuffer.Contains(cardsPicked[i]))
                getTheCard[i] = true;
            else
                getTheCard[i] = (Main.randomGenerator.NextDouble() < probToFind);
        }
        int nbCardsActuallyPicked = 0;
        List<Element> newInBuffer = new List<Element>();
        for (int i = 0; i < nbCards; i++) {
            if (getTheCard[i]) {
                addCardToPlayer (cardsPicked[i]);
                newInBuffer.Add(cardsPicked[i]);
                nbCardsActuallyPicked++;
            }
            else if (!cardsBuffer.Contains(cardsPicked[i])) {
                cardsBuffer.Add(cardsPicked[i]);
                newInBuffer.Add(cardsPicked[i]);
            }
        }
        if (askInPeriodicTable) {
            for (int i=0;i<cardsBuffer.Count;i++) {
                if (!newInBuffer.Contains(cardsBuffer[i])) {
                    addCardToPlayer (cardsBuffer[i]);
                    nbCardsActuallyPicked++;
                    i--;
                }
            }
        }

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

                int[] permutation = new int[Main.players.Count];
                for (int i=0;i<permutation.Length;i++)
                    permutation[i] = i;
                for (int i=0;i<permutation.Length;i++) {
                    int j = Main.randomGenerator.Next(permutation.Length);
                    int temp = permutation[i];
                    permutation[i] = permutation[j];
                    permutation[j] = temp;
                }
                Player cible = null;
                for (int i=0;i<Main.players.Count;i++) {
                    Player p = Main.players[permutation[i]];
                    if (p != this) {
                        if (p.isPlaying) {
                            if (cible == null)
                                cible = p;
                            else if ((cible.room > p.room) || ((difficulty == 2) && (cible.room == p.room) && (cible.deck.getTotalNbCards() < (p.deck.getTotalNbCards()+5))))
                                // On choisit le joueur le plus avancé. En cas d'égalité, l'IA difficile choisit le joueur qui a le moins de cartes, s'il y a un différentiel suffisament important (pour laisser un peu d'aléatoire)
                                cible = p;
                        }
                    }
                }
                if (cible != null) {
                    Main.infoDialog(name + " effectue la réaction \""+ r.reagents+" → "+r.products +"\" sur "+ cible.name, delegate {
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
                double[] nbTurnsBeforeEffect = new double[penalties.Count];
                double[] usedEnergy = new double[penalties.Count];
                for (int i=0;i<nbTurnsBeforeEffect.Length;i++) {
                    Reaction r = penalties[i];
                    if (r.type.name == "Poison")
                        nbTurnsBeforeEffect[i] = ((PoisonReaction) r).nbOfTurns;
                    else if (r.type.name == "Radioactif")
                        nbTurnsBeforeEffect[i] = UraniumReaction.NB_EDGES;
                    usedEnergy[i] = r.cost-r.gain;
                }
                int idChoix = 0;
                // On choisit la réaction qui a un effet le plus tôt possible, et en cas d'égalité, la moins coûteuse en NRJ
                for (int i=1;i<nbTurnsBeforeEffect.Length;i++) {
                    if ((nbTurnsBeforeEffect[i] < nbTurnsBeforeEffect[idChoix]) || ((nbTurnsBeforeEffect[i] == nbTurnsBeforeEffect[idChoix]) && (usedEnergy[i] < usedEnergy[idChoix])))
                        idChoix = i;
                }
                return penalties[idChoix];
            }
        }

        // Recherche d'un minimum
        reaction = potential[0];
        foreach (Reaction r in potential)
            if (r.cost < reaction.cost)
                reaction = r;

        return reaction;
    }

    public override void moveToNextRoom (int side = 0) {
        if (Main.moveLock) {
            Main.postTask (delegate { moveToNextRoom (side); }, 0.1f);
            return;
        }
        rooms.transform.Find ("Salle " + room + "/Obstacle" + (side == 2 ? "2" : "")).gameObject.SetActive (false);

        room++;
        updateRanks ();
        
        foreach (Penalty p in penalties)
            p.Remove ();
        penalties.Clear ();
        progressMoveToNextRoom (side);
        Main.moveLock = true;

        if (room >= NB_ROOMS)
            Main.infoDialog (name + " lance une réaction ... et gagne !",
                delegate
                {
                    isPlaying = false;
                    Main.winners.Add (this);
                    thinkLater (); // Pour conclure les dernières actions
                });

        else
            Main.infoDialog (name + " lance une réaction... et passe à la salle"
                + " suivante !",
                delegate {  think (); });
    }

    /// <summary>
    /// Simple fonction qui retarde la reflexion jusqu'à la fin de tout mouvement.
    /// </summary>
    private void thinkLater ()
    {
        if (Main.moveLock)
            Main.postTask (delegate { thinkLater (); }, 0.1f);
        else
            EndTurn ();
    }
}