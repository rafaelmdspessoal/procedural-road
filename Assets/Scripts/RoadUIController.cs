using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoadUIController : MonoBehaviour
{
    public static RoadUIController Instance { get; private set; }

    public Action OnStartPlacingRoads, OnRoadRemoved;
    public Action<RoadObjectSO> OnStraightRoadPlacement, OnCurveRoadPlacement;
    public Button buildRoadButton, placeStraightButton, placeCurveRoadButton, removeRoadButton;

    public Color outlineColor;
    List<Button> buttonList;

    public RoadObjectSO roadObjectSO;

    private void Awake() {
        Instance = this;
    }

    private void Start()
    {
        buttonList = new List<Button> { placeStraightButton, placeCurveRoadButton, buildRoadButton, removeRoadButton };

        buildRoadButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(buildRoadButton);
            OnStartPlacingRoads?.Invoke();

        });
        placeStraightButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(placeStraightButton);
            OnStraightRoadPlacement?.Invoke(roadObjectSO);

        });
        placeCurveRoadButton.onClick.AddListener(() =>
        {
            ResetButtonColor();
            ModifyOutline(placeCurveRoadButton);
            OnCurveRoadPlacement?.Invoke(roadObjectSO);

        });
        removeRoadButton.onClick.AddListener(() => {
            ResetButtonColor();
            ModifyOutline(removeRoadButton);
            OnRoadRemoved?.Invoke();

        });
        placeStraightButton.gameObject.SetActive(false);
        placeCurveRoadButton.gameObject.SetActive(false);
    }

    private void ModifyOutline(Button button)
    {
        Outline outline = button.GetComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.enabled = true;
    }

    private void ResetButtonColor()
    {
        foreach (Button button in buttonList)
        {
            button.GetComponent<Outline>().enabled = false;
        }
    }
}