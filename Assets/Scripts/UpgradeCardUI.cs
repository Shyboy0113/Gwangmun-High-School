using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 강화 카드 한 장의 표시와 클릭 처리. (배포본 — 학생은 안 건드린다)
/// </summary>
public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button button;

    private UpgradeData data;
    private UpgradeManager manager;

    public void Show(UpgradeData upgradeData, UpgradeManager upgradeManager)
    {
        data = upgradeData;
        manager = upgradeManager;

        if (nameText != null) nameText.text = data.upgradeName;
        if (descriptionText != null) descriptionText.text = data.description;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = data.icon != null;
        }

        if (button == null) button = GetComponent<Button>();

        if (button != null)
        {
            // 카드가 재사용되므로 이전 판의 리스너가 남아 있으면
            // 한 번 클릭에 여러 강화가 먹는다.
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        if (manager != null && data != null) manager.ApplyUpgrade(data);
    }
}
