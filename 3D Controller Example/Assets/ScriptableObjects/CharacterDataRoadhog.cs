using UnityEngine;

[CreateAssetMenu(fileName = "Roadhog", menuName = "Player/Roadhog")]
public class CharacterDataRoadhog : ScriptableObject
{
    public int Vidas = 5;
    public float Speed = 7;
    public float JumpForce = 10f;
    public int frupoints = 0;
    public int cajasDestruidas = 0;
    void Start()
    {
        
    }

    
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
