using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SwipeInputHandler : MonoBehaviour
{
    [Header("Swipe Config")]
    public float minSwipeLength = 50f;
    public float maxTrackTime = 1f;

   // private PlayerInputActions inputActions;
    private List<Vector2> swipePoints = new List<Vector2>();
    private float swipeStartTime;
    private bool isSwiping = false;

    public Vector3 SwipeDirection { get; private set; }
    public float SwipeLength { get; private set; }
    public float SpinStrength { get; private set; }
    public Vector3 SpinAxis { get; private set; }

    public delegate void OnSwipeComplete(Vector3 dir, float length, float spin, Vector3 spinAxis);
    public event OnSwipeComplete SwipeCompleted;

    private void Awake()
    {
       // inputActions = new PlayerInputActions();

       // inputActions.Touch.PointerPress.started += ctx => StartSwipe();
       // inputActions.Touch.PointerPress.canceled += ctx => EndSwipe();
    }

    //private void OnEnable() => inputActions.Enable();
    //private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        if (!isSwiping) return;

       // Vector2 pointerPos = inputActions.Touch.PointerPosition.ReadValue<Vector2>();

        if (Time.time - swipeStartTime > maxTrackTime)
        {
            EndSwipe(); // таймаут
            return;
        }

       // if (swipePoints.Count == 0 || Vector2.Distance(swipePoints[^1], pointerPos) > 5f)
        {
            //swipePoints.Add(pointerPos);
        }
    }

    private void StartSwipe()
    {
        isSwiping = true;
        swipeStartTime = Time.time;
        swipePoints.Clear();

       // Vector2 startPos = inputActions.Touch.PointerPosition.ReadValue<Vector2>();
       // swipePoints.Add(startPos);
    }

    private void EndSwipe()
    {
        if (!isSwiping || swipePoints.Count < 2)
            return;

        isSwiping = false;

        Vector2 start = swipePoints[0];
        Vector2 end = swipePoints[^1];
        SwipeLength = Vector2.Distance(start, end);

        if (SwipeLength < minSwipeLength)
            return;

        Vector2 screenDir = (end - start).normalized;

        // Перевод в мировое пространство
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        SwipeDirection = (right * screenDir.x + forward * screenDir.y).normalized;

        // Оценка кривизны (спина)
        float deviation = 0f;
        for (int i = 1; i < swipePoints.Count - 1; i++)
        {
            deviation += Mathf.Abs(DistanceFromPointToLine(swipePoints[i], start, end));
        }

        SpinStrength = Mathf.Clamp01(deviation / SwipeLength);
        SpinAxis = Vector3.Cross(SwipeDirection, Vector3.up).normalized;

        SwipeCompleted?.Invoke(SwipeDirection, SwipeLength, SpinStrength, SpinAxis);
    }

    private float DistanceFromPointToLine(Vector2 point, Vector2 a, Vector2 b)
    {
        float A = point.x - a.x;
        float B = point.y - a.y;
        float C = b.x - a.x;
        float D = b.y - a.y;

        float dot = A * C + B * D;
        float len_sq = C * C + D * D;
        float param = len_sq > 0 ? dot / len_sq : -1;

        float xx = a.x + param * C;
        float yy = a.y + param * D;

        float dx = point.x - xx;
        float dy = point.y - yy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
