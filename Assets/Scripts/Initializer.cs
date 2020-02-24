using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    private GameObject gc;
    // Start is called before the first frame update
    void Start()
    {
        gc = GameObject.Find("Controller");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitialBoard(int row_num, int column_num,int cheeses_num,int cat_num,int mouse_num)
    {
        RemoveAllUnits();
        gc.GetComponent<GameController>().board = new GameObject[row_num,column_num];
        Vector3 pos;
        GameObject grid;
        GameObject camera = GameObject.Find("Main Camera");
        camera.transform.position = new Vector3(0, Mathf.Max(row_num, column_num) * 10, 0);
        // Create Grid
        for (int i = 0; i< row_num; i++)
        {
            for(int j = 0; j < column_num; j++)
            {
                pos.x = (i - row_num / (float)2) * 10+5;
                pos.y = 0;
                pos.z = ( j- column_num / (float)2) * 10+5;

                grid = Instantiate(gc.GetComponent<GameController>().pre_grid) as GameObject;
                grid.transform.position = pos;
                grid.GetComponent<Grid>().state = Grid.State.Empty;
                grid.GetComponent<Grid>().row = i;
                grid.GetComponent<Grid>().column = j;
                grid.transform.name = "Grid" + (i * column_num + j).ToString();
                gc.GetComponent<GameController>().board[i,j] = grid;

            }
        }

        // Create Cats
        bool flag = true;
        List<GameObject> cats = new List<GameObject>();
        for (int i = 0; i < cat_num; i++)
        {
            int r = 0, c = 0;
            flag = true;
            while (flag)
            {
                r = (int)Random.Range(0, row_num);
                c = (int)Random.Range(0, column_num);
                if (gc.GetComponent<GameController>().board[r, c].GetComponent<Grid>().state == Grid.State.Empty)
                    flag = false;
            }
            GameObject cat;
            cat = Instantiate(gc.GetComponent<GameController>().pre_cat) as GameObject;
            cat.GetComponent<Cat>().currentGrid = gc.GetComponent<GameController>().board[r, c];
            cat.transform.position = new Vector3((r - row_num / (float)2) * 10 + 5, 0, (c - column_num / (float)2) * 10 + 5);
            gc.GetComponent<GameController>().board[r, c].GetComponent<Grid>().state = Grid.State.Cat;
            cats.Add(cat);
        }
        
        gc.GetComponent<GameController>().cat = cats;


        // Create Mice

        List<GameObject> mice = new List<GameObject>();
        for (int i = 0; i < mouse_num; i++) {
            int r=0, c=0;
            flag = true;
            while (flag)
            {
                r = (int)Random.Range(0, row_num);
                c = (int)Random.Range(0, column_num);
                if (gc.GetComponent<GameController>().board[r, c].GetComponent<Grid>().state == Grid.State.Empty)
                    flag = false;
            }
            GameObject mouse;
            mouse = Instantiate(gc.GetComponent<GameController>().pre_mouse) as GameObject;
            mouse.GetComponent<Mouse>().currentGrid = gc.GetComponent<GameController>().board[r, c];
            mouse.transform.position = new Vector3((r - row_num / (float)2) * 10 + 5, 0, (c - column_num / (float)2) * 10 + 5);
            gc.GetComponent<GameController>().board[r, c].GetComponent<Grid>().state = Grid.State.Mouse;
            mice.Add(mouse);
        }
        gc.GetComponent<GameController>().mouse = mice;


        // Create Chesses
        gc.GetComponent<GameController>().cheese = new List<GameObject>();
        int count = 0;
        while (count<cheeses_num) {
            int r = (int)Random.Range(0, row_num);
            int c = (int)Random.Range(0, column_num);
            if (gc.GetComponent<GameController>().board[r, c].GetComponent<Grid>().state == Grid.State.Empty)
            {
                GameObject cheese = Instantiate(gc.GetComponent<GameController>().pre_chess) as GameObject;
                gc.GetComponent<GameController>().cheese.Add(cheese);
                cheese.GetComponent<Cheese>().currentGrid = gc.GetComponent<GameController>().board[r, c];
                cheese.transform.position = new Vector3((r - row_num / (float)2) * 10 + 5, 0, (c - column_num / (float)2) * 10 + 5);
                gc.GetComponent<GameController>().board[r, c].GetComponent<Grid>().state = Grid.State.Cheese;
                count++;
            } 
        }

        gc.GetComponent<GameController>().PathFinding();

    }

    public void InitialBoard()
    {
        InitialBoard(10, 10, 3,1,1);
    }

    private void RemoveAllUnits()
    {
        GameObject[] l =GameObject.FindGameObjectsWithTag("Cheese");
        for(int i  = 0; i < l.Length; i++)
        {
            GameObject.Destroy(l[i]); 
        }
        l = GameObject.FindGameObjectsWithTag("Cat");
        for (int i = 0; i < l.Length; i++)
        {
            GameObject.Destroy(l[i]);
        }
        l = GameObject.FindGameObjectsWithTag("Mouse");
        for (int i = 0; i < l.Length; i++)
        {
            GameObject.Destroy(l[i]);
        }
        l = GameObject.FindGameObjectsWithTag("Grid");
        for (int i = 0; i < l.Length; i++)
        {
            GameObject.Destroy(l[i]);
        }
    }
}
