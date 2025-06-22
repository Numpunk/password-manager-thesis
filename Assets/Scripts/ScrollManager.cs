using UnityEngine;
using UnityEngine.UI;

public class ScrollManager : MonoBehaviour
{
    [SerializeField]
    private Transform passwordsList;
    [SerializeField]
    private float scrollSpeed = 400f;
    [SerializeField]
    private GameObject mainPage;

    [Space(10)]
    [SerializeField]
    private Scrollbar scrollbar;

    public float passwordsSpacing = 200;

    private float maxScrollY;
    private float minScrollY;

    public static ScrollManager instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        maxScrollY = 240;
        minScrollY = 240;
    }
    private void Update()
    {
        if (mainPage.activeInHierarchy)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                Vector3 newPos = passwordsList.localPosition;
                if (newPos.y - scrollSpeed < minScrollY) newPos.y = minScrollY;
                else newPos.y -= scrollSpeed;
                passwordsList.localPosition = newPos;

                if (scrollbar.interactable) scrollbar.value = (newPos.y - minScrollY) / (maxScrollY - minScrollY);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                Vector3 newPos = passwordsList.localPosition;
                if (newPos.y + scrollSpeed > maxScrollY) newPos.y = maxScrollY;
                else newPos.y += scrollSpeed;
                passwordsList.localPosition = newPos;

                if (scrollbar.interactable) scrollbar.value = (newPos.y - minScrollY) / (maxScrollY - minScrollY);
            }
        }
    }
    public void OnScrollValueChanged()
    {
        float scrollValue = scrollbar.value;
        Vector3 newPos = passwordsList.localPosition;
        newPos.y = (maxScrollY - minScrollY) * scrollValue + minScrollY;
        passwordsList.localPosition = newPos;
    }
    public void OnInstantiatedList(int entriesCount)
    {
        maxScrollY = (entriesCount * passwordsSpacing);
        if (maxScrollY - 2450 < minScrollY) maxScrollY = minScrollY;
        else maxScrollY -= 2450;

        if (passwordsList.localPosition.y > maxScrollY) passwordsList.localPosition = new Vector3(passwordsList.position.x, maxScrollY, passwordsList.position.z); //if previous scroll exceeds new passwords list (on delete)

        float visiblePasswordsHeight = passwordsList.parent.GetComponent<RectTransform>().rect.height;
        float allPasswordsHeight = entriesCount * passwordsSpacing;
        if (allPasswordsHeight > visiblePasswordsHeight)
        {
            scrollbar.size = visiblePasswordsHeight / allPasswordsHeight;
            scrollbar.interactable = true;
        }
        else
        {
            scrollbar.size = 1;
            scrollbar.interactable = false;
        }
    }
}
