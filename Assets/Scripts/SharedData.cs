namespace SharedSpace
{
    /// <summary>
    /// Константы, определяющие состояние игры, для удобства чтения кода
    /// </summary>
    public enum EState
    {
        MENU = 0,
        GAME = 1,
        GAME_DIALOG = 2,
        MAP_SETTINGS = 3,
        TARGET_CREATOR = 4,
        EXIT_GAME = 5,
    }
    /// <summary>
    /// Тип анимации элементов карты
    /// </summary>
    public enum EPartState
    {
        SHOW = 0,
        IDLE = 1,
        HIDE = 2,
        ANIM = 3,
    } 

    public class SharedData
    {
        /// <summary>
        /// Состояние игры
        /// </summary>
        static public EState GameState = EState.MENU;
        /// <summary>
        /// Глобальный счетчик созданых частей тела змей
        /// </summary>
        static public int BodyCount = 0;
        /// <summary>
        /// Текущий счет игры
        /// </summary>
        static public int CurrentScore = 0;
        /// <summary>
        /// Лучший счет игры
        /// </summary>
        static public int BestScore = 0;
    }
}