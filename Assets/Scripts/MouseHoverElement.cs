using UnityEngine;

public class MouseHoverElement : MonoBehaviour
{
    [SerializeField]
    private Texture2D cursorLinkHoverTexture;
    [SerializeField]
    private Texture2D cursorLightBeamHoverTexture;
    [SerializeField]
    private Texture2D cursorDarkBeamHoverTexture;
    [SerializeField]
    private Vector2 linkHotSpot = new Vector2(6, 0);
    [SerializeField]
    private Vector2 beamHotSpot = new Vector2(0, 0);

    public static MouseHoverElement instance;
    private void Awake()
    {
        if(instance!=this) instance = this;
    }
    public void ChangeCursorToLink()
    {
        Cursor.SetCursor(cursorLinkHoverTexture, linkHotSpot, CursorMode.Auto);
    }
    public void ChangeCursorToLightBeam()
    {
        Cursor.SetCursor(cursorLightBeamHoverTexture, beamHotSpot, CursorMode.Auto);
    }
    public void ChangeCursorToDarkBeam()
    {
        Cursor.SetCursor(cursorDarkBeamHoverTexture, beamHotSpot, CursorMode.Auto);
    }
    public void ChangeCursorToDefault()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
