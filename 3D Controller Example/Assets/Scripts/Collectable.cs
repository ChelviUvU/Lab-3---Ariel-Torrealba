using UnityEngine;

public class Collectable : MonoBehaviour
{
    public float speed = 18f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(speed * Time.deltaTime, 0f, 0f));
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Destroy(gameObject);
            Debug.Log("OBTENIDO!!!");
        }
    }
}
