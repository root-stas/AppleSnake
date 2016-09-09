using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Vuforia;
using SharedSpace;

public class MainSrc : MonoBehaviour
{
    /// <summary>
    /// Начальное время на выполнения задания
    /// </summary>
    public int StartTime = 60;
    /// <summary>
    /// Шаг, с которым будет уменьшаться начальное время на каждом новом уровне
    /// </summary>
    public int TimeStep = 5;
    /// <summary>
    /// Так сказать чувствительность при сравнении рисунка с заданием, чем больше тем больше отклонений может быть от задания (был квадрат, нарисовали круг, а игра посчитает что все нормально)
    /// </summary>
    [Range(0.2f, 1f)]
    public float Difficulty = 0.5f;

    /// <summary>
    /// Объект указателя с эффектом хвоста кометы
    /// </summary>
    private Transform Cursor;

    private Slider MapWSlider;
    private Slider MapLSlider;
    private TMap GameMap;
    private List<TSnake> Snakes = new List<TSnake>();

    /// Элементы игрового интерфейса и игровых экранов 
    /// <summary>
    /// Текстовое поле отображающие остаток времени на выполнение задания
    /// </summary>
    private Text InfoTxT;
    /// Элементы игрового интерфейса и игровых экранов 
    /// <summary>
    /// Текстовое поле отображающие остаток времени на выполнение задания
    /// </summary>
    private Text CurScoreTxT;
    /// <summary>
    /// Текстовое поле отображающие счет игрока
    /// </summary>
    private Text BestScoreTxT;
    /// <summary>
    /// Экран меню, кнопки новой игры, редактора и выхода
    /// </summary>
    private GameObject MenuUI;
    /// <summary>
    /// Игровой экран, кнопка выхода в меню и время раунда
    /// </summary>
    private GameObject GameUI;
    /// <summary>
    /// Экран диалога редактора, !!!set size map!!! или выход в меню
    /// </summary>
    private GameObject EditorUI;
    /// <summary>
    /// Окно проигрыша со счетом и перезапуском уровня
    /// </summary>
    private GameObject DialogUI;
    /// <summary>
    /// Окно проигрыша со счетом и перезапуском уровня
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
        InfoTxT = GameObject.Find("InfoTxt").GetComponent<Text>() as Text;
        MapWSlider = GameObject.Find("MapWSlider").GetComponent<Slider>();
        MapLSlider = GameObject.Find("MapLSlider").GetComponent<Slider>();

