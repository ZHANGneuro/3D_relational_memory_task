
using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.UIElements;
using UnityStandardAssets.Characters.FirstPerson;
using System.Diagnostics;
using System.Threading;
using TMPro;
using System.Runtime.InteropServices;

public class task_main : MonoBehaviour
{
	// bool is_meg = true;
	// [DllImport("inpoutx64.dll")]
	// private static extern UInt32 IsInpOutDriverOpen();

	// [DllImport("inpoutx64.dll")]
	// private static extern void Out32(int PortAddress, int Data);

	// int PortAddress = Convert.ToInt32("D020", 16);

	public static string subject_ID = "";
	public static string subject_session = "";

	public static string output_beh_path;
	public static string output_timing_path;
	public static string output_millisecondRecord_path;

	public Camera MainCamera;
	public Camera temp_camera;
	public GameObject character_object_c;
	public GameObject character_object_i;
	public Texture2D mouseCursor;

	static float reference_coordinate_x = 100.0f;
	static float reference_coordinate_z = 100.0f;
	static float reference_coordinate_y = 21.8f;

	// mouselook
	public float mouseSensitivity = 150.0f; //100.0f

	private float rotY; // rotation around the up/y axis
	private float rotX; // rotation around the right/x axis

	public static Vector3 subject_position = new Vector3(reference_coordinate_x, reference_coordinate_y, reference_coordinate_z);
	public static Vector3 hidden_position = new Vector3(0, 0, 0);

	public static string[][] reorganized_DataSet;
	int trial_ith = 0;
	List<int> trial_order = new List<int>();

	Camera m_MainCamera;
	Camera temp_camera_copy;

	GameObject canvasobject;
	GameObject canvas_fixation;
	GameObject textObject;
	GameObject clicked_object;

	bool exp_information_input = false;
	bool isOn_expStart_screen = false;
	bool trial_running = false;
	bool shown_warning = false;
	bool mouse_clicked = false;

	Vector3 character_worldPos;
	Vector3 screenPos_target;
	Vector3 screenPos_lookingat;
	Vector3 character_originPos;
	Vector3 object1_3dPos;
	Vector3 object2_3dPos;
	Vector3 object3_3dPos;
	Vector3 object4_3dPos;

	List<string> list_object_highlighted;
	List<GameObject> list_object_presented;
	List<string> list_object_backend;

	List<int> list_angles;

	List<string> output_beh;
	List<string> output_timing;
	List<string> output_millisecondRecord = new List<string>();

	Color32 backgroundColor = new Color32(192, 192, 192, 160);
	Color fontColor = Color.black;

	float distance_camera_and_character = 10.0f;

	CursorMode cursorMode = CursorMode.ForceSoftware;// CursorMode.Auto;

	UnityEngine.UI.Image InstructionScreen;

	int num_of_trial;
	float sub_fee = 0.0f;
	string object_should_be;
	string object_id_clicked;
	string[] cur_sequence;

	Stopwatch exp_time = new Stopwatch();

