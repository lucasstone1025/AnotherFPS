using UnityEngine;
using UnityEngine.Events;
public class NewMonoBehaviourScript : MonoBehaviour
{
    public UnityEvent OnGunShoot;
    public float FireCooldown;

    public bool Automatic;

    private float CurrentCooldown;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentCooldown = FireCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if(Automatic)
        {
            if(Input.GetMouseButton(0))
            {
                if(CurrentCooldown <= 0f)
                {
                    OnGunShoot?.Invoke();
                    CurrentCooldown = FireCooldown;
                }
                else
                {
                    if(Input.GetMouseButton(0))
                    {
                        
                    }
                }
            }
        }  
    }
}
