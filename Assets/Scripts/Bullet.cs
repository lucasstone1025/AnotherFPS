using UnityEngine;

public class Bullet : MonoBehaviour
{
    private void OnCollisionEnter(Collision objHit)
    {
        if (objHit.gameObject.CompareTag("Target"))
        {
            print("hit " + objHit.gameObject.name + " !");

            CreateBulletImpactEffect(objHit);

            Destroy(gameObject);
        }

        if (objHit.gameObject.CompareTag("Wall"))
        {
            print("hit a wall!");

            CreateBulletImpactEffect(objHit);

            Destroy(gameObject);
        }

    }
    
    void CreateBulletImpactEffect(Collision objHit)
    {
        ContactPoint contact = objHit.contacts[0];

        GameObject hole = Instantiate(
            GlobalReferences.instance.bulletImpactEffectPrefab,
            contact.point,
            Quaternion.LookRotation(contact.normal)
        );

        hole.transform.SetParent(objHit.gameObject.transform);
    }

}
