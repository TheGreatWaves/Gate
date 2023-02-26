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

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void PlacePortal(ref GameObject portal, GameObject portalPrefab, Vector3 location, Quaternion rotation)
    {
        Grid grid = FindObjectOfType<Grid>();
        Vector3Int cellPosition = grid.WorldToCell(location);
        Vector3 snappedPosition = grid.CellToWorld(cellPosition);
        snappedPosition = snappedPosition.WithAxis(Axis.X, (rotation.eulerAngles.z == 45 ? snappedPosition.x + 1.125f : snappedPosition.x));
        snappedPosition = snappedPosition.WithAxis(Axis.X, (rotation.eulerAngles.z == 135 ? snappedPosition.x - 0.125f : snappedPosition.x));

        // Check if there is already a portal at the new location
        if ((_bluePortal != null && _bluePortal.transform.position == snappedPosition) || (_orangePortal != null && _orangePortal.transform.position == snappedPosition))
        {
            // There is already a portal at the new location, do not create a new portal
            return;
        }

        if (portal != null)
        {
            portal.transform.position = snappedPosition;
            portal.transform.rotation = rotation;

            if (_bluePortalScript) _bluePortalScript.CalculateDirection();
            if (_orangePortalScript) _orangePortalScript.CalculateDirection();
        }
        else 
        {
            portal = Instantiate(portalPrefab, snappedPosition, rotation);
            TryConnect();
        }

        var portalScript = portal.GetComponent<Portal>();
        if (!portalScript.CheckPortalGrounded())
        {
            var angle = (int)rotation.eulerAngles.z;

            switch (angle)
            {
                // Vertical
                case 0:
                    Debug.Log("Up Down");
                    break;
                case 180:
                    Debug.Log("Up Down");
                    break;

                // Horizontal
                case 90:
                    var vecLeft = new Vector2(-1, 0);
                    var vecRight = new Vector2(1, 0);
                    if (portalScript.CheckPortalGrounded(vecLeft))
                    {
                        // Debug.Log("Move left");
                        // PlacePortal(ref portal, portalPrefab, location + (Vector3)vecLeft, rotation);
                    }
                    else if (portalScript.CheckPortalGrounded(vecRight))
                    {
                        // Debug.Log("Move right");
                        // PlacePortal(ref portal, portalPrefab, location + (Vector3)vecRight, rotation);
                    }
                    break;
                case 270:
                    break;
                default:
                    break;
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
