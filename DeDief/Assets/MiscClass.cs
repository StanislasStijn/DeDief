using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new misc class", menuName = "Item/Misc")]
public class MiscClass : ItemClass
{
    public override ItemClass GetItem() { return this; }
    public override WeaponClass GetWeapon() { return null; }
    public override MiscClass GetMisc() { return this; }
}