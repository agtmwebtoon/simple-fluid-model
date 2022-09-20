using UnityEngine;

namespace DefaultNamespace
{
    [System.Serializable]
    public class TransformData
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 velocity;
        
        
        public TransformData(Vector3 _position, Vector3 _rotation, Vector3 _velocity)
        {
            position = _position;
            rotation = _rotation;
            velocity = _velocity;
        }
        
        
    }
}