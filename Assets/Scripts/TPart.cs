using UnityEngine;
using SharedSpace;

public class TPart : MonoBehaviour {

    public EPartState State;
    //0 - Wall, 1 - Apple
    public byte Type;

    private float Speed = 5f;
    private Vector3 StartSize = Vector3.one * 0.1f;
    private Vector3 StartPosition;
    private Vector3 NewPosition;

    // Use this for initialization
    void Start () {
        transform.localScale = StartSize;
        StartPosition = transform.localPosition;
    }
	
    public void NewLive(Vector3 _pos)
    {
        NewPosition = _pos;
    }

	// Update is called once per frame
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
                transform.localPosition = StartPosition + Vector3.up * Mathf.PerlinNoise(Time.time, 10) * 0.2f;
                break;
        }
	}
}