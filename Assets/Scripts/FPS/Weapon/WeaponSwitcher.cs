using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    public WeaponBase[] weapons;
    public int index;

    void Start() => Equip(index);

    void Update()
    {

        WeaponBase current = Current;

        if (current != null && current.isReloading)
            return;

        // 1,2,3...
        for (int i = 0; i < weapons.Length && i < 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) Equip(i);

        // ÈÙ
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) Equip((index - 1 + weapons.Length) % weapons.Length);
        else if (scroll < 0f) Equip((index + 1) % weapons.Length);
    }

    void Equip(int i)
    {
        index = Mathf.Clamp(i, 0, weapons.Length - 1);
        for (int k = 0; k < weapons.Length; k++)
        {
            weapons[k].gameObject.SetActive(k == index);
            // ÀüÈ¯ ½Ã °­Á¦ ¼û±è(Reload)
            if (weapons[k].reloadUI) weapons[k].reloadUI.ShowReload(false);
        }
    }

    public WeaponBase Current => weapons != null && weapons.Length > 0 ? weapons[index] : null;
}
