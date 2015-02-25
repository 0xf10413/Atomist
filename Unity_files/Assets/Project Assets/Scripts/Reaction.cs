using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contient les informations d'une réaction chimique : réactifs, produits
/// </summary>
public class Reaction {

	string reagents  { get; set; } // Texte contenant les réactifs
	string products  { get; set; } // Texte contenant les produits
	List<KeyValuePair<Element,int>> reagentsList  { get; set; } // Liste de couples (élément, stoechiométrie)
	ReactionType type { get; set; } // Type de réaction (glace, feu, etc)
    
    /// <summary>
    /// Constructeur de la réaction
    /// </summary>
    /// <param name="tReagents">Un texte contenant les réactifs</param>
    /// <param name="tProducts">Un texte contenant les produits</param>
    /// <param name="lReagents">Une liste de réactifs sous la forme ((H,2),(O,1))</param>
    /// <param name="nType">Le type de réaction</param>
	public Reaction(string tReagents, string tProducts, List<KeyValuePair<Element,int>> lReagents, ReactionType nType) {
		reagents = tReagents;
		products = tProducts;
		reagentsList = lReagents;
		type = nType;
	}
}
