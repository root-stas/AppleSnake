using UnityEngine;
using SharedSpace;

public class TPart : MonoBehaviour {
    /// <summary>
    /// Состояние элемента карты
    /// </summary>
    public EPartState State;
    
    /// <summary>
    /// Тип объекта: 0 - стена, 1 - яблоко
    /// </summary>
    public byte Type;
    //Положение на карте
    public int _width, _length;
    /// <summary>
    /// Скорость анимации
    /// </summary>
    private float Speed = 5f;
    private Vector3 StartSize = Vector3.one * 0.1f;
    private Vector3 StartPosition;
    private Vector3 NewPosition;

    //Установка на карте
    void Start () {
        transform.localScale = StartSize;
        StartPosition = transform.localPosition;
    }
	//Перемещение объекта на новую позицию
    public void NewLive(Vector3 _pos)
    {
        NewPosition = _pos;
    }

	//Обновление анимации
	void Update () {
        switch (State)
        {
            case EPartState.SHOW:
                transform.localScale = Vector3.Slerp(transform.localScale, Vector3.one, Speed * Time.deltaTime);
                if (transform.localScale.x > 0.95f) State = EPartState.IDLE;
                break;
            case EPartState.IDLE:
                if (transform.localScale.x != 1f) transform.localScale = Vector3.one;
                if (Type == 1) State = EPartState.ANIM;
                break;
            case EPartState.HIDE:
                if (transform.localScale.x > StartSize.x + 0.05f) transform.localScale = Vector3.Slerp(transform.localScale, StartSize, Speed * Time.deltaTime);
                else if (NewPosition != null)
                {
                    transform.localPosition = StartPosition = NewPosition;
                    State = EPartState.SHOW;
                }
                break;
            case EPartState.ANIM:
                transform.GetChild(0).RotateAround(transform.GetChild(0).position, transform.up, 10f * Speed * Time.deltaTime);
                transform.localPosition = StartPosition + Vector3.up * Mathf.PerlinNoise(Time.time, 10) * 0.1f;
                break;
        }
	}
}