using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Vuforia;
using SharedSpace;

public class MainSrc : MonoBehaviour
{
    /// <summary>
    /// Скорость змейки
    /// </summary>
    [Range(0.02f, 0.05f)]
    public float SnakeSpeed = 0.02f;

    /// <summary>
    /// Объект указателя с эффектом хвоста кометы
    /// </summary>
    private Transform Cursor;

    private Slider MapWSlider;
    private Slider MapLSlider;
    /// <summary>
    /// Объект карты уровня
    /// </summary>
    private TMap GameMap;
    /// <summary>
    /// Список хранящий всех змей на уровне
    /// </summary>
    private List<TSnake> Snakes = new List<TSnake>();

    /// Элементы игрового интерфейса и игровых экранов 
    /// <summary>
    /// Текстовое поле отображающее счет игры
    /// </summary>
    private Text CurScoreTxT;
    /// <summary>
    /// Текстовое поле отображающее лучший счет игрока
    /// </summary>
    private Text BestScoreTxT;
    /// <summary>
    /// Экран меню, кнопки новой игры, редактора маркера и выхода
    /// </summary>
    private GameObject MenuUI;
    /// <summary>
    /// Игровой экран, кнопка выхода в меню и счет раунда
    /// </summary>
    private GameObject GameUI;
    /// <summary>
    /// Экран диалога размера карты
    /// </summary>
    private GameObject EditorUI;
    /// <summary>
    /// Окно проигрыша со счетом и перезапуском уровня
    /// </summary>
    private GameObject DialogUI;
    /// <summary>
    /// Экран создания маркера
    /// </summary>
    private GameObject TargetUI;

    /// <summary>
    /// Инициализация игры, сбор всех необходимых компонентов из сцены и создание новых
    /// </summary>
    void Start()
    {
        //Сбор всех необходимых компонентов из сцены
        Cursor = GameObject.Find("CursorFX").transform;
        MenuUI = GameObject.Find("Menu");
        GameUI = GameObject.Find("Game");
        EditorUI = GameObject.Find("Editor");
        DialogUI = GameObject.Find("Fail");
        TargetUI = GameObject.Find("CreateMaskUI");
        CurScoreTxT = GameObject.Find("TimeTxt").GetComponent<Text>() as Text;
        BestScoreTxT = GameObject.Find("ScoreTxt").GetComponent<Text>() as Text;
        MapWSlider = GameObject.Find("MapWSlider").GetComponent<Slider>();
        MapLSlider = GameObject.Find("MapLSlider").GetComponent<Slider>();

        //Инициализация экрана меню и запуск таймера
        MenuBtn((int)EState.MENU);
        StartCoroutine(OneSecEvent());
    }

    /// <summary>
    /// Таймер в 1с, обновляет счет игры на экране
    /// </summary>
    IEnumerator OneSecEvent()
    {
        while (true)
        {
            if (SharedData.GameState == EState.GAME)
                CurScoreTxT.text = SharedData.CurrentScore.ToString();
            yield return new WaitForSeconds(1);
        }
    }

