using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace __Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class MetaData : MonoBehaviour
    {
        /*
        public int BlocID;
        public Transform junctionEst, junctionOuest;
        public float ChallengeRating = 1;
        public MetaAugmentingTile[] Additions;*/


        private Vector2 _speedX = new Vector2(50,25);

        public Vector2Int _pointLeft { get; private set; } = Vector2Int.zero;
        public Vector2Int _pointRight{ get; private set; } = Vector2Int.zero;

        private float _friction = 0.3f;

        private float _distance;

        private float _jumpingForce = 60f;

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

        private void Awake()
        {

            
            // Initialisation, pas de liberté possible
            _liberty = new Texture2D(2048,2048,TextureFormat.ARGB32, false);
            for (var xT = 0 ; xT < _liberty.width ; xT++)
            {
                for (var yT = 0 ; yT < _liberty.height ; yT++) _liberty.SetPixel(xT,yT, new Color(0f,0f,0f,0.5f));
            }
            _friction = gameObject.GetComponent<PolygonCollider2D>().friction;
            
            /** TODO: [BUG] positionner le bloc sur la texture.
             * Colorier le bloc en blanc 100% transparent sur la texture.
             * Cela permettra de sommer les textures et de n'avoir visible que les degrés de liberté du niveau
             * /!\ lors de la somme, attention à supprimer les projections dépassantes (via une "ombre") 
             */
            Texture2D piece = Instantiate(gameObject.GetComponent<SpriteRenderer>().sprite.texture);
            //TextureScale.Bilinear(piece,32, 32);
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
                        if (i + 1 < h && _pointLeft == Vector2Int.zero)
                        {
                            if (piece.GetPixel(j , i+1).a == 0f)
                            {
                                _pointLeft = new Vector2Int(j, i);
                            }
                        }else if (i + 1 == h && _pointLeft == Vector2Int.zero)
                        {
                            _pointLeft = new Vector2Int(j, i);
                        }

                        if (i + 1 < h)
                        {
                            if (piece.GetPixel(j, i + 1).a != 0f)
                            {
                                _liberty.SetPixel(j + offsetH, i + offsetW, new Color(0,0,0,0f));
                            }
                        }

                        
                        //_liberty.SetPixel(j + offsetH, i + offsetW, new Color(i * 1f / h, 0, 0));//j*1f/w,0));
                        _pointRight = new Vector2Int(j,i);
                    }
                }
            }
            
            _pointLeft+=new Vector2Int(offsetW,offsetH);
            _pointRight+=new Vector2Int(offsetW,offsetH);
            //Debug.Log(_pointLeft);
            //Debug.Log(_pointRight);
            
            Vector2 differential = (_pointRight - _pointLeft);
            var differentialNorm = differential.normalized;
            //Debug.Log(differentialNorm);
            var pixel = _pointLeft;
            
            // Libertés basiques au sol
            var saveX = _pointLeft.x;
            var saveY = _pointLeft.y + 1;
            var y = saveY + 0f;
            var x = _pointLeft.x + 0f;
            while((_pointRight.x < _pointLeft.x && x > _pointRight.x) || (_pointRight.x > _pointLeft.x && x < _pointRight.x))
            {
                x += differentialNorm.x;
                y += differentialNorm.y;
                if (Mathf.RoundToInt(x) != saveX || Mathf.RoundToInt(y) != saveY)
                {
                    var newX = Mathf.RoundToInt(x + 1f); 
                    var newY = Mathf.RoundToInt(y + 1f); 
                    pixel = new Vector2Int(newX, newY);

                    var color = _liberty.GetPixel(pixel.x, pixel.y);
                    //liberté gauche et droite
                    color.g += (_speedX.x / 255);
                    color.a = 0f;
                    color.r = 1f; //+= (_speedX.y / 255);
                    _liberty.SetPixel(pixel.x, pixel.y, color);

                    //Hauteur du joueur (judicieux? -> calculable à part)
                    for (var yT = 0; yT < _playerHeight; yT++)
                    {
                        var color2 = _liberty.GetPixel(pixel.x, pixel.y + yT);
                        //liberté gauche et droite
                        color2.g += (_speedX.x / 255);
                        color2.r += (_speedX.y / 255);
                        _liberty.SetPixel(pixel.x, pixel.y + yT, color2);
                    }

                    //Saut statique
                    for (var yT= 0; yT < _jumpingForce; yT++)
                    {
                        var color2 = _liberty.GetPixel(pixel.x, pixel.y + yT);
                        //liberté gauche et droite
                        color2.g += (_speedX.x / 255);
                        color2.r += (_speedX.y / 255);
                        //color2.a += 0.3f; //aff0
                        _liberty.SetPixel(pixel.x, pixel.y + yT, color2);
                    }

                    //Todo: ajout valeures effectives personnage
                    //Courbe de saut et coloriage
                    var startX = pixel.x+1;
                    var xFunct = startX+0f;
                    var startY = pixel.y;
                    var yFunct = startY+0f;
                    var angleDeg = 60;
                    var angle = angleDeg * Mathf.PI / 180; //Radians !!
                    Vector2 norm = _jumpingForce * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Vector2Int save = new Vector2Int(startX, startY);

                    while (xFunct < _liberty.width && xFunct >= 0 && yFunct < _liberty.height && yFunct >= 0)
                    {
                        norm.x = _jumpingForce * Mathf.Cos(angle);
                        norm.y = _jumpingForce * Mathf.Sin(angle) - (_gravity*(xFunct-startX)/(norm.x));
                        norm = norm.normalized;
                        yFunct += norm.y;
                        xFunct += norm.x;

                        if (save.x != Mathf.RoundToInt(xFunct) || save.y != Mathf.RoundToInt(yFunct))
                        {
                            save.x = Mathf.RoundToInt(xFunct);
                            save.y = Mathf.RoundToInt(yFunct);
                            var color2 = _liberty.GetPixel(save.x, save.y);
                            if (color2.a == 0f) break;
                            color2.g += (_speedX.x / 255);    
                            _liberty.SetPixel(save.x, save.y, color2);
                            
                        }
                    }
                    
                    startX = pixel.x+1;
                    xFunct = startX;
                    startY = pixel.y;
                    yFunct = startY;
                    angleDeg = 120;
                    angle = angleDeg * Mathf.PI / 180; //Radians !!
                    norm = _jumpingForce * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    save = new Vector2Int(startX, startY);
                    while (xFunct < _liberty.width && xFunct >= 0 && yFunct < _liberty.height && yFunct >= 0)
                    {
                        norm.x = _jumpingForce * Mathf.Cos(angle);
                        norm.y = _jumpingForce * Mathf.Sin(angle) - (_gravity*(xFunct-startX)/(norm.x));
                        norm = norm.normalized;
                        yFunct += norm.y;
                        xFunct += norm.x;

                        if (save.x != Mathf.RoundToInt(xFunct) || save.y != Mathf.RoundToInt(yFunct))
                        {
                            save.x = Mathf.RoundToInt(xFunct);
                            save.y = Mathf.RoundToInt(yFunct);
                            var color2 = _liberty.GetPixel(save.x, save.y);
                            if (color2.a == 0f) break;
                            color2.r += (_speedX.x / 255);    
                            _liberty.SetPixel(save.x, save.y, color2);
                            
                        }
                    }

                    //Todo: Air Control
                    //Colorier toute l'intégrale avec gauche/droite

                }

                saveX = Mathf.RoundToInt(x+1f);
                saveY = Mathf.RoundToInt(y+1f);

            }
            
            //Inertia
            for (int i = 0; i < Mathf.FloorToInt(_speedX.x * _friction); i++)
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
        }
*/
        private void OnGUI()
        {
            //Debug.Log("GUI of " + this.name + " : " + (_liberty != null));//aff0
            if (_liberty)
            {
                //GUI.DrawTexture(new Rect(new Vector2(0,0),new Vector2(500,500)), _liberty);
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
