using UnityEngine;

namespace View.VFX.TestScripts
{
    [SelectionBase]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float rotateSensHoriz = 1.5f;
        [SerializeField] private float rotateSensVert = 1.5f;
        [SerializeField] private float maxVertAngle = 90.0f;

        private Transform cachedTransform;
        
        private Vector3 firstPoint;
        private Vector3 secondPoint;
        private float xAngle;
        private float yAngle;
        private float xAngTemp;
        private float yAngTemp;

        [HideInInspector] [SerializeField] private RectTransform ignoredTouchZone;
    
        void Start()
        {
        
            // joystick = FindObjectOfType<FloatingJoystick>();
            cachedTransform = transform;
#if UNITY_EDITOR && !UNITY_REMOTE 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
#endif
        }
    
        void Update()
        {
            cachedTransform.position += GetMovementVector();
#if UNITY_EDITOR && !UNITY_REMOTE
            cachedTransform.localEulerAngles = MouseRotation();
#else
        TouchRotation();
#endif
        }

        private void TouchRotation()
        {
            if (Input.touchCount > 0)
            {
                var touchIndex = 0;
                if (ignoredTouchZone.rect.Contains(Input.GetTouch(0).position))
                {
                    if (Input.touchCount > 1)
                        touchIndex = 1;
                    else
                        return;
                }
                if (Input.GetTouch(touchIndex).phase == TouchPhase.Began)
                {
                    firstPoint = Input.GetTouch(touchIndex).position;
                    xAngTemp = xAngle;
                    yAngTemp = yAngle;
                }
                if (Input.GetTouch(touchIndex).phase == TouchPhase.Moved)
                {
                    secondPoint = Input.GetTouch(touchIndex).position;
                    xAngle = xAngTemp + (secondPoint.x - firstPoint.x) * 180.0f / Screen.width;
                    yAngle = yAngTemp - (secondPoint.y - firstPoint.y) * 90.0f / Screen.height;
                    transform.rotation = Quaternion.Euler(yAngle, xAngle, 0.0f);
                }
            }
        }
        private Vector3 MouseRotation()
        {
            var cameraRotation = cachedTransform.localEulerAngles;
            var rotation = cameraRotation;

            rotation.x -= Input.GetAxis("Mouse Y") * rotateSensVert; // vertical
            rotation.x = rotation.x > 180 ? rotation.x - 360 : rotation.x;
            rotation.x = Mathf.Clamp(rotation.x, -maxVertAngle, maxVertAngle);
            rotation.y += Input.GetAxis("Mouse X") * rotateSensHoriz; // horizontal
            return rotation;
        }

        private Vector3 GetMovementVector()
        {
            var moveForward = cachedTransform.forward;
            var moveSide = cachedTransform.right;
            var moveVertical = Vector3.up;

            moveForward.y = 0;
            moveSide.y = 0;
            var moveDir = GetMovementDir();
            moveForward *= Time.deltaTime * moveSpeed * moveDir.z;
            moveSide *= Time.deltaTime * moveSpeed * moveDir.x;
            moveVertical *= Time.deltaTime * moveSpeed * GetVerticalMovement();
            return moveForward + moveSide + moveVertical;
        }

        Vector3 GetMovementDir()
        {
            var result = Vector3.zero;
        
#if UNITY_EDITOR && !UNITY_REMOTE
            result.x += Input.GetAxis("Horizontal");
            result.z += Input.GetAxis("Vertical");
#else
        result.x += joystick.Horizontal;
        result.z += joystick.Vertical;
#endif
            return result;
        }
    
        private float GetVerticalMovement()
        {
        
            if (Input.GetKey(KeyCode.LeftShift))
                return -1f;
            return Input.GetKey(KeyCode.Space) ? 1f : 0f;
        }
    }
}