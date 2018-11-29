﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Typewriter : MonoBehaviour {

	[SerializeField]
	private TypewriterAudio typewriterAudio;
	[SerializeField]
	private Text textBox;
	[SerializeField]
	private BlinkingGraphic lineFinishedIndicator;
	[SerializeField]
	private bool clearLines = false;
	[SerializeField]
	private bool clearOnFinish = false;
	[SerializeField]
	private int lineBreakSize = 2;

	private const float textSpeed = 1f;

	private TextFeed textFeed;

	private bool typing = false;
	private bool interrupt = false;

	public void Interrupt() {
		if (typing) {
			interrupt = true;
		}
	}

	public void TypeDialogue(Dialogue dialogue) {
		StartCoroutine(TypeDialogueRoutine(dialogue));
	}

	private void Awake() {
		textFeed = new TextFeed();
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Return)) {
			Interrupt();
		}
	}
	
	public IEnumerator TypeDialogueRoutine(Dialogue dialogue) {
		typing = true;
		textFeed.Clear();
		for (int i = 0; i < dialogue.Lines.Count; i++) {
			DialogueLine line = dialogue.Lines[i];
			yield return StartCoroutine(TypeLineRoutine(line));
			if (line.waitForInput) {
				yield return StartCoroutine(WaitForInterrupt());
			} else {
				yield return StartCoroutine(PauseAfterLine(line.finishPauseTime));
			}
			if (i < dialogue.Lines.Count - 1) {
				textFeed.AddLineBreak(lineBreakSize);
			}
		}
		if (clearOnFinish) {
			ClearText();
		}
		typing = false;
	}

	private IEnumerator TypeLineRoutine(DialogueLine line) {
		typewriterAudio.SetCharacter(line.character);

		if (clearLines) {
			textFeed.Clear();
		}

		textFeed.LoadLine(line);

		string displayText;
		while (textFeed.ShowNextChar(out displayText)) {
			if (interrupt) {
				interrupt = false;
				displayText = textFeed.ShowAllChars();
				DisplayText(displayText);
				break;
			} else {
				DisplayText(displayText);
				yield return new WaitForSeconds(line.textSpeed * textSpeed);
			}
		}
	}

	private IEnumerator WaitForInterrupt() {
		lineFinishedIndicator.StartBlinking();
		yield return new WaitForSeconds(0.5f);
		while (true) {
			if (interrupt) {
				interrupt = false;
				break;
			}
			yield return null;
		}
		lineFinishedIndicator.StopBlinking();
	}

	private IEnumerator PauseAfterLine(float pauseTime) {
		float timer = 0;
		while (timer < pauseTime) {
			if (interrupt) {
				interrupt = false;
				break;
			} else {
				timer += Time.deltaTime;
				yield return null;
			}
		}
	}

	void DisplayText(string text) {
		textBox.text = text;
		typewriterAudio.PlayClip();
	}

	public void ClearText() {
		textBox.text = "";
		textFeed.Clear();
	}
}
