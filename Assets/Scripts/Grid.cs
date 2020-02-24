using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int row,column;
    public Color color;
    public State state;

    public enum State {Empty,Cat,Mouse,Cheese,CatAndCheese};
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
