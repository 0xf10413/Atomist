using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contient la réaction de type uranium (oui une classe rien que pour lui)
/// </summary>
public class UraniumReaction : DelayedReaction {

    public static int NB_EDGES = 6;

    public static ReactionType uraniumReaction = new ReactionType("Radioactif");
    private static Sprite[] diceRessources;

    private static List<KeyValuePair<Element,int>> uraniumReagents() {
        List<KeyValuePair<Element,int>> res = new List<KeyValuePair<Element,int>>();
        res.Add(new KeyValuePair<Element,int>(Main.elements.Find(elt=>elt.symbole=="U"),1));
        return res;
    }

    /// <summary>
    /// Constructeur de la réaction
    /// </summary>
    public UraniumReaction() : base("U", "Th", uraniumReagents(), uraniumReaction, 4, 5, "À chaque tour, et jusqu'au prochain changement de salle, un dé est lancé. Si le dé tombe sur "+ NB_EDGES +", le joueur passe son tour.", "Désintégration de l'uranium") {
        if (diceRessources == null) {
            diceRessources = new Sprite[NB_EDGES];
            for (int i=0;i<NB_EDGES;i++)
                diceRessources[i] = Resources.Load<Sprite>("Images/Dices/edge"+ (i+1));
        }
    }

    private static bool diceRollSkipped; // Oui, une variable globale. Oui, c'est moche

    public override void inflict(Player target)
    {
        GameObject penaltyToken = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PenaltyToken"));
        penaltyToken.transform.SetParent(target.playerScreen.transform.Find("Board Container/BoardGame/PenaltyTokensContainer"+ target.room));
        target.penalties.Insert (0, new Penalty(p => {
            if (target.hisTurn()) {
                target.undoTurn ();
                GameObject mask = Main.AddMask();
                GameObject uraniumDialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/UraniumDialog"));
                GameObject msg = uraniumDialog.transform.Find("Message").gameObject;
                msg.GetComponent<Text>().text = "Au tour de "+ Main.currentPlayer().name + ".\nL'uranium va-t'il se désintégrer ?";
                GameObject ok = uraniumDialog.transform.Find("Ok Button").gameObject;
                GameObject dice = uraniumDialog.transform.Find("Dice").gameObject;
                Main.addClickEvent(ok, delegate {
                    diceRollSkipped = true;
                    int rand = Main.randomGenerator.Next(NB_EDGES);
                    dice.GetComponent<Image>().sprite = diceRessources[rand];
                    onFinishRoll(rand, mask, uraniumDialog, p);
                });
                   Main.autoFocus(ok);
                uraniumDialog.transform.SetParent(mask.transform,false);
                diceRollSkipped = false;
                launchDiceAnimation(p,mask,uraniumDialog);
                return true;
            }
            return false;
        }, delegate {
            GameObject.Destroy(penaltyToken);
        }));
    }
    
    private void launchDiceAnimation(Penalty penalty, GameObject mask, GameObject dialog) {
        launchDiceAnimation(penalty,mask,dialog,20);
    }
    private void launchDiceAnimation(Penalty penalty, GameObject mask, GameObject dialog, int nbFrames) {
        if (diceRollSkipped)
            return;
        GameObject dice = dialog.transform.Find("Dice").gameObject;
        int rand;
        do {
            rand = Main.randomGenerator.Next(NB_EDGES);
        } while (dice.GetComponent<Image>().sprite == diceRessources[rand]);
        dice.GetComponent<Image>().sprite = diceRessources[rand];
        if (nbFrames == 0)
            onFinishRoll(rand, mask, dialog, penalty);
        else {
            Main.postTask(delegate {
                launchDiceAnimation(penalty,mask,dialog,nbFrames-1);
            }, 0.1f);
        }
    }

    /// <summary>
    /// Fonction à appeler à la fin d'un lancer de dé
    /// </summary>
    /// <param name="rand">Le nombre entre 1 et 6</param>
    /// <param name="mask">Le masque</param>
    /// <param name="dialog">La boite de dialogue</param>
    /// <param name="penalty">La pénalité</param>
    private void onFinishRoll(int rand, GameObject mask, GameObject dialog, Penalty penalty) {
        GameObject msg = dialog.transform.Find("Message").gameObject;
        GameObject ok = dialog.transform.Find("Ok Button").gameObject;
        ok.transform.Find("Text").gameObject.GetComponent<Text>().text = "Ok";
        Main.removeEvents(ok);
        if (rand == (NB_EDGES-1)) {
            msg.GetComponent<Text>().text = "L'uranium s'est désintégré !\nTour annulé.";
            Main.addClickEvent(ok, delegate {
                GameObject.Destroy(mask);
                penalty.Remove();
                Main.currentPlayer().penalties.Remove(penalty);
                Main.currentPlayer().EndTurn();
            });
            Main.autoFocus(ok);
        }
        else {
            msg.GetComponent<Text>().text = "L'uranium ne s'est pas désintégré.";
            Main.addClickEvent(ok, delegate {
                GameObject.Destroy(mask);
                Main.currentPlayer().BeginTurn();
            });
        }
    }

    private void submitDialog() {
    }
}
