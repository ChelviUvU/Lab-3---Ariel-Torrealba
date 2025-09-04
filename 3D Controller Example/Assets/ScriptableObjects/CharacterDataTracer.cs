using UnityEngine;


[CreateAssetMenu(fileName = "Player", menuName = "Player/Tracer")]
public class CharacterDataTracer : ScriptableObject
{
    public int Vidas = 3;
    public float Speed = 10;
    public float JumpForce = 15f;
    public int frupoints = 0;
    public int cajasDestruidas = 0;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetStats()
    {
        Vidas = 3;
        frupoints = 0;
        cajasDestruidas = 0;
        
    }

}