        //Инициализация экрана меню и запуск таймера
        MenuBtn((int)EState.MENU);
        StartCoroutine(OneSecEvent());
    }

    /// <summary>
    /// Таймер в 1с, обновляет ... игры
    /// </summary>
    /// <returns></returns>
    IEnumerator OneSecEvent()
    {
        while (true)
        {
            if (SharedData.GameState == EState.GAME)
            {
                CurScoreTxT.text = SharedData.CurrentScore.ToString();
            }
            yield return new WaitForSeconds(1);
        }
    }
    private Vector2 MouseDownPos;
    //Обновление экрана игры и обработка ввода
    void Update()
    {
        if (Cursor != null) Cursor.position = Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(1f);
        switch (SharedData.GameState)
        {
            case EState.GAME:
                if (Input.GetButtonUp("Cancel")) MenuBtn((int)EState.GAME_DIALOG);

                float _input = Input.GetAxis("Horizontal");
                for (int _snakeID = 0; _snakeID < Snakes.Count; _snakeID++)
                {
                    if (Snakes[_snakeID].MoveState == 1)
                    {
                        if (_input > 0.5f) { Snakes[_snakeID].MoveState = 2; }
                        if (_input < -0.5f) { Snakes[_snakeID].MoveState = 0; }

                        if (Input.GetMouseButtonUp(0))
                        {
                            MouseDownPos = Camera.main.WorldToScreenPoint(Snakes[_snakeID].transform.position);
                            if (Snakes[_snakeID].Direction == 0)
                                if (MouseDownPos.y > Input.mousePosition.y)
                                    Snakes[_snakeID].MoveState = 2;
                                else
                                    Snakes[_snakeID].MoveState = 0;
                            if (Snakes[_snakeID].Direction == 1)
                                if (MouseDownPos.x > Input.mousePosition.x)
                                    Snakes[_snakeID].MoveState = 2;
                                else
                                    Snakes[_snakeID].MoveState = 0;
                            if (Snakes[_snakeID].Direction == 3)
                                if (MouseDownPos.x > Input.mousePosition.x)
                                    Snakes[_snakeID].MoveState = 0;
                                else
                                    Snakes[_snakeID].MoveState = 2;
                            if (Snakes[_snakeID].Direction == 2)
                                if (MouseDownPos.y > Input.mousePosition.y)
                                    Snakes[_snakeID].MoveState = 0;
                                else
                                    Snakes[_snakeID].MoveState = 2;
                        }
                    }
                }
                if (_input > 0.5f || _input < -0.5f) Input.ResetInputAxes();

                break;
            case EState.MAP_SETTINGS:
                if (Input.GetButtonUp("Cancel")) MenuBtn((int)EState.MENU);
                if (Input.GetKeyUp(KeyCode.Return)) MenuBtn((int)EState.GAME);
                break;
            case EState.TARGET_CREATOR:
                if (Input.GetButtonUp("Cancel")) MenuBtn((int)EState.MENU);
                if (Input.GetMouseButtonUp(0))
                {         
                    // Trigger an autofocus event
                    CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO);
                }
                if (Input.GetKeyUp(KeyCode.Return))
                {
                    UDTEventHandler UDTE = GameObject.Find("UserDefinedTargetBuilder").GetComponent<UDTEventHandler>();
                    UDTE.BuildNewTarget();
                }
                break;
            case EState.MENU:
                if (Input.GetButtonUp("Cancel")) Application.Quit();
                if (Input.GetKeyUp(KeyCode.Return)) MenuBtn((int)EState.GAME);
                if (Input.GetKeyUp(KeyCode.Space)) MenuBtn((int)EState.TARGET_CREATOR);
                break;
            case EState.GAME_DIALOG:
                if (Input.GetButtonUp("Cancel")) MenuBtn((int)EState.MENU);
                if (Input.GetKeyUp(KeyCode.Return)) MenuBtn((int)EState.GAME);
                break;
        }
    }

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
            //Cut snake
            GameObject Snake = (GameObject)Object.Instantiate(Resources.Load("Prefabs/SnakeStart"));
            Snake.transform.SetParent(transform);
            Snake.transform.localScale = Vector3.one;
            Snake.transform.localPosition = Vector3.zero;
            Snakes.Add(Snake.AddComponent<TSnake>());
            Snake.transform.GetChild(0).localRotation = Snakes[_ID].transform.GetChild(0).localRotation;
            Snakes[Snakes.Count - 1].ChackCollider = true;
            Snakes[Snakes.Count - 1].GameMap = GameMap;
            Snakes[Snakes.Count - 1].Direction = Snakes[_ID].Direction;
            Snakes[Snakes.Count - 1].WidthPos = Snakes[_ID].Body[0].WidthPos;
            Snakes[Snakes.Count - 1].LengthPos = Snakes[_ID].Body[0].LengthPos;

            if (Snakes[_ID].Direction == 2) Snakes[Snakes.Count - 1].WidthPos -= 2;
            if (Snakes[_ID].Direction == 1) Snakes[Snakes.Count - 1].LengthPos -= 2;
            if (Snakes[_ID].Direction == 3) Snakes[Snakes.Count - 1].LengthPos += 2;
            if (Snakes[_ID].Direction == 0) Snakes[Snakes.Count - 1].WidthPos += 2;

            Snakes[Snakes.Count - 1].Main = GetComponent<MainSrc>();

            for (int _id = _partID + 1; _id >= 0; _id--)
            {
                GameObject.Destroy(Snakes[_ID].Body[_id].transform.gameObject);
                Snakes[_ID].Body.Remove(Snakes[_ID].Body[_id]);
                if (_id == _partID + 1)
                {
                    Snakes[_ID].AddNewPart(false, _id);
                }
            }

            _snake.ChackCollider = true;
        }
        if (Snakes.Count < 1) MenuBtn((int)EState.GAME_DIALOG);
    }

    /// <summary>
    /// Изменение состояния игры в зависимости от значения параметра
    /// </summary>
    /// <param name="_newState">задает новое состояние игре, принимает значения EState</param>
    public void MenuBtn(int _newState)
    {
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
                GameObject Snake = (GameObject)Object.Instantiate(Resources.Load("Prefabs/SnakeStart"));
                Snake.transform.SetParent(transform);
                Snake.transform.localScale = Vector3.one;
                Snake.transform.localPosition = Vector3.zero;
                Snakes.Add(Snake.AddComponent<TSnake>());
                Snakes[0].GameMap = GameMap;
                Snakes[0].Main = GetComponent<MainSrc>();
                GameMap.MapWidth = (int)MapWSlider.value;
                GameMap.MapLength = (int)MapLSlider.value;
                GameMap.GenerateMap();

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