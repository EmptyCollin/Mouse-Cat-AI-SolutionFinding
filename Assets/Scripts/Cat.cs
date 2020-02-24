using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat : MonoBehaviour
{
    public GameObject currentGrid;
    public List<GameController.Pair> path;
    public bool shouldBeRemove;
    private GameObject gc;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
        gc = GameObject.Find("Controller");
        shouldBeRemove = false;
    }

    public class Board: IEquatable<Board>
    {
        public int turns;
        public Board parent;
        public List<Board> children;
        public List<GameController.Pair> cheeses;
        public GameController.Pair mouse;
        public GameController.Pair cat;

        public enum State { normal,// waiting for check
            open,
            close }

        //  Fn, Hn, Gn and BoardState are only used for A*
        private float fn = 0, hn = 0, gn = 0;
        public float Gn {
            get { return hn; }
            set { gn = value; fn = gn + hn; }
        }
        public float Hn {
            get { return hn; }
            set { hn = value; fn = gn + hn; }
        }
        public float Fn {
            get { return fn; }
        }
        public State BoardState = State.normal;

        public Board() {
            turns = 0;
            parent = null;
            children = new List<Board>();
            cheeses = new List<GameController.Pair>();
            mouse = new GameController.Pair();
            cat = new GameController.Pair();
        }

        public Board(int t, Board p, List<Board> c, List<GameController.Pair> ch, GameController.Pair m, GameController.Pair ca) {
            turns = t;
            parent = p;
            children = c;
            cheeses = ch;
            mouse = m;
            cat = ca;
        }
        public bool Equals(Board o) {
            if (o.turns != turns) return false;
            if (o.cheeses.Count != cheeses.Count) return false;
            if (o.cat.r != cat.r || o.cat.c != cat.c || o.mouse.r != mouse.r || o.mouse.c != mouse.c) return false;
            return true;
        }
    }

    class myComparer : IComparer<Board>
    {
        public int Compare(Board x, Board y)
        {
            if ((float)x.Fn > (float)y.Fn) return 1;
            else if (x.Fn < y.Fn) return -1;
            else return 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
    private int AstarSearch(Board current, GameController.SearchRule rule, List<GameController.Pair> mousePath) {
        List<Board> openList = new List<Board>();
        List<Board> closeList = new List<Board>();
        Board checkBoard;
        current.Gn = 0;
        current.Hn = HnForAStar(current,rule);

        openList.Add(current);

        int p = 0;
        while (openList.Count > 0)
        {
            p++;
            openList.Sort(new myComparer());
            checkBoard = openList[0];
            openList.Remove(openList[0]);
            closeList.Add(checkBoard);

            // check cheeses
            List<GameController.Pair> current_cheeses = new List<GameController.Pair>();
            for (int i = 0; i < checkBoard.cheeses.Count; i++)
            {
                if (checkBoard.mouse.r == checkBoard.cheeses[i].r && checkBoard.mouse.c == checkBoard.cheeses[i].c)
                {
                    continue;
                }
                else
                {
                    current_cheeses.Add(checkBoard.cheeses[i]);
                }
            }
            if (current_cheeses.Count <= 0)
            {
                continue;
            }
            // check catching
            if (checkBoard.cat.r == checkBoard.mouse.r && checkBoard.cat.c == checkBoard.mouse.c)
            {
                Board temp = checkBoard;
                while (temp != null)
                {
                    path.Insert(0, temp.cat);
                    temp = temp.parent;
                }
                path.Remove(path[0]);
                break;
            }

            List<GameController.Pair> candidates = FindCandidates(checkBoard.cat);
            for (int i = 0; i < candidates.Count; i++)
            {
                Board next = new Board(checkBoard.turns + 1, checkBoard, new List<Board> (), current_cheeses, mousePath[checkBoard.turns], candidates[i]);
                next.Hn = HnForAStar(next, rule);
                next.Gn = next.turns;

                Board findBoard = FindBoard(openList, next);
                if (findBoard != null)
                {
                    if (findBoard.Gn > next.Gn) {
                        ChangeParent(findBoard, next);
                    }
                }
                else {
                    findBoard = FindBoard(closeList, next);
                    if (FindBoard(closeList, next)!=null)
                    {
                        if (findBoard.Gn > next.Gn)
                        {
                            ChangeParent(findBoard, next);
                        }
                    }
                    else
                    {
                        checkBoard.children.Add(next);
                        openList.Add(next);
                    }
                }
            }
        }
        return p;
    }
    */

    public static List<GameController.Pair> FindCandidates(GameController.Pair pos, GameObject[,] board)
    {
        int width = board.GetLength(1);
        int height = board.GetLength(0);
        List<GameController.Pair> candidates = new List<GameController.Pair>();
        GameController.Pair p;

        int r = pos.r;
        int c = pos.c;

        p = new GameController.Pair();
        p.r = r - 2;
        p.c = c - 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r - 2;
        p.c = c + 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r - 1;
        p.c = c + 2;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r + 1;
        p.c = c + 2;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r + 2;
        p.c = c + 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r + 2;
        p.c = c - 1;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r + 1;
        p.c = c - 2;
        candidates.Add(p);

        p = new GameController.Pair();
        p.r = r - 1;
        p.c = c - 2;
        candidates.Add(p);

        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            if (candidates[i].r < 0 || candidates[i].r >= height || candidates[i].c < 0 || candidates[i].c >= width)
                candidates.Remove(candidates[i]);
        }
        return candidates;
    }
}
