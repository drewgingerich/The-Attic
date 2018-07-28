﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ItemDisplay : MonoBehaviour {

	public UnityEvent OnStart;
	public UnityEvent OnEnd;

	[SerializeField] DialogueCharacter character;
	[SerializeField] RawImage picture;
	[SerializeField] Text textBox;
	[SerializeField] float textSpeed = 0.03f;

	public void DisplayItem(Item item, System.Action callback) {
		StopAllCoroutines();
		gameObject.SetActive(true);
		picture.texture = item.picture;
		StartCoroutine(DisplayItemRoutine(item, callback));
	}

	IEnumerator DisplayItemRoutine(Item item, System.Action callback) {
		OnStart.Invoke();
		StringBuilder sb = new StringBuilder();
		yield return StartCoroutine(DisplayItemDescriptionRoutine(item.description, sb));
		yield return null;
		yield return StartCoroutine(DisplayItemTextRoutine(item.text, sb));
		callback();
		OnEnd.Invoke();
		yield return null;
		gameObject.SetActive(false);
	}

	IEnumerator DisplayItemDescriptionRoutine(string description, StringBuilder sb) {
		string charColor = ColorUtility.ToHtmlStringRGB(character.color);
		sb.AppendFormat("<i><color=#{0}></color><color=#000000ff>{1}</color></i>", charColor, description);

		int endTagLength = "</color></i>".Length;
		int visibleTextOffset = "</color><color=#000000ff>".Length;

		for (int i = 0; i < description.Length; i++) {
			int currentIndex = sb.Length - endTagLength - description.Length + i;
			sb.Remove(currentIndex, 1);
			sb.Insert(currentIndex - visibleTextOffset, description[i]);
			textBox.text = sb.ToString();
			yield return new WaitForSeconds(textSpeed);
		}

		while (!CheckForInterrupt()) {
			yield return null;
		}
	}

	IEnumerator DisplayItemTextRoutine(string text, StringBuilder sb) {
		sb.Append('\n');
		sb.Append('\n');
		sb.Append(text);
		textBox.text = sb.ToString();

		while (!CheckForInterrupt()) {
			yield return null;
		}
	}

	bool CheckForInterrupt() {
		return Input.GetMouseButtonUp(0)
			|| Input.GetKeyUp(KeyCode.Return)
			|| Input.GetKeyUp(KeyCode.Space)
			|| Input.GetKeyUp(KeyCode.DownArrow);
	}
}
