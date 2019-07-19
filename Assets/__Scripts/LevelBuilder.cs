using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace __Scripts
{
    [RequireComponent(typeof(Tilemap))]
    public class LevelBuilder : MonoBehaviour
    {
        private Tilemap _tilemap;
        
        private GameObject _start;

        [Range(10,30)] public int levelSize = 10;
        [Range(0,1)] public float CR = .5f;
        public Texture2D mapLiberty;
        public GameObject end;
        private int _blocSize = Mathf.RoundToInt(512*0.2f);
        

        // Start is called before the first frame update
        private void Start()
        {
            Debug.Log("Start of level building");
            _tilemap = GetComponent<Tilemap>();
            foreach(var t in GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name.Equals("Start")) _start = t.gameObject;
            }
            mapLiberty = new Texture2D(levelSize * _blocSize, 15 * _blocSize);
            for (int i = 0; i < mapLiberty.width; i++)
            {
                for (int j = 0; j < mapLiberty.height; j++)
                {
                    mapLiberty.SetPixel(i,j,new Color(0,0,0,0.5f));
                }
            }
            //mapLiberty.Apply();
            var blocs =Resources.LoadAll<GameObject>("__Prefabs/Tiles");
            //Todo: Difficulté : courbe décroissante de la surface sans danger
            // %surface(position) = (1 - CR) * (levelSize - position) / levelSize
            // augmentation croissante de difficulté selon le Challenge Rating
            // 1ère passe

            var pos = _start.transform.position;
            var actualSize = 0;
            var sector = 3; // On considère 3 cases entre le début du dernier bloc et la fin du secteur (2 blocs est le saut quasi pixel-perfect à plat)
            while (actualSize < levelSize)
            {
                //todo: check surface of blue presence in sector
                var dangerArea = 0f;
                var cpt = 0;
                for (var i = pos.x; i < pos.x+sector; i++)
                {
                    for (var j = pos.y; j < pos.y+sector; j++)
                    {
                        dangerArea += CountBloc(i, j);
                        cpt++;
                    }
                }

                dangerArea /= dangerArea / cpt;
                
                var targetPercentile = (actualSize*1f / levelSize) * CR;
                var percentile = dangerArea - targetPercentile; 
                
                //instanciation
                var bInst = Instantiate(blocs[Random.Range(0, blocs.Length)],this.transform);
                
                PlaceBloc(Mathf.FloorToInt(pos.x+1), Mathf.FloorToInt(pos.y), percentile, bInst);
                pos.x += 1+percentile;
                actualSize++;
            }
            FillDeathPits();

        }

        private void PlaceBloc(int i, int j, float offset, GameObject bloc)
        {
            var liberty = bloc.GetComponent<MetaData>()._liberty;
            liberty.Resize(Mathf.FloorToInt(liberty.width * 0.2f), Mathf.FloorToInt(liberty.height * 0.2f));
            for (var k = 0; k < liberty.width; k++)
            {
                for (var l = 0; l < liberty.height; l++)
                {
                    var col = liberty.GetPixel(i, j);
                    col += mapLiberty.GetPixel(i * _blocSize + k + Mathf.FloorToInt((1-offset) * _blocSize), j * _blocSize + l + Mathf.FloorToInt((1-offset) * _blocSize));
                    mapLiberty.SetPixel(i * _blocSize + k + Mathf.FloorToInt((1-offset) * _blocSize), j * _blocSize + l + Mathf.FloorToInt((1-offset) * _blocSize), col);
                }
            }
            bloc.transform.position = new Vector3(i+offset,j,0);
            //mapLiberty.Apply();
        }
        private float CountBloc(float i, float j)
        {
            var count = 0f;
            var k = 0; var l = 0;
            for (k = 0; k < _blocSize; k++)
            {
                for (l = 0; l < _blocSize; l++)
                {
                    var pix = mapLiberty.GetPixel(Mathf.FloorToInt(i) * _blocSize + k + Mathf.FloorToInt(_blocSize), Mathf.FloorToInt(j) * _blocSize + Mathf.FloorToInt(_blocSize));
                    count += pix.b * pix.a;
                }
            }
            //mapLiberty.Apply();
            return count / (k*l);
        }

        private void FillDeathPits()
        {
            for (var i = 0; i < mapLiberty.width; i++)
            {
                for (var j = 0; j < mapLiberty.height; j++)
                {
                    var pix = mapLiberty.GetPixel(i, j);
                    if (pix.a != 0f)
                    {
                        pix.b += 50; // ARBITRAIRE ???
                        mapLiberty.SetPixel(i,j, pix);
                    }
                    else break;
                }
            }
            mapLiberty.Apply();
        }
        private void OnGUI()
        {
            if (mapLiberty)
            {
                GUI.DrawTexture(new Rect(new Vector2(0,0),new Vector2(mapLiberty.width,mapLiberty.height)), mapLiberty);
            }
        }
    }
}
