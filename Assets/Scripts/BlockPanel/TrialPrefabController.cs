using UnityEngine;
using UnityEngine.EventSystems;
using Image = UnityEngine.UI.Image;

namespace BlockPanel
{
    public class TrialPrefabController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public bool toRemove;

        private void Start()
        {
            toRemove = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var panel = GetComponent<Image>();
            panel.color = Color.gray;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var panel = GetComponent<Image>();
            panel.color = Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            toRemove = true;
        }
        
    }
}