	void Start()
	{
		Screen.SetResolution(1024, 768, true, 60);
		m_MainCamera = Instantiate(MainCamera, subject_position, Quaternion.Euler(0, 0, 0)); //
		temp_camera_copy = Instantiate(temp_camera, hidden_position, Quaternion.Euler(0, 0, 0));
		m_MainCamera.enabled = true;

		// if (is_meg)
		// {
		// 	// var aaa = IsInpOutDriverOpen();
		// 	// UnityEngine.Debug.LogError("address = " + PortAddress.ToString());
		// }

		Application.targetFrameRate = 50;
		Vector2 hotSpot = new Vector2(mouseCursor.width / 2, mouseCursor.width / 2);
		UnityEngine.Cursor.SetCursor(mouseCursor, hotSpot, cursorMode);
		UnityEngine.Cursor.lockState = CursorLockMode.None;
		exp_information_input = true;
	}
	void OnGUI()
	{
		if (exp_information_input)
		{
			GUI.Window(0, new Rect(Screen.width / 2 - 500 / 2, Screen.height / 5, 500, 500), ShowGUI, " ");
		}
	}
	void ShowGUI(int windowID)
	{
		GUI.skin.textField.fontSize = 40;
		var Gui_subNum = GUI.skin.GetStyle("Label");
		Gui_subNum.alignment = TextAnchor.MiddleCenter;
		Gui_subNum.fontSize = 40;
		GUI.Label(new Rect(25, 100, 120, 50), "被试号", Gui_subNum);
		subject_ID = GUI.TextField(new Rect(150, 100, 200, 50), subject_ID, 5);
		var Gui_session = GUI.skin.GetStyle("Label");
		Gui_session.alignment = TextAnchor.MiddleCenter;
		Gui_session.fontSize = 40;
		GUI.Label(new Rect(25, 250, 120, 50), "Sess", Gui_session);
		subject_session = GUI.TextField(new Rect(150, 250, 280, 50), subject_session, 12);
		GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
		myButtonStyle.fontSize = 40;
		if (shown_warning)
		{
			var give_warning = GUI.skin.GetStyle("Label");
			give_warning.alignment = TextAnchor.MiddleCenter;
			give_warning.fontSize = 30;
			GUI.Label(new Rect((500 - 350) / 2, 320, 350, 50), "请输入session，数字1-5", give_warning);
		}
		if (GUI.Button(new Rect((500 - 150) / 2, 400, 150, 50), "OK", myButtonStyle))
		{
			if (subject_session == "")
			{
				shown_warning = true;
			}
			else
			{
				exp_information_input = false;
				output_beh_path = Application.dataPath + "/sub" + subject_ID + "_sess" + subject_session + "_beh.txt";
				output_timing_path = Application.dataPath + "/sub" + subject_ID + "_sess" + subject_session + "_timing.txt";
				output_millisecondRecord_path = Application.dataPath + "/sub" + subject_ID + "_sess" + subject_session + "_millisecondRecord.txt";
				Invoke("showcase_formal", 0f);
			}
		}
	}

	void showcase_formal()
	{
		task_create_trialTable create_trial_table = temp_camera_copy.GetComponent<task_create_trialTable>();
		// get stimuli set
		create_trial_table.create_trial_table();
		// trial order
		num_of_trial = reorganized_DataSet.Length;
		cur_sequence = reorganized_DataSet[0];

		List<int> temp_list = new List<int>();
		temp_list = Enumerable.Range(0, reorganized_DataSet.Length).ToList();
		trial_order = new task_utility().ShuffleList(temp_list);
		getMessageScreen();
		showInstruction("记住下方符号的顺序", 300, -50, 50);
		showInstruction("准备好后，按键5次提示开始", 250, -150, 50);
		show_sequence();
		isOn_expStart_screen = true;
	}

	void show_fixation()
	{
		UnityEngine.Cursor.visible = false;

		getMessageScreen();
		canvas_fixation = new GameObject("Canvas");
		canvas_fixation.AddComponent<Canvas>();
		// canvas2.renderMode = RenderMode.WorldSpace;

		var image_object = canvas_fixation.AddComponent<UnityEngine.UI.Image>();
		image_object.transform.SetParent(InstructionScreen.transform);
		image_object.rectTransform.sizeDelta = new Vector2(60, 60);
		image_object.rectTransform.anchoredPosition = new Vector3(0, 0, 0);
		Texture2D fix_png = (Texture2D)Resources.Load("fixation");
		Sprite fix = Sprite.Create(fix_png, new Rect(0.0f, 0.0f, 128, 128), new Vector2(0.2f, 0.2f));
		image_object.sprite = fix;

		//clear records
		list_object_presented = new List<GameObject>();
		list_object_backend = new List<string>();
		list_object_highlighted = new List<string>();
		list_angles = new List<int>();
		output_beh = new List<string>();
		output_timing = new List<string>();

		// if (is_meg)
		// {
		// 	Out32(PortAddress, 5);
		// }
		output_timing.Add(exp_time.ElapsedMilliseconds.ToString() + "\t");
		Invoke("draw_character", 1.0f);
	}


