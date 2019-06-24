using System;
using UnityEngine;

namespace __Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Mace : MonoBehaviour
    {

        public bool IsMace = true;
        public float recoverySpeed = 1;
        
        private Vector3 _startingPosition;
        [SerializeField] private Transform _detectionPoint;
        private Rigidbody2D _rigidbody2D;

        private bool _stoppedFalling = false;
        
        // Start is called before the first frame update
        private void Awake()
        {
            _startingPosition = gameObject.transform.position;
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (this.transform.position.y >= _startingPosition.y)
            {
                _stoppedFalling = false;
                _rigidbody2D.velocity = new Vector2(0,0);
                _rigidbody2D.bodyType = RigidbodyType2D.Static;
                var position = _detectionPoint.position;
                var hit = Physics2D.Raycast(position, Vector2.down, Mathf.Infinity, 1<<LayerMask.NameToLayer("Player"));
                //Debug.DrawRay(position, Vector2.down, Color.red);
                if (hit.collider)
                {
                    _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                    //Debug.Log(hit.collider.gameObject.layer);
                    //Debug.Log("Target : " + LayerMask.NameToLayer("Player"));
                }
            }
            else if (IsMace)
            {
                
                if(_stoppedFalling)
                    _rigidbody2D.AddForce(Vector2.up * recoverySpeed);
                else
                if (Math.Abs(_rigidbody2D.velocity.y) < 0.01)
                    _stoppedFalling = true;
            }
        }
        
    }
}
