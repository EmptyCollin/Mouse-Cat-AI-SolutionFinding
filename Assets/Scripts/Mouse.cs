using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour
{

    public GameObject currentGrid;
    public List<GameController.Pair> path;
    public bool shouldBeRemove;

    private GameObject gc;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
        gc = GameObject.Find("Controller");
        shouldBeRemove = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static GameController.Pair NextStep(GameController.Pair pos, List<GameController.Pair> cheeses,int width,int height) {

        float minDis = 10000000;
        int index = 0;
        float distance;

        List<GameController.Pair> candidates = FindCandidates(pos,width,height);
        for (int i = 0; i < candidates.Count; i++)
        {
            for (int j = 0; j < cheeses.Count; j++) {
                distance = GameController.Distance(candidates[i], cheeses[j]);
                if (distance < minDis)
                {
                    minDis = distance;
                    index = i;
                }
            }
            
            
        }
        return candidates[index];

    }

    public static List<GameController.Pair> FindCandidates(GameController.Pair pos,int width, int height) {
        List<GameController.Pair> candidates = new List<GameController.Pair>();
        GameController.Pair p;

        int r = pos.r;
        int c = pos.c;

        p = new GameController.Pair();
        p.r = r + 1;
        p.c = c + 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r - 1;
        p.c = c - 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r - 1;
        p.c = c;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r - 1;
        p.c = c + 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r;
        p.c = c + 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r + 1;
        p.c = c;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r + 1;
        p.c = c - 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r;
        p.c = c - 1;
        candidates.Add(p);


        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            if (candidates[i].r < 0 || candidates[i].r >= height || candidates[i].c < 0 || candidates[i].c >= width)
                candidates.Remove(candidates[i]);
        }
        return candidates;
    } 


}