	void draw_character()
	{
		Destroy(canvasobject);
		Destroy(canvas_fixation);

		character_originPos = new task_utility().get_loc_1st_char1(distance_camera_and_character, reference_coordinate_x, reference_coordinate_y, reference_coordinate_z);

		// ScreenCapture.CaptureScreenshot("/Users/bo/Desktop/0.png");

		// organize and show predefined-characters
		cur_sequence = reorganized_DataSet[trial_order[trial_ith]];
		output_beh.Add(cur_sequence[0] + "\t");
		output_beh.Add(cur_sequence[1] + "\t");
		output_beh.Add(cur_sequence[2] + "\t");
		output_beh.Add(cur_sequence[3] + "\t");

		list_object_backend.Add(cur_sequence[0]);
		list_object_backend.Add(cur_sequence[1]);
		list_object_backend.Add(cur_sequence[2]);
		list_object_backend.Add(cur_sequence[3]);

		output_beh.Add(cur_sequence[4] + "\t");
		output_beh.Add(cur_sequence[5] + "\t");
		output_beh.Add(cur_sequence[6] + "\t");
		list_angles.Add(Int32.Parse(cur_sequence[4]));
		list_angles.Add(Int32.Parse(cur_sequence[5]));
		list_angles.Add(Int32.Parse(cur_sequence[6]));

		// place character
		m_MainCamera.enabled = false;
		m_MainCamera.transform.position = hidden_position;
		temp_camera_copy.transform.position = subject_position;
		for (int ith_character = 0; ith_character < list_object_backend.Count; ith_character++)
		{
			if (ith_character == 0)
			{
				GameObject cur_object = showObject(character_originPos, list_object_backend[ith_character], ith_character + 1);
				list_object_presented.Add(cur_object);
				object1_3dPos = cur_object.transform.position;
			}
			else
			{
				calculate_new_char_position(list_angles[ith_character - 1], list_object_presented[ith_character - 1].transform.position);
				GameObject cur_object = showObject(character_worldPos, list_object_backend[ith_character], ith_character + 1);
				list_object_presented.Add(cur_object);
				if (ith_character == 1)
				{
					object2_3dPos = cur_object.transform.position;
				}
				if (ith_character == 2)
				{
					object3_3dPos = cur_object.transform.position;
				}
				if (ith_character == 3)
				{
					object4_3dPos = cur_object.transform.position;
				}
			}
		}
		Invoke("motion_start", 0.0f);
	}

	void motion_start()
	{
		// place characters finished, make temp_camera off, and camera on
		temp_camera_copy.enabled = false;
		temp_camera_copy.transform.position = hidden_position;

		m_MainCamera.enabled = true;
		m_MainCamera.transform.position = subject_position;

		m_MainCamera.transform.forward = (character_originPos - subject_position).normalized;

		UnityEngine.Cursor.lockState = CursorLockMode.Locked;

		output_timing.Add(exp_time.ElapsedMilliseconds.ToString() + "\t");

		// if (is_meg)
		// {
		// 	Out32(PortAddress, 10);
		// }
		// Vector3 rot = m_MainCamera.transform.rotation;
		rotX = m_MainCamera.transform.localRotation.eulerAngles.x;
		rotY = m_MainCamera.transform.localRotation.eulerAngles.y;
		trial_running = true;
	}

