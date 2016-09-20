using UnityEngine;
using SharedSpace;

public class TMap {
    //Параметры карты
    public int MapWidth = 5;
    public int MapLength = 5;
    public float Step = 0.13f;
    public Transform transform;
    public int MaxCountApples = 4;
    public Plane GroundPlane;
    //Счетчики
    private int AppleOnMap = 0;
    private int ExtraBox = 0;
    /// <summary>
    /// Структура ячейки карты
    /// </summary>
    private struct Node
    {
        //0 - Wall, 1 - Apple, 2 - Ground
        public byte Type;
        public Vector3 Position;
        public TPart Part;
        /// <summary>
        /// Инициализация блока на карте, получает на вход тип блока, положение и если нужно - объект
        /// </summary>
        public Node(byte _type, Vector3 _position, TPart _part = null)
        {
            Type = _type;
            Position = _position;
            Part = _part;
            if (_part == null) Type = 2;
        }
    }
    /// <summary>
    /// Карта
    /// </summary>
    private Node[,] MapGrid;

    /// <summary>
    /// Создание карты
    /// </summary>
    public void GenerateMap() {
        MapGrid = new Node[MapWidth, MapLength];
        AppleOnMap = 0;
        ExtraBox = 0;
        GroundPlane = new Plane(transform.up, transform.position);

        for (int _w = 0; _w < MapWidth; _w++)
            for (int _l = 0; _l < MapLength; _l++)
            {
                if (_w == 0 || _l == 0 || _w == MapWidth - 1 || _l == MapLength - 1)
                    AddPart(0, _w, _l);
                else
                {
                    if (Random.Range(0, 100) > 97 && ExtraBox < MapWidth * MapLength * 0.05f + 1)
                    {
                        AddPart(0, _w, _l);
                        ExtraBox++;
                    } else
                        MapGrid[_w, _l] = new Node(2, new Vector3((_w - MapWidth / 2), 0f, (_l - MapLength / 2)) * Step);
                }
            }
        while (AppleOnMap < MaxCountApples)
        {
            int _w = Random.Range(1, MapWidth - 1);
            int _l = Random.Range(1, MapLength - 1);
            while (MapGrid[_w, _l].Type != 2)
            {
                _w = Random.Range(1, MapWidth - 1);
                _l = Random.Range(1, MapLength - 1);
            }
            AppleOnMap++;
            AddPart(1, _w, _l);
        }
    }
    /// <summary>
    /// Добавление нового объекта в карту
    /// </summary>
    private void AddPart(byte _type, int _width, int _length)
    {
        GameObject InitPart = (GameObject)Object.Instantiate(Resources.Load("Prefabs/" + (_type == 0 ? "WoodBox" : "Apple")));
        InitPart.transform.SetParent(transform);
        InitPart.transform.localScale = Vector3.one;
        InitPart.transform.localPosition = new Vector3((_width - MapWidth / 2), 0f, (_length - MapLength / 2)) * Step;
        TPart PartAI = InitPart.AddComponent<TPart>();
        PartAI.State = EPartState.SHOW;
        PartAI.Type = _type;
        PartAI._length = _length;
        PartAI._width = _width;
        MapGrid[_width, _length] = new Node(1, InitPart.transform.localPosition, PartAI);
    }
    /// <summary>
    /// Получение координат элемента по позиции в сетке
    /// </summary>
    public Vector3 GetPos(int _width, int _length)
    {
        Vector3 result = Vector3.zero;
        if (MapGrid != null && (_width > -1 && _width < MapWidth && _length > -1 && _length < MapLength))
            result = MapGrid[_width, _length].Position;
        return result;
    }
    /// <summary>
    /// Очистка карты
    /// </summary>
    public void CleanMap()
    {
        if (MapGrid == null) return;
        for (int _w = 0; _w < MapWidth; _w++)
            for (int _l = 0; _l < MapLength; _l++)
                if (MapGrid[_w, _l].Part != null) GameObject.Destroy(MapGrid[_w, _l].Part.gameObject);
        MapGrid = null;

        System.GC.Collect();
    }
    /// <summary>
    /// Кушаем яблоко и создаем новое
    /// </summary>
    public void EatApple(int _width, int _length)
    {
        if (MapGrid[_width, _length].Type == 1) {
            MapGrid[_width, _length].Part.State = EPartState.HIDE;
            SharedData.CurrentScore++;
            int _w = Random.Range(1, MapWidth - 1);
            int _l = Random.Range(1, MapLength - 1);
            while (MapGrid[_w, _l].Type != 2)
            {
                _w = Random.Range(1, MapWidth - 1);
                _l = Random.Range(1, MapLength - 1);
            } 
            MapGrid[_w, _l].Part = MapGrid[_width, _length].Part;
            MapGrid[_w, _l].Part._width = _w;
            MapGrid[_w, _l].Part._length = _l;
            MapGrid[_w, _l].Part.NewLive(MapGrid[_w, _l].Position);
            MapGrid[_w, _l].Type = 1;
            MapGrid[_width, _length].Type = 2;
            MapGrid[_width, _length].Part = null;
        }
    }
}