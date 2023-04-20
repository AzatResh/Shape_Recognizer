using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;

using PDollarGestureRecognizer;

public class ShapeRecognizer : MonoBehaviour
{
	[Header ("UI")]
	[SerializeField] private RectTransform drawArea;
	[SerializeField] private TextMeshProUGUI message;

	[SerializeField] private Image answerImage; // картинка где высвечивается ответ

	[Header ("Prefabs")]
    [SerializeField] private LineRenderer gestureOnScreenPrefab;

	private int vertexCount = 0;
	private int strokeId = -1;

	private Vector3 virtualKeyPosition = Vector2.zero;
	private Rect onScreenRect;
	private LineRenderer currentGestureLineRenderer;

	private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
	private List<Gesture> trainingSet = new List<Gesture>();
	private List<Point> points = new List<Point>();

	private bool recognized;

	private void Start () {
		//загрузить xml-модели с основной папки
		TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("ResourcesOfShapes/");
		foreach (TextAsset gestureXml in gesturesXml){
			trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));}

		//загрузить xml-модели с другой папки
		/*string[] filePaths = Directory.GetFiles((Path.Combine( Application.dataPath, "../Assets/Resources/GestureSet")).ToString(), "*.xml");
		foreach (string filePath in filePaths)
			trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
        */
	}

	private void Update () {

		virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);

		onScreenRect = drawArea.rect;
		onScreenRect.Set(onScreenRect.x + onScreenRect.width/2, onScreenRect.y + onScreenRect.height/2, onScreenRect.width, onScreenRect.height);

		if (onScreenRect.Contains(virtualKeyPosition)) {
			if (Input.GetMouseButtonDown(0)) { //создание LineRenderer для рисования при нажатии
				if (recognized) {
					ClearDrawing(); // после проверки очистить
				}
				++strokeId;

				LineRenderer tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation);
				currentGestureLineRenderer = tmpGesture;
				
				gestureLinesRenderer.Add(currentGestureLineRenderer);
				vertexCount = 0;
			}
			
			if (Input.GetMouseButton(0)) { //рисование в drawArea
				points.Add(new Point(virtualKeyPosition.x, -virtualKeyPosition.y, strokeId));
				currentGestureLineRenderer.positionCount = (++vertexCount);
				currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));
			}
		}
	}

	public void CheckDrawing(){
		recognized = true;

		if(points.Count == 0) {message.text = ""; return;}

		Gesture candidate = new Gesture(points.ToArray());
		Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

		if(gestureResult.Score<0.8f || gestureResult.GestureClass=="null") {
			message.text = "неправильно";
			answerImage.color = Color.red; 
			}
		else {
			if(gestureResult.GestureClass == "true") message.text = "правильно";
			else message.text = gestureResult.GestureClass;
			answerImage.color = Color.green; 
			}
	}

	public void ClearDrawing(){
		answerImage.color = Color.gray;
		recognized = false;
		strokeId = -1;
		points.Clear();

		foreach (LineRenderer lineRenderer in gestureLinesRenderer) {
			lineRenderer.positionCount = 0;
			Destroy(lineRenderer.gameObject);
		}
		gestureLinesRenderer.Clear();
		message.text = "";
	}

	// метод для добавления новой модели
	/*
	public void AddNewGesture(){
		string fileName = String.Format("{0}/{1}-{2}.xml", (Path.Combine( Application.dataPath, "../Assets/Resources/ResourcesOfShapes/")), inputField.text, DateTime.Now.ToFileTime());
		#if !UNITY_WEBPLAYER
			GestureIO.WriteGesture(points.ToArray(), inputField.text, fileName);
		#endif

		trainingSet.Add(new Gesture(points.ToArray(), inputField.text));
		inputField.text = "";
	}
	*/
}
