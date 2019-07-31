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

        [Range(1,30)] public int levelSize = 10;
        [Range(0,1)] public float CR = .5f;
        public Texture2D mapLiberty;
        public GameObject end;
        private int _blocSize = Mathf.RoundToInt(512);
        

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
                
                //create Accurate % megabloc
                
                var targetPercentile = (actualSize*1f / levelSize) * CR;
                var percentile = dangerArea - targetPercentile; 
                
                var megaBlocs = new List<GameObject>();
                for (var i = 0; i < 4; i++)
                {
                    var instMega = Instantiate(blocs[Random.Range(0, blocs.Length)],this.transform);
                    megaBlocs.Add(instMega);
                    
                    if (i > 1)
                    {
                        var actual = instMega.GetComponent<MetaData>();
                        var last = megaBlocs[megaBlocs.Count - 1].GetComponent<MetaData>();
                        //associate blocs in megabloc
                        var positionToPlace = Place(actual, last, percentile);
                        PlaceBloc(pos.x+positionToPlace.x,pos.y+positionToPlace.y,instMega);
                        pos = instMega.transform.position;
                        Debug.Log(positionToPlace);

                    }
                    else
                    {
                        PlaceBloc(pos.x+1, pos.y,instMega);
                        pos = instMega.transform.position;
                    }

                }
                
                
                //instanciation
                //var bInst = Instantiate(blocs[Random.Range(0, blocs.Length)],this.transform);
                
                //PlaceBloc(Mathf.FloorToInt(pos.x+1), Mathf.FloorToInt(pos.y), percentile, bInst);
                //pos.x += 1+percentile;
                actualSize++;
            }
            PlaceBloc(pos.x+1, pos.y,Instantiate(end));
            FillDeathPits();

        }

        private Vector3 Place(MetaData actual, MetaData last, float target)
        {
            /**
             * increments ou 0.5 in position
             * 512 pix =~ 1
             *
             * test from last._pointRight to 2 above/2 further and 2 under
             *
             */

            var best = 0f;
            var bestV = new Vector2(0,0);
            
            for (float i = -2; i < 2; i+=0.5f)
            {
                for (float j = 0; j < 2; j+=0.5f)
                {
                    if (j == 0.5f) continue;
                    var endPoint = actual._pointLeft;
                    endPoint.x = endPoint.x + 512 + Mathf.RoundToInt(j * 512);
                    endPoint.y = endPoint.y + Mathf.RoundToInt(i * 512);
                    if(!PathAvailable(last._pointRight,endPoint, last._liberty)) continue;
                    
                    
                    //blue % in area
                    var count = 0f;
                    if (last._pointRight == endPoint) count = 1f;
                    else
                    {
                        //TODO: remplaçable par le nombre de pixels verts arrivant sur le sol de 2
                        var k = 0;
                        var l = 0;
                        for (k = last._pointRight.x; k <= endPoint.x; k++)
                        {
                            for (l = 0; l < _blocSize; l++)
                            {

                                var pix = mapLiberty.GetPixel(
                                    Mathf.FloorToInt(i) * _blocSize + k + Mathf.FloorToInt(_blocSize),
                                    Mathf.FloorToInt(j) * _blocSize + Mathf.FloorToInt(_blocSize));
                                count += pix.b * pix.a;
                            }
                            //mapLiberty.Apply();

                        }

                        count /= (k * l);
                    }

                    if ((Mathf.Abs(target - best) < Mathf.Abs(target - count))) continue;
                    best = count; //change 50 to best area variable 
                    bestV.x = j;
                    bestV.y = i;
                }
            }
            return bestV;
        }

        private static bool PathAvailable(Vector2Int depart, Vector2Int fin, Texture2D map)
        {
            if (fin.x >= map.width || fin.y >= map.height || fin.y < 0) return false;
            var pathFound = false;
            for (var i = depart.x; i < fin.x; i++)
            {
                pathFound = false;
                for (var j = 0; j < map.height; j++)
                {
                    var c = map.GetPixel(i, j);
                    if (!(c.g > 0)) continue;
                    pathFound = true;
                    break;
                }
                if (!pathFound) break;
            }
            return pathFound;
        }
        private void PlaceBloc(float i, float j, GameObject bloc)
        {
            /*
            var liberty = bloc.GetComponent<MetaData>()._liberty;
            liberty.Resize(Mathf.FloorToInt(liberty.width * 0.2f), Mathf.FloorToInt(liberty.height * 0.2f));
            for (var k = 0; k < liberty.width; k++)
            {
                for (var l = 0; l < liberty.height; l++)
                {
                    var col = liberty.GetPixel(i, j);
                    col += mapLiberty.GetPixel(i * _blocSize + k + Mathf.FloorToInt((1-offset) * _blocSize), 
                        j* _blocSize + l + Mathf.FloorToInt((1-offset) * _blocSize));
                    mapLiberty.SetPixel(i * _blocSize + k + Mathf.FloorToInt((1-offset) * _blocSize), 
                        j * _blocSize + l + Mathf.FloorToInt((1-offset) * _blocSize), col);
                }
            }*/
            bloc.transform.position = new Vector3(i/*+offset*/,j,0);
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
                    var pix = mapLiberty.GetPixel(Mathf.FloorToInt(i) * _blocSize + k + Mathf.FloorToInt(_blocSize), 
                        Mathf.FloorToInt(j) * _blocSize + Mathf.FloorToInt(_blocSize));
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
