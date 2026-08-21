using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class GlobalCameraController : MonoBehaviour
{
    [System.Serializable]
    public class SecondaryCameraPoint
    {
        public int pageNumber;
        public Transform target;
    }

    [Header("Primary Page Camera Points (1-Based Order)")]
    [SerializeField] private List<Transform> pageCameraPoints = new();

    [Header("Secondary Camera Points (Used after first visit)")]
    [SerializeField] private List<SecondaryCameraPoint> secondaryCameraPoints = new();

    [Header("Movement")]
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    public UnityEvent OnMoveStart;
    public UnityEvent OnMoveEnd;

    private Coroutine routine;
    private int currentPageNumber = 1;
    private Camera mainCamera;

    private Dictionary<int, int> pageVisitCount = new();

    private void OnEnable()
    {
        PageNavigationController.OnPageChanged += MoveToPage;
        mainCamera = Camera.main;
    }

    private void OnDisable()
    {
        PageNavigationController.OnPageChanged -= MoveToPage;
    }

    private void MoveToPage(int pageNumber)
    {
        // Check array bounds using 0-based conversion
        if (pageNumber < 1 || pageNumber - 1 >= pageCameraPoints.Count)
            return;

        currentPageNumber = pageNumber;

        if (!pageVisitCount.ContainsKey(pageNumber))
            pageVisitCount[pageNumber] = 0;

        pageVisitCount[pageNumber]++;

        Transform target = GetTargetForPage(pageNumber);

        if (target != null)
            StartMove(target);
    }

    private Transform GetTargetForPage(int pageNumber)
    {
        int visitCount = pageVisitCount[pageNumber];

        if (visitCount == 1)
            return pageCameraPoints[pageNumber - 1]; // 0-Based array lookup

        for (int i = 0; i < secondaryCameraPoints.Count; i++)
        {
            if (secondaryCameraPoints[i].pageNumber == pageNumber)
            {
                if (secondaryCameraPoints[i].target != null)
                    return secondaryCameraPoints[i].target;
            }
        }

        return pageCameraPoints[pageNumber - 1]; // Fallback
    }

    public void ResetToPageDefault()
    {
        MoveToPage(currentPageNumber);
    }

    public void MoveTo(Transform target)
    {
        if (target == null) return;
        StartMove(target);
    }

    private void StartMove(Transform target)
    {
        if (mainCamera == null) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Transform target)
    {
        OnMoveStart?.Invoke();
        Transform camTransform = mainCamera.transform;

        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;
        Vector3 endPos = target.position;
        Quaternion endRot = target.rotation;

        float t = 0f;
        while (t < moveDuration)
        {
            float progress = ease.Evaluate(t / moveDuration);
            camTransform.position = Vector3.Lerp(startPos, endPos, progress);
            camTransform.rotation = Quaternion.Slerp(startRot, endRot, progress);
            t += Time.deltaTime;
            yield return null;
        }

        camTransform.position = endPos;
        camTransform.rotation = endRot;
        OnMoveEnd?.Invoke();
    }
}