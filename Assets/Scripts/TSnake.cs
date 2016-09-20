using UnityEngine;
using System.Collections.Generic;
using SharedSpace;

public class TSnake : MonoBehaviour
{
    /// <summary>
    /// Класс частей тела змеи
    /// </summary>
    public class TBody
    {
        /// <summary>
        /// Позиция следования
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Следующее направление взгляда части тела
        /// </summary>
        public int NextDirection;
        /// <summary>
        /// Текущее направление взгляда части тела
        /// </summary>
        public int CurDirection;
        /// <summary>
        /// Ссылка на объект тела
        /// </summary>
        public Transform transform;

        /// <summary>
        /// Инициализация части тела 
        /// </summary>
        public TBody(Vector3 _pos, byte _dir, Transform _trans)
        {
            Position = _pos;
            NextDirection = CurDirection = _dir;
            transform = _trans;
        }
    }

    //0 - turn left, 2 - turn right, 1 - move
    /// <summary>
    /// Триггер указывающий на необходимость поворота змеи
    /// </summary>
    public byte MoveState = 1;
    /// <summary>
    /// Ссылка на карту уровня
    /// </summary>
    public TMap GameMap;
    /// <summary>
    /// Скорость змеи
    /// </summary>
    public float Speed;
    /// <summary>
    /// Ссылка на главный класс, для общения змей между собой
    /// </summary>
    public MainSrc Main;
    //0 - forward, 1 - right, 2 - back, 3 - left
    /// <summary>
    /// Направление движения змеи
    /// </summary>
    public byte Direction = 0;
    //Позиция головы змеи на карте
    public int WidthPos;
    public int LengthPos;
    /// <summary>
    /// Триггер контролирующий проверку столкновений
    /// </summary>
    public bool ChackCollider = true;
    /// <summary>
    /// Список хранящий куски тела змеи
    /// </summary>
    public List<TBody> Body = new List<TBody>();
    /// <summary>
    /// Позиция следования головы змеи
    /// </summary>
    public Vector3 NextPos = Vector3.zero;
    /// <summary>
    /// Триггер следования частей тела за головой
    /// </summary>
    bool MoveBody = false;
    /// <summary>
    /// Метод замены или добавления части тела змее
    /// </summary>
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
        }
        else
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

    void Update()
    {
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
            //Обновление информации частям тела змеи о пути следования
            if (Body.Count > 0)
            {
                for (int _part = 0; _part < Body.Count - 1; _part++)
                {
                    Body[_part].Position = Body[_part + 1].Position;
                    Body[_part].NextDirection = Body[_part + 1].CurDirection;
                }

                Body[Body.Count - 1].Position = transform.localPosition;
                Body[Body.Count - 1].NextDirection = Direction;
            }
            //Поворот головы
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
            //Определение следующей точки на карте для следования
            NextPos = transform.GetChild(0).right * GameMap.Step + transform.localPosition;
            WidthPos = Mathf.RoundToInt(NextPos.x / GameMap.Step + GameMap.MapWidth / 2);
            LengthPos = Mathf.RoundToInt(NextPos.z / GameMap.Step + GameMap.MapLength / 2);
            NextPos.x = (WidthPos - GameMap.MapWidth / 2) * GameMap.Step;
            NextPos.z = (LengthPos - GameMap.MapLength / 2) * GameMap.Step;
        }
    }
    /// <summary>
    /// Уничтожение змеи
    /// </summary>
    public void Die()
    {
        for (int _part = Body.Count - 1; _part >= 0; _part--)
            GameObject.Destroy(Body[_part].transform.gameObject);
        Body.Clear();
        GameObject.Destroy(this.gameObject);
    }
    /// <summary>
    /// Проверка столкновений с предметами на карте
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!ChackCollider) return;
        //Кушаем яблоко
        if (other.tag == "Apple")
        {
            GameMap.EatApple(WidthPos, LengthPos);
            AddNewPart(true);
        }
        //Убиваем змею
        if (other.tag == "Wall" || (other.tag == "Corner" && Body.Count > 3)) Main.SnakeReport(this);
        //Делим змею
        if (other.tag == "Body")
        {
            int _part;
            for (_part = 1; _part < Body.Count - 3; _part++)
                if (Body[_part].transform.gameObject.name == other.gameObject.transform.parent.gameObject.name)
                {
                    Main.SnakeReport(this, _part);
                    ChackCollider = false;
                    break;
                }
        }
    }
}