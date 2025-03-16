using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    [SerializeField] TerrainMessenger messenger;
    Terrain terrain;
    TerrainCollider terrainCollider;

    void Start()
    {
        terrain = GetComponent<Terrain>();
        terrainCollider = GetComponent<TerrainCollider>();

        messenger.Init(terrain, terrainCollider.terrainData);
    }

    void OnMouseDown()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            messenger.OnClick(hit.point);
    }
}
