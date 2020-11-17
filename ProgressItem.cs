using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using Zenject;

[RequireComponent(typeof(Image), typeof(RectTransform))]
[ExecuteInEditMode()]
public class ProgressItem : MonoBehaviour
{
    [SerializeField]
    private RectTransform _rectTransform;
    [SerializeField]
    private ThemeImage _image;
    [SerializeField]
    private ThemedImage _defaultImageData;
    [SerializeField]
    private ThemedImage _achiveImageData;
    [SerializeField]
    private ProgressBar _progressBar;
    [SerializeField]
    private int _achieveValue;
    [SerializeField]
    private UnityEvent _onAchieve;

    private bool _itemIsShown = false;

    public int AchieveValue
    {
        get => _achieveValue;
        set
        {
            _achieveValue = value;
            UpdatePosition();
        }
    }

    private bool isAchieved = false;
    private Tween _animation;
    private Tween _appearanceAnimation;

    private IAudioManager _audioManager;

    [Inject]
    private void Setup(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    private void Start()
    {
        if (_image != null && !_itemIsShown)
            _image.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        ShowItem(false);

        if (_progressBar != null)
        {
            _progressBar.OnProgressAnimated += UpdateItemWithAnimation;
            _progressBar.OnProgress += UpdateItem;
        }
    }

    void OnDisable()
    {
        if (_progressBar != null)
        { 
            _progressBar.OnProgressAnimated -= UpdateItemWithAnimation;
            _progressBar.OnProgress -= UpdateItem;
        }

        _animation?.Kill();
        _appearanceAnimation?.Kill();
    }

    void UpdateItem(float progressValue) => UpdateSprite(progressValue, false);

    void UpdateItemWithAnimation(float progressValue) => UpdateSprite(progressValue, true);

    void UpdateSprite(float progressValue, bool isAnimate)
    {
        var newIsAchieved = progressValue >= _achieveValue;

        if (newIsAchieved != isAchieved)
        {
            if (newIsAchieved)
            {
                if (_image != null && (_achiveImageData != null || _defaultImageData != null))
                {
                    _image.ThemedImage = _achiveImageData != null ? _achiveImageData : _defaultImageData;
                }

                if (isAnimate)
                {
                    _audioManager.Play(SoundName.EarnedStar, false);
                    AnimateAchieve();
                    _onAchieve.Invoke();
                }
            }
            else
            {
                if (_image != null && _defaultImageData != null)
                {
                    _image.ThemedImage = _defaultImageData;
                }
            }
            isAchieved = newIsAchieved;
        }
    }

    void UpdatePosition()
    {
        var progressBarTransform = _progressBar.GetComponent<RectTransform>();
        var progressBarMinAnchorX = progressBarTransform.anchorMin.x;
        var progressBarMaxAnchorX = progressBarTransform.anchorMax.x;

        var percent = (_achieveValue - _progressBar.minimum) / (float)(_progressBar.maximum - _progressBar.minimum);

        var starAnchorX = progressBarMinAnchorX != progressBarMaxAnchorX ?
            Mathf.Clamp(progressBarMinAnchorX + (progressBarMaxAnchorX - progressBarMinAnchorX) * percent, progressBarMinAnchorX, progressBarMaxAnchorX)
            : percent;

        var minAnchor = _rectTransform.anchorMin;
        minAnchor.x = starAnchorX;
        _rectTransform.anchorMin = minAnchor;

        var maxAnchor = _rectTransform.anchorMax;
        maxAnchor.x = starAnchorX;
        _rectTransform.anchorMax = maxAnchor;

        ShowItem();
    }

    private void ShowItem(bool isAnimated = true)
    {
        if (_image == null)
            return;

        _itemIsShown = true;
        _image.gameObject.SetActive(true);

        if (!isAnimated)
            return;

        var image = _image.GetComponent<Image>();
        if (image != null)
        {
            DOTween.defaultTimeScaleIndependent = true;
            _appearanceAnimation = image.DOFade(1f, 0.5f).From(0f);
            _appearanceAnimation.OnKill(() => image.SetAlpha(1f));
            DOTween.defaultTimeScaleIndependent = false;
        }
    }

    void AnimateAchieve()
    {
        _animation = transform.DOPunchScale(new Vector3(1.5f, 1.5f, 1.5f), 1f, vibrato: 4, elasticity: 0);
    }
}
