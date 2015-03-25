using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contient une réaction à retardement, c'est-à-dire une réaction qui a un effet sur le joueur au bout d'un certain nombre de tours.
/// </summary>
abstract public class DelayedReaction : Reaction {

    /// <summary>
    /// Constructeur de la réaction
    /// </summary>
    /// <param name="tReagents">Un texte contenant les réactifs</param>
    /// <param name="tProducts">Un texte contenant les produits</param>
    /// <param name="lReagents">Une liste de réactifs sous la forme ((H,2),(O,1))</param>
    /// <param name="nType">Le type de réaction</param>
	public DelayedReaction(string tReagents, string tProducts, List<KeyValuePair<Element,int>> lReagents, ReactionType nType, int cCost, int gGain) : 
        base(tReagents,tProducts,lReagents,nType, cCost, gGain) {
	}

    abstract public void inflict (Player cible);

    public override void effect(Player maker) {
        GameObject mask = Main.AddMask();
        mask.SetActive(false); // On cache le masque tamporairement sinon la fenêtre de dialogue est affichée subitement au mauvais endroit
        GameObject dialog = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerSelectorDialog"));
        dialog.transform.SetParent(mask.transform);
        dialog.transform.localPosition = new Vector3(0,0,0);
        mask.SetActive(true); // On réaffiche le masque maintenant que le cadre est bien placé
        Main.addClickEvent(mask, delegate {
            GameObject.Destroy(mask);
        });
        foreach (Player p in Main.players) {
            if (p != maker) {
                GameObject playerSelector = (GameObject) GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerSelectorButton"));
                playerSelector.transform.SetParent(dialog.transform.Find("PlayersList"));
                playerSelector.transform.Find("Text").gameObject.GetComponent<Text>().text = p.name;

                Player localVarP = p; 
                Main.addClickEvent(playerSelector, delegate {
                    Object.Destroy(dialog);
                    Object.Destroy(mask);
                    maker.consumeForReaction(this);
                    inflict(localVarP);
                });
            }
        }
    }
}
