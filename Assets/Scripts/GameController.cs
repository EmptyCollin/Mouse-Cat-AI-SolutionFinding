using System;
using System.Collections.Generic;
using Unity;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public UnityEngine.Object pre_grid;
    public UnityEngine.Object pre_cat;
    public UnityEngine.Object pre_mouse;
    public UnityEngine.Object pre_chess;

    public GameObject[,] board;
    public List<GameObject> cheese;
    public List<GameObject> mouse;
    public List<GameObject> cat;
    public enum SearchRule { DFS, BFS, AStar1, AStar2, AStar3, IDDFS };
    public enum GameState { Uninitial, InGame, EndGame };
    public enum Players { Mouse, Cat };
    public enum PlayerBehavior { ShowPath, MoveToTarget };
    //public enum Events { Continue, EndGame }

    private GameObject initializer;
    private GameObject notification;
    public SearchRule searchRule;
    public GameState gameState;
    public Players currentPlayer;
    public PlayerBehavior playerBehavior;
    public struct Pair
    {
        public int r;
        public int c;
    };
    private List<GameObject> choosenNode;

    public class PairWithIndex{
        public Pair p;
        public int index;

        public PairWithIndex(Pair _p, int _i) {
            p = _p;
            index = _i;
        }

    }

    public class BoardState : IEquatable<BoardState>
    {
        public BoardState parent;
        public List<BoardState> children;
        public List<PairWithIndex> cats;
        public List<PairWithIndex> mice;
        public List<Pair> cheeses;

        //  Fn, Hn, Gn are only used for A*
        private float fn = 0, hn = 0, gn = 0;
        public float Gn
        {
            get { return gn; }
            set { gn = value; fn = gn + hn; }
        }
        public float Hn
        {
            get { return hn; }
            set { hn = value; fn = gn + hn; }
        }
        public float Fn
        {
            get { return fn; }
        }

        public BoardState(BoardState _parent, 
                          List<BoardState> _children, 
                          List<PairWithIndex> _cats,
                          List<PairWithIndex> _mice,
                          List<Pair> _cheeses)
        {
            parent = _parent;
            children = _children;
            cheeses = _cheeses;
            mice = _mice;
            cats = _cats;
        }
        public bool Equals(BoardState o)
        {
            for (int i = 0; i < cats.Count;i++) {
                if (cats[i].p.r != o.cats[i].p.r || cats[i].p.c != o.cats[i].p.c) return false;
            }
            if (mice.Count != o.mice.Count)
            {
                return false;
            }
            else {
                for (int i = 0; i < mice.Count; i++)
                {
                    if (mice[i].p.r != o.mice[i].p.r || mice[i].p.c != o.mice[i].p.c) return false;
                }
            }
            if (cheeses.Count != o.cheeses.Count)
            {
                return false;
            }
            else {
                for (int i = 0; i < cheeses.Count; i++)
                {
                    if (cheeses[i].r != o.cheeses[i].r || cheeses[i].c != o.cheeses[i].c) return false;
                }
            }
            return true;
        }
    }

    class myComparer : IComparer<BoardState>
    {
        public int Compare(BoardState x, BoardState y)
        {
            if ((float)x.Fn > (float)y.Fn) return 1;
            else if (x.Fn < y.Fn) return -1;
            else return 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Resource load
        pre_grid = Resources.Load("Prefabs/Grid");
        pre_cat = Resources.Load("Prefabs/Cat");
        pre_mouse = Resources.Load("Prefabs/Mouse");
        pre_chess = Resources.Load("Prefabs/Cheese");

        // record initializer
        initializer = GameObject.Find("Initializer");
        notification = GameObject.Find("Notification");

        //
        gameState = GameState.Uninitial;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPath() {
        notification.GetComponent<Notification>().ChangeContent("It's "+currentPlayer.ToString()+"'s turn");
        ShowAvaiablePos();
        choosenNode = NextStep();
        HighlightTarge();
        RotateToTarget();
        playerBehavior = PlayerBehavior.MoveToTarget;

    }

    private List<GameObject> NextStep()
    {
        int r, c;
        List<GameObject> result = new List<GameObject>();
        if (currentPlayer == Players.Cat)
        {
            for (int i = 0; i < cat.Count; i++)
            {
                r = cat[i].GetComponent<Cat>().path[0].r;
                c = cat[i].GetComponent<Cat>().path[0].c;
                cat[i].GetComponent<Cat>().path.Remove(cat[i].GetComponent<Cat>().path[0]);
                result.Add(board[r, c]);
            }
        }
        else  {
            for (int i = 0; i < mouse.Count; i++) {

                Pair pos = new Pair();
                pos.r = mouse[i].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().row;
                pos.c = mouse[i].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().column;
                List<Pair> cheeses = new List<Pair>();
                for (int j = 0; j < cheese.Count; j++) {
                    int y = cheese[j].GetComponent<Cheese>().currentGrid.GetComponent<Grid>().row;
                    int x = cheese[j].GetComponent<Cheese>().currentGrid.GetComponent<Grid>().column;

                    cheeses.Add(new Pair { r = y, c = x });
                }
                Pair mouseSetp = Mouse.NextStep(pos,cheeses,board.GetLength(1),board.GetLength(0));
                result.Add(board[mouseSetp.r, mouseSetp.c]);
            }

        }
        return result;
    }

    public void MoveToTarget()
    {
        UpdateBoard();
        ClearBoardColor();

        if (mouse.Count<=0) {
            notification.GetComponent<Notification>().ChangeContent("All Mice Are Caught by Cats", Color.red);
            gameState = GameState.EndGame;
        }

        else if (cheese.Count<=0) {
            notification.GetComponent<Notification>().ChangeContent("Mice Eat all Cheeses", Color.blue);
            gameState = GameState.EndGame;
        }
            
        else notification.GetComponent<Notification>().ChangeContent("Game Continues",Color.white);

        SwitchTurn();

    }

    private void ShowAvaiablePos() {
        if (currentPlayer == Players.Cat) {
            for (int j = 0; j < cat.Count; j++) {
                Pair pos = new Pair();
                pos.r = cat[j].GetComponent<Cat>().currentGrid.GetComponent<Grid>().row;
                pos.c = cat[j].GetComponent<Cat>().currentGrid.GetComponent<Grid>().column;
                List<Pair> can = Cat.FindCandidates(pos,board);

                for (int i = 0; i < can.Count; i++)
                {
                    board[can[i].r, can[i].c].GetComponent<MeshRenderer>().material.color = new Color(0.3f, 0.1f, 0.1f, 0.3f);
                }
            }


        }
        else if (currentPlayer == Players.Mouse){
            for (int j = 0; j < mouse.Count; j++) {
                Pair pos = new Pair();
                pos.r = mouse[j].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().row;
                pos.c = mouse[j].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().column;
                List<Pair> can = Mouse.FindCandidates(pos, board.GetLength(1), board.GetLength(0));

                for (int i = 0; i < can.Count; i++)
                {
                    board[can[i].r, can[i].c].GetComponent<MeshRenderer>().material.color = new Color(0.1f, 0.1f, 0.3f, 0.3f);
                }
            }

        }
    }

    public void PathFinding() {

        GameObject calculationTime = GameObject.Find("CTime");
        List<PairWithIndex> mice = new List<PairWithIndex>();
        for (int i = 0; i < mouse.Count; i++) {
            int row = mouse[i].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().row;
            int column = mouse[i].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().column;
            Pair p = new Pair
            {
                r = row,
                c = column
            };
            mice.Add(new PairWithIndex(p, i));
        }

        List<PairWithIndex> cats = new List<PairWithIndex>();
        for (int i = 0; i < cat.Count; i++)
        {
            int row = cat[i].GetComponent<Cat>().currentGrid.GetComponent<Grid>().row;
            int column = cat[i].GetComponent<Cat>().currentGrid.GetComponent<Grid>().column;
            Pair p = new Pair
            {
                r = row,
                c = column
            };
            cats.Add(new PairWithIndex(p, i));
        }

        List<Pair> cheeses = new List<Pair>();
        for (int i = 0; i < cheese.Count; i++)
        {
            int row = cheese[i].GetComponent<Cheese>().currentGrid.GetComponent<Grid>().row;
            int column = cheese[i].GetComponent<Cheese>().currentGrid.GetComponent<Grid>().column;
            Pair p = new Pair
            {
                r = row,
                c = column
            };
            cheeses.Add(p);
        }
        List<List<Pair>> micePath;
        List<List<Pair>> catsPath;

        int searchTimes;
        if (searchRule == SearchRule.IDDFS)
        {
            searchTimes = IDDFS(cats, mice, cheeses, out catsPath, out micePath, searchRule);
        }
        else {
            searchTimes = Searching(cats, mice, cheeses, out catsPath, out micePath, searchRule);
        }

        calculationTime.GetComponent<UnityEngine.UI.Text>().text = "Searching Record:" + searchTimes + " times";

        if (catsPath.Count > 0)
        {
            for (int i = 0; i < cats.Count; i++)
            {
                cat[i].GetComponent<Cat>().path = catsPath[i];
            }
            gameState = GameState.InGame;
        }
        else
        {
            gameState = GameState.EndGame;
            notification.GetComponent<Notification>().ChangeContent("Cats can't catch all mice");
        }
    }

    private int IDDFS(List<PairWithIndex> cats, List<PairWithIndex> mice, List<Pair> cheeses, out List<List<Pair>> catsPath, out List<List<Pair>> micePath, SearchRule searchRule)
    {
        // initialize
        int count = 0;

        BoardState current = new BoardState(null, new List<BoardState>(), cats, mice, cheeses);
        current.Gn = 0;
        current.Hn = 0;

        micePath = new List<List<Pair>>();
        catsPath = new List<List<Pair>>();
        int limit = 0;
        int maxDepth = -1;
        while (catsPath.Count == 0)
        {
            if (maxDepth >= 0) {
                break;
            }
            count += DLS(current, cheeses, catsPath, micePath, limit,out maxDepth);
            limit++;
        }

        return count;
    }

    private int DLS(BoardState current, List<Pair> cheeses, List<List<Pair>> catsPath, List<List<Pair>> micePath, int limit, out int maxDepth)
    {
        // initialize
        int count = 0;
        int height = board.GetLength(0);
        int width = board.GetLength(1);
        List<BoardState> searchList = new List<BoardState>();
        searchList.Add(current);
        BoardState checkBoard;
        maxDepth = -1;
        while (searchList.Count > 0)
        {
            count++;

            checkBoard = searchList[0];
            searchList.Remove(searchList[0]);

            // check cheeses
            List<Pair> currentCheeses;
            int cheesesNum = CheckCheeses(checkBoard.mice, checkBoard.cheeses, out currentCheeses);
            if (cheesesNum == 0) {
                maxDepth = limit;
                continue;
            }


            // check mice
            List<PairWithIndex> currentMice;
            int miceNum = CheckMice(checkBoard.mice, checkBoard.cats, checkBoard, out currentMice, micePath);
            if (miceNum == 0)
            {
                for (int i = 0; i < checkBoard.cats.Count; i++)
                {
                    List<Pair> catPath = new List<Pair>();
                    BoardState temp = checkBoard;
                    int index = checkBoard.cats[i].index;
                    while (temp.parent != null)
                    {
                        for (int k = 0; k < temp.cats.Count; k++)
                        {
                            if (temp.cats[k].index == index)
                            {
                                catPath.Insert(0, temp.cats[k].p);
                                break;
                            }
                        }
                        temp = temp.parent;
                    }
                    catsPath.Add(catPath);
                }
                Debug.Log("Solution is found in depth of " + checkBoard.Gn);
                break;
            }

            // mice move, only one possibility
            List<PairWithIndex> miceMove = new List<PairWithIndex>();
            for (int i = 0; i < currentMice.Count; i++)
            {
                // mice's next step
                Pair p = Mouse.NextStep(currentMice[i].p, currentCheeses, width, height);
                int index = currentMice[i].index;
                PairWithIndex nextStep = new PairWithIndex(p, index);
                miceMove.Add(nextStep);
            }

            // cats move, expand all possible states
            List<List<PairWithIndex>> catsCandidateSet = new List<List<PairWithIndex>>();
            for (int i = 0; i < checkBoard.cats.Count; i++)
            {
                List<PairWithIndex> oneCatMove = new List<PairWithIndex>();
                List<Pair> candidates = Cat.FindCandidates(checkBoard.cats[i].p, board);
                for (int j = 0; j < candidates.Count; j++)
                {
                    Pair p = candidates[j];
                    int index = checkBoard.cats[i].index;
                    oneCatMove.Add(new PairWithIndex(p, index));
                }
                catsCandidateSet.Add(oneCatMove);
            }

            List<List<PairWithIndex>> catsMove = new List<List<PairWithIndex>>();
            Expand(catsMove, catsCandidateSet, new List<PairWithIndex>(), 0);

            for (int i = 0; i < catsMove.Count; i++)
            {
                BoardState next = new BoardState(checkBoard, null, catsMove[i], miceMove, currentCheeses);
                next.Gn = checkBoard.Gn + 1;
                bool flag = false;
                for (int x = 0; x < searchList.Count; x++)
                {
                    if (searchList[x].Equals(next))
                    {
                        flag = true;
                        break;
                    }

                }

                if (!flag)
                {
                    searchList.Add(next);
                }
            }

        }
        return count;
    }

    private int Searching(List<PairWithIndex> cats,
                             List<PairWithIndex> mice, 
                             List<Pair> cheeses,
                             out List<List<Pair>> catsPath,
                             out List<List<Pair>> micePath,
                             SearchRule rule)
    {
        // initialize
        int count = 0;
        int height = board.GetLength(0);
        int width = board.GetLength(1);
        BoardState current = new BoardState(null, new List<BoardState>(), cats, mice, cheeses);
        current.Gn = 0;
        current.Hn = 0;
        List<BoardState> searchList = new List<BoardState>();
        searchList.Add(current);
        BoardState checkBoard;
        micePath = new List<List<Pair>>();
        catsPath = new List<List<Pair>>();

        while (searchList.Count > 0) {
            count++;

            if (searchRule == SearchRule.AStar1 || searchRule == SearchRule.AStar2 || searchRule == SearchRule.AStar3)
                searchList.Sort(new myComparer());

            checkBoard = searchList[0];
            searchList.Remove(searchList[0]);

            // check cheeses
            List<Pair> currentCheeses;
            int cheesesNum = CheckCheeses(checkBoard.mice, checkBoard.cheeses, out currentCheeses);
            if (cheesesNum == 0) continue;

            // check mice
            List<PairWithIndex> currentMice;
            int miceNum = CheckMice(checkBoard.mice, checkBoard.cats, checkBoard, out currentMice,micePath);
            if (miceNum == 0) {
                for (int i = 0; i < checkBoard.cats.Count; i++) {
                    List<Pair> catPath = new List<Pair>();
                    BoardState temp = checkBoard;
                    int index = checkBoard.cats[i].index;
                    while (temp.parent != null)
                    {
                        for (int k = 0; k < temp.cats.Count; k++)
                        {
                            if (temp.cats[k].index == index)
                            {
                                catPath.Insert(0, temp.cats[k].p);
                                break;
                            }
                        }
                        temp = temp.parent;
                    }
                    catsPath.Add(catPath);
                }
                break;
            }

            // mice move, only one possibility
            List<PairWithIndex> miceMove = new List<PairWithIndex>();
            for (int i = 0; i < currentMice.Count; i++) {
                // mice's next step
                Pair p = Mouse.NextStep(currentMice[i].p, currentCheeses,width,height);
                int index = currentMice[i].index;
                PairWithIndex nextStep = new PairWithIndex(p,index);
                miceMove.Add(nextStep);
            }

            // cats move, expand all possible states
            List<List<PairWithIndex>> catsCandidateSet = new List<List<PairWithIndex>>();
            for (int i = 0; i < checkBoard.cats.Count; i++) {
                List<PairWithIndex> oneCatMove = new List<PairWithIndex>();
                List<Pair> candidates = Cat.FindCandidates(checkBoard.cats[i].p,board);
                for (int j = 0; j < candidates.Count; j++) {
                    Pair p = candidates[j];
                    int index = checkBoard.cats[i].index;
                    oneCatMove.Add(new PairWithIndex(p, index));
                }
                catsCandidateSet.Add(oneCatMove);
            }

            List<List<PairWithIndex>> catsMove = new List<List<PairWithIndex>>();
            Expand(catsMove, catsCandidateSet,new List<PairWithIndex>(),0);

            switch (searchRule)
            {
                case SearchRule.DFS:
                    DFS(searchList, checkBoard, catsMove, miceMove, currentCheeses);
                    break;
                case SearchRule.BFS:
                    BFS(searchList, checkBoard, catsMove, miceMove, currentCheeses);
                    break;
                case SearchRule.AStar1:
                    AStar1(searchList, checkBoard, catsMove, miceMove, currentCheeses);
                    break;
                case SearchRule.AStar2:
                    AStar2(searchList, checkBoard, catsMove, miceMove, currentCheeses);
                    break;
                case SearchRule.AStar3:
                    AStar3(searchList, checkBoard, catsMove, miceMove, currentCheeses);
                    break;
            }
                                   
        }
        return count;
    }

    private void AStar3(List<BoardState> searchList, BoardState checkBoard, List<List<PairWithIndex>> catsMove, List<PairWithIndex> miceMove, List<Pair> currentCheeses)
    {
        for (int i = 0; i < catsMove.Count; i++)
        {
            BoardState next = new BoardState(checkBoard, new List<BoardState>(), catsMove[i], miceMove, currentCheeses);
            next.Gn = checkBoard.Gn + 1;
            next.Hn = SumOfMinDistance(catsMove[i], miceMove) - currentCheeses.Count;

            bool flag = false;
            for (int x = 0; x < searchList.Count; x++) {
                if (searchList[x].Equals(next))
                {
                    flag = true;
                    break;
                }

            }

            if (!flag)
            {
                checkBoard.children.Add(next);
                searchList.Add(next);
            }
            else
                if (next.Fn < FindBoardState(next, searchList).Fn)
                ChangeParent(FindBoardState(next, searchList), checkBoard);
        }
    }

    private void AStar2(List<BoardState> searchList, BoardState checkBoard, List<List<PairWithIndex>> catsMove, List<PairWithIndex> miceMove, List<Pair> currentCheeses)
    {
        for (int i = 0; i < catsMove.Count; i++)
        {
            BoardState next = new BoardState(checkBoard, new List<BoardState>(), catsMove[i], miceMove, currentCheeses);
            next.Gn = checkBoard.Gn + 1;
            next.Hn = -currentCheeses.Count;
            bool flag = false;
            for (int x = 0; x < searchList.Count; x++)
            {
                if (searchList[x].Equals(next))
                {
                    flag = true;
                    break;
                }

            }

            if (!flag)
            {
                checkBoard.children.Add(next);
                searchList.Add(next);
            }
            else
                if (next.Fn < FindBoardState(next, searchList).Fn)
                ChangeParent(FindBoardState(next, searchList), checkBoard);
        }
    }

    private void AStar1(List<BoardState> searchList, BoardState checkBoard, List<List<PairWithIndex>> catsMove, List<PairWithIndex> miceMove, List<Pair> currentCheeses)
    {
        for (int i = 0; i < catsMove.Count; i++)
        {
            BoardState next = new BoardState(checkBoard, new List<BoardState>(), catsMove[i], miceMove, currentCheeses);
            next.Gn = checkBoard.Gn + 1;
            next.Hn = SumOfMinDistance(catsMove[i], miceMove);
            bool flag = false;
            for (int x = 0; x < searchList.Count; x++)
            {
                if (searchList[x].Equals(next))
                {
                    flag = true;
                    break;
                }

            }

            if (!flag)
            {
                checkBoard.children.Add(next);
                searchList.Add(next);
            }
            else
                if(next.Fn< FindBoardState(next, searchList).Fn)
                    ChangeParent(FindBoardState(next, searchList), checkBoard);
        }
    }
    private BoardState FindBoardState(BoardState board,List<BoardState> l) {
        for (int i = 0; i < l.Count; i++) {
            if (board.Equals(l[i])) {
                return l[i];
            }
        }
        return null;
    }

    private void ChangeParent(BoardState board, BoardState parent)
    {
        board.parent = parent;
        board.Gn = parent.Gn + 1;
        UpdateChildren(board);

    }

    private void UpdateChildren(BoardState b)
    {
        for (int i = 0; i < b.children.Count; i++)
        {
            b.children[i].Gn = b.Gn + 1;
            UpdateChildren(b.children[i]);
        }
    }

    private float SumOfMinDistance(List<PairWithIndex> cats, List<PairWithIndex> mice) {
        float sum = 0;

        for (int i = 0; i < cats.Count; i++) {
            float minDis = 10000000;
            for (int j = 0; j < mice.Count; j++) {
                if (Distance(cats[i].p, mice[j].p) < minDis)
                    minDis = Distance(cats[i].p, mice[j].p);
            }
            sum += minDis;
        }
        return sum;
    }

    public static float Distance(Pair p1, Pair p2)
    {
        return Mathf.Sqrt(Mathf.Pow(p1.r - p2.r, 2) + Mathf.Pow(p1.c - p2.c, 2));
    }


    private void BFS(List<BoardState> searchList, BoardState checkBoard, List<List<PairWithIndex>> catsMove, List<PairWithIndex> miceMove, List<Pair> currentCheeses)
    {
        for (int i = 0; i < catsMove.Count; i++)
        {
            BoardState next = new BoardState(checkBoard, null, catsMove[i], miceMove, currentCheeses);
            bool flag = false;
            for (int x = 0; x < searchList.Count; x++)
            {
                if (searchList[x].Equals(next))
                {
                    flag = true;
                    break;
                }

            }

            if (!flag)
            {
                searchList.Add(next);
            }
        }
    }

    private void DFS(List<BoardState> searchList, BoardState checkBoard, List<List<PairWithIndex>> catsMove, List<PairWithIndex> miceMove, List<Pair> currentCheeses)
    {
        for (int i = 0; i < catsMove.Count; i++)
        {
            BoardState next = new BoardState(checkBoard, null, catsMove[i], miceMove, currentCheeses);
            bool flag = false;
            for (int x = 0; x < searchList.Count; x++)
            {
                if (searchList[x].Equals(next))
                {
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                searchList.Add(next);
            }
        }
    }

    private void Expand(List<List<PairWithIndex>> catsMove, List<List<PairWithIndex>> catsCandidateSet, List<PairWithIndex> combination, int n)
    {
        if (n < catsCandidateSet.Count)
        {
            for (int i = 0; i < catsCandidateSet[n].Count; i++) {
                List<PairWithIndex> newCombination = new List<PairWithIndex>();
                for (int j = 0; j < combination.Count; j++) {
                    newCombination.Add(combination[j]);
                }
                newCombination.Add(catsCandidateSet[n][i]);
                Expand(catsMove, catsCandidateSet, newCombination, n + 1);
            }
        }
        else {
            catsMove.Add(combination);
        }
    }

    private int CheckMice(List<PairWithIndex> mice, List<PairWithIndex> cats, BoardState current, out List<PairWithIndex> currentMice, List<List<Pair>>micePath)
    {
        currentMice = new List<PairWithIndex>();
        for (int i = 0; i < mice.Count; i++)
        {
            bool isCatched = false;
            for (int j = 0; j < cats.Count; j++)
            {
                if (mice[i].p.r == cats[j].p.r && mice[i].p.c == cats[j].p.c)
                {
                    isCatched = true;
                    List<Pair> mousePath = new List<Pair>();
                    BoardState temp = current;
                    int index = mice[i].index;
                    while (temp.parent != null) {
                        for (int k = 0; k < temp.mice.Count; k++) {
                            if (temp.mice[k].index == index) {
                                mousePath.Insert(0, temp.mice[k].p);
                                break;
                            }
                        }
                        temp = temp.parent;
                    }
                    micePath.Add(mousePath);
                    break;
                }
            }
            if (isCatched) continue;
            else currentMice.Add(mice[i]);
        }
        return currentMice.Count;
    }

    private int CheckCheeses(List<PairWithIndex> mice, List<Pair> cheeses, out List<Pair> currentCheeses)
    {
        currentCheeses = new List<Pair>();
        for (int i = 0; i < cheeses.Count; i++) {
            bool isEaten = false;
            for (int j = 0; j < mice.Count; j++) {
                if (cheeses[i].r == mice[j].p.r && cheeses[i].c == mice[j].p.c) {
                    isEaten = true;
                    break;
                }
            }
            if (isEaten) continue;
            else currentCheeses.Add(cheeses[i]);
        }
        return currentCheeses.Count;
    }

    

    private void RotateToTarget() {
        if (currentPlayer == Players.Cat)
        {
            for (int i = 0; i < cat.Count; i++)
            {
                cat[i].transform.LookAt(new Vector3((choosenNode[i].GetComponent<Grid>().row - board.GetLength(0) / (float)2) * 10 + 5,
                        0f,
                      (choosenNode[i].GetComponent<Grid>().column - board.GetLength(1) / (float)2) * 10 + 5),
                      new Vector3(-1, 0, 1).normalized);
                cat[i].transform.RotateAround(cat[i].transform.position, new Vector3(0, 1, 0), 90);
            }


        }

        else
            for (int i = 0; i < mouse.Count; i++) {
                mouse[i].transform.LookAt(new Vector3((choosenNode[i].GetComponent<Grid>().row - board.GetLength(0) / (float)2) * 10 + 5,
                                    0f,
                                  (choosenNode[i].GetComponent<Grid>().column - board.GetLength(1) / (float)2) * 10 + 5));
            }

    }

    private void HighlightTarge() {
        if (currentPlayer == Players.Cat)
            for (int i = 0; i < cat.Count; i++)
            {
                choosenNode[i].GetComponent<MeshRenderer>().material.color = new Color(0.9f, 0.1f, 0.1f, 0.8f);
            }

        else {  
            for (int i = 0; i < mouse.Count; i++)
            {
                choosenNode[i].GetComponent<MeshRenderer>().material.color = new Color(0.1f, 0.1f, 0.9f, 0.8f);
            }
        }
    }

    private void ClearBoardColor() {

        for (int i = 0; i < board.GetLength(0); i++) {
            for (int j = 0; j < board.GetLength(1); j++) {
                board[i, j].GetComponent<MeshRenderer>().material = Resources.Load("grid_material") as Material;
            }            

        }
    }

    private void UpdateBoard() {

        // set attributes for all unities
        for (int i = choosenNode.Count-1; i >=0 ; i--)
        {
            int nextR = choosenNode[i].GetComponent<Grid>().row;
            int nextC = choosenNode[i].GetComponent<Grid>().column;
            if (currentPlayer == Players.Cat)
            {
                for (int j = mouse.Count - 1; j >= 0; j--)
                {
                    if (mouse[j].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().row == nextR && 
                        mouse[j].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().column == nextC)
                    {
                        mouse[j].GetComponent<Mouse>().shouldBeRemove = true;
                    }
                    cat[i].GetComponent<Cat>().currentGrid.GetComponent<Grid>().row = nextR;
                    cat[i].GetComponent<Cat>().currentGrid.GetComponent<Grid>().column = nextC;
                    cat[i].transform.position = board[nextR, nextC].transform.position;
                }
            }
            else {
                bool isCatched = false;
                for (int j = cat.Count - 1; j >= 0; j--)
                {
                    if (cat[j].GetComponent<Cat>().currentGrid.GetComponent<Grid>().row == nextR && 
                        cat[j].GetComponent<Cat>().currentGrid.GetComponent<Grid>().column == nextC)
                    {
                        mouse[i].GetComponent<Mouse>().shouldBeRemove = true;
                        isCatched = true;
                        break;
                    }
                }
                if (isCatched) continue;
                for (int j = cheese.Count - 1; j >= 0; j--) {
                    if (cheese[j].GetComponent<Cheese>().currentGrid.GetComponent<Grid>().row == nextR && 
                        cheese[j].GetComponent<Cheese>().currentGrid.GetComponent<Grid>().column == nextC)
                    {
                        cheese[j].GetComponent<Cheese>().shouldBeRemove = true;
                        break;
                    }
                }
                mouse[i].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().row = nextR;
                mouse[i].GetComponent<Mouse>().currentGrid.GetComponent<Grid>().column = nextC;
                mouse[i].transform.position = board[nextR, nextC].transform.position;
            }
        }

        // remove destoried unities
        for (int i = mouse .Count - 1; i >= 0; i--)
        {
            if (mouse[i].GetComponent<Mouse>().shouldBeRemove)
            {
                GameObject.Destroy(mouse[i]);
                mouse.Remove(mouse[i]);
            }
        }
        for (int i = cheese.Count - 1; i >= 0; i--)
        {
            if (cheese[i].GetComponent<Cheese>().shouldBeRemove)
            {
                GameObject.Destroy(cheese[i]);
                cheese.Remove(cheese[i]);
            }
        }

    }


    private void SwitchTurn() {
        currentPlayer = currentPlayer == Players.Cat ? Players.Mouse : Players.Cat;
        playerBehavior = PlayerBehavior.ShowPath;
    }

}
