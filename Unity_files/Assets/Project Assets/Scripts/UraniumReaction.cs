using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contient la réaction de type uranium (oui une classe rien que pour lui)
/// </summary>
public class UraniumReaction : DelayedReaction {

    public static ReactionType uraniumReaction = new ReactionType("Radioactif");
    public const float PROBA_DESINTEGRATION = 0.17f;

    private static List<KeyValuePair<Element,int>> uraniumReagents() {
        List<KeyValuePair<Element,int>> res = new List<KeyValuePair<Element,int>>();
        res.Add(new KeyValuePair<Element,int>(Main.elements.Find(elt=>elt.symbole=="U"),1));
        return res;
    }

    /// <summary>
    /// Constructeur de la réaction
    /// </summary>
    public UraniumReaction() : base("U", "Th", uraniumReagents(), uraniumReaction, 4, 5, "À chaque tour, le joueur a une chance sur <b>"+ Mathf.Round(1f/PROBA_DESINTEGRATION) +"</b> de passer son tour, jusqu'au prochain changement de salle.", "Désintégration de l'uranium") {
    }

    public override void inflict(Player target)
    {
        GameObject penaltyToken = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PenaltyToken"));
        penaltyToken.transform.SetParent(target.playerScreen.transform.Find("BoardGame/PenaltyTokensContainer"+ target.room));
        target.penalties.Add (new Penalty (delegate {
            if (Main.randomGenerator.NextDouble() < PROBA_DESINTEGRATION) {
                if (target.hisTurn()) { // Si on n'a pas déjà sauté le tour du joueur
                    target.undoTurn ();
                    Main.infoDialog ("Au tour de "+ target.name +"\nL'uranium s'est désintégré ! Tour annulé.", delegate {
                        target.EndTurn();
                    });
                }
                return true;
            }
            return false;
        }, delegate {
            GameObject.Destroy(penaltyToken);
        }));
    }
}
