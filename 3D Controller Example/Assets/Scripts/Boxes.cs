
using System.Collections;
using UnityEngine;
public class Boxes : MonoBehaviour
{
    [Header("Caja Normal")]
    public int jumpResistance = 1;
    public GameObject collectiblePrefab;

    [Header("TNT")]
    public bool isTNT;
    public float radiusExplosion = 5f;
    public bool alreadyBoom = false;
    public bool alreadyBounce = false;

    [Header("NITRO")]
    public bool isNITRO;
    public void DamageToBox()
    {
        if (isTNT || isNITRO)
        {
            if (!alreadyBounce)
            {
                
                alreadyBounce = true;

                if (isTNT)
                {
                    
                    StartCoroutine(CooldownTNT());
                }
                else if (isNITRO)
                {
                    
                    BoxExplosion();
                }
            }
            
            return;
        }

        
        jumpResistance--;

        if (collectiblePrefab != null)
        {
            Instantiate(collectiblePrefab, transform.position, collectiblePrefab.transform.rotation);
        }

        if (jumpResistance <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void BoxExplosion()
    {
        if (!alreadyBoom)
        {
            alreadyBoom = true;

            Collider[] explosions = Physics.OverlapSphere(transform.position, radiusExplosion);
            foreach (Collider explosion in explosions)
            {
                if (explosion.CompareTag("Player"))
                {
                    PlayerCharacterController PCC = GameObject.Find("Player").GetComponent<PlayerCharacterController>();
                    PCC.life--;
                }
                if (explosion.CompareTag("Box"))
                {
                    Destroy(explosion.gameObject);
                }

                if (isNITRO || isTNT)
                {
                    Boxes otherBox = explosion.gameObject.GetComponent<Boxes>();
                    if (otherBox != null)
                    {
                        otherBox.BoxExplosion();
                    }
                }

            }
            Debug.Log("BOOM!");
            Destroy(gameObject);
        }
    }

    

    public IEnumerator CooldownTNT()
    {
        yield return new WaitForSeconds(3f);
        BoxExplosion();
    }
    
}