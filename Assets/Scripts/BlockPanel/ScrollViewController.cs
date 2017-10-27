using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using Trial=data2.Data.Trial;

public class ScrollViewController : MonoBehaviour
{

	public GameObject prefab;
	public Text text;
	public ScrollRect scrollView;
	public RectTransform content;
	
	public List<ItemView> views = new List<ItemView>();


	private void Start()
	{
		var t1 = new Trial() {Note = "a"};
		var t2 = new Trial() {Note = "b"};
		var t3 = new Trial() {Note = "c"};
		var arr = new Trial[3] {t1, t2, t3};
		views.Clear();
		foreach (var t in arr)
		{
			var instance = GameObject.Instantiate(prefab);
			instance.transform.SetParent(content, false);
			var view = initializeItemView(instance, t);
			views.Add(view);
		}
	}

	ItemView initializeItemView(GameObject viewGameObject, Trial t)
	{
		ItemView view = new ItemView(viewGameObject.transform);
		view.text.text = t.Note;
		return view;
	}

	public class ItemView
	{
		public Text text;
		public ItemView(Transform root)
		{
			text = root.Find("Text").GetComponent<Text>();
		}
	}
}
