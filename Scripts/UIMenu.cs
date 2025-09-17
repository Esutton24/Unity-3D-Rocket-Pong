using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIMenu : MonoBehaviour
{
    public System.Action ExitMenu;

    [SerializeField] protected InputManager _input;
    [SerializeField] protected Selectable _currentSelectable;
    [SerializeField] private bool _isBaseMenu;
    [SerializeField] private bool _keepSelectableOnBack;

    private UIMenu _prevMenu;
    private Coroutine C_Move;
    private Direction _prevMoveDirection = Direction.None;
    private TMP_Dropdown _prevDropdown;
    private bool _justActivated = false;
    private bool _enabled = false;

    private const float TIME_BEFORE_MOVE = 0.25f;

    public void SetActive(UIMenu prevMenu)
    {
        _prevMoveDirection = Direction.None; //Resetting MoveDirection
        //Only run this if turning on after being off
        if (!_enabled)
        {
            //If newly setting menu active
            if (prevMenu != null)
            {
                if (_prevDropdown == null)
                    _prevMenu = prevMenu;
                //if we dont have a current selectable, get one
                SetSelectableFirst();
                _prevMenu.gameObject.SetActive(false);
            }
            else if (_currentSelectable == null || !_keepSelectableOnBack)
                SetSelectableFirst();
            //Make sure to set the highlighted object
            HighlightCurrentSelectable();
            _justActivated = true;
            gameObject.SetActive(true);
        }
    }
    protected virtual void Navigate(Direction moveDirection)
    {
        //if we dont have the current selectable, get one
        if (_currentSelectable == null)
        {
            SetSelectableFirst();
            StopAutoMove();
        }
        if ((_currentHighlightedGO == null || !_currentHighlightedGO.Equals(_currentSelectable.gameObject)) && moveDirection != Direction.None)
        {
            HighlightCurrentSelectable();
            StopAutoMove();
            return;
        }

        if (moveDirection.Equals(_prevMoveDirection))
        {
            //If we already have auto move on, return
            if (C_Move != null)
            {
                return;
            }
            //If there isnt a move direction, return
            if (moveDirection.Equals(Direction.None)) return;
        }
        else if (C_Move != null) StopAutoMove();

        bool stillTryMove = true;
        if (_currentSelectable.GetType().Equals(typeof(Scrollbar)) && !handleScrollBar()) stillTryMove = false;
        if (_currentSelectable.GetType().Equals(typeof(Slider)) && !handleSlider()) stillTryMove = false;

        if (stillTryMove)
            tryMove();

        _prevMoveDirection = moveDirection;

        HighlightCurrentSelectable();

        void tryMove()
        {
            if (moveDirection == Direction.None) return;
            Selectable moveSelectable;

            switch (moveDirection)
            {
                case Direction.Up:
                    moveSelectable = _currentSelectable.FindSelectableOnUp();
                    break;
                case Direction.Down:
                    moveSelectable = _currentSelectable.FindSelectableOnDown();
                    break;
                case Direction.Left:
                    moveSelectable = _currentSelectable.FindSelectableOnLeft();
                    break;
                case Direction.Right:
                    moveSelectable = _currentSelectable.FindSelectableOnRight();
                    break;
                default:
                    moveSelectable = null;
                    break;
            }

            if (moveSelectable == null)
            {
                if (_prevDropdown == null) return;
                _currentSelectable = _prevDropdown;
                DeactivatePrevDD();
                return;
            }

            _currentSelectable = moveSelectable;

            if (_prevDropdown != null)
                handleDDNav(moveSelectable);

            C_Move = StartCoroutine(autoMoveTimer());

            AudioManager.instance.PlaySound("Tick", Vector3.zero, 1f);
        }
        bool handleScrollBar()
        {
            Scrollbar scrollbar = _currentSelectable as Scrollbar;
            int direction = 0;
            switch (scrollbar.direction)
            {
                case Scrollbar.Direction.BottomToTop:
                    if (moveDirection == Direction.Up)
                        direction = 1;
                    else if (moveDirection == Direction.Down)
                        direction = -1;
                    break;
                case Scrollbar.Direction.TopToBottom:
                    if (moveDirection == Direction.Up)
                        direction = -1;
                    else if (moveDirection == Direction.Down)
                        direction = 1;
                    break;
                case Scrollbar.Direction.LeftToRight:
                    if (moveDirection == Direction.Right)
                        direction = 1;
                    else if (moveDirection == Direction.Left)
                        direction = -1;
                    break;
                case Scrollbar.Direction.RightToLeft:
                    if (moveDirection == Direction.Right)
                        direction = -1;
                    else if (moveDirection == Direction.Left)
                        direction = 1;
                    break;
            }
            float value = scrollbar.value + direction * Time.unscaledDeltaTime;
            scrollbar.value = Mathf.Clamp01(value);
            scrollbar.onValueChanged.Invoke(scrollbar.value);
            return direction == 0 || value != scrollbar.value;
        }
        void handleDDNav(Selectable s)
        {
            _prevDropdown.RefreshShownValue();
            System.Type selectableType = _currentSelectable.GetType();
            if (selectableType.Equals(typeof(Toggle)))
            {
                var scrollRect = s.GetComponentInParent<ScrollRect>();
                var optionsArr = scrollRect.content;
                int optionsIndex = s.transform.GetSiblingIndex();
                int optionsAmount = optionsArr.childCount;
                Scrollbar scrollBar = scrollRect.verticalScrollbar;
                switch (scrollBar.direction)
                {
                    case Scrollbar.Direction.LeftToRight:
                    case Scrollbar.Direction.BottomToTop:
                        scrollBar.value = 1 - (float)optionsIndex / (optionsAmount - 1);
                        break;
                    case Scrollbar.Direction.RightToLeft:
                    case Scrollbar.Direction.TopToBottom:
                        scrollBar.value = (float)optionsIndex / (optionsAmount - 1);
                        break;
                }
                scrollBar.onValueChanged.Invoke(scrollBar.value);
                return;
            }
            if (!selectableType.Equals(typeof(Scrollbar)))
                DeactivatePrevDD();
        }
        bool handleSlider()
        {
            Slider slider = _currentSelectable as Slider;
            int direction = 0;
            switch (slider.direction)
            {
                case Slider.Direction.BottomToTop:
                    if (moveDirection == Direction.Up)
                        direction = 1;
                    else if (moveDirection == Direction.Down)
                        direction = -1;
                    break;
                case Slider.Direction.TopToBottom:
                    if (moveDirection == Direction.Up)
                        direction = -1;
                    else if (moveDirection == Direction.Down)
                        direction = 1;
                    break;
                case Slider.Direction.LeftToRight:
                    if (moveDirection == Direction.Right)
                        direction = 1;
                    else if (moveDirection == Direction.Left)
                        direction = -1;
                    break;
                case Slider.Direction.RightToLeft:
                    if (moveDirection == Direction.Right)
                        direction = -1;
                    else if (moveDirection == Direction.Left)
                        direction = 1;
                    break;
            }
            float value = slider.value + direction * Time.unscaledDeltaTime;
            slider.value = Mathf.Clamp01(value);
            slider.onValueChanged.Invoke(slider.value);
            return direction == 0 || value != slider.value;
        }
        IEnumerator autoMoveTimer()
        {
            yield return new WaitForSecondsRealtime(TIME_BEFORE_MOVE);
            C_Move = null;
            Navigate(_prevMoveDirection);
        }
    }
    public void Back()
    {
        _currentSelectable = null;
        _prevMoveDirection = Direction.None;
        _justActivated = false;
        if (C_Move != null)
            StopCoroutine(C_Move);
        if (_prevDropdown != null)
        {
            DeactivatePrevDD();
            return;
        }
        if (_prevMenu == null)
        {
            if (ExitMenu != null)
            {
                ExitMenu?.Invoke();
                gameObject.SetActive(false);
            }
            return;
        }
        AudioManager.instance.PlaySound("Woosh", Vector2.zero, 0.5f);
        gameObject.SetActive(false);
        _prevMenu.SetActive(null);
        _prevMenu = null;
    }
    public void OnSelectablePress()
    {
        if (!enabled) return;
        Debug.Log($"Pressed: {_currentSelectable.name}", _currentSelectable);
        System.Type sType = _currentSelectable.GetType();
        if (sType.Equals(typeof(Button)))
            button(_currentSelectable as Button);
        else if (sType.Equals(typeof(Toggle)))
            toggle(_currentSelectable as Toggle);
        else if (sType.Equals(typeof(TMP_Dropdown)))
            dropDown(_currentSelectable as TMP_Dropdown);
        AudioManager.instance.PlaySound("Open", Vector2.zero, 1);
        void button(Button b) => b.onClick.Invoke();
        void toggle(Toggle t)
        {
            t.isOn = !t.isOn;
            DeactivatePrevDD();
        }
        void dropDown(TMP_Dropdown dd)
        {
            if (dd.IsExpanded)
                DeactivatePrevDD();
            else
            {
                dd.Show();
                _prevDropdown = dd;
                dd.RefreshShownValue();
                _currentSelectable = dd.GetComponentInChildren<Selectable>();
            }
        }
    }
    protected virtual void Update()
    {
        //Process Movement Values into directions
        Direction inputDirection = _input.Axis1Value.ToDirection();
        Navigate(inputDirection);
        _justActivated = false;
    }
    protected virtual void Awake()
    {
        if (_justActivated) return;
        //If this is the baseMenu then set active
        if (_isBaseMenu)
            SetActive(null);
        else
            gameObject.SetActive(false);
    }
    protected virtual void OnEnable()
    {
        if (_enabled) { Debug.Log("Enabled Twice?", gameObject); return; }
        _input.PauseButtonPerformed += Back; //When the pause button is pressed, try to go back to the previous menu
        _input.Button1Performed += OnSelectablePress; //When b1 is pressed, try to press the current selectable
        _justActivated = true;
        _enabled = true;
    }
    protected virtual void OnDisable()
    {
        //Unregistering
        _input.PauseButtonPerformed -= Back;
        _input.Button1Performed -= OnSelectablePress;
        _justActivated = false;
        _enabled = false;
    }
    #region HelperMethods
    void DeactivatePrevDD()
    {
        if (_prevDropdown == null) return;
        if (_prevDropdown.gameObject == null)
        {
            _prevDropdown = null;
            return;
        }
        _prevDropdown.RefreshShownValue();
        _prevDropdown.Hide();
        _currentSelectable = _prevDropdown;
        _prevDropdown = null;
    }
    void SetSelectableFirst()
    {
        Selectable firstSelectableChild = GetComponentInChildren<Selectable>(true);
        _currentSelectable = firstSelectableChild;
    }
    void HighlightCurrentSelectable() => _currentHighlightedGO = _currentSelectable.gameObject;
    GameObject _currentHighlightedGO
    {
        get { return EventSystem.current.currentSelectedGameObject; }
        set
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(value, new BaseEventData(EventSystem.current));
        }
    }
    Selectable _currentHighlightedSelectable
    {
        get
        {
            if (_currentHighlightedGO.TryGetComponent(out Selectable s))
                return s;
            return null;
        }
    }

    void StopAutoMove()
    {
        if (C_Move == null) return;
        StopCoroutine(C_Move);
        C_Move = null;
    }
    #endregion
}