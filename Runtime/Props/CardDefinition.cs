using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card Definition", menuName = "Very Real Help/Prop/CardDefinition")]
public class CardDefinition : ScriptableObject
{
    public Sprite Deck;
    public List<Sprite> Cards;
}