    //Обновление экрана игры и обработка ввода
    void Update()
    {
        //Рисуем эффект указателя
        if (Cursor != null) Cursor.position = Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(1f);
        switch (SharedData.GameState)
        {
            case EState.GAME:
                if (Input.GetButtonUp("Cancel")) MenuBtn((int)EState.GAME_DIALOG);

                //Обработка ввода и поворот змей если нужно
                float _input = Input.GetAxis("Horizontal");

                if (Input.GetMouseButtonUp(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    float rayDistance;
                    if (GameMap.GroundPlane.Raycast(ray, out rayDistance))
                    {
                        Vector3 Dot = ray.GetPoint(rayDistance) - transform.position;
                        for (int _snakeID = 0; _snakeID < Snakes.Count; _snakeID++)
                        {
                            _input = (Snakes[_snakeID].NextPos.x - Snakes[_snakeID].transform.localPosition.x) * (Dot.z - Snakes[_snakeID].transform.localPosition.z) - (Snakes[_snakeID].NextPos.z - Snakes[_snakeID].transform.localPosition.z) * (Dot.x - Snakes[_snakeID].transform.localPosition.x);
                            if (_input > 0f) Snakes[_snakeID].MoveState = 0;
                            if (_input < 0f) Snakes[_snakeID].MoveState = 2;
                            _input = 0f;
                        }
                    }
                }

                //Поворот стрелками или геймпадом
                if (_input > 0.5f || _input < -0.5f)
                {
                    for (int _snakeID = 0; _snakeID < Snakes.Count; _snakeID++)
                    if (Snakes[_snakeID].MoveState == 1)
                    {
                        if (_input > 0.5f) Snakes[_snakeID].MoveState = 2;
                        if (_input < -0.5f) Snakes[_snakeID].MoveState = 0;
                    }
                    Input.ResetInputAxes(); _input = 0;
                }

                break;
            case EState.MAP_SETTINGS:
                if (Input.GetButtonUp("Cancel")) MenuBtn((int)EState.MENU);
                if (Input.GetKeyUp(KeyCode.Return)) MenuBtn((int)EState.GAME);
                break;
            case EState.TARGET_CREATOR:
                if (Input.GetButtonUp("Cancel")) MenuBtn((int)EState.MENU);
                if (Input.GetMouseButtonUp(0))
                    CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
                if (Input.GetButtonUp("Submit"))
                {
                    UDTEventHandler UDTE = GameObject.Find("UserDefinedTargetBuilder").GetComponent<UDTEventHandler>();
                    UDTE.BuildNewTarget();
                }
                break;
            case EState.MENU:
                if (Input.GetButtonUp("Cancel")) Application.Quit();
                if (Input.GetButtonUp("Submit")) MenuBtn((int)EState.GAME);
                if (Input.GetButtonUp("Fire2")) MenuBtn((int)EState.TARGET_CREATOR);
                break;
            case EState.GAME_DIALOG:
                if (Input.GetButtonUp("Cancel")) MenuBtn((int)EState.MENU);
                if (Input.GetButtonUp("Submit")) MenuBtn((int)EState.GAME);
                break;
        }
    }
    /// <summary>
    /// Метод убивающий или разрезающий змей, если не передать второй параметр - то убивает змею переданную в первом параметре
    /// </summary>
    public void SnakeReport(TSnake _snake, int _partID = -1)
    {
        int _ID = Snakes.IndexOf(_snake);
        if (_partID == -1)
        {
            _snake.Die();
            Snakes.Remove(_snake);
        }
        else
        {
            //Создаем новую змею с головой в хвосте делимой
            GameObject Snake = (GameObject)Object.Instantiate(Resources.Load("Prefabs/SnakeStart"));
            Snake.transform.SetParent(transform);
            Snake.transform.localScale = Vector3.one;
            Snake.transform.localPosition = Snakes[_ID].Body[0].Position;
            Snake.transform.GetChild(0).localRotation = Snakes[_ID].transform.GetChild(0).localRotation;
            Snakes.Add(Snake.AddComponent<TSnake>());
            Snakes[Snakes.Count - 1].NextPos = Snakes[_ID].Body[0].Position;
            Snakes[Snakes.Count - 1].ChackCollider = true;
            Snakes[Snakes.Count - 1].GameMap = GameMap;
            Snakes[Snakes.Count - 1].Speed = SnakeSpeed;
            Snakes[Snakes.Count - 1].Main = GetComponent<MainSrc>();

            //Удаляем часть теля в старой змее и создаем аналогичные в новой
            for (int _id = _partID + 1; _id >= 0; _id--)
            {
                if (_id > 0 && _id < _partID)
                    Snakes[Snakes.Count - 1].AddNewPart(true);

                GameObject.Destroy(Snakes[_ID].Body[_id].transform.gameObject);
                Snakes[_ID].Body.Remove(Snakes[_ID].Body[_id]);
                if (_id == _partID + 1)
                    Snakes[_ID].AddNewPart(false, _id);
            }

            _snake.ChackCollider = true;
        }
        //Если не осталось змей - то сообщаем об окончании уровня
        if (Snakes.Count < 1) MenuBtn((int)EState.GAME_DIALOG);
    }

    /// <summary>
    /// Изменение состояния игры в зависимости от значения параметра
    /// </summary>
    /// <param name="_newState">задает новое состояние игре, принимает значения EState</param>
    public void MenuBtn(int _newState)
    {
        //Фокусируем изображение физической камеры
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);

        //Обнуление состояния
        if (EditorUI != null) EditorUI.SetActive(false);
        if (DialogUI != null) DialogUI.SetActive(false);
        if (GameUI != null) GameUI.SetActive(false);
        if (MenuUI != null) MenuUI.SetActive(false);
        if (TargetUI != null) TargetUI.SetActive(false);
        if (Cursor != null) Cursor.gameObject.SetActive(false);
        SharedData.GameState = (EState)_newState;
        if (GameMap != null) GameMap.CleanMap();
        for (int _id = 0; _id < Snakes.Count; _id++)
        {
            Snakes[_id].Die();
            Snakes.RemoveAt(_id);
        }
        Snakes.Clear();
        SharedData.BodyCount = 0;

        //Задание нового состояния
        switch (SharedData.GameState)
        {
            case EState.MENU:
                if (MenuUI != null) MenuUI.SetActive(true);
                break;
            case EState.GAME:
                if (GameUI != null) GameUI.SetActive(true);
                if (Cursor != null) Cursor.gameObject.SetActive(true);
                SharedData.CurrentScore = 0;
                CurScoreTxT.text = "0";
                if (GameMap == null)
                {
                    GameMap = new TMap();
                    GameMap.transform = transform;

                }
                GameMap.MapWidth = (int)MapWSlider.value;
                GameMap.MapLength = (int)MapLSlider.value;
                GameMap.GenerateMap();
                //Создаем первую змею
                GameObject Snake = (GameObject)Object.Instantiate(Resources.Load("Prefabs/SnakeStart"));
                Snake.transform.SetParent(transform);
                Snake.transform.localScale = Vector3.one;
                Snake.transform.localPosition = Vector3.zero;
                Snakes.Add(Snake.AddComponent<TSnake>());
                Snakes[0].GameMap = GameMap;
                Snakes[0].Speed = SnakeSpeed;
                Snakes[0].Main = GetComponent<MainSrc>();

                break;
            case EState.MAP_SETTINGS:
                if (EditorUI != null) EditorUI.SetActive(true);
                break;
            case EState.EXIT_GAME:
                Application.Quit();
                break;
            case EState.TARGET_CREATOR:
                if (TargetUI != null) TargetUI.SetActive(true);
                break;
            case EState.GAME_DIALOG:
                if (DialogUI != null) DialogUI.SetActive(true);
                if (SharedData.BestScore < SharedData.CurrentScore) SharedData.BestScore = SharedData.CurrentScore;
                //System.Environment.NewLine _へ__(‾◡◝ )> "\r\n"
                BestScoreTxT.text = "Текущий счет: " + System.Environment.NewLine + SharedData.CurrentScore + System.Environment.NewLine + "Лучший счет: " + System.Environment.NewLine + SharedData.BestScore;
                break;
        }
    }
}