using UnityEngine;
public static class StaticMethodsMovement
{
    public static Vector3 GetAxisMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        return new Vector3(x, 0, z);
    }
    public static Vector2 GetAxisRotation()
    {
        float x = Input.GetAxis("Mouse X") * Time.deltaTime;
        float y = Input.GetAxis("Mouse Y") * Time.deltaTime;
        return new Vector2(x, y);
    }
    public static Vector3 GetVectorMovement(Transform transform, Vector3 axis)
    {
        Vector3 move = transform.right * axis.x + transform.forward * axis.z;
        return move;
    }
    public static bool IsGrounded(Vector3 groundPosition, float groundDistance, LayerMask groundMask)
    {
        return Physics.CheckSphere(groundPosition, groundDistance, groundMask);
    }
}
