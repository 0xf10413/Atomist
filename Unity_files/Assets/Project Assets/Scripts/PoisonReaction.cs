using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PoisonReaction : DelayedReaction
{
    public int nbOfTurns { get; private set; }
    public PoisonReaction (string tReagents, string tProducts, List<KeyValuePair<Element, int>> lReagents, ReactionType nType, int cCost, int gGain, int nnbOfTurns, string tInfo)
        : base (tReagents, tProducts, lReagents, nType, cCost, gGain, "Au bout de <b>"+ nnbOfTurns +"</b> tours, si le joueur n'a pas changé de salle, il passe son tour.", tInfo)
    {
        nbOfTurns = nnbOfTurns;
    }

    public override void inflict(Player target)
    {
        GameObject penaltyToken = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PenaltyToken"));
        penaltyToken.transform.Find("RemainingTurns").GetComponent<Text>().text = nbOfTurns.ToString();
        penaltyToken.transform.SetParent(target.playerScreen.transform.Find("Board Container/BoardGame/PenaltyTokensContainer"+ target.room));

        int remainingTurns = nbOfTurns;

        target.penalties.Add (new Penalty (p => {
            if (remainingTurns > 0) {
                penaltyToken.transform.Find("RemainingTurns").GetComponent<Text>().text = remainingTurns.ToString();
                remainingTurns--;
            }
            else {
                if (target.hisTurn()) { // Si on n'a pas déjà sauté le tour du joueur (avec une autre pénalité)
                    target.undoTurn ();
                    Main.infoDialog ("Au tour de "+ target.name +"\nTour annulé !", delegate {
                        target.EndTurn();
                    });
                }
                p.Remove();
            }
            return true;
        }, delegate {
            GameObject.Destroy(penaltyToken);
        }));
    }
}