using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfoManager : MonoBehaviour
{
  [Tooltip("The camera to set on the world space UI canvas for each instantiated image info.")]
  [SerializeField] private Camera m_WorldSpaceCanvasCamera;

  public Camera worldSpaceCanvasCamera
  {
    get { return m_WorldSpaceCanvasCamera; }
    set { m_WorldSpaceCanvasCamera = value; }
  }

  [Tooltip("If an image is detected but no source texture can be found, this texture is used instead.")]
  [SerializeField] private Texture2D m_DefaultTexture;
  [SerializeField] private LineRenderer _line;

  private List<Vector3> points;

  public Texture2D defaultTexture
  {
    get { return m_DefaultTexture; }
    set { m_DefaultTexture = value; }
  }

  private ARTrackedImageManager m_TrackedImageManager;

  private void Awake()
  {
    points = new List<Vector3>();
    m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
  }

  private void OnEnable()
  {
    m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
  }

  private void OnDisable()
  {
    m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
  }

  private void UpdateInfo(ARTrackedImage trackedImage)
  {
    // Set canvas camera
    var canvas = trackedImage.GetComponentInChildren<Canvas>();
    canvas.worldCamera = worldSpaceCanvasCamera;


    // Update information about the tracked image   
    var text = canvas.GetComponentInChildren<Text>();
    text.text =
      $"{trackedImage.referenceImage.name}"
      + $"\ntrackingState: {trackedImage.trackingState}"
      + $"\nGUID: {trackedImage.referenceImage.guid}"
      + $"\nReference size: {trackedImage.referenceImage.size * 100f} cm"
      + $"\nDetected size: {trackedImage.size * 100f} cm";

    var planeParentGo = trackedImage.transform.GetChild(0).gameObject;
    var planeGo = planeParentGo.transform.GetChild(0).gameObject;

    // Disable the visual plane if it is not being tracked
    if (trackedImage.trackingState != TrackingState.None)
    {
      planeGo.SetActive(true);

      // The image extents is only valid when the image is being tracked
      trackedImage.transform.localScale = new Vector3(trackedImage.size.x, 1f, trackedImage.size.y);

      // store center point for line renderer
      points.Add(trackedImage.transform.position);

      // Set the texture
      var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
      material.mainTexture = (trackedImage.referenceImage.texture == null) ? defaultTexture : trackedImage.referenceImage.texture;
    }
    else
    {
      planeGo.SetActive(false);
    }
  }

  private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
  {
    points = new List<Vector3>();

    foreach (var trackedImage in eventArgs.added)
    {
      // Give the initial image a reasonable default scale
      trackedImage.transform.localScale = new Vector3(0.01f, 1f, 0.01f);

      UpdateInfo(trackedImage);
    }

    foreach (var trackedImage in eventArgs.updated)
      UpdateInfo(trackedImage);

    if (_line != null )
      _line.SetPositions(points.ToArray());
  }
}