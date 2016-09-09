using UnityEngine;
using System.Collections.Generic;
using SharedSpace;

public class TSnake : MonoBehaviour
{
    public class TBody
    {
        public Vector3 Position;
        public int NextDirection;
        public int CurDirection;
        public Transform transform;
        public int WidthPos;
        public int LengthPos;

        public TBody(Vector3 _pos, byte _dir, Transform _trans)
        {
            Position = _pos;
            NextDirection = CurDirection = _dir;
            transform = _trans;
        }
    }

    //0 - turn left, 2 - turn right, 1 - move
    public byte MoveState = 1;
    public TMap GameMap;
    public float Speed = 0.5f;
    public MainSrc Main;

    //0 - forward, 1 - right, 2 - back, 3 - left
    public byte Direction = 0;

    public int WidthPos = 2;
    public int LengthPos = 2;
    public bool ChackCollider = true;

    public List<TBody> Body = new List<TBody>();

    private Vector3 NextPos = Vector3.zero;
    private Vector3 NextPartPos = Vector3.zero;
    bool MoveBody = false;

    // Use this for initialization
    void Start()
    {
        if (GameMap != null) NextPos = GameMap.GetPos(WidthPos, LengthPos);
        transform.localPosition = NextPos;
    }

    public void AddNewPart(bool _new, int _id = -1)
    {
        GameObject Snake;
        if (_new)
        {
            Snake = (GameObject)Object.Instantiate(Resources.Load("Prefabs/Snake" + (Body.Count == 0 ? "Finish" : "Body")));
            Snake.transform.SetParent(transform.parent);
            Snake.transform.localScale = Vector3.one;
            Snake.name = "Part" + SharedData.BodyCount;
            SharedData.BodyCount++;
            Snake.transform.rotation = transform.rotation;
            Snake.transform.localPosition = transform.localPosition;
            Snake.transform.GetChild(0).localRotation = transform.GetChild(0).localRotation;

            MoveBody = false;
            Body.Add(new TBody(transform.localPosition, Direction, Snake.transform));
        } else
        {
            Snake = (GameObject)Object.Instantiate(Resources.Load("Prefabs/SnakeFinish"));
            Snake.transform.SetParent(transform.parent);
            Snake.transform.localScale = Vector3.one;
            Snake.name = "Part" + _id;
            Snake.transform.GetChild(0).localRotation = Body[_id].transform.GetChild(0).localRotation;
            Snake.transform.localPosition = Body[_id].transform.localPosition;

            GameObject.Destroy(Body[_id].transform.gameObject);

            Body[_id].transform = Snake.transform;
        }
    }

    private Vector2 MouseDownPos;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            AddNewPart(true);
        }

        if ((NextPos - transform.localPosition).sqrMagnitude > 0.001f)
        {
            transform.localPosition += (NextPos - transform.localPosition).normalized * Speed * Time.deltaTime;
            if (MoveBody)
                for (int _part = Body.Count - 1; _part >= 0; _part--)
                {
                    Body[_part].transform.localPosition += (Body[_part].Position - Body[_part].transform.localPosition).normalized * Speed * Time.deltaTime;

                    int _turn = 1;
                    if (Body[_part].CurDirection > Body[_part].NextDirection) _turn = -1;
                    if (Body[_part].CurDirection == 0 && Body[_part].NextDirection == 3) _turn = -1;
                    if (Body[_part].CurDirection == 3 && Body[_part].NextDirection == 0) _turn = 1;
                    if (Body[_part].CurDirection == Body[_part].NextDirection) _turn = 0;

                    Body[_part].transform.GetChild(0).RotateAround(Body[_part].transform.position, transform.up, _turn * 90);
                    Body[_part].CurDirection = Body[_part].NextDirection;
                }
        }
        else
        {
            if (Body.Count > 0)
            {
                for (int _part = 0; _part < Body.Count - 1; _part++)
                {
                    Body[_part].WidthPos = WidthPos;
                    Body[_part].LengthPos = LengthPos;
                    Body[_part].Position = Body[_part + 1].Position;
                    Body[_part].NextDirection = Body[_part + 1].CurDirection;
                }

                Body[Body.Count - 1].Position = transform.localPosition;
                Body[Body.Count - 1].NextDirection = Direction;
            }
            if (MoveState == 0)
            {
                Direction = Direction > 0 ? (byte)(Direction - 1) : (byte)3;
                transform.GetChild(0).RotateAround(transform.position, transform.up, -90);
            }
            if (MoveState == 2)
            {
                Direction = Direction < 3 ? (byte)(Direction + 1) : (byte)0;
                transform.GetChild(0).RotateAround(transform.position, transform.up, 90);
            }
            MoveState = 1;
            MoveBody = true;
            ChackCollider = true;

            if (Direction == 2) WidthPos--;
            if (Direction == 1) LengthPos--;
            if (Direction == 3) LengthPos++;
            if (Direction == 0) WidthPos++;

            WidthPos = Mathf.Clamp(WidthPos, 0, GameMap.MapWidth - 1);
            LengthPos = Mathf.Clamp(LengthPos, 0, GameMap.MapLength - 1);

            if (GameMap != null) NextPos = GameMap.GetPos(WidthPos, LengthPos);
        }
    }

    public void Die()
    {
        for (int _part = Body.Count - 1; _part >= 0; _part--)
            GameObject.Destroy(Body[_part].transform.gameObject);
        Body.Clear();
        GameObject.Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ChackCollider) return;

        if (other.tag == "Apple")
        {
            GameMap.EatApple(WidthPos, LengthPos);
            AddNewPart(true);
        }

        if (other.tag == "Wall" || (other.tag == "Corner" && Body.Count > 3)) Main.SnakeReport(this);

        if (other.tag == "Body") {
            int _part;
            for (_part = 1; _part < Body.Count - 3; _part++)
                if (Body[_part].transform.gameObject.name == other.gameObject.transform.parent.gameObject.name) {
                    Main.SnakeReport(this, _part);
                    ChackCollider = false;
                    break;
                }
        }
    }
}