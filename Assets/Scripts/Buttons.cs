using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Buttons : MonoBehaviour
{

    private Text row_num, column_num, cheese_num,cat_num,mouse_num;
    private GameObject initializer, searchRule;
    private GameObject gc, notification;
    private GameObject nextStep;

    private bool isAuto;

    // Start is called before the first frame update
    void Start()
    {
        row_num = GameObject.Find("Row_num").transform.Find("Text").GetComponent<Text>();
        column_num = GameObject.Find("Column_num").transform.Find("Text").GetComponent<Text>();
        cheese_num = GameObject.Find("Cheese_num").transform.Find("Text").GetComponent<Text>();
        cat_num = GameObject.Find("Cat_num").transform.Find("Text").GetComponent<Text>();
        mouse_num = GameObject.Find("Mouse_num").transform.Find("Text").GetComponent<Text>();
        searchRule = GameObject.Find("Rules");
        initializer = GameObject.Find("Initializer");
        notification = GameObject.Find("Notification");
        gc = GameObject.Find("Controller");


        nextStep = GameObject.Find("NextStep");

    }

    // Update is called once per frame
    void Update()
    {

        if (gc.GetComponent<GameController>().gameState == GameController.GameState.InGame)
        {
            nextStep.GetComponent<Image>().color = Color.white ;
        }
        else {
            nextStep.GetComponent<Image>().color = Color.gray;
        }

    }

    public void SetBoard()
    {
        int r = int.Parse(row_num.text);
        int c = int.Parse(column_num.text);
        int ch = int.Parse(cheese_num.text);
        int cat = int.Parse(cat_num.text);
        int mouse = int.Parse(mouse_num.text);

        switch (searchRule.GetComponent<Dropdown>().value) {
            case 0:
                gc.GetComponent<GameController>().searchRule = GameController.SearchRule.DFS;
                break;
            case 1:
                gc.GetComponent<GameController>().searchRule = GameController.SearchRule.BFS;
                break;
            case 2:
                gc.GetComponent<GameController>().searchRule = GameController.SearchRule.AStar1;
                break;
            case 3:
                gc.GetComponent<GameController>().searchRule = GameController.SearchRule.AStar2;
                break;
            case 4:
                gc.GetComponent<GameController>().searchRule = GameController.SearchRule.AStar3;
                break;
            default:
                gc.GetComponent<GameController>().searchRule = GameController.SearchRule.IDDFS;
                break;

        }

        if (r * c >= ch + 2 && r > 0 && c > 0 && ch > 0)
        {
            notification.GetComponent<Notification>().ChangeContent("Board Initialized");
            initializer.GetComponent<Initializer>().InitialBoard(r, c, ch, cat, mouse);

        }

        else {
            notification.GetComponent<Notification>().ChangeContent("Default Board Initialized");
            initializer.GetComponent<Initializer>().InitialBoard(10, 10, 3,1,1);

        }

        gc.GetComponent<GameController>().currentPlayer = GameController.Players.Mouse;
        gc.GetComponent<GameController>().playerBehavior = GameController.PlayerBehavior.ShowPath;

    }


    public void NextStep() {

        if (gc.GetComponent<GameController>().board == null)
        {
            notification.GetComponent<Notification>().ChangeContent("Plase Set Board First");
            return ;
        }
        if (gc.GetComponent<GameController>().gameState == GameController.GameState.InGame) {
            if (gc.GetComponent<GameController>().playerBehavior == GameController.PlayerBehavior.ShowPath)
            {
                gc.GetComponent<GameController>().ShowPath();
                return;
            }
            else if (gc.GetComponent<GameController>().playerBehavior == GameController.PlayerBehavior.MoveToTarget)
            {
                gc.GetComponent<GameController>().MoveToTarget();
            }
            return;
        }

    }

    /*
    public void EnableAutoRun() {
        isAuto = true;
        StartCoroutine("AutoRun");
        autoRun.GetComponent<Button>().onClick.RemoveAllListeners();
        autoRun.GetComponent<Button>().onClick.AddListener(DisableAutoRun);
    }

    public void DisableAutoRun()
    {
        isAuto = false;
        StopAllCoroutines();

        autoRun.GetComponent<Button>().onClick.RemoveAllListeners();
        autoRun.GetComponent<Button>().onClick.AddListener(EnableAutoRun);
    }

    IEnumerator AutoRun() {
        bool flag = true;
        while (flag){
            flag = OneStep();
            yield return new WaitForSeconds(1.5f);
        }
        
    }
    */
}
