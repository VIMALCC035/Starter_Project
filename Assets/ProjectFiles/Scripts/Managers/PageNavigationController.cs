using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class PageNavigationController : MonoBehaviour
{
    [Header("Navigation Buttons")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;

    [Header("Page Display")]
    [SerializeField] private TMP_Text pageNumberText;

    [Header("Testing Mode (Ignore Locks)")]
    [SerializeField] private bool testing = false;

    [Header("Requires Interaction Per Page (Navigation Source)")]
    [SerializeField] private List<bool> requiresInteraction = new();

    // Events
    public static event Action<int> OnPageChanged;
    public static event Action OnNavigationUnlockRequested;

    // State (1-Based)
    public static int CurrentPageNumber { get; private set; }
    public static PageNavigationController Instance { get; private set; }

    [SerializeField] private int currentPageNumber = 1;

    // Runtime State
    private readonly HashSet<int> visitedPages = new();
    private readonly HashSet<int> completedPages = new();

    private int NavigationPageCount => Mathf.Max(1, requiresInteraction.Count);

    private void Awake()
    {
        Instance = this;
        // Clamp to ensure it starts at 1
        currentPageNumber = Mathf.Clamp(currentPageNumber, 1, NavigationPageCount);
    }

    private void OnEnable()
    {
        OnNavigationUnlockRequested += EnableNavigationButtons;
    }

    private void Start()
    {
        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (previousButton) previousButton.onClick.AddListener(PreviousPage);

        visitedPages.Add(currentPageNumber);

        UpdateButtons();
        UpdateDisplay();
        RaisePageChanged();
    }

    private void OnDisable()
    {
        OnNavigationUnlockRequested -= EnableNavigationButtons;
    }

    private void OnDestroy()
    {
        if (nextButton) nextButton.onClick.RemoveListener(NextPage);
        if (previousButton) previousButton.onClick.RemoveListener(PreviousPage);
        if (Instance == this) Instance = null;
    }

    public void NextPage()
    {
        if (currentPageNumber >= NavigationPageCount)
            return;

        currentPageNumber++;
        visitedPages.Add(currentPageNumber);

        UpdateButtons();
        UpdateDisplay();
        RaisePageChanged();
    }

    public void PreviousPage()
    {
        if (currentPageNumber <= 1)
            return;

        currentPageNumber--;
        visitedPages.Add(currentPageNumber);

        UpdateButtons();
        UpdateDisplay();
        RaisePageChanged();
    }

    private void RaisePageChanged()
    {
        CurrentPageNumber = currentPageNumber;
        OnPageChanged?.Invoke(currentPageNumber);
    }

    private void UpdateButtons()
    {
        if (testing)
        {
            SetNormalButtonState();
            return;
        }

        // Convert 1-based page number to 0-based index for the list
        int listIndex = currentPageNumber - 1;
        bool needsInteraction = listIndex < requiresInteraction.Count && requiresInteraction[listIndex];
        bool isCompleted = completedPages.Contains(currentPageNumber);

        if (previousButton)
            previousButton.interactable = currentPageNumber > 1;

        if (nextButton)
        {
            nextButton.interactable = !needsInteraction || isCompleted;
        }
    }

    private void SetNormalButtonState()
    {
        if (previousButton) previousButton.interactable = currentPageNumber > 1;
        if (nextButton) nextButton.interactable = true;
    }

    public void EnableNavigationButtons()
    {
        completedPages.Add(currentPageNumber);
        UpdateButtons();
    }

    public static void RequestNavigationUnlock()
    {
        OnNavigationUnlockRequested?.Invoke();
    }

    private void UpdateDisplay()
    {
        if (pageNumberText)
            pageNumberText.text = $"{currentPageNumber}/{NavigationPageCount}";
    }
}