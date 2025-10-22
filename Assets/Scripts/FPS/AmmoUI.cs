using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public WeaponSwitcher switcher;
    public Text ammoText;

    void Update()
    {
        var w = switcher ? switcher.Current : null;
        if (!w || !ammoText) return;
        ammoText.text = $"{w.CurrentAmmo} / {w.ReserveAmmo}";
    }
}
