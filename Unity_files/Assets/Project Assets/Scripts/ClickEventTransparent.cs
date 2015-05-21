using UnityEngine;
 
public class ClickEventTransparent : MonoBehaviour, ICanvasRaycastFilter {
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera) {
        return false;
    }
}