	void Update()
	{

		if (isOn_expStart_screen)
		{
			UnityEngine.Cursor.visible = false;
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				isOn_expStart_screen = false;
				Destroy(canvasobject);
				Destroy(textObject);
				exp_time.Reset();
				exp_time.Start();
				// if (is_meg)
				// {
				// 	Out32(PortAddress, 0);
				// }
				show_fixation();
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//Cursor.lockState = CursorLockMode.None;
			//Cursor.visible = true;
			UnityEngine.Debug.Log("quit");
			Application.Quit();
		}

		if (trial_running)
		{
			UnityEngine.Cursor.visible = true;

			float mouseX = Input.GetAxis("Mouse X");
			float mouseY = -Input.GetAxis("Mouse Y");
			rotX += mouseY * mouseSensitivity * Time.deltaTime;
			rotY += mouseX * mouseSensitivity * Time.deltaTime;

			// output
			output_millisecondRecord.Add(trial_ith.ToString() + "\t");
			output_millisecondRecord.Add(exp_time.ElapsedMilliseconds.ToString() + "\t");
			output_millisecondRecord.Add(m_MainCamera.transform.position.x.ToString() + "\t");
			output_millisecondRecord.Add(m_MainCamera.transform.position.y.ToString() + "\t");
			output_millisecondRecord.Add(m_MainCamera.transform.position.z.ToString() + "\t");
			output_millisecondRecord.Add(object1_3dPos.x.ToString() + "\t");
			output_millisecondRecord.Add(object1_3dPos.y.ToString() + "\t");
			output_millisecondRecord.Add(object1_3dPos.z.ToString() + "\t");
			output_millisecondRecord.Add(object2_3dPos.x.ToString() + "\t");
			output_millisecondRecord.Add(object2_3dPos.y.ToString() + "\t");
			output_millisecondRecord.Add(object2_3dPos.z.ToString() + "\t");
			output_millisecondRecord.Add(object3_3dPos.x.ToString() + "\t");
			output_millisecondRecord.Add(object3_3dPos.y.ToString() + "\t");
			output_millisecondRecord.Add(object3_3dPos.z.ToString() + "\t");
			output_millisecondRecord.Add(object4_3dPos.x.ToString() + "\t");
			output_millisecondRecord.Add(object4_3dPos.y.ToString() + "\t");
			output_millisecondRecord.Add(object4_3dPos.z.ToString() + "\t");
			output_millisecondRecord.Add((rotX % 360).ToString() + "\t");
			output_millisecondRecord.Add((rotY % 360).ToString() + "\t");
			output_millisecondRecord.Add(mouse_clicked.ToString() + Environment.NewLine);

			mouse_clicked = false;
			m_MainCamera.transform.localRotation = Quaternion.Euler(rotX, rotY, 0.0f); ;

			RaycastHit hit;
			int layerMask = 1 << 8;
			Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);

			if ((Input.GetMouseButtonDown(0) | Input.GetMouseButtonDown(1) | Input.GetMouseButtonDown(2)))
			{
				// if (is_meg)
				// {
				// 	Out32(PortAddress, 55);
				// }
			}


			if ((Input.GetMouseButtonDown(0) | Input.GetMouseButtonDown(1) | Input.GetMouseButtonDown(2)) && Physics.Raycast(ray, out hit, Mathf.Infinity, ~layerMask) && GameObject.Find(hit.collider.name).tag != "clicked")
			{
				mouse_clicked = true;
				object_id_clicked = hit.collider.name;

				clicked_object = GameObject.Find(hit.collider.name);
				clicked_object.tag = "clicked";

				if (list_object_highlighted.Count == 0)
				{
					object_should_be = list_object_backend[0];

					output_timing.Add(exp_time.ElapsedMilliseconds.ToString() + "\t");
					if (clicked_object.name == object_should_be)
					{
						sub_fee = sub_fee + 0.1f;
						list_object_highlighted.Add(hit.collider.name);
						drawLine();
						// if (is_meg)
						// {
						// 	Out32(PortAddress, 15);
						// }
						update_halo("c");
						// ScreenCapture.CaptureScreenshot("/Users/bo/Desktop/1.png");
					}
					else
					{
						output_beh.Add("finish-0\t");
						output_beh.Add("fee-" + sub_fee + Environment.NewLine);
						output_timing.Add("NA" + "\t");
						output_timing.Add("NA" + "\t");
						output_timing.Add("NA" + Environment.NewLine);
						// if (is_meg)
						// {
						// 	Out32(PortAddress, 35);
						// }
						update_halo("i");
					}
				}
				else if (list_object_highlighted.Count == 1)
				{
					object_should_be = list_object_backend[1];
					output_timing.Add(exp_time.ElapsedMilliseconds.ToString() + "\t");
					if (clicked_object.name == object_should_be)
					{
						sub_fee = sub_fee + 0.1f;
						list_object_highlighted.Add(hit.collider.name);
						drawLine();
						// if (is_meg)
						// {
						// 	Out32(PortAddress, 20);
						// }
						update_halo("c");
						// ScreenCapture.CaptureScreenshot("/Users/bo/Desktop/2.png");
					}
					else
					{
						output_beh.Add("finish-1\t");
						output_beh.Add("fee-" + sub_fee + Environment.NewLine);
						output_timing.Add("NA" + "\t");
						output_timing.Add("NA" + Environment.NewLine);
						// if (is_meg)
						// {
						// 	Out32(PortAddress, 40);
						// }
						update_halo("i");
					}
				}
				else if (list_object_highlighted.Count == 2)
				{
					object_should_be = list_object_backend[2];
					output_timing.Add(exp_time.ElapsedMilliseconds.ToString() + "\t");
					if (clicked_object.name == object_should_be)
					{
						sub_fee = sub_fee + 0.1f;
						list_object_highlighted.Add(hit.collider.name);
						drawLine();
						// if (is_meg)
						// {
						// 	Out32(PortAddress, 25);
						// }
						update_halo("c");
						// ScreenCapture.CaptureScreenshot("/Users/bo/Desktop/3.png");
					}
					else
					{
						output_beh.Add("finish-2\t");
						output_beh.Add("fee-" + sub_fee + Environment.NewLine);
						output_timing.Add("NA" + Environment.NewLine);
						// if (is_meg)
						// {
						// 	Out32(PortAddress, 45);
						// }
						update_halo("i");
					}
				}
				else if (list_object_highlighted.Count == 3)
				{
					object_should_be = list_object_backend[3];
					output_timing.Add(exp_time.ElapsedMilliseconds.ToString() + Environment.NewLine);
					if (clicked_object.name == object_should_be)
					{
						trial_running = false;
						sub_fee = sub_fee + 0.1f;
						list_object_highlighted.Add(hit.collider.name);
						drawLine();
						output_beh.Add("finish-4\t");
						output_beh.Add("fee-" + sub_fee + Environment.NewLine);
						// if (is_meg)
						// {
						// 	Out32(PortAddress, 30);
						// }
						update_halo("c");
						// ScreenCapture.CaptureScreenshot("/Users/bo/Desktop/4.png");
						Invoke("go_next_trial", 0.5f);
					}
					else
					{
						output_beh.Add("finish-3\t");
						output_beh.Add("fee-" + sub_fee + Environment.NewLine);
						// if (is_meg)
						// {
						// 	Out32(PortAddress, 50);
						// }
						update_halo("i");
					}

				}

			}
		}

	}

	void go_next_trial()
	{
		for (int ith_obj = 0; ith_obj < list_object_presented.Count; ith_obj++)
		{
			Destroy(list_object_presented[ith_obj]);
		}
		LineRenderer lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.positionCount = 0;
		write_to_text();
	}

	void update_halo(string type)
	{
		if (type == "c")
		{
			Component halo = clicked_object.GetComponent("Halo");
			halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
			// halo_state set_halo_state = clicked_object.GetComponent<halo_state>();
			// set_halo_state.cur_halo_state = true;
		}
		if (type == "i")
		{
			trial_running = false;
			Destroy(list_object_presented[Int32.Parse(object_id_clicked) - 1]);
			int stimulus_order = list_object_backend.IndexOf(object_id_clicked) + 1;
			clicked_object = updateObject(clicked_object.transform.position, object_id_clicked, stimulus_order);
			clicked_object.tag = "clicked";
			Component halo = clicked_object.GetComponent("Halo");
			halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
			list_object_presented.Add(clicked_object);
			// Invoke("go_next_trial", 0.5f);
		}

	}


	// ---------------------------------------------------------------------------------------------------
	// ---------------------------------------------------------------------------------------------------
	void calculate_new_char_position(int cur_angle, Vector3 object_Pos)
	{
		temp_camera_copy.transform.forward = (object_Pos - subject_position).normalized;
		screenPos_lookingat = temp_camera_copy.WorldToScreenPoint(object_Pos);

		float distance_among_characters = 300.0f;

		while (true)
		{

			double iter_x = Math.Cos((Math.PI / 180) * cur_angle) * distance_among_characters;
			double iter_y = Math.Sin((Math.PI / 180) * cur_angle) * distance_among_characters;

			screenPos_target = new Vector3(screenPos_lookingat[0] + (float)iter_x, screenPos_lookingat[1] + (float)iter_y, 0f);

			Ray ray = temp_camera_copy.ScreenPointToRay(screenPos_target);
			character_worldPos = ray.GetPoint(distance_camera_and_character);

			List<float> temp_list = new List<float>();
			for (int ith_placed_object = 0; ith_placed_object < list_object_presented.Count; ith_placed_object++)
			{
				float mag_dif = Vector3.Distance(list_object_presented[ith_placed_object].transform.position, character_worldPos);
				temp_list.Add(mag_dif);
			}

			bool checker = temp_list.Any(x => x <= 5.0f);
			if (checker)
			{
				distance_among_characters = distance_among_characters + 5.0f;
			}
			else
			{
				break;
			}
		}
	}

	GameObject showObject(Vector3 stimuli_position, string character_identity, int stimulus_order)
	{
		GameObject cur_object = Instantiate(character_object_c, hidden_position, Quaternion.Euler(0, 0, 0));
		cur_object.name = character_identity;

		cur_object.transform.position = stimuli_position;
		cur_object.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
		cur_object.transform.forward = (stimuli_position - subject_position).normalized;
		UnityEngine.UI.Image IMAGE_component;

		cur_object.AddComponent<Canvas>();
		IMAGE_component = cur_object.AddComponent<UnityEngine.UI.Image>();

		// add halo effect
		Component halo = cur_object.GetComponent("Halo");
		halo.GetType().GetProperty("enabled").SetValue(halo, false, null);

		// pick character
		string cur_path = "sub_" + subject_ID + "_sess_" + subject_session + "_order_" + stimulus_order + "_stimulus_" + character_identity;
		Texture2D char_image = (Texture2D)Resources.Load(cur_path);
		Rect rect = new Rect(0f, 0f, 128f, 128f);
		Sprite icon = Sprite.Create(char_image, rect, new Vector2(0.8f, 0.8f));
		IMAGE_component.sprite = icon;
		var sprite_width = IMAGE_component.sprite.rect.width;

		// add collider
		BoxCollider cur_boxCollider = cur_object.GetComponent<BoxCollider>();
		cur_boxCollider.center = hidden_position;
		cur_boxCollider.size = new Vector3(sprite_width, sprite_width, sprite_width);
		return cur_object;
	}

	GameObject updateObject(Vector3 stimuli_position, string character_identity, int stimulus_order)
	{
		GameObject cur_object = Instantiate(character_object_i, hidden_position, Quaternion.Euler(0, 0, 0));
		cur_object.name = character_identity;

		cur_object.transform.position = stimuli_position;
		cur_object.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
		cur_object.transform.forward = (stimuli_position - subject_position).normalized;
		UnityEngine.UI.Image IMAGE_component;

		cur_object.AddComponent<Canvas>();
		IMAGE_component = cur_object.AddComponent<UnityEngine.UI.Image>();

		// add halo effect
		Component halo = cur_object.GetComponent("Halo");
		halo.GetType().GetProperty("enabled").SetValue(halo, false, null);

		// pick character
		Texture2D char_image = (Texture2D)Resources.Load("sub_" + subject_ID + "_sess_" + subject_session + "_order_" + stimulus_order + "_stimulus_" + character_identity);
		Rect rect = new Rect(0f, 0f, 128f, 128f);
		Sprite icon = Sprite.Create(char_image, rect, new Vector2(0.8f, 0.8f));
		IMAGE_component.sprite = icon;
		var sprite_width = IMAGE_component.sprite.rect.width;

		// add collider
		BoxCollider cur_boxCollider = cur_object.GetComponent<BoxCollider>();
		cur_boxCollider.center = hidden_position;
		cur_boxCollider.size = new Vector3(sprite_width, sprite_width, sprite_width);
		return cur_object;
	}


	void drawLine()
	{
		if (list_object_highlighted.Count >= 2)
		{
			LineRenderer lineRenderer = GetComponent<LineRenderer>();
			Vector3[] positions = new Vector3[list_object_highlighted.Count];
			lineRenderer.positionCount = list_object_highlighted.Count;
			for (int ith_pos = 0; ith_pos < list_object_highlighted.Count; ith_pos++)
			{
				GameObject cur_obj = GameObject.Find(list_object_highlighted[ith_pos]);
				positions[ith_pos] = cur_obj.transform.localPosition;
				lineRenderer.SetPosition(ith_pos, cur_obj.transform.localPosition);
			}
		}
	}


	void getMessageScreen()
	{
		canvasobject = new GameObject("Canvas");
		Canvas cur_canvas = canvasobject.AddComponent<Canvas>();
		cur_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		InstructionScreen = canvasobject.AddComponent<UnityEngine.UI.Image>();
		InstructionScreen.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
		InstructionScreen.rectTransform.anchoredPosition = Vector3.zero;
		InstructionScreen.color = backgroundColor;
	}
	void showInstruction(string inputText, int inputXposition, int inputYposition, int inputFontSize)
	{
		textObject = new GameObject("Text");
		textObject.transform.SetParent(InstructionScreen.transform);
		var text = textObject.AddComponent<Text>();
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		text.color = fontColor;
		text.text = inputText;
		RectTransform text_size = text.GetComponent<RectTransform>();
		text_size.sizeDelta = new Vector2(1024, 768);
		text_size.anchoredPosition = new Vector3(inputXposition, inputYposition, 0);
		text.fontSize = inputFontSize;
	}

	void show_sequence()
	{
		GameObject cur_canvas1 = new GameObject("Canvas");
		var image_object1 = cur_canvas1.AddComponent<UnityEngine.UI.Image>();
		image_object1.transform.SetParent(InstructionScreen.transform);
		image_object1.rectTransform.sizeDelta = new Vector2(150, 150);
		image_object1.rectTransform.anchoredPosition = new Vector2(-300, 0);
		Texture2D stimuli_1 = (Texture2D)Resources.Load("sub_" + subject_ID + "_sess_" + subject_session + "_order_1_stimulus_" + cur_sequence[0]);
		image_object1.sprite = Sprite.Create(stimuli_1, new Rect(0.0f, 0.0f, 128, 128), new Vector2(1f, 1f));

		GameObject cur_canvas2 = new GameObject("Canvas");
		var image_object2 = cur_canvas2.AddComponent<UnityEngine.UI.Image>();
		image_object2.transform.SetParent(InstructionScreen.transform);
		image_object2.rectTransform.sizeDelta = new Vector2(150, 150);
		image_object2.rectTransform.anchoredPosition = new Vector2(-100, 0);
		Texture2D stimuli_2 = (Texture2D)Resources.Load("sub_" + subject_ID + "_sess_" + subject_session + "_order_2_stimulus_" + cur_sequence[1]);
		image_object2.sprite = Sprite.Create(stimuli_2, new Rect(0.0f, 0.0f, 128, 128), new Vector2(1f, 1f));

		GameObject cur_canvas3 = new GameObject("Canvas");
		var image_object3 = cur_canvas3.AddComponent<UnityEngine.UI.Image>();
		image_object3.transform.SetParent(InstructionScreen.transform);
		image_object3.rectTransform.sizeDelta = new Vector2(150, 150);
		image_object3.rectTransform.anchoredPosition = new Vector2(100, 0);
		Texture2D stimuli_3 = (Texture2D)Resources.Load("sub_" + subject_ID + "_sess_" + subject_session + "_order_3_stimulus_" + cur_sequence[2]);
		image_object3.sprite = Sprite.Create(stimuli_3, new Rect(0.0f, 0.0f, 128, 128), new Vector2(1f, 1f));

		GameObject cur_canvas4 = new GameObject("Canvas");
		var image_object4 = cur_canvas4.AddComponent<UnityEngine.UI.Image>();
		image_object4.transform.SetParent(InstructionScreen.transform);
		image_object4.rectTransform.sizeDelta = new Vector2(150, 150);
		image_object4.rectTransform.anchoredPosition = new Vector2(300, 0);
		Texture2D stimuli_4 = (Texture2D)Resources.Load("sub_" + subject_ID + "_sess_" + subject_session + "_order_4_stimulus_" + cur_sequence[3]);
		image_object4.sprite = Sprite.Create(stimuli_4, new Rect(0.0f, 0.0f, 128, 128), new Vector2(1f, 1f));

	}


	void write_to_text()
	{
		using (StreamWriter writer = new StreamWriter(output_beh_path, true))
		{
			for (int i = 0; i < output_beh.Count; i++)
			{
				writer.Write(output_beh.ElementAt(i));
			}
			writer.Close();
		}
		using (StreamWriter writer = new StreamWriter(output_timing_path, true))
		{
			for (int i = 0; i < output_timing.Count; i++)
			{
				writer.Write(output_timing.ElementAt(i));
			}
			writer.Close();
		}
		using (StreamWriter writer = new StreamWriter(output_millisecondRecord_path, true))
		{
			for (int i = 0; i < output_millisecondRecord.Count; i++)
			{
				writer.Write(output_millisecondRecord.ElementAt(i));
			}
			writer.Close();
		}
		trial_ith = trial_ith + 1;

		if (trial_ith < num_of_trial)
		{
			// Invoke("show_fixation", 0);
		}
		else if (trial_ith == num_of_trial)
		{
			thank_you();
		}
	}

	void thank_you()
	{
		Destroy(canvasobject);
		UnityEngine.Cursor.visible = true;
		getMessageScreen();
		showInstruction("请不要动，等待主试指导", 250, -150, 50);
		exp_time.Stop();
	}

}
