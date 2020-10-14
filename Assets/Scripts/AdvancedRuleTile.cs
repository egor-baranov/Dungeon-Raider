using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "2D Extras/Tiles/Advanced Rule Tile")]
public class AdvancedRuleTile : RuleTile
{
    public List<TileBase> siblings;

    public override bool RuleMatch(int neighbor, TileBase other)
    {
        switch (neighbor)
        {
            case UnityEngine.RuleTile.TilingRule.Neighbor.This:
                return (siblings.Contains(other)
                    || base.RuleMatch(neighbor, other));
            case UnityEngine.RuleTile.TilingRule.Neighbor.NotThis:
                return (!siblings.Contains(other)
                    && base.RuleMatch(neighbor, other));
        }
        return base.RuleMatch(neighbor, other);
    }
}