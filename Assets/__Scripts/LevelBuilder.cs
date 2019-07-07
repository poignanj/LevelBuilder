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

        public int levelSize;
        [Range(0,1)] public float CR = .5f;
        public Texture2D mapLiberty;
        public GameObject end;
        public int blocSize;
        

        // Start is called before the first frame update
        private void Start()
        {
            _tilemap = GetComponent<Tilemap>();
            foreach(var t in GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name.Equals("Start")) _start = t.gameObject;
            }
            mapLiberty = new Texture2D(levelSize * blocSize, 20 * blocSize);
            
            
            var blocsArray =Resources.LoadAll<GameObject>("__Prefabs/Blocs");
            var blocs = blocsArray.ToList();
            
            //Difficulté : courbe décroissante de la surface sans danger
            // %surface(position) = (1 - CR) * (levelSize - position) / levelSize
            // augmentation croissante de difficulté selon le Challenge Rating
            // 1ère passe

            var pos = _start.transform.position;
            var actualSize = 0;
            while (actualSize < levelSize)
            {
                //todo: check surface of blue presence in sector
                var percentile = .6f;
                PlaceBloc(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y+1), percentile, Instantiate(blocs[Random.Range(0,blocs.Count)]));

                pos.y += 1;
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
                    col += mapLiberty.GetPixel(i * blocSize + k + Mathf.FloorToInt((1-offset) * blocSize), j * blocSize + l + Mathf.FloorToInt((1-offset) * blocSize));
                    mapLiberty.SetPixel(i * blocSize + k, j * blocSize + l, col);
                }
            }
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
                    }
                    else break;
                }
            }
        }
        
    }
}
