using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PortalColour
{
    Orange,
    Blue
}

public class PortalGun : MonoBehaviour
{
    [SerializeField] private GameObject _bluePortalPrefab;
    [SerializeField] private GameObject _orangePortalPrefab;
    private GameObject _bluePortal;
    private GameObject _orangePortal;
    private Portal _bluePortalScript;
    private Portal _orangePortalScript;
    private bool _connected = false;
    public static PortalGun instance;

    private void Start() {
        Awake();
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            _orangePortal = Instantiate(_orangePortalPrefab, new Vector3(0,0,-100), Quaternion.identity);
            _bluePortal = Instantiate( _bluePortalPrefab, new Vector3(0,0,-100), Quaternion.identity);
            TryConnect();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PlacePortal(ref GameObject portal, GameObject portalPrefab, Vector3 location, Quaternion rotation, bool tryOnce = false, bool locationOverride = false)
    {
        bool existingPortal = portal != null;
        // Take snapshot for fall back
        var positionSnapshot = portal ? portal.transform.position : new Vector3(0,0,-1);
        var rotationSnapshot = portal ? portal.transform.rotation : new Quaternion(-1,-1,-1,-1);

        Grid grid = FindObjectOfType<Grid>();
        Vector3Int cellPosition = grid.WorldToCell(location);
        Vector3 snappedPosition = grid.CellToWorld(cellPosition);
        snappedPosition = snappedPosition.WithAxis(Axis.X, (rotation.eulerAngles.z == 45 ? snappedPosition.x + 1.125f : snappedPosition.x));
        snappedPosition = snappedPosition.WithAxis(Axis.X, (rotation.eulerAngles.z == 135 ? snappedPosition.x - 0.125f : snappedPosition.x));

        if ((_bluePortal.transform.position == snappedPosition) 
        || (_orangePortal.transform.position == snappedPosition))
        {
            return;
        }
        
        

        // if (locationOverride) Debug.Log("LOCATION OVERRIDE");
        portal.transform.position = snappedPosition;
        portal.transform.rotation = rotation;

        if (_bluePortalScript) _bluePortalScript.CalculateDirection();
        if (_orangePortalScript) _orangePortalScript.CalculateDirection();

        var portalScript = portal.GetComponent<Portal>();
        portalScript.CheckCanTeleport();
        if (!tryOnce && !portalScript.ValidPlacement(Vector2.zero))
        {
            // Attempt to find a valid placement for the portal at the corrected position
            Vector2[] directions = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right, Vector2.up + Vector2.right,  Vector2.up + Vector2.left};
            bool validPlacementFound = false;

            foreach (Vector2 direction in directions)
            {
                if (portalScript.ValidPlacement(direction))
                {
                    PlacePortal(ref portal, portalPrefab, location + (Vector3)direction, rotation, true);
                    validPlacementFound = true;
                    break;
                }
            }

            if (!validPlacementFound)
            {
                // Debug.Log("No valid place found");
                portal.transform.position = positionSnapshot;
                portal.transform.rotation = rotationSnapshot;
            }
        }
    }

    public void ShootPortal(PortalColour color, Vector3 location, Quaternion rotation)
    {
        if (color == PortalColour.Blue)
        {
            PlacePortal(ref _bluePortal, _bluePortalPrefab, location, rotation);
        }
        else if (color == PortalColour.Orange)
        {
            PlacePortal(ref _orangePortal, _orangePortalPrefab, location, rotation);
        }
    }

    public void TryConnect()
    {
        if (_connected) return;

        if (_bluePortal != null && _orangePortal != null)
        {
            _bluePortalScript = _bluePortal.GetComponent<Portal>();
            _orangePortalScript = _orangePortal.GetComponent<Portal>();
            _bluePortalScript.Connect(_orangePortalScript);
            _connected = true;
        }
    }

    public void DestroyPortal(PortalColour color)
    {
        if (color == PortalColour.Blue)
        {
            if (_bluePortal != null)
            {
                Destroy(_bluePortal);
                _bluePortal = null;
            }
        }
        else if (color == PortalColour.Orange)
        {
            if (_orangePortal != null)
            {
                Destroy(_orangePortal);
                _orangePortal = null;
            }
        }
    }


}
