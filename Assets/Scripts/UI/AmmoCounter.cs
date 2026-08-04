using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoCounter : MonoBehaviour
{
    public HandheldObject handHeld;
    public Image icon;
    public TMP_Text lbl_AmmoCount;
    public TMP_Text lbl_ClipCount;
    public Color StandardAmmo;
    public Color LowAmmo;

    public void Update()
    {
        if (handHeld.isGun)
        {
            HandheldGun handheldGun = (HandheldGun)handHeld;

            if (!lbl_ClipCount.enabled)
            {
                lbl_ClipCount.enabled = true;
            }

            lbl_AmmoCount.text = handheldGun.ammoCount.ToString();
            lbl_ClipCount.text = handheldGun.clipCount.ToString();

            icon.sprite = handheldGun.gunProfile.ammoIcon;

            if (handheldGun.ammoCount <= handheldGun.gunProfile.magazineSize / 10)
            {
                lbl_AmmoCount.color = LowAmmo;
            }
            else
            {
                lbl_AmmoCount.color = StandardAmmo;
            }

            if (handheldGun.clipCount <= 1)
            {
                lbl_ClipCount.color = LowAmmo;
            }
            else
            {
                lbl_ClipCount.color = StandardAmmo;
            }
        }
        if (handHeld.isGrenade)
        {
            HandheldGrenade handheldGrenade = (HandheldGrenade)handHeld;

            if (lbl_ClipCount.enabled)
            {
                lbl_ClipCount.enabled = false;
            }

            lbl_AmmoCount.text = handheldGrenade.ammoCount.ToString();

            icon.sprite = handheldGrenade.grenadeProfile.ammoIcon;

            if (handheldGrenade.ammoCount <= 1)
            {
                lbl_AmmoCount.color = LowAmmo;
            }
            else
            {
                lbl_AmmoCount.color = StandardAmmo;
            }
        }
    }
}
