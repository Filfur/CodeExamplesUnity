using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[ExecuteInEditMode()]
public class ProgressBar : MonoBehaviour
{
    public float minimum;
    public float maximum;
    public float current;
    public bool isShowPercents;
    [SerializeField]
    private Image _mask;
    [SerializeField]
    private Image _fill;
    [SerializeField]
    private Text _percentsText;

    public delegate void ProgressAction(float value);
    public event ProgressAction OnProgressAnimated;
    public event ProgressAction OnProgress;

    public void Update()
    {
        if (_percentsText != null)
            _percentsText.gameObject.SetActive(isShowPercents);

#if UNITY_EDITOR
        UpdateCurrentFill();
#endif
    }

    public void SetCurrent(float newValue)
    {
        current = newValue;
        OnProgress?.Invoke(newValue);
        UpdateCurrentFill();
    }

    public virtual Tween AnimateCurrent(float newValue, float totalDuration)
    {
        var newCurrent = Mathf.Min(newValue, maximum);
        float duration = Mathf.Abs(newCurrent - current) / (maximum - minimum) * totalDuration;
        return DOTween.To((value) => {
            current = value;
            OnProgressAnimated?.Invoke(value);
            UpdateCurrentFill();
        }, current, newCurrent, duration).OnComplete(() => {
            current = newValue;
            OnProgressAnimated?.Invoke(newValue);
            UpdateCurrentFill();
        });
    }

    protected virtual void UpdateCurrentFill()
    {
        float currentOffset = current - minimum;
        float maxOffset = maximum - minimum;
        float progress = currentOffset / maxOffset;
        _mask.fillAmount = progress;
        _percentsText.text = $"{(int)(progress * 100)}%";
    }

    protected void TriggerOnProgressAnimated(float value)
    {
        OnProgressAnimated?.Invoke(value);
    }
}
