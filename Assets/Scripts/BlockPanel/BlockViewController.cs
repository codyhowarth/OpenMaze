using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UI;
using Trial=data2.Data.Trial;

namespace BlockPanel
{
	public class BlockViewController : MonoBehaviour
	{	

		public GameObject prefab;
		public RectTransform content;

		public List<ItemView> views = new List<ItemView>();


		private void Start()
		{
			;
		}
		
		// Remove Views that are marked for deletion
		private void Update()
		{
			foreach (var view in views)
			{
				// Remove if toRemove flag is true.
				if (!view.root.GetComponent<TrialPrefabController>().toRemove) continue;
				RemoveItemView(view);
			}
		}

		public void clearAll()
		{
			foreach (var view in views)
			{
				Destroy(view.root);
			}
			views.Clear();
		}

		public void addToBlock(Trial t)
		{
			Debug.Log("hi");
			var instance = Instantiate(prefab);
			instance.transform.SetParent(content, false);
			views.Add(initializeItemView(instance, t));
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
