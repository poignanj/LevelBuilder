using System;
using UnityEngine;

namespace __Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class MetaData : MonoBehaviour
    {
        /*
        public int BlocID;
        public Transform junctionEst, junctionOuest;
        public float ChallengeRating = 1;
        public MetaAugmentingTile[] Additions;*/


        private Vector2 _speedX = new Vector2(50,25);

        private Vector2Int _pointLeft = Vector2Int.zero, _pointRight = Vector2Int.zero;

        private float _inertia = 0.3f;

        private float _distance;

        private float _jumpingForce = 30f;

        private float _jumpingMaxDistance = 1;

        private float _playerHeight = 1.5f;

        /***
         * _liberty représente des libertés de mouvement du joueur autours du bloc.
         * Chaque liberté correspond à une couleur.
         * R : Mouvement horizontal vers la gauche
         * G : Mouvement horizontal vers la droite
         * B : Mouvement vertical vers le haut (saut)
         *
         * Chaque valeur de liberté est pondéré par la vitesse atteignable à cet endroit sur l'axe donné
         * 
         */
        public Texture2D _liberty;
        private float _gravity = 9.81f;

        private void Start()
        {

            
            // Initialisation, pas de liberté possible
            _liberty = new Texture2D(1024,1024,TextureFormat.ARGB32, false);
            for (var x = 0 ; x < _liberty.width ; x++)
            {
                for (var y = 0 ; y < _liberty.height ; y++) _liberty.SetPixel(x,y, new Color(0f,0f,0f,0.5f));
            }
            
            /** TODO: positionner le bloc sur la texture.
             * Colorier le bloc en blanc 100% transparent sur la texture.
             * Cela permettra de sommer les textures et de n'avoir visible que les degrés de liberté du niveau
             * /!\ lors de la somme, attention à supprimer les projections dépassantes (via une "ombre") 
             */
            var piece = gameObject.GetComponent<SpriteRenderer>().sprite.texture;
            var w = piece.width;
            var h = piece.height;
            var offsetW = (_liberty.width / 2) - (w / 2);
            var offsetH = (_liberty.height / 2) - (h / 2);
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    var color = piece.GetPixel(j, i);
                    if (color.a != 0f)
                    {
                        if (_pointLeft == Vector2Int.zero)
                        {
                            _pointLeft = new Vector2Int(j,i);
                        }

                        //_liberty.SetPixel(j + offsetH, i + offsetW, new Color(0,0,0,0f));
                        _pointRight = new Vector2Int(j,i);
                    }
                }
            }
            _pointLeft+=new Vector2Int(offsetW,offsetH);
            
            _pointRight+=new Vector2Int(offsetW,offsetH);
            Debug.Log(_pointLeft);
            Debug.Log(_pointRight);
            
            
            Vector2 differential = (_pointRight - _pointLeft);
            var differentialNorm = differential.normalized;
            Debug.Log(differentialNorm);
            var pixel = _pointLeft;
            // Libertés basiques au sol
            for (var x = _pointLeft.x; x < _pointRight.x; x++)
            {
                pixel = new Vector2Int(x, _pointLeft.y + Mathf.FloorToInt((x - _pointLeft.x) * differentialNorm.y));
                
                
                var color = _liberty.GetPixel(pixel.x, pixel.y);
                //liberté gauche et droite
                color.g += (_speedX.x / 255);
                color.a = 0f;
                color.r =1f;//+= (_speedX.y / 255);
                _liberty.SetPixel(pixel.x, pixel.y, color);
                
                //Hauteur du joueur (judicieux? -> calculable à part)
                for (var y = 0; y < _playerHeight; y++)
                {
                    var color2 = _liberty.GetPixel(pixel.x, pixel.y + y);
                    //liberté gauche et droite
                    color2.g += (_speedX.x / 255);
                    color2.r += (_speedX.y / 255);
                    _liberty.SetPixel(pixel.x, pixel.y + y, color2);
                }
                
                //Saut statique
                for(var y = 0; y < _jumpingForce;y++) {
                    var color2 = _liberty.GetPixel(pixel.x, pixel.y + y);
                    //liberté gauche et droite
                    color2.g += (_speedX.x / 255);
                    color2.r += (_speedX.y / 255);
                    //color2.a += 0.3f; //aff0
                    _liberty.SetPixel(pixel.x, pixel.y + y, color2);
                }
                
                //Todo: Saut directionnel? ajout valeures effectives personnage
                //Courbe de saut et coloriage
                var startX = x;
                var xFunct = startX;
                float startY = pixel.y-1;
                var yFunct = startY;
                var angleDeg = 45;
                var inc = 0;
                if (angleDeg > 90) inc = -1;
                else inc = 1;
                var angle = angleDeg * Mathf.PI/180; //Radians !!
                
                //Debug.Log(xFunct+" ::: "+yFunct); //aff0
                while (xFunct < _liberty.width && xFunct >= 0 && yFunct < _liberty.height && yFunct >=0)
                {
                    var color2 = _liberty.GetPixel(xFunct, Mathf.FloorToInt(yFunct));
                    if (color2.a == 0f) break;
                    color2.g += (_speedX.x / 255);
                    //color2.r += (_speedX.y / 255);
                    //color2.b += 50;// (_jumpingForce/255);
                    _liberty.SetPixel(xFunct, Mathf.FloorToInt(yFunct), color2);
                    xFunct+=inc;
                    yFunct = -0.5f * (Mathf.Pow(xFunct - startX, 2) * _gravity / (Mathf.Pow(Mathf.Cos(angle),2)*Mathf.Pow(_jumpingForce,2))) + Mathf.Tan(angle) * (xFunct - startX) + startY;
                }

                startX = x;
                xFunct = startX;
                startY = pixel.y-1;
                yFunct = startY;
                angleDeg = 135;
                if (angleDeg > 90) inc = -1;
                else inc = 1;
                angle = angleDeg * Mathf.PI/180; //Radians !!
                
                //Debug.Log(xFunct+" ::: "+yFunct); //aff0
                while (xFunct < _liberty.width && xFunct >= 0 && yFunct < _liberty.height && yFunct >=0)
                {
                    var color2 = _liberty.GetPixel(xFunct, Mathf.FloorToInt(yFunct));
                    if (color2.a == 0f) break;
                    //color2.g += (_speedX.x / 255);
                    color2.r += (_speedX.y / 255);
                    //color2.b += 50;// (_jumpingForce/255);
                    _liberty.SetPixel(xFunct, Mathf.FloorToInt(yFunct), color2);
                    xFunct+=inc;
                    yFunct = -0.5f * (Mathf.Pow(xFunct - startX, 2) * _gravity / (Mathf.Pow(Mathf.Cos(angle),2)*Mathf.Pow(_jumpingForce,2))) + Mathf.Tan(angle) * (xFunct - startX) + startY;
                }

                //Todo: Air Control
                //Colorier toute l'intégrale avec gauche/droite
                
                
                
            }
            
            //Inertia
            for (int i = 0; i < Mathf.FloorToInt(_speedX.x * _inertia); i++)
            {
                for (int j = 0; j < _pointLeft.y; j++)
                {
                    var pix = _liberty.GetPixel(_pointLeft.x - i, j);
                    pix.b += 0.3f;
                    _liberty.SetPixel(_pointLeft.x - i,j, pix);
                }

                for (int j = 0; j < _pointRight.y; j++)
                {
                    var pix = _liberty.GetPixel(_pointRight.x + i, j);
                    pix.b += 0.3f;
                    _liberty.SetPixel(_pointRight.x + i, j, pix);
                }
            }
            
            
            _liberty.Apply();
            
        }

        /*private void Update()
        {
            //TODO: Selon cases autour, adapter difficulté
            Gizmos.color = Color.green;
            var maxHeight = _playerHeight + _jumpingForce;
            var maxJump = _jumpingMaxDistance * _speedX.x;
            var vGauche = new Vector2();
            var vDroite = new Vector2();

            var curve = vDroite.x * maxJump;
        }*/

        private void OnGUI()
        {
            if (_liberty)
            {
                GUI.DrawTexture(new Rect(new Vector2(0,0),new Vector2(500,500)), _liberty);
            }
        }

        /*
        public GameObject[] GetTilesInDir(char dir)
        {
            Debug.Log("Accès : " + name + " : " + dir);
            switch (dir)
            {
                case 'N':
                    if(tilesN.Length>0)
                        return tilesN;
                    break;
                case 'E':
                    if(tilesE.Length>0)
                        return tilesE;
                    break;
                case 'S':
                    if(tilesS.Length>0)
                        return tilesS;
                    break;
                case 'O':
                    if(tilesO.Length>0)
                        return tilesO;
                    break;
                default:
                    break;
            }
            return null;
        }*/
    }
}
