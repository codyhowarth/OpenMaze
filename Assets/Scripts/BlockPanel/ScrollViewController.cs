using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Trial=data2.Data.Trial;

namespace BlockPanel
{
	public class ScrollViewController : MonoBehaviour
	{	

		public GameObject prefab;
		public RectTransform content;
		public List<ItemView> views = new List<ItemView>();
		public RectTransform BlockPanel;


		private void Start()
		{
			var t1 = new Trial() {Note = "Trial 1"};
			var t2 = new Trial() {Note = "Trial 2"};
			var t3 = new Trial() {Note = "Trial 3"};
			var t4 = new Trial() {Note = "Trial 4"};
			var t5 = new Trial() {Note = "Trial 5"};
			var t6 = new Trial() {Note = "Trial 6"};
			var t7 = new Trial() {Note = "Trial 7"};
			var t8 = new Trial() {Note = "Trial 8"};
			
			var arr = new Trial[8] {t1, t2, t3, t4, t5, t6, t7, t8};
			views.Clear();
			foreach (var t in arr)
			{
				var instance = Instantiate(prefab);
				instance.transform.SetParent(content, false);
				var view = initializeItemView(instance, t);
				views.Add(view);
			}
		}
		
		// Remove Views that are marked for deletion
		private void Update()
		{
			foreach (var view in views)
			{
				// Remove if toRemove flag is true.
				if (!view.root.GetComponent<TrialPrefabController>().toRemove) continue;
				// RemoveItemView(view);
				BlockPanel.GetComponent<BlockViewController>().addToBlock(view.t);
				break;
			}
		}

		void RemoveItemView(ItemView view)
		{
			Destroy(view.root);
			views.Remove(view);
		}

		ItemView initializeItemView(GameObject viewGameObject, Trial t)
		{
			var view = new ItemView(viewGameObject.transform);
			view.text.text = t.Note;
			view.t = t;
			return view;
		}

		public class ItemView
		{
			public Trial t;
			public Text text;
			public GameObject root;
			public ItemView(Transform root)
			{
				text = root.Find("note").GetComponent<Text>();
				this.root = root.gameObject;	
			}
		}
	}
}